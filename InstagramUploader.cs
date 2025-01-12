using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Logger;
using System;
using System.IO;
using System.Threading.Tasks;

namespace VideoUploaderScheduler
{
    public class InstagramUploader
    {
        private IInstaApi _instaApi;
        private static readonly object _lockObject = new object();
        private static InstagramUploader _instance;
        private bool _isAuthenticated;
        private readonly string _sessionFolder = "InstagramSessions";

        private InstagramUploader()
        {
            _isAuthenticated = false;
            if (!Directory.Exists(_sessionFolder))
            {
                Directory.CreateDirectory(_sessionFolder);
            }
        }

        public static InstagramUploader Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObject)
                    {
                        _instance ??= new InstagramUploader();
                    }
                }
                return _instance;
            }
        }

        public async Task AuthenticateAsync(string username, string password)
        {
            try
            {
                var userSession = new UserSessionData
                {
                    UserName = username,
                    Password = password
                };

                _instaApi = InstaApiBuilder.CreateBuilder()
                    .SetUser(userSession)
                    .UseLogger(new DebugLogger(LogLevel.Exceptions))
                    .Build();

                // Önceki oturum verisi varsa yükle
                var sessionFile = Path.Combine(_sessionFolder, $"{username}_session.bin");
                if (File.Exists(sessionFile))
                {
                    var sessionData = await File.ReadAllBytesAsync(sessionFile);
                    await _instaApi.LoadStateDataFromStreamAsync(new MemoryStream(sessionData));
                }

                if (!_instaApi.IsUserAuthenticated)
                {
                    // Login
                    var loginResult = await _instaApi.LoginAsync();
                    if (!loginResult.Succeeded)
                    {
                        throw new Exception($"Instagram login hatası: {loginResult.Info.Message}");
                    }

                    // Oturum verilerini kaydet
                    var state = _instaApi.GetStateDataAsStream();
                    using (var fileStream = File.Create(sessionFile))
                    {
                        state.Seek(0, SeekOrigin.Begin);
                        await state.CopyToAsync(fileStream);
                    }
                }

                _isAuthenticated = true;

                // Kimlik bilgilerini kaydet
                var credentials = await CredentialStore.LoadCredentials();
                credentials.InstagramUsername = username;
                credentials.InstagramSessionFile = sessionFile;
                await CredentialStore.SaveCredentials(credentials);
            }
            catch (Exception ex)
            {
                _isAuthenticated = false;
                throw new Exception($"Instagram kimlik doğrulama hatası: {ex.Message}", ex);
            }
        }

        public async Task UploadMediaAsync(string filePath, string caption)
        {
            if (!_isAuthenticated)
            {
                throw new InvalidOperationException("Instagram'a yükleme yapmadan önce kimlik doğrulaması yapmalısınız.");
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Yüklenecek medya dosyası bulunamadı: {filePath}");
            }

            try
            {
                var extension = Path.GetExtension(filePath).ToLower();
                var isVideo = extension == ".mp4" || extension == ".mov";

                if (isVideo)
                {
                    await UploadVideoAsync(filePath, caption);
                }
                else
                {
                    await UploadPhotoAsync(filePath, caption);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Medya yükleme hatası: {ex.Message}", ex);
            }
        }

        private async Task UploadPhotoAsync(string photoPath, string caption)
        {
            try
            {
                OnUploadProgressChanged(new UploadProgressEventArgs(0, 100));

                var mediaUploadResult = await _instaApi.MediaProcessor.UploadPhotoAsync(photoPath, caption);
                
                if (!mediaUploadResult.Succeeded)
                {
                    throw new Exception($"Fotoğraf yükleme başarısız: {mediaUploadResult.Info.Message}");
                }

                OnUploadProgressChanged(new UploadProgressEventArgs(100, 100));
                OnUploadCompleted(new UploadCompletedEventArgs(
                    mediaUploadResult.Value.Pk.ToString(),
                    $"https://instagram.com/p/{mediaUploadResult.Value.Code}"
                ));
            }
            catch (Exception ex)
            {
                OnUploadError(new UploadErrorEventArgs(ex.Message));
                throw;
            }
        }

        private async Task UploadVideoAsync(string videoPath, string caption)
        {
            try
            {
                OnUploadProgressChanged(new UploadProgressEventArgs(0, 100));

                var mediaUploadResult = await _instaApi.MediaProcessor.UploadVideoAsync(videoPath, caption);
                
                if (!mediaUploadResult.Succeeded)
                {
                    throw new Exception($"Video yükleme başarısız: {mediaUploadResult.Info.Message}");
                }

                OnUploadProgressChanged(new UploadProgressEventArgs(100, 100));
                OnUploadCompleted(new UploadCompletedEventArgs(
                    mediaUploadResult.Value.Pk.ToString(),
                    $"https://instagram.com/p/{mediaUploadResult.Value.Code}"
                ));
            }
            catch (Exception ex)
            {
                OnUploadError(new UploadErrorEventArgs(ex.Message));
                throw;
            }
        }

        public async Task CheckAuthenticationStatus()
        {
            if (_instaApi != null)
            {
                var currentUser = await _instaApi.UserProcessor.GetCurrentUserAsync();
                _isAuthenticated = currentUser.Succeeded;
            }
            else
            {
                _isAuthenticated = false;
            }
        }

        // Event handlers for upload progress
        public event EventHandler<UploadProgressEventArgs> UploadProgressChanged;
        public event EventHandler<UploadErrorEventArgs> UploadError;
        public event EventHandler<UploadCompletedEventArgs> UploadCompleted;

        protected virtual void OnUploadProgressChanged(UploadProgressEventArgs e)
        {
            UploadProgressChanged?.Invoke(this, e);
        }

        protected virtual void OnUploadError(UploadErrorEventArgs e)
        {
            UploadError?.Invoke(this, e);
        }

        protected virtual void OnUploadCompleted(UploadCompletedEventArgs e)
        {
            UploadCompleted?.Invoke(this, e);
        }

        public bool IsAuthenticated => _isAuthenticated;
    }
}