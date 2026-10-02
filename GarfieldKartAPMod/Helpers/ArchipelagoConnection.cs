using Aube.Relays;
using UnityEngine;

namespace GarfieldKartAPMod.Helpers
{
    internal static class ArchipelagoConnection
    {
#if DEBUG
        public static string Host = "localhost";
        public static string Slot = "Jeff-GK";
#else
        public static string Host = "archipelago.gg";
        public static string Slot = "";
#endif
        public static string Port = "38281";
        public static string Password = "";

        private const string PopupTitle = "Archipelago Connection Lost";

        // Set from the socket thread, handled on the main thread in Update
        private static volatile bool disconnectPending;

        private static int reconnectFrame = -1;
        private static Relay reconnectLoadingRelay;

        public static void Initialize()
        {
            (string host, string port, string slot, string password) = FileWriter.ReadLastConnection();
            if (!string.IsNullOrEmpty(host)) Host = host;
            if (!string.IsNullOrEmpty(port)) Port = port;
            if (!string.IsNullOrEmpty(slot)) Slot = slot;
            if (!string.IsNullOrEmpty(password)) Password = password;

            GarfieldKartAPMod.APClient.OnDisconnected += () => disconnectPending = true;
        }

        // Returns null on success, otherwise the reason to show the player
        public static string TryConnect()
        {
            if (string.IsNullOrWhiteSpace(Host)) return "Please enter a host!";
            if (string.IsNullOrWhiteSpace(Slot)) return "Please enter a slot name!";
            if (!int.TryParse(Port, out int port) || port <= 0 || port > 65535) return "Invalid port number!";

            ArchipelagoClient client = GarfieldKartAPMod.APClient;
            client.Connect(Host.Trim(), port, Slot.Trim(), Password);
            if (!client.IsConnected) return client.LastConnectionError ?? "Could not connect.";

            FileWriter.WriteLastConnection(Host.Trim(), port, Slot.Trim(), Password);
            return null;
        }

        public static void Update()
        {
            GarfieldKartAPMod.APClient?.CheckConnection();

            if (disconnectPending)
            {
                disconnectPending = false;
                OnDisconnected();
            }

            // Run a frame after the loading popup opens so it's on screen while Connect blocks
            if (reconnectFrame >= 0 && Time.frameCount > reconnectFrame)
            {
                reconnectFrame = -1;
                FinishReconnect();
            }
        }

        private static void OnDisconnected()
        {
            if (GarfieldKartAPMod.APClient.IsConnected) return;
            ShowReconnectPrompt("Lost connection to Archipelago.");
        }

        private static void ShowReconnectPrompt(string reason)
        {
            PopupManager.OpenPopup($"{reason}\n\nReconnect to {Host}:{Port} as {Slot}?",
                PopupHD.POPUP_TYPE.YES_NO_QUESTION, PopupHD.POPUP_PRIORITY.NETWORK,
                closeCallback: OnReconnectPromptClosed, popupTitleLocKey: PopupTitle);
        }

        private static void OnReconnectPromptClosed(PopupHD.CLOSE_REASON reason)
        {
            if (reason != PopupHD.CLOSE_REASON.VALIDATE)
            {
                ReturnToTitle();
                return;
            }

            reconnectLoadingRelay = new Relay();
            PopupManager.OpenPopup("Reconnecting to Archipelago...", PopupHD.POPUP_TYPE.LOADING,
                PopupHD.POPUP_PRIORITY.NETWORK, reconnectLoadingRelay);
            reconnectFrame = Time.frameCount + 1;
        }

        private static void FinishReconnect()
        {
            string error = TryConnect();
            reconnectLoadingRelay?.Dispatch();
            reconnectLoadingRelay = null;

            if (error == null)
                ArchipelagoPopupManager.ShowInfo("Reconnected to Archipelago!");
            else
                ShowReconnectPrompt($"Reconnect failed: {error}");
        }

        // Mirrors MenuHDPause.OnSubmitQuit when mid-race, minus the bits only the pause menu owns
        private static void ReturnToTitle()
        {
            if (MenuManager.CurrentPage == PAGE.ENGAGEMENT_SCREEN)
            {
                ApConnectionPanel.Refresh();
                ApConnectionPanel.SetStatus("Disconnected from Archipelago.");
            }
            else if (Singleton<GameManager>.Instance.GameMode is InGameGameMode)
            {
                HUDInGameHD.RaceHUD?.ExitRace();
                TimeManager.Unpause();
                MenuManager.GoToMenuFromRace(PAGE.ENGAGEMENT_SCREEN);
            }
            else
            {
                MenuManager.OrderPreviousMenu(PAGE.ENGAGEMENT_SCREEN);
            }
        }
    }
}
