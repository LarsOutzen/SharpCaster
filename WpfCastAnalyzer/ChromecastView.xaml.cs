using Sharpcaster.Core.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace WpfCastAnalyzer
{
    /// <summary>
    /// Interaction logic for ChromecastView.xaml
    /// </summary>
    public partial class ChromecastView : UserControl
    {
        ChromecastViewModel MyViewModel { get { return DataContext as ChromecastViewModel; } }

        public ChromecastView()
        {
            InitializeComponent();
            this.SelectCastApp.Items.Add("CC32E753");   // Spotify 
            this.SelectCastApp.Items.Add("B3419EF5");   // Chromecaster
            this.SelectCastApp.SelectedItem = "B3419EF5"; //"CC32E753";
        }


        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            CallAsyncWithExceptionHandling(MyViewModel.Connect, this.SelectCastApp.SelectedItem.ToString());
        }
        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            CallAsyncWithExceptionHandling(MyViewModel.RefreshStatus);
        }
        private void BtnDisConnect_Click(object sender, RoutedEventArgs e)
        {
            MyViewModel.Disconnect();
        }

        private void ClrLog_Click(object sender, RoutedEventArgs e)
        {
            MyViewModel.MediaStati.Clear();
        }

        private void GetStatus_Click(object sender, RoutedEventArgs e)
        {
            CallAsyncWithExceptionHandling(MyViewModel.GetMediaStatusAsync);
        }

        private void LoadMedia_Click(object sender, RoutedEventArgs e)
        {
            Media m = new Media
            {
                ContentUrl = "http://mp3stream3.apasf.apa.at:8000/;",
                StreamType = StreamType.Live,
                ContentType = "audio/mp4"
            };
            CallAsyncWithExceptionHandling(MyViewModel.LoadMedia, m);
        }

        private void LoadMedia2_Click(object sender, RoutedEventArgs e)
        {
            Media m = new Media
            {
                ContentUrl = $"http://192.168.177.44:50002/m/MP3/{Track.Text}.mp3",
                StreamType = StreamType.Live,
                ContentType = "audio/mp4"
            };
            CallAsyncWithExceptionHandling(MyViewModel.LoadMedia, m);
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            CallAsyncWithExceptionHandling(MyViewModel.PauseAsync);
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            CallAsyncWithExceptionHandling(MyViewModel.PlayAsync);
        }
        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            CallAsyncWithExceptionHandling(MyViewModel.StopAsync);
        }


        private void CallAsyncWithExceptionHandling<T>(Func<Media, Task<T>> asyncMethod, Media m)
        {
            var t = asyncMethod(m);
            t.GetAwaiter().OnCompleted(() => {
                if (t.IsFaulted)
                {
                    DisplayException(t.Exception, $"Error Loading Media '{m.ContentUrl}'");
                }
            });
        }

        private void CallAsyncWithExceptionHandling(Func<string, Task> asyncMethod, string s)
        {
            var t = asyncMethod(s);
            t.GetAwaiter().OnCompleted(() => {
                if (t.IsFaulted)
                {
                    DisplayException(t.Exception, $"Error calling with '{s}'");
                }
            });
        }

        private void CallAsyncWithExceptionHandling<T>(Func<Task<T>> asyncMethod)
        {
            var t = asyncMethod();
            t.GetAwaiter().OnCompleted(() => {
                if (t.IsFaulted)
                {
                    DisplayException(t.Exception);
                }
            });
        }

        private void CallAsyncWithExceptionHandling(Func<Task> asyncMethod)
        {
            var t = asyncMethod();
            t.GetAwaiter().OnCompleted(() => {
                if (t.IsFaulted)
                {
                    DisplayException(t.Exception);
                }
            });
        }

        private void DisplayException(Exception ex, string message = "")
        {
            Dispatcher.Invoke(() => {
                string msg = GetExceptionMessage(ex);
                MessageBox.Show(msg, message);
            });
        }

        private string GetExceptionMessage(Exception ex)
        {
            return ex.Message + ( (ex.InnerException != null) ? Environment.NewLine + GetExceptionMessage(ex.InnerException) : "" );
        }
    }
}
