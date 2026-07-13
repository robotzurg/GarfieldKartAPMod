using System.Collections.Generic;

namespace GarfieldKartAPMod
{
    public static class ArchipelagoConstants
    {
        // ========== LOCATION CONSTANTS (what you check in-game) ==========
        // These are the location IDs that get sent when you achieve something

        // Single Race Victories (1-16)
        public const long LOC_CATZ_IN_THE_HOOD_VICTORY = 1;
        public const long LOC_CRAZY_DUNES_VICTORY = 2;
        public const long LOC_PALEROCK_LAKE_VICTORY = 3;
        public const long LOC_CITY_SLICKER_VICTORY = 4;
        public const long LOC_COUNTRY_BUMPKIN_VICTORY = 5;
        public const long LOC_SPOOKY_MANOR_VICTORY = 6;
        public const long LOC_MALLY_MARKET_VICTORY = 7;
        public const long LOC_VALLEY_OF_THE_KINGS_VICTORY = 8;
        public const long LOC_MISTY_FOR_ME_VICTORY = 9;
        public const long LOC_SNEAK_A_PEAK_VICTORY = 10;
        public const long LOC_BLAZING_OASIS_VICTORY = 11;
        public const long LOC_PASTACOSI_FACTORY_VICTORY = 12;
        public const long LOC_MYSTERIOUS_TEMPLE_VICTORY = 13;
        public const long LOC_PROHIBITED_SITE_VICTORY = 14;
        public const long LOC_CASKOU_PARK_VICTORY = 15;
        public const long LOC_LOOPY_LAGOON_VICTORY = 16;

        // Time Trial Bronze (21-36)
        public const long LOC_CATZ_IN_THE_HOOD_TIME_TRIAL_BRONZE = 21;
        public const long LOC_CRAZY_DUNES_TIME_TRIAL_BRONZE = 22;
        public const long LOC_PALEROCK_LAKE_TIME_TRIAL_BRONZE = 23;
        public const long LOC_CITY_SLICKER_TIME_TRIAL_BRONZE = 24;
        public const long LOC_COUNTRY_BUMPKIN_TIME_TRIAL_BRONZE = 25;
        public const long LOC_SPOOKY_MANOR_TIME_TRIAL_BRONZE = 26;
        public const long LOC_MALLY_MARKET_TIME_TRIAL_BRONZE = 27;
        public const long LOC_VALLEY_OF_THE_KINGS_TIME_TRIAL_BRONZE = 28;
        public const long LOC_MISTY_FOR_ME_TIME_TRIAL_BRONZE = 29;
        public const long LOC_SNEAK_A_PEAK_TIME_TRIAL_BRONZE = 30;
        public const long LOC_BLAZING_OASIS_TIME_TRIAL_BRONZE = 31;
        public const long LOC_PASTACOSI_FACTORY_TIME_TRIAL_BRONZE = 32;
        public const long LOC_MYSTERIOUS_TEMPLE_TIME_TRIAL_BRONZE = 33;
        public const long LOC_PROHIBITED_SITE_TIME_TRIAL_BRONZE = 34;
        public const long LOC_CASKOU_PARK_TIME_TRIAL_BRONZE = 35;
        public const long LOC_LOOPY_LAGOON_TIME_TRIAL_BRONZE = 36;

        // Time Trial Silver (41-56)
        public const long LOC_CATZ_IN_THE_HOOD_TIME_TRIAL_SILVER = 41;
        public const long LOC_CRAZY_DUNES_TIME_TRIAL_SILVER = 42;
        public const long LOC_PALEROCK_LAKE_TIME_TRIAL_SILVER = 43;
        public const long LOC_CITY_SLICKER_TIME_TRIAL_SILVER = 44;
        public const long LOC_COUNTRY_BUMPKIN_TIME_TRIAL_SILVER = 45;
        public const long LOC_SPOOKY_MANOR_TIME_TRIAL_SILVER = 46;
        public const long LOC_MALLY_MARKET_TIME_TRIAL_SILVER = 47;
        public const long LOC_VALLEY_OF_THE_KINGS_TIME_TRIAL_SILVER = 48;
        public const long LOC_MISTY_FOR_ME_TIME_TRIAL_SILVER = 49;
        public const long LOC_SNEAK_A_PEAK_TIME_TRIAL_SILVER = 50;
        public const long LOC_BLAZING_OASIS_TIME_TRIAL_SILVER = 51;
        public const long LOC_PASTACOSI_FACTORY_TIME_TRIAL_SILVER = 52;
        public const long LOC_MYSTERIOUS_TEMPLE_TIME_TRIAL_SILVER = 53;
        public const long LOC_PROHIBITED_SITE_TIME_TRIAL_SILVER = 54;
        public const long LOC_CASKOU_PARK_TIME_TRIAL_SILVER = 55;
        public const long LOC_LOOPY_LAGOON_TIME_TRIAL_SILVER = 56;

        // Time Trial Gold (61-76)
        public const long LOC_CATZ_IN_THE_HOOD_TIME_TRIAL_GOLD = 61;
        public const long LOC_CRAZY_DUNES_TIME_TRIAL_GOLD = 62;
        public const long LOC_PALEROCK_LAKE_TIME_TRIAL_GOLD = 63;
        public const long LOC_CITY_SLICKER_TIME_TRIAL_GOLD = 64;
        public const long LOC_COUNTRY_BUMPKIN_TIME_TRIAL_GOLD = 65;
        public const long LOC_SPOOKY_MANOR_TIME_TRIAL_GOLD = 66;
        public const long LOC_MALLY_MARKET_TIME_TRIAL_GOLD = 67;
        public const long LOC_VALLEY_OF_THE_KINGS_TIME_TRIAL_GOLD = 68;
        public const long LOC_MISTY_FOR_ME_TIME_TRIAL_GOLD = 69;
        public const long LOC_SNEAK_A_PEAK_TIME_TRIAL_GOLD = 70;
        public const long LOC_BLAZING_OASIS_TIME_TRIAL_GOLD = 71;
        public const long LOC_PASTACOSI_FACTORY_TIME_TRIAL_GOLD = 72;
        public const long LOC_MYSTERIOUS_TEMPLE_TIME_TRIAL_GOLD = 73;
        public const long LOC_PROHIBITED_SITE_TIME_TRIAL_GOLD = 74;
        public const long LOC_CASKOU_PARK_TIME_TRIAL_GOLD = 75;
        public const long LOC_LOOPY_LAGOON_TIME_TRIAL_GOLD = 76;

