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

        Decoders.MJPEGStream mjpegstream;

        DateTime SleepTime = DateTime.Now.AddMinutes(5);
        string PreviewToken = "";
        int PreviewIndex = -1;
        WriteableBitmap writeableBitmap;
        string x = "ThWmZq4t7w!z%C*F-JaNdRfUjXn2r5u8";
        string y = "+KbPeShVmYq3t6w9y$B&E)H@McQfTjWn";
        
        public MainWindow()
        {
            InitializeComponent();

            pictureBoxLoading.Image = System.Drawing.Image.FromFile("images/ajax-loader.gif");

            //vlcControl.Play("url");

            InputManager.Current.PreProcessInput += Current_PreProcessInput;

            previewTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150), IsEnabled = true };
            previewTimer.Tick += PreviewTimer_Tick;

            zoomStopTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500), IsEnabled = true };
            zoomStopTimer.Tick += ZoomStopTimer_Tick;
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
            if (SleepTime > DateTime.Now)
            {
                try
                {
                    if (CameraConfig == null) return;
                    if (CameraListBox.SelectedIndex == -1) return;

                    var SelectedCamera = CameraConfig.Cameras[CameraListBox.SelectedIndex];

                    if (SelectedCamera.Stream == "")
                    {
                        previewTimer.Stop();
                        return;
                    }

                    if (SelectedCamera.Stream.EndsWith(".mjpg"))
                    {
                        previewTimer.Stop();

                        //mjpeg
                        if (mjpegstream != null)
                        {
                            mjpegstream.Stop();                            
                        }

                        mjpegstream = new Decoders.MJPEGStream(SelectedCamera.Stream);

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
                                }));
                        };

                        if (PreviewUnavailableBorder.Visibility == Visibility.Visible)
                        {
                            PreviewUnavailableBorder.Visibility = Visibility.Collapsed;
                        }

                        mjpegstream.Start();

                        return;
                    }

                    if (mjpegstream != null)
                    {
                        mjpegstream.Stop();
                    }

                    if (PreviewToken == "" || PreviewIndex != CameraListBox.SelectedIndex)
                    {
                        using (var client = new HttpClient())
                        {
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            byte[] key1b = System.Text.Encoding.UTF8.GetBytes(x);
                            byte[] key2b = System.Text.Encoding.UTF8.GetBytes(y);
                            string payload = Encryption.AESThenHMAC.SimpleDecrypt(Properties.Settings.Default.CamPreviewTokenPayloadEncrpyted, key1b, key2b);

                            var task = client.PostAsync(Properties.Settings.Default.CamPreviewTokenURL.Replace("{hostname}", SelectedCamera.Stream), new StringContent(payload, Encoding.UTF8, "application/json"));

                            if (!task.Result.IsSuccessStatusCode)
                            {
                                previewTimer.Stop();
                                PreviewUnavailableBorder.Visibility = Visibility.Visible;
                                PreviewUnavailableLabel.Text = "Preview Unavailable";
                                return;
                            }
                            else if (PreviewUnavailableBorder.Visibility == Visibility.Visible)
                            {
                                PreviewUnavailableBorder.Visibility = Visibility.Collapsed;
                            }

                            string responseBody = task.Result.Content.ReadAsStringAsync().Result;
                            var tokenresponse = Newtonsoft.Json.JsonConvert.DeserializeObject<Aver520LoginResult>(responseBody);
                            PreviewToken = tokenresponse.data.token;

                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", PreviewToken);
                            task = client.PostAsync(Properties.Settings.Default.CamPreviewSubscribeURL.Replace("{hostname}", SelectedCamera.Stream), new StringContent("", Encoding.UTF8, "application/json"));

                            if (!task.Result.IsSuccessStatusCode)
                            {
                                previewTimer.Stop();
                                PreviewUnavailableBorder.Visibility = Visibility.Visible;
                                PreviewUnavailableLabel.Text = "Preview Unavailable";
                                return;
                            }
                            else if (PreviewUnavailableBorder.Visibility == Visibility.Visible)
                            {
                                PreviewUnavailableBorder.Visibility = Visibility.Collapsed;
                            }
                        }

                        PreviewIndex = CameraListBox.SelectedIndex;
                    }

                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", PreviewToken);
                        var task = client.GetAsync(Properties.Settings.Default.CamPreviewURL.Replace("{hostname}", SelectedCamera.Stream));

                        if (!task.Result.IsSuccessStatusCode)
                        {
                            previewTimer.Stop();
                            PreviewToken = "";
                            PreviewUnavailableBorder.Visibility = Visibility.Visible;
                            PreviewUnavailableLabel.Text = "Preview Unavailable";
                            return;
                        }
                        else if (PreviewUnavailableBorder.Visibility == Visibility.Visible)
                        {
                            PreviewUnavailableBorder.Visibility = Visibility.Collapsed;
                        }

                        var imageByteArray = task.Result.Content.ReadAsByteArrayAsync().Result;
                        using (var ms = new MemoryStream(imageByteArray))
                        {
                            var formsBitmap = new Bitmap(ms);
                            var width = formsBitmap.Width;
                            var height = formsBitmap.Height;

                            if (writeableBitmap == null || height != writeableBitmap.PixelHeight || width != writeableBitmap.PixelWidth)
                            {
                                writeableBitmap = new WriteableBitmap(width, height, 72, 72, PixelFormats.Pbgra32, null);
                                PreviewImage.Source = writeableBitmap;
                            }


                            BitmapData data = formsBitmap.LockBits(new Rectangle(0, 0, formsBitmap.Width, formsBitmap.Height),
                                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                            try
                            {
                                writeableBitmap.WritePixels(new Int32Rect(0, 0, width, height), data.Scan0, data.Stride * data.Height, data.Stride);
                            }
                            finally
                            {
                                formsBitmap.UnlockBits(data);
                            }

                            formsBitmap.Dispose();
                        }
                    }
                }
                catch (Exception ex)
                {
                    PreviewUnavailableLabel.Text = "Preview Unavailable";
                    PreviewUnavailableBorder.Visibility = Visibility.Visible;
                    previewTimer.Stop();
                    return;
                }
            }
            else if (PreviewUnavailableBorder.Visibility == Visibility.Collapsed)
            {
                PreviewUnavailableLabel.Text = "Preview Paused";
                PreviewUnavailableBorder.Visibility = Visibility.Visible;
            }
        }

        private void Current_PreProcessInput(object sender, PreProcessInputEventArgs e)
        {
            SleepTime = DateTime.Now.AddSeconds(60);
        }      

        /* - this is encapsulated in the server now 
        void SendUDPPacket(byte[] packetToSend)
        {
            System.Net.Sockets.UdpClient udpClient = new System.Net.Sockets.UdpClient("10.13.34.8", 52381);
            System.Net.IPEndPoint RemoteIPEndPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Any, 0);
            try
            {
                udpClient.Send(packetToSend, packetToSend.Length);

                var receive = udpClient.Receive(ref RemoteIPEndPoint);

                string returnData = Encoding.UTF8.GetString(receive);
                Console.WriteLine("Return: " + returnData);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        /*
         * 
         *  01 00 00 07 00 00 00 01 81 01 04 3f 02 01 ff
            String for preset.  The second to last set changes to specify preset
            
            Up: 01 00 00 09 00 00 00 01 81 01 06 01 09 07 03 01 ff
            
            Down: 01 00 00 09 00 00 00 01 81 01 06 01 09 07 03 02 ff
            
            Left: 01 00 00 09 00 00 00 01 81 01 06 01 09 07 01 03 ff
            
            Right: 01 00 00 09 00 00 00 01 81 01 06 01 09 07 02 03 ff
            
            Stop Pan: 01 00 00 09 00 00 00 01 81 01 06 01 01 01 03 03 ff
                        
            Zoom in: 01 00 00 07 00 00 00 01 81 01 04 07 21 ff
            
            Zoom out: 01 00 00 07 00 00 00 01 81 01 04 07 31 ff
            
            Stop Zoom: 01 00 00 06 00 00 00 03 81 01 04 07 00 ff
            
            */

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

            /*var currentAssembly = Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            // Default installation path of VideoLAN.LibVLC.Windows
            var libDirectory = new DirectoryInfo(Path.Combine(currentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));

            this.WindowsFormsHost.Child = vlcControl;

            vlcControl.BeginInit();
            vlcControl.VlcLibDirectory = libDirectory;
            vlcControl.EndInit();*/

            LoadingBox.Visibility = Visibility.Collapsed;
            
            this.DataContext = CameraConfig;
            CameraListBox.SelectedIndex = 0;
            //this.Height = Math.Max(730, 380 + CameraConfig.Cameras.Max(one => one.Presets.Count) / 2 * 80); 
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

            var SelectedCamera = CameraConfig.Cameras[CameraListBox.SelectedIndex];

            //vlcControl.Stop();
            //vlcControl.Play(SelectedCamera.Stream);

            PresetListBox.ItemsSource = SelectedCamera.Presets;
        }

        private void ShowPreviewCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded)
            {
                //WindowsFormsHost.Visibility = Visibility.Visible;
                //this.Width = 1024;
            }
        }

        private void ShowPreviewCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded)
            {
                //WindowsFormsHost.Visibility = Visibility.Collapsed;
                //this.Width = 315;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (mjpegstream != null)
            {
                mjpegstream.Stop();
            }
        }
    }
}
