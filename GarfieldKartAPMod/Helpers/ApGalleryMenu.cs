using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GarfieldKartAPMod.Helpers
{
    // Replaces the artwork gallery with a grid of every randomised item, lit when received and
    // dimmed when not.
    //
    // The vanilla menu is left in place and only hidden, so MenuHDGallery still owns the input
    // and B backs out through its OnCancelAction as usual. The artwork tiles are made
    // non-interactable as well as hidden: submitting one would push the menu into
    // ARTWORK_DETAIL behind our panel, where B would only return to the (empty) list.
    internal static class ApGalleryMenu
    {
        private const string RootName = "APGalleryRoot";
        private const int Columns = 8;
        private const float TileSize = 116f;
        private const float TileSpacing = 10f;

        private static readonly Color ReceivedTint = Color.white;
        private static readonly Color MissingTint = new Color(0.14f, 0.14f, 0.17f, 0.9f);
        // Received, but the game had no icon for it
        private static readonly Color IconlessTint = new Color(1f, 1f, 1f, 0.4f);

        // The header label keeps the colour it was cloned with; this dark orange sits behind it
        // as a drop shadow so it reads against the gallery background
        private static readonly Color HeaderShadowColor = new Color(0.42f, 0.16f, 0.02f, 1f);
        private static readonly Vector2 HeaderShadowOffset = new Vector2(2f, -2f);
        private const float HeaderFontSize = 32f;
        private const float HeaderHeight = 24f;

        // Index into UISprites.TutoBonusIcons per bonus, taken from
        // KartSelectionHatBonus.GetBonusSprite. Ordered to match ITEM_PIE..ITEM_SPRING (901-909).
        private static readonly (BonusCategory Bonus, int IconIndex)[] BonusIcons =
        {
            (BonusCategory.PIE, 4),
            (BonusCategory.AUTOLOCK_PIE, 6),
            (BonusCategory.DIAMOND, 9),
            (BonusCategory.MAGIC, 7),
            (BonusCategory.PARFUME, 1),
            (BonusCategory.LASAGNA, 0),
            (BonusCategory.UFO, 11),
            (BonusCategory.NAP, 10),
            (BonusCategory.SPRING, 2)
        };

        private class Entry
        {
            public Sprite Icon;
            public bool Received;
        }

        private class Section
        {
            public string Header;
            // Empty for a section that's only a running total, like the puzzle pieces
            public List<Entry> Entries;
        }

        public static void Refresh(MenuHDGallery menu, TextMeshProUGUI titleLabel, Canvas listCanvas,
            GameObject submitButton, InfoBox infoBox)
        {
            if (menu == null) return;

            bool replace = ArchipelagoHelper.IsConnectedAndEnabled;

            // Hide the vanilla gallery, or hand it back if the session went away
            if (listCanvas != null) listCanvas.gameObject.SetActive(!replace);
            if (submitButton != null) submitButton.SetActive(!replace);

            // Deactivate info box
            if (replace) infoBox?.SetText("");
            foreach (GalleryArtwork artwork in menu.GetComponentsInChildren<GalleryArtwork>(true))
            {
                // Only the 16 list tiles are given a button by InitButton()
                if (artwork.Button != null) artwork.Button.interactable = !replace;
            }

            Transform existing = menu.transform.Find(RootName);
            if (existing != null)
            {
                // Renamed so the Find above can't turn it up again before Destroy runs
                existing.name = RootName + "_old";
                Object.Destroy(existing.gameObject);
            }

            if (!replace) return;

            if (titleLabel != null) titleLabel.text = "Archipelago Items";
            BuildPanel(menu, titleLabel);
        }

        private static void BuildPanel(MenuHDGallery menu, TextMeshProUGUI fontSource)
        {
            GameObject root = new GameObject(RootName, typeof(RectTransform));
            root.transform.SetParent(menu.transform, false);
            
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 1f);
            rootRect.anchorMax = new Vector2(0.5f, 1f);
            rootRect.pivot = new Vector2(0.5f, 1f);
            rootRect.anchoredPosition = new Vector2(0f, -TopMargin);
            rootRect.sizeDelta = new Vector2(Columns * (TileSize + TileSpacing), 0f);

            VerticalLayoutGroup layout = root.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.spacing = 6f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = root.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            List<Section> sections = BuildSections();
            if (sections.Count == 0)
            {
                CreateHeader(root.transform, fontSource, "Nothing is randomized in this seed.");
                return;
            }

            foreach (Section section in sections)
            {
                CreateHeader(root.transform, fontSource, section.Header);
                if (section.Entries.Count > 0) CreateGrid(root.transform, section.Entries);
            }

            ShrinkToFit(menu, rootRect);
        }

        // Clearance kept above the panel for the title and below it for the footer
        private const float TopMargin = 240f;
        private const float BottomMargin = 70f;

        private static void ShrinkToFit(MenuHDGallery menu, RectTransform rootRect)
        {
            if (menu.transform is not RectTransform menuRect) return;
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(rootRect);
            
            float available = menuRect.rect.height - TopMargin - BottomMargin;
            float needed = LayoutUtility.GetPreferredHeight(rootRect);
            if (needed <= 0f || available <= 0f || needed <= available) return;
            
            float scale = available / needed;
            rootRect.localScale = Vector3.one * scale;
            Log.Message($"[Gallery] Panel needs {needed:F0} of {available:F0} available, scaled to {scale:F2}");
        }

        private static List<Section> BuildSections()
        {
            List<Section> sections = new List<Section>();

            sections.Add(BuildGoalSection());
            if (ArchipelagoHelper.IsPuzzleRandomizationEnabled())
                sections.Add(BuildPuzzleSection());
            if (ArchipelagoHelper.IsCharRandomizerEnabled())
                sections.Add(Grid("RACERS", BuildDriverEntries(character: true)));
            if (ArchipelagoHelper.IsKartRandomizerEnabled())
                sections.Add(Grid("KARTS", BuildDriverEntries(character: false)));
            if (ArchipelagoHelper.IsHatRandomizerEnabled())
                sections.Add(Grid("HATS", BuildHatEntries()));
            if (ArchipelagoHelper.IsSpoilerRandomizerEnabled())
                sections.Add(Grid("SPOILERS", BuildSpoilerEntries()));
            if (ArchipelagoHelper.IsItemRandomizerEnabled())
                sections.Add(Grid("ITEMS", BuildBonusEntries()));

            return sections;
        }

        private static Section Grid(string title, List<Entry> entries)
        {
            int received = entries.Count(entry => entry.Received);
            return new Section { Header = $"{title}   {received}/{entries.Count}", Entries = entries };
        }
        
        private static Section BuildGoalSection()
        {
            string header = $"GOAL: ";
            switch (ArchipelagoGoalManager.GetGoalId())
            {
                case ArchipelagoConstants.GOAL_GRAND_PRIX:
                    header += "GET FIRST IN ALL CUPS ON GRAND PRIX"; break;
                case ArchipelagoConstants.GOAL_RACES:
                    header += "GET FIRST IN ALL RACES"; break;
                case ArchipelagoConstants.GOAL_TIME_TRIALS:
                    header += "DEFEAT ALL TIME TRIALS"; break;
                case ArchipelagoConstants.GOAL_PUZZLE_PIECE:
                    header += "GET THE REQUIRED PUZZLE PIECES BELOW, THEN WIN A RACE"; break;
            }

            return new Section { Header = header, Entries = [] };
        }
        
        private static Section BuildPuzzleSection()
        {
            int received = ArchipelagoItemTracker.GetOverallPuzzlePieceCount();

            string header;
            try
            {
                if (ArchipelagoGoalManager.GetGoalId() == ArchipelagoConstants.GOAL_PUZZLE_PIECE)
                    header = $"PUZZLE PIECES   {received}/{ArchipelagoHelper.GetRequiredPuzzlePieceCount()}";
                else
                    header = $"PUZZLE PIECES   {received}/{ArchipelagoHelper.GetPuzzlePieceCount()}";
            }
            catch (SlotDataException)
            {
                header = $"PUZZLE PIECES   {received}";
            }

            return new Section { Header = header, Entries = [] };
        }

        // Characters and karts both run in ECharacter order, one item id each
        private static List<Entry> BuildDriverEntries(bool character)
        {
            long baseItem = character
                ? ArchipelagoConstants.ITEM_CHARACTER_GARFIELD
                : ArchipelagoConstants.ITEM_KART_FORMULA_ZZZZ;

            List<Entry> entries = new List<Entry>();
            for (int i = 0; i < 8; i++)
            {
                entries.Add(new Entry
                {
                    Icon = character ? UISprites.CharactersFaces[i] : UISprites.KartIcons[i],
                    Received = ArchipelagoItemTracker.HasItem(baseItem + i)
                });
            }

            return entries;
        }

        private static List<Entry> BuildHatEntries()
        {
            return BuildCosmeticEntries(
                PlayerGameEntities.HatList?.Cast<IconCarac>(),
                ArchipelagoConstants.GetHatItemId,
                UISprites.HatIcons);
        }

        private static List<Entry> BuildSpoilerEntries()
        {
            return BuildCosmeticEntries(
                PlayerGameEntities.KartCustomList?.Cast<IconCarac>(),
                ArchipelagoConstants.GetSpoilerItemId,
                UISprites.KartCustomIcons);
        }
        
        private static List<Entry> BuildCosmeticEntries(IEnumerable<IconCarac> all,
            System.Func<string, long> toItemId, Dictionary<string, Sprite> icons)
        {
            List<Entry> entries = new List<Entry>();
            if (all == null) return entries;

            HashSet<long> seen = new HashSet<long>();
            foreach (IconCarac carac in all)
            {
                long itemId = toItemId(carac.name);
                if (itemId == -1 || !seen.Add(itemId)) continue;

                icons.TryGetValue(carac.name, out Sprite icon);
                entries.Add(new Entry { Icon = icon, Received = ArchipelagoItemTracker.HasItem(itemId) });
            }

            return entries;
        }

        private static List<Entry> BuildBonusEntries()
        {
            return BonusIcons.Select(bonus => new Entry
            {
                Icon = UISprites.TutoBonusIcons[bonus.IconIndex],
                Received = ArchipelagoItemTracker.HasBonusAvailable(bonus.Bonus)
            }).ToList();
        }

        // The shadow and the label have to be siblings under an empty container, not parent and
        // child: a Graphic always draws before its own children, so a shadow parented under the
        // label would land on top of it whatever its sibling index is.
        private static void CreateHeader(Transform parent, TextMeshProUGUI fontSource, string text)
        {
            GameObject container = new GameObject("APGalleryHeader", typeof(RectTransform));
            container.transform.SetParent(parent, false);

            LayoutElement element = container.AddComponent<LayoutElement>();
            element.minHeight = HeaderHeight;
            element.preferredHeight = HeaderHeight;

            TextMeshProUGUI shadow = CreateHeaderLabel(container.transform, fontSource, text, "APGalleryHeaderShadow");

            // Cloned from the shadow rather than built fresh, so the two can't disagree on font,
            // size, auto-sizing or margins. Cloned before the shadow is recoloured, so the label
            // keeps the colour it inherited from the menu.
            TextMeshProUGUI label = Object.Instantiate(shadow.gameObject, container.transform)
                .GetComponent<TextMeshProUGUI>();
            label.gameObject.name = "APGalleryHeaderLabel";
            label.transform.SetAsLastSibling();

            shadow.color = HeaderShadowColor;

            // With stretch anchors the displacement has to go through offsetMin/Max -
            // anchoredPosition has no effect
            shadow.rectTransform.offsetMin = HeaderShadowOffset;
            shadow.rectTransform.offsetMax = HeaderShadowOffset;
        }

        private static TextMeshProUGUI CreateHeaderLabel(Transform parent, TextMeshProUGUI fontSource,
            string text, string name)
        {
            GameObject go;
            if (fontSource != null)
            {
                // Cloning the menu's own label keeps the font, material and canvas setup
                go = Object.Instantiate(fontSource.gameObject, parent);
                foreach (Transform child in go.transform) Object.Destroy(child.gameObject);
                foreach (Component component in go.GetComponents<Component>())
                {
                    if (component is RectTransform || component is CanvasRenderer || component is TextMeshProUGUI) continue;
                    Object.Destroy(component);
                }
            }
            else
            {
                go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
                go.transform.SetParent(parent, false);
            }

            go.name = name;
            go.SetActive(true);

            TextMeshProUGUI label = go.GetComponent<TextMeshProUGUI>();
            label.text = text;
            label.alignment = TextAlignmentOptions.Left;
            // A cloned label can arrive with auto-sizing on, which drives the size itself and
            // ignores fontSize outright
            label.enableAutoSizing = false;
            label.fontSize = HeaderFontSize;
            label.enableWordWrapping = false;
            label.overflowMode = TextOverflowModes.Overflow;
            label.raycastTarget = false;
            label.margin = Vector4.zero;

            RectTransform rect = label.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;

            return label;
        }

        private static void CreateGrid(Transform parent, List<Entry> entries)
        {
            GameObject go = new GameObject("APGalleryGrid", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            // GridLayoutGroup reports its own preferred height, so the parent vertical group
            // sizes it without needing a ContentSizeFitter here
            GridLayoutGroup grid = go.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(TileSize, TileSize);
            grid.spacing = new Vector2(TileSpacing, TileSpacing);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = Columns;
            grid.childAlignment = TextAnchor.UpperLeft;

            foreach (Entry entry in entries)
            {
                GameObject tile = new GameObject("APGalleryTile", typeof(RectTransform), typeof(Image));
                tile.transform.SetParent(go.transform, false);

                Image image = tile.GetComponent<Image>();
                image.sprite = entry.Icon;
                image.preserveAspect = true;
                image.raycastTarget = false;
                image.color = entry.Icon == null && entry.Received
                    ? IconlessTint
                    : entry.Received ? ReceivedTint : MissingTint;
            }
        }
    }
}