        // Time Trial Platinum (81-96)
        public const long LOC_CATZ_IN_THE_HOOD_TIME_TRIAL_PLATINUM = 81;
        public const long LOC_CRAZY_DUNES_TIME_TRIAL_PLATINUM = 82;
        public const long LOC_PALEROCK_LAKE_TIME_TRIAL_PLATINUM = 83;
        public const long LOC_CITY_SLICKER_TIME_TRIAL_PLATINUM = 84;
        public const long LOC_COUNTRY_BUMPKIN_TIME_TRIAL_PLATINUM = 85;
        public const long LOC_SPOOKY_MANOR_TIME_TRIAL_PLATINUM = 86;
        public const long LOC_MALLY_MARKET_TIME_TRIAL_PLATINUM = 87;
        public const long LOC_VALLEY_OF_THE_KINGS_TIME_TRIAL_PLATINUM = 88;
        public const long LOC_MISTY_FOR_ME_TIME_TRIAL_PLATINUM = 89;
        public const long LOC_SNEAK_A_PEAK_TIME_TRIAL_PLATINUM = 90;
        public const long LOC_BLAZING_OASIS_TIME_TRIAL_PLATINUM = 91;
        public const long LOC_PASTACOSI_FACTORY_TIME_TRIAL_PLATINUM = 92;
        public const long LOC_MYSTERIOUS_TEMPLE_TIME_TRIAL_PLATINUM = 93;
        public const long LOC_PROHIBITED_SITE_TIME_TRIAL_PLATINUM = 94;
        public const long LOC_CASKOU_PARK_TIME_TRIAL_PLATINUM = 95;
        public const long LOC_LOOPY_LAGOON_TIME_TRIAL_PLATINUM = 96;

        // Cup Victories (101-104)
        public const long LOC_LASAGNA_CUP_VICTORY = 101;
        public const long LOC_PIZZA_CUP_VICTORY = 102;
        public const long LOC_BURGER_CUP_VICTORY = 103;
        public const long LOC_ICE_CREAM_CUP_VICTORY = 104;

        // Per-CC Victories (701-784)
        // Race: 700 + cc * 20 + race victory id (1-16) -> 50cc 701-716, 100cc 721-736, 150cc 741-756
        // Cup:  760 + cc * 10 + cup number (1-4)       -> 50cc 761-764, 100cc 771-774, 150cc 781-784
        public const long LOC_RACE_VICTORY_CC_BASE = 700;
        public const long LOC_RACE_VICTORY_CC_GAP = 20;
        public const long LOC_CUP_VICTORY_CC_BASE = 760;
        public const long LOC_CUP_VICTORY_CC_GAP = 10;

        // Puzzle Pieces as Locations (201-248) - Used when puzzle pieces are checks
        public const long LOC_CATZ_IN_THE_HOOD_PUZZLE_PIECE_1 = 201;
        public const long LOC_CATZ_IN_THE_HOOD_PUZZLE_PIECE_2 = 202;
        public const long LOC_CATZ_IN_THE_HOOD_PUZZLE_PIECE_3 = 203;
        public const long LOC_CRAZY_DUNES_PUZZLE_PIECE_1 = 204;
        public const long LOC_CRAZY_DUNES_PUZZLE_PIECE_2 = 205;
        public const long LOC_CRAZY_DUNES_PUZZLE_PIECE_3 = 206;
        public const long LOC_PALEROCK_LAKE_PUZZLE_PIECE_1 = 207;
        public const long LOC_PALEROCK_LAKE_PUZZLE_PIECE_2 = 208;
        public const long LOC_PALEROCK_LAKE_PUZZLE_PIECE_3 = 209;
        public const long LOC_CITY_SLICKER_PUZZLE_PIECE_1 = 210;
        public const long LOC_CITY_SLICKER_PUZZLE_PIECE_2 = 211;
        public const long LOC_CITY_SLICKER_PUZZLE_PIECE_3 = 212;
        public const long LOC_COUNTRY_BUMPKIN_PUZZLE_PIECE_1 = 213;
        public const long LOC_COUNTRY_BUMPKIN_PUZZLE_PIECE_2 = 214;
        public const long LOC_COUNTRY_BUMPKIN_PUZZLE_PIECE_3 = 215;
        public const long LOC_SPOOKY_MANOR_PUZZLE_PIECE_1 = 216;
        public const long LOC_SPOOKY_MANOR_PUZZLE_PIECE_2 = 217;
        public const long LOC_SPOOKY_MANOR_PUZZLE_PIECE_3 = 218;
        public const long LOC_MALLY_MARKET_PUZZLE_PIECE_1 = 219;
        public const long LOC_MALLY_MARKET_PUZZLE_PIECE_2 = 220;
        public const long LOC_MALLY_MARKET_PUZZLE_PIECE_3 = 221;
        public const long LOC_VALLEY_OF_THE_KINGS_PUZZLE_PIECE_1 = 222;
        public const long LOC_VALLEY_OF_THE_KINGS_PUZZLE_PIECE_2 = 223;
        public const long LOC_VALLEY_OF_THE_KINGS_PUZZLE_PIECE_3 = 224;
        public const long LOC_MISTY_FOR_ME_PUZZLE_PIECE_1 = 225;
        public const long LOC_MISTY_FOR_ME_PUZZLE_PIECE_2 = 226;
        public const long LOC_MISTY_FOR_ME_PUZZLE_PIECE_3 = 227;
        public const long LOC_SNEAK_A_PEAK_PUZZLE_PIECE_1 = 228;
        public const long LOC_SNEAK_A_PEAK_PUZZLE_PIECE_2 = 229;
        public const long LOC_SNEAK_A_PEAK_PUZZLE_PIECE_3 = 230;
        public const long LOC_BLAZING_OASIS_PUZZLE_PIECE_1 = 231;
        public const long LOC_BLAZING_OASIS_PUZZLE_PIECE_2 = 232;
        public const long LOC_BLAZING_OASIS_PUZZLE_PIECE_3 = 233;
        public const long LOC_PASTACOSI_FACTORY_PUZZLE_PIECE_1 = 234;
        public const long LOC_PASTACOSI_FACTORY_PUZZLE_PIECE_2 = 235;
        public const long LOC_PASTACOSI_FACTORY_PUZZLE_PIECE_3 = 236;
        public const long LOC_MYSTERIOUS_TEMPLE_PUZZLE_PIECE_1 = 237;
        public const long LOC_MYSTERIOUS_TEMPLE_PUZZLE_PIECE_2 = 238;
        public const long LOC_MYSTERIOUS_TEMPLE_PUZZLE_PIECE_3 = 239;
        public const long LOC_PROHIBITED_SITE_PUZZLE_PIECE_1 = 240;
        public const long LOC_PROHIBITED_SITE_PUZZLE_PIECE_2 = 241;
        public const long LOC_PROHIBITED_SITE_PUZZLE_PIECE_3 = 242;
        public const long LOC_CASKOU_PARK_PUZZLE_PIECE_1 = 243;
        public const long LOC_CASKOU_PARK_PUZZLE_PIECE_2 = 244;
        public const long LOC_CASKOU_PARK_PUZZLE_PIECE_3 = 245;
        public const long LOC_LOOPY_LAGOON_PUZZLE_PIECE_1 = 246;
        public const long LOC_LOOPY_LAGOON_PUZZLE_PIECE_2 = 247;
        public const long LOC_LOOPY_LAGOON_PUZZLE_PIECE_3 = 248;

