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

        public MainWindow() {
            var logger = new ApplicationInsightLogger();
            InitializeComponent();
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
                    this.SelectCastDevice.Items.Add(cc);
                    if (cc.Name.Contains("Büro"))
                    {
                        this.SelectCastDevice.SelectedItem = cc;
                    }
                }
            });
        }

        private void SelectCastDevice_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var ccr = this.SelectCastDevice.SelectedItem as ChromecastReceiver;
            if (ccr != null) {
                this.Status.Text = ccr.Name + Environment.NewLine;
                this.Status.Text += ccr.Model + Environment.NewLine;
                this.Status.Text += $"{ccr.DeviceUri}:{ccr.Port}" + Environment.NewLine;
                foreach (var ei in ccr.ExtraInformation) {
                    this.Status.Text += $"   {ei.Key}: '{ei.Value}'" + Environment.NewLine;
                }
               this.Status.Text += ccr.Status + Environment.NewLine;
            }
        }

        private void VolumeCtrlLocal_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            var ccr = this.SelectCastDevice.SelectedItem as ChromecastReceiver;
            if (MyClients.TryGetValue(ccr.Name, out ChromecastClient ccc))
            {
                ccc.GetChannel<IReceiverChannel>().SetVolume(e.NewValue / 100);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            var t = Connect();   
        }

        private async Task Connect() {
            try {
                ChromecastReceiver ccr = this.SelectCastDevice.SelectedItem as ChromecastReceiver;
                if (ccr != null) {
                    ChromecastClient client;
                    if (!MyClients.TryGetValue(ccr.Name, out client)) {
                        client = new ChromecastClient();
                        var status = await client.ConnectChromecast(ccr);
                        DisplayCcStatus(status);

                        status = await client.LaunchApplicationAsync("B3419EF5"); // This joins if app is already runnning on device
                        DisplayCcStatus(status);

                        MyClients.Add(ccr.Name, client);
                        client.Disconnected += Client_Disconnected;
                        client.GetChannel<ReceiverChannel>().StatusChanged += ReceiverStatusChanged;
                        client.GetChannel<IMediaChannel>().StatusChanged += MediaStatusChanged;
                    } else {
                        DisplayCcStatus(client.GetChromecastStatus());
                    }
                }
            } catch (Exception ex) {
                DisplayException(ex);
            }
        }
        
        private void ClrlOG_Click(object sender, RoutedEventArgs e) {
            this.Status.Text = "";
        }

        private void GetMedia_Click(object sender, RoutedEventArgs e) {
            var t = LoadMedia();
        }

        private async Task LoadMedia() {
            try { 
                ChromecastReceiver ccr = this.SelectCastDevice.SelectedItem as ChromecastReceiver;
                ChromecastClient client;
                if (MyClients.TryGetValue(ccr?.Name, out client)) {
                    //var mchannel = client.GetChannel<IMediaChannel>();
                    var media = new Media {
                        ContentUrl = "http://mp3stream3.apasf.apa.at:8000/;",
                        ContentType = "audio/mp4"
                        
                    };
                    var ms = await client.GetChannel<IMediaChannel>().LoadAsync(media);
                    DisplayMediaStatus(ms);
                }
            } catch (Exception ex) {
                DisplayException(ex);
            }
        }


        private void MediaStatusChanged(object sender, EventArgs e) {
            var status = (sender as MediaChannel).Status;
            foreach (var s in status) {
                DisplayMediaStatus(s);
            }
        }

        private void ReceiverStatusChanged(object sender, EventArgs e) {
            ChromecastStatus status = (sender as ReceiverChannel)?.Status;
            DisplayCcStatus(status);
            if (status.Applications == null) {
                // This is the indication that somebody switched off the speaker device with its On/Off Button
                (sender as ReceiverChannel).Client.DisconnectAsync();
            }
            Dispatcher.Invoke(() => {
                this.VolumeCtrlDevice.Value = (status.Volume.Level??0.0) * 100.0;
            });
        }

        private void Client_Disconnected(object sender, EventArgs e) {
            Dispatcher.Invoke(() => {
                this.Status.Text += "**Disconnected**" + Environment.NewLine;
                ChromecastReceiver ccr = this.SelectCastDevice.SelectedItem as ChromecastReceiver;
                MyClients.Remove(ccr?.Name);
            });
        }
        
        private void Action2_Click(object sender, RoutedEventArgs e) {
            ChromecastReceiver ccr = this.SelectCastDevice.SelectedItem as ChromecastReceiver;
            ChromecastClient client;
            if (MyClients.TryGetValue(ccr?.Name, out client)) {
                var mc = client.GetChannel<MediaChannel>();
                CallAsyncWithExceptionHandling(mc.PauseAsync);
            }
        }

        private void Action3_Click(object sender, RoutedEventArgs e) {
            ChromecastReceiver ccr = this.SelectCastDevice.SelectedItem as ChromecastReceiver;
            ChromecastClient client;
            if (MyClients.TryGetValue(ccr?.Name, out client)) {
                var mc = client.GetChannel<MediaChannel>();
                CallAsyncWithExceptionHandling(mc.PlayAsync);
            }
        }

        private void Action4_Click(object sender, RoutedEventArgs e) {
            ChromecastReceiver ccr = this.SelectCastDevice.SelectedItem as ChromecastReceiver;
            ChromecastClient client;
            if (MyClients.TryGetValue(ccr?.Name, out client)) {
                //var ms = client.GetMediaStatus();
                var mc = client.GetChannel<MediaChannel>();
                CallAsyncWithExceptionHandling(mc.GetStatusAsync);
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e) {
            ChromecastReceiver ccr = this.SelectCastDevice.SelectedItem as ChromecastReceiver;
            ChromecastClient client;
            if (MyClients.TryGetValue(ccr?.Name, out client)) {
                var mc = client.GetChannel<MediaChannel>();
                CallAsyncWithExceptionHandling(mc.StopAsync);
            }
        }


       
        private void CallAsyncWithExceptionHandling(Func<Task<MediaStatus>> asyncMethod) {
            var t = asyncMethod();
            t.GetAwaiter().OnCompleted(() => {
                if (t.IsFaulted) {
                    DisplayException(t.Exception);
                }
            });
        }

        private void DisplayCcStatus(ChromecastStatus status) {
            Dispatcher.InvokeAsync(() => {
                this.Status.Text += "ChromecastStatus:" + Environment.NewLine;
                if (Status != null) {
                    this.Status.Text += $"{status.IsActiveInput}/{status.IsActiveInput}/{status.Applications?.Count()}/{status.Volume.Level}/{status.Volume.Muted}" + Environment.NewLine;
                    if (status.Applications != null) {
                        foreach (var a in status.Applications) {
                            this.Status.Text += $"app>{a.AppId}/{a.DisplayName}/{a.SessionId}/{a.StatusText}/{a.TransportId}" + Environment.NewLine;
                        }
                    }
                } else {
                    this.Status.Text += "<null>";
                }
            });
        }

        private void DisplayMediaStatus(MediaStatus status) {
            Dispatcher.InvokeAsync(() => {
                this.Status.Text += "Mediastatus:" + Environment.NewLine;
                this.Status.Text += $"{status.CurrentItemId}/{status.CurrentTime}/{status.ExtendedStatus}/{status.Volume.Level}/{status.Volume.Muted}/{status.IdleReason}/{status.Media?.ContentUrl}/{status.MediaSessionId}/{status.PlaybackRate}/{status.PlayerState}/{status.RepeatMode}/{status.SupportedMediaCommands}" + Environment.NewLine;
            });
        }

        private void DisplayException(Exception ex) {
            Dispatcher.Invoke(() => {
                this.Status.Text += "Exception:" + Environment.NewLine;
                this.Status.Text += ex.ToString();
            });
        }

      
    }
}
