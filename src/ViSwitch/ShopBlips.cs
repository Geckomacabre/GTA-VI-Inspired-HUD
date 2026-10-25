// Shop icons for Lucia and Jason. The game shows the clothes stores', barbers' and Ammu-Nation's map icons for
// Michael, Franklin and Trevor, and hides them for anyone else, so as Lucia or Jason the map had no shops on it (a
// player: "the shop location not showing in map"). While one of them is the player, every store that has no visible
// icon of its kind from the game gets one of ours (the game's own sprite, short range, as the game shows them), and
// they go again on switching back. The stores are GTA6Hud's own: its clothes and barber racks (ShopStands, grouped
// into one spot per store) and its Ammu-Nation doors. ViExtras.ini [Switches] ShopIcons = False leaves the map alone.
using System;
using System.Collections.Generic;
using System.IO;
using GTA;
using GTA.Math;
using GTA.Native;

namespace ViSwitch
{
    public partial class ViSwitch
    {
        const int ClothesSprite = 73, BarberSprite = 71, AmmuSprite = 110;
        const float SameStore = 40f;

        sealed class ShopSpot
        {
            public int Sprite;
            public string Name;
            public Vector3 At;
            public int Blip;
        }

        List<ShopSpot> _shopSpots;
        bool _shopIcons = true, _shopIconsRead;
        int _shopBlipsAt;

        void ShopBlips(int player)
        {
            int now = RealNow();
            if (now < _shopBlipsAt) return;
            _shopBlipsAt = now + 2000;
            if (!_shopIconsRead)
            {
                _shopIconsRead = true;
                _shopIcons = ReadIniBool(Path.Combine(BaseDirectory, "ViExtras.ini"), "Switches", "ShopIcons", true);
                if (!_shopIcons) Log("shop icons: [Switches] ShopIcons = False");
            }
            int model = player != 0 ? Call<int>(ENTITY_MODEL, player) : 0;
            bool want = _shopIcons && model != 0 && (model == _lucia || (_jasonOn && model == _jason));
            if (!want) { RemoveShopBlips(); return; }
            if (_shopSpots == null && !BuildShopSpots()) return;

            var ours = new HashSet<int>();
            foreach (ShopSpot s in _shopSpots) if (s.Blip != 0) ours.Add(s.Blip);
            int added = 0, dropped = 0;
            foreach (ShopSpot s in _shopSpots)
            {
                bool game = GameShowsBlip(s.Sprite, s.At, ours);
                if (game && s.Blip != 0) { RemoveBlip(s); dropped++; }
                else if (!game && s.Blip == 0) { AddBlip(s); added++; }
            }
            if (added > 0 || dropped > 0) Log("shop icons: " + added + " added, " + dropped + " left to the game's own");
        }

        // A blip of this sprite near the spot that the player can see (shown, not see-through), other than ours.
        static bool GameShowsBlip(int sprite, Vector3 at, HashSet<int> ours)
        {
            for (int b = Function.Call<int>(Hash.GET_FIRST_BLIP_INFO_ID, sprite); Function.Call<bool>(Hash.DOES_BLIP_EXIST, b);
                 b = Function.Call<int>(Hash.GET_NEXT_BLIP_INFO_ID, sprite))
            {
                if (ours.Contains(b)) continue;
                if (Function.Call<int>(Hash.GET_BLIP_INFO_ID_DISPLAY, b) == 0 || Function.Call<int>(Hash.GET_BLIP_ALPHA, b) == 0) continue;
                Vector3 p = Function.Call<Vector3>(Hash.GET_BLIP_INFO_ID_COORD, b);
                float dx = p.X - at.X, dy = p.Y - at.Y;
                if (dx * dx + dy * dy < SameStore * SameStore) return true;
            }
            return false;
        }

        void AddBlip(ShopSpot s)
        {
            int b = Function.Call<int>(Hash.ADD_BLIP_FOR_COORD, s.At.X, s.At.Y, s.At.Z);
            if (b == 0) return;
            Function.Call(Hash.SET_BLIP_SPRITE, b, s.Sprite);
            Function.Call(Hash.SET_BLIP_AS_SHORT_RANGE, b, true);
            Function.Call(Hash.SET_BLIP_DISPLAY, b, 2);
            Function.Call(Hash.BEGIN_TEXT_COMMAND_SET_BLIP_NAME, "STRING");
            Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, s.Name);
            Function.Call(Hash.END_TEXT_COMMAND_SET_BLIP_NAME, b);
            s.Blip = b;
        }

        static void RemoveBlip(ShopSpot s)
        {
            if (s.Blip == 0) return;
            var arg = new OutputArgument(s.Blip);
            if (Function.Call<bool>(Hash.DOES_BLIP_EXIST, s.Blip)) Function.Call(Hash.REMOVE_BLIP, arg);
            s.Blip = 0;
        }

        void RemoveShopBlips()
        {
            if (_shopSpots == null) return;
            int n = 0;
            foreach (ShopSpot s in _shopSpots) if (s.Blip != 0) { RemoveBlip(s); n++; }
            if (n > 0) Log("shop icons: " + n + " removed");
        }

        // One spot per store: GTA6Hud's racks grouped by distance (barbers apart), and its Ammu-Nation doors.
        bool BuildShopSpots()
        {
            if (!LookStores()) return false;
            var spots = new List<ShopSpot>();
            for (int i = 0; i < _storeRacks.Length; i++)
            {
                bool barber = _storeKeys != null && _storeKeys[i] != null && _storeKeys[i].StartsWith("BARBER", StringComparison.Ordinal);
                Group(spots, barber ? BarberSprite : ClothesSprite, barber ? "Barber" : "Clothes Store", _storeRacks[i]);
            }
            foreach (Vector3 d in _ammuDoors) Group(spots, AmmuSprite, "Ammu-Nation", d);
            _shopSpots = spots;
            Log("shop icons: " + spots.Count + " stores (" + spots.FindAll(s => s.Sprite == ClothesSprite).Count + " clothes, "
                + spots.FindAll(s => s.Sprite == BarberSprite).Count + " barbers, " + spots.FindAll(s => s.Sprite == AmmuSprite).Count + " Ammu-Nation)");
            return true;
        }

        static void Group(List<ShopSpot> spots, int sprite, string name, Vector3 at)
        {
            foreach (ShopSpot s in spots)
                if (s.Sprite == sprite && s.At.DistanceTo2D(at) < SameStore) return;
            spots.Add(new ShopSpot { Sprite = sprite, Name = name, At = at });
        }
    }
}
