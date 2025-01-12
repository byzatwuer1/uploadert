using Quartz;
using Quartz.Impl;
using System;
using System.Threading.Tasks;

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
        // Create job
        IJobDetail job = JobBuilder.Create<VideoUploadJob>()
            .WithIdentity($"upload_{DateTime.Now.Ticks}", "uploadGroup")
            .UsingJobData("filePath", uploadInfo.FilePath)
            .UsingJobData("title", uploadInfo.Title)
            .UsingJobData("description", uploadInfo.Description)
            .UsingJobData("platform", uploadInfo.Platform)
            .UsingJobData("youtubeUsername", uploadInfo.YouTubeUsername)
            .UsingJobData("youtubePassword", uploadInfo.YouTubePassword)
            .UsingJobData("instagramUsername", uploadInfo.InstagramUsername)
            .UsingJobData("instagramPassword", uploadInfo.InstagramPassword)
            .Build();

        // Create trigger
        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity($"trigger_{DateTime.Now.Ticks}", "uploadGroup")
            .StartAt(uploadInfo.ScheduledTime)
            .Build();

        // Schedule the job
        await _scheduler.ScheduleJob(job, trigger);
    }
}

public class VideoUploadJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var dataMap = context.JobDetail.JobDataMap;
            string platform = dataMap.GetString("platform");
            string filePath = dataMap.GetString("filePath");
            string title = dataMap.GetString("title");
            string description = dataMap.GetString("description");

            if (platform == "YouTube")
            {
                var uploader = new YouTubeUploader();
                await uploader.AuthenticateAsync();
                await uploader.UploadVideoAsync(filePath, title, description, new string[] { });
            }
            else if (platform == "Instagram")
            {
                var uploader = new InstagramUploader(
                    dataMap.GetString("instagramUsername"),
                    dataMap.GetString("instagramPassword")
                );
                await uploader.UploadVideoAsync(filePath, description);
            }
        }
        catch (Exception ex)
        {
            throw new JobExecutionException(ex);
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
    public string YouTubeUsername { get; set; }
    public string YouTubePassword { get; set; }
    public string InstagramUsername { get; set; }
    public string InstagramPassword { get; set; }
}