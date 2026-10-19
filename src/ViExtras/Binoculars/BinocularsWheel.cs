using System;
using System.Collections;
using System.Reflection;

namespace ViExtras
{
    /// <summary>
    /// The binoculars on GTA6Hud's weapon wheel. An empty weapon tile (the one
    /// bottom right, [Binoculars] WheelRing 3, or the next empty one round the
    /// wheel when that holds a weapon) shows GTA6Hud's own binoculars item, and
    /// letting the wheel go on it brings these binoculars up.
    ///
    /// GTA6Hud builds its weapon tiles when the wheel opens or turns a page
    /// (BuildSlots) and draws them every frame after that, so the empty tile is
    /// filled in once it has built them, by reflection. Its CloseWheel does
    /// nothing for a tile without a weapon in it; the pick is read here, on the
    /// frame the wheel shuts.
    /// </summary>
    internal static class BinocularsWheel
    {
        private const BindingFlags Own = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const int WeaponsTab = 0;    // GTA6Hud.WheelTab.Weapons
        private const string Label = "BINOCULARS";

        private static bool _looked, _wasOpen;
        private static FieldInfo _open, _tab, _slots, _highlight;
        private static FieldInfo _ring, _weapon, _isUnarmed, _isEquipment, _label, _available, _variationIndex, _variationCount;
        private static int _icon = 1;        // GTA6Hud.EquipmentNames: FLASHLIGHT, BINOCULARS, FISHING ROD

        private static bool Look(object hud)
        {
            if (_looked) return _open != null;
            _looked = true;
            try
            {
                Type t = hud.GetType();
                Type slot = t.GetNestedType("WheelSlot", BindingFlags.NonPublic | BindingFlags.Public);
                if (slot != null)
                {
                    _ring = slot.GetField("Ring", Own);
                    _weapon = slot.GetField("Weapon", Own);
                    _isUnarmed = slot.GetField("IsUnarmed", Own);
                    _isEquipment = slot.GetField("IsEquipment", Own);
                    _label = slot.GetField("Label", Own);
                    _available = slot.GetField("Available", Own);
                    _variationIndex = slot.GetField("VariationIndex", Own);
                    _variationCount = slot.GetField("VariationCount", Own);
                }
                if (t.GetField("EquipmentNames", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null) is string[] names)
                    _icon = Array.IndexOf(names, Label);
                _open = t.GetField("_wheelOpen", Own);
                _tab = t.GetField("_wheelTab", Own);
                _slots = t.GetField("_slots", Own);
                _highlight = t.GetField("_highlightIndex", Own);
                if (_open == null || _tab == null || _slots == null || _highlight == null || _ring == null || _weapon == null ||
                    _isUnarmed == null || _isEquipment == null || _label == null || _available == null ||
                    _variationIndex == null || _variationCount == null || _icon < 0)
                {
                    _open = null;
                    Log.Write("Binoculars", "GTA6Hud's weapon wheel was not as expected; no binoculars tile on it ([Binoculars] Key still works)");
                    return false;
                }
                Log.Write("Binoculars", "binoculars tile on GTA6Hud's weapon wheel");
                return true;
            }
            catch (Exception ex)
            {
                _open = null;
                Log.Write("Binoculars", "GTA6Hud's weapon wheel: " + ex.Message);
                return false;
            }
        }

        private static bool Ours(object slot) => (bool)_isEquipment.GetValue(slot) && _weapon.GetValue(slot) == null;

        private static bool Empty(object slot) => _weapon.GetValue(slot) == null && !(bool)_isUnarmed.GetValue(slot) && !(bool)_isEquipment.GetValue(slot);

        // Puts the binoculars on an empty weapon tile, if GTA6Hud's last build has none yet.
        private static void Fill(IList slots, int ring)
        {
            int target = -1, best = int.MaxValue;
            for (int i = 0; i < slots.Count; i++)
            {
                object s = slots[i];
                if (Ours(s)) return;
                if (!Empty(s)) continue;
                int away = (((int)_ring.GetValue(s) - ring) % 8 + 8) % 8;    // the first empty one clockwise from `ring`
                if (away < best) { best = away; target = i; }
            }
            if (target < 0) return;
            object slot = slots[target];
            _isEquipment.SetValue(slot, true);
            _label.SetValue(slot, Label);
            _available.SetValue(slot, true);
            _variationIndex.SetValue(slot, _icon);
            _variationCount.SetValue(slot, 1);
            slots[target] = slot;
        }

        /// <summary>
        /// Every frame: keeps the tile on GTA6Hud's weapon wheel while it is open.
        /// True on the frame the wheel shuts with the binoculars picked.
        /// </summary>
        public static bool Picked(int ring)
        {
            object hud = HudBridge.Gta6Hud();
            if (hud == null || !Look(hud)) return false;
            try
            {
                bool open = (bool)_open.GetValue(hud);
                bool shut = _wasOpen && !open;
                _wasOpen = open;
                if (!open && !shut) return false;
                IList slots = (IList)_slots.GetValue(hud);
                if (open)
                {
                    Fill(slots, ring);
                    return false;
                }
                // shut this frame: on the binoculars tile, and not shut for a menu GTA6Hud opened over it
                if (Convert.ToInt32(_tab.GetValue(hud)) != WeaponsTab || HudBridge.Gta6HudUiOpen) return false;
                int h = (int)_highlight.GetValue(hud);
                return h >= 0 && h < slots.Count && Ours(slots[h]);
            }
            catch (Exception ex)
            {
                _open = null;
                Log.Write("Binoculars", "GTA6Hud's weapon wheel: " + ex.Message);
                return false;
            }
        }
    }
}
