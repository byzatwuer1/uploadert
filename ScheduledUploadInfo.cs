using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VideoUploaderScheduler
{
    public class ScheduledUploadInfo
    {
        // Temel özellikler
        public string JobId { get; set; }
        public string FilePath { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string[] Tags { get; set; }
        public string Platform { get; set; }
        public DateTime ScheduledTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public UploadStatus Status { get; set; }

        // Platform spesifik özellikler
        public YouTubeUploadOptions YouTubeOptions { get; set; }
        public InstagramUploadOptions InstagramOptions { get; set; }

        // İzleme ve hata yönetimi
        public int RetryCount { get; set; }
        public string LastError { get; set; }
        public DateTime? LastAttempt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public ScheduledUploadInfo()
        {
            JobId = Guid.NewGuid().ToString();
            CreatedAt = DateTime.UtcNow;
            Status = UploadStatus.Pending;
            Tags = Array.Empty<string>();
            YouTubeOptions = new YouTubeUploadOptions();
            InstagramOptions = new InstagramUploadOptions();
            RetryCount = 0;
        }

        public bool IsReadyToUpload()
        {
            return Status == UploadStatus.Pending && 
                   DateTime.UtcNow >= ScheduledTime && 
                   RetryCount < Constants.MaxRetryAttempts;
        }

        public void MarkAsCompleted()
        {
            Status = UploadStatus.Completed;
            CompletedAt = DateTime.UtcNow;
        }

        public void MarkAsFailed(string error)
        {
            LastError = error;
            LastAttempt = DateTime.UtcNow;
            RetryCount++;

            if (RetryCount >= Constants.MaxRetryAttempts)
            {
                Status = UploadStatus.Failed;
            }
        }

        public bool ShouldRetry()
        {
            if (Status != UploadStatus.Failed && RetryCount < Constants.MaxRetryAttempts)
            {
                var lastAttemptTime = LastAttempt ?? DateTime.MinValue;
                var waitTime = TimeSpan.FromMinutes(Math.Pow(2, RetryCount)); // Exponential backoff
                return DateTime.UtcNow >= lastAttemptTime.Add(waitTime);
            }
            return false;
        }
    }

    public class YouTubeUploadOptions
    {
        public string Privacy { get; set; } = "private";
        public string Category { get; set; } = "22"; // People & Blogs
        public bool MadeForKids { get; set; } = false;
        public string Language { get; set; } = "tr";
        public string PlaylistId { get; set; }
        public Dictionary<string, string> CustomProperties { get; set; }

        public YouTubeUploadOptions()
        {
            CustomProperties = new Dictionary<string, string>();
        }
    }

    public class InstagramUploadOptions
    {
        public bool IsReel { get; set; } = false;
        public bool ShareToFeed { get; set; } = true;
        public string[] MentionUsers { get; set; }
        public string[] Hashtags { get; set; }
        public string Location { get; set; }
        public Dictionary<string, string> CustomProperties { get; set; }

        public InstagramUploadOptions()
        {
            MentionUsers = Array.Empty<string>();
            Hashtags = Array.Empty<string>();
            CustomProperties = new Dictionary<string, string>();
        }
    }

    public enum UploadStatus
    {
        Pending,
        Processing,
        Completed,
        Failed,
        Cancelled
    }

    public static class Constants
    {
        public const int MaxRetryAttempts = 3;
        public const int MaxConcurrentUploads = 2;
        public const int DefaultChunkSize = 256 * 1024; // 256 KB
        
        public static readonly TimeSpan MaxRetryWindow = TimeSpan.FromHours(24);
        public static readonly TimeSpan MinRetryDelay = TimeSpan.FromMinutes(1);
        public static readonly TimeSpan MaxRetryDelay = TimeSpan.FromHours(1);

        public static class YouTube
        {
            public static readonly string[] AllowedPrivacySettings = { "private", "unlisted", "public" };
            public static readonly string[] AllowedFileTypes = { ".mp4", ".avi", ".mov", ".wmv" };
            public const long MaxFileSize = 128L * 1024L * 1024L * 1024L; // 128 GB
        }

        public static class Instagram
        {
            public static readonly string[] AllowedFileTypes = { ".mp4", ".jpg", ".jpeg", ".png" };
            public const long MaxVideoSize = 100L * 1024L * 1024L; // 100 MB
            public const long MaxImageSize = 8L * 1024L * 1024L; // 8 MB
            public const int MaxHashtags = 30;
            public const int MaxMentions = 20;
        }
    }

    public class UploadValidationException : Exception
    {
        public string PropertyName { get; }

        public UploadValidationException(string message, string propertyName) 
            : base(message)
        {
            PropertyName = propertyName;
        }
    }

    public static class UploadInfoValidator
    {
        public static void Validate(ScheduledUploadInfo uploadInfo)
        {
            if (string.IsNullOrWhiteSpace(uploadInfo.FilePath))
                throw new UploadValidationException("Dosya yolu boş olamaz.", nameof(uploadInfo.FilePath));

            if (!System.IO.File.Exists(uploadInfo.FilePath))
                throw new UploadValidationException("Dosya bulunamadı.", nameof(uploadInfo.FilePath));

            if (string.IsNullOrWhiteSpace(uploadInfo.Title))
                throw new UploadValidationException("Başlık boş olamaz.", nameof(uploadInfo.Title));

            if (uploadInfo.ScheduledTime < DateTime.UtcNow)
                throw new UploadValidationException("Geçmiş bir tarih seçilemez.", nameof(uploadInfo.ScheduledTime));

            ValidatePlatformSpecificOptions(uploadInfo);
        }

        private static void ValidatePlatformSpecificOptions(ScheduledUploadInfo uploadInfo)
        {
            var fileExt = System.IO.Path.GetExtension(uploadInfo.FilePath).ToLower();
            var fileInfo = new System.IO.FileInfo(uploadInfo.FilePath);

            switch (uploadInfo.Platform.ToLower())
            {
                case "youtube":
                    if (!Constants.YouTube.AllowedFileTypes.Contains(fileExt))
                        throw new UploadValidationException("Desteklenmeyen dosya türü.", nameof(uploadInfo.FilePath));

                    if (fileInfo.Length > Constants.YouTube.MaxFileSize)
                        throw new UploadValidationException("Dosya boyutu çok büyük.", nameof(uploadInfo.FilePath));

                    if (!Constants.YouTube.AllowedPrivacySettings.Contains(uploadInfo.YouTubeOptions.Privacy))
                        throw new UploadValidationException("Geçersiz gizlilik ayarı.", "Privacy");
                    break;

                case "instagram":
                    if (!Constants.Instagram.AllowedFileTypes.Contains(fileExt))
                        throw new UploadValidationException("Desteklenmeyen dosya türü.", nameof(uploadInfo.FilePath));

                    var maxSize = fileExt == ".mp4" ? 
                        Constants.Instagram.MaxVideoSize : 
                        Constants.Instagram.MaxImageSize;

                    if (fileInfo.Length > maxSize)
                        throw new UploadValidationException("Dosya boyutu çok büyük.", nameof(uploadInfo.FilePath));

                    if (uploadInfo.InstagramOptions.Hashtags?.Length > Constants.Instagram.MaxHashtags)
                        throw new UploadValidationException("Çok fazla hashtag.", "Hashtags");

                    if (uploadInfo.InstagramOptions.MentionUsers?.Length > Constants.Instagram.MaxMentions)
                        throw new UploadValidationException("Çok fazla mention.", "MentionUsers");
                    break;

                default:
                    throw new UploadValidationException("Desteklenmeyen platform.", nameof(uploadInfo.Platform));
            }
        }
    }
}