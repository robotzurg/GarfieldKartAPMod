using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Packets;
using System;
using System.Collections.Generic;

namespace GarfieldKartAPMod
{
    public class ArchipelagoClient
    {
        private ArchipelagoSession session;
        private Queue<(long itemId, int playerId, string itemName, string playerName)> pendingNotifications = new Queue<(long, int, string, string)>();
        
        public bool IsConnected => session?.Socket.Connected ?? false;

        public event Action OnConnected;
        public event Action<string> OnConnectionFailed;
        public event Action OnDisconnected;

        public void Connect(string hostname, int port, string slotName, string password = "")
        {
            try
            {
                Log.Message($"Attempting to connect to {hostname}:{port} as {slotName}");

                session = ArchipelagoSessionFactory.CreateSession(hostname, port);

                session.Socket.ErrorReceived += OnError;
                session.Socket.SocketClosed += OnSocketClosed;

                LoginResult result = session.TryConnectAndLogin(
                    "Garfield Kart - Furious Racing",
                    slotName,
                    ItemsHandlingFlags.AllItems,
                    new Version(0, 6, 3),
                    password: string.IsNullOrEmpty(password) ? null : password
                );

                if (result.Successful)
                {
                    LoginSuccessful loginSuccess = (LoginSuccessful)result;
                    GarfieldKartAPMod.sessionSlotData = loginSuccess.SlotData;
                    Log.Message($"Connected successfully! Slot: {loginSuccess.Slot}");

                    // Subscribe to item received events
                    session.Items.ItemReceived += OnItemReceived;

                    // Load items we already have
                    ArchipelagoItemTracker.LoadFromServer();

                    OnConnected?.Invoke();
                }
                else
                {
                    LoginFailure failure = (LoginFailure)result;
                    string errorMsg = string.Join(", ", failure.Errors);
                    Log.Error($"Connection failed: {errorMsg}");
                    OnConnectionFailed?.Invoke(errorMsg);
                    session = null;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Connection exception: {ex.Message}");
                OnConnectionFailed?.Invoke(ex.Message);
                session = null;
            }
        }

        public ArchipelagoSession GetSession()
        {
            return session;
        }

        public void Disconnect()
        {
            if (session != null)
            {
                session.Socket.DisconnectAsync();
                session = null;
                Log.Message("Disconnected from Archipelago");
            }
        }

        private void OnItemReceived(ReceivedItemsHelper helper)
        {
            var item = helper.PeekItem();

            string itemName = session.Items.GetItemName(item.ItemId);
            string playerName = session.Players.GetPlayerName(item.Player);

            Log.Message($"Item Received: {itemName} from {playerName}");

            ArchipelagoItemTracker.AddReceivedItem(item.ItemId);

            pendingNotifications.Enqueue((item.ItemId, item.Player, itemName, playerName));

            helper.DequeueItem();
        }

        private void OnError(Exception ex, string message)
        {
            Log.Error($"Socket error: {message} - {ex.Message}");
        }

        private void OnSocketClosed(string reason)
        {
            Log.Warning($"Socket closed: {reason}");
            OnDisconnected?.Invoke();
        }

        public void SendLocation(long locationId)
        {
            if (IsConnected)
            {
                session.Locations.CompleteLocationChecks(locationId);
                ArchipelagoItemTracker.AddCheckedLocation(locationId);
                Log.Message($"Sent location check: {locationId}");

            }
        }

        public bool HasPendingNotifications()
        {
            return pendingNotifications.Count > 0;
        }

        public (long itemId, int playerId, string itemName, string playerName) DequeuePendingNotification()
        {
            return pendingNotifications.Dequeue();
        }
    }
}