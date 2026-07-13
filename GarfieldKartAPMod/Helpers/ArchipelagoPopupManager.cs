namespace GarfieldKartAPMod.Helpers
{
    // Single entry point for showing the game's native popups
    internal static class ArchipelagoPopupManager
    {
        public static void ShowInfo(string message)
        {
            Show(message, PopupHD.POPUP_TYPE.INFORMATION);
        }

        public static void ShowWarning(string message)
        {
            Show(message, PopupHD.POPUP_TYPE.WARNING);
        }

        public static void ShowError(string message)
        {
            Show(message, PopupHD.POPUP_TYPE.ERROR);
        }

        // Inspirational Garfield Quote filler: pop a random quote from the list
        public static void ShowRandomGarfieldQuote()
        {
            string[] quotes = ArchipelagoConstants.GARFIELD_QUOTES;
            string quote = quotes[UnityEngine.Random.Range(0, quotes.Length)];
            ShowInfo($"Garfield has an inspirational quote to help you!\n\n\"{quote}\"");
        }

        private static void Show(string message, PopupHD.POPUP_TYPE type)
        {
            PopupManager.OpenPopup(message, type, PopupHD.POPUP_PRIORITY.NORMAL);
        }
    }
}
