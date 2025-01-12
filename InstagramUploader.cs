using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.Models.Media;  // IMedia interface'i için
using InstagramApiSharp.Classes.Models.Media.InstaMedia;  // Ek medya tipleri için
using InstagramApiSharp.Logger;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace VideoUploaderScheduler
{
    public class InstagramUploader
    {
        private static InstagramUploader _instance;
        private static readonly object _lockObject = new object();
        private readonly ILogger<InstagramUploader> _logger;
        private IInstaApi _instaApi;
        private bool _isAuthenticated;
        private readonly string _sessionPath;

        public event EventHandler<UploadProgressEventArgs> UploadProgressChanged;
        public event EventHandler<UploadErrorEventArgs> UploadError;
        public event EventHandler<UploadCompletedEventArgs> UploadCompleted;
        public event EventHandler<UploadStartedEventArgs> UploadStarted;

        private InstagramUploader(ILogger<InstagramUploader> logger)
        {
            _logger = logger;
            _sessionPath = Path.Combine("Config", "instagram_session.bin");
            _isAuthenticated = false;
        }

        public static InstagramUploader Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObject)
                    {
                        _instance ??= new InstagramUploader(
                            new LoggerFactory().CreateLogger<InstagramUploader>());
                    }
                }
                return _instance;
            }
        }

        public bool IsAuthenticated => _isAuthenticated;

        public async Task AuthenticateAsync(string username, string password)
        {
            try
            {
                _logger.LogInformation("Instagram kimlik doğrulama başlatılıyor...");

                var userSession = new UserSessionData
                {
                    UserName = username,
                    Password = password
                };

                _instaApi = InstaApiBuilder.CreateBuilder()
                    .SetUser(userSession)
                    .UseLogger(new DebugLogger(LogLevel.All))
                    .Build();

                if (File.Exists(_sessionPath))
                {
                    _logger.LogInformation("Önceki oturum bulundu, yükleniyor...");
                    await LoadSessionAsync();
                }

                if (!_instaApi.IsUserAuthenticated)
                {
                    _logger.LogInformation("Yeni oturum açılıyor...");
                    var loginResult = await _instaApi.LoginAsync();
                    if (!loginResult.Succeeded)
                    {
                        throw new Exception($"Instagram girişi başarısız: {loginResult.Info.Message}");
                    }
                    await SaveSessionAsync();
                }

                _isAuthenticated = true;
                _logger.LogInformation("Instagram kimlik doğrulama başarılı");
            }
            catch (Exception ex)
            {
                _isAuthenticated = false;
                _logger.LogError(ex, "Instagram kimlik doğrulama hatası");
                throw new Exception("Instagram kimlik doğrulama başarısız", ex);
            }
        }

        private async Task LoadSessionAsync()
        {
            try
            {
                var sessionData = await File.ReadAllBytesAsync(_sessionPath);
                await _instaApi.LoadStateDataFromStringAsync(Convert.ToBase64String(sessionData));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Oturum yüklenirken hata oluştu");
                throw;
            }
        }

        private async Task SaveSessionAsync()
        {
            try
            {
                var sessionData = await _instaApi.GetStateDataAsStringAsync();
                await File.WriteAllBytesAsync(_sessionPath, 
                    Convert.FromBase64String(sessionData));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Oturum kaydedilirken hata oluştu");
                throw;
            }
        }

        public async Task<string> UploadMediaAsync(string filePath, string caption, InstagramUploadOptions options = null)
        {
            if (!_isAuthenticated)
                throw new InvalidOperationException("Instagram'a yükleme yapmadan önce kimlik doğrulaması yapmalısınız.");

            options ??= new InstagramUploadOptions();
            var fileInfo = new FileInfo(filePath);
            var mediaType = GetMediaType(filePath);

            try
            {
                OnUploadStarted(new UploadStartedEventArgs(
                    Guid.NewGuid().ToString(),
                    "Instagram",
                    filePath,
                    fileInfo.Length));

                IResult<IMedia> result;
                
                switch (mediaType)
                {
                    case MediaType.Video:
                        result = await UploadVideoAsync(filePath, caption, options);
                        break;
                    
                    case MediaType.Image:
                        result = await UploadImageAsync(filePath, caption, options);
                        break;
                    
                    default:
                        throw new NotSupportedException("Desteklenmeyen medya türü");
                }

                if (!result.Succeeded)
                {
                    throw new Exception($"Yükleme başarısız: {result.Info.Message}");
                }

                OnUploadCompleted(new UploadCompletedEventArgs(
                    result.Value.Pk,
                    result.Value.Code,
                    platform: "Instagram"));

                return result.Value.Pk;
            }
            catch (Exception ex)
            {
                OnUploadError(new UploadErrorEventArgs(
                    ex.Message,
                    platform: "Instagram",
                    exception: ex));
                throw;
            }
        }

        private async Task<IResult<InstaMedia>> UploadVideoAsync(
            string filePath, 
            string caption, 
            InstagramUploadOptions options)
        {
            try
            {
                var video = new InstaVideoUpload
                {
                    Video = new InstaVideo(filePath, 0, 0),
                    Caption = FormatCaption(caption, options)
                };

                var progress = new Progress<InstaUploaderProgress>(p =>
                {
                    OnUploadProgressChanged(new UploadProgressEventArgs(
                        p.UploadedBytes,
                        p.TotalBytes,
                        platform: "Instagram"));
                });

                return options.IsReel ?
                    await _instaApi.MediaProcessor.UploadReelVideoAsync(video, progress) :
                    await _instaApi.MediaProcessor.UploadVideoAsync(video, progress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Video yükleme hatası");
                throw;
            }
        }
public class InstagramUploadOptions
{
    public bool IsReel { get; set; } = false;
    public string[] Hashtags { get; set; } = Array.Empty<string>();
    public string[] MentionUsers { get; set; } = Array.Empty<string>();
    public string Location { get; set; } = string.Empty;
}
        private async Task<IResult<InstaMedia>> UploadImageAsync(
            string filePath, 
            string caption, 
            InstagramUploadOptions options)
        {
            try
            {
                var image = new InstaImageUpload
                {
                    Uri = filePath,
                    Caption = FormatCaption(caption, options)
                };

                return await _instaApi.MediaProcessor.UploadPhotoAsync(image);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Resim yükleme hatası");
                throw;
            }
        }

        private string FormatCaption(string caption, InstagramUploadOptions options)
        {
            var formattedCaption = caption;

            if (options.Hashtags?.Length > 0)
            {
                formattedCaption += "\n\n" + string.Join(" ", 
                    options.Hashtags.Select(tag => $"#{tag.TrimStart('#')}"));
            }

            if (options.MentionUsers?.Length > 0)
            {
                formattedCaption += "\n\n" + string.Join(" ", 
                    options.MentionUsers.Select(user => $"@{user.TrimStart('@')}"));
            }

            return formattedCaption;
        }

        private MediaType GetMediaType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLower();
            return extension switch
            {
                ".mp4" => MediaType.Video,
                ".jpg" or ".jpeg" or ".png" => MediaType.Image,
                _ => throw new NotSupportedException($"Desteklenmeyen dosya türü: {extension}")
            };
        }

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

        protected virtual void OnUploadStarted(UploadStartedEventArgs e)
        {
            UploadStarted?.Invoke(this, e);
        }

        private enum MediaType
        {
            Image,
            Video
        }
    }
}