using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

namespace GarfieldKartAPMod
{
    public static class UITextureSwapper
    {
        private static Sprite archipelagoSprite;
        private static bool initialized = false;
        private static bool hasSwappedThisMenu = false;

        public static void Initialize()
        {
            if (initialized) return;

            Log.Message("Initializing UI texture swapper...");
            LoadArchipelagoSprite();
            initialized = true;
        }

        private static void LoadArchipelagoSprite()
        {
            try
            {
                // Load from embedded resource
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();

                // List all embedded resources (for debugging)
                string[] resourceNames = assembly.GetManifestResourceNames();
                Log.Message($"Found {resourceNames.Length} embedded resources:");
                foreach (string name in resourceNames)
                {
                    Log.Message($"  - {name}");
                }

                // Try to find the archipelago logo
                // The resource name format is typically: Namespace.Folder.Filename
                string resourceName = null;

                // Try common patterns
                string[] possibleNames = new string[]
                {
            "GarfieldKartAPMod.archipelago_logo.png",
            "GarfieldKartAPMod.Resources.archipelago_logo.png",
            "archipelago_logo.png"
                };

                foreach (string possible in possibleNames)
                {
                    if (resourceNames.Contains(possible))
                    {
                        resourceName = possible;
                        break;
                    }
                }

                // Or just find anything with "archipelago" in the name
                if (resourceName == null)
                {
                    foreach (string name in resourceNames)
                    {
                        if (name.ToLower().Contains("archipelago"))
                        {
                            resourceName = name;
                            break;
                        }
                    }
                }

                if (resourceName != null)
                {
                    Log.Message($"Loading embedded resource: {resourceName}");

                    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                    {
                        if (stream != null)
                        {
                            byte[] imageData = new byte[stream.Length];
                            stream.Read(imageData, 0, (int)stream.Length);

                            Texture2D texture = new Texture2D(2, 2);
                            texture.LoadImage(imageData);
                            texture.Apply();

                            // Create sprite from texture
                            archipelagoSprite = Sprite.Create(
                                texture,
                                new Rect(0, 0, texture.width, texture.height),
                                new Vector2(0.5f, 0.5f),
                                100.0f
                            );

                            Log.Message($"Successfully loaded Archipelago logo ({texture.width}x{texture.height})");
                            return;
                        }
                    }
                }

                Log.Warning("Could not find archipelago_logo.png in embedded resources");
                CreateDefaultSprite();
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load Archipelago sprite: {ex.Message}\n{ex.StackTrace}");
                CreateDefaultSprite();
            }
        }

        private static void CreateDefaultSprite()
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

            archipelagoSprite = Sprite.Create(
                texture,
                new Rect(0, 0, 64, 64),
                new Vector2(0.5f, 0.5f),
                100.0f
            );

            Log.Message("Created default red placeholder sprite");
        }

        public static void ResetSwapFlag()
        {
            hasSwappedThisMenu = false;
        }

        public static void SwapPuzzlePieceIcons(MenuHDTrackSelection menu)
        {
            if (hasSwappedThisMenu)
            {
                return;
            }
            if (archipelagoSprite == null)
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

                // Method 1: Unity UI (newer)
                var images = menu.GetComponentsInChildren<UnityEngine.UI.Image>(true);
                foreach (var image in images)
                {
                    if (image.sprite != null)
                    {
                        string spriteName = image.sprite.name.ToLower();
                        string objName = image.gameObject.name.ToLower();

                        if (spriteName.Contains("icnpuzzle") || objName.Contains("icnpuzzle"))
                        {
                            image.sprite = archipelagoSprite;
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