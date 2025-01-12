namespace VideoUploaderScheduler
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this.components = new System.ComponentModel.Container();
            
            // TabControl
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageAuth = new System.Windows.Forms.TabPage();
            this.tabPageUpload = new System.Windows.Forms.TabPage();

            // Authentication Page Components
            this.grpYouTube = new System.Windows.Forms.GroupBox();
            this.btnYouTubeAuth = new System.Windows.Forms.Button();
            this.lblYouTubeStatus = new System.Windows.Forms.Label();

            this.grpInstagram = new System.Windows.Forms.GroupBox();
            this.txtInstagramUsername = new System.Windows.Forms.TextBox();
            this.txtInstagramPassword = new System.Windows.Forms.TextBox();
            this.btnInstagramAuth = new System.Windows.Forms.Button();
            this.lblInstagramStatus = new System.Windows.Forms.Label();
            this.lblInstagramUsername = new System.Windows.Forms.Label();
            this.lblInstagramPassword = new System.Windows.Forms.Label();

            // Upload Page Components
            this.grpUploadDetails = new System.Windows.Forms.GroupBox();
            this.txtFilePath = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtTitle = new System.Windows.Forms.TextBox();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.txtTags = new System.Windows.Forms.TextBox();
            this.dateTimePickerUpload = new System.Windows.Forms.DateTimePicker();
            this.cmbPlatform = new System.Windows.Forms.ComboBox();
            this.btnScheduleUpload = new System.Windows.Forms.Button();
            this.lblFilePath = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            this.lblTags = new System.Windows.Forms.Label();
            this.lblScheduleTime = new System.Windows.Forms.Label();
            this.lblPlatform = new System.Windows.Forms.Label();

            this.lstScheduledUploads = new System.Windows.Forms.ListView();
            this.refreshTimer = new System.Windows.Forms.Timer(this.components);

            // Form Properties
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 600);
            this.MinimumSize = new System.Drawing.Size(800, 500);
            this.Text = "Video Yükleme Planlayıcı";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            // TabControl Setup
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Controls.Add(this.tabPageAuth);
            this.tabControl.Controls.Add(this.tabPageUpload);
            this.Controls.Add(this.tabControl);

            // Authentication Tab
            this.tabPageAuth.Text = "Kimlik Doğrulama";
            this.tabPageAuth.Padding = new System.Windows.Forms.Padding(10);

            // YouTube Group
            this.grpYouTube.Text = "YouTube Kimlik Doğrulama";
            this.grpYouTube.Size = new System.Drawing.Size(400, 100);
            this.grpYouTube.Location = new System.Drawing.Point(20, 20);
            this.grpYouTube.Controls.Add(this.btnYouTubeAuth);
            this.grpYouTube.Controls.Add(this.lblYouTubeStatus);

            this.btnYouTubeAuth.Text = "YouTube'a Bağlan";
            this.btnYouTubeAuth.Size = new System.Drawing.Size(150, 30);
            this.btnYouTubeAuth.Location = new System.Drawing.Point(20, 30);

            this.lblYouTubeStatus.Text = "Bağlı değil";
            this.lblYouTubeStatus.AutoSize = true;
            this.lblYouTubeStatus.Location = new System.Drawing.Point(180, 35);
            this.lblYouTubeStatus.ForeColor = System.Drawing.Color.Red;

            // Instagram Group
            this.grpInstagram.Text = "Instagram Kimlik Doğrulama";
            this.grpInstagram.Size = new System.Drawing.Size(400, 150);
            this.grpInstagram.Location = new System.Drawing.Point(20, 140);

            this.lblInstagramUsername.Text = "Kullanıcı Adı:";
            this.lblInstagramUsername.Location = new System.Drawing.Point(20, 30);
            this.lblInstagramUsername.AutoSize = true;

            this.txtInstagramUsername.Location = new System.Drawing.Point(120, 27);
            this.txtInstagramUsername.Size = new System.Drawing.Size(200, 22);

            this.lblInstagramPassword.Text = "Şifre:";
            this.lblInstagramPassword.Location = new System.Drawing.Point(20, 60);
            this.lblInstagramPassword.AutoSize = true;

            this.txtInstagramPassword.Location = new System.Drawing.Point(120, 57);
            this.txtInstagramPassword.Size = new System.Drawing.Size(200, 22);
            this.txtInstagramPassword.UseSystemPasswordChar = true;

            this.btnInstagramAuth.Text = "Instagram'a Bağlan";
            this.btnInstagramAuth.Size = new System.Drawing.Size(150, 30);
            this.btnInstagramAuth.Location = new System.Drawing.Point(120, 90);

            this.lblInstagramStatus.Text = "Bağlı değil";
            this.lblInstagramStatus.AutoSize = true;
            this.lblInstagramStatus.Location = new System.Drawing.Point(280, 95);
            this.lblInstagramStatus.ForeColor = System.Drawing.Color.Red;

            // Upload Tab
            this.tabPageUpload.Text = "Yükleme Planla";
            this.tabPageUpload.Padding = new System.Windows.Forms.Padding(10);

            // Upload Details Group
            this.grpUploadDetails.Text = "Yükleme Detayları";
            this.grpUploadDetails.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpUploadDetails.Height = 250;
            this.grpUploadDetails.Padding = new System.Windows.Forms.Padding(10);

            // File Selection Controls
            this.lblFilePath.Text = "Dosya:";
            this.lblFilePath.Location = new System.Drawing.Point(20, 30);
            this.lblFilePath.AutoSize = true;

            this.txtFilePath.Location = new System.Drawing.Point(120, 27);
            this.txtFilePath.Size = new System.Drawing.Size(500, 22);
            this.txtFilePath.ReadOnly = true;

            this.btnBrowse.Text = "Gözat...";
            this.btnBrowse.Location = new System.Drawing.Point(630, 26);
            this.btnBrowse.Size = new System.Drawing.Size(80, 25);

            // Title and Description
            this.lblTitle.Text = "Başlık:";
            this.lblTitle.Location = new System.Drawing.Point(20, 60);
            this.lblTitle.AutoSize = true;

            this.txtTitle.Location = new System.Drawing.Point(120, 57);
            this.txtTitle.Size = new System.Drawing.Size(500, 22);

            this.lblDescription.Text = "Açıklama:";
            this.lblDescription.Location = new System.Drawing.Point(20, 90);
            this.lblDescription.AutoSize = true;

            this.txtDescription.Location = new System.Drawing.Point(120, 87);
            this.txtDescription.Size = new System.Drawing.Size(500, 60);
            this.txtDescription.Multiline = true;

            // Tags
            this.lblTags.Text = "Etiketler:";
            this.lblTags.Location = new System.Drawing.Point(20, 160);
            this.lblTags.AutoSize = true;

            this.txtTags.Location = new System.Drawing.Point(120, 157);
            this.txtTags.Size = new System.Drawing.Size(500, 22);

            // Platform and Schedule
            this.lblPlatform.Text = "Platform:";
            this.lblPlatform.Location = new System.Drawing.Point(20, 190);
            this.lblPlatform.AutoSize = true;

            this.cmbPlatform.Location = new System.Drawing.Point(120, 187);
            this.cmbPlatform.Size = new System.Drawing.Size(200, 24);
            this.cmbPlatform.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;

            this.lblScheduleTime.Text = "Yükleme Zamanı:";
            this.lblScheduleTime.Location = new System.Drawing.Point(350, 190);
            this.lblScheduleTime.AutoSize = true;

            this.dateTimePickerUpload.Location = new System.Drawing.Point(470, 187);
            this.dateTimePickerUpload.Size = new System.Drawing.Size(150, 22);
            this.dateTimePickerUpload.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerUpload.CustomFormat = "dd.MM.yyyy HH:mm";

            // Schedule Button
            this.btnScheduleUpload.Text = "Yüklemeyi Planla";
            this.btnScheduleUpload.Location = new System.Drawing.Point(120, 220);
            this.btnScheduleUpload.Size = new System.Drawing.Size(150, 30);

            // Scheduled Uploads List
            this.lstScheduledUploads.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstScheduledUploads.View = System.Windows.Forms.View.Details;
            this.lstScheduledUploads.FullRowSelect = true;
            this.lstScheduledUploads.GridLines = true;

            // Timer
            this.refreshTimer.Interval = 30000; // 30 seconds

            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageAuth;
        private System.Windows.Forms.TabPage tabPageUpload;
        private System.Windows.Forms.GroupBox grpYouTube;
        private System.Windows.Forms.Button btnYouTubeAuth;
        private System.Windows.Forms.Label lblYouTubeStatus;
        private System.Windows.Forms.GroupBox grpInstagram;
        private System.Windows.Forms.TextBox txtInstagramUsername;
        private System.Windows.Forms.TextBox txtInstagramPassword;
        private System.Windows.Forms.Button btnInstagramAuth;
        private System.Windows.Forms.Label lblInstagramStatus;
        private System.Windows.Forms.Label lblInstagramUsername;
        private System.Windows.Forms.Label lblInstagramPassword;
        private System.Windows.Forms.GroupBox grpUploadDetails;
        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox txtTitle;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.TextBox txtTags;
        private System.Windows.Forms.DateTimePicker dateTimePickerUpload;
        private System.Windows.Forms.ComboBox cmbPlatform;
        private System.Windows.Forms.Button btnScheduleUpload;
        private System.Windows.Forms.Label lblFilePath;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label lblTags;
        private System.Windows.Forms.Label lblScheduleTime;
        private System.Windows.Forms.Label lblPlatform;
        private System.Windows.Forms.ListView lstScheduledUploads;
        private System.Windows.Forms.Timer refreshTimer;
    }
}