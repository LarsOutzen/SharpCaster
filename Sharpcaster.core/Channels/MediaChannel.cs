using Sharpcaster.Core.Interfaces;
using Sharpcaster.Core.Messages.Media;
using Sharpcaster.Core.Models.ChromecastStatus;
using Sharpcaster.Core.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharpcaster.Core.Channels
{
    /// <summary>
    /// Media channel
    /// </summary>
    public class MediaChannel : StatusChannel<MediaStatusMessage, IEnumerable<MediaStatus>>, IMediaChannel
    {
        /// <summary>
        /// Initializes a new instance of MediaChannel class
        /// </summary>
        public MediaChannel() : base("media")
        {
        }


        private async Task<MediaStatus> SendAsync(IMessageWithId message, ChromecastApplication application)
        {
            try
            {
                var s = await SendAsync<MediaStatusMessage>(message, application.TransportId);
                return s.Status?.FirstOrDefault();
            }
            catch (Exception e)
            {
                Status = null;
                throw e;
            }
        }

        private async Task<MediaStatus> SendAsync(MediaSessionMessage message)
        {
            var chromecastStatus = Client.GetChromecastStatus();
            if ((Status?.Count()??0) > 0) {
                var MediaStatus = Status.First();
                if (MediaStatus.IdleReason != "CANCELLED") {
                    message.MediaSessionId = Status?.First().MediaSessionId;
                }
            }
            if ((message.MediaSessionId != null) || (message.Type.Equals("GET_STATUS"))) {
                return await SendAsync(message, chromecastStatus.Applications[0]);
            }
            throw new ArgumentNullException("MediaSessionId");
        }

        /// <summary>
        /// Loads a media
        /// </summary>
        /// <param name="media">media to load</param>
        /// <param name="autoPlay">true to play the media directly, false otherwise</param>
        /// <returns>media status</returns>
        public async Task<MediaStatus> LoadAsync(Media media, bool autoPlay = true)
        {
            var status = Client.GetChromecastStatus();
            return await SendAsync(new LoadMessage() { SessionId = status.Applications[0].SessionId, Media = media, AutoPlay = autoPlay }, status.Applications[0]);
        }

        /// <summary>
        /// Plays the media
        /// </summary>
        /// <returns>media status</returns>
        public async Task<MediaStatus> PlayAsync()
        {
            return await SendAsync(new PlayMessage());
        }

        /// <summary>
        /// Pauses the media
        /// </summary>
        /// <returns>media status</returns>
        public async Task<MediaStatus> PauseAsync()
        {

            return await SendAsync(new PauseMessage());
        }

        /// <summary>
        /// Stops the media
        /// </summary>
        /// <returns>media status</returns>
        public async Task<MediaStatus> StopAsync()
        {
            return await SendAsync(new StopMessage());
        }

        /// <summary>
        /// Seeks to the specified time
        /// </summary>
        /// <param name="seconds">time in seconds</param>
        /// <returns>media status</returns>
        public async Task<MediaStatus> SeekAsync(double seconds)
        {
            return await SendAsync(new SeekMessage() { CurrentTime = seconds });
        }

        /// <summary>
        /// Seeks to the specified time
        /// </summary>
        /// <returns>media status</returns>
        public async Task<MediaStatus> GetStatusAsync() {
            return await SendAsync(new GetStatusMessage());
        }

    }
}