        // Lap Sanity Locations (500-659) - 10 laps per track, race_index * 10 + 500
        public const long LOC_CATZ_IN_THE_HOOD_LAP_SANITY     = 500;
        public const long LOC_CRAZY_DUNES_LAP_SANITY           = 510;
        public const long LOC_PALEROCK_LAKE_LAP_SANITY          = 520;
        public const long LOC_CITY_SLICKER_LAP_SANITY           = 530;
        public const long LOC_COUNTRY_BUMPKIN_LAP_SANITY        = 540;
        public const long LOC_SPOOKY_MANOR_LAP_SANITY           = 550;
        public const long LOC_MALLY_MARKET_LAP_SANITY           = 560;
        public const long LOC_VALLEY_OF_THE_KINGS_LAP_SANITY    = 570;
        public const long LOC_MISTY_FOR_ME_LAP_SANITY           = 580;
        public const long LOC_SNEAK_A_PEAK_LAP_SANITY           = 590;
        public const long LOC_BLAZING_OASIS_LAP_SANITY          = 600;
        public const long LOC_PASTACOSI_FACTORY_LAP_SANITY      = 610;
        public const long LOC_MYSTERIOUS_TEMPLE_LAP_SANITY      = 620;
        public const long LOC_PROHIBITED_SITE_LAP_SANITY        = 630;
        public const long LOC_CASKOU_PARK_LAP_SANITY            = 640;
        public const long LOC_LOOPY_LAGOON_LAP_SANITY           = 650;

        // Cup Unlock Spoiler Locations (301-314)
        // Spoiler 1: 301-304, Spoiler 2: 311-314 (sequential by cup index 0-3)
        public const long LOC_LASAGNA_CUP_UNLOCK_SPOILER_1 = 301;
        public const long LOC_PIZZA_CUP_UNLOCK_SPOILER_1 = 302;
        public const long LOC_BURGER_CUP_UNLOCK_SPOILER_1 = 303;
        public const long LOC_ICE_CREAM_CUP_UNLOCK_SPOILER_1 = 304;
        public const long LOC_LASAGNA_CUP_UNLOCK_SPOILER_2 = 311;
        public const long LOC_PIZZA_CUP_UNLOCK_SPOILER_2 = 312;
        public const long LOC_BURGER_CUP_UNLOCK_SPOILER_2 = 313;
        public const long LOC_ICE_CREAM_CUP_UNLOCK_SPOILER_2 = 314;

        // Hat Unlock Locations (401-416) - ids mirror race victory ids (400 + victory id)
        public const long LOC_CATZ_IN_THE_HOOD_HAT_UNLOCK = 401;
        public const long LOC_CRAZY_DUNES_HAT_UNLOCK = 402;
        public const long LOC_PALEROCK_LAKE_HAT_UNLOCK = 403;
        public const long LOC_CITY_SLICKER_HAT_UNLOCK = 404;
        public const long LOC_COUNTRY_BUMPKIN_HAT_UNLOCK = 405;
        public const long LOC_SPOOKY_MANOR_HAT_UNLOCK = 406;
        public const long LOC_MALLY_MARKET_HAT_UNLOCK = 407;
        public const long LOC_VALLEY_OF_THE_KINGS_HAT_UNLOCK = 408;
        public const long LOC_MISTY_FOR_ME_HAT_UNLOCK = 409;
        public const long LOC_SNEAK_A_PEAK_HAT_UNLOCK = 410;
        public const long LOC_BLAZING_OASIS_HAT_UNLOCK = 411;
        public const long LOC_PASTACOSI_FACTORY_HAT_UNLOCK = 412;
        public const long LOC_MYSTERIOUS_TEMPLE_HAT_UNLOCK = 413;
        public const long LOC_PROHIBITED_SITE_HAT_UNLOCK = 414;
        public const long LOC_CASKOU_PARK_HAT_UNLOCK = 415;
        public const long LOC_LOOPY_LAGOON_HAT_UNLOCK = 416;

        // Character Victory Locations (1001-1008)
        public const long LOC_WIN_RACE_AS_GARFIELD = 1001;
        public const long LOC_WIN_RACE_AS_JON = 1002;
        public const long LOC_WIN_RACE_AS_LIZ = 1003;
        public const long LOC_WIN_RACE_AS_ODIE = 1004;
        public const long LOC_WIN_RACE_AS_ARLENE = 1005;
        public const long LOC_WIN_RACE_AS_NERMAL = 1006;
        public const long LOC_WIN_RACE_AS_SQUEAK = 1007;
        public const long LOC_WIN_RACE_AS_HARRY = 1008;

        // Car Victory Locations (1051-1058)
        public const long LOC_WIN_RACE_WITH_FORMULA_ZZZZ = 1051;
        public const long LOC_WIN_RACE_WITH_ABSTRACT_KART = 1052;
        public const long LOC_WIN_RACE_WITH_MEDI_KART = 1053;
        public const long LOC_WIN_RACE_WITH_WOOF_MOBILE = 1054;
        public const long LOC_WIN_RACE_WITH_KISSY_KART = 1055;
        public const long LOC_WIN_RACE_WITH_CUTIE_PIE_CAT = 1056;
        public const long LOC_WIN_RACE_WITH_RAT_RACER = 1057;
        public const long LOC_WIN_RACE_WITH_MUCK_MADNESS = 1058;

