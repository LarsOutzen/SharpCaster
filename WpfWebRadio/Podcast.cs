using System;
using System.Collections.Generic;
using System.Xml;

namespace WpfWebRadio {
    public class Podcast {
        private List<Tuple<string, string, double>> Titles = new List<Tuple<string, string, double>>();

        public string ButtonTitle { get; set; }
        public string BaseUrl { get; set; }


        public string CurTitle { get; set; }
        public string CurUrl { get; set; }
        public object CurDuration { get; set; }

        public Podcast(string[] args) {
            BaseUrl = args[0];
            ButtonTitle = args[1];
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
            }
        }

        internal void PlayNewest() {
            CurTitle = Titles[0].Item1;
            CurUrl = Titles[0].Item2;
        }

        public override string ToString() {
            return BaseUrl;
        }
    }
}