using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using MyMvvm;

namespace WpfWebRadio {
    public class MainVm : BaseViewModel {

   
        private IPlaybackVm CurrentStation = null;
        private List<IPlaybackVm> Stations = new List<IPlaybackVm>();
        private double? duration;
        private double? curtime;


          

        public MainVm() {
           
        }


        internal void SetDuration(double? dur) {
            duration = dur;
            Duration = (curtime?.ToString() ?? "--") + "/" + (duration?.ToString() ?? "--");
        }

        internal void SetCurrentTime(double? currentTime) {
            curtime = currentTime;
            Duration = (curtime?.ToString() ?? "--") + "/" + (duration?.ToString() ?? "--");
        }

        internal void AddCurrentTime(double dt) {
            curtime += dt;
            Duration = (curtime?.ToString() ?? "--") + "/" + (duration?.ToString() ?? "--");
        }

        public void AddStation(IPlaybackVm svm) {
            Stations.Add(svm);
        }

        private string _StatusLine = null;
        public string StatusLine {
            get { return _StatusLine; }
            set { RaisePcIfChanged(() => _StatusLine, value, () => StatusLine); }
        }

        private bool _IsPlaying = false;
        public bool IsPlaying {
            get { return _IsPlaying; }
            set { RaisePcIfChanged(() => _IsPlaying, value, () => IsPlaying); }
        }


        private string _Duration = "--/--";
        public string Duration {
            get { return _Duration?.ToString()??"--"; }
            set { RaisePcIfChanged(() => _Duration, value, () => Duration); }
        }

      
        public void SelectStation(StationVm svm) {
            CurrentStation?.Deselect();
            CurrentStation = svm;
            SelectCurrentStation();
        }

        public void SelectStation(string mediaUrl) {
            CurrentStation?.Deselect();
            CurrentStation = Stations.Where(svm=>svm.HasUrl(mediaUrl)).FirstOrDefault();
            SelectCurrentStation(mediaUrl);
        }

        private void SelectCurrentStation(string mediaUrl = null) {
            if (CurrentStation != null) {
                CurrentStation.Select(mediaUrl);
                IsPlaying = true;
            } else {
                IsPlaying = false;
            }
        }


    }
}
