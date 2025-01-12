using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models; // IMedia için gerekli
using InstagramApiSharp.Logger;
using System;
using System.IO;
using System.Threading.Tasks;
namespace VideoUploaderScheduler
{
    public class UploadScheduler
    {
        private readonly IScheduler _scheduler;
        private bool _isStarted;

        public UploadScheduler()
        {
            var factory = new StdSchedulerFactory();
            _scheduler = factory.GetScheduler().Result;
            _isStarted = false;
        }

        public async Task StartAsync()
        {
            if (!_isStarted)
            {
                await _scheduler.Start();
                _isStarted = true;
            }
        }

        public async Task StopAsync()
        {
            if (_isStarted)
            {
                await _scheduler.Shutdown(true);
                _isStarted = false;
            }
        }

        public async Task ScheduleUpload(DateTime uploadTime, string platform, string filePath, 
            string title, string description, string[] tags = null)
        {
            if (uploadTime <= DateTime.Now)
            {
                throw new ArgumentException("Yükleme zamanı geçmiş bir zaman olamaz.");
            }

            // Benzersiz bir job ve trigger ID oluştur
            string jobId = $"Upload_{Guid.NewGuid():N}";
            string triggerId = $"Trigger_{jobId}";

            // Job verilerini hazırla
            var jobData = new JobDataMap
            {
                { "platform", platform },
                { "filePath", filePath },
                { "title", title },
                { "description", description },
                { "tags", tags }
            };

            // Job'ı oluştur
            IJobDetail job = JobBuilder.Create<UploadJob>()
                .WithIdentity(jobId)
                .UsingJobData(jobData)
                .Build();

            // Trigger'ı oluştur
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity(triggerId)
                .StartAt(uploadTime)
                .Build();

            // Job'ı zamanla
            await _scheduler.ScheduleJob(job, trigger);
        }

        public async Task<bool> CancelUpload(string jobId)
        {
            return await _scheduler.DeleteJob(new JobKey(jobId));
        }

        public async Task<ScheduledUploadInfo[]> GetScheduledUploads()
        {
            var jobs = await _scheduler.GetCurrentlyExecutingJobs();
            var scheduledUploads = new System.Collections.Generic.List<ScheduledUploadInfo>();

            foreach (var jobContext in jobs)
            {
                var jobDetail = jobContext.JobDetail;
                var trigger = jobContext.Trigger;
                var jobData = jobDetail.JobDataMap;

                scheduledUploads.Add(new ScheduledUploadInfo
                {
                    JobId = jobDetail.Key.Name,
                    Platform = jobData.GetString("platform"),
                    FilePath = jobData.GetString("filePath"),
                    Title = jobData.GetString("title"),
                    Description = jobData.GetString("description"),
                    Tags = jobData.GetString("tags")?.Split(','),
                    ScheduledTime = trigger.GetNextFireTimeUtc()?.LocalDateTime ?? DateTime.MinValue
                });
            }

            return scheduledUploads.ToArray();
        }
    }

    public class UploadJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var jobData = context.JobDetail.JobDataMap;
            var platform = jobData.GetString("platform");
            var filePath = jobData.GetString("filePath");
            var title = jobData.GetString("title");
            var description = jobData.GetString("description");
            var tags = jobData.GetString("tags")?.Split(',');

            try
            {
                switch (platform.ToLower())
                {
                    case "youtube":
                        await YouTubeUploader.Instance.UploadVideoAsync(
                            filePath, title, description, tags);
                        break;

                    case "instagram":
                        await InstagramUploader.Instance.UploadMediaAsync(
                            filePath, description);
                        break;

                    default:
                        throw new ArgumentException($"Desteklenmeyen platform: {platform}");
                }

                // Başarılı yükleme log'u
                await LogUploadResult(context.JobDetail.Key.Name, true, null);
            }
            catch (Exception ex)
            {
                // Hata log'u
                await LogUploadResult(context.JobDetail.Key.Name, false, ex.Message);
                throw new JobExecutionException(ex);
            }
        }

        private async Task LogUploadResult(string jobId, bool success, string errorMessage)
        {
            var logEntry = new UploadLog
            {
                JobId = jobId,
                Timestamp = DateTime.Now,
                Success = success,
                ErrorMessage = errorMessage
            };

            await LogManager.SaveLogAsync(logEntry);
        }
    }

    public class ScheduledUploadInfo
    {
        public string JobId { get; set; }
        public string Platform { get; set; }
        public string FilePath { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string[] Tags { get; set; }
        public DateTime ScheduledTime { get; set; }
    }

    public class UploadLog
    {
        public string JobId { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }

    public static class LogManager
    {
        private static readonly string LogFile = "upload_logs.json";

        public static async Task SaveLogAsync(UploadLog log)
        {
            try
            {
                var logs = await LoadLogsAsync();
                logs.Add(log);
                
                var json = System.Text.Json.JsonSerializer.Serialize(logs, 
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                
                await File.WriteAllTextAsync(LogFile, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Log kaydetme hatası: {ex.Message}");
            }
        }

        public static async Task<List<UploadLog>> LoadLogsAsync()
        {
            if (!File.Exists(LogFile))
                return new List<UploadLog>();

            try
            {
                var json = await File.ReadAllTextAsync(LogFile);
                return System.Text.Json.JsonSerializer.Deserialize<List<UploadLog>>(json) 
                    ?? new List<UploadLog>();
            }
            catch
            {
                return new List<UploadLog>();
            }
        }
    }
}