        // Item Find Locations (1101-1109)
        public const long LOC_FIND_ITEM_PIE = 1101;
        public const long LOC_FIND_ITEM_HOMING_PIE = 1102;
        public const long LOC_FIND_ITEM_DIAMOND = 1103;
        public const long LOC_FIND_ITEM_MAGIC_WAND = 1104;
        public const long LOC_FIND_ITEM_PERFUME = 1105;
        public const long LOC_FIND_ITEM_LASAGNA = 1106;
        public const long LOC_FIND_ITEM_UFO = 1107;
        public const long LOC_FIND_ITEM_PILLOW = 1108;
        public const long LOC_FIND_ITEM_SPRING = 1109;

        // ========== ITEM CONSTANTS (what you receive from the server) ==========
        // These are the item IDs that come from Archipelago

        // Puzzle Piece (global item, ID 49)
        public const long ITEM_PUZZLE_PIECE = 49;

        // Course Unlocks (100-116)
        public const long ITEM_PROGRESSIVE_COURSE_UNLOCK = 100;
        public const long ITEM_COURSE_UNLOCK_CATZ_IN_THE_HOOD = 101;
        public const long ITEM_COURSE_UNLOCK_CRAZY_DUNES = 102;
        public const long ITEM_COURSE_UNLOCK_PALEROCK_LAKE = 103;
        public const long ITEM_COURSE_UNLOCK_CITY_SLICKER = 104;
        public const long ITEM_COURSE_UNLOCK_COUNTRY_BUMPKIN = 105;
        public const long ITEM_COURSE_UNLOCK_SPOOKY_MANOR = 106;
        public const long ITEM_COURSE_UNLOCK_MALLY_MARKET = 107;
        public const long ITEM_COURSE_UNLOCK_VALLEY_OF_THE_KINGS = 108;
        public const long ITEM_COURSE_UNLOCK_MISTY_FOR_ME = 109;
        public const long ITEM_COURSE_UNLOCK_SNEAK_A_PEAK = 110;
        public const long ITEM_COURSE_UNLOCK_BLAZING_OASIS = 111;
        public const long ITEM_COURSE_UNLOCK_PASTACOSI_FACTORY = 112;
        public const long ITEM_COURSE_UNLOCK_MYSTERIOUS_TEMPLE = 113;
        public const long ITEM_COURSE_UNLOCK_PROHIBITED_SITE = 114;
        public const long ITEM_COURSE_UNLOCK_CASKOU_PARK = 115;
        public const long ITEM_COURSE_UNLOCK_LOOPY_LAGOON = 116;

        // Cup Unlocks (200-204)
        public const long ITEM_PROGRESSIVE_CUP_UNLOCK = 200;
        public const long ITEM_CUP_UNLOCK_LASAGNA = 201;
        public const long ITEM_CUP_UNLOCK_PIZZA = 202;
        public const long ITEM_CUP_UNLOCK_BURGER = 203;
        public const long ITEM_CUP_UNLOCK_ICE_CREAM = 204;

        // Character Unlocks (301-308)
        public const long ITEM_CHARACTER_GARFIELD = 301;
        public const long ITEM_CHARACTER_JON = 302;
        public const long ITEM_CHARACTER_LIZ = 303;
        public const long ITEM_CHARACTER_ODIE = 304;
        public const long ITEM_CHARACTER_ARLENE = 305;
        public const long ITEM_CHARACTER_NERMAL = 306;
        public const long ITEM_CHARACTER_SQUEAK = 307;
        public const long ITEM_CHARACTER_HARRY = 308;

        // Kart Unlocks (351-358)
        public const long ITEM_KART_FORMULA_ZZZZ = 351;
        public const long ITEM_KART_ABSTRACT_KART = 352;
        public const long ITEM_KART_MEDI_KART = 353;
        public const long ITEM_KART_WOOF_MOBILE = 354;
        public const long ITEM_KART_KISSY_KART = 355;
        public const long ITEM_KART_CUTIE_PIE_CAT = 356;
        public const long ITEM_KART_RAT_RACER = 357;
        public const long ITEM_KART_MUCK_MADNESS = 358;

        // Unlock Hat Items (421-436)
        public const long ITEM_UNLOCK_BEDDY_BYE_CAP = 421;
        public const long ITEM_UNLOCK_WHIZZY_WIZARD = 422;
        public const long ITEM_UNLOCK_TIC_TOQUE = 423;
        public const long ITEM_UNLOCK_ELASTO_HAT = 424;
        public const long ITEM_UNLOCK_CHEFS_SPECIAL = 425;
        public const long ITEM_UNLOCK_CUTIE_PIE_CROWN = 426;
        public const long ITEM_UNLOCK_VIKING_HELMET = 427;
        public const long ITEM_UNLOCK_STINK_O_RAMA = 428;
        public const long ITEM_UNLOCK_SPACE_BUBBLE = 429;
        public const long ITEM_UNLOCK_PIZZAIOLO_HAT = 430;
        public const long ITEM_UNLOCK_BUNNY_BAND = 431;
        public const long ITEM_UNLOCK_JOE_MONTAGNA = 432;
        public const long ITEM_UNLOCK_ARISTO_CATIC_BICORN = 433;
        public const long ITEM_UNLOCK_TOUTANKHAMEOW = 434;
        public const long ITEM_UNLOCK_APPRENTICE_SORCERER = 435;
        public const long ITEM_UNLOCK_MULE_HEAD = 436;

        // Unlock Spoiler Items (521-528)
        public const long ITEM_UNLOCK_BOMBASTIC_SPOILER = 521;
        public const long ITEM_UNLOCK_WHACKY_SPOILER = 522;
        public const long ITEM_UNLOCK_SUPERFIT_SPOILER = 523;
        public const long ITEM_UNLOCK_CYCLOBONE_SPOILER = 524;
        public const long ITEM_UNLOCK_FOXY_SPOILER = 525;
        public const long ITEM_UNLOCK_SHIMMERING_SPOILER = 526;
        public const long ITEM_UNLOCK_HOLEY_MOLEY_SPOILER = 527;
        public const long ITEM_UNLOCK_STAINED_SPOILER = 528;

        // Item Box Randomizer Items (901-909)
        public const long ITEM_PIE = 901;
        public const long ITEM_HOMING_PIE = 902;
        public const long ITEM_DIAMOND = 903;
        public const long ITEM_MAGIC_WAND = 904;
        public const long ITEM_PERFUME = 905;
        public const long ITEM_LASAGNA = 906;
        public const long ITEM_UFO = 907;
        public const long ITEM_PILLOW = 908;
        public const long ITEM_SPRING = 909;
        
