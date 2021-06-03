using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TwitchGQL;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace TwitchKeyboard
{
    public class TwitchController
    {
        /// <summary>
        /// GraphQL query for getting custom rewards from channel
        /// </summary>
        private string gqlRewards = @"
        [
            {
            ""operationName"": ""ChannelPointsContext"",
            ""variables"": {
                    ""channelLogin"": ""CHANNEL"",
                ""includeGoalTypes"": [
                    ""CREATOR"",
                    ""BOOST""
                ]
            },
            ""extensions"": {
                    ""persistedQuery"": {
                        ""version"": 1,
                    ""sha256Hash"": ""1530a003a7d374b0380b79db0be0534f30ff46e61cffa2bc0e2468a909fbc024""
                    }
                }
            }
        ]";

        /// <summary>
        /// Twitch site client id
        /// </summary>
        private string clientId = "kimne78kx3ncx6brgo4mv6wki5h1ko";

        /// <summary>
        /// All custom rewards on current channel
        /// </summary>
        public List<CustomReward> customRewards { get; set; } = new();

        /// <summary>
        /// TwitchLib client for chat
        /// </summary>
        private TwitchClient client = new TwitchClient();

        /// <summary>
        /// Channel name
        /// </summary>
        private string channel = null;

        /// <summary>
        /// Twitch connection state
        /// </summary>
        public TwitchConnectionState connectionState{ get; private set; } = TwitchConnectionState.DISCONNECTED;

        public delegate void OnConnectionStateChangedHandler(object sender, TwitchConnectionState e);
        public event OnConnectionStateChangedHandler OnConnectionStateChanged;

        public delegate void OnMessageHandler(object sender, ChatMessage e);
        public event OnMessageHandler OnMessage;

        public delegate void OnRewardHandler(object sender, string rewardId, ChatMessage e);
        public event OnRewardHandler OnReward;

        /// <summary>
        /// If true, client won't reconnect after disconnecting
        /// </summary>
        private bool manualDisconnect = false;

        public TwitchController()
        {
            Random rnd = new();
            this.client.Initialize(new TwitchLib.Client.Models.ConnectionCredentials($"justinfan{rnd.Next(100, 9999)}", ""));

            this.client.OnConnected += Client_OnConnected;
            this.client.OnJoinedChannel += Client_OnJoinedChannel;
            this.client.OnConnectionError += Client_OnConnectionError;
            this.client.OnDisconnected += Client_OnDisconnected;
            this.client.OnMessageReceived += Client_OnMessageReceived;
        }

        private void Client_OnMessageReceived(object sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
        {
            if (e.ChatMessage.RawIrcMessage.Contains("custom-reward-id"))
            {

                int pFrom = e.ChatMessage.RawIrcMessage.IndexOf("custom-reward-id=") + "custom-reward-id=".Length;
                int pTo = e.ChatMessage.RawIrcMessage.LastIndexOf(";display-name");

                string rewardId = e.ChatMessage.RawIrcMessage.Substring(pFrom, pTo - pFrom);

                this.OnReward?.Invoke(this, rewardId, e.ChatMessage);
            }
            else
            {
                this.OnMessage?.Invoke(this, e.ChatMessage);
            }
        }

        private void Client_OnDisconnected(object sender, TwitchLib.Communication.Events.OnDisconnectedEventArgs e)
        {
            this.connectionState = TwitchConnectionState.DISCONNECTED;
            this.OnConnectionStateChanged(this, this.connectionState);

            if (this.manualDisconnect)
            {
                this.manualDisconnect = false;
                return;
            }

            this.client.Reconnect();
        }

        private void Client_OnConnectionError(object sender, TwitchLib.Client.Events.OnConnectionErrorArgs e)
        {
            this.connectionState = TwitchConnectionState.ERROR;
            this.OnConnectionStateChanged(this, this.connectionState);
        }

        private void Client_OnJoinedChannel(object sender, TwitchLib.Client.Events.OnJoinedChannelArgs e)
        {
            this.connectionState = TwitchConnectionState.JOINED;
            this.OnConnectionStateChanged(this, this.connectionState);
        }

        private void Client_OnConnected(object sender, TwitchLib.Client.Events.OnConnectedArgs e)
        {
            this.connectionState = TwitchConnectionState.CONNECTED;
            this.OnConnectionStateChanged(this, this.connectionState);

            try
            {
                this.client.JoinChannel(channel);
            }
            catch
            {
                this.connectionState = TwitchConnectionState.ERROR;
                this.OnConnectionStateChanged(this, this.connectionState);
            }
        }

        /// <summary>
        /// Connect to the chat and load all channel rewards
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public async Task JoinChannel(string channel)
        {
            HttpClient http = new HttpClient();
            string request = gqlRewards.Replace("CHANNEL", channel).Replace("\r\n", "").Replace(" ", "");
            var content = new StringContent(request, Encoding.UTF8, "application/json");
            http.Timeout = new TimeSpan(0, 0, 5);
            content.Headers.Add("Client-id", this.clientId);
            var response = await http.PostAsync("https://gql.twitch.tv/gql", content);
            string res = await response.Content.ReadAsStringAsync();
            this.customRewards = JsonConvert.DeserializeObject<List<RewardsRoot>>(res)[0].data.community.channel.communityPointsSettings.customRewards;

            try
            {
                this.channel = channel;
                this.client.Connect();
            }
            catch
            {
                this.connectionState = TwitchConnectionState.ERROR;
                this.OnConnectionStateChanged(this, this.connectionState);
            }
        }

        public void Disconnect ()
        {
            this.manualDisconnect = true;
            this.client.Disconnect();
        }
    }

    public enum TwitchConnectionState
    {
        DISCONNECTED = 0,
        IN_PROGRESS,
        CONNECTED,
        JOINED,
        ERROR
    }
}
