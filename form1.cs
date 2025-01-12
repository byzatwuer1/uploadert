using System;
using System.Windows.Forms;

namespace VideoUploaderScheduler
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

    private async void ScheduleButton_Click(object sender, EventArgs e)
{
    string youtubeUsername = youtubeUsernameTextBox.Text;
    string youtubePassword = youtubePasswordTextBox.Text;
    string instagramUsername = instagramUsernameTextBox.Text;
    string instagramPassword = instagramPasswordTextBox.Text;
    string filePath = filePathTextBox.Text;
    DateTime uploadTime = dateTimePicker.Value;
    string platform = platformComboBox.SelectedItem?.ToString();

    if (string.IsNullOrEmpty(platform))
    {
        MessageBox.Show("Please select a platform");
        return;
    }

    try
    {
        Worker worker = new Worker();
        await worker.UploadVideo(filePath, platform, uploadTime, youtubeUsername, youtubePassword, instagramUsername, instagramPassword);
        MessageBox.Show("Upload scheduled successfully!");
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error scheduling upload: {ex.Message}");
    }
}
    }
}