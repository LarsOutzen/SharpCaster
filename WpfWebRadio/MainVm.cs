using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMvvm;

namespace WpfWebRadio {
    public class MainVm : BaseViewModel {

        private StationVm CurrentStation = null;

        private List<StationVm> Stations = new List<StationVm>();
        public void AddStation(StationVm svm) {
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

        public void SelectStation(StationVm svm) {
            CurrentStation?.Deselect();
            CurrentStation = svm;
            SelectCurrentStation();
        }


        public void SelectStation(string mediaUrl) {
            CurrentStation?.Deselect();
            CurrentStation = Stations.Where(svm=>svm.Url == mediaUrl).FirstOrDefault();
            SelectCurrentStation();
        }

        private void SelectCurrentStation() {
            if (CurrentStation != null) {
                CurrentStation.Select();
                IsPlaying = true;
            } else {
                IsPlaying = false;
            }
        }

    }
}
