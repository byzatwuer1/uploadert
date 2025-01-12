using System;
using System.Windows.Forms;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace VideoUploaderScheduler
{
    public partial class Form1 : Form
    {
        private readonly ILogger<Form1> _logger;
        private readonly UploadScheduler _scheduler;
        private readonly CredentialStore _credentialStore;
        private readonly YouTubeUploader _youtubeUploader;
        private readonly InstagramUploader _instagramUploader;
        private bool _isAuthenticating;

        public Form1(
            ILogger<Form1> logger,
            UploadScheduler scheduler,
            CredentialStore credentialStore,
            YouTubeUploader youtubeUploader,
            InstagramUploader instagramUploader)
        {
            InitializeComponent();

            _logger = logger;
            _scheduler = scheduler;
            _credentialStore = credentialStore;
            _youtubeUploader = youtubeUploader;
            _instagramUploader = instagramUploader;

            InitializeFormComponents();
            LoadSavedCredentials();
        }

        private void InitializeFormComponents()
        {
            // Platform seçim combobox'ını doldur
            cmbPlatform.Items.AddRange(new object[] { "YouTube", "Instagram" });
            cmbPlatform.SelectedIndex = 0;

            // Event handlers
            btnBrowse.Click += BtnBrowse_Click;
            btnScheduleUpload.Click += BtnScheduleUpload_Click;
            btnYouTubeAuth.Click += BtnYouTubeAuth_Click;
            btnInstagramAuth.Click += BtnInstagramAuth_Click;
            cmbPlatform.SelectedIndexChanged += CmbPlatform_SelectedIndexChanged;

            // ListView kolonları
            lstScheduledUploads.View = View.Details;
            lstScheduledUploads.Columns.AddRange(new ColumnHeader[]
            {
                new ColumnHeader { Text = "Platform", Width = 100 },
                new ColumnHeader { Text = "Dosya", Width = 200 },
                new ColumnHeader { Text = "Başlık", Width = 200 },
                new ColumnHeader { Text = "Planlanan Zaman", Width = 150 },
                new ColumnHeader { Text = "Durum", Width = 100 }
            });

            // Tarih seçici minimum değeri
            dateTimePickerUpload.MinDate = DateTime.Now;

            // Form yüklenme eventi
            this.Load += Form1_Load;

            // Timer for refreshing uploads list
            var refreshTimer = new Timer
            {
                Interval = 30000 // 30 seconds
            };
            refreshTimer.Tick += RefreshTimer_Tick;
            refreshTimer.Start();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                await RefreshScheduledUploadsList();
                UpdateAuthenticationStatus();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Form yüklenirken hata oluştu");
                MessageBox.Show(
                    "Form yüklenirken bir hata oluştu: " + ex.Message,
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Video Dosyaları|*.mp4;*.avi;*.mov;*.wmv|Tüm Dosyalar|*.*";
                openFileDialog.FilterIndex = 1;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtFilePath.Text = openFileDialog.FileName;
                    txtTitle.Text = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                }
            }
        }

        private async void BtnScheduleUpload_Click(object sender, EventArgs e)
        {
            if (ValidateUploadInputs())
            {
                try
                {
                    btnScheduleUpload.Enabled = false;

                    var uploadInfo = new UploadInfo
                    {
                        FilePath = txtFilePath.Text,
                        Title = txtTitle.Text,
                        Description = txtDescription.Text,
                        Platform = cmbPlatform.SelectedItem.ToString(),
                        ScheduledTime = dateTimePickerUpload.Value
                    };

                    await _scheduler.ScheduleJobAsync(uploadInfo);

                    MessageBox.Show(
                        "Yükleme başarıyla planlandı!",
                        "Başarılı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    ClearUploadForm();
                    await RefreshScheduledUploadsList();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Yükleme planlanırken hata oluştu");
                    MessageBox.Show(
                        "Yükleme planlanırken bir hata oluştu: " + ex.Message,
                        "Hata",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                finally
                {
                    btnScheduleUpload.Enabled = true;
                }
            }
        }

        private async void BtnYouTubeAuth_Click(object sender, EventArgs e)
        {
            if (_isAuthenticating) return;

            try
            {
                _isAuthenticating = true;
                btnYouTubeAuth.Enabled = false;
                lblYouTubeStatus.Text = "Kimlik doğrulanıyor...";

                await _youtubeUploader.AuthenticateAsync();

                lblYouTubeStatus.Text = "Bağlandı";
                lblYouTubeStatus.ForeColor = Color.Green;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "YouTube kimlik doğrulama hatası");
                lblYouTubeStatus.Text = "Bağlantı hatası";
                lblYouTubeStatus.ForeColor = Color.Red;
                MessageBox.Show(
                    "YouTube kimlik doğrulama hatası: " + ex.Message,
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                _isAuthenticating = false;
                btnYouTubeAuth.Enabled = true;
            }
        }

        private async void BtnInstagramAuth_Click(object sender, EventArgs e)
        {
            if (_isAuthenticating) return;

            try
            {
                _isAuthenticating = true;
                btnInstagramAuth.Enabled = false;
                lblInstagramStatus.Text = "Kimlik doğrulanıyor...";

                await _instagramUploader.AuthenticateAsync(
                    txtInstagramUsername.Text,
                    txtInstagramPassword.Text);

                lblInstagramStatus.Text = "Bağlandı";
                lblInstagramStatus.ForeColor = Color.Green;

                // Save credentials
                await SaveInstagramCredentials();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Instagram kimlik doğrulama hatası");
                lblInstagramStatus.Text = "Bağlantı hatası";
                lblInstagramStatus.ForeColor = Color.Red;
                MessageBox.Show(
                    "Instagram kimlik doğrulama hatası: " + ex.Message,
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                _isAuthenticating = false;
                btnInstagramAuth.Enabled = true;
            }
        }

        private async Task SaveInstagramCredentials()
        {
            var credentials = await _credentialStore.LoadCredentials();
            credentials.InstagramUsername = txtInstagramUsername.Text;
            credentials.InstagramPassword = txtInstagramPassword.Text;
            await _credentialStore.SaveCredentials(credentials);
        }

        private async void RefreshTimer_Tick(object sender, EventArgs e)
        {
            await RefreshScheduledUploadsList();
        }

        private async Task RefreshScheduledUploadsList()
        {
            try
            {
                var uploads = await _scheduler.GetScheduledUploads();
                UpdateUploadsList(uploads);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yükleme listesi yenilenirken hata oluştu");
            }
        }

        private void UpdateUploadsList(ScheduledUploadInfo[] uploads)
        {
            lstScheduledUploads.Items.Clear();

            foreach (var upload in uploads)
            {
                var item = new ListViewItem(new[]
                {
                    upload.Platform,
                    Path.GetFileName(upload.FilePath),
                    upload.Title,
                    upload.ScheduledTime.ToString("g"),
                    GetUploadStatus(upload)
                });

                lstScheduledUploads.Items.Add(item);
            }
        }

        private string GetUploadStatus(ScheduledUploadInfo upload)
        {
            if (upload.ScheduledTime > DateTime.Now)
                return "Bekliyor";
            else
                return "İşleniyor";
        }

        private bool ValidateUploadInputs()
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

            if (dateTimePickerUpload.Value <= DateTime.Now)
            {
                MessageBox.Show("Lütfen gelecekte bir tarih seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void ClearUploadForm()
        {
            txtFilePath.Clear();
            txtTitle.Clear();
            txtDescription.Clear();
            dateTimePickerUpload.Value = DateTime.Now.AddMinutes(5);
        }

        private async void LoadSavedCredentials()
        {
            try
            {
                var credentials = await _credentialStore.LoadCredentials();
                txtInstagramUsername.Text = credentials.InstagramUsername;
                txtInstagramPassword.Text = credentials.InstagramPassword;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kayıtlı kimlik bilgileri yüklenirken hata oluştu");
            }
        }

        private void UpdateAuthenticationStatus()
        {
            lblYouTubeStatus.Text = _youtubeUploader.IsAuthenticated ? "Bağlandı" : "Bağlı değil";
            lblYouTubeStatus.ForeColor = _youtubeUploader.IsAuthenticated ? Color.Green : Color.Red;

            lblInstagramStatus.Text = _instagramUploader.IsAuthenticated ? "Bağlandı" : "Bağlı değil";
            lblInstagramStatus.ForeColor = _instagramUploader.IsAuthenticated ? Color.Green : Color.Red;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            
            if (e.CloseReason == CloseReason.UserClosing)
            {
                var result = MessageBox.Show(
                    "Uygulamayı kapatmak istiyor musunuz? Planlanmış yüklemeler iptal edilecek.",
                    "Onay",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}