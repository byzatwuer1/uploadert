namespace VideoUploaderScheduler
{
    partial class Form1 : System.Windows.Forms.Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox youtubeUsernameTextBox;
        private System.Windows.Forms.TextBox youtubePasswordTextBox;
        private System.Windows.Forms.TextBox instagramUsernameTextBox;
        private System.Windows.Forms.TextBox instagramPasswordTextBox;
        private System.Windows.Forms.TextBox filePathTextBox;
        private System.Windows.Forms.DateTimePicker dateTimePicker;
        private System.Windows.Forms.ComboBox platformComboBox;
        private System.Windows.Forms.Button scheduleButton;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            youtubeUsernameTextBox = new System.Windows.Forms.TextBox();
            youtubePasswordTextBox = new System.Windows.Forms.TextBox();
            instagramUsernameTextBox = new System.Windows.Forms.TextBox();
            instagramPasswordTextBox = new System.Windows.Forms.TextBox();
            filePathTextBox = new System.Windows.Forms.TextBox();
            dateTimePicker = new System.Windows.Forms.DateTimePicker();
            platformComboBox = new System.Windows.Forms.ComboBox();
            scheduleButton = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // youtubeUsernameTextBox
            // 
            youtubeUsernameTextBox.Location = new System.Drawing.Point(12, 12);
            youtubeUsernameTextBox.Name = "youtubeUsernameTextBox";
            youtubeUsernameTextBox.Size = new System.Drawing.Size(200, 20);
            youtubeUsernameTextBox.TabIndex = 0;
            youtubeUsernameTextBox.PlaceholderText = "YouTube Username";
            // 
            // youtubePasswordTextBox
            // 
            youtubePasswordTextBox.Location = new System.Drawing.Point(12, 38);
            youtubePasswordTextBox.Name = "youtubePasswordTextBox";
            youtubePasswordTextBox.Size = new System.Drawing.Size(200, 20);
            youtubePasswordTextBox.TabIndex = 1;
            youtubePasswordTextBox.UseSystemPasswordChar = true;
            youtubePasswordTextBox.PlaceholderText = "YouTube Password";
            // 
            // instagramUsernameTextBox
            // 
            instagramUsernameTextBox.Location = new System.Drawing.Point(12, 64);
            instagramUsernameTextBox.Name = "instagramUsernameTextBox";
            instagramUsernameTextBox.Size = new System.Drawing.Size(200, 20);
            instagramUsernameTextBox.TabIndex = 2;
            instagramUsernameTextBox.PlaceholderText = "Instagram Username";
            // 
            // instagramPasswordTextBox
            // 
            instagramPasswordTextBox.Location = new System.Drawing.Point(12, 90);
            instagramPasswordTextBox.Name = "instagramPasswordTextBox";
            instagramPasswordTextBox.Size = new System.Drawing.Size(200, 20);
            instagramPasswordTextBox.TabIndex = 3;
            instagramPasswordTextBox.UseSystemPasswordChar = true;
            instagramPasswordTextBox.PlaceholderText = "Instagram Password";
            // 
            // filePathTextBox
            // 
            filePathTextBox.Location = new System.Drawing.Point(12, 116);
            filePathTextBox.Name = "filePathTextBox";
            filePathTextBox.Size = new System.Drawing.Size(200, 20);
            filePathTextBox.TabIndex = 4;
            filePathTextBox.PlaceholderText = "Video File Path";
            // 
            // dateTimePicker
            // 
            dateTimePicker.Location = new System.Drawing.Point(12, 142);
            dateTimePicker.Name = "dateTimePicker";
            dateTimePicker.Size = new System.Drawing.Size(200, 20);
            dateTimePicker.TabIndex = 5;
            // 
            // platformComboBox
            // 
            platformComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            platformComboBox.FormattingEnabled = true;
            platformComboBox.Items.AddRange(new object[] { "YouTube", "Instagram" });
            platformComboBox.Location = new System.Drawing.Point(12, 168);
            platformComboBox.Name = "platformComboBox";
            platformComboBox.Size = new System.Drawing.Size(200, 21);
            platformComboBox.TabIndex = 6;
            // 
            // scheduleButton
            // 
            scheduleButton.Location = new System.Drawing.Point(12, 195);
            scheduleButton.Name = "scheduleButton";
            scheduleButton.Size = new System.Drawing.Size(200, 23);
            scheduleButton.TabIndex = 7;
            scheduleButton.Text = "Schedule Upload";
            scheduleButton.UseVisualStyleBackColor = true;
            scheduleButton.Click += ScheduleButton_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(224, 230);
            Controls.Add(scheduleButton);
            Controls.Add(platformComboBox);
            Controls.Add(dateTimePicker);
            Controls.Add(filePathTextBox);
            Controls.Add(instagramPasswordTextBox);
            Controls.Add(instagramUsernameTextBox);
            Controls.Add(youtubePasswordTextBox);
            Controls.Add(youtubeUsernameTextBox);
            Name = "Form1";
            Text = "Video Uploader";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}