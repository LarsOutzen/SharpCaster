using Sharpcaster.Core.Interfaces;
using System;
using Sharpcaster.Core.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tmds.MDns;
using System.Linq;

namespace Sharpcaster.Discovery
{
    /// <summary>
    /// Find the available chromecast receivers using mDNS protocol
    /// </summary>
    public class MdnsChromecastLocator : IChromecastLocator, IDisposable
    {
		private readonly SemaphoreSlim _serviceAddedSemaphore = new SemaphoreSlim(1, 1);
		private readonly List<ChromecastReceiver> _discoveredDevices = new List<ChromecastReceiver>();
        private readonly ServiceBrowser _serviceBrowser;

        public MdnsChromecastLocator()
        {
            _serviceBrowser = new ServiceBrowser();
            _serviceBrowser.ServiceAdded += OnServiceAdded;
  
        }

        public event EventHandler<ChromecastReceiver> ChromecastReceivedFound;


        private void OnServiceAdded(object sender, ServiceAnnouncementEventArgs e)
        {
            _serviceAddedSemaphore.Wait();
            try
            {
                var txtValues = e.Announcement.Txt
                    .Select(i => i.Split('='))
                    .ToDictionary(y => y[0], y => y[1]);
                if (!txtValues.ContainsKey("fn")) return;
                var ip = e.Announcement.Addresses[0];
                Uri.TryCreate("https://" + ip, UriKind.Absolute, out Uri myUri);
                var chromecast = new ChromecastReceiver
                {
                    DeviceUri = myUri,
                    Name = txtValues["fn"],
                    Model = txtValues["md"],
                    Version = txtValues["ve"],
                    ExtraInformation = txtValues,
                    Status = txtValues["rs"],
                    Port = e.Announcement.Port
                };
                ChromecastReceivedFound?.Invoke(this, chromecast);
                _discoveredDevices.Add(chromecast);
            }
            finally
            {
                _serviceAddedSemaphore.Release();
            }
        }

        /// <summary>
        /// Find the available chromecast receivers
        /// </summary>
        public Task<IEnumerable<ChromecastReceiver>> FindReceiversAsync()
        {
            var cts = new CancellationTokenSource(2000);

            return FindReceiversAsync(cts.Token);
        }

        /// <summary>
        /// Find the available chromecast receivers
        /// </summary>
        /// <typeparam name="cancellationToken">Enable to cancel the operation before timeout</typeparam>
        /// <typeparam name="timeOut">Define custom timeout when required, default is 2000 ms</typeparam>
        /// <returns>a collection of chromecast receivers</returns>
        public async Task<IEnumerable<ChromecastReceiver>> FindReceiversAsync(CancellationToken cancellationToken)
        {
            _discoveredDevices.Clear();

            if (_serviceBrowser.IsBrowsing)
                _serviceBrowser.StopBrowse();
            
            _serviceBrowser.StartBrowse("_googlecast._tcp");

            while (!cancellationToken.IsCancellationRequested)
                await Task.Delay(100).ConfigureAwait(false);
            
            _serviceBrowser.StopBrowse();
            return _discoveredDevices;
        }

        #region IDisposable Support
        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _serviceBrowser.ServiceAdded -= OnServiceAdded;

            _disposed = true;
        }

        public void Dispose() => Dispose(true);
        #endregion
    }
}
