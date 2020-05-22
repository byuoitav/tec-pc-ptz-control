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
using Vlc.DotNet.Forms;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ComponentModel;

namespace BYUPTZControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        VlcControl vlcControl = new VlcControl();

        public MainWindow()
        {
            InitializeComponent();
                        
            this.WindowsFormsHost.Child = vlcControl;

            var currentAssembly = Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            // Default installation path of VideoLAN.LibVLC.Windows
            var libDirectory = new DirectoryInfo(Path.Combine(currentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));

            vlcControl.BeginInit();
            vlcControl.VlcLibDirectory = libDirectory;
            vlcControl.EndInit();

            //vlcControl.Play("url");
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

        private void preset1Button_Click(object sender, RoutedEventArgs e)
        {
            //preset 1
            //SendUDPPacket(new byte[] { 0x01, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x01, 0x81, 0x01, 0x04, 0x3f, 0x02, 0x01, 0xff });            
        }

        private void preset2Button_Click(object sender, RoutedEventArgs e)
        {
            //preset 2
            //SendUDPPacket(new byte[] { 0x01, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x01, 0x81, 0x01, 0x04, 0x3f, 0x02, 0x02, 0xff });
        }

        private void preset3Button_Click(object sender, RoutedEventArgs e)
        {
            //preset 3
            //SendUDPPacket(new byte[] { 0x01, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x01, 0x81, 0x01, 0x04, 0x3f, 0x02, 0x03, 0xff });
        }

        private void preset4Button_Click(object sender, RoutedEventArgs e)
        {
            //preset 4
            //SendUDPPacket(new byte[] { 0x01, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x01, 0x81, 0x01, 0x04, 0x3f, 0x02, 0x04, 0xff });
        }

        private void upButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //up
            //SendUDPPacket(new byte[] { 0x01, 0x00, 0x00, 0x09, 0x00, 0x00, 0x00, 0x01, 0x81, 0x01, 0x06, 0x01, 0x09, 0x09, 0x03, 0x01, 0xff });
        }

        private void stopMovementButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //stop
            //SendUDPPacket(new byte[] { 0x01, 0x00, 0x00, 0x09, 0x00, 0x00, 0x00, 0x01, 0x81, 0x01, 0x06, 0x01, 0x01, 0x01, 0x03, 0x03, 0xff });
        }

        private void downButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //down
            //SendUDPPacket(new byte[] { 0x01, 0x00, 0x00, 0x09, 0x00, 0x00, 0x00, 0x01, 0x81, 0x01, 0x06, 0x01, 0x09, 0x09, 0x03, 0x02, 0xff });
        }

        private void leftButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //left
            //SendUDPPacket(new byte[] { 0x01, 0x00, 0x00, 0x09, 0x00, 0x00, 0x00, 0x01, 0x81, 0x01, 0x06, 0x01, 0x09, 0x07, 0x01, 0x03, 0xff });
        }
        
        private void rightButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //right
            //SendUDPPacket(new byte[] { 0x01, 0x00, 0x00, 0x09, 0x00, 0x00, 0x00, 0x01, 0x81, 0x01, 0x06, 0x01, 0x09, 0x07, 0x02, 0x03, 0xff });
        }

        private void zoomInButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //zoom in
            //SendUDPPacket(new byte[] { 0x01, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x01, 0x81, 0x01, 0x04, 0x07, 0x21, 0xff });
        }

        private void zoomOutButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //zoom out            
            //SendUDPPacket(new byte[] { 0x01, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x01, 0x81, 0x01, 0x04, 0x07, 0x31, 0xff });
        }

        private void zoomOutButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //stop zoom
            //SendUDPPacket(new byte[] { 0x01, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00, 0x03, 0x81, 0x01, 0x04, 0x07, 0x00, 0xff });            
        }

        private void WindowsFormsHost_ChildChanged(object sender, System.Windows.Forms.Integration.ChildChangedEventArgs e)
        {

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
            LoadingBox.Visibility = Visibility.Collapsed;

            this.DataContext = CameraConfig;
            CameraListBox.SelectedIndex = 0;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            //load the config
            var configURL = Properties.Settings.Default.ConfigurationURL.Replace("{hostname}", Environment.MachineName.ToUpper());

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var task = client.GetAsync(configURL);
                if (!task.Result.IsSuccessStatusCode)
                {
                    MessageBox.Show("Unable to load configuration for host [" + Environment.MachineName + "]");
                    System.Windows.Application.Current.Shutdown();
                    return;
                }

                string responseBody = task.Result.Content.ReadAsStringAsync().Result;
                CameraConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<CameraList>(responseBody);
            }
        }
    }
}
