using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Reflection;

namespace GarfieldKartAPMod
{
    public static class UITextureSwapper
    {
        public static string spriteFolder = "Resources/Sprites";

        private static Sprite baseArchipelagoSprite;
        public static Sprite puzzlePieceFilledSprite;
        public static Sprite puzzlePieceEmptySprite;
        public static Sprite mainMenuLogoSprite;
        public static Sprite galleryIconSprite;
        private static bool initialized;
        private static bool hasSwappedThisMenu;

        public static void Initialize()
        {
            if (initialized) return;

            Log.Message("Initializing UI texture swapper...");

            bool allSpritesLoaded = true;
            allSpritesLoaded = TryLoadSprite("garfkart_ap_puzzle_filled.png", out baseArchipelagoSprite);
            allSpritesLoaded = allSpritesLoaded && TryLoadSprite("garfkart_ap_puzzle_filled.png", out puzzlePieceFilledSprite);
            allSpritesLoaded = allSpritesLoaded && TryLoadSprite("garfkart_ap_puzzle_empty.png", out puzzlePieceEmptySprite);
            allSpritesLoaded = allSpritesLoaded && TryLoadSprite("logo_garfAP_complete.png", out mainMenuLogoSprite);
            allSpritesLoaded = allSpritesLoaded && TryLoadSprite("garfkart_ap_icon.png", out galleryIconSprite);

            if (allSpritesLoaded)
            {
                initialized = true;
            }
        }

        private static bool TryLoadSprite(string path, out Sprite targetSprite)
        {
            string nameSpace = typeof(UITextureSwapper).Namespace;

            targetSprite = null;

            try
            {
                // Load from embedded resource
                Assembly assembly = Assembly.GetExecutingAssembly();

                string[] resourceNames = assembly.GetManifestResourceNames();

#if DEBUG
                // List all resources (debug only)
                Log.Message($"Found {resourceNames.Length} embedded resources:");
                foreach (string name in resourceNames)
                {
                    Log.Message($"  - {name}");
                }
#endif

                string resourceName = null;
                foreach (string name in resourceNames)
                {
                    if (name != path && !name.EndsWith($".{path}")) continue;
                    if (resourceName != null)
                    {
                        throw new ApplicationException("Duplicate resource name found, unable to load the correct texture: " + name);
                    }

                    resourceName = name;
                }

                if (resourceName == null)
                {
                    Log.Warning($"Couldn't find {path} in embedded resources, loading default sprite instead.");
                    targetSprite = CreateDefaultSprite();
                    return false;
                }

                Log.Message($"Loading embedded resource: {resourceName}");

                using Stream stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    // Something went wrong, I can't be bothered to figure out what
                    Log.Error($"Failed to load {path} for unknown reasons :)");
                    targetSprite = CreateDefaultSprite();
                    return false;
                }

                byte[] imageData = new byte[stream.Length];
                stream.Read(imageData, 0, (int)stream.Length);

                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(imageData);
                texture.Apply();

                // Create sprite from texture
                targetSprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100.0f
                );

                Log.Message($"Successfully loaded {path} ({texture.width}x{texture.height})");
                return true;
            }

