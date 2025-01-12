using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Serilog;

namespace VideoUploaderScheduler
{
    static class Program
    {
        public static IConfiguration Configuration { get; private set; }

        [STAThread]
        static async Task Main(string[] args)
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // Uygulama başlangıç kontrolleri
                InitializeApplication();

                // Host oluştur ve çalıştır
                using IHost host = CreateHostBuilder(args).Build();
                await host.StartAsync();

                // Windows Form uygulamasını çalıştır
                var services = host.Services;
                Application.Run(services.GetRequiredService<Form1>());

                // Graceful shutdown
                await host.StopAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Kritik hata oluştu:\n\n{ex.Message}\n\nUygulama kapatılacak.",
                    "Kritik Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                // Log the error
                Log.Fatal(ex, "Uygulama kritik hata nedeniyle sonlandırıldı");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void InitializeApplication()
        {
            // Configuration builder
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // Initialize Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .Enrich.FromLogContext()
                .WriteTo.File(
                    path: Path.Combine("Logs", "log-.txt"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 31)
                .WriteTo.Console()
                .CreateLogger();

            // Ensure required directories exist
            EnsureDirectoriesExist();

            // Validate environment
            ValidateEnvironment();

            // Check for updates
            _ = CheckForUpdatesAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton(Configuration);
                    
                    // Core services
                    services.AddSingleton<UploadScheduler>();
                    services.AddSingleton<CredentialStore>();
                    services.AddSingleton<YouTubeUploader>();
                    services.AddSingleton<InstagramUploader>();

                    // Worker service
                    services.AddHostedService<Worker>();

                    // Windows Forms
                    services.AddSingleton<Form1>();

                    // Add additional services
                    ConfigureAdditionalServices(services);
                })
                .UseSerilog()
                .ConfigureLogging((hostContext, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddSerilog(dispose: true);
                });

        private static void ConfigureAdditionalServices(IServiceCollection services)
        {
            // Add any additional service configurations here
            services.Configure<UploadSettings>(Configuration.GetSection("UploadSettings"));
            services.AddHttpClient();
        }

        private static void EnsureDirectoriesExist()
        {
            var directories = new[]
            {
                AppSettings.Paths.LogDirectory,
                AppSettings.Paths.CredentialsDirectory,
                AppSettings.Paths.TempDirectory,
                AppSettings.Paths.YouTubeSessionsDirectory,
                AppSettings.Paths.InstagramSessionsDirectory,
                "Config"
            };

            foreach (var dir in directories)
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
        }

        private static void ValidateEnvironment()
        {
            // Check .NET version
            if (Environment.Version < new Version(6, 0))
            {
                throw new Exception("Bu uygulama için .NET 6.0 veya üzeri gereklidir.");
            }

            // Check client_secret.json
            if (!File.Exists("client_secret.json"))
            {
                throw new FileNotFoundException(
                    "client_secret.json dosyası bulunamadı. " +
                    "Lütfen Google Cloud Console'dan indirdiğiniz dosyayı uygulama klasörüne kopyalayın.");
            }

            // Check appsettings.json
            if (!File.Exists("appsettings.json"))
            {
                CreateDefaultAppSettings();
            }

            // Check disk space
            CheckDiskSpace();
        }

        private static void CreateDefaultAppSettings()
        {
            var defaultSettings = new
            {
                Logging = new
                {
                    LogLevel = new
                    {
                        Default = "Information",
                        Microsoft = "Warning"
                    }
                },
                UploadSettings = new
                {
                    MaxConcurrentUploads = 3,
                    DefaultPrivacyStatus = "private",
                    AutoRetryCount = 3,
                    TempDirectory = "Temp",
                    MaxFileSize = 2147483648 // 2GB
                },
                AllowedFileTypes = new[] { ".mp4", ".avi", ".mov", ".wmv", ".jpg", ".jpeg", ".png" }
            };

            var json = System.Text.Json.JsonSerializer.Serialize(defaultSettings, 
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("appsettings.json", json);
        }

        private static void CheckDiskSpace()
        {
            var drive = new DriveInfo(Path.GetPathRoot(AppContext.BaseDirectory));
            var minimumSpace = 1024L * 1024L * 1024L; // 1 GB

            if (drive.AvailableFreeSpace < minimumSpace)
            {
                throw new Exception("Yetersiz disk alanı. En az 1 GB boş alan gereklidir.");
            }
        }

        private static async Task CheckForUpdatesAsync()
        {
            try
            {
                using var client = new System.Net.Http.HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "VideoUploaderScheduler");
                
                var response = await client.GetAsync(
                    "https://api.github.com/repos/byzatwuer1/uploadert/releases/latest");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var release = System.Text.Json.JsonSerializer.Deserialize<GitHubRelease>(content);

                    var currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                    var latestVersion = new Version(release.TagName.TrimStart('v'));

                    if (latestVersion > currentVersion)
                    {
                        Log.Information("Yeni sürüm mevcut: {Version}", release.TagName);
                        // TODO: Implement update notification
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Güncelleme kontrolü başarısız");
            }
        }
    }

    public class GitHubRelease
    {
        public string TagName { get; set; }
        public string Name { get; set; }
        public string Body { get; set; }
        public bool Draft { get; set; }
        public bool Prerelease { get; set; }
    }

    public class UploadSettings
    {
        public int MaxConcurrentUploads { get; set; }
        public string DefaultPrivacyStatus { get; set; }
        public int AutoRetryCount { get; set; }
        public string TempDirectory { get; set; }
        public long MaxFileSize { get; set; }
    }
}