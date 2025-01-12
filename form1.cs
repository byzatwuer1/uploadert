using System;
using System.Windows.Forms;

namespace VideoUploaderScheduler
{
    public partial class Form1 : Form
    {
        private readonly YouTubeUploader _youtubeUploader;
        private readonly InstagramUploader _instagramUploader;
        private readonly Scheduler _scheduler;

        public Form1()
        {
            InitializeComponent();
            _youtubeUploader = new YouTubeUploader();
            _instagramUploader = new InstagramUploader();
            _scheduler = new Scheduler();
            platformComboBox.SelectedIndex = 0;
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Video files (*.mp4;*.avi;*.mov)|*.mp4;*.avi;*.mov|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    filePathTextBox.Text = openFileDialog.FileName;
                }
            }
        }

        private async void ScheduleButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePathTextBox.Text))
                {
                    MessageBox.Show("Please select a video file.");
                    return;
                }

                scheduleButton.Enabled = false;
                statusLabel.Text = "Scheduling upload...";

                // Combine date and time
                DateTime scheduledDateTime = dateTimePicker.Value.Date.Add(timePicker.Value.TimeOfDay);

                if (scheduledDateTime <= DateTime.Now)
                {
                    MessageBox.Show("Please select a future date and time.");
                    return;
                }

                var uploadInfo = new UploadInfo
                {
                    FilePath = filePathTextBox.Text,
                    Title = titleTextBox.Text,
                    Description = descriptionTextBox.Text,
                    Platform = platformComboBox.SelectedItem.ToString(),
                    ScheduledTime = scheduledDateTime,
                    YouTubeUsername = youtubeUsernameTextBox.Text,
                    YouTubePassword = youtubePasswordTextBox.Text,
                    InstagramUsername = instagramUsernameTextBox.Text,
                    InstagramPassword = instagramPasswordTextBox.Text
                };

                await _scheduler.ScheduleJobAsync(uploadInfo);
                statusLabel.Text = $"Upload scheduled for {scheduledDateTime}";
                MessageBox.Show("Upload has been scheduled successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error scheduling upload: {ex.Message}");
                statusLabel.Text = "Error scheduling upload";
            }
            finally
            {
                scheduleButton.Enabled = true;
            }
        }
    }
}