using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using GarfieldKartAPMod.Helpers;
using UnityEngine;

namespace GarfieldKartAPMod
{
    public static class DeathLinkManager
    {
        private static DeathLinkService service;
        
        private static volatile bool deathPending;
        private static string pendingSource;
        private static string pendingCause;

        // Suppress outgoing DeathLinks briefly after applying a received one, so a
        // forced respawn can never send a death back
        private static float suppressSendUntil;

        public static void OnSessionConnected(ArchipelagoSession session)
        {
            service = session.CreateDeathLinkService();
            service.OnDeathLinkReceived += OnDeathLinkReceived;
            ApplyConfig();
        }

        public static void OnDisconnected()
        {
            service = null;
            deathPending = false;
        }
        
        public static void ApplyConfig()
        {
            if (service == null) return;

            if (ArchipelagoHelper.IsDeathLinkEnabled())
            {
                service.EnableDeathLink();
                Log.Message("DeathLink enabled");
            }
            else
            {
                service.DisableDeathLink();
                Log.Message("DeathLink disabled");
            }
        }

        // Socket thread: only record the death, Unity APIs are main-thread only
        private static void OnDeathLinkReceived(DeathLink deathLink)
        {
            pendingSource = deathLink.Source;
            pendingCause = deathLink.Cause;
            deathPending = true;
        }

        // Called every frame from the plugin's Update loop
        public static void ProcessPendingDeath()
        {
            if (!deathPending) return;
            deathPending = false;

            if (!ArchipelagoHelper.IsDeathLinkEnabled()) return;

            Kart kart = FindLocalPlayerKart();
            if (kart == null)
            {
                Log.Message($"DeathLink from {pendingSource} ignored (not in a race)");
                return;
            }

            string reason = string.IsNullOrEmpty(pendingCause) ? $"DeathLink from {pendingSource}" : pendingCause;
            Log.Message($"DeathLink received: {reason}");
            GarfieldKartAPMod.APClient?.QueueNotification($"DeathLink: {reason}");

            suppressSendUntil = Time.realtimeSinceStartup + 3f;
            kart.ForceRespawn();
        }
        
        public static void OnLocalPlayerFell()
        {
            if (Time.realtimeSinceStartup < suppressSendUntil) return;
            SendDeath($"{SlotName()} fell off the track");
        }

        // Debug helper for sending
        public static void SendDebugDeath()
        {
            SendDeath($"{SlotName()} pressed the death button");
        }

        // Debug helper for receiving
        public static void SimulateReceivedDeath()
        {
            pendingSource = "Debug";
            pendingCause = "Simulated DeathLink";
            deathPending = true;
        }

        private static void SendDeath(string cause)
        {
            if (service == null || !ArchipelagoHelper.IsDeathLinkEnabled()) return;

            try
            {
                service.SendDeathLink(new DeathLink(SlotName(), cause));
                Log.Message($"Sent DeathLink: {cause}");
            }
            catch (System.Exception ex)
            {
                Log.Error($"Failed to send DeathLink: {ex.Message}");
            }
        }

        private static string SlotName()
        {
            return GarfieldKartAPMod.APClient?.SlotName ?? "Player";
        }

        private static Kart FindLocalPlayerKart()
        {
            foreach (Driver driver in Object.FindObjectsOfType<Driver>())
            {
                if (driver.IsHuman && driver.IsLocal && driver.Kart != null)
                    return driver.Kart;
            }

            return null;
        }
    }
}
