using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using ExtraSlots;
using ExtraSlots.HotBars;
using HarmonyLib;

namespace ExtraSlots_MultipleHotkeyUsage;

[BepInPlugin(PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(DEPENDENCY_GUID)]
public class ExtraSlots_MultipleHotkeyUsage : BaseUnityPlugin
{
    public const string PLUGIN_GUID = "kabooma.ExtraSlots_MultipleHotkeyUsage";
    public const string DEPENDENCY_GUID = "shudnal.ExtraSlots";
    internal static new ManualLogSource Logger;
        
    private void Awake()
    {
        Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll();
    }

    // Checks ALL HotBars for slots being pressed. Returns list of those slots
    public static List<Slots.Slot> GetSlotsWithShortcutDown() {
        List<Slots.Slot> foodSlots = FoodSlotsHotBar.hotBarSlots.Where((s) => s.IsShortcutDown()).ToList();
        List<Slots.Slot> quickSlots = QuickSlotsHotBar.hotBarSlots.Where((s) => s.IsShortcutDown()).ToList();
        List<Slots.Slot> allSlots = foodSlots.Concat(quickSlots).ToList();
        return allSlots;
    }

    public static List<ItemDrop.ItemData> GetItemsToUse() {
        List<Slots.Slot> activeSlots = GetSlotsWithShortcutDown();
        return activeSlots.Select((s) => s.Item).ToList();
    }

    [HarmonyPatch(typeof(QuickBars), nameof(QuickBars.UpdateItemUse))]
    public static class QuickBarsPatch
    {
        public static bool Prefix() {
            if (Player.m_localPlayer.TakeInput()) {
                List<ItemDrop.ItemData> itemsToUse = GetItemsToUse();
                if (itemsToUse.Count == 0) return false;

                foreach (ItemDrop.ItemData item in itemsToUse) {
                    Player.m_localPlayer.UseItem(Slots.PlayerInventory, item, fromInventoryGui: false);
                }
            }

            return false;
        }
    }
}
