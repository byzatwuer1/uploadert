using System;
using System.Threading.Tasks;

public class Worker
{
    private readonly YouTubeUploader _youtubeUploader = new YouTubeUploader();

    public async Task UploadVideo(string filePath, string platform, DateTime uploadTime, string youtubeUsername, string youtubePassword, string instagramUsername, string instagramPassword)
    {
        // Zamanlama işlemi
        var scheduler = new Scheduler();
        await scheduler.ScheduleJobAsync(uploadTime);

        // Platforma göre yükleme işlemi
        if (platform == "YouTube")
        {
            var title = "Video Title";
            var description = "Video Description";
            var tags = new string[] { "tag1", "tag2" };
            await _youtubeUploader.UploadVideoAsync(filePath, title, description, tags);
        }
        else if (platform == "Instagram")
        {
            var instagramUploader = new InstagramUploader(instagramUsername, instagramPassword);
            instagramUploader.UploadVideo(filePath, "This is a test video upload.");
        }
    }
}