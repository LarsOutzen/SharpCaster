using Sharpcaster;
using Sharpcaster.Core.Channels;
using Sharpcaster.Core.Interfaces;
using Sharpcaster.Core.Models;
using Sharpcaster.Core.Models.ChromecastStatus;
using Sharpcaster.Core.Models.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Wlb.MvvmBase;

namespace WpfCastAnalyzer
{
    public class ChromecastViewModel : BaseViewModel
    {
        #region Constructor & Binding Properties
        public ChromecastReceiver receiver { get; private set; } = null;
        private ChromecastClient client{ get; set; } = null;


        private BindingList<MediaStatusViewModel> _MediaStati = new BindingList<MediaStatusViewModel>();
        public BindingList<MediaStatusViewModel> MediaStati { get { return _MediaStati; }
            set { this.RaisePCifChanged(ref _MediaStati, value, () => MediaStati); }
        }

        private BindingList<ChromecastApplication> _Applications = new BindingList<ChromecastApplication>();
        public BindingList<ChromecastApplication> Applications
        {
            get { return _Applications; }
            set { this.RaisePCifChanged(ref _Applications, value, () => Applications); }
        }


        public ChromecastViewModel(ChromecastReceiver cc)
        {
            receiver = cc;
            ReceiverStatus = cc.Status;
        }

        public string Name
        {
            get { return "Name: " + receiver.Name; }
            set { }
        }
        public string Model
        {
            get { return "Model: " + receiver.Model; }
            set { }
        }
        public string Uri
        {
            get { return "Device Uri: " + receiver.DeviceUri.ToString(); }
            set { }
        }
        private string _ccrStatus;
        public string ReceiverStatus
        {
            get { return _ccrStatus; }
            set { RaisePCifChanged(ref _ccrStatus, value, () => ReceiverStatus);  }
        }

        private bool? _IsActiveInput;
        public bool? IsActiveInput
        {
            get { return _IsActiveInput; }
            set { RaisePCifChanged(ref _IsActiveInput, value, () => IsActiveInput); }
        }

        private bool? _IsStandby;
        public bool? IsStandby
        {
            get { return _IsStandby; }
            set { RaisePCifChanged(ref _IsStandby, value, () => IsStandby); }
        }

        private double? _VolumeLevel;
        public double? VolumeLevel
        {
            get { return _VolumeLevel; }
            set { RaisePCifChanged(ref _VolumeLevel, value, () => VolumeLevel); }
        }

        private bool? _VolumeMuted;
        public bool? VolumeMuted
        {
            get { return _VolumeMuted; }
            set { RaisePCifChanged(ref _VolumeMuted, value, () => VolumeMuted); }
        }

        private string _ConnectedApp;
        public string ConnectedApp
        {
            get { return _ConnectedApp; }
            set { if (RaisePCifChanged(ref _ConnectedApp, value, () => ConnectedApp)) {
                    RaisePC(() => IsAppConnected);
                    } }
        }

        public bool IsAppConnected
        {
            get { return !String.IsNullOrEmpty(ConnectedApp); }
        }

        public bool IsAppConnectedIsMediaSessionAvailable
        {
            get { return true; }
        }
        #endregion

        #region Commands

        public async Task RefreshStatus()
        {
            ChromecastStatus cs;
            if (client != null)
            {
                // Use client if availebale and connected.
                cs = await client.GetChannel<ReceiverChannel>()?.GetChromecastStatusAsync();
            } else {
                // Lets make a new Client and connect.
                client = new ChromecastClient();
                ConnectedApp = "";
                cs = await client.ConnectChromecast(receiver);
                client.Disconnected += Client_Disconnected;
            }
            ProcessCcStatus(cs);
        }

        public async Task Connect(string appId)
        {
            if (ConnectedApp != appId)
            {
                if (client == null)
                {
                    client = new ChromecastClient();
                    var s1 = await client.ConnectChromecast(receiver);
                    ProcessCcStatus(s1);
                }

                var ccStatus = await client.LaunchApplicationAsync(appId); // This joins if app is already runnning on device
                ProcessCcStatus(ccStatus);

                ConnectedApp = appId;
                client.Disconnected += Client_Disconnected;
                client.GetChannel<ReceiverChannel>().StatusChanged += ReceiverStatusChanged;
                client.GetChannel<IMediaChannel>().StatusChanged += MediaStatusChanged;
            } 
        }

