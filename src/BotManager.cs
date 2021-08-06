using Microsoft.CognitiveServices.Speech;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TwitchLib.Api.V5;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace fermiac
{
    public class BotManager : IDisposable
    {
        BotState state;
        TwitchClient client;
        SpeechConfig speechConf;
        TimedQueue<Tuple<string, string>> chatQueue;
        Queue<actions.Action> actionQueue;
        List<(DateTime, BotTrigger)> RATL;
        List<string> chatters;
        Random r;
        bool keepTalking;

        public Action<LogMsg> logger;
        
        public delegate void FermiacTalkStartEvent();
        public delegate void FermiacTalkEndEvent();
        public event FermiacTalkStartEvent FermiacTalkStart;
        public event FermiacTalkEndEvent FermiacTalkEnd;

        private void log(string type, string message)
        {
            if(logger != null) {
                logger.Invoke(new LogMsg() { type = type, msg = message });
            }
        }

        private void actionLoop()
        {
            while (keepTalking)
            {
                // check on random actions
                var todo = RATL?.Where(rx => rx.Item1 < DateTime.Now).ToList();
                foreach(var t in todo) {
                    RATL.Remove(t);
                    if(string.IsNullOrEmpty(t.Item2.triggerOn)) {
                        // only auto-requeue when no specific trigger defined
                        actionQueue.Enqueue(t.Item2.ToAction());
                    }
                    enqueueTrigger(t.Item2); 
                }
                if (actionQueue.Count > 0)
                {
                    var act = actionQueue.Dequeue();
                    log("trace", $"firing action [{act.Name}]");
                    act.Enact(this);
                }
                Thread.Sleep(50);
            }
        }

        public BotManager()
        {
            chatQueue = new TimedQueue<Tuple<string, string>>();
            actionQueue = new Queue<actions.Action>();
            chatters = new List<string>();
            RATL = new List<(DateTime, BotTrigger)>();
            keepTalking = true;
            Task.Run(() => chatLoop());
            Task.Run(() => actionLoop());
            r = new Random(Convert.ToInt32(DateTime.Now.Ticks % int.MaxValue));
        }

        private void loadState()
        {
            if (System.IO.File.Exists("state.json"))
            {
                var bs = System.IO.File.ReadAllText("state.json");
                state = Newtonsoft.Json.JsonConvert.DeserializeObject<BotState>(bs);
                foreach(var bt in state.triggers.Where(tx => tx.frequency.ToLower() == "random")) {
                    enqueueTrigger(bt);
                }
            }
            else
            {
                state = new BotState();
                saveState();
            }
        }

        private void saveState()
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(state);
            System.IO.File.WriteAllText("state.json", json);
        }
        public void connect(string username, string accessToken, string channel, string speechKey, string speechRegion)
        {
            ConnectionCredentials credentials = new ConnectionCredentials(username, accessToken);
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            WebSocketClient customClient = new WebSocketClient(clientOptions);
            client = new TwitchClient(customClient);
            client.Initialize(credentials, channel);

            log("info", $"connected to twitch chat client");

            var tapi = new TwitchLib.Api.TwitchAPI();
            tapi.Settings.ClientId = username;
            tapi.Settings.AccessToken = accessToken;

            var tlp = new TwitchLib.PubSub.TwitchPubSub(null);
            tlp.ListenToRewards(channel);
            tlp.Connect();
            
            log("info", $"connected to twitch pubsub");
            
            client.OnLog += Client_OnLog;
            client.OnJoinedChannel += Client_OnJoinedChannel;
            client.OnMessageReceived += Client_OnMessageReceived;
            client.OnWhisperReceived += Client_OnWhisperReceived;
            client.OnNewSubscriber += Client_OnNewSubscriber;
            client.OnConnected += Client_OnConnected;
            client.OnRaidNotification += Client_OnRaidNotification;
            client.OnReSubscriber += Client_OnReSubscriber;
            client.OnGiftedSubscription += Client_OnGiftedSubscription;
            client.Connect();

            speechConf = SpeechConfig.FromSubscription(speechKey, speechRegion);
            loadState();

        }

        #region triggers
        private void enqueueTrigger(BotTrigger bt)
        {
            try {
                switch(bt.frequency.ToLower())
                {
                    case "random":
                        {
                            var next = r.Next((Int32)bt.options["randomedges"][0].Value, (Int32)bt.options["randomedges"][1].Value);
                            var nextTrigger = DateTime.Now.AddSeconds(next);
                            RATL.Add((nextTrigger, bt));
                            log("info", $"enqueued random action {bt.action} for {next} seconds in the future");
                            break;
                        }
                    default:
                        {
                            // treat as chat action
                            actionQueue.Enqueue(bt.options.ToAction());
                            if (bt.frequency.ToLower() == "once") bt.fired = true;
                            saveState();
                            log("info", $"enqueued random action {bt.action}");
                            break;
                        }
                }
            } catch (Exception ex) {
                log("err", $"exception triggering {bt.action}, {ex.ToString()}");
            }
        }

        #endregion

        #region twitch client events
        private void Client_OnReSubscriber(object sender, OnReSubscriberArgs e)
        {
            Speak(e.ReSubscriber.DisplayName, "analogcomputer thanks you for enabling his video game habit", 0);
        }

        private void Client_OnRaidNotification(object sender, OnRaidNotificationArgs e)
        {
            Speak("announcement", string.Format("raid by {0} in progress, hold on to your britches!", e.RaidNotification.DisplayName), 0);
        }

        private void Client_OnGiftedSubscription(object sender, OnGiftedSubscriptionArgs e)
        {
            if(e.GiftedSubscription.IsAnonymous) {
                Speak("announcement", string.Format("analog's mysterious benefactor has gifted a sub to {0}", e.GiftedSubscription.MsgParamRecipientDisplayName), 0);
            } else {
                Speak("announcement", string.Format("analog thanks {1} for gifting a sub to {0}", e.GiftedSubscription.MsgParamRecipientDisplayName, e.GiftedSubscription.DisplayName), 0);
            }
        }

        private void Client_OnLog(object sender, OnLogArgs e)
        {
            Debug.WriteLine($"{e.DateTime.ToString()}: {e.BotUsername} - {e.Data}");
        }

        private void Client_OnConnected(object sender, OnConnectedArgs e)
        {
            Debug.WriteLine($"Connected to {e.AutoJoinChannel}");
        }

        private void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            Speak("announcement", "The bot is online", 0);
        }

        private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            // filter emotes
            var elist = e.ChatMessage.EmoteSet.Emotes.Select(x => x.Name);
            var msg = e.ChatMessage.Message;
            foreach (var em in elist)
            {
                msg = msg.Replace(em, "");
            }
            if (!msg.StartsWith('!'))
            {
                Speak(e.ChatMessage.Username, msg, 0);
            } else
            {
                // treat as bang command
                var segments = msg.Split(' ');
                var cmd = segments[0].Replace("!", "");
                var commands = state.triggers.TriggerOn("bang")
                                             .TriggerFor(cmd.ToLower())
                                             .NotFired();
                foreach(var c in commands) {
                    actionQueue.Enqueue(c.ToAction());
                }
            }
            if(e.ChatMessage.Bits > 0) {
                Speak("fermiac", $"thanks for the {e.ChatMessage.Bits} bits {e.ChatMessage.Username}!", 0);                
            }
            if (!chatters.Contains(e.ChatMessage.Username))
            {
                chatters.Add(e.ChatMessage.Username);
                var thingsToDo = state.triggers.TriggerOn("firstchat")
                                               .TriggerFor(e.ChatMessage.Username)
                                               .NotFired();
                foreach (var thing in thingsToDo)
                {
                    actionQueue.Enqueue(thing.ToAction());
                    if (thing.frequency.ToLower() == "once") thing.fired = true;
                    saveState();
                }
            }
        }

        private void Client_OnWhisperReceived(object sender, OnWhisperReceivedArgs e)
        {
            //if (e.WhisperMessage.Username == "my_friend")
            //client.SendWhisper(e.WhisperMessage.Username, "Hey! Whispers are so cool!!");
        }

        private void Client_OnNewSubscriber(object sender, OnNewSubscriberArgs e)
        {
            Speak(e.Subscriber.DisplayName, "analogcomputer thanks you for enabling his video game habit", 0);
        }

        #endregion

        #region azure tts
        private string ssmlTemplate
        {
            get
            {
                return ("<speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xml:lang=\"en-US\"><voice name=\"{0}\">{2}. {1}</voice></speak>");
            }
        }

        private string filter(string input)
        {
            var rgx = new Regex(@"^\w+!(\w.*)");
            var cheers = new string[] { "cheer", "uni", "pogchamp", "cheerwal", "corgo", "showlove", "party", "seemsgood", "pride", "kappa", "frankerz", "heyguys", "dansgame", "elegiggle", "trihard", "kreygasm", "swiftrage", "notlikethis", "failfish", "vohiyo", "pjsalt", "mrdestructoid", "bday", "ripcheer", "shamrock" };
            foreach (var c in cheers)
            {
                var rgl = new Regex(@"\W*((?i)" + c + @"(?-i))\d+");
                input = rgl.Replace(input, "bits");
            }
            return (rgx.Replace(input, ""));
        }

        private string map(string user)
        {
            switch (user.ToLower())
            {
                case "analogcomputer": return ("en-US-ZiraRUS");
                case "announcement": return ("en-AU-HayleyRUS");
                case "fermiac": return ("en-GB-George-Apollo");
                default: return ("en-US-Guy24kRUS");
            }
        }

        public void Speak(string user, string message, int delayMS)
        {
            chatQueue.Enqueue(new Tuple<string, string>(user, message), delayMS);
        }
        
        public void Text(string channel, string message) {
            client.SendMessage("analogcomputer", message);
        }

        private void chatLoop()
        {
            while (keepTalking)
            {
                try
                {
                    if (chatQueue.CanDequeue)
                    {
                        var msgT = chatQueue.Dequeue();
                        if (!msgT.Item2.ToLower().StartsWith("/me")) // skip actions
                        {
                            var user = msgT.Item1;
                            var message = filter(msgT.Item2);
                            var voice = map(user);
                            if (!string.IsNullOrEmpty(message))
                            {
                                var msg = string.Format(ssmlTemplate, voice, message, user);
                                if (user.ToLower() == "fermiac") Text("analogcomputer", message);
                                using (var synthesizer = new SpeechSynthesizer(speechConf))
                                {
                                    SpeechSynthesisResult result = null;
                                    var t = Task.Run(async () =>
                                    {
                                        if (user.ToLower() == "fermiac") FermiacTalkStart?.Invoke();
                                        result = await synthesizer.SpeakSsmlAsync(msg);
                                        if (user.ToLower() == "fermiac") FermiacTalkEnd?.Invoke();
                                    });
                                    t.Wait();
                                    if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                                    {
                                        log("chat", $"user [{user}] chatted [{message}]");
                                    }
                                    else if (result.Reason == ResultReason.Canceled)
                                    {
                                        var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                                        log("err", $"CANCELED: Reason={cancellation.Reason}");

                                        if (cancellation.Reason == CancellationReason.Error)
                                        {
                                            log("err", $"CANCELED: ErrorCode={cancellation.ErrorCode}");
                                            log("err", $"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]");
                                        }
                                    }
                                    result.Dispose();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log("err", $"exception in chat loop, {ex.ToString()}");
                }
                Thread.Sleep(100);
            }
        }
        #endregion

        public void Dispose()
        {
            keepTalking = false;
            client.Disconnect();
        }
    }
}