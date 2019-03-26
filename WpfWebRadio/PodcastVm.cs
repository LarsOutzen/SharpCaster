using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using MyMvvm;

namespace WpfWebRadio {
    public class PodcastVm :BaseViewModel, IPlaybackVm {
        private List<Tuple<string, string, double>> Titles = new List<Tuple<string, string, double>>();

        public string PodcastTitle { get; set; }
        public string BaseUrl { get; set; }

        
        public object CurDuration { get; set; }


        private string _SelectedTitle = null;
        public string SelectedTitle {
            get { return _SelectedTitle; }
            set { RaisePcIfChanged(() => _SelectedTitle, value, () => SelectedTitle); }
        }


        private List<string> _AllTitles = new List<string>();
        public List<string> AllTitles {
            get { return _AllTitles; }
            set { RaisePcIfChanged(() => _AllTitles, value, () => AllTitles); }
        }



        private string _Url = null;
        public string Url {
            get { return _Url; }
            set { RaisePcIfChanged(() => _Url, value, () => Url); }
        }

        private bool _IsPlaying = false;
        public bool IsPlaying {
            get { return _IsPlaying; }
            set { RaisePcIfChanged(() => _IsPlaying, value, () => IsPlaying); }
        }

        private string _Title = null;
        public string Title {
            get { return _Title; }
            set { RaisePcIfChanged(() => _Title, value, () => Title); }
        }


        public PodcastVm(string[] args) {
            BaseUrl = args[0];
            PodcastTitle = args[1];
            Title = PodcastTitle;
            // image = args[2];
        }

        public void LoadTitles() {
            //This loads the Podcast
            XmlDocument doc = new XmlDocument();
            doc.Load(BaseUrl);

            //This builds a list of the Item nodes
            XmlNodeList items = doc.SelectNodes("//item");

            //This loops through the list and writes out the title and URL.
            for(int i = 0; i < items.Count; i++) {
                Titles.Add(new Tuple<string, string, double>(
                    items[i].SelectSingleNode("title").InnerText,
                    items[i].SelectSingleNode("enclosure").Attributes["url"].Value,
                    0.0
                ));
                AllTitles.Add(items[i].SelectSingleNode("title").InnerText);
            }
            if (AllTitles.Count>0) {
                SelectedTitle = AllTitles[0];
            }
        }

        internal void PlaySelected() {
            if (SelectedTitle != null) {
                var t = Titles.Where(ti => ti.Item1 == SelectedTitle).FirstOrDefault();
                if (t != null) {
                    //Title = t.Item1;
                    Url = t.Item2;
                    IsPlaying = true;
                }
            }            
        }

        public override string ToString() {
            return BaseUrl;
        }

        public void Deselect() {
            Title = PodcastTitle;
            Url = null;
            IsPlaying = false;
        }

        public void Select(string mediaUrl) {
            if (!String.IsNullOrEmpty(mediaUrl)) {
                var t = Titles.Where(ti => ti.Item2 == mediaUrl).FirstOrDefault();
                if(t != null) {
                    SelectedTitle = t.Item1;
                }
            }    
            PlaySelected();
        }

        public bool HasUrl(string mediaUrl) {
            return Titles.Exists(t => (t.Item2 == mediaUrl));
        }
    }
}