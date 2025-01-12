using System;
using System.Threading.Tasks;
using VideoUploaderScheduler;
using Quartz;
using Quartz.Impl;

namespace VideoUploaderScheduler
{
    public class Scheduler
    {
        private readonly IScheduler _scheduler;

        public Scheduler()
        {
            StdSchedulerFactory factory = new StdSchedulerFactory();
            _scheduler = factory.GetScheduler().Result;
            _scheduler.Start().Wait();
        }

        public async Task ScheduleJobAsync(UploadInfo uploadInfo)
        {
            if (uploadInfo == null)
                throw new ArgumentNullException(nameof(uploadInfo));

            // Create job with unique identity
            IJobDetail job = JobBuilder.Create<VideoUploadJob>()
                .WithIdentity($"upload_{Guid.NewGuid():N}", "uploadGroup")
                .UsingJobData("filePath", uploadInfo.FilePath)
                .UsingJobData("title", uploadInfo.Title)
                .UsingJobData("description", uploadInfo.Description)
                .UsingJobData("platform", uploadInfo.Platform)
                .Build();

            // Create trigger with scheduled time
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity($"trigger_{Guid.NewGuid():N}", "uploadGroup")
                .StartAt(uploadInfo.ScheduledTime)
                .Build();

            // Schedule the job
            await _scheduler.ScheduleJob(job, trigger);
        }

        public async Task Stop()
        {
            if (_scheduler != null)
            {
                await _scheduler.Shutdown();
            }
        }
    }

    public class VideoUploadJob : IJob
    {
        private readonly IUploadService _uploadService;

        public VideoUploadJob()
        {
            _uploadService = new UploadService();
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var dataMap = context.JobDetail.JobDataMap;
                string platform = dataMap.GetString("platform");
                string filePath = dataMap.GetString("filePath");
                string title = dataMap.GetString("title");
                string description = dataMap.GetString("description");

                await _uploadService.UploadAsync(new UploadInfo
                {
                    FilePath = filePath,
                    Title = title,
                    Description = description,
                    Platform = platform,
                    ScheduledTime = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                throw new JobExecutionException("Upload failed", ex);
            }
        }
    }

    public interface IUploadService
    {
        Task UploadAsync(UploadInfo uploadInfo);
    }

    public class UploadService : IUploadService
    {
        public async Task UploadAsync(UploadInfo uploadInfo)
        {
            switch (uploadInfo.Platform.ToLower())
            {
                case "youtube":
                    var youtubeUploader = new YouTubeUploader();
                    await youtubeUploader.UploadVideoAsync(
                        uploadInfo.FilePath,
                        uploadInfo.Title,
                        uploadInfo.Description,
                        new string[] { });
                    break;

                case "instagram":
                    var instagramUploader = InstagramUploader.Instance;
                    await instagramUploader.UploadMediaAsync(
                        uploadInfo.FilePath,
                        uploadInfo.Description);
                    break;

                default:
                    throw new ArgumentException($"Unsupported platform: {uploadInfo.Platform}");
            }
        }
    }

    public class UploadInfo
    {
        public string FilePath { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Platform { get; set; }
        public DateTime ScheduledTime { get; set; }
    }
}