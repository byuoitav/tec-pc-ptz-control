using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

using System.IO;
using System.Reflection;
//using Vlc.DotNet.Forms;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ComponentModel;
using System.Windows.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.Security;
using System.Diagnostics;

namespace BYUPTZControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //VlcControl vlcControl = new VlcControl();
        DispatcherTimer previewTimer;
        DispatcherTimer zoomStopTimer;
        DispatcherTimer refreshTimer;
        Decoders.MJPEGStream mjpegstream;
        int SecondsToPause = 5;
        DateTime SleepTime;
        int PreviewStreamErrorCount = 0;
        bool IsClosing = false;
                        
        public MainWindow()
        {
            InitializeComponent();

            DateTime SleepTime = DateTime.Now.AddSeconds(SecondsToPause);

            pictureBoxLoading.Image = System.Drawing.Image.FromFile("images/ajax-loader.gif");

            //vlcControl.Play("url");

            InputManager.Current.PreProcessInput += Current_PreProcessInput;

            previewTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(SecondsToPause), IsEnabled = true };
            previewTimer.Tick += PreviewTimer_Tick;

            zoomStopTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500), IsEnabled = true };
            zoomStopTimer.Tick += ZoomStopTimer_Tick;

            refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(5), IsEnabled = true };
            refreshTimer.Tick += RefreshTimer_Tick;
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.RunWorkerAsync();
        }

        private void ZoomStopTimer_Tick(object sender, EventArgs e)
        {
            if (CameraListBox.SelectedIndex < 0) return;

            var SelectedCamera = CameraConfig.Cameras[CameraListBox.SelectedIndex];
            ExecuteRequest(SelectedCamera.ZoomStop);

            zoomStopTimer.Stop();
        }

        private void PreviewTimer_Tick(object sender, EventArgs e)
        {
            if (PreviewStreamErrorCount > 0) return;

            if (SleepTime < DateTime.Now)
            {
                if (PreviewUnavailableBorder.Visibility == Visibility.Collapsed)
                {
                    PreviewUnavailableLabel.Text = "Preview Paused";
                    PreviewUnavailableBorder.Visibility = Visibility.Visible;

                    if (mjpegstream != null) mjpegstream.Stop();
                }
            }
            else
            {
                if (PreviewUnavailableBorder.Visibility != Visibility.Collapsed)
                {
                    PreviewUnavailableLabel.Text = "Preview Paused";
                    PreviewUnavailableBorder.Visibility = Visibility.Collapsed;
                }

            }
        }

        private void Current_PreProcessInput(object sender, PreProcessInputEventArgs e)
        {
            SleepTime = DateTime.Now.AddSeconds(SecondsToPause);

            if (CameraListBox.SelectedIndex > -1 && mjpegstream != null && !mjpegstream.IsRunning && PreviewStreamErrorCount < 3)
            {
                StartMjpegStream();
            }
        }     

        public void ExecuteRequest(string url)
        {
            HttpClient client = new HttpClient();
            client.GetAsync(url);
            Debug.Print(url);
        }

        private void preset1Button_Click(object sender, RoutedEventArgs e)
        {
            if (CameraListBox.SelectedIndex < 0) return;

            var selectedPreset = ((Button)sender).DataContext as Preset;
            ExecuteRequest(selectedPreset.SetPreset);
        }

        private void upButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (CameraListBox.SelectedIndex < 0) return;

            var SelectedCamera = CameraConfig.Cameras[CameraListBox.SelectedIndex];
            ExecuteRequest(SelectedCamera.TiltUp);
        }

        private void stopMovementButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (CameraListBox.SelectedIndex < 0) return;

            var SelectedCamera = CameraConfig.Cameras[CameraListBox.SelectedIndex];
            ExecuteRequest(SelectedCamera.PanTiltStop);
            System.Threading.Thread.Sleep(250);
            ExecuteRequest(SelectedCamera.PanTiltStop);
        }

        private void downButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (CameraListBox.SelectedIndex < 0) return;

            var SelectedCamera = CameraConfig.Cameras[CameraListBox.SelectedIndex];
            ExecuteRequest(SelectedCamera.TiltDown);
        }

        private void leftButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (CameraListBox.SelectedIndex < 0) return;

            var SelectedCamera = CameraConfig.Cameras[CameraListBox.SelectedIndex];
            ExecuteRequest(SelectedCamera.PanLeft);
        }
        
        private void rightButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (CameraListBox.SelectedIndex < 0) return;

            var SelectedCamera = CameraConfig.Cameras[CameraListBox.SelectedIndex];
            ExecuteRequest(SelectedCamera.PanRight);
        }

        private void zoomInButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (CameraListBox.SelectedIndex < 0) return;

            var SelectedCamera = CameraConfig.Cameras[CameraListBox.SelectedIndex];
            ExecuteRequest(SelectedCamera.ZoomIn);
        }

        private void zoomOutButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (CameraListBox.SelectedIndex < 0) return;

            var SelectedCamera = CameraConfig.Cameras[CameraListBox.SelectedIndex];
            ExecuteRequest(SelectedCamera.ZoomOut);
        }

        private void zoomOutButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (CameraListBox.SelectedIndex < 0) return;
            zoomStopTimer.Stop();
            var SelectedCamera = CameraConfig.Cameras[CameraListBox.SelectedIndex];
            ExecuteRequest(SelectedCamera.ZoomStop);
            zoomStopTimer.Start();

        }

        CameraList CameraConfig { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadingBox.Visibility = Visibility.Visible;

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.RunWorkerAsync();
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result?.ToString() == "shutdown")
            {
                Application.Current.Shutdown();
                return;
            }

            if (mjpegstream != null)
            {
                mjpegstream.Stop();
            }
            
            LoadingBox.Visibility = Visibility.Collapsed;

            int selIndex = CameraListBox.SelectedIndex;
            if (selIndex < 0) selIndex = 0;
            this.DataContext = CameraConfig;
            CameraListBox.SelectedIndex = selIndex >= CameraConfig.Cameras.Count ? 0 : selIndex;            
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            //load vlc
            
            //load the config
            var configURL = Properties.Settings.Default.ConfigurationURL.Replace("{hostname}", Environment.MachineName.ToUpper());

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var task = client.GetAsync(configURL);
                if (!task.Result.IsSuccessStatusCode)
                {
                    MessageBox.Show("Unable to load configuration for host [" + Environment.MachineName + "]");
                    e.Result = "shutdown";                    
                    return;
                }

                string responseBody = task.Result.Content.ReadAsStringAsync().Result;
                CameraConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<CameraList>(responseBody);
            }
        }

        private void CameraListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //they picked a camera
            if (CameraListBox.SelectedIndex < 0) return;

            //mjpeg preview
            if (mjpegstream != null)
            {
                mjpegstream.Stop();
            }

            PreviewUnavailableLabel.Text = "Loading Preview...";
            PreviewImage.Source = null;
            PreviewUnavailableBorder.Visibility = Visibility.Visible;

            var SelectedCamera = CameraConfig.Cameras[CameraListBox.SelectedIndex];

            this.DataContext = CameraConfig;
            PresetListBox.ItemsSource = SelectedCamera.Presets;

            PreviewStreamErrorCount = 0;

            StartMjpegStream();
        }

        private void StartMjpegStream()
        {
            if (CameraListBox.SelectedIndex < 0 || IsClosing) return;

            var SelectedCamera = CameraConfig.Cameras[CameraListBox.SelectedIndex];

            //mjpeg preview
            if (mjpegstream != null)
            {
                mjpegstream.Stop();
            }

            mjpegstream = new Decoders.MJPEGStream(SelectedCamera.Stream);

            mjpegstream.VideoSourceError += Mjpegstream_VideoSourceError;

            var x = 0;

            mjpegstream.NewFrame += img => {
                Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Render,
                    new Action(() => {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.StreamSource = new MemoryStream(img);
                        bmp.EndInit();
                        bmp.Freeze();
                        PreviewImage.Source = bmp;

                        if (PreviewUnavailableBorder.Visibility == Visibility.Visible)
                        {
                            PreviewUnavailableBorder.Visibility = Visibility.Collapsed;
                        }
                    }));
            };

            if (PreviewUnavailableBorder.Visibility == Visibility.Visible)
            {
                PreviewUnavailableBorder.Visibility = Visibility.Collapsed;
            }

            mjpegstream.Start();

            SleepTime = DateTime.Now.AddSeconds(SecondsToPause);
        }

        private void Mjpegstream_VideoSourceError(string obj)
        {            
            Debug.Print("Error - " + obj);

            Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Render,
                    new Action(() =>
                    {
                        
                        PreviewUnavailableLabel.Text = "Preview Unavailable";
                        PreviewUnavailableBorder.Visibility = Visibility.Visible;
                        
                    }));

            PreviewStreamErrorCount++;

            mjpegstream.Stop();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            IsClosing = true;

            if (mjpegstream != null)
            {
                mjpegstream.Stop();
            }
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink hl = (Hyperlink)sender;
            string navigateUri = hl.NavigateUri.ToString();
            Process.Start(new ProcessStartInfo(navigateUri));
            e.Handled = true;         
        }
    }
}
