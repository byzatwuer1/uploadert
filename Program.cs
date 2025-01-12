using System;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace VideoUploaderScheduler
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Uygulama başlangıç kontrollerini yap
                PerformStartupChecks();

                // Ana formu başlat
                Application.Run(new Form1());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Uygulama başlatılırken bir hata oluştu:\n\n{ex.Message}",
                    "Kritik Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static void PerformStartupChecks()
        {
            // Gerekli klasörleri oluştur
            CreateRequiredDirectories();

            // Yapılandırma dosyalarını kontrol et
            CheckConfigurationFiles();

            // Güncellemeleri kontrol et
            _ = Task.Run(CheckForUpdatesAsync);
        }

        private static void CreateRequiredDirectories()
        {
            var directories = new[]
            {
                "Logs",
                "InstagramSessions",
                "YouTubeSessions",
                "Temp"
            };

            foreach (var dir in directories)
            {
                if (!System.IO.Directory.Exists(dir))
                {
                    System.IO.Directory.CreateDirectory(dir);
                }
            }
        }

        private static void CheckConfigurationFiles()
        {
            // app.config kontrolü
            if (!System.IO.File.Exists("app.config"))
            {
                CreateDefaultAppConfig();
            }

            // YouTube client secret kontrolü
            if (!System.IO.File.Exists("client_secret.json"))
            {
                MessageBox.Show(
                    "YouTube API client_secret.json dosyası bulunamadı.\n" +
                    "Lütfen Google Cloud Console'dan indirdiğiniz client_secret.json dosyasını " +
                    "uygulama klasörüne kopyalayın.",
                    "Yapılandırma Hatası",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private static void CreateDefaultAppConfig()
        {
            var config = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<configuration>
    <appSettings>
        <add key=""MaxConcurrentUploads"" value=""3"" />
        <add key=""DefaultPrivacyStatus"" value=""private"" />
        <add key=""AutoRetryCount"" value=""3"" />
        <add key=""LogLevel"" value=""Info"" />
    </appSettings>
</configuration>";

            System.IO.File.WriteAllText("app.config", config);
        }

        private static async Task CheckForUpdatesAsync()
        {
            try
            {
                var currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                // GitHub API'yi kullanarak son sürümü kontrol et
                using (var client = new System.Net.Http.HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "VideoUploaderScheduler");
                    var response = await client.GetAsync("https://api.github.com/repos/byzatwuer1/uploadert/releases/latest");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        // JSON içeriğinden sürüm bilgisini çıkar
                        // Bu örnek için basit bir kontrol yapılıyor
                        if (content.Contains("tag_name"))
                        {
                            MessageBox.Show(
                                "Yeni bir sürüm mevcut!\n" +
                                "Lütfen GitHub sayfasından son sürümü indirin.",
                                "Güncelleme Mevcut",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Güncelleme kontrolü yapılırken hata oluştu: {ex.Message}");
                // Güncelleme kontrolü hatası kritik olmadığı için kullanıcıya gösterilmiyor
            }
        }

        public static class AppSettings
        {
            public static int MaxConcurrentUploads
            {
                get
                {
                    var value = System.Configuration.ConfigurationManager.AppSettings["MaxConcurrentUploads"];
                    return int.TryParse(value, out int result) ? result : 3;
                }
            }

            public static string DefaultPrivacyStatus
            {
                get
                {
                    return System.Configuration.ConfigurationManager.AppSettings["DefaultPrivacyStatus"] ?? "private";
                }
            }

            public static int AutoRetryCount
            {
                get
                {
                    var value = System.Configuration.ConfigurationManager.AppSettings["AutoRetryCount"];
                    return int.TryParse(value, out int result) ? result : 3;
                }
            }

            public static string LogLevel
            {
                get
                {
                    return System.Configuration.ConfigurationManager.AppSettings["LogLevel"] ?? "Info";
                }
            }
        }
    }
}