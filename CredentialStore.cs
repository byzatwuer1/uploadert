using System;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;

namespace VideoUploaderScheduler
{
    public class CredentialStore
    {
        private const string CREDENTIALS_FILE = "credentials.json";
        
        public class Credentials
        {
            public string YouTubeRefreshToken { get; set; }
            public string InstagramUsername { get; set; }
            public string InstagramPassword { get; set; }
            public string InstagramSessionFile { get; set; }
        }

        public static async Task SaveCredentials(Credentials credentials)
        {
            try
            {
                var json = JsonSerializer.Serialize(credentials, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                await File.WriteAllTextAsync(CREDENTIALS_FILE, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Kimlik bilgileri kaydedilirken hata oluştu: {ex.Message}");
            }
        }

        public static async Task<Credentials> LoadCredentials()
        {
            try
            {
                if (!File.Exists(CREDENTIALS_FILE))
                    return new Credentials();

                var json = await File.ReadAllTextAsync(CREDENTIALS_FILE);
                var credentials = JsonSerializer.Deserialize<Credentials>(json);
                return credentials ?? new Credentials();
            }
            catch (Exception ex)
            {
                throw new Exception($"Kimlik bilgileri yüklenirken hata oluştu: {ex.Message}");
            }
        }
    }
}