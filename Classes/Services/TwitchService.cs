using Classes.APIModels.TwitchGQL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace TwitchKeyboard.Classes.Services
{
    public class TwitchService
    {
        /// <summary>
        /// GraphQL query for getting custom rewards from channel
        /// </summary>
        private readonly string gqlRewards = @"
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
        private readonly string clientId = "kimne78kx3ncx6brgo4mv6wki5h1ko";

        /// <summary>
        /// All custom rewards on current channel
        /// </summary>
        public CustomReward[] customRewards { get; set; } = Array.Empty<CustomReward>();

        /// <summary>
        /// TwitchLib client for chat
        /// </summary>
        private readonly TwitchClient client = new();

        /// <summary>
        /// Channel name
        /// </summary>
        public string channel { get; private set; } = null;

        /// <summary>
        /// Twitch connection state
        /// </summary>
        public TwitchConnectionState connectionState { get; private set; } = TwitchConnectionState.DISCONNECTED;

        public delegate void OnConnectionStateChangedHandler(object sender, TwitchConnectionState e);
        public event OnConnectionStateChangedHandler OnConnectionStateChanged;

        public delegate void OnMessageHandler(object sender, ChatMessage e);
        public event OnMessageHandler OnMessage;

        public delegate void OnRewardHandler(object sender, string rewardId, ChatMessage e);
        public event OnRewardHandler OnReward;

        public delegate void OnBitsHandler(object sender, ChatMessage e);
        public event OnBitsHandler OnBits;

        /// <summary>
        /// If true, client won't reconnect after disconnecting
        /// </summary>
        private bool manualDisconnect = false;

        public TwitchService()
        {
            Random rnd = new();
            client.Initialize(new ConnectionCredentials($"justinfan{rnd.Next(100, 9999)}", ""));

            client.OnConnected += Client_OnConnected;
            client.OnJoinedChannel += Client_OnJoinedChannel;
            client.OnConnectionError += Client_OnConnectionError;
            client.OnDisconnected += Client_OnDisconnected;

            client.OnMessageReceived += Client_OnMessageReceived;
        }

        private void Client_OnMessageReceived(object sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
        {
            if (e.ChatMessage.RawIrcMessage.Contains("custom-reward-id"))
            {

                int pFrom = e.ChatMessage.RawIrcMessage.IndexOf("custom-reward-id=") + "custom-reward-id=".Length;
                int pTo = e.ChatMessage.RawIrcMessage.LastIndexOf(";display-name");

                string rewardId = e.ChatMessage.RawIrcMessage[pFrom..pTo];

                OnReward?.Invoke(this, rewardId, e.ChatMessage);
            }
            else if (e.ChatMessage.Bits > 0)
            {
                OnBits?.Invoke(this, e.ChatMessage);
            }
            else
            {
                OnMessage?.Invoke(this, e.ChatMessage);
            }
        }

        private void Client_OnDisconnected(object sender, TwitchLib.Communication.Events.OnDisconnectedEventArgs e)
        {
            connectionState = TwitchConnectionState.DISCONNECTED;
            OnConnectionStateChanged(this, connectionState);

            if (manualDisconnect)
            {
                manualDisconnect = false;
                return;
            }

            client.Reconnect();
        }

        private void Client_OnConnectionError(object sender, TwitchLib.Client.Events.OnConnectionErrorArgs e)
        {
            connectionState = TwitchConnectionState.ERROR;
            OnConnectionStateChanged(this, connectionState);
        }

        private void Client_OnJoinedChannel(object sender, TwitchLib.Client.Events.OnJoinedChannelArgs e)
        {
            connectionState = TwitchConnectionState.JOINED;
            OnConnectionStateChanged(this, connectionState);
        }

        private void Client_OnConnected(object sender, TwitchLib.Client.Events.OnConnectedArgs e)
        {
            connectionState = TwitchConnectionState.CONNECTED;
            OnConnectionStateChanged(this, connectionState);

            try
            {
                client.JoinChannel(channel);
            }
            catch
            {
                connectionState = TwitchConnectionState.ERROR;
                OnConnectionStateChanged(this, connectionState);
            }
        }

        /// <summary>
        /// Connect to the chat and load all channel rewards
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public async Task JoinChannel(string channel)
        {
            HttpClient http = new();
            string request = gqlRewards.Replace("CHANNEL", channel).Replace("\r\n", "").Replace(" ", "");
            var content = new StringContent(request, Encoding.UTF8, "application/json");
            http.Timeout = new TimeSpan(0, 0, 5);
            content.Headers.Add("Client-id", clientId);
            var response = await http.PostAsync("https://gql.twitch.tv/gql", content);
            string res = await response.Content.ReadAsStringAsync();
            customRewards = JsonConvert.DeserializeObject<RewardsRoot[]>(res)[0].data.community.channel.communityPointsSettings.customRewards;

            try
            {
                this.channel = channel;
                client.Connect();
            }
            catch
            {
                connectionState = TwitchConnectionState.ERROR;
                OnConnectionStateChanged(this, connectionState);
            }
        }

        public void Disconnect()
        {
            manualDisconnect = true;
            client.Disconnect();
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
