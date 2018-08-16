using Sharpcaster;
using Sharpcaster.Core.Channels;
using Sharpcaster.Core.Interfaces;
using Sharpcaster.Core.Models;
using Sharpcaster.Core.Models.ChromecastStatus;
using Sharpcaster.Core.Models.Media;
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
using System.Windows.Shapes;

namespace WpfWebRadio {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private const string SharpcasterAppId = "B3419EF5";
        private Dictionary<string, ChromecastClient> MyClients = new Dictionary<string, ChromecastClient>();
        private ChromecastClient SelectedChromeCastClient = null;

        public MainWindow() {
            InitializeComponent();
            ReadStationConfig();
            CallAsyncWithExceptionHandling(FindChromcastsAsync(), (ex) => { DisplayException(ex); });
        }

        private void ReadStationConfig() {
         //   int i = 0;
            foreach (string settingLine in Properties.Settings.Default.MyStations) {
                string[] args = settingLine.Split('|');
                Button b = new Button() {
           //         Name = $"Button_{i++}",
                    MinWidth = 60,
                    MinHeight = 60,
                    Margin = new Thickness(8),
                    Content = args[1],      // The Stations name
                    DataContext = args[0],  // The Stations URI
                    IsEnabled = false
                };
                b.Click += StationButton_Click; ;
                this.StationPanel.Children.Add(b);
            }
        }

        private async Task FindChromcastsAsync() {
            IChromecastLocator locator = new Sharpcaster.Discovery.MdnsChromecastLocator();
            var chromecasts = await locator.FindReceiversAsync();
            Dispatcher.Invoke(() => {
                foreach (var cc in chromecasts) {
                    this.SelectCastDevice.Items.Add(cc);
                    if (cc.Name.Equals(Properties.Settings.Default.DefaultCastName)) {
                        this.SelectCastDevice.SelectedItem = cc;
                    }
                }
                Status.Text = $"Found {chromecasts.Count()} Chromecast(s).";
            });
        }

        private void SelectCastDevice_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var ccr = (this.SelectCastDevice.SelectedItem as ChromecastReceiver);
            if (ccr != null) {
                if (!MyClients.TryGetValue(ccr.Name, out this.SelectedChromeCastClient)) {
                    // First use of this receiver. Lets connect a Client.
                    DisableButtons();
                    this.SelectedChromeCastClient = null;
                    CallAsyncWithExceptionHandling(ConnectClient(ccr), (ex) => { DisplayException(ex); });
                } else {
                    var st = this.SelectedChromeCastClient?.GetChromecastStatus();
                    Status.Text = $"Switched to {ccr.Name}. Volume: {st?.Volume?.Level} ";
                }
            }
        }

        private async Task ConnectClient(ChromecastReceiver ccr) {
            ChromecastClient client = new ChromecastClient();
            var status = await client.ConnectChromecast(ccr);
            status = await client.LaunchApplicationAsync(SharpcasterAppId); // This joins if app is already runnning on device

            MyClients.Add(ccr.Name, client);
            client.Disconnected += Client_Disconnected;
            client.GetChannel<ReceiverChannel>().StatusChanged += Client_ReceiverStatusChanged;
            client.GetChannel<IMediaChannel>().StatusChanged += Client_MediaStatusChanged;

            Dispatcher.Invoke(() => {
                this.SelectedChromeCastClient = client;
                EnableButtons();
                Status.Text = $"App: {status.Applications[0].DisplayName} on {ccr.Name} connected. Volume: {status.Volume.Level}";
            });
        }

        private void Client_MediaStatusChanged(object sender, EventArgs e) {
            MediaChannel mc = sender as MediaChannel;
            ChromecastClient client = (mc?.Client as ChromecastClient);
            string receiverName = MyClients.FirstOrDefault(x => x.Value == client).Key;

            MediaStatus ms = client?.GetMediaStatus();
            if (ms != null) {
                Dispatcher.InvokeAsync(() => Status.Text = $"{receiverName}/M[{ms.MediaSessionId}]: {ms.CurrentTime}/{ms.PlayerState}/{ms.Media?.ContentUrl}/{ms.IdleReason}");
            }
        }

        private void Client_ReceiverStatusChanged(object sender, EventArgs e) {
            ReceiverChannel rc = sender as ReceiverChannel;
            ChromecastClient client = (rc?.Client as ChromecastClient);
            string receiverName = MyClients.FirstOrDefault(x => x.Value == client).Key;

            ChromecastStatus status = rc?.Status;
            ChromecastApplication a = status.Applications?.Where(ap => ap.AppId == SharpcasterAppId).FirstOrDefault();
            if (a == null) {
                // This is the indication that somebody switched off the (speaker) device with its On/Off Button
                CallAsyncWithExceptionHandling(DisconnectDeviceAsync(receiverName, rc), (ex)=>DisplayException(ex));
            }
            Dispatcher.InvokeAsync(() => Status.Text = $"{receiverName}/C[{a?.AppId}]: {a?.DisplayName}/{status.Volume.Level}");
        }

        private void Client_Disconnected(object sender, EventArgs e) {
            var client = sender as ChromecastClient;
            Dispatcher.InvokeAsync(() => Status.Text += $"*** Disconnected App.");
        }

        private async Task DisconnectDeviceAsync(String receiverName, ReceiverChannel rc) {
            Dispatcher.Invoke(() => {
                MyClients.Remove(receiverName);
                if (rc.Client == SelectedChromeCastClient) {
                    SelectedChromeCastClient = null;
                    DisableButtons();
                }
            });
            await rc.Client.DisconnectAsync();
        }

        private void StationButton_Click(object sender, RoutedEventArgs e) {
            Button b = sender as Button;
            CallAsyncWithExceptionHandling(LoadMedia((string)b.DataContext), (ex) => DisplayException(ex));
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e) {
            CallAsyncWithExceptionHandling(SelectedChromeCastClient?.GetChannel<IMediaChannel>()?.StopAsync(), (ex) => DisplayException(ex));
        }

        private async Task LoadMedia(string meduiaUrl) {
            var media = new Media {
                ContentUrl = meduiaUrl,
                StreamType = StreamType.Live,
                ContentType = "audio/mp4"

            };
            await SelectedChromeCastClient?.GetChannel<IMediaChannel>()?.LoadAsync(media);
        }


        private void DisableButtons() {
            foreach (var c in this.StationPanel.Children) {
                Button b = c as Button;
                if (b != null) {
                    b.IsEnabled = false;
                }
            }
        }

        private void EnableButtons() {
            foreach (var c in this.StationPanel.Children) {
                Button b = c as Button;
                if (b != null) {
                    b.IsEnabled = true;
                }
            }
        }

        private void DisplayException(Exception ex) {
            Dispatcher.Invoke(() => {
                Status.Text = ex.Message;
                if (ex.InnerException != null) {
                    MessageBox.Show(ex.Message + Environment.NewLine + ex.InnerException.Message);
                }
            });
        }

        private void CallAsyncWithExceptionHandling(Task t, Action<Exception> callback) {
            t.GetAwaiter().OnCompleted(() => {
                if (t.IsFaulted) {
                    callback(t.Exception);
                }
            });
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e) {
            //foreach (var cc in MyClients.Values) {
            //    try {
            //        var t = cc.DisconnectAsync();
            //    } catch (Exception ex) {
            //        DisplayException(ex);
            //    }
            //}
        }

    }
}
