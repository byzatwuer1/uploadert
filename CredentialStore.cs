using System;
using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace VideoUploaderScheduler
{
    public class CredentialStore
    {
        private readonly ILogger<CredentialStore> _logger;
        private readonly string _credentialsPath;
        private readonly string _encryptionKeyPath;
        private static readonly byte[] Salt = new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 };

        public CredentialStore(ILogger<CredentialStore> logger)
        {
            _logger = logger;
            _credentialsPath = Path.Combine("Config", "credentials.enc");
            _encryptionKeyPath = Path.Combine("Config", "key.dat");
            
            EnsureDirectoryExists();
            EnsureEncryptionKeyExists();
        }

        private void EnsureDirectoryExists()
        {
            var directory = Path.GetDirectoryName(_credentialsPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private void EnsureEncryptionKeyExists()
        {
            if (!File.Exists(_encryptionKeyPath))
            {
                using (var rng = new RNGCryptoServiceProvider())
                {
                    var key = new byte[32];
                    rng.GetBytes(key);
                    File.WriteAllBytes(_encryptionKeyPath, key);
                }
            }
        }

        public async Task SaveCredentials(Credentials credentials)
        {
            try
            {
                var json = JsonSerializer.Serialize(credentials);
                var encryptedData = await EncryptAsync(json);
                await File.WriteAllBytesAsync(_credentialsPath, encryptedData);
                _logger.LogInformation("Kimlik bilgileri başarıyla kaydedildi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kimlik bilgileri kaydedilirken hata oluştu");
                throw new Exception("Kimlik bilgileri kaydedilemedi", ex);
            }
        }

        public async Task<Credentials> LoadCredentials()
        {
            try
            {
                if (!File.Exists(_credentialsPath))
                {
                    return new Credentials();
                }

                var encryptedData = await File.ReadAllBytesAsync(_credentialsPath);
                var json = await DecryptAsync(encryptedData);
                return JsonSerializer.Deserialize<Credentials>(json) ?? new Credentials();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kimlik bilgileri yüklenirken hata oluştu");
                return new Credentials();
            }
        }

        private async Task<byte[]> EncryptAsync(string plainText)
        {
            try
            {
                var key = await File.ReadAllBytesAsync(_encryptionKeyPath);

                using var aes = Aes.Create();
                var keyDerivation = new Rfc2898DeriveBytes(key, Salt);
                aes.Key = keyDerivation.GetBytes(32);
                aes.IV = keyDerivation.GetBytes(16);

                using var msEncrypt = new MemoryStream();
                using (var cryptoStream = new CryptoStream(msEncrypt, aes.CreateEncryptor(), CryptoStreamMode.Write))
                using (var swEncrypt = new StreamWriter(cryptoStream))
                {
                    await swEncrypt.WriteAsync(plainText);
                }

                return msEncrypt.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Şifreleme işlemi sırasında hata oluştu");
                throw new Exception("Şifreleme işlemi başarısız oldu", ex);
            }
        }

        private async Task<string> DecryptAsync(byte[] cipherText)
        {
            try
            {
                var key = await File.ReadAllBytesAsync(_encryptionKeyPath);

                using var aes = Aes.Create();
                var keyDerivation = new Rfc2898DeriveBytes(key, Salt);
                aes.Key = keyDerivation.GetBytes(32);
                aes.IV = keyDerivation.GetBytes(16);

                using var msDecrypt = new MemoryStream(cipherText);
                using var cryptoStream = new CryptoStream(msDecrypt, aes.CreateDecryptor(), CryptoStreamMode.Read);
                using var srDecrypt = new StreamReader(cryptoStream);

                return await srDecrypt.ReadToEndAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Şifre çözme işlemi sırasında hata oluştu");
                throw new Exception("Şifre çözme işlemi başarısız oldu", ex);
            }
        }

        public async Task ClearCredentials()
        {
            try
            {
                if (File.Exists(_credentialsPath))
                {
                    File.Delete(_credentialsPath);
                }
                _logger.LogInformation("Kimlik bilgileri başarıyla temizlendi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kimlik bilgileri temizlenirken hata oluştu");
                throw new Exception("Kimlik bilgileri temizlenemedi", ex);
            }
        }

        public async Task<bool> HasCredentials()
        {
            return File.Exists(_credentialsPath);
        }
    }

    public class Credentials
    {
        public string InstagramUsername { get; set; } = string.Empty;
        public string InstagramPassword { get; set; } = string.Empty;
        public string InstagramSessionFile { get; set; } = string.Empty;
        public string YouTubeRefreshToken { get; set; } = string.Empty;
    }

    public static class EncryptionHelper
    {
        public static string GenerateRandomKey(int length = 32)
        {
            using var rng = new RNGCryptoServiceProvider();
            var bytes = new byte[length];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        public static byte[] GenerateRandomBytes(int length = 32)
        {
            using var rng = new RNGCryptoServiceProvider();
            var bytes = new byte[length];
            rng.GetBytes(bytes);
            return bytes;
        }
    }
}