        // Filler Items (1000+)
        public const long ITEM_RANDOM_ITEM_BOX_FILLER = 1000;
        public const long ITEM_START_BOOST_HELPER_FILLER = 1001;
        public const long ITEM_STRONGER_ITEM_BOXES_FILLER = 1002; // Unused
        public const long ITEM_QUOTE_FILLER = 1003;

        // Trap Items (1500+)
        public const long ITEM_MIRROR_TRAP = 1500;
        public const long ITEM_SLEEP_TRAP = 1501;
        public const long ITEM_GRAYSCALE_TRAP = 1502;
        public const long ITEM_BROKEN_DRIFT_TRAP = 1503;
        public const long ITEM_BOUNCE_TRAP = 1504;

        // Goals
        public const long GOAL_GRAND_PRIX = 0;
        public const long GOAL_RACES = 1;
        public const long GOAL_TIME_TRIALS = 2;
        public const long GOAL_PUZZLE_PIECE = 3;

        // Options
        public const long OPTION_RANDOMIZE_RACES_CUPS = 0;
        public const long OPTION_RANDOMIZE_RACES_RACES = 1;
        public const long OPTION_RANDOMIZE_RACES_BOTH = 2;

        public const long OPTION_TRAP_HANDLING_TIME = 0;
        public const long OPTION_TRAP_HANDLING_RACE = 1;
        public const long OPTION_TRAP_HANDLING_WIN = 2;

        // Time traps stay active
        public const float TRAP_DISABLE_SECONDS = 60f;
        
        // ========== GARFIELD QUOTES ==========
        public static readonly string[] GARFIELD_QUOTES = [
            "Love me, feed me, never leave me.",
            "I am hungry. Therefore I am.",
            "Oh no! I overslept! I’m late! For my nap.",
            "Eat every meal as though it were your last.",
            "The most active thing about me is my imagination.",
            "A little ego goes nowhere.",
            "I’ll rise, but I won’t shine.",
            "Once again I’m saved by the miracle of… lasagna!",
            "So much time, and so little... I need to do.",
            "I just need a little quality time with man's real best friend, television.",
            "Sure, Jon. I'll eat all your lasagna for you.",
            "I'll purr like a Ferrari. Make that a Jaguar.",
            "A smart cat knows just how far to go without crossing over the line"
        ];

        // ========== HELPER METHODS ==========

        public static string GetSceneNameFromTrackId(TrackId trackId)
        {
            return trackId switch
            {
                // LASAGNA CUP (Championship 1)
                TrackId.E2C1 => "E2C1", // Catz in the Hood
                TrackId.E4C1 => "E4C1", // Crazy Dunes
                TrackId.E3C1 => "E3C1", // Palerock Lake
                TrackId.E1C1 => "E1C1", // City Slicker
                
                // PIZZA CUP (Championship 2)
                TrackId.E3C2 => "E3C2", // Country Bumpkin
                TrackId.E2C2 => "E2C2", // Spooky Manor
                TrackId.E1C2 => "E1C2", // Mally Market
                TrackId.E4C2 => "E4C2", // Valley of the Kings
                
                // BURGER CUP (Championship 3)
                TrackId.E1C3 => "E1C3", // Misty for Me
                TrackId.E3C3 => "E3C3", // Sneak a Peak
                TrackId.E4C3 => "E4C3", // Blazing Oasis
                TrackId.E2C3 => "E2C3", // Pastacosi Factory
                
                // ICE CREAM CUP (Championship 4)
                TrackId.E4C4 => "E4C4", // Mysterious Temple
                TrackId.E1C4 => "E1C4", // Prohibited Site
                TrackId.E2C4 => "E2C4", // Caskou Park
                TrackId.E3C4 => "E3C4", // Loopy Lagoon
                
                _ => null
            };
        }

        public static long GetRaceVictoryLoc(string startScene)
        {
            return startScene switch
            {
                "E2C1" => LOC_CATZ_IN_THE_HOOD_VICTORY,
                "E4C1" => LOC_CRAZY_DUNES_VICTORY,
                "E3C1" => LOC_PALEROCK_LAKE_VICTORY,
                "E1C1" => LOC_CITY_SLICKER_VICTORY,
                "E3C2" => LOC_COUNTRY_BUMPKIN_VICTORY,
                "E2C2" => LOC_SPOOKY_MANOR_VICTORY,
                "E1C2" => LOC_MALLY_MARKET_VICTORY,
                "E4C2" => LOC_VALLEY_OF_THE_KINGS_VICTORY,
                "E1C3" => LOC_MISTY_FOR_ME_VICTORY,
                "E3C3" => LOC_SNEAK_A_PEAK_VICTORY,
                "E4C3" => LOC_BLAZING_OASIS_VICTORY,
                "E2C3" => LOC_PASTACOSI_FACTORY_VICTORY,
                "E4C4" => LOC_MYSTERIOUS_TEMPLE_VICTORY,
                "E1C4" => LOC_PROHIBITED_SITE_VICTORY,
                "E2C4" => LOC_CASKOU_PARK_VICTORY,
                "E3C4" => LOC_LOOPY_LAGOON_VICTORY,
                _ => -1
            };
        }

        // A win on a higher CC also counts as a win on every lower CC
        public static List<long> GetRaceVictoryCCLocs(string startScene, Difficulty difficulty)
        {
            var returnedList = new List<long>();
            long baseLoc = GetRaceVictoryLoc(startScene);
            if (baseLoc == -1) return returnedList;
            for (int cc = 0; cc <= (int)difficulty; cc++)
            {
                returnedList.Add(LOC_RACE_VICTORY_CC_BASE + cc * LOC_RACE_VICTORY_CC_GAP + baseLoc);
            }
            return returnedList;
        }

        public static long GetCupVictoryLoc(int cupId)
        {
            if (cupId < 0 || cupId > 3) return -1;
            return LOC_LASAGNA_CUP_VICTORY + cupId;
        }

        public static List<long> GetCupVictoryCCLocs(int cupId, Difficulty difficulty)
        {
            var returnedList = new List<long>();
            if (cupId < 0 || cupId > 3) return returnedList;
            for (int cc = 0; cc <= (int)difficulty; cc++)
            {
                returnedList.Add(LOC_CUP_VICTORY_CC_BASE + cc * LOC_CUP_VICTORY_CC_GAP + (cupId + 1));
            }
            return returnedList;
        }

