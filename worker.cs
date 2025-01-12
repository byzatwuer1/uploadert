using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace VideoUploaderScheduler
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly UploadScheduler _uploadScheduler;
        private readonly ConcurrentDictionary<string, UploadInfo> _activeUploads;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);
        private readonly YouTubeUploader _youtubeUploader;
        private readonly InstagramUploader _instagramUploader;
        private readonly CredentialStore _credentialStore;

        public Worker(
            ILogger<Worker> logger,
            UploadScheduler uploadScheduler,
            YouTubeUploader youtubeUploader,
            InstagramUploader instagramUploader,
            CredentialStore credentialStore)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _uploadScheduler = uploadScheduler ?? throw new ArgumentNullException(nameof(uploadScheduler));
            _youtubeUploader = youtubeUploader ?? throw new ArgumentNullException(nameof(youtubeUploader));
            _instagramUploader = instagramUploader ?? throw new ArgumentNullException(nameof(instagramUploader));
            _credentialStore = credentialStore ?? throw new ArgumentNullException(nameof(credentialStore));
            _activeUploads = new ConcurrentDictionary<string, UploadInfo>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await _uploadScheduler.StartAsync();
                _logger.LogInformation("Upload Scheduler başlatıldı: {time}", DateTimeOffset.Now);

                while (!stoppingToken.IsCancellationRequested)
                {
                    await ProcessScheduledUploads(stoppingToken);
                    await Task.Delay(_checkInterval, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Worker servisinde hata oluştu");
                throw;
            }
            finally
            {
                await _uploadScheduler.StopAsync();
                _logger.LogInformation("Upload Scheduler durduruldu: {time}", DateTimeOffset.Now);
            }
        }

        private async Task ProcessScheduledUploads(CancellationToken stoppingToken)
        {
            try
            {
                var scheduledUploads = await _uploadScheduler.GetScheduledUploads();

                foreach (var upload in scheduledUploads)
                {
                    if (stoppingToken.IsCancellationRequested)
                        break;

                    if (upload.ScheduledTime <= DateTime.UtcNow && 
                        !_activeUploads.ContainsKey(upload.JobId))
                    {
                        await ProcessUpload(upload);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Zamanlanmış yüklemeler işlenirken hata oluştu");
            }
        }

        private async Task ProcessUpload(ScheduledUploadInfo upload)
        {
            if (!_activeUploads.TryAdd(upload.JobId, new UploadInfo 
            { 
                FilePath = upload.FilePath,
                Title = upload.Title,
                Description = upload.Description,
                Platform = upload.Platform,
                ScheduledTime = upload.ScheduledTime
            }))
            {
                return;
            }

            try
            {
                _logger.LogInformation(
                    "Yükleme başlatılıyor - Platform: {platform}, Dosya: {file}", 
                    upload.Platform, 
                    upload.FilePath);

                switch (upload.Platform?.ToLower())
                {
                    case "youtube":
                        await ProcessYouTubeUpload(upload);
                        break;

                    case "instagram":
                        await ProcessInstagramUpload(upload);
                        break;

                    default:
                        _logger.LogWarning(
                            "Desteklenmeyen platform: {platform}", 
                            upload.Platform);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Yükleme işlemi sırasında hata - JobId: {jobId}", 
                    upload.JobId);
            }
            finally
            {
                _activeUploads.TryRemove(upload.JobId, out _);
            }
        }

        private async Task ProcessYouTubeUpload(ScheduledUploadInfo upload)
        {
            if (!_youtubeUploader.IsAuthenticated)
            {
                await _youtubeUploader.AuthenticateAsync();
            }

            _youtubeUploader.UploadProgressChanged += (s, e) =>
            {
                var progress = (int)((double)e.BytesSent / e.TotalBytes * 100);
                _logger.LogInformation(
                    "YouTube yükleme ilerlemesi - JobId: {jobId}, Progress: {progress}%",
                    upload.JobId,
                    progress);
            };

            var videoId = await _youtubeUploader.UploadVideoAsync(
                upload.FilePath,
                upload.Title,
                upload.Description,
                upload.Tags);

            _logger.LogInformation(
                "YouTube yükleme tamamlandı - JobId: {jobId}, VideoId: {videoId}",
                upload.JobId,
                videoId);
        }

        private async Task ProcessInstagramUpload(ScheduledUploadInfo upload)
        {
            if (!_instagramUploader.IsAuthenticated)
            {
                var credentials = await _credentialStore.LoadCredentials();
                await _instagramUploader.AuthenticateAsync(
                    credentials.InstagramUsername,
                    credentials.InstagramPassword);
            }

            _instagramUploader.UploadProgressChanged += (s, e) =>
            {
                var progress = (int)((double)e.BytesSent / e.TotalBytes * 100);
                _logger.LogInformation(
                    "Instagram yükleme ilerlemesi - JobId: {jobId}, Progress: {progress}%",
                    upload.JobId,
                    progress);
            };

            await _instagramUploader.UploadMediaAsync(
                upload.FilePath,
                upload.Description);

            _logger.LogInformation(
                "Instagram yükleme tamamlandı - JobId: {jobId}",
                upload.JobId);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Worker servisi durduruluyor...");
            await base.StopAsync(cancellationToken);
        }
    }
}