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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageAuth = new System.Windows.Forms.TabPage();
            this.tabPageUpload = new System.Windows.Forms.TabPage();
            
            // Authentication Tab Components
            this.grpYouTube = new System.Windows.Forms.GroupBox();
            this.btnYouTubeAuth = new System.Windows.Forms.Button();
            this.lblYouTubeStatus = new System.Windows.Forms.Label();
            
            this.grpInstagram = new System.Windows.Forms.GroupBox();
            this.txtInstagramUsername = new System.Windows.Forms.TextBox();
            this.txtInstagramPassword = new System.Windows.Forms.TextBox();
            this.btnInstagramAuth = new System.Windows.Forms.Button();
            this.lblInstagramStatus = new System.Windows.Forms.Label();
            
            // Upload Tab Components
            this.grpUploadDetails = new System.Windows.Forms.GroupBox();
            this.txtFilePath = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtTitle = new System.Windows.Forms.TextBox();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.txtTags = new System.Windows.Forms.TextBox();
            this.dateTimePickerUpload = new System.Windows.Forms.DateTimePicker();
            this.cmbPlatform = new System.Windows.Forms.ComboBox();
            this.btnScheduleUpload = new System.Windows.Forms.Button();
            
            this.lstScheduledUploads = new System.Windows.Forms.ListView();
            
            // Initialize TabControl
            this.tabControl1.SuspendLayout();
            
            // Tab Control
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.Size = new System.Drawing.Size(760, 537);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.Controls.Add(this.tabPageAuth);
            this.tabControl1.Controls.Add(this.tabPageUpload);

            // Authentication Tab
            this.tabPageAuth.Location = new System.Drawing.Point(4, 22);
            this.tabPageAuth.Name = "tabPageAuth";
            this.tabPageAuth.Size = new System.Drawing.Size(752, 511);
            this.tabPageAuth.TabIndex = 0;
            this.tabPageAuth.Text = "Kimlik Doğrulama";
            this.tabPageAuth.UseVisualStyleBackColor = true;
            
            // YouTube Group
            this.grpYouTube.Location = new System.Drawing.Point(20, 20);
            this.grpYouTube.Name = "grpYouTube";
            this.grpYouTube.Size = new System.Drawing.Size(700, 100);
            this.grpYouTube.Text = "YouTube Kimlik Doğrulama";
            
            this.btnYouTubeAuth.Location = new System.Drawing.Point(20, 30);
            this.btnYouTubeAuth.Size = new System.Drawing.Size(150, 30);
            this.btnYouTubeAuth.Text = "YouTube'a Bağlan";
            
            this.lblYouTubeStatus.Location = new System.Drawing.Point(180, 35);
            this.lblYouTubeStatus.AutoSize = true;
            
            // Instagram Group
            this.grpInstagram.Location = new System.Drawing.Point(20, 140);
            this.grpInstagram.Size = new System.Drawing.Size(700, 150);
            this.grpInstagram.Text = "Instagram Kimlik Doğrulama";
            
            this.txtInstagramUsername.Location = new System.Drawing.Point(20, 30);
            this.txtInstagramUsername.Size = new System.Drawing.Size(200, 20);
            this.txtInstagramUsername.PlaceholderText = "Instagram Kullanıcı Adı";
            
            this.txtInstagramPassword.Location = new System.Drawing.Point(20, 60);
            this.txtInstagramPassword.Size = new System.Drawing.Size(200, 20);
            this.txtInstagramPassword.PasswordChar = '*';
            this.txtInstagramPassword.PlaceholderText = "Instagram Şifresi";
            
            this.btnInstagramAuth.Location = new System.Drawing.Point(20, 90);
            this.btnInstagramAuth.Size = new System.Drawing.Size(150, 30);
            this.btnInstagramAuth.Text = "Instagram'a Bağlan";
            
            this.lblInstagramStatus.Location = new System.Drawing.Point(180, 95);
            this.lblInstagramStatus.AutoSize = true;

            // Upload Tab
            this.tabPageUpload.Location = new System.Drawing.Point(4, 22);
            this.tabPageUpload.Name = "tabPageUpload";
            this.tabPageUpload.Size = new System.Drawing.Size(752, 511);
            this.tabPageUpload.TabIndex = 1;
            this.tabPageUpload.Text = "Yükleme Planla";
            this.tabPageUpload.UseVisualStyleBackColor = true;
            
            // Upload Details Group
            this.grpUploadDetails.Location = new System.Drawing.Point(20, 20);
            this.grpUploadDetails.Size = new System.Drawing.Size(700, 250);
            this.grpUploadDetails.Text = "Yükleme Detayları";
            
            this.txtFilePath.Location = new System.Drawing.Point(20, 30);
            this.txtFilePath.Size = new System.Drawing.Size(500, 20);
            this.txtFilePath.PlaceholderText = "Dosya Yolu";
            
            this.btnBrowse.Location = new System.Drawing.Point(530, 29);
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.Text = "Gözat...";
            
            this.txtTitle.Location = new System.Drawing.Point(20, 60);
            this.txtTitle.Size = new System.Drawing.Size(500, 20);
            this.txtTitle.PlaceholderText = "Başlık";
            
            this.txtDescription.Location = new System.Drawing.Point(20, 90);
            this.txtDescription.Size = new System.Drawing.Size(500, 60);
            this.txtDescription.Multiline = true;
            this.txtDescription.PlaceholderText = "Açıklama";
            
            this.txtTags.Location = new System.Drawing.Point(20, 160);
            this.txtTags.Size = new System.Drawing.Size(500, 20);
            this.txtTags.PlaceholderText = "Etiketler (virgülle ayırın)";
            
            this.dateTimePickerUpload.Location = new System.Drawing.Point(20, 190);
            this.dateTimePickerUpload.Size = new System.Drawing.Size(200, 20);
            
            this.cmbPlatform.Location = new System.Drawing.Point(230, 190);
            this.cmbPlatform.Size = new System.Drawing.Size(150, 20);
            this.cmbPlatform.Items.AddRange(new object[] { "YouTube", "Instagram" });
            this.cmbPlatform.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            
            this.btnScheduleUpload.Location = new System.Drawing.Point(20, 220);
            this.btnScheduleUpload.Size = new System.Drawing.Size(150, 30);
            this.btnScheduleUpload.Text = "Yüklemeyi Planla";
            
            // Scheduled Uploads List
            this.lstScheduledUploads.Location = new System.Drawing.Point(20, 290);
            this.lstScheduledUploads.Size = new System.Drawing.Size(700, 200);
            this.lstScheduledUploads.View = System.Windows.Forms.View.Details;
            this.lstScheduledUploads.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                new System.Windows.Forms.ColumnHeader() { Text = "Platform", Width = 100 },
                new System.Windows.Forms.ColumnHeader() { Text = "Dosya", Width = 200 },
                new System.Windows.Forms.ColumnHeader() { Text = "Başlık", Width = 200 },
                new System.Windows.Forms.ColumnHeader() { Text = "Planlanan Zaman", Width = 150 }
            });

            // Form Properties
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.tabControl1);
            this.Name = "Form1";
            this.Text = "Video Yükleme Planlayıcı";
            this.tabControl1.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
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
        private System.Windows.Forms.GroupBox grpUploadDetails;
        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox txtTitle;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.TextBox txtTags;
        private System.Windows.Forms.DateTimePicker dateTimePickerUpload;
        private System.Windows.Forms.ComboBox cmbPlatform;
        private System.Windows.Forms.Button btnScheduleUpload;
        private System.Windows.Forms.ListView lstScheduledUploads;
    }
}