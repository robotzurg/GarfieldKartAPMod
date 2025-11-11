using System.Runtime.Remoting.Messaging;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine.SceneManagement;

namespace GarfieldKartAPMod
{
    public static class ArchipelagoConstants
    {
        // ========== LOCATION CONSTANTS (what you check in-game) ==========
        // These are the location IDs that get sent when you accomplish something

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

        // Cup Unlock Spoiler Locations (301-374)
        // Combined tier spoiler locations (301-314)
        public const long LOC_LASAGNA_CUP_UNLOCK_SPOILER_1 = 301;
        public const long LOC_PIZZA_CUP_UNLOCK_SPOILER_1 = 302;
        public const long LOC_BURGER_CUP_UNLOCK_SPOILER_1 = 303;
        public const long LOC_ICE_CREAM_CUP_UNLOCK_SPOILER_1 = 304;
        public const long LOC_LASAGNA_CUP_UNLOCK_SPOILER_2 = 311;
        public const long LOC_PIZZA_CUP_UNLOCK_SPOILER_2 = 312;
        public const long LOC_BURGER_CUP_UNLOCK_SPOILER_2 = 313;
        public const long LOC_ICE_CREAM_CUP_UNLOCK_SPOILER_2 = 314;

        // Bronze spoiler locations (321-334)
        public const long LOC_LASAGNA_CUP_UNLOCK_BRONZE_SPOILER_1 = 321;
        public const long LOC_PIZZA_CUP_UNLOCK_BRONZE_SPOILER_1 = 322;
        public const long LOC_BURGER_CUP_UNLOCK_BRONZE_SPOILER_1 = 323;
        public const long LOC_ICE_CREAM_CUP_UNLOCK_BRONZE_SPOILER_1 = 324;
        public const long LOC_LASAGNA_CUP_UNLOCK_BRONZE_SPOILER_2 = 331;
        public const long LOC_PIZZA_CUP_UNLOCK_BRONZE_SPOILER_2 = 332;
        public const long LOC_BURGER_CUP_UNLOCK_BRONZE_SPOILER_2 = 333;
        public const long LOC_ICE_CREAM_CUP_UNLOCK_BRONZE_SPOILER_2 = 334;

        // Silver spoiler locations (341-354)
        public const long LOC_LASAGNA_CUP_UNLOCK_SILVER_SPOILER_1 = 341;
        public const long LOC_PIZZA_CUP_UNLOCK_SILVER_SPOILER_1 = 342;
        public const long LOC_BURGER_CUP_UNLOCK_SILVER_SPOILER_1 = 343;
        public const long LOC_ICE_CREAM_CUP_UNLOCK_SILVER_SPOILER_1 = 344;
        public const long LOC_LASAGNA_CUP_UNLOCK_SILVER_SPOILER_2 = 351;
        public const long LOC_PIZZA_CUP_UNLOCK_SILVER_SPOILER_2 = 352;
        public const long LOC_BURGER_CUP_UNLOCK_SILVER_SPOILER_2 = 353;
        public const long LOC_ICE_CREAM_CUP_UNLOCK_SILVER_SPOILER_2 = 354;

        // Gold spoiler locations (361-374)
        public const long LOC_LASAGNA_CUP_UNLOCK_GOLD_SPOILER_1 = 361;
        public const long LOC_PIZZA_CUP_UNLOCK_GOLD_SPOILER_1 = 362;
        public const long LOC_BURGER_CUP_UNLOCK_GOLD_SPOILER_1 = 363;
        public const long LOC_ICE_CREAM_CUP_UNLOCK_GOLD_SPOILER_1 = 364;
        public const long LOC_LASAGNA_CUP_UNLOCK_GOLD_SPOILER_2 = 371;
        public const long LOC_PIZZA_CUP_UNLOCK_GOLD_SPOILER_2 = 372;
        public const long LOC_BURGER_CUP_UNLOCK_GOLD_SPOILER_2 = 373;
        public const long LOC_ICE_CREAM_CUP_UNLOCK_GOLD_SPOILER_2 = 374;

        // Hat Unlock Locations (401-416) - Combined tier
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

