using System;

namespace VideoUploaderScheduler
{
    public class UploadProgressEventArgs : EventArgs
    {
        public long BytesSent { get; }
        public long TotalBytes { get; }
        public double ProgressPercentage => (double)BytesSent / TotalBytes * 100;
        public DateTime Timestamp { get; }
        public string JobId { get; }
        public string Platform { get; }
        public TimeSpan? EstimatedTimeRemaining { get; }
        public double? CurrentSpeed { get; } // bytes per second

        public UploadProgressEventArgs(
            long bytesSent, 
            long totalBytes, 
            string jobId = "", 
            string platform = "",
            TimeSpan? estimatedTimeRemaining = null,
            double? currentSpeed = null)
        {
            BytesSent = bytesSent;
            TotalBytes = totalBytes;
            Timestamp = DateTime.UtcNow;
            JobId = jobId;
            Platform = platform;
            EstimatedTimeRemaining = estimatedTimeRemaining;
            CurrentSpeed = currentSpeed;
        }
    }

    public class UploadErrorEventArgs : EventArgs
    {
        public string ErrorMessage { get; }
        public Exception Exception { get; }
        public string JobId { get; }
        public string Platform { get; }
        public DateTime Timestamp { get; }
        public bool CanRetry { get; }
        public int RetryCount { get; }
        public TimeSpan? RetryAfter { get; }

        public UploadErrorEventArgs(
            string errorMessage, 
            string jobId = "", 
            string platform = "",
            Exception exception = null,
            bool canRetry = true,
            int retryCount = 0,
            TimeSpan? retryAfter = null)
        {
            ErrorMessage = errorMessage;
            JobId = jobId;
            Platform = platform;
            Exception = exception;
            Timestamp = DateTime.UtcNow;
            CanRetry = canRetry;
            RetryCount = retryCount;
            RetryAfter = retryAfter;
        }
    }

    public class UploadCompletedEventArgs : EventArgs
    {
        public string MediaId { get; }
        public string MediaUrl { get; }
        public string JobId { get; }
        public string Platform { get; }
        public DateTime Timestamp { get; }
        public TimeSpan Duration { get; }
        public long TotalBytes { get; }
        public Dictionary<string, string> AdditionalData { get; }

        public UploadCompletedEventArgs(
            string mediaId, 
            string mediaUrl, 
            string jobId = "",
            string platform = "",
            TimeSpan? duration = null,
            long totalBytes = 0,
            Dictionary<string, string> additionalData = null)
        {
            MediaId = mediaId;
            MediaUrl = mediaUrl;
            JobId = jobId;
            Platform = platform;
            Timestamp = DateTime.UtcNow;
            Duration = duration ?? TimeSpan.Zero;
            TotalBytes = totalBytes;
            AdditionalData = additionalData ?? new Dictionary<string, string>();
        }
    }

    public class UploadStartedEventArgs : EventArgs
    {
        public string JobId { get; }
        public string Platform { get; }
        public string FilePath { get; }
        public long FileSize { get; }
        public DateTime Timestamp { get; }
        public Dictionary<string, string> UploadParameters { get; }

        public UploadStartedEventArgs(
            string jobId,
            string platform,
            string filePath,
            long fileSize,
            Dictionary<string, string> uploadParameters = null)
        {
            JobId = jobId;
            Platform = platform;
            FilePath = filePath;
            FileSize = fileSize;
            Timestamp = DateTime.UtcNow;
            UploadParameters = uploadParameters ?? new Dictionary<string, string>();
        }
    }

    public class UploadCanceledEventArgs : EventArgs
    {
        public string JobId { get; }
        public string Platform { get; }
        public DateTime Timestamp { get; }
        public string Reason { get; }
        public bool CanResume { get; }
        public long BytesUploaded { get; }

        public UploadCanceledEventArgs(
            string jobId,
            string platform,
            string reason = "",
            bool canResume = false,
            long bytesUploaded = 0)
        {
            JobId = jobId;
            Platform = platform;
            Timestamp = DateTime.UtcNow;
            Reason = reason;
            CanResume = canResume;
            BytesUploaded = bytesUploaded;
        }
    }

    public class UploadPausedEventArgs : EventArgs
    {
        public string JobId { get; }
        public string Platform { get; }
        public DateTime Timestamp { get; }
        public string Reason { get; }
        public long BytesUploaded { get; }
        public TimeSpan? ResumeAfter { get; }

        public UploadPausedEventArgs(
            string jobId,
            string platform,
            string reason = "",
            long bytesUploaded = 0,
            TimeSpan? resumeAfter = null)
        {
            JobId = jobId;
            Platform = platform;
            Timestamp = DateTime.UtcNow;
            Reason = reason;
            BytesUploaded = bytesUploaded;
            ResumeAfter = resumeAfter;
        }
    }

    public static class UploadEventExtensions
    {
        public static string FormatSpeed(double bytesPerSecond)
        {
            string[] sizes = { "B/s", "KB/s", "MB/s", "GB/s" };
            int order = 0;
            double speed = bytesPerSecond;

            while (speed >= 1024 && order < sizes.Length - 1)
            {
                order++;
                speed = speed / 1024;
            }

            return $"{speed:0.##} {sizes[order]}";
        }

        public static string FormatTime(TimeSpan timeSpan)
        {
            if (timeSpan.TotalSeconds < 60)
                return $"{timeSpan.Seconds}s";
            if (timeSpan.TotalMinutes < 60)
                return $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
            return $"{timeSpan.Hours}h {timeSpan.Minutes}m {timeSpan.Seconds}s";
        }
    }
}