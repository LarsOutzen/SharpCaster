using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfWebRadio {
    public interface IPlaybackVm {
        string Url { get; set; }

        void Deselect();
        void Select(string mediaUrl);
        bool HasUrl(string mediaUrl);
    }
}