            catch (Exception ex)
            {
                Log.Error($"Failed to load {path}: {ex.Message}\n{ex.StackTrace}");
                targetSprite = CreateDefaultSprite();
                return false;
            }
        }

        private static Sprite CreateDefaultSprite()
        {
            // Create a bright red square as placeholder
            Texture2D texture = new Texture2D(64, 64);
            Color[] pixels = new Color[64 * 64];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.red;
            }

            texture.SetPixels(pixels);
            texture.Apply();

            Log.Message("Created default red placeholder sprite");

            return Sprite.Create(
                texture,
                new Rect(0, 0, 64, 64),
                new Vector2(0.5f, 0.5f),
                100.0f
            );

        }

        public static void ResetSwapFlag()
        {
            hasSwappedThisMenu = false;
        }

        public static void SwapMainMenuLogo(GameObject root)
        {
            if (mainMenuLogoSprite == null)
            {
                Log.Error("Cannot swap - main menu logo sprite not loaded");
                return;
            }

            try
            {
                int swapCount = 0;
                bool alreadySwapped = false;

                var images = root.GetComponentsInChildren<UnityEngine.UI.Image>(true);
                foreach (var image in images)
                {
                    if (image.sprite == null) continue;
                    if (image.sprite == mainMenuLogoSprite)
                    {
                        alreadySwapped = true;
                        continue;
                    }

                    string spriteName = image.sprite.name.ToLower();
                    string objName = image.gameObject.name.ToLower();

                    if (!spriteName.Contains("titlelogo") && !objName.Contains("titlelogo")) continue;
                    image.sprite = mainMenuLogoSprite;
                    image.preserveAspect = true;
                    swapCount++;
                    Log.Message($"Swapped main menu logo on: {image.gameObject.name}");
                }

                if (swapCount == 0 && !alreadySwapped)
                {
                    Log.Warning("No main menu logo image found to swap");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to swap main menu logo: {ex.Message}");
            }
        }
        
        private const float GalleryIconScale = 0.7f;

        // Shift it a bit to line up
        private static readonly Vector2 GalleryIconNudge = new Vector2(10f, 0f);

        public static void SwapGalleryButtonIcon(GameObject galleryButton)
        {
            if (galleryButton == null) return;
            if (galleryIconSprite == null)
            {
                Log.Error("Cannot swap - AP gallery icon sprite not loaded");
                return;
            }

            try
            {
                Button button = galleryButton.GetComponent<Button>();
                Graphic background = button != null ? button.targetGraphic : null;

                int swapCount = 0;
                foreach (Image image in galleryButton.GetComponentsInChildren<Image>(true))
                {
                    if (image == background) continue;
                    // Already ours, so the scale and nudge below must not be applied a second time
                    if (image.sprite == galleryIconSprite) continue;

                    string objName = image.gameObject.name.ToLower();
                    string spriteName = image.sprite != null ? image.sprite.name.ToLower() : "";

                    // Logged either way, so the BepInEx log shows what's actually on the button
                    // if this heuristic picks the wrong image (or none at all)
                    Log.Message($"Gallery button image: {image.gameObject.name} (sprite: {spriteName})");
                    if (!objName.Contains("icn") && !objName.Contains("icon")
                        && !spriteName.Contains("icn") && !spriteName.Contains("icon")) continue;

                    image.sprite = galleryIconSprite;
                    image.preserveAspect = true;

                    // Set rather than multiplied, so re-entering the menu can't shrink it again.
                    // Scaled instead of resized because a stretched icon has no sizeDelta to cut.
                    image.rectTransform.localScale = Vector3.one * GalleryIconScale;
                    image.rectTransform.anchoredPosition += GalleryIconNudge;
                    swapCount++;
                }

                if (swapCount == 0) Log.Warning("No icon image found on the Gallery button to swap");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to swap gallery button icon: {ex.Message}");
            }
        }

        public static void SwapPuzzlePieceIcons(GameObject menu)
        {
            if (hasSwappedThisMenu)
            {
                Log.Error("Cannot swap - already swapped this menu");
                return;
            }
            if (baseArchipelagoSprite == null)
            {
                Log.Error("Cannot swap - Archipelago sprite not loaded");
                return;
            }

            try
            {
                int swapCount = 0;

                var images = menu.GetComponentsInChildren<UnityEngine.UI.Image>(true);
                foreach (var image in images)
                {
                    if (image.sprite == null) continue;
                    string spriteName = image.sprite.name.ToLower();
                    string objName = image.gameObject.name.ToLower();

                    if (!spriteName.Contains("icnpuzzle") && !objName.Contains("icnpuzzle") &&
                        !spriteName.Contains("icnpuzzlefull") && !objName.Contains("icnpuzzlefull")) continue;
                    image.sprite = baseArchipelagoSprite;
                    swapCount++;
                    Log.Message($"Swapped UI.Image on: {image.gameObject.name}");
                }

                Log.Message($"Swapped {swapCount} puzzle piece icons");

                hasSwappedThisMenu = true;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to swap puzzle icons: {ex.Message}");
            }
        }
    }
}