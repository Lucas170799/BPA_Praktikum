using BPA_Praktikum;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;

namespace Bpa_Praktikum
{
    internal class OpcUaClient
    {
        public int ClientRunTime { get; set; }
        public int ReconnectPeriod { get; set; }
        public string EndpointUrl { get; set; }
        public bool AutoAccept { get; set; }

        static ExitCode exitCode;

        public static ExitCode ExitCode { get => exitCode; }

        private SessionReconnectHandler ReconnectHandler { get; set; }
        private Session Session { get; set; }
        public TimeTracker TimeTracker { get; set; }


        public OpcUaClient(int clientRunTime, int reconnectPeriod, string endpointUrl, bool autoAccept)
        {
            //die Konstruktorparameter müssen auf die gegebenen Properties gemappt werden
            ClientRunTime = clientRunTime;
            ReconnectPeriod = reconnectPeriod;
            EndpointUrl = endpointUrl;
            AutoAccept = autoAccept;
            TimeTracker = new TimeTracker();
        }

        private async Task InitializeClient()
        {

            Session = await CreateSessionAsync();
            Session.KeepAlive += Client_KeepAlive;

            Console.WriteLine("Create a subscription with publishing interval of 1 second.");
            //Erstelle eine Instanz des Subscription Objekts mit einem Publishing Interval von 1000
            //Die Subscription soll die Defaultsubscription des Session Objektes sein
            var subscription = new Subscription(Session.DefaultSubscription) { PublishingInterval = 1000 };

            Console.WriteLine("Add a list of items (server current time and status) to the subscription.");
            //Erstelle eine Liste von Monitored Items, Jedes Item wird mit dem subscription.DefaultItem parameter instanziiert
            //und kann als Parameter DisplayName und StartNodeId besitzen.
            //Die StartNodeIds sind in der Sensor Klasse hinterlegt und können genutzt werden.
            //Überlege dir, welche Nodes für die Messung der Zeiten des Prozesses wichtig sind. 
            var subscriptionList = new List<MonitoredItem> {
                new MonitoredItem(subscription.DefaultItem)
                {
                    DisplayName = "Bg30", StartNodeId = Sensors.Bg30
                },new MonitoredItem(subscription.DefaultItem)
                {
                    DisplayName = "Bg31", StartNodeId = Sensors.Bg31
                }
            };
            //iteriere über die gesamte SubscriptionList
            //und füge dem Notification Property jedes Monitored Items den OnNotification Eventhandler hinzu
            subscriptionList.ForEach(i => i.Notification += OnNotification);
            //füge der subscription die subscriptionList hinzu
            subscription.AddItems(subscriptionList);

            Console.WriteLine("Add the subscription to the session.");
            //füge der Session die subscription hinzu
            Session.AddSubscription(subscription);
            subscription.Create();
            Console.WriteLine("Running...Press Ctrl-C to exit...");
        }

        private void OnNotification(MonitoredItem item, MonitoredItemNotificationEventArgs e)
        {
            //Überprüfe, welche Node sich ändert.
            //Messe anhand des Bg30 und Bg31 den kompletten Prozess und gib die Zeit aus.
            //Wenn Bg31 true --> TimeTracker.Start()
            //Wenn Bg30 true --> TimeTracker.Stop()
            
            foreach (var value in item.DequeueValues())
            {
                Console.WriteLine("{0}: {1}, {2}, {3}", item.DisplayName, value.Value, value.SourceTimestamp, value.StatusCode);
            
                if(item.StartNodeId.ToString() == Sensors.Bg31 && (bool)value.Value)
                {
                    Console.WriteLine($"Start: {TimeTracker.Start()}");
                }
                else if(item.StartNodeId.ToString() == Sensors.Bg30 && (bool)value.Value)
                {
                    Console.WriteLine($"End: {TimeTracker.Stop()}");
                    Console.WriteLine($"Difference: {TimeTracker.GetDifference()}");
                }
            }
        }
        #region Run
        public void Run()
        {
            try
            {
                InitializeClient().Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex.Message);
                return;
            }

            ManualResetEvent quitEvent = new ManualResetEvent(false);
            try
            {
                Console.CancelKeyPress += (sender, eArgs) =>
                {
                    quitEvent.Set();
                    eArgs.Cancel = true;
                };
            }
            catch
            {
            }

            // wait for timeout or Ctrl-C
            quitEvent.WaitOne(ClientRunTime);

            // return error conditions
            if (Session.KeepAliveStopped)
            {
                exitCode = ExitCode.ErrorNoKeepAlive;
                return;
            }

            exitCode = ExitCode.Ok;
        }
        #endregion
        #region Session
        private async Task<Session> CreateSessionAsync()
        {
            var instance = new ApplicationInstance
            {
                ApplicationType = Opc.Ua.ApplicationType.Client,
                ApplicationName = "Praktikum_BPA",
                ConfigSectionName = "config"
            };
            var config = await instance.LoadApplicationConfiguration(false);

            bool haveAppCertificate = await instance.CheckApplicationInstanceCertificate(false, 0);

            var selectedEndpoint = CoreClientUtils.SelectEndpoint(EndpointUrl, haveAppCertificate, 15000);
            Console.WriteLine("    Selected endpoint uses: {0}",
                selectedEndpoint.SecurityPolicyUri.Substring(selectedEndpoint.SecurityPolicyUri.LastIndexOf('#') + 1));

            Console.WriteLine("Create a session with OPC UA server.");
            var endpointConfiguration = EndpointConfiguration.Create(config);
            var endpoint = new ConfiguredEndpoint(null, selectedEndpoint, endpointConfiguration);
            return await Session.Create(config, endpoint, false, "OPC UA Console Client", 60000, new UserIdentity(new AnonymousIdentityToken()), null);

        }
        #endregion
        #region ReconnectHandler
        private void Client_KeepAlive(Session sender, KeepAliveEventArgs e)
        {
            if (e.Status != null && ServiceResult.IsNotGood(e.Status))
            {
                Console.WriteLine("{0} {1}/{2}", e.Status, sender.OutstandingRequestCount, sender.DefunctRequestCount);

                if (ReconnectHandler == null)
                {
                    Console.WriteLine("--- RECONNECTING ---");
                    ReconnectHandler = new SessionReconnectHandler();
                    ReconnectHandler.BeginReconnect(sender, ReconnectPeriod * 1000, callback: Client_ReconnectComplete);
                }
            }
        }

        private void Client_ReconnectComplete(object sender, EventArgs e)
        {
            // ignore callbacks from discarded objects.
            if (!Object.ReferenceEquals(sender, ReconnectHandler))
            {
                return;
            }

            Session = ReconnectHandler.Session;
            ReconnectHandler.Dispose();
            ReconnectHandler = null;

            Console.WriteLine("--- RECONNECTED ---");
        }
        #endregion
    }


}