        public static long GetPuzzlePieceLoc(string startScene, int puzzleIndex)
        {
            return startScene switch
            {
                // LASAGNA CUP
                "E2C1" => LOC_CATZ_IN_THE_HOOD_PUZZLE_PIECE_1 + puzzleIndex,
                "E4C1" => LOC_CRAZY_DUNES_PUZZLE_PIECE_1 + puzzleIndex,
                "E3C1" => LOC_PALEROCK_LAKE_PUZZLE_PIECE_1 + puzzleIndex,
                "E1C1" => LOC_CITY_SLICKER_PUZZLE_PIECE_1 + puzzleIndex,
                // PIZZA CUP
                "E3C2" => LOC_COUNTRY_BUMPKIN_PUZZLE_PIECE_1 + puzzleIndex,
                "E2C2" => LOC_SPOOKY_MANOR_PUZZLE_PIECE_1 + puzzleIndex,
                "E1C2" => LOC_MALLY_MARKET_PUZZLE_PIECE_1 + puzzleIndex,
                "E4C2" => LOC_VALLEY_OF_THE_KINGS_PUZZLE_PIECE_1 + puzzleIndex,
                // BURGER CUP
                "E1C3" => LOC_MISTY_FOR_ME_PUZZLE_PIECE_1 + puzzleIndex,
                "E3C3" => LOC_SNEAK_A_PEAK_PUZZLE_PIECE_1 + puzzleIndex,
                "E4C3" => LOC_BLAZING_OASIS_PUZZLE_PIECE_1 + puzzleIndex,
                "E2C3" => LOC_PASTACOSI_FACTORY_PUZZLE_PIECE_1 + puzzleIndex,
                // ICE CREAM CUP
                "E4C4" => LOC_MYSTERIOUS_TEMPLE_PUZZLE_PIECE_1 + puzzleIndex,
                "E1C4" => LOC_PROHIBITED_SITE_PUZZLE_PIECE_1 + puzzleIndex,
                "E2C4" => LOC_CASKOU_PARK_PUZZLE_PIECE_1 + puzzleIndex,
                "E3C4" => LOC_LOOPY_LAGOON_PUZZLE_PIECE_1 + puzzleIndex,
                _ => -1
            };
        }

        public static long GetLapSanityLoc(string startScene, int lapIndex)
        {
            long baseId = startScene switch
            {
                // LASAGNA CUP
                "E2C1" => LOC_CATZ_IN_THE_HOOD_LAP_SANITY,
                "E4C1" => LOC_CRAZY_DUNES_LAP_SANITY,
                "E3C1" => LOC_PALEROCK_LAKE_LAP_SANITY,
                "E1C1" => LOC_CITY_SLICKER_LAP_SANITY,
                // PIZZA CUP
                "E3C2" => LOC_COUNTRY_BUMPKIN_LAP_SANITY,
                "E2C2" => LOC_SPOOKY_MANOR_LAP_SANITY,
                "E1C2" => LOC_MALLY_MARKET_LAP_SANITY,
                "E4C2" => LOC_VALLEY_OF_THE_KINGS_LAP_SANITY,
                // BURGER CUP
                "E1C3" => LOC_MISTY_FOR_ME_LAP_SANITY,
                "E3C3" => LOC_SNEAK_A_PEAK_LAP_SANITY,
                "E4C3" => LOC_BLAZING_OASIS_LAP_SANITY,
                "E2C3" => LOC_PASTACOSI_FACTORY_LAP_SANITY,
                // ICE CREAM CUP
                "E4C4" => LOC_MYSTERIOUS_TEMPLE_LAP_SANITY,
                "E1C4" => LOC_PROHIBITED_SITE_LAP_SANITY,
                "E2C4" => LOC_CASKOU_PARK_LAP_SANITY,
                "E3C4" => LOC_LOOPY_LAGOON_LAP_SANITY,
                _ => -1
            };
            if (baseId == -1) return -1;
            return baseId + lapIndex;
        }

