using System;
using System.Collections.Generic;
using System.Linq;
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
            CreateProgress();
        }
        internal double? GetDuration() {
            return duration;
        }

        internal void SetCurrentTime(double? currentTime) {
            curtime = currentTime;
            CreateProgress();
        }

        internal void AddCurrentTime(double dt) {
            if(curtime != null && curtime > 0.0) {
                curtime += dt;
                CreateProgress();
            }
        }

        private void CreateProgress() {
            Duration = (curtime?.ToString() ?? "--") + "/" + (duration?.ToString() ?? "--");
            if(curtime != null && duration != null) {
                Progress = (int)(curtime * 100/ duration);
            } else {
                Progress = 50;
            }
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
            get { return _Duration?.ToString() ?? "--"; }
            set { RaisePcIfChanged(() => _Duration, value, () => Duration); }
        }

        private int _Progress = 50;
        public int Progress {
            get { return _Progress; }
            set { 
                if (RaisePcIfChanged(() => _Progress, value, () => Progress)) {
                    ProgressSlider = value;
                }
            }
        }


        private int _ProgressSlider = 50;
        public int ProgressSlider {
            get { return _ProgressSlider; }
            set { RaisePcIfChanged(() => _ProgressSlider, value, () => ProgressSlider); }
        }


        public void SelectStation(StationVm svm) {
            CurrentStation?.Deselect();
            CurrentStation = svm;
            SelectCurrentStation();
        }

        public void SelectStation(string mediaUrl) {
            CurrentStation?.Deselect();
            CurrentStation = Stations.Where(svm => svm.HasUrl(mediaUrl)).FirstOrDefault();
            SelectCurrentStation(mediaUrl);
        }

        private void SelectCurrentStation(string mediaUrl = null) {
            if(CurrentStation != null) {
                CurrentStation.Select(mediaUrl);
                IsPlaying = true;
            } else {
                IsPlaying = false;
            }
        }

        
    }
}
