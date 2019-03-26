using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using MyMvvm;

namespace WpfWebRadio {
    public class StationVm :BaseViewModel  {

        private string _Url = null;
        public string Url {
            get { return _Url; }
            set { RaisePcIfChanged(() => _Url, value, () => Url);  }
        }

        private string _Title = null;
        public string Title {
            get { return _Title; }
            set { RaisePcIfChanged(() => _Title, value, () => Title); }
        }

        private bool _IsPlaying = false;
        public bool IsPlaying {
            get { return _IsPlaying; }
            set { RaisePcIfChanged(() => _IsPlaying, value, () => IsPlaying); }
        }

        public void Deselect() {
            IsPlaying = false;
        }

        public void Select() {
            IsPlaying = true;
        }

    
    }
}
