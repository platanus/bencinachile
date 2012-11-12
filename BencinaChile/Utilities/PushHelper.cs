using Microsoft.Phone.Notification;
using System;
using System.Net;
using System.Text;

namespace BencinaChile.Utilities
{
    public class PushHelper
    {
        #region Public Events

        public delegate void UriUpdatedEventHandler(string uri);

        public delegate void ErrorEventHandler(NotificationChannelErrorEventArgs e);

        public event ErrorEventHandler Error;

        public delegate void RawNotificationReceivedEventHandler(string data);

        public event RawNotificationReceivedEventHandler RawNotificationReceived;

        #endregion

        #region Public Methods

        public void RegisterPushNotifications()
        {
            if (pushChannel != null) return;

            pushChannel = HttpNotificationChannel.Find(channelName);

            // If the channel was not found, then create a new connection to the push service.
            if (pushChannel == null)
            {
                pushChannel = new HttpNotificationChannel(channelName, "PositiveSSL CA");

                // Register for all the events before attempting to open the channel.
                pushChannel.ChannelUriUpdated += PushChannel_ChannelUriUpdated;
                pushChannel.ErrorOccurred += PushChannel_ErrorOccurred;
                pushChannel.HttpNotificationReceived += PushChannel_HttpNotificationReceived;

                pushChannel.Open();
                pushChannel.BindToShellToast();
                pushChannel.BindToShellTile();

                System.Diagnostics.Debug.WriteLine("Connetion: " + pushChannel.ConnectionStatus.ToString());
                System.Diagnostics.Debug.WriteLine("Bound: " + pushChannel.IsShellTileBound.ToString());
            }
            else
            {


                System.Diagnostics.Debug.WriteLine("Connetion2: " + pushChannel.ConnectionStatus.ToString());
                System.Diagnostics.Debug.WriteLine("Bound2: " + pushChannel.IsShellTileBound.ToString());
                
                // The channel was already open, so just register for all the events.
                pushChannel.ChannelUriUpdated += PushChannel_ChannelUriUpdated;
                pushChannel.ErrorOccurred += PushChannel_ErrorOccurred;
                pushChannel.HttpNotificationReceived += PushChannel_HttpNotificationReceived;
            }

            //if (UriUpdated != null && pushChannel.ChannelUri != null)
            //{
            //    UriUpdated(pushChannel.ChannelUri.ToString());
            //}
        }

        public void CloseChannel()
        {
            pushChannel.Close();
            pushChannel = null;
        }

        #endregion

        #region Public Properties

        public string PushChannelUri
        {
            get
            {
                if (pushChannel.ChannelUri == null)
                {
                    return null;
                }
                else
                {
                    return pushChannel.ChannelUri.ToString();
                }
            }
        }

        #endregion

        #region Private Fields

        /// Holds the push channel that is created or found.
        private static HttpNotificationChannel pushChannel;

        private const string channelName = "BencinaChileChannel";

        #endregion

        #region Private Methods

        private void PushChannel_ChannelUriUpdated(object sender, NotificationChannelUriEventArgs e)
        {
            //App.Current.Couch.RegisterPushKey(e.ChannelUri);
            // Save e.ChannelUri
            var url = "http://192.168.50.2:3500/api/windows_notification?developer_key=1";

            WebClient webClient = new WebClient();
            webClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            var uri = new Uri(url, UriKind.Absolute);
            StringBuilder postData = new StringBuilder();
            postData.AppendFormat("{0}={1}", "latitude", "-71.434235");
            postData.AppendFormat("&{0}={1}", "longitude", "30.5435435");
            postData.AppendFormat("&{0}={1}", "gasoline_type", "g95");
            postData.AppendFormat("&{0}={1}", "address", "Vitacura, santiago");
            postData.AppendFormat("&{0}={1}", "endpoint", e.ChannelUri.ToString());

            webClient.Headers[HttpRequestHeader.ContentLength] = postData.Length.ToString();
            //webClient.UploadStringCompleted += new UploadStringCompletedEventHandler(webClient_UploadStringCompleted);
            //webClient.UploadProgressChanged += webClient_UploadProgressChanged;
            webClient.UploadStringAsync(uri, "POST", postData.ToString());  

            System.Diagnostics.Debug.WriteLine("Changed to:" + e.ChannelUri.ToString());
        }

        private void PushChannel_ErrorOccurred(object sender, NotificationChannelErrorEventArgs e)
        {
            // Error handling logic for your particular application would be here.
            if (Error != null)
            {
                Error(e);
            }
        }

        private void PushChannel_HttpNotificationReceived(object sender, HttpNotificationEventArgs e)
        {
            using (var reader = new System.IO.StreamReader(e.Notification.Body))
            {
                var data = reader.ReadToEnd();

                if (RawNotificationReceived != null)
                {
                    RawNotificationReceived(data);
                }
            }
        }

        #endregion
    }
}