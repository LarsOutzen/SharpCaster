using Sharpcaster;
using Sharpcaster.Core.Channels;
using Sharpcaster.Core.Interfaces;
using Sharpcaster.Core.Models;
using Sharpcaster.Core.Models.ChromecastStatus;
using Sharpcaster.Core.Models.Media;
using Sharpcaster.Logging.ApplicationInsight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
using System.Windows.Shapes;

namespace WpfCastAnalyzer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private Dictionary<string, ChromecastClient> MyClients = new Dictionary<string, ChromecastClient>();
        private ChromecastViewModel CcViewModel { get { return this.SelectCastDevice?.SelectedItem as ChromecastViewModel; } }

        public MainWindow() {
            var logger = new ApplicationInsightLogger();
            InitializeComponent();
            this.SelectCastApp.Items.Add("CC32E753");   // Spotify 
            this.SelectCastApp.Items.Add("B3419EF5");   // Chromecaster
            this.SelectCastApp.SelectedItem = "B3419EF5"; //"CC32E753";
            var t = FindChromcasts();
        }

        private void RefreshDevices_Click(object sender, RoutedEventArgs e)
        {
            this.SelectCastDevice.Items.Clear();
            var t = FindChromcasts();
        }

        private async Task FindChromcasts() {
            IChromecastLocator locator = new Sharpcaster.Discovery.MdnsChromecastLocator();
            var chromecasts = await locator.FindReceiversAsync();
            Dispatcher.Invoke(() => {
                foreach (var cc in chromecasts) {
                    var vm = new ChromecastViewModel(cc);
                    this.SelectCastDevice.Items.Add(vm);
                    if (cc.Name.Contains("Büro"))
                    {
                        this.SelectCastDevice.SelectedItem = vm;
                    }
                }
            });
        }

        //private void SelectCastDevice_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        //    this.var ccr = CcViewModel?.receiver;
        //    if (ccr != null) {
               
        //    }
        //}

        private void VolumeCtrlLocal_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (CcViewModel != null)
            {
                var t = CcViewModel.SetVolume(e.NewValue / 100);
            }
        }

        private void Connect_Click(object sender, RoutedEventArgs e) {
            var t = Connect();   
        }

        private async Task Connect() {
            try {
                await CcViewModel?.Connect(this.SelectCastApp.SelectedItem.ToString());
                //DisplayCcStatus(status);
            } catch (Exception ex) {
                DisplayException(ex);
            }
        }
        
        //private void ClrLog_Click(object sender, RoutedEventArgs e) {
        //    CcViewModel.MediaStati.Clear();
        //}

        //private void LoadMedia_Click(object sender, RoutedEventArgs e) {
        //    if (CcViewModel != null)
        //    {
        //        Media m = new Media
        //        {
        //            ContentUrl = "http://mp3stream3.apasf.apa.at:8000/;",
        //            StreamType = StreamType.Live,
        //            ContentType = "audio/mp4"
        //        };
        //        CallAsyncWithExceptionHandling(CcViewModel.LoadMedia, m);
        //    }
        //}

        //private void LoadMedia2_Click(object sender, RoutedEventArgs e)
        //{
        //    if (CcViewModel != null)
        //    {
        //        Media m = new Media
        //        {
        //            ContentUrl = $"http://192.168.177.44:50002/m/MP3/{Track.Text}.mp3",
        //            StreamType = StreamType.Live,
        //            ContentType = "audio/mp4"
        //        };
        //        CallAsyncWithExceptionHandling(CcViewModel.LoadMedia, m);
        //    } ;
        //}

        //private void Pause_Click(object sender, RoutedEventArgs e) {
        //    if (CcViewModel != null)
        //    {
        //        CallAsyncWithExceptionHandling(CcViewModel.PauseAsync);
        //    }
        //}
   
        //private void Play_Click(object sender, RoutedEventArgs e) {
        //    if (CcViewModel != null)
        //    {
        //        CallAsyncWithExceptionHandling(CcViewModel.PlayAsync);
        //    }
        //}

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (CcViewModel != null)
            {
                CallAsyncWithExceptionHandling(CcViewModel.RefreshStatus);
            }
        }

        //private void GetStatus_Click(object sender, RoutedEventArgs e) {
        //    if (CcViewModel != null)
        //    {
        //        CallAsyncWithExceptionHandling(CcViewModel.GetMediaStatusAsync);
        //    }
        //}

        //private void Stop_Click(object sender, RoutedEventArgs e) {
        //    if (CcViewModel != null)
        //    {
        //        CallAsyncWithExceptionHandling(CcViewModel.StopAsync);
        //    }
        //}


        private void CallAsyncWithExceptionHandling<T>(Func<Media, Task<T>> asyncMethod, Media m)
        {
            var t = asyncMethod(m);
            t.GetAwaiter().OnCompleted(() => {
                if (t.IsFaulted)
                {
                    DisplayException(t.Exception, $"Error Loading Media '{m.ContentUrl}'");
                }
            });
        }

        private void CallAsyncWithExceptionHandling<T>(Func<Task<T>> asyncMethod) {
            var t = asyncMethod();
            t.GetAwaiter().OnCompleted(() => {
                if (t.IsFaulted) {
                    DisplayException(t.Exception);
                }
            });
        }

        private void CallAsyncWithExceptionHandling(Func<Task> asyncMethod)
        {
            var t = asyncMethod();
            t.GetAwaiter().OnCompleted(() => {
                if (t.IsFaulted)
                {
                    DisplayException(t.Exception);
                }
            });
        }

        //private void DisplayMediaStatus(MediaStatus status) {
        //    Dispatcher.InvokeAsync(() => {
        //        //this.Status.Text += "Mediastatus:" + Environment.NewLine;
        //        //if (status != null)
        //        //{
        //        //    this.Status.Text += $"{status.CurrentItemId}/{status.CurrentTime}/{status.ExtendedStatus}/{status.Volume.Level}/{status.Volume.Muted}/{status.IdleReason}/{status.Media?.ContentUrl}/{status.MediaSessionId}/{status.PlaybackRate}/{status.PlayerState}/{status.RepeatMode}/{status.SupportedMediaCommands}" + Environment.NewLine;
        //        //} else
        //        //{
        //        //    this.Status.Text += $"<not available>";
        //        //}
        //    });
        //}

        private void DisplayException( Exception ex, string message = "") {
            Dispatcher.Invoke(() => {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.InnerException?.Message, message);
            });
        }

    
    }
}
