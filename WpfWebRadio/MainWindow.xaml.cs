using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Sharpcaster;
using Sharpcaster.Core.Channels;
using Sharpcaster.Core.Interfaces;
using Sharpcaster.Core.Models;
using Sharpcaster.Core.Models.ChromecastStatus;
using Sharpcaster.Core.Models.Media;

namespace WpfWebRadio {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private static readonly int C_SYNC_SECONDS = 10;
        private Timer timer = new Timer();
        private int syncTicks = C_SYNC_SECONDS;

        private MainVm MainViewModel = new MainVm();

        private const string SharpcasterAppId = "B3419EF5";
        private Dictionary<string, ChromecastClient> MyClients = new Dictionary<string, ChromecastClient>();
        private ChromecastClient SelectedChromeCastClient = null;

        public MainWindow() {
            InitializeComponent();
            DataContext = MainViewModel;
            ReadStationConfig();
            CallAsyncWithExceptionHandling(FindChromcastsAsync(), (ex) => { DisplayException(ex); });
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e) {
            if(MainViewModel.IsPlaying) {
                MainViewModel.AddCurrentTime(1.0);
                syncTicks--;
                if(syncTicks <= 0) {
                    syncTicks = C_SYNC_SECONDS;
                    SelectedChromeCastClient?.GetChannel<MediaChannel>()?.GetStatusAsync();
                }
            }
        }

        // Create GUI Section with Station/Podcast buttons from Config
        private void ReadStationConfig() {
            foreach(string settingLine in Properties.Settings.Default.MyStations) {
                string[] args = settingLine.Split('|');
                AddRadioStation(new StationVm() { Url = args[0], Title = args[1] });
            }
            foreach(string settingLine in Properties.Settings.Default.MyPodcasts) {
                string[] args = settingLine.Split('|');
                PodcastVm pc = new PodcastVm(args);
                pc.LoadTitles();
                AddPodcast(pc);
            }
        }

        private void AddPodcast(PodcastVm pc) {
            MainViewModel.AddStation(pc);
            StackPanel sp = new StackPanel() {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(8),
                IsEnabled = false
            };

            ComboBox cb = new ComboBox() {
                MinWidth = 60,
                MinHeight = 60,
                DataContext = pc,
                IsEnabled = true,
                BorderThickness = new Thickness(0, 4, 4, 4)
            };
            cb.VerticalContentAlignment = VerticalAlignment.Center;
            cb.SelectionChanged += Podcast_SelectionChanged;

            Button b = new Button() {
                MinWidth = 60,
                MinHeight = 60,
                DataContext = pc,
                IsEnabled = true,
                BorderThickness = new Thickness(4, 4, 0, 4)
            };
            sp.Children.Add(b);
            sp.Children.Add(cb);

            Binding myBinding = new Binding("Title") { Source = pc };
            b.SetBinding(Button.ContentProperty, myBinding);

            Binding myBinding2 = new Binding("IsPlaying") { Source = pc };
            myBinding2.Converter = new IsPlayingConverter();
            b.SetBinding(Button.BackgroundProperty, myBinding2);

            Binding myBinding3 = new Binding("AllTitles") { Source = pc };
            cb.SetBinding(ComboBox.ItemsSourceProperty, myBinding3);

            Binding myBinding4 = new Binding("SelectedTitle") { Source = pc };
            cb.SetBinding(ComboBox.SelectedItemProperty, myBinding4);

            b.Click += PodcastButton_Click;
            this.StationPanel.Children.Add(sp);
        }

        private void AddRadioStation(StationVm svm) {
            MainViewModel.AddStation(svm);

            Button b = new Button() {
                MinWidth = 60,
                MinHeight = 60,
                Margin = new Thickness(8),
                DataContext = svm,
                IsEnabled = false
            };
            Binding myBinding = new Binding("Title") { Source = svm };
            b.SetBinding(Button.ContentProperty, myBinding);

            Binding myBinding2 = new Binding("IsPlaying") { Source = svm };
            myBinding2.Converter = new IsPlayingConverter();
            b.SetBinding(Button.BackgroundProperty, myBinding2);

            b.Click += StationButton_Click; ;
            this.StationPanel.Children.Add(b);
        }


        private async Task FindChromcastsAsync() {
            IChromecastLocator locator = new Sharpcaster.Discovery.MdnsChromecastLocator();
            var chromecasts = await locator.FindReceiversAsync();
            Dispatcher.Invoke(() => {
                foreach(var cc in chromecasts) {
                    this.SelectCastDevice.Items.Add(cc);
                    if(cc.Name.Equals(Properties.Settings.Default.DefaultCastName)) {
                        this.SelectCastDevice.SelectedItem = cc;
                    }
                }
                MainViewModel.StatusLine = $"Found {chromecasts.Count()} Chromecast(s).";
            });
        }

