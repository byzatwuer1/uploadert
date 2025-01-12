using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Google.Apis.Util.Store;  // FileDataStore için gerekli
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class YouTubeUploader
{
    private readonly string[] Scopes = { YouTubeService.Scope.YoutubeUpload };
    private readonly string ApplicationName = "VideoUploaderScheduler";
    private readonly string clientSecretPath = "client_secret.json";

    public async Task UploadVideoAsync(string filePath, string title, string description, string[] tags)
    {
        UserCredential credential;
        using (var stream = new FileStream(clientSecretPath, FileMode.Open, FileAccess.Read))
        {
            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,  // Load yerine FromStream kullanıyoruz
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore("VideoUploaderScheduler")
            );
        }

        var youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });

        var video = new Video();
        video.Snippet = new VideoSnippet();
        video.Snippet.Title = title;
        video.Snippet.Description = description;
        video.Snippet.Tags = tags;
        video.Snippet.CategoryId = "22";
        video.Status = new VideoStatus();
        video.Status.PrivacyStatus = "private";

        using (var fileStream = new FileStream(filePath, FileMode.Open))
        {
            var videosInsertRequest = youtubeService.Videos.Insert(video, "snippet,status", fileStream, "video/*");
            await videosInsertRequest.UploadAsync();
        }
    }
}