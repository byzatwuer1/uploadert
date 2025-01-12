using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace VideoUploaderScheduler
{
    public static class Program
    {
        private static IConfiguration Configuration { get; set; }
        private static readonly string AppSettingsFile = "appsettings.json";
        private static readonly string LogPath = Path.Combine("Logs", "log-.txt");

        [STAThread]
        static async Task Main()
        {
            try
            {
                // Windows Forms ayarları
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Başlangıç kontrolleri
                await InitializeApplicationAsync();

                // Host oluşturma ve çalıştırma
                using var host = CreateHostBuilder().Build();
                await host.StartAsync();

                // Ana formu başlatma
                var services = host.Services;
                var mainForm = services.GetRequiredService<Form1>();
                Application.Run(mainForm);

                // Graceful shutdown
                await host.StopAsync();
            }
            catch (Exception ex)
            {
                HandleStartupError(ex);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static async Task InitializeApplicationAsync()
        {
            try
            {
                // Dizinleri oluştur
                CreateRequiredDirectories();

                // Konfigürasyon dosyasını kontrol et ve oluştur
                await EnsureConfigurationFileExistsAsync();

                // Konfigürasyonu yükle
                Configuration = CreateConfiguration();

                // Logger'ı yapılandır
                ConfigureLogging();

                // Güncellemeleri kontrol et
                await CheckForUpdatesAsync();

                // Sistem gereksinimlerini kontrol et
                CheckSystemRequirements();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Uygulama başlatma hatası", ex);
            }
        }

        private static IConfiguration CreateConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(AppSettingsFile, optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        private static void ConfigureLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.File(
                    LogPath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 31,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
        }

        private static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .UseSerilog()
                .ConfigureServices((context, services) =>
                {
                    // Core services
                    services.AddSingleton(Configuration);
                    services.AddLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.AddSerilog(dispose: true);
                    });

                    // Forms
                    services.AddSingleton<Form1>();

                    // Business services
                    services.AddSingleton<UploadScheduler>();
                    services.AddSingleton<YouTubeUploader>();
                    services.AddSingleton<InstagramUploader>();
                    services.AddSingleton<CredentialStore>();

                    // Background services
                    services.AddHostedService<Worker>();

                    // HTTP client
                    services.AddHttpClient();

                    // Additional configurations
                    ConfigureAdditionalServices(services);
                });

        private static void ConfigureAdditionalServices(IServiceCollection services)
        {
            services.Configure<UploadSettings>(Configuration.GetSection("UploadSettings"));
            services.Configure<SecuritySettings>(Configuration.GetSection("SecuritySettings"));
        }

        private static void CreateRequiredDirectories()
        {
            var directories = new[]
            {
                "Logs",
                "Config",
                "Cache",
                "Temp",
                Path.Combine("Sessions", "YouTube"),
                Path.Combine("Sessions", "Instagram"),
                Path.Combine("Config", "Credentials")
            };

            foreach (var dir in directories)
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
        }

        private static async Task EnsureConfigurationFileExistsAsync()
        {
            if (!File.Exists(AppSettingsFile))
            {
                var defaultConfig = new
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
                    SecuritySettings = new
                    {
                        EncryptCredentials = true,
                        AutoLogoutMinutes = 30,
                        AllowedFileTypes = new[] { ".mp4", ".jpg", ".jpeg", ".png" }
                    }
                };

                var json = System.Text.Json.JsonSerializer.Serialize(defaultConfig, 
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(AppSettingsFile, json);
            }
        }

        private static async Task CheckForUpdatesAsync()
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("VideoUploaderScheduler");
                var response = await client.GetAsync("https://api.github.com/repos/byzatwuer1/uploadert/releases/latest");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (content.Contains("tag_name"))
                    {
                        ShowUpdateNotification();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Güncelleme kontrolü başarısız oldu");
            }
        }

        private static void CheckSystemRequirements()
        {
            // .NET sürüm kontrolü
            if (Environment.Version.Major < 6)
            {
                throw new ApplicationException(".NET 6.0 veya üzeri gereklidir.");
            }

            // Disk alanı kontrolü
            var drive = new DriveInfo(Path.GetPathRoot(Environment.CurrentDirectory));
            var minSpace = 1024L * 1024L * 1024L; // 1GB
            if (drive.AvailableFreeSpace < minSpace)
            {
                throw new ApplicationException("Yetersiz disk alanı. En az 1GB boş alan gereklidir.");
            }

            // Gerekli dosya izinlerini kontrol et
            CheckDirectoryPermissions();
        }

        private static void CheckDirectoryPermissions()
        {
            var testDirectories = new[] { "Logs", "Config", "Temp" };
            foreach (var dir in testDirectories)
            {
                try
                {
                    var testFile = Path.Combine(dir, $"test_{Guid.NewGuid()}.tmp");
                    File.WriteAllText(testFile, "test");
                    File.Delete(testFile);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"'{dir}' dizininde yazma izni yok.", ex);
                }
            }
        }

        private static void HandleStartupError(Exception ex)
        {
            var message = "Uygulama başlatılırken kritik bir hata oluştu:\n\n" +
                         $"{ex.Message}\n\n" +
                         "Detaylı hata bilgisi log dosyasına kaydedildi.";

            Log.Fatal(ex, "Uygulama başlatma hatası");

            MessageBox.Show(
                message,
                "Kritik Hata",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private static void ShowUpdateNotification()
        {
            MessageBox.Show(
                "Yeni bir sürüm mevcut!\nLütfen GitHub sayfasından son sürümü indirin.",
                "Güncelleme Mevcut",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }

    public class UploadSettings
    {
        public int MaxConcurrentUploads { get; set; }
        public string DefaultPrivacyStatus { get; set; }
        public int AutoRetryCount { get; set; }
        public string TempDirectory { get; set; }
        public long MaxFileSize { get; set; }
    }

    public class SecuritySettings
    {
        public bool EncryptCredentials { get; set; }
        public int AutoLogoutMinutes { get; set; }
        public string[] AllowedFileTypes { get; set; }
    }
}