using Sharpcaster.Core.Models.Media;
using Wlb.MvvmBase;

namespace WpfCastAnalyzer
{
    public class MediaStatusViewModel : BaseViewModel
    {
        private MediaStatus ms;

        public int CurrentItemId
        {
            get { return ms.CurrentItemId; }
            set { this.RaisePCifChanged(ms, () => ms.CurrentItemId, value); }
        }
        public double CurrentTime
        {
            get { return ms.CurrentTime; }
            set { this.RaisePCifChanged(ms, () => ms.CurrentTime, value); }
        }
        public string IdleReason
        {
            get { return ms.IdleReason; }
            set { this.RaisePCifChanged(ms, () => ms.IdleReason, value); }
        }
        public long MediaSessionId
        {
            get { return ms.MediaSessionId; }
            set { this.RaisePCifChanged(ms, () => ms.MediaSessionId, value); }
        }
        public int PlaybackRate
        {
            get { return ms.PlaybackRate; }
            set { this.RaisePCifChanged(ms, () => ms.PlaybackRate, value); }
        }
        public string PlayerState
        {
            get { return ms.PlayerState.ToString(); }
            set { /*this.RaisePCifChanged(ms, () => ms.PlayerState, value);*/ }
        }
        public string RepeatMode
        {
            get { return ms.RepeatMode; }
            set { this.RaisePCifChanged(ms, () => ms.RepeatMode, value); }
        }
        public int SupportedMediaCommands
        {
            get { return ms.SupportedMediaCommands; }
            set { this.RaisePCifChanged(ms, () => ms.SupportedMediaCommands, value);  }
        }
        public double? MediaVolume
        {
            get { return ms.Volume.Level; }
            set { this.RaisePCifChanged(ms, () => ms.Volume.Level, value); }
        }
        public bool? MediaMuted
        {
            get { return ms.Volume.Muted; }
            set { this.RaisePCifChanged(ms, () => ms.Volume.Muted, value); }
        }

        public string MediaContentType
        {
            get { return ms.Media?.ContentType; }
            //set { this.RaisePCifChanged(ms, () => ms.Volume.Muted, value); }
        }

        public string MediaContentUrl
        {
            get { return ms.Media?.ContentUrl; }
            //set { this.RaisePCifChanged(ms, () => ms.Volume.Muted, value); }
        }


        public MediaStatusViewModel(MediaStatus s)
        {
            this.ms = s;
        }
    }
}