        // Hat Bronze Unlock Locations (421-436)
        public const long LOC_CATZ_IN_THE_HOOD_BRONZE_HAT_UNLOCK = 421;
        public const long LOC_CRAZY_DUNES_BRONZE_HAT_UNLOCK = 422;
        public const long LOC_PALEROCK_LAKE_BRONZE_HAT_UNLOCK = 423;
        public const long LOC_CITY_SLICKER_BRONZE_HAT_UNLOCK = 424;
        public const long LOC_COUNTRY_BUMPKIN_BRONZE_HAT_UNLOCK = 425;
        public const long LOC_SPOOKY_MANOR_BRONZE_HAT_UNLOCK = 426;
        public const long LOC_MALLY_MARKET_BRONZE_HAT_UNLOCK = 427;
        public const long LOC_VALLEY_OF_THE_KINGS_BRONZE_HAT_UNLOCK = 428;
        public const long LOC_MISTY_FOR_ME_BRONZE_HAT_UNLOCK = 429;
        public const long LOC_SNEAK_A_PEAK_BRONZE_HAT_UNLOCK = 430;
        public const long LOC_BLAZING_OASIS_BRONZE_HAT_UNLOCK = 431;
        public const long LOC_PASTACOSI_FACTORY_BRONZE_HAT_UNLOCK = 432;
        public const long LOC_MYSTERIOUS_TEMPLE_BRONZE_HAT_UNLOCK = 433;
        public const long LOC_PROHIBITED_SITE_BRONZE_HAT_UNLOCK = 434;
        public const long LOC_CASKOU_PARK_BRONZE_HAT_UNLOCK = 435;
        public const long LOC_LOOPY_LAGOON_BRONZE_HAT_UNLOCK = 436;

        // Hat Silver Unlock Locations (441-456)
        public const long LOC_CATZ_IN_THE_HOOD_SILVER_HAT_UNLOCK = 441;
        public const long LOC_CRAZY_DUNES_SILVER_HAT_UNLOCK = 442;
        public const long LOC_PALEROCK_LAKE_SILVER_HAT_UNLOCK = 443;
        public const long LOC_CITY_SLICKER_SILVER_HAT_UNLOCK = 444;
        public const long LOC_COUNTRY_BUMPKIN_SILVER_HAT_UNLOCK = 445;
        public const long LOC_SPOOKY_MANOR_SILVER_HAT_UNLOCK = 446;
        public const long LOC_MALLY_MARKET_SILVER_HAT_UNLOCK = 447;
        public const long LOC_VALLEY_OF_THE_KINGS_SILVER_HAT_UNLOCK = 448;
        public const long LOC_MISTY_FOR_ME_SILVER_HAT_UNLOCK = 449;
        public const long LOC_SNEAK_A_PEAK_SILVER_HAT_UNLOCK = 450;
        public const long LOC_BLAZING_OASIS_SILVER_HAT_UNLOCK = 451;
        public const long LOC_PASTACOSI_FACTORY_SILVER_HAT_UNLOCK = 452;
        public const long LOC_MYSTERIOUS_TEMPLE_SILVER_HAT_UNLOCK = 453;
        public const long LOC_PROHIBITED_SITE_SILVER_HAT_UNLOCK = 454;
        public const long LOC_CASKOU_PARK_SILVER_HAT_UNLOCK = 455;
        public const long LOC_LOOPY_LAGOON_SILVER_HAT_UNLOCK = 456;

        // Hat Gold Unlock Locations (461-476)
        public const long LOC_CATZ_IN_THE_HOOD_GOLD_HAT_UNLOCK = 461;
        public const long LOC_CRAZY_DUNES_GOLD_HAT_UNLOCK = 462;
        public const long LOC_PALEROCK_LAKE_GOLD_HAT_UNLOCK = 463;
        public const long LOC_CITY_SLICKER_GOLD_HAT_UNLOCK = 464;
        public const long LOC_COUNTRY_BUMPKIN_GOLD_HAT_UNLOCK = 465;
        public const long LOC_SPOOKY_MANOR_GOLD_HAT_UNLOCK = 466;
        public const long LOC_MALLY_MARKET_GOLD_HAT_UNLOCK = 467;
        public const long LOC_VALLEY_OF_THE_KINGS_GOLD_HAT_UNLOCK = 468;
        public const long LOC_MISTY_FOR_ME_GOLD_HAT_UNLOCK = 469;
        public const long LOC_SNEAK_A_PEAK_GOLD_HAT_UNLOCK = 470;
        public const long LOC_BLAZING_OASIS_GOLD_HAT_UNLOCK = 471;
        public const long LOC_PASTACOSI_FACTORY_GOLD_HAT_UNLOCK = 472;
        public const long LOC_MYSTERIOUS_TEMPLE_GOLD_HAT_UNLOCK = 473;
        public const long LOC_PROHIBITED_SITE_GOLD_HAT_UNLOCK = 474;
        public const long LOC_CASKOU_PARK_GOLD_HAT_UNLOCK = 475;
        public const long LOC_LOOPY_LAGOON_GOLD_HAT_UNLOCK = 476;

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