        public async Task<MediaStatus> LoadMedia(Media media)
        {
            MediaStatus ms = null;
            if (client != null)
            {
                var mc = client.GetChannel<MediaChannel>();
                ms = await mc.LoadAsync(media);
            }
            return ms;
        }

        public async Task<MediaStatus> PauseAsync()
        {
            MediaStatus ms = null;
            if (client != null)
            {
                var mc = client.GetChannel<MediaChannel>();
                ms = await mc.PauseAsync();
            }
            return ms;
        }

        public async Task<MediaStatus> PlayAsync()
        {
            MediaStatus ms = null;
            if (client != null)
            {
                var mc = client.GetChannel<MediaChannel>();
                ms = await mc.PlayAsync();
            }
            return ms;
        }

        public async Task<MediaStatus> GetMediaStatusAsync()
        {
            MediaStatus ms = null;
            if (client != null)
            {
                var mc = client.GetChannel<MediaChannel>();
                ms = await mc.GetStatusAsync();
            }
            return ms;
        }

        public async Task<MediaStatus> StopAsync()
        {
            MediaStatus ms = null;
            if (client != null)
            {
                var mc = client.GetChannel<MediaChannel>();
                ms = await mc.StopAsync();
            }
            return ms;
        }

        public async Task SetVolume(double v)
        {
            if (v > 0.5) { v = 0.5; }
            var rc = client.GetChannel<IReceiverChannel>();
            await rc?.SetVolume(v);
        }


        #endregion

        private void ProcessCcStatus(ChromecastStatus status)
        {
            ReceiverStatus = receiver.Status;
            IsActiveInput = status?.IsActiveInput;
            IsStandby = status?.IsStandBy;
            VolumeLevel = status?.Volume?.Level ?? 0.0;
            VolumeMuted = status?.Volume?.Muted;
            Applications.Clear();
            if (status?.Applications != null)
            {
                foreach (var a in status.Applications)
                {
                    Applications.Add(a);
                }
            }
        }


        #region chromecast Event - Handler 
        private void MediaStatusChanged(object sender, EventArgs e)
        {
            var status = (sender as MediaChannel).Status;
            Dispatcher.CurrentDispatcher.InvokeAsync(() => {
                foreach (var s in status)
                {
                    MediaStati.Add(new MediaStatusViewModel(s));
                }
            });
        }

        private void ReceiverStatusChanged(object sender, EventArgs e)
        {
            var ccStatus = (sender as ReceiverChannel)?.Status;
            if (ccStatus != null)
            {
                Dispatcher.CurrentDispatcher.InvokeAsync(() =>
                {
                    ProcessCcStatus(ccStatus);
                    if (ccStatus.Applications != null)
                    {
                        if (ccStatus.Applications.Where(a => a.AppId == ConnectedApp).FirstOrDefault() == null)
                        {
                            // Ups jetzt scheint wer anderer der Herr dieses Gerätes zu sein....
                            (sender as ReceiverChannel).Client.DisconnectAsync();
                            ConnectedApp = "";
                        }
                    }
                    else
                    {
                        // ??? 
                        // This is the indication that somebody switched off the speaker device with its On/Off Button
                        (sender as ReceiverChannel).Client.DisconnectAsync();
                        ConnectedApp = "";
                    }
                });
            }
        }

        private void Client_Disconnected(object sender, EventArgs e)
        {
            Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                client = null;
                ConnectedApp = "";
            });
        }

        #endregion


}
public static class MyExtensions
    {
        public static string AsMediaStatusLine(this MediaStatus ms)
        {
            return $"MediaStatus: Id:{ms?.CurrentItemId} Time:{(ms?.CurrentTime.ToString())??"--"}/{ms?.Media?.Duration} IR:{ms?.IdleReason} MsId: {ms?.MediaSessionId} McUrl:{ms?.Media?.ContentUrl}";
        }
    }
    
}
