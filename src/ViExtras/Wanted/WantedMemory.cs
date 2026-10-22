using System;
using GTA;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// What the police saw this pursuit, for the wanted tells: vice_hud's ViceWantedMemory
    /// (client_awareness.lua) and its camera sightings (client.lua tellSightings), the way the
    /// FiveM version keeps them when fenix-police isn't running.
    ///
    /// Note() is called while a cop has line of sight (WantedStars' "contact"). It records the
    /// outfit, whether the face was uncovered, and the plate of the vehicle you were in. The
    /// tells then light
    ///   outfit   while what you wear still matches what they saw
    ///   face     while they have seen your face and it is still uncovered
    ///   vehicle  while you sit in the vehicle with that plate
    /// so changing clothes, masking up or swapping cars visibly works. Plates, not models: a
    /// second car of the same model is not the one they saw. A camera sighting holds until
    /// the stars are gone, as vice_hud's does. Everything is cleared when the wanted level is.
    ///
    /// The tells used to come from GTA6Hud's own police memory, which is off since 2.5.5
    /// (GTA6Hud.ini [Police] MemoryEnabled = False) and so remembered nothing: only the
    /// weapon tell still lit. This one keeps nothing between pursuits and changes nothing in
    /// the game (no re-wanting, no patrols); it only picks which tells show.
    /// </summary>
    internal static class WantedMemory
    {
        // client_awareness.lua: GTA6Hud's outfit set minus component 1, the mask, which is the
        // face tell's business; and the hat and glasses props.
        private static readonly int[] OutfitComponents = { 3, 4, 5, 6, 7, 8, 9, 10, 11 };
        private static readonly int[] OutfitProps = { 0, 1 };

        // client.lua NEARBY_SUSPECT_RANGE: within this, "suspects" (people) rather than "suspect" (person).
        private const float NearbySuspectRange = 15f;

        private static bool _noted;
        private static string _outfit, _plate;
        private static bool _faceSeen, _camera;
        private static int[] _protagonists;

        /// <summary>A cop can see the player now: what they see (ViceWantedMemory.note).</summary>
        public static void Note(Ped ped)
        {
            _noted = true;
            _outfit = OutfitSig(ped);
            if (!FaceHidden(ped)) _faceSeen = true;
            _plate = PlateOf(ped) ?? _plate;
        }

        /// <summary>A camera saw the player while wanted; it holds until the stars are gone.</summary>
        public static void SeenOnCamera() => _camera = true;

        /// <summary>The wanted level is gone: every sighting is spent.</summary>
        public static void Clear()
        {
            _noted = false;
            _outfit = _plate = null;
            _faceSeen = _camera = false;
        }

        public static bool Camera => _camera;
        public static bool Outfit(Ped ped) => _noted && _outfit != null && _outfit == OutfitSig(ped);
        public static bool Face(Ped ped) => _noted && _faceSeen && !FaceHidden(ped);

        public static bool Vehicle(Ped ped)
        {
            if (!_noted || _plate == null) return false;
            string plate = PlateOf(ped);
            return plate != null && plate == _plate;
        }

        /// <summary>
        /// countNearbyPlayers: someone close enough to be in the same "suspects identified"
        /// report. Single player has no other players, so it's the other story characters
        /// (Michael, Franklin, Trevor and Lucia) within the same 15 m.
        /// </summary>
        public static bool SuspectNearby(Ped ped)
        {
            if (_protagonists == null)
                _protagonists = new[]
                {
                    225514697, -1692214353, -1686040670,   // player_zero, player_one, player_two
                    Function.Call<int>(Hash.GET_HASH_KEY, "lucia_s"),
                    Function.Call<int>(Hash.GET_HASH_KEY, "lucia_nophys"),
                };
            foreach (Ped p in World.GetNearbyPeds(ped, NearbySuspectRange))
            {
                if (p == null || !p.Exists() || p == ped || p.IsDead) continue;
                if (Array.IndexOf(_protagonists, p.Model.Hash) >= 0) return true;
            }
            return false;
        }

        // Note and Outfit both ask in the same tick while a cop is looking: worked out once a frame.
        private static int _sigFrame = -1, _sigPed;
        private static string _sig;

        private static string OutfitSig(Ped ped)
        {
            int frame = Game.FrameCount;
            if (frame == _sigFrame && ped.Handle == _sigPed) return _sig;
            _sigFrame = frame;
            _sigPed = ped.Handle;
            var sb = new System.Text.StringBuilder(96);
            foreach (int c in OutfitComponents)
                sb.Append(Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, ped, c)).Append(':')
                  .Append(Function.Call<int>(Hash.GET_PED_TEXTURE_VARIATION, ped, c)).Append(',');
            foreach (int p in OutfitProps)
                sb.Append('p').Append(Function.Call<int>(Hash.GET_PED_PROP_INDEX, ped, p)).Append(':')
                  .Append(Function.Call<int>(Hash.GET_PED_PROP_TEXTURE_INDEX, ped, p)).Append(',');
            _sig = sb.ToString();
            return _sig;
        }

        // Component 1 is the mask slot; anything but the empty drawable covers the face.
        private static bool FaceHidden(Ped ped) => Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, ped, 1) > 0;

        private static string PlateOf(Ped ped)
        {
            Vehicle veh = ped.CurrentVehicle;
            if (veh == null || !veh.Exists()) return null;
            string plate = Function.Call<string>(Hash.GET_VEHICLE_NUMBER_PLATE_TEXT, veh);
            if (plate == null) return null;
            plate = plate.Trim().ToUpperInvariant();
            return plate.Length > 0 ? plate : null;
        }
    }
}
