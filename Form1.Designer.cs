using System;
using System.Drawing; // Make sure to include this namespace
using System.Windows.Forms;

namespace VideoUploaderScheduler
{
    partial class Form1 : System.Windows.Forms.Form
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox youtubeUsernameTextBox;
        private System.Windows.Forms.TextBox youtubePasswordTextBox;
        private System.Windows.Forms.TextBox instagramUsernameTextBox;
        private System.Windows.Forms.TextBox instagramPasswordTextBox;
        private System.Windows.Forms.TextBox filePathTextBox;
        private System.Windows.Forms.TextBox titleTextBox;
        private System.Windows.Forms.TextBox descriptionTextBox;
        private System.Windows.Forms.DateTimePicker dateTimePicker;
        private System.Windows.Forms.DateTimePicker timePicker;
        private System.Windows.Forms.ComboBox platformComboBox;
        private System.Windows.Forms.Button scheduleButton;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.Label scheduleLabel;
        private System.Windows.Forms.Label statusLabel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            youtubeUsernameTextBox = new TextBox();
            youtubePasswordTextBox = new TextBox();
            instagramUsernameTextBox = new TextBox();
            instagramPasswordTextBox = new TextBox();
            filePathTextBox = new TextBox();
            titleTextBox = new TextBox();
            descriptionTextBox = new TextBox();
            dateTimePicker = new DateTimePicker();
            timePicker = new DateTimePicker();
            platformComboBox = new ComboBox();
            scheduleButton = new Button();
            browseButton = new Button();
            scheduleLabel = new Label();
            statusLabel = new Label();

            SuspendLayout();

            // YouTube Username
            youtubeUsernameTextBox.Location = new Point(12, 12);
            youtubeUsernameTextBox.Size = new Size(200, 20);
            youtubeUsernameTextBox.PlaceholderText = "YouTube Username";

            // YouTube Password
            youtubePasswordTextBox.Location = new Point(12, 38);
            youtubePasswordTextBox.Size = new Size(200, 20);
            youtubePasswordTextBox.UseSystemPasswordChar = true;
            youtubePasswordTextBox.PlaceholderText = "YouTube Password";

            // Instagram Username
            instagramUsernameTextBox.Location = new Point(12, 64);
            instagramUsernameTextBox.Size = new Size(200, 20);
            instagramUsernameTextBox.PlaceholderText = "Instagram Username";

            // Instagram Password
            instagramPasswordTextBox.Location = new Point(12, 90);
            instagramPasswordTextBox.Size = new Size(200, 20);
            instagramPasswordTextBox.UseSystemPasswordChar = true;
            instagramPasswordTextBox.PlaceholderText = "Instagram Password";

            // File Path
            filePathTextBox.Location = new Point(12, 116);
            filePathTextBox.Size = new Size(160, 20);
            filePathTextBox.PlaceholderText = "Video File Path";

            // Browse Button
            browseButton.Location = new Point(172, 116);
            browseButton.Size = new Size(40, 20);
            browseButton.Text = "...";
            browseButton.Click += new EventHandler(BrowseButton_Click);

            // Title
            titleTextBox.Location = new Point(12, 142);
            titleTextBox.Size = new Size(200, 20);
            titleTextBox.PlaceholderText = "Video Title";

            // Description
            descriptionTextBox.Location = new Point(12, 168);
            descriptionTextBox.Size = new Size(200, 60);
            descriptionTextBox.Multiline = true;
            descriptionTextBox.PlaceholderText = "Video Description";

            // Schedule Label
            scheduleLabel.Location = new Point(12, 234);
            scheduleLabel.Size = new Size(200, 20);
            scheduleLabel.Text = "Schedule Date and Time:";

            // Date Picker
            dateTimePicker.Location = new Point(12, 254);
            dateTimePicker.Size = new Size(200, 20);
            dateTimePicker.Format = DateTimePickerFormat.Short;

            // Time Picker
            timePicker.Location = new Point(12, 280);
            timePicker.Size = new Size(200, 20);
            timePicker.Format = DateTimePickerFormat.Time;
            timePicker.ShowUpDown = true;

            // Platform ComboBox
            platformComboBox.Location = new Point(12, 306);
            platformComboBox.Size = new Size(200, 21);
            platformComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            platformComboBox.Items.AddRange(new object[] { "YouTube", "Instagram" });

            // Schedule Button
            scheduleButton.Location = new Point(12, 332);
            scheduleButton.Size = new Size(200, 23);
            scheduleButton.Text = "Schedule Upload";
            scheduleButton.Click += new EventHandler(ScheduleButton_Click);

            // Status Label
            statusLabel.Location = new Point(12, 358);
            statusLabel.Size = new Size(200, 40);
            statusLabel.AutoSize = true;

            // Form
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(224, 410);
            Controls.AddRange(new Control[] {
                youtubeUsernameTextBox,
                youtubePasswordTextBox,
                instagramUsernameTextBox,
                instagramPasswordTextBox,
                filePathTextBox,
                browseButton,
                titleTextBox,
                descriptionTextBox,
                scheduleLabel,
                dateTimePicker,
                timePicker,
                platformComboBox,
                scheduleButton,
                statusLabel
            });

            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "Form1";
            Text = "Video Uploader Scheduler";
            ResumeLayout(false);
            PerformLayout();
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Video files (*.mp4;*.avi;*.mov)|*.mp4;*.avi;*.mov|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    filePathTextBox.Text = openFileDialog.FileName;
                }
            }
        }

        private void ScheduleButton_Click(object sender, EventArgs e)
        {
            // Collect data from form fields
            var uploadInfo = new UploadInfo
            {
                FilePath = filePathTextBox.Text,
                Title = titleTextBox.Text,
                Description = descriptionTextBox.Text,
                Platform = platformComboBox.SelectedItem.ToString(),
                ScheduledTime = dateTimePicker.Value.Date + timePicker.Value.TimeOfDay,
                YouTubeUsername = youtubeUsernameTextBox.Text,
                YouTubePassword = youtubePasswordTextBox.Text,
                InstagramUsername = instagramUsernameTextBox.Text,
                InstagramPassword = instagramPasswordTextBox.Text
            };

            // Schedule the upload
            var scheduler = new Scheduler();
            scheduler.ScheduleJobAsync(uploadInfo).Wait();

            statusLabel.Text = "Upload scheduled successfully!";
        }
    }
}