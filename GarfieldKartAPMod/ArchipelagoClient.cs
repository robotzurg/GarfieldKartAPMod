using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Packets;
using System;

namespace GarfieldKartAPMod
{
    public class ArchipelagoClient
    {
        private ArchipelagoSession session;
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

            Log.Message($"Item Received: {itemName}");

            // Add to tracker
            ArchipelagoItemTracker.AddReceivedItem(itemName);

            // Override unlock state
            UnlockItemFromArchipelago(itemName);

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
                Log.Message($"Sent location check: {locationId}");
            }
        }

        public void SendLocationFromName(string locationName)
        {
            if (IsConnected)
            {
                long id = session.Locations.GetLocationIdFromName("Garfield Kart - Furious Racing", locationName);
                session.Locations.CompleteLocationChecks(id);
                Log.Message($"Sent location check: {id} - {locationName}");
            }
        }

        private void UnlockItemFromArchipelago(string itemName)
        {
            // Map Archipelago item names to game IDs
            string gameId = MapItemNameToGameId(itemName);

            if (!string.IsNullOrEmpty(gameId))
            {
                ArchipelagoUnlockOverride.UnlockItem(gameId);
                Log.Message($"Unlocked via AP override: {gameId}");
            }
        }

        private string MapItemNameToGameId(string apItemName)
        {
            // Map AP item names to game internal IDs
            return apItemName switch
            {
                "Lasagna Cup" => "Cup_Lasagna",
                "Pizza Cup" => "Cup_Pizza",
                "Burger Cup" => "Cup_Burger",
                "Ice Cream Cup" => "Cup_IceCream",

                "Arlene Kart" => "Kart_Arlene",
                "Harry Kart" => "Kart_Harry",
                "Lyman Kart" => "Kart_Lyman",
                // etc...

                _ => null
            };
        }
    }
}