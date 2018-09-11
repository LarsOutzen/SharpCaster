using Sharpcaster;
using Sharpcaster.Core.Channels;
using Sharpcaster.Core.Interfaces;
using Sharpcaster.Core.Models;
using Sharpcaster.Core.Models.ChromecastStatus;
using Sharpcaster.Core.Models.Media;
using Sharpcaster.Logging.ApplicationInsight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfCastAnalyzer {
   

    public partial class MainWindow : Window {

        private Dictionary<string, ChromecastClient> MyClients = new Dictionary<string, ChromecastClient>();
        private ChromecastViewModel CcViewModel { get { return this.SelectCastDevice?.SelectedItem as ChromecastViewModel; } }

        public MainWindow() {
            var logger = new ApplicationInsightLogger();
            InitializeComponent();
            var t = FindChromcasts();
        }

        private void RefreshDevices_Click(object sender, RoutedEventArgs e)
        {
            foreach (var cc in SelectCastDevice.Items)
            {
                (cc as ChromecastViewModel).Disconnect();
                (cc as ChromecastViewModel).Dispose();
            }
            this.SelectCastDevice.Items.Clear();
            var t = FindChromcasts();
        }

        private async Task FindChromcasts() {
            IChromecastLocator locator = new Sharpcaster.Discovery.MdnsChromecastLocator();
            var chromecasts = await locator.FindReceiversAsync();
            Dispatcher.Invoke(() => {
                foreach (var cc in chromecasts) {
                    var vm = new ChromecastViewModel(cc);
                    this.SelectCastDevice.Items.Add(vm);
                    if (cc.Name.Contains("Büro"))
                    {
                        this.SelectCastDevice.SelectedItem = vm;
                    }
                }
            });
        }

        private void VolumeCtrlLocal_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (CcViewModel != null)
            {
                var t = CcViewModel.SetVolume(e.NewValue / 100);
            }
        }



    
    }
}