        private void SelectCastDevice_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var ccr = (this.SelectCastDevice.SelectedItem as ChromecastReceiver);
            if(ccr != null) {
                if(!MyClients.TryGetValue(ccr.Name, out this.SelectedChromeCastClient)) {
                    // First use of this receiver. Lets connect a Client.
                    DisableButtons();
                    this.SelectedChromeCastClient = null;
                    CallAsyncWithExceptionHandling(ConnectClient(ccr), (ex) => { DisplayException(ex); });
                } else {
                    var st = this.SelectedChromeCastClient?.GetChromecastStatus();
                    MainViewModel.StatusLine = $"Switched to {ccr.Name}. Volume: {st?.Volume?.Level} ";
                    EnableButtons(null);
                    VolumeCtrlLocal.Value = (st?.Volume?.Level ?? 0) * 100;
                    VolumeCtrlDevice.Value = (st?.Volume?.Level ?? 0) * 100;
                    CallAsyncWithExceptionHandling(this.SelectedChromeCastClient?.GetChannel<MediaChannel>().GetStatusAsync(),
                                                   (ex) => { DisplayException(ex); });
                }
            }
        }

        private async Task ConnectClient(ChromecastReceiver ccr) {
            ChromecastClient client = new ChromecastClient();
            var status = await client.ConnectChromecast(ccr);
            // Check if somebody other is using this device.
            bool? others = status.Applications?.Exists(a => a.AppId != SharpcasterAppId);
            if(others == true) {
                // Force a connect !?
            } else {
                status = await client.LaunchApplicationAsync(SharpcasterAppId); // This joins if app is already runnning on device

                MyClients.Add(ccr.Name, client);
                client.Disconnected += Client_Disconnected;
                client.GetChannel<ReceiverChannel>().StatusChanged += Client_ReceiverStatusChanged;
                client.GetChannel<IMediaChannel>().StatusChanged += Client_MediaStatusChanged;

                MediaStatus ms = await client.GetChannel<MediaChannel>().GetStatusAsync();
                string mediaUrl = ms?.Media?.ContentUrl;


                Dispatcher.Invoke(() => {
                    this.SelectedChromeCastClient = client;
                    EnableButtons(mediaUrl);
                    MainViewModel.StatusLine = $"App: {status.Applications[0].DisplayName} on {ccr.Name} connected. Volume: {status?.Volume?.Level} Url:{mediaUrl}";
                    MainViewModel.SetDuration(ms?.Media?.Duration);
                    MainViewModel.SetCurrentTime(ms?.CurrentTime);

                    VolumeCtrlLocal.Value = (status?.Volume?.Level ?? 0) * 100;
                    VolumeCtrlDevice.Value = (status?.Volume?.Level ?? 0) * 100;
                });
            }
        }

        private void Client_MediaStatusChanged(object sender, EventArgs e) {
            MediaChannel mc = sender as MediaChannel;
            ChromecastClient client = (mc?.Client as ChromecastClient);
            string receiverName = MyClients.FirstOrDefault(x => x.Value == client).Key;

            MediaStatus ms = client?.GetMediaStatus();
            string medienUrl = ms?.Media?.ContentUrl;
            if(ms != null) {
                Dispatcher.InvokeAsync(() => {
                    MainViewModel.StatusLine = $"{receiverName}/M[{ms.MediaSessionId}]: {ms.CurrentTime}/{ms.PlayerState}/{ms.Media?.ContentUrl}/{ms.IdleReason}";
                    MainViewModel.SetCurrentTime(ms.CurrentTime);
                    if((this.SelectedChromeCastClient == client) && (!string.IsNullOrEmpty(medienUrl))) {
                        EnableButtons(medienUrl);
                    }
                    if (MainViewModel.IsPlaying && ms.PlayerState == PlayerStateType.Idle) {
                        EnableButtons(null);
                        MainViewModel.SetDuration(null);
                    }
                });
            }
        }

        private void Client_ReceiverStatusChanged(object sender, EventArgs e) {
            ReceiverChannel rc = sender as ReceiverChannel;
            ChromecastClient client = (rc?.Client as ChromecastClient);
            string receiverName = MyClients.FirstOrDefault(x => x.Value == client).Key;

            ChromecastStatus status = rc?.Status;
            ChromecastApplication a = status.Applications?.Where(ap => ap.AppId == SharpcasterAppId).FirstOrDefault();
            if(a == null) {
                // This is the indication that somebody switched off the (speaker) device with its On/Off Button
                CallAsyncWithExceptionHandling(DisconnectDeviceAsync(receiverName, rc), (ex) => DisplayException(ex));
            }
            Dispatcher.InvokeAsync(() => {
                MainViewModel.StatusLine = $"{receiverName}/C[{a?.AppId}]: {a?.DisplayName}/{status?.Volume?.Level}";
                VolumeCtrlDevice.Value = (status?.Volume?.Level ?? 0) * 100;
            });
        }

        private void Client_Disconnected(object sender, EventArgs e) {
            var client = sender as ChromecastClient;
            Dispatcher.InvokeAsync(() => MainViewModel.StatusLine += $"*** Disconnected App.");
        }

        private async Task DisconnectDeviceAsync(String receiverName, ReceiverChannel rc) {
            Dispatcher.Invoke(() => {
                MyClients.Remove(receiverName);
                if(rc.Client == SelectedChromeCastClient) {
                    SelectedChromeCastClient = null;
                    DisableButtons();
                }
            });
            await rc.Client.DisconnectAsync();
        }

        private void PodcastButton_Click(object sender, RoutedEventArgs e) {
            Button b = sender as Button;
            try {
                PodcastVm pc = (PodcastVm)b.DataContext;
                pc.PlaySelected();
                CallAsyncWithExceptionHandling(LoadMedia(pc.Url), (ex) => DisplayException(ex));
            } catch(Exception ex) {
                DisplayException(ex);
            }
        }

        private void Podcast_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ComboBox cb = sender as ComboBox;
            try {
                PodcastVm pc = (PodcastVm)cb.DataContext;
                if(pc.IsPlaying) {
                    pc.PlaySelected();
                    CallAsyncWithExceptionHandling(LoadMedia(pc.Url), (ex) => DisplayException(ex));
                }
            } catch(Exception ex) {
                DisplayException(ex);
            }
        }

        private void StationButton_Click(object sender, RoutedEventArgs e) {
            Button b = sender as Button;
            StationVm svm = (StationVm)b.DataContext;
            MainViewModel.SelectStation(svm);
            CallAsyncWithExceptionHandling(LoadMedia(svm.Url), (ex) => DisplayException(ex));
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e) {
            if(MainViewModel.IsPlaying) {
                CallAsyncWithExceptionHandling(SelectedChromeCastClient?.GetChannel<IMediaChannel>()?.StopAsync(), (ex) => DisplayException(ex));
                MainViewModel.SetDuration(null);
            }
            EnableButtons(null);
        }

        private void VolumeCtrlLocal_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            CallAsyncWithExceptionHandling(SelectedChromeCastClient?.GetChannel<ReceiverChannel>()?.SetVolume(e.NewValue / 100), (ex) => DisplayException(ex));
        }


        private async Task LoadMedia(string meduiaUrl) {
            var media = new Media {
                ContentUrl = meduiaUrl,
                StreamType = StreamType.Live,
                ContentType = "audio/mp4"
            };
            var ms = await SelectedChromeCastClient?.GetChannel<IMediaChannel>()?.LoadAsync(media);
            MainViewModel.SetDuration(ms?.Media?.Duration);
        }


        private void DisableButtons() {
            foreach(var c in this.StationPanel.Children) {
                FrameworkElement uie = c as FrameworkElement;
                if(uie != null && uie.Name != "StopBtn") {
                    uie.IsEnabled = false;
                }
            }
        }

        private void EnableButtons(string mediaUrl) {
            foreach(var c in this.StationPanel.Children) {
                FrameworkElement uie = c as FrameworkElement;
                if(uie != null && uie.Name != "StopBtn") {
                    uie.IsEnabled = true;
                }
            }
            MainViewModel.SelectStation(mediaUrl);
        }

        private void DisplayException(Exception ex) {
            Dispatcher.Invoke(() => {
                MainViewModel.StatusLine = ex.Message;
                if(ex.InnerException != null) {
                    System.Windows.MessageBox.Show(ex.Message + Environment.NewLine + ex.InnerException.Message);
                }
            });
        }

        private void CallAsyncWithExceptionHandling(Task t, Action<Exception> callback) {
            t.GetAwaiter().OnCompleted(() => {
                if(t.IsFaulted) {
                    callback(t.Exception);
                }
            });
        }

  
        private void Window_Unloaded(object sender, RoutedEventArgs e) {

        }

        private bool InUserDrag = false;
        private void ProgressSl_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e) {
            InUserDrag = true;
        }
        private void ProgressSl_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e) {
            InUserDrag = false;
            DoSeek();
        }

        private void ProgressSl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if(!InUserDrag && e.NewValue != MainViewModel.Progress) {
                DoSeek();
            } 
        }

        private void DoSeek() {
            double? dur = MainViewModel.GetDuration();
            if(dur != null) {
                double seek = (dur ?? 0.0) * ProgressSl.Value / 100.0;
                if(seek >= 0) {
                    CallAsyncWithExceptionHandling(SelectedChromeCastClient?.GetChannel<IMediaChannel>()?.SeekAsync(seek), (ex) => DisplayException(ex));
                }
            } else {
                MainViewModel.ProgressSlider = MainViewModel.Progress;
            }
        }
    }
}

#region IValueConverter for IsPlaying -> Background Brush 
class IsPlayingConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        bool? isSet = value as bool?;
        if(isSet ?? false) {
            return Brushes.Brown;
        }
        return Brushes.LightGray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        Brush b = value as Brush;
        if(b.Equals(Brushes.Brown)) {
            return true;
        }
        return false;
    }
}
    #endregion