        public static List<long> GetTimeTrialLocs(string startScene, E_TimeTrialMedal medal)
        {
            int diffIndex = (int)medal;
            var returnedList = new List<long>();

            if (diffIndex == 0)
            {
                return returnedList;
            }

            switch (startScene)
            {
                // LASAGNA CUP
                case "E2C1":
                    if (diffIndex >= 1) returnedList.Add(LOC_CATZ_IN_THE_HOOD_TIME_TRIAL_BRONZE);
                    if (diffIndex >= 2) returnedList.Add(LOC_CATZ_IN_THE_HOOD_TIME_TRIAL_SILVER);
                    if (diffIndex >= 3) returnedList.Add(LOC_CATZ_IN_THE_HOOD_TIME_TRIAL_GOLD);
                    if (diffIndex >= 4) returnedList.Add(LOC_CATZ_IN_THE_HOOD_TIME_TRIAL_PLATINUM);
                    break;
                case "E4C1":
                    if (diffIndex >= 1) returnedList.Add(LOC_CRAZY_DUNES_TIME_TRIAL_BRONZE);
                    if (diffIndex >= 2) returnedList.Add(LOC_CRAZY_DUNES_TIME_TRIAL_SILVER);
                    if (diffIndex >= 3) returnedList.Add(LOC_CRAZY_DUNES_TIME_TRIAL_GOLD);
                    if (diffIndex >= 4) returnedList.Add(LOC_CRAZY_DUNES_TIME_TRIAL_PLATINUM);
                    break;
                case "E3C1":
                    if (diffIndex >= 1) returnedList.Add(LOC_PALEROCK_LAKE_TIME_TRIAL_BRONZE);
                    if (diffIndex >= 2) returnedList.Add(LOC_PALEROCK_LAKE_TIME_TRIAL_SILVER);
                    if (diffIndex >= 3) returnedList.Add(LOC_PALEROCK_LAKE_TIME_TRIAL_GOLD);
                    if (diffIndex >= 4) returnedList.Add(LOC_PALEROCK_LAKE_TIME_TRIAL_PLATINUM);
                    break;
                case "E1C1":
                    if (diffIndex >= 1) returnedList.Add(LOC_CITY_SLICKER_TIME_TRIAL_BRONZE);
                    if (diffIndex >= 2) returnedList.Add(LOC_CITY_SLICKER_TIME_TRIAL_SILVER);
                    if (diffIndex >= 3) returnedList.Add(LOC_CITY_SLICKER_TIME_TRIAL_GOLD);
                    if (diffIndex >= 4) returnedList.Add(LOC_CITY_SLICKER_TIME_TRIAL_PLATINUM);
                    break;

                // PIZZA CUP
                case "E3C2":
                    if (diffIndex >= 1) returnedList.Add(LOC_COUNTRY_BUMPKIN_TIME_TRIAL_BRONZE);
                    if (diffIndex >= 2) returnedList.Add(LOC_COUNTRY_BUMPKIN_TIME_TRIAL_SILVER);
                    if (diffIndex >= 3) returnedList.Add(LOC_COUNTRY_BUMPKIN_TIME_TRIAL_GOLD);
                    if (diffIndex >= 4) returnedList.Add(LOC_COUNTRY_BUMPKIN_TIME_TRIAL_PLATINUM);
                    break;
                case "E2C2":
                    if (diffIndex >= 1) returnedList.Add(LOC_SPOOKY_MANOR_TIME_TRIAL_BRONZE);
                    if (diffIndex >= 2) returnedList.Add(LOC_SPOOKY_MANOR_TIME_TRIAL_SILVER);
                    if (diffIndex >= 3) returnedList.Add(LOC_SPOOKY_MANOR_TIME_TRIAL_GOLD);
                    if (diffIndex >= 4) returnedList.Add(LOC_SPOOKY_MANOR_TIME_TRIAL_PLATINUM);
                    break;
                case "E1C2":
                    if (diffIndex >= 1) returnedList.Add(LOC_MALLY_MARKET_TIME_TRIAL_BRONZE);
                    if (diffIndex >= 2) returnedList.Add(LOC_MALLY_MARKET_TIME_TRIAL_SILVER);
                    if (diffIndex >= 3) returnedList.Add(LOC_MALLY_MARKET_TIME_TRIAL_GOLD);
                    if (diffIndex >= 4) returnedList.Add(LOC_MALLY_MARKET_TIME_TRIAL_PLATINUM);
                    break;
                case "E4C2":
                    if (diffIndex >= 1) returnedList.Add(LOC_VALLEY_OF_THE_KINGS_TIME_TRIAL_BRONZE);
                    if (diffIndex >= 2) returnedList.Add(LOC_VALLEY_OF_THE_KINGS_TIME_TRIAL_SILVER);
                    if (diffIndex >= 3) returnedList.Add(LOC_VALLEY_OF_THE_KINGS_TIME_TRIAL_GOLD);
                    if (diffIndex >= 4) returnedList.Add(LOC_VALLEY_OF_THE_KINGS_TIME_TRIAL_PLATINUM);
                    break;

                // BURGER CUP
                case "E1C3":
                    if (diffIndex >= 1) returnedList.Add(LOC_MISTY_FOR_ME_TIME_TRIAL_BRONZE);
                    if (diffIndex >= 2) returnedList.Add(LOC_MISTY_FOR_ME_TIME_TRIAL_SILVER);
                    if (diffIndex >= 3) returnedList.Add(LOC_MISTY_FOR_ME_TIME_TRIAL_GOLD);
                    if (diffIndex >= 4) returnedList.Add(LOC_MISTY_FOR_ME_TIME_TRIAL_PLATINUM);
                    break;
                case "E3C3":
                    if (diffIndex >= 1) returnedList.Add(LOC_SNEAK_A_PEAK_TIME_TRIAL_BRONZE);
                    if (diffIndex >= 2) returnedList.Add(LOC_SNEAK_A_PEAK_TIME_TRIAL_SILVER);
                    if (diffIndex >= 3) returnedList.Add(LOC_SNEAK_A_PEAK_TIME_TRIAL_GOLD);
                    if (diffIndex >= 4) returnedList.Add(LOC_SNEAK_A_PEAK_TIME_TRIAL_PLATINUM);
                    break;
                case "E4C3":
                    if (diffIndex >= 1) returnedList.Add(LOC_BLAZING_OASIS_TIME_TRIAL_BRONZE);
                    if (diffIndex >= 2) returnedList.Add(LOC_BLAZING_OASIS_TIME_TRIAL_SILVER);
                    if (diffIndex >= 3) returnedList.Add(LOC_BLAZING_OASIS_TIME_TRIAL_GOLD);
                    if (diffIndex >= 4) returnedList.Add(LOC_BLAZING_OASIS_TIME_TRIAL_PLATINUM);
                    break;
                case "E2C3":
                    if (diffIndex >= 1) returnedList.Add(LOC_PASTACOSI_FACTORY_TIME_TRIAL_BRONZE);
                    if (diffIndex >= 2) returnedList.Add(LOC_PASTACOSI_FACTORY_TIME_TRIAL_SILVER);
                    if (diffIndex >= 3) returnedList.Add(LOC_PASTACOSI_FACTORY_TIME_TRIAL_GOLD);
                    if (diffIndex >= 4) returnedList.Add(LOC_PASTACOSI_FACTORY_TIME_TRIAL_PLATINUM);
                    break;

                // ICE CREAM CUP
                case "E4C4":
                    if (diffIndex >= 1) returnedList.Add(LOC_MYSTERIOUS_TEMPLE_TIME_TRIAL_BRONZE);
                    if (diffIndex >= 2) returnedList.Add(LOC_MYSTERIOUS_TEMPLE_TIME_TRIAL_SILVER);
                    if (diffIndex >= 3) returnedList.Add(LOC_MYSTERIOUS_TEMPLE_TIME_TRIAL_GOLD);
                    if (diffIndex >= 4) returnedList.Add(LOC_MYSTERIOUS_TEMPLE_TIME_TRIAL_PLATINUM);
                    break;
                case "E1C4":
                    if (diffIndex >= 1) returnedList.Add(LOC_PROHIBITED_SITE_TIME_TRIAL_BRONZE);
                    if (diffIndex >= 2) returnedList.Add(LOC_PROHIBITED_SITE_TIME_TRIAL_SILVER);
                    if (diffIndex >= 3) returnedList.Add(LOC_PROHIBITED_SITE_TIME_TRIAL_GOLD);
                    if (diffIndex >= 4) returnedList.Add(LOC_PROHIBITED_SITE_TIME_TRIAL_PLATINUM);
                    break;
                case "E2C4":
                    if (diffIndex >= 1) returnedList.Add(LOC_CASKOU_PARK_TIME_TRIAL_BRONZE);
                    if (diffIndex >= 2) returnedList.Add(LOC_CASKOU_PARK_TIME_TRIAL_SILVER);
                    if (diffIndex >= 3) returnedList.Add(LOC_CASKOU_PARK_TIME_TRIAL_GOLD);
                    if (diffIndex >= 4) returnedList.Add(LOC_CASKOU_PARK_TIME_TRIAL_PLATINUM);
                    break;
                case "E3C4":
                    if (diffIndex >= 1) returnedList.Add(LOC_LOOPY_LAGOON_TIME_TRIAL_BRONZE);
                    if (diffIndex >= 2) returnedList.Add(LOC_LOOPY_LAGOON_TIME_TRIAL_SILVER);
                    if (diffIndex >= 3) returnedList.Add(LOC_LOOPY_LAGOON_TIME_TRIAL_GOLD);
                    if (diffIndex >= 4) returnedList.Add(LOC_LOOPY_LAGOON_TIME_TRIAL_PLATINUM);
                    break;
            }

            return returnedList;
        }

