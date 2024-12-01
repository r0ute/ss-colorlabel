using System;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using MyBox;
using TMPro;
using UnityEngine;
using VLB;

namespace SS.src;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;


    static Color FREEZER_COLOR = new(0.298f, 0.604f, 1, 1);

    static Color FRIDGE_COLOR = new(0.4f, 0.851f, 0.69f, 1);

    static Color CRATE_COLOR = new(0.545f, 0.271f, 0.075f, 1);

    static Color EDIBLE_COLOR = new(0.639f, 0.843f, 0.478f, 1);

    static Color DRINK_COLOR = new(0.275f, 0.51f, 0.706f, 1);

    static Color CLEANING_COLOR = new(0.961f, 0.961f, 0.961f, 1);

    static Color BOOK_COLOR = new(0.824f, 0.706f, 0.549f, 1);


    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;

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
                .ForEach(product => Logger.LogDebug($"Product: name={product},category={product.Category}"));

        }


        [HarmonyPatch(typeof(Label), nameof(Label.DisplaySetup))]
        [HarmonyPostfix]
        static void OnLabelDisplaySetup(DisplaySlot displaySlot, ref Label __instance)
        {
            SetColor(__instance, displaySlot.ProductID);
        }


        [HarmonyPatch(typeof(Label), nameof(Label.RackSetup))]
        [HarmonyPostfix]
        static void OnLabelRackSetup(RackSlot rackSlot, ref Label __instance)
        {
            SetColor(__instance, rackSlot.Data.ProductID);
        }

        private static void SetColor(Label label, int productId)
        {
            var product = Singleton<IDManager>.Instance.ProductSO(productId);
            var color = Color.white;

            if (product.ProductDisplayType == DisplayType.FRIDGE)
            {
                color = FRIDGE_COLOR;
            }
            else if (product.ProductDisplayType == DisplayType.FREEZER)
            {
                color = FREEZER_COLOR;
            }
            else if (product.ProductDisplayType == DisplayType.CRATE)
            {
                color = CRATE_COLOR;
            }
            else if (product.Category == ProductSO.ProductCategory.EDIBLE)
            {
                color = EDIBLE_COLOR;
            }
            else if (product.Category == ProductSO.ProductCategory.DRINK)
            {
                color = DRINK_COLOR;
            }
            else if (product.Category == ProductSO.ProductCategory.CLEANING)
            {
                color = CLEANING_COLOR;
            }
            else if (product.Category == ProductSO.ProductCategory.BOOK)
            {
                color = BOOK_COLOR;
            }


            label.transform.Find("UI/Icon BG").GetComponent<MeshRenderer>().material.color = color;
        }


    }
}