        // Puzzle Pieces as Items (1-48)
        public const long ITEM_CATZ_IN_THE_HOOD_PUZZLE_PIECE_1 = 1;
        public const long ITEM_CATZ_IN_THE_HOOD_PUZZLE_PIECE_2 = 2;
        public const long ITEM_CATZ_IN_THE_HOOD_PUZZLE_PIECE_3 = 3;
        public const long ITEM_CRAZY_DUNES_PUZZLE_PIECE_1 = 4;
        public const long ITEM_CRAZY_DUNES_PUZZLE_PIECE_2 = 5;
        public const long ITEM_CRAZY_DUNES_PUZZLE_PIECE_3 = 6;
        public const long ITEM_PALEROCK_LAKE_PUZZLE_PIECE_1 = 7;
        public const long ITEM_PALEROCK_LAKE_PUZZLE_PIECE_2 = 8;
        public const long ITEM_PALEROCK_LAKE_PUZZLE_PIECE_3 = 9;
        public const long ITEM_CITY_SLICKER_PUZZLE_PIECE_1 = 10;
        public const long ITEM_CITY_SLICKER_PUZZLE_PIECE_2 = 11;
        public const long ITEM_CITY_SLICKER_PUZZLE_PIECE_3 = 12;
        public const long ITEM_COUNTRY_BUMPKIN_PUZZLE_PIECE_1 = 13;
        public const long ITEM_COUNTRY_BUMPKIN_PUZZLE_PIECE_2 = 14;
        public const long ITEM_COUNTRY_BUMPKIN_PUZZLE_PIECE_3 = 15;
        public const long ITEM_SPOOKY_MANOR_PUZZLE_PIECE_1 = 16;
        public const long ITEM_SPOOKY_MANOR_PUZZLE_PIECE_2 = 17;
        public const long ITEM_SPOOKY_MANOR_PUZZLE_PIECE_3 = 18;
        public const long ITEM_MALLY_MARKET_PUZZLE_PIECE_1 = 19;
        public const long ITEM_MALLY_MARKET_PUZZLE_PIECE_2 = 20;
        public const long ITEM_MALLY_MARKET_PUZZLE_PIECE_3 = 21;
        public const long ITEM_VALLEY_OF_THE_KINGS_PUZZLE_PIECE_1 = 22;
        public const long ITEM_VALLEY_OF_THE_KINGS_PUZZLE_PIECE_2 = 23;
        public const long ITEM_VALLEY_OF_THE_KINGS_PUZZLE_PIECE_3 = 24;
        public const long ITEM_MISTY_FOR_ME_PUZZLE_PIECE_1 = 25;
        public const long ITEM_MISTY_FOR_ME_PUZZLE_PIECE_2 = 26;
        public const long ITEM_MISTY_FOR_ME_PUZZLE_PIECE_3 = 27;
        public const long ITEM_SNEAK_A_PEAK_PUZZLE_PIECE_1 = 28;
        public const long ITEM_SNEAK_A_PEAK_PUZZLE_PIECE_2 = 29;
        public const long ITEM_SNEAK_A_PEAK_PUZZLE_PIECE_3 = 30;
        public const long ITEM_BLAZING_OASIS_PUZZLE_PIECE_1 = 31;
        public const long ITEM_BLAZING_OASIS_PUZZLE_PIECE_2 = 32;
        public const long ITEM_BLAZING_OASIS_PUZZLE_PIECE_3 = 33;
        public const long ITEM_PASTACOSI_FACTORY_PUZZLE_PIECE_1 = 34;
        public const long ITEM_PASTACOSI_FACTORY_PUZZLE_PIECE_2 = 35;
        public const long ITEM_PASTACOSI_FACTORY_PUZZLE_PIECE_3 = 36;
        public const long ITEM_MYSTERIOUS_TEMPLE_PUZZLE_PIECE_1 = 37;
        public const long ITEM_MYSTERIOUS_TEMPLE_PUZZLE_PIECE_2 = 38;
        public const long ITEM_MYSTERIOUS_TEMPLE_PUZZLE_PIECE_3 = 39;
        public const long ITEM_PROHIBITED_SITE_PUZZLE_PIECE_1 = 40;
        public const long ITEM_PROHIBITED_SITE_PUZZLE_PIECE_2 = 41;
        public const long ITEM_PROHIBITED_SITE_PUZZLE_PIECE_3 = 42;
        public const long ITEM_CASKOU_PARK_PUZZLE_PIECE_1 = 43;
        public const long ITEM_CASKOU_PARK_PUZZLE_PIECE_2 = 44;
        public const long ITEM_CASKOU_PARK_PUZZLE_PIECE_3 = 45;
        public const long ITEM_LOOPY_LAGOON_PUZZLE_PIECE_1 = 46;
        public const long ITEM_LOOPY_LAGOON_PUZZLE_PIECE_2 = 47;
        public const long ITEM_LOOPY_LAGOON_PUZZLE_PIECE_3 = 48;

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

