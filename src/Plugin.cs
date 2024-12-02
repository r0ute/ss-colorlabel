using System;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using MyBox;
using UnityEngine;

namespace SS.src;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;


    static readonly Color FREEZER_COLOR = new(0.298f, 0.604f, 1, 1);

    static readonly Color FRIDGE_COLOR = new(0.4f, 0.851f, 0.69f, 1);

    static readonly Color CRATE_COLOR = new(0.545f, 0.271f, 0.075f, 1);

    static readonly Color SHELF_COLOR = new(0.827f, 0.827f, 0.827f, 1);

    static readonly Color EDIBLE_COLOR = new(0.639f, 0.843f, 0.478f, 1);

    static readonly Color DRINK_COLOR = new(0.275f, 0.51f, 0.706f, 1);

    static readonly Color CLEANING_COLOR = new(0.961f, 0.961f, 0.961f, 1);

    static readonly Color BOOK_COLOR = new(0.824f, 0.706f, 0.549f, 1);

    internal static ConfigEntry<bool> ColorDisplayLabel;

    internal static ConfigEntry<bool> ColorRackLabel;

    internal static ConfigEntry<bool> InGameProductNameColor;

    internal static ConfigEntry<Color> DisplayTypeFreezerColor;

    internal static ConfigEntry<Color> DisplayTypeFridgeColor;

    internal static ConfigEntry<Color> DisplayTypeCrateColor;

    internal static ConfigEntry<Color> DisplayTypeShelfColor;

    internal static ConfigEntry<Color> ProductCategoryEdibleColor;

    internal static ConfigEntry<Color> ProductCategoryDrinkColor;

    internal static ConfigEntry<Color> ProductCategoryCleaningColor;

    internal static ConfigEntry<Color> ProductCategoryBookColor;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;

        ColorDisplayLabel = Config.Bind("*General*", "ColorDisplayLabel", true, "Color labeling for display slots");
        ColorRackLabel = Config.Bind("*General*", "ColorRackLabel", true, "Color labeling for rack slots");
        InGameProductNameColor = Config.Bind("*General*", "InGameProductNameColor", false,
            "Apply in-game product name colors (currently, the same color is used for all products)");

        DisplayTypeFreezerColor = Config.Bind("DisplayType", "FreezerColor", FREEZER_COLOR, "FREEZER");
        DisplayTypeFridgeColor = Config.Bind("DisplayType", "FridgeColor", FRIDGE_COLOR, "FRIDGE_COLOR");
        DisplayTypeCrateColor = Config.Bind("DisplayType", "CrateColor", CRATE_COLOR, "CRATE_COLOR");
        DisplayTypeShelfColor = Config.Bind("DisplayType", "ShelfColor", SHELF_COLOR, "SHELF_COLOR");

        ProductCategoryEdibleColor = Config.Bind("ProductCategory", "EdibleColor", EDIBLE_COLOR, "EDIBLE_COLOR");
        ProductCategoryDrinkColor = Config.Bind("ProductCategory", "DrinkColor", DRINK_COLOR, "DRINK_COLOR");
        ProductCategoryCleaningColor = Config.Bind("ProductCategory", "CleaningColor", CLEANING_COLOR, "CLEANING_COLOR");
        ProductCategoryBookColor = Config.Bind("ProductCategory", "BookColor", BOOK_COLOR, "BOOK_COLOR");

        Harmony harmony = new(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll(typeof(Patches));

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }


    class Patches
    {

        [HarmonyPatch(typeof(DayCycleManager), "Start")]
        [HarmonyPostfix]
        static void OnDayStart()
        {

            Array.ForEach((DisplayType[])Enum.GetValues(typeof(DisplayType)), new Action<DisplayType>(LogProduct));

        }

        private static void LogProduct(DisplayType displayType)
        {
            Logger.LogDebug(displayType);

            Singleton<IDManager>.Instance.Products
                .Where((product) => product.ProductDisplayType == displayType)
                .OrderBy(product => product.Category)
                .ThenBy(product => product.ProductName)
                .ForEach(product => Logger.LogDebug($"Product: name={product},category={product.Category},color={product.ProductNameColor}"));

        }


        [HarmonyPatch(typeof(Label), nameof(Label.DisplaySetup))]
        [HarmonyPostfix]
        static void OnLabelDisplaySetup(DisplaySlot displaySlot, ref Label __instance)
        {
            if (!ColorDisplayLabel.Value)
            {
                return;
            }


            SetColor(__instance, displaySlot.ProductID);
        }


        [HarmonyPatch(typeof(Label), nameof(Label.RackSetup))]
        [HarmonyPostfix]
        static void OnLabelRackSetup(RackSlot rackSlot, ref Label __instance)
        {
            if (!ColorRackLabel.Value)
            {
                return;
            }

            SetColor(__instance, rackSlot.Data.ProductID);
        }

        private static void SetColor(Label label, int productId)
        {
            var product = Singleton<IDManager>.Instance.ProductSO(productId);
            Color color;

            if (InGameProductNameColor.Value)
            {
                color = product.ProductNameColor;
            }
            else if (product.ProductDisplayType == DisplayType.FRIDGE)
            {
                color = DisplayTypeFridgeColor.Value;
            }
            else if (product.ProductDisplayType == DisplayType.FREEZER)
            {
                color = DisplayTypeFreezerColor.Value;
            }
            else if (product.ProductDisplayType == DisplayType.CRATE)
            {
                color = DisplayTypeCrateColor.Value;
            }
            else if (product.Category == ProductSO.ProductCategory.EDIBLE)
            {
                color = ProductCategoryEdibleColor.Value;
            }
            else if (product.Category == ProductSO.ProductCategory.DRINK)
            {
                color = ProductCategoryDrinkColor.Value;
            }
            else if (product.Category == ProductSO.ProductCategory.CLEANING)
            {
                color = ProductCategoryCleaningColor.Value;
            }
            else if (product.Category == ProductSO.ProductCategory.BOOK)
            {
                color = ProductCategoryBookColor.Value;
            }
            else
            {
                color = DisplayTypeShelfColor.Value;
            }


            label.transform.Find("UI/Icon BG").GetComponent<MeshRenderer>().material.color = color;
        }


    }
}
