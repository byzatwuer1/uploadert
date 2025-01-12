using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace VideoUploaderScheduler
{
    public class YouTubeUploader
    {
        private readonly string[] Scopes = { YouTubeService.Scope.YoutubeUpload };
        private readonly string ApplicationName = "VideoUploaderScheduler";
        private readonly string ClientSecretPath = "client_secret.json";
        private YouTubeService _youtubeService;
        private static readonly object _lockObject = new object();
        private static YouTubeUploader _instance;
        private bool _isAuthenticated;

        private YouTubeUploader()
        {
            _isAuthenticated = false;
        }

        public static YouTubeUploader Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObject)
                    {
                        _instance ??= new YouTubeUploader();
                    }
                }
                return _instance;
            }
        }

        public async Task AuthenticateAsync()
        {
            if (!File.Exists(ClientSecretPath))
            {
                throw new FileNotFoundException(
                    "client_secret.json dosyası bulunamadı. Lütfen Google Cloud Console'dan indirdiğiniz client_secret.json dosyasını uygulama dizinine kopyalayın.");
            }

            try
            {
                UserCredential credential;
                using (var stream = new FileStream(ClientSecretPath, FileMode.Open, FileAccess.Read))
                {
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore("YouTube.Upload.Auth.Store")
                    );
                }

                _youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName
                });

                _isAuthenticated = true;

                // Kimlik bilgilerini kaydet
                var credentials = await CredentialStore.LoadCredentials();
                credentials.YouTubeRefreshToken = credential.Token.RefreshToken;
                await CredentialStore.SaveCredentials(credentials);
            }
            catch (Exception ex)
            {
                _isAuthenticated = false;
                throw new Exception($"YouTube kimlik doğrulama hatası: {ex.Message}", ex);
            }
        }

        public async Task UploadVideoAsync(string filePath, string title, string description, string[] tags, string privacyStatus = "private")
        {
            if (!_isAuthenticated)
            {
                throw new InvalidOperationException("YouTube'a yükleme yapmadan önce kimlik doğrulaması yapmalısınız.");
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Yüklenecek video dosyası bulunamadı: {filePath}");
            }

            try
            {
                var video = new Video
                {
                    Snippet = new VideoSnippet
                    {
                        Title = title,
                        Description = description,
                        Tags = tags,
                        CategoryId = "22" // Entertainment category
                    },
                    Status = new VideoStatus
                    {
                        PrivacyStatus = privacyStatus // "private", "public", or "unlisted"
                    }
                };

                using (var fileStream = new FileStream(filePath, FileMode.Open))
                {
                    var videosInsertRequest = _youtubeService.Videos.Insert(
                        video,
                        "snippet,status",
                        fileStream,
                        "video/*"
                    );

                    videosInsertRequest.ProgressChanged += VideosInsertRequest_ProgressChanged;
                    videosInsertRequest.ResponseReceived += VideosInsertRequest_ResponseReceived;

                    await videosInsertRequest.UploadAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Video yükleme hatası: {ex.Message}", ex);
            }
        }

        private void VideosInsertRequest_ProgressChanged(Google.Apis.Upload.IUploadProgress progress)
        {
            switch (progress.Status)
            {
                case Google.Apis.Upload.UploadStatus.Uploading:
                    OnUploadProgressChanged(new UploadProgressEventArgs(
                        progress.BytesSent,
                        progress.TotalBytes ?? 0
                    ));
                    break;

                case Google.Apis.Upload.UploadStatus.Failed:
                    OnUploadError(new UploadErrorEventArgs(
                        "Video yükleme başarısız oldu: " + progress.Exception?.Message
                    ));
                    break;
            }
        }

        private void VideosInsertRequest_ResponseReceived(Video video)
        {
            OnUploadCompleted(new UploadCompletedEventArgs(
                video.Id,
                $"https://www.youtube.com/watch?v={video.Id}"
            ));
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
    }

    public class UploadProgressEventArgs : EventArgs
    {
        public long BytesSent { get; }
        public long TotalBytes { get; }
        public double ProgressPercentage => (double)BytesSent / TotalBytes * 100;

        public UploadProgressEventArgs(long bytesSent, long totalBytes)
        {
            BytesSent = bytesSent;
            TotalBytes = totalBytes;
        }
    }

    public class UploadErrorEventArgs : EventArgs
    {
        public string ErrorMessage { get; }

        public UploadErrorEventArgs(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }

    public class UploadCompletedEventArgs : EventArgs
    {
        public string VideoId { get; }
        public string VideoUrl { get; }

        public UploadCompletedEventArgs(string videoId, string videoUrl)
        {
            VideoId = videoId;
            VideoUrl = videoUrl;
        }
    }
}