        public static long GetHatLoc(string startScene)
        {
            long victoryLoc = GetRaceVictoryLoc(startScene);
            if (victoryLoc == -1) return -1;
            return LOC_CATZ_IN_THE_HOOD_HAT_UNLOCK - LOC_CATZ_IN_THE_HOOD_VICTORY + victoryLoc;
        }

        public static long GetHatItemId(string hat)
        {
            switch (hat)
            {
                // LASAGNA CUP
                // CATZ IN THE HOOD
                case "EgyptPriestHatN":
                case "EgyptPriestHatR":
                case "EgyptPriestHatU": return ITEM_UNLOCK_TIC_TOQUE;
                // CRAZY DUNES
                case "SleepingHatN":
                case "SleepingHatR":
                case "SleepingHatU": return ITEM_UNLOCK_BEDDY_BYE_CAP;
                // PALEROCK LAKE
                case "PharaonHatN":
                case "PharaonHatR":
                case "PharaonHatU": return ITEM_UNLOCK_TOUTANKHAMEOW;
                // CITY SLICKER
                case "BeautyHatN":
                case "BeautyHatR":
                case "BeautyHatU": return ITEM_UNLOCK_ARISTO_CATIC_BICORN;

                // PIZZA CUP
                // COUNTRY BUMPKIN
                case "PiratHatN":
                case "PiratHatR":
                case "PiratHatU": return ITEM_UNLOCK_STINK_O_RAMA;
                // SPOOKY MANOR
                case "FootballHelmetN":
                case "FootballHelmetR":
                case "FootballHelmetU": return ITEM_UNLOCK_JOE_MONTAGNA;
                // MALLY MARKET
                case "ChickenHatN":
                case "ChickenHatR":
                case "ChickenHatU": return ITEM_UNLOCK_ELASTO_HAT;
                // VALLEY OF THE KINGS
                case "SpaceHelmetN":
                case "SpaceHelmetR":
                case "SpaceHelmetU": return ITEM_UNLOCK_SPACE_BUBBLE;

                // BURGER CUP
                // PLAY MISTY FOR ME
                case "CrownHatN":
                case "CrownHatR":
                case "CrownHatU": return ITEM_UNLOCK_CUTIE_PIE_CROWN;
                // SNEAK-A-PEAK
                case "PizzaioloHatN":
                case "PizzaioloHatR":
                case "PizzaioloHatU": return ITEM_UNLOCK_PIZZAIOLO_HAT;
                // BLAZING OASIS
                case "VikingHelmetN":
                case "VikingHelmetR":
                case "VikingHelmetU": return ITEM_UNLOCK_VIKING_HELMET;
                // PASTACOSI FACTORY
                case "MagicHatN":
                case "MagicHatR":
                case "MagicHatU": return ITEM_UNLOCK_WHIZZY_WIZARD;

                // ICE CREAM CUP
                // MYSTERIOUS TEMPLE
                case "WizardHatN":
                case "WizardHatR":
                case "WizardHatU": return ITEM_UNLOCK_APPRENTICE_SORCERER;
                // PROHIBITED SITE
                case "DunkeyHatN":
                case "DunkeyHatR":
                case "DunkeyHatU": return ITEM_UNLOCK_MULE_HEAD;
                // CASKOU PARK
                case "PastryHatN":
                case "PastryHatR":
                case "PastryHatU": return ITEM_UNLOCK_CHEFS_SPECIAL;
                // LOOPY LAGOON
                case "RabbitHatN":
                case "RabbitHatR":
                case "RabbitHatU": return ITEM_UNLOCK_BUNNY_BAND;

                default: return -1;
            }
        }

        public static List<long> GetSpoilerLocs(int cupId)
        {
            var returnedList = new List<long>();
            if (cupId < 0 || cupId > 3) return returnedList;
            returnedList.Add(LOC_LASAGNA_CUP_UNLOCK_SPOILER_1 + cupId);
            returnedList.Add(LOC_LASAGNA_CUP_UNLOCK_SPOILER_2 + cupId);
            return returnedList;
        }

        public static long GetSpoilerItemId(string custom)
        {
            switch (custom)
            {
                // LASAGNA CUP
                case "KGC_ManiabilityN":
                case "KGC_ManiabilityR":
                case "KGC_ManiabilityU": return ITEM_UNLOCK_BOMBASTIC_SPOILER;

                case "KJC_SpeedN":
                case "KJC_SpeedR":
                case "KJC_SpeedU": return ITEM_UNLOCK_WHACKY_SPOILER;

                // PIZZA CUP
                case "KLC_AccelerationN":
                case "KLC_AccelerationR":
                case "KLC_AccelerationU": return ITEM_UNLOCK_SUPERFIT_SPOILER;

                case "KOC_AccelerationN":
                case "KOC_AccelerationR":
                case "KOC_AccelerationU": return ITEM_UNLOCK_CYCLOBONE_SPOILER;

                // BURGER CUP
                case "KAC_AccelerationN":
                case "KAC_AccelerationR":
                case "KAC_AccelerationU": return ITEM_UNLOCK_FOXY_SPOILER;

                case "KNC_AccelerationN":
                case "KNC_AccelerationR":
                case "KNC_AccelerationU": return ITEM_UNLOCK_SHIMMERING_SPOILER;

                // ICE CREAM CUP
                case "KSC_ManiabilityN":
                case "KSC_ManiabilityR":
                case "KSC_ManiabilityU": return ITEM_UNLOCK_HOLEY_MOLEY_SPOILER;

                case "KHC_SpeedN":
                case "KHC_SpeedR":
                case "KHC_SpeedU": return ITEM_UNLOCK_STAINED_SPOILER;

                default: return -1;
            }
        }
    }
}