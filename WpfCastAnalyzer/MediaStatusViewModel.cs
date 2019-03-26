using Sharpcaster.Core.Models.Media;
using MyMvvm;

namespace WpfCastAnalyzer
{
    public class MediaStatusViewModel : BaseViewModel
    {
        private MediaStatus ms;

        public int CurrentItemId
        {
            get { return ms.CurrentItemId; }
            set { this.RaisePcIfChanged(() => ms.CurrentItemId, value, ()=>CurrentItemId, ms); }
        }
        public double CurrentTime
        {
            get { return ms.CurrentTime; }
            set { this.RaisePcIfChanged(() => ms.CurrentTime, value, ()=>CurrentTime, ms); }
        }
        public string IdleReason
        {
            get { return ms.IdleReason; }
            set { this.RaisePcIfChanged(() => ms.IdleReason, value, () => IdleReason, ms); }
        }
        public long MediaSessionId
        {
            get { return ms.MediaSessionId; }
            set { this.RaisePcIfChanged(() => ms.MediaSessionId, value, ()=>MediaSessionId, ms); }
        }
        public int PlaybackRate
        {
            get { return ms.PlaybackRate; }
            set { this.RaisePcIfChanged(() => ms.PlaybackRate, value, ()=>MediaSessionId, ms); }
        }
        public string PlayerState
        {
            get { return ms.PlayerState.ToString(); }
            set { /*this.RaisePCifChanged(ms, () => ms.PlayerState, value);*/ }
        }
        public string RepeatMode
        {
            get { return ms.RepeatMode; }
            set { this.RaisePcIfChanged(() => ms.RepeatMode, value, () => ms.RepeatMode, ms); }
        }
        public int SupportedMediaCommands
        {
            get { return ms.SupportedMediaCommands; }
            set { this.RaisePcIfChanged(() => ms.SupportedMediaCommands, value, ()=>SupportedMediaCommands, ms);  }
        }
        public double? MediaVolume
        {
            get { return ms.Volume.Level; }
            set { this.RaisePcIfChanged(() => ms.Volume.Level, value, ()=>MediaVolume, ms); }
        }
        public bool? MediaMuted
        {
            get { return ms.Volume.Muted; }
            set { this.RaisePcIfChanged(() => ms.Volume.Muted, value, ()=>MediaMuted, ms); }
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