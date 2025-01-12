using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace VideoUploaderScheduler
{
    public class YouTubeUploader
    {
        private static YouTubeUploader _instance;
        private static readonly object _lockObject = new object();
        private YouTubeService _youtubeService;
        private bool _isAuthenticated;
        private readonly string _clientSecretPath = "client_secret.json";
        private readonly string _credentialPath = "youtube_credentials";

        public YouTubeUploader()
        {
            _isAuthenticated = false;
            if (!Directory.Exists(_credentialPath))
            {
                Directory.CreateDirectory(_credentialPath);
            }
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
            try
            {
                if (!File.Exists(_clientSecretPath))
                {
                    throw new FileNotFoundException(
                        "client_secret.json dosyası bulunamadı. " +
                        "Lütfen Google Cloud Console'dan indirdiğiniz client_secret.json dosyasını uygulama klasörüne kopyalayın.");
                }

                using (var stream = new FileStream(_clientSecretPath, FileMode.Open, FileAccess.Read))
                {
                    var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        new[] 
                        { 
                            YouTubeService.Scope.YoutubeUpload,
                            YouTubeService.Scope.YoutubeReadonly 
                        },
                        "user",
                        CancellationToken.None,
                        new FileDataStore(_credentialPath, true)
                    );

                    _youtubeService = new YouTubeService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "VideoUploaderScheduler"
                    });

                    // Test authentication
                    var channelsListRequest = _youtubeService.Channels.List("snippet");
                    channelsListRequest.Mine = true;
                    var channelsListResponse = await channelsListRequest.ExecuteAsync();

                    if (channelsListResponse.Items.Count == 0)
                    {
                        throw new Exception("YouTube kanalı bulunamadı.");
                    }

                    _isAuthenticated = true;
                }
            }
            catch (Exception ex)
            {
                _isAuthenticated = false;
                throw new Exception("YouTube kimlik doğrulama hatası: " + ex.Message, ex);
            }
        }

        public async Task<string> UploadVideoAsync(string filePath, string title, string description, string[] tags, string privacyStatus = "private")
        {
            if (!_isAuthenticated)
                throw new InvalidOperationException("YouTube'a yükleme yapmadan önce kimlik doğrulaması yapmalısınız.");

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Video dosyası bulunamadı: {filePath}");

            var video = new Video
            {
                Snippet = new VideoSnippet
                {
                    Title = title,
                    Description = description,
                    Tags = tags ?? new string[] { },
                    CategoryId = "22" // People & Blogs
                },
                Status = new VideoStatus
                {
                    PrivacyStatus = privacyStatus,
                    SelfDeclaredMadeForKids = false
                }
            };

            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                var videosInsertRequest = _youtubeService.Videos.Insert(
                    video,
                    "snippet,status",
                    fileStream,
                    GetMimeTypeFromFileName(filePath));

                videosInsertRequest.ChunkSize = ResumableUpload.MinimumChunkSize;
                videosInsertRequest.ProgressChanged += VideosInsertRequest_ProgressChanged;
                videosInsertRequest.ResponseReceived += VideosInsertRequest_ResponseReceived;

                try
                {
                    OnUploadProgressChanged(new UploadProgressEventArgs(0, fileStream.Length));
                    var uploadResult = await videosInsertRequest.UploadAsync();

                    if (uploadResult.Status == UploadStatus.Failed)
                    {
                        var error = uploadResult.Exception?.Message ?? "Bilinmeyen hata";
                        OnUploadError(new UploadErrorEventArgs(error));
                        throw new Exception($"Video yükleme başarısız: {error}");
                    }

                    return uploadResult.VideoId;
                }
                catch (Exception ex)
                {
                    OnUploadError(new UploadErrorEventArgs(ex.Message));
                    throw new Exception("Video yükleme hatası: " + ex.Message, ex);
                }
            }
        }

        private string GetMimeTypeFromFileName(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLower();
            return ext switch
            {
                ".mp4" => "video/mp4",
                ".avi" => "video/avi",
                ".mov" => "video/quicktime",
                ".wmv" => "video/x-ms-wmv",
                _ => "video/mp4" // Default to mp4
            };
        }

        private void VideosInsertRequest_ProgressChanged(IUploadProgress progress)
        {
            switch (progress.Status)
            {
                case UploadStatus.Uploading:
                    OnUploadProgressChanged(new UploadProgressEventArgs(
                        progress.BytesSent,
                        progress.BytesSent)); // Total bytes burada düzeltilmeli
                    break;

                case UploadStatus.Failed:
                    OnUploadError(new UploadErrorEventArgs(
                        progress.Exception?.Message ?? "Bilinmeyen hata"));
                    break;

                case UploadStatus.Completed:
                    OnUploadProgressChanged(new UploadProgressEventArgs(
                        progress.BytesSent,
                        progress.BytesSent));
                    break;
            }
        }

        private void VideosInsertRequest_ResponseReceived(Video video)
        {
            OnUploadCompleted(new UploadCompletedEventArgs(
                video.Id,
                $"https://youtube.com/watch?v={video.Id}"));
        }

        public async Task<List<PlaylistItem>> GetUploadedVideos(int maxResults = 50)
        {
            if (!_isAuthenticated)
                throw new InvalidOperationException("YouTube API'ye erişim için kimlik doğrulaması gerekli.");

            try
            {
                var channelsListRequest = _youtubeService.Channels.List("contentDetails");
                channelsListRequest.Mine = true;
                var channelsListResponse = await channelsListRequest.ExecuteAsync();

                if (channelsListResponse.Items.Count == 0)
                    return new List<PlaylistItem>();

                var uploadPlaylistId = channelsListResponse.Items[0].ContentDetails.RelatedPlaylists.Uploads;

                var playlistItemsRequest = _youtubeService.PlaylistItems.List("snippet");
                playlistItemsRequest.PlaylistId = uploadPlaylistId;
                playlistItemsRequest.MaxResults = maxResults;

                var playlistItemsResponse = await playlistItemsRequest.ExecuteAsync();
                return new List<PlaylistItem>(playlistItemsResponse.Items);
            }
            catch (Exception ex)
            {
                throw new Exception("Yüklenen videoları getirme hatası: " + ex.Message, ex);
            }
        }

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