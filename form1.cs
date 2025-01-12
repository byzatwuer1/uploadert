using System;
using System.Windows.Forms;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace VideoUploaderScheduler
{
    public partial class Form1 : Form
    {
        private readonly UploadScheduler _scheduler;
        private readonly List<ScheduledUpload> _scheduledUploads;

        public Form1()
        {
            InitializeComponent();
            _scheduler = new UploadScheduler();
            _scheduledUploads = new List<ScheduledUpload>();
            
            // Form yüklendiğinde çalışacak event
            this.Load += Form1_Load;
            
            // Buton click eventlerini bağla
            btnYouTubeAuth.Click += BtnYouTubeAuth_Click;
            btnInstagramAuth.Click += BtnInstagramAuth_Click;
            btnBrowse.Click += BtnBrowse_Click;
            btnScheduleUpload.Click += BtnScheduleUpload_Click;

            // Platform seçimi değiştiğinde
            cmbPlatform.SelectedIndexChanged += CmbPlatform_SelectedIndexChanged;

            // Minimum tarih ayarı
            dateTimePickerUpload.MinDate = DateTime.Now;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                await _scheduler.StartAsync();
                await CheckAuthenticationStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Başlatma hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task CheckAuthenticationStatus()
        {
            var credentials = await CredentialStore.LoadCredentials();

            // YouTube durumunu kontrol et
            if (!string.IsNullOrEmpty(credentials.YouTubeRefreshToken))
            {
                lblYouTubeStatus.Text = "✓ YouTube Bağlantısı Aktif";
                lblYouTubeStatus.ForeColor = System.Drawing.Color.Green;
            }
            else
            {
                lblYouTubeStatus.Text = "✗ YouTube Bağlantısı Yok";
                lblYouTubeStatus.ForeColor = System.Drawing.Color.Red;
            }

            // Instagram durumunu kontrol et
            if (!string.IsNullOrEmpty(credentials.InstagramSessionFile))
            {
                lblInstagramStatus.Text = "✓ Instagram Bağlantısı Aktif";
                lblInstagramStatus.ForeColor = System.Drawing.Color.Green;
            }
            else
            {
                lblInstagramStatus.Text = "✗ Instagram Bağlantısı Yok";
                lblInstagramStatus.ForeColor = System.Drawing.Color.Red;
            }
        }

        private async void BtnYouTubeAuth_Click(object sender, EventArgs e)
        {
            try
            {
                btnYouTubeAuth.Enabled = false;
                btnYouTubeAuth.Text = "Bağlanıyor...";

                await YouTubeUploader.Instance.AuthenticateAsync();
                
                await CheckAuthenticationStatus();
                MessageBox.Show("YouTube kimlik doğrulama başarılı!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"YouTube kimlik doğrulama hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnYouTubeAuth.Enabled = true;
                btnYouTubeAuth.Text = "YouTube'a Bağlan";
            }
        }

        private async void BtnInstagramAuth_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtInstagramUsername.Text) || 
                string.IsNullOrWhiteSpace(txtInstagramPassword.Text))
            {
                MessageBox.Show("Lütfen Instagram kullanıcı adı ve şifrenizi girin.", "Uyarı", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnInstagramAuth.Enabled = false;
                btnInstagramAuth.Text = "Bağlanıyor...";

                await InstagramUploader.Instance.AuthenticateAsync(
                    txtInstagramUsername.Text.Trim(),
                    txtInstagramPassword.Text.Trim()
                );

                await CheckAuthenticationStatus();
                MessageBox.Show("Instagram kimlik doğrulama başarılı!", "Başarılı", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Başarılı girişten sonra şifreyi temizle
                txtInstagramPassword.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Instagram kimlik doğrulama hatası: {ex.Message}", "Hata", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnInstagramAuth.Enabled = true;
                btnInstagramAuth.Text = "Instagram'a Bağlan";
            }
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Video Dosyaları|*.mp4;*.avi;*.mov|Resim Dosyaları|*.jpg;*.jpeg;*.png|Tüm Dosyalar|*.*";
                openFileDialog.FilterIndex = 1;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtFilePath.Text = openFileDialog.FileName;
                }
            }
        }

        private void CmbPlatform_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Instagram seçiliyse tag alanını gizle
            txtTags.Enabled = cmbPlatform.SelectedItem.ToString() == "YouTube";
        }

        private async void BtnScheduleUpload_Click(object sender, EventArgs e)
        {
            if (!ValidateUploadForm())
                return;

            try
            {
                btnScheduleUpload.Enabled = false;
                string platform = cmbPlatform.SelectedItem.ToString();
                string[] tags = platform == "YouTube" ? txtTags.Text.Split(',') : null;

                await _scheduler.ScheduleUpload(
                    dateTimePickerUpload.Value,
                    platform,
                    txtFilePath.Text.Trim(),
                    txtTitle.Text.Trim(),
                    txtDescription.Text.Trim(),
                    tags
                );

                // ListView'e ekle
                AddScheduledUploadToList(platform, txtFilePath.Text, txtTitle.Text, dateTimePickerUpload.Value);

                MessageBox.Show("Yükleme başarıyla planlandı!", "Başarılı", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                ClearUploadForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Yükleme planlanırken hata oluştu: {ex.Message}", "Hata", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnScheduleUpload.Enabled = true;
            }
        }

        private bool ValidateUploadForm()
        {
            if (string.IsNullOrWhiteSpace(txtFilePath.Text))
            {
                MessageBox.Show("Lütfen bir dosya seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!File.Exists(txtFilePath.Text))
            {
                MessageBox.Show("Seçilen dosya bulunamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Lütfen bir başlık girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (cmbPlatform.SelectedItem == null)
            {
                MessageBox.Show("Lütfen bir platform seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void AddScheduledUploadToList(string platform, string filePath, string title, DateTime scheduledTime)
        {
            var item = new ListViewItem(new[] {
                platform,
                Path.GetFileName(filePath),
                title,
                scheduledTime.ToString("dd.MM.yyyy HH:mm")
            });

            lstScheduledUploads.Items.Add(item);
        }

        private void ClearUploadForm()
        {
            txtFilePath.Clear();
            txtTitle.Clear();
            txtDescription.Clear();
            txtTags.Clear();
            dateTimePickerUpload.Value = DateTime.Now;
            cmbPlatform.SelectedIndex = -1;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            _scheduler.StopAsync().Wait();
        }
    }

    public class ScheduledUpload
    {
        public string Platform { get; set; }
        public string FilePath { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string[] Tags { get; set; }
        public DateTime ScheduledTime { get; set; }
    }
}