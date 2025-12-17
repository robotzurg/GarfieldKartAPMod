using UnityEngine;
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
        private static bool initialized;
        private static bool hasSwappedThisMenu;

        public static void Initialize()
        {
            if (initialized) return;

            Log.Message("Initializing UI texture swapper...");

            bool allSpritesLoaded = true;
            allSpritesLoaded = TryLoadSprite("archipelago_logo.png", out baseArchipelagoSprite);
            allSpritesLoaded = allSpritesLoaded && TryLoadSprite("garfkart_ap_puzzle_filled.png", out puzzlePieceFilledSprite);
            allSpritesLoaded = allSpritesLoaded && TryLoadSprite("garfkart_ap_puzzle_empty.png", out puzzlePieceEmptySprite);

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

        public static void SwapPuzzlePieceIcons(GameObject menu)
        {
            if (hasSwappedThisMenu)
            {
                return;
            }
            if (baseArchipelagoSprite == null)
            {
                Log.Error("Cannot swap - Archipelago sprite not loaded");
                return;
            }

            if (!GarfieldKartAPMod.APClient.IsConnected)
            {
                return; // Only swap when connected
            }

            try
            {
                int swapCount = 0;

                var images = menu.GetComponentsInChildren<UnityEngine.UI.Image>(true);
                foreach (var image in images)
                {
                    if (image.sprite != null)
                    {
                        string spriteName = image.sprite.name.ToLower();
                        string objName = image.gameObject.name.ToLower();

                        if (spriteName.Contains("icnpuzzle") || objName.Contains("icnpuzzle") ||
                            spriteName.Contains("icnpuzzlefull") || objName.Contains("icnpuzzlefull"))
                        {
                            image.sprite = baseArchipelagoSprite;
                            swapCount++;
                            Log.Message($"Swapped UI.Image on: {image.gameObject.name}");
                        }
                    }
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