        // Time Trial Unlocks (150-166)
        public const long ITEM_PROGRESSIVE_TIME_TRIAL_UNLOCK = 150;
        public const long ITEM_TIME_TRIAL_UNLOCK_CATZ_IN_THE_HOOD = 151;
        public const long ITEM_TIME_TRIAL_UNLOCK_CRAZY_DUNES = 152;
        public const long ITEM_TIME_TRIAL_UNLOCK_PALEROCK_LAKE = 153;
        public const long ITEM_TIME_TRIAL_UNLOCK_CITY_SLICKER = 154;
        public const long ITEM_TIME_TRIAL_UNLOCK_COUNTRY_BUMPKIN = 155;
        public const long ITEM_TIME_TRIAL_UNLOCK_SPOOKY_MANOR = 156;
        public const long ITEM_TIME_TRIAL_UNLOCK_MALLY_MARKET = 157;
        public const long ITEM_TIME_TRIAL_UNLOCK_VALLEY_OF_THE_KINGS = 158;
        public const long ITEM_TIME_TRIAL_UNLOCK_MISTY_FOR_ME = 159;
        public const long ITEM_TIME_TRIAL_UNLOCK_SNEAK_A_PEAK = 160;
        public const long ITEM_TIME_TRIAL_UNLOCK_BLAZING_OASIS = 161;
        public const long ITEM_TIME_TRIAL_UNLOCK_PASTACOSI_FACTORY = 162;
        public const long ITEM_TIME_TRIAL_UNLOCK_MYSTERIOUS_TEMPLE = 163;
        public const long ITEM_TIME_TRIAL_UNLOCK_PROHIBITED_SITE = 164;
        public const long ITEM_TIME_TRIAL_UNLOCK_CASKOU_PARK = 165;
        public const long ITEM_TIME_TRIAL_UNLOCK_LOOPY_LAGOON = 166;

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

        // Car Unlocks (351-358)
        public const long ITEM_CAR_FORMULA_ZZZZ = 351;
        public const long ITEM_CAR_ABSTRACT_KART = 352;
        public const long ITEM_CAR_MEDI_KART = 353;
        public const long ITEM_CAR_WOOF_MOBILE = 354;
        public const long ITEM_CAR_KISSY_KART = 355;
        public const long ITEM_CAR_CUTIE_PIE_CAT = 356;
        public const long ITEM_CAR_RAT_RACER = 357;
        public const long ITEM_CAR_MUCK_MADNESS = 358;

        // Progressive Hat Unlocks (401-416)
        public const long ITEM_PROGRESSIVE_BEDDY_BYE_CAP = 401;
        public const long ITEM_PROGRESSIVE_WHIZZY_WIZARD = 402;
        public const long ITEM_PROGRESSIVE_TIC_TOQUE = 403;
        public const long ITEM_PROGRESSIVE_ELASTO_HAT = 404;
        public const long ITEM_PROGRESSIVE_CHEFS_SPECIAL = 405;
        public const long ITEM_PROGRESSIVE_CUTIE_PIE_CROWN = 406;
        public const long ITEM_PROGRESSIVE_VIKING_HELMET = 407;
        public const long ITEM_PROGRESSIVE_STINK_O_RAMA = 408;
        public const long ITEM_PROGRESSIVE_SPACE_BUBBLE = 409;
        public const long ITEM_PROGRESSIVE_PIZZAIOLO_HAT = 410;
        public const long ITEM_PROGRESSIVE_BUNNY_BAND = 411;
        public const long ITEM_PROGRESSIVE_JOE_MONTAGNA = 412;
        public const long ITEM_PROGRESSIVE_ARISTO_CATIC_BICORN = 413;
        public const long ITEM_PROGRESSIVE_TOUTANKHAMEOW = 414;
        public const long ITEM_PROGRESSIVE_APPRENTICE_SORCERER = 415;
        public const long ITEM_PROGRESSIVE_MULE_HEAD = 416;

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

        // Bronze Hat Items (441-456)
        public const long ITEM_BEDDY_BYE_CAP_BRONZE = 441;
        public const long ITEM_WHIZZY_WIZARD_BRONZE = 442;
        public const long ITEM_TIC_TOQUE_BRONZE = 443;
        public const long ITEM_ELASTO_HAT_BRONZE = 444;
        public const long ITEM_CHEFS_SPECIAL_BRONZE = 445;
        public const long ITEM_CUTIE_PIE_CROWN_BRONZE = 446;
        public const long ITEM_VIKING_HELMET_BRONZE = 447;
        public const long ITEM_STINK_O_RAMA_BRONZE = 448;
        public const long ITEM_SPACE_BUBBLE_BRONZE = 449;
        public const long ITEM_PIZZAIOLO_HAT_BRONZE = 450;
        public const long ITEM_BUNNY_BAND_BRONZE = 451;
        public const long ITEM_JOE_MONTAGNA_BRONZE = 452;
        public const long ITEM_ARISTO_CATIC_BICORN_BRONZE = 453;
        public const long ITEM_TOUTANKHAMEOW_BRONZE = 454;
        public const long ITEM_APPRENTICE_SORCERER_BRONZE = 455;
        public const long ITEM_MULE_HEAD_BRONZE = 456;

        // Silver Hat Items (461-476)
        public const long ITEM_BEDDY_BYE_CAP_SILVER = 461;
        public const long ITEM_WHIZZY_WIZARD_SILVER = 462;
        public const long ITEM_TIC_TOQUE_SILVER = 463;
        public const long ITEM_ELASTO_HAT_SILVER = 464;
        public const long ITEM_CHEFS_SPECIAL_SILVER = 465;
        public const long ITEM_CUTIE_PIE_CROWN_SILVER = 466;
        public const long ITEM_VIKING_HELMET_SILVER = 467;
        public const long ITEM_STINK_O_RAMA_SILVER = 468;
        public const long ITEM_SPACE_BUBBLE_SILVER = 469;
        public const long ITEM_PIZZAIOLO_HAT_SILVER = 470;
        public const long ITEM_BUNNY_BAND_SILVER = 471;
        public const long ITEM_JOE_MONTAGNA_SILVER = 472;
        public const long ITEM_ARISTO_CATIC_BICORN_SILVER = 473;
        public const long ITEM_TOUTANKHAMEOW_SILVER = 474;
        public const long ITEM_APPRENTICE_SORCERER_SILVER = 475;
        public const long ITEM_MULE_HEAD_SILVER = 476;

        // Gold Hat Items (481-496)
        public const long ITEM_BEDDY_BYE_CAP_GOLD = 481;
        public const long ITEM_WHIZZY_WIZARD_GOLD = 482;
        public const long ITEM_TIC_TOQUE_GOLD = 483;
        public const long ITEM_ELASTO_HAT_GOLD = 484;
        public const long ITEM_CHEFS_SPECIAL_GOLD = 485;
        public const long ITEM_CUTIE_PIE_CROWN_GOLD = 486;
        public const long ITEM_VIKING_HELMET_GOLD = 487;
        public const long ITEM_STINK_O_RAMA_GOLD = 488;
        public const long ITEM_SPACE_BUBBLE_GOLD = 489;
        public const long ITEM_PIZZAIOLO_HAT_GOLD = 490;
        public const long ITEM_BUNNY_BAND_GOLD = 491;
        public const long ITEM_JOE_MONTAGNA_GOLD = 492;
        public const long ITEM_ARISTO_CATIC_BICORN_GOLD = 493;
        public const long ITEM_TOUTANKHAMEOW_GOLD = 494;
        public const long ITEM_APPRENTICE_SORCERER_GOLD = 495;
        public const long ITEM_MULE_HEAD_GOLD = 496;

        // Progressive Spoiler Unlocks (501-508)
        public const long ITEM_PROGRESSIVE_BOMBASTIC_SPOILER = 501;
        public const long ITEM_PROGRESSIVE_WHACKY_SPOILER = 502;
        public const long ITEM_PROGRESSIVE_SUPERFIT_SPOILER = 503;
        public const long ITEM_PROGRESSIVE_CYCLOBONE_SPOILER = 504;
        public const long ITEM_PROGRESSIVE_FOXY_SPOILER = 505;
        public const long ITEM_PROGRESSIVE_SHIMMERING_SPOILER = 506;
        public const long ITEM_PROGRESSIVE_HOLEY_MOLEY_SPOILER = 507;
        public const long ITEM_PROGRESSIVE_STAINED_SPOILER = 508;

        // Unlock Spoiler Items (521-528)
        public const long ITEM_UNLOCK_BOMBASTIC_SPOILER = 521;
        public const long ITEM_UNLOCK_WHACKY_SPOILER = 522;
        public const long ITEM_UNLOCK_SUPERFIT_SPOILER = 523;
        public const long ITEM_UNLOCK_CYCLOBONE_SPOILER = 524;
        public const long ITEM_UNLOCK_FOXY_SPOILER = 525;
        public const long ITEM_UNLOCK_SHIMMERING_SPOILER = 526;
        public const long ITEM_UNLOCK_HOLEY_MOLEY_SPOILER = 527;
        public const long ITEM_UNLOCK_STAINED_SPOILER = 528;

        // Bronze Spoiler Items (541-548)
        public const long ITEM_BOMBASTIC_SPOILER_BRONZE = 541;
        public const long ITEM_WHACKY_SPOILER_BRONZE = 542;
        public const long ITEM_SUPERFIT_SPOILER_BRONZE = 543;
        public const long ITEM_CYCLOBONE_SPOILER_BRONZE = 544;
        public const long ITEM_FOXY_SPOILER_BRONZE = 545;
        public const long ITEM_SHIMMERING_SPOILER_BRONZE = 546;
        public const long ITEM_HOLEY_MOLEY_SPOILER_BRONZE = 547;
        public const long ITEM_STAINED_SPOILER_BRONZE = 548;

        // Silver Spoiler Items (561-568)
        public const long ITEM_BOMBASTIC_SPOILER_SILVER = 561;
        public const long ITEM_WHACKY_SPOILER_SILVER = 562;
        public const long ITEM_SUPERFIT_SPOILER_SILVER = 563;
        public const long ITEM_CYCLOBONE_SPOILER_SILVER = 564;
        public const long ITEM_FOXY_SPOILER_SILVER = 565;
        public const long ITEM_SHIMMERING_SPOILER_SILVER = 566;
        public const long ITEM_HOLEY_MOLEY_SPOILER_SILVER = 567;
        public const long ITEM_STAINED_SPOILER_SILVER = 568;

        // Gold Spoiler Items (581-588)
        public const long ITEM_BOMBASTIC_SPOILER_GOLD = 581;
        public const long ITEM_WHACKY_SPOILER_GOLD = 582;
        public const long ITEM_SUPERFIT_SPOILER_GOLD = 583;
        public const long ITEM_CYCLOBONE_SPOILER_GOLD = 584;
        public const long ITEM_FOXY_SPOILER_GOLD = 585;
        public const long ITEM_SHIMMERING_SPOILER_GOLD = 586;
        public const long ITEM_HOLEY_MOLEY_SPOILER_GOLD = 587;
        public const long ITEM_STAINED_SPOILER_GOLD = 588;

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
        public const long ITEM_FILLER = 1000;
        public const long ITEM_SPRING_PICKUP = 1001;
        public const long ITEM_PIE_PICKUP = 1002;
        public const long ITEM_RANDOM_BOX = 1003;
        public const long ITEM_START_BOOST_HELPER = 1004;

        // Trap Items (1500+)

        // Goals
        public const long GOAL_GRAND_PRIX = 0;
        public const long GOAL_RACES = 1;
        public const long GOAL_TIME_TRIALS = 2;
        public const long GOAL_PUZZLE_PIECE = 3;

        // ========== HELPER METHODS ==========

        public static string GetSceneNameFromTrackId(TrackId trackId)
        {
            switch (trackId)
            {
                // LASAGNA CUP (Championship 1)
                case TrackId.E2C1: return "E2C1"; // Catz in the Hood
                case TrackId.E4C1: return "E4C1"; // Crazy Dunes
                case TrackId.E3C1: return "E3C1"; // Palerock Lake
                case TrackId.E1C1: return "E1C1"; // City Slicker

                // PIZZA CUP (Championship 2)
                case TrackId.E3C2: return "E3C2"; // Country Bumpkin
                case TrackId.E2C2: return "E2C2"; // Spooky Manor
                case TrackId.E1C2: return "E1C2"; // Mally Market
                case TrackId.E4C2: return "E4C2"; // Valley of the Kings

                // BURGER CUP (Championship 3)
                case TrackId.E1C3: return "E1C3"; // Misty for Me
                case TrackId.E3C3: return "E3C3"; // Sneak a Peak
                case TrackId.E4C3: return "E4C3"; // Blazing Oasis
                case TrackId.E2C3: return "E2C3"; // Pastacosi Factory

                // ICE CREAM CUP (Championship 4)
                case TrackId.E4C4: return "E4C4"; // Mysterious Temple
                case TrackId.E1C4: return "E1C4"; // Prohibited Site
                case TrackId.E2C4: return "E2C4"; // Caskou Park
                case TrackId.E3C4: return "E3C4"; // Loopy Lagoon

                default: return null;
            }
        }

        public static long GetRaceVictoryLoc(string startScene)
        {
            switch (startScene)
            {
                case "E2C1": return LOC_CATZ_IN_THE_HOOD_VICTORY;
                case "E4C1": return LOC_CRAZY_DUNES_VICTORY;
                case "E3C1": return LOC_PALEROCK_LAKE_VICTORY;
                case "E1C1": return LOC_CITY_SLICKER_VICTORY;
                case "E3C2": return LOC_COUNTRY_BUMPKIN_VICTORY;
                case "E2C2": return LOC_SPOOKY_MANOR_VICTORY;
                case "E1C2": return LOC_MALLY_MARKET_VICTORY;
                case "E4C2": return LOC_VALLEY_OF_THE_KINGS_VICTORY;
                case "E1C3": return LOC_MISTY_FOR_ME_VICTORY;
                case "E3C3": return LOC_SNEAK_A_PEAK_VICTORY;
                case "E4C3": return LOC_BLAZING_OASIS_VICTORY;
                case "E2C3": return LOC_PASTACOSI_FACTORY_VICTORY;
                case "E4C4": return LOC_MYSTERIOUS_TEMPLE_VICTORY;
                case "E1C4": return LOC_PROHIBITED_SITE_VICTORY;
                case "E2C4": return LOC_CASKOU_PARK_VICTORY;
                case "E3C4": return LOC_LOOPY_LAGOON_VICTORY;
                default: return -1;
            }
        }

        public static long GetPuzzlePiece(string startScene, int puzzleIndex)
        {
            switch (startScene)
            {
                // LASAGNA CUP
                case "E2C1": return ITEM_CATZ_IN_THE_HOOD_PUZZLE_PIECE_1 + puzzleIndex;
                case "E4C1": return ITEM_CRAZY_DUNES_PUZZLE_PIECE_1 + puzzleIndex;
                case "E3C1": return ITEM_PALEROCK_LAKE_PUZZLE_PIECE_1 + puzzleIndex;
                case "E1C1": return ITEM_CITY_SLICKER_PUZZLE_PIECE_1 + puzzleIndex;
                // PIZZA CUP
                case "E3C2": return ITEM_COUNTRY_BUMPKIN_PUZZLE_PIECE_1 + puzzleIndex;
                case "E2C2": return ITEM_SPOOKY_MANOR_PUZZLE_PIECE_1 + puzzleIndex;
                case "E1C2": return ITEM_MALLY_MARKET_PUZZLE_PIECE_1 + puzzleIndex;
                case "E4C2": return ITEM_VALLEY_OF_THE_KINGS_PUZZLE_PIECE_1 + puzzleIndex;
                // BURGER CUP
                case "E1C3": return ITEM_MISTY_FOR_ME_PUZZLE_PIECE_1 + puzzleIndex;
                case "E3C3": return ITEM_SNEAK_A_PEAK_PUZZLE_PIECE_1 + puzzleIndex;
                case "E4C3": return ITEM_BLAZING_OASIS_PUZZLE_PIECE_1 + puzzleIndex;
                case "E2C3": return ITEM_PASTACOSI_FACTORY_PUZZLE_PIECE_1 + puzzleIndex;
                // ICE CREAM CUP
                case "E4C4": return ITEM_MYSTERIOUS_TEMPLE_PUZZLE_PIECE_1 + puzzleIndex;
                case "E1C4": return ITEM_PROHIBITED_SITE_PUZZLE_PIECE_1 + puzzleIndex;
                case "E2C4": return ITEM_CASKOU_PARK_PUZZLE_PIECE_1 + puzzleIndex;
                case "E3C4": return ITEM_LOOPY_LAGOON_PUZZLE_PIECE_1 + puzzleIndex;
                default: return -1;
            }
        }

        public static long GetPuzzlePieceLoc(string startScene, int puzzleIndex)
        {
            switch (startScene)
            {
                // LASAGNA CUP
                case "E2C1": return LOC_CATZ_IN_THE_HOOD_PUZZLE_PIECE_1 + puzzleIndex;
                case "E4C1": return LOC_CRAZY_DUNES_PUZZLE_PIECE_1 + puzzleIndex;
                case "E3C1": return LOC_PALEROCK_LAKE_PUZZLE_PIECE_1 + puzzleIndex;
                case "E1C1": return LOC_CITY_SLICKER_PUZZLE_PIECE_1 + puzzleIndex;
                // PIZZA CUP
                case "E3C2": return LOC_COUNTRY_BUMPKIN_PUZZLE_PIECE_1 + puzzleIndex;
                case "E2C2": return LOC_SPOOKY_MANOR_PUZZLE_PIECE_1 + puzzleIndex;
                case "E1C2": return LOC_MALLY_MARKET_PUZZLE_PIECE_1 + puzzleIndex;
                case "E4C2": return LOC_VALLEY_OF_THE_KINGS_PUZZLE_PIECE_1 + puzzleIndex;
                // BURGER CUP
                case "E1C3": return LOC_MISTY_FOR_ME_PUZZLE_PIECE_1 + puzzleIndex;
                case "E3C3": return LOC_SNEAK_A_PEAK_PUZZLE_PIECE_1 + puzzleIndex;
                case "E4C3": return LOC_BLAZING_OASIS_PUZZLE_PIECE_1 + puzzleIndex;
                case "E2C3": return LOC_PASTACOSI_FACTORY_PUZZLE_PIECE_1 + puzzleIndex;
                // ICE CREAM CUP
                case "E4C4": return LOC_MYSTERIOUS_TEMPLE_PUZZLE_PIECE_1 + puzzleIndex;
                case "E1C4": return LOC_PROHIBITED_SITE_PUZZLE_PIECE_1 + puzzleIndex;
                case "E2C4": return LOC_CASKOU_PARK_PUZZLE_PIECE_1 + puzzleIndex;
                case "E3C4": return LOC_LOOPY_LAGOON_PUZZLE_PIECE_1 + puzzleIndex;
                default: return -1;
            }
        }
    }
}