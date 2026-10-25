// Jason's look. GTA6Hud's wardrobe is Lucia's and the three story characters' only, so as Jason the clothes stores had
// nothing for him (a player: "please add the same customization options for Jason like Lucia ... changing hair colour,
// and changing outfits"). At any rack in a clothes store or a barber's, ~INPUT_CONTEXT~ opens his look: every part his
// model has more than one of (hair, beard, top, legs, shoes...), stepped through style by style and colour by colour.
// The parts are read off the model in the game, so any Jason download works. Kept, his look is saved
// (Hud\Save\jason_look.txt) and put back on whenever his ped is made.
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
        const float RackReach = 1.8f;

        // the parts, in the order they're offered, and their names (the game's ped component slots)
        static readonly int[] LookParts = { 2, 1, 3, 11, 8, 4, 6, 5, 9, 7, 10, 0 };
        static readonly string[] PartNames = { "FACE", "BEARD", "HAIR", "TOP", "LEGS", "HANDS", "SHOES", "NECK", "UNDERSHIRT", "VEST", "BADGES", "JACKET" };

        sealed class LookPart
        {
            public int Comp;
            public readonly List<int[]> Options = new List<int[]>();   // { drawable, texture }
            public int At;
        }

        bool _lookOpen;
        List<LookPart> _lookParts;
        int _lookPart;
        int[,] _lookWas;          // [comp, 0/1]: as he was when it opened
        int _lookNearAt;
        bool _lookNear;

        string LookFile => Path.Combine(BaseDirectory, "Hud", "Save", "jason_look.txt");

        void JasonWardrobe(int player)
        {
            if (_lookOpen) { DriveLook(player); return; }
            if (!_jasonOn || player == 0 || Call<int>(ENTITY_MODEL, player) != _jason) return;
            int now = RealNow();
            if (now >= _lookNearAt)
            {
                _lookNearAt = now + 250;
                _lookNear = NearRack(player) && CanChangeLook(player);
            }
            if (!_lookNear) return;
            Help("~INPUT_CONTEXT~ Change Jason's look");
            if (Function.Call<bool>(Hash.IS_CONTROL_JUST_PRESSED, 0, 51)) OpenLook(player);
        }

        bool NearRack(int player)
        {
            if (!LookStores()) return false;
            Vector3 at = Call<Vector3>(ENTITY_COORDS, player, true);
            foreach (Vector3 r in _storeRacks)
            {
                float dx = r.X - at.X, dy = r.Y - at.Y;
                if (dx * dx + dy * dy < RackReach * RackReach && Math.Abs(r.Z - at.Z) < 2f) return true;
            }
            return false;
        }

        bool CanChangeLook(int player)
        {
            return !Function.Call<bool>(Hash.IS_PED_IN_ANY_VEHICLE, player, false) && Call<int>(WANTED_LEVEL, Call<int>(PLAYER_ID)) == 0
                   && !Call<bool>(MISSION_FLAG) && _flyTo == null && !Call<bool>(SWITCH_IN_PROGRESS) && !Game.IsPaused && !_open;
        }

        void OpenLook(int player)
        {
            _lookParts = new List<LookPart>();
            _lookWas = new int[12, 2];
            for (int c = 0; c < 12; c++)
            {
                _lookWas[c, 0] = Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, player, c);
                _lookWas[c, 1] = Function.Call<int>(Hash.GET_PED_TEXTURE_VARIATION, player, c);
            }
            foreach (int c in LookParts)
            {
                var part = new LookPart { Comp = c };
                int drawables = Function.Call<int>(Hash.GET_NUMBER_OF_PED_DRAWABLE_VARIATIONS, player, c);
                for (int d = 0; d < drawables; d++)
                {
                    int textures = Math.Max(1, Function.Call<int>(Hash.GET_NUMBER_OF_PED_TEXTURE_VARIATIONS, player, c, d));
                    for (int t = 0; t < textures; t++)
                        if (Function.Call<bool>(Hash.IS_PED_COMPONENT_VARIATION_VALID, player, c, d, t)) part.Options.Add(new[] { d, t });
                }
                if (part.Options.Count < 2) continue;
                part.At = Math.Max(0, part.Options.FindIndex(o => o[0] == _lookWas[c, 0] && o[1] == _lookWas[c, 1]));
                _lookParts.Add(part);
            }
            if (_lookParts.Count == 0) { Log("Jason's look: his model has nothing to change"); return; }
            _lookPart = 0;
            _lookOpen = true;
            Call(CLEAR_TASKS, player);
            Log("Jason's look: open (" + _lookParts.Count + " parts)");
        }

        void DriveLook(int player)
        {
            if (player == 0 || Call<int>(ENTITY_MODEL, player) != _jason || Call<bool>(ENTITY_DEAD, player, false))
            {
                _lookOpen = false;
                return;
            }
            Function.Call(Hash.DISABLE_ALL_CONTROL_ACTIONS, 0);
            Function.Call(Hash.SET_GAMEPLAY_CAM_RELATIVE_HEADING, 180f);   // his front
            LookPart part = _lookParts[_lookPart];

            if (Pressed(172)) _lookPart = (_lookPart + _lookParts.Count - 1) % _lookParts.Count;       // up
            else if (Pressed(173)) _lookPart = (_lookPart + 1) % _lookParts.Count;                    // down
            else if (Pressed(174) || Pressed(175))                                                   // left / right
            {
                int step = Pressed(174) ? -1 : 1;
                part.At = (part.At + step + part.Options.Count) % part.Options.Count;
                int[] o = part.Options[part.At];
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, player, part.Comp, o[0], o[1], 0);
                Function.Call(Hash.PLAY_SOUND_FRONTEND, -1, "NAV_LEFT_RIGHT", "HUD_FRONTEND_DEFAULT_SOUNDSET", true);
            }
            else if (Pressed(201))                                                                   // keep
            {
                SaveLook(player);
                _lookOpen = false;
                Function.Call(Hash.PLAY_SOUND_FRONTEND, -1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", true);
                Log("Jason's look: kept");
                return;
            }
            else if (Pressed(202))                                                                   // undo
            {
                for (int c = 0; c < 12; c++) Function.Call(Hash.SET_PED_COMPONENT_VARIATION, player, c, _lookWas[c, 0], _lookWas[c, 1], 0);
                _lookOpen = false;
                Function.Call(Hash.PLAY_SOUND_FRONTEND, -1, "BACK", "HUD_FRONTEND_DEFAULT_SOUNDSET", true);
                Log("Jason's look: undone");
                return;
            }

            part = _lookParts[_lookPart];
            int[] now = part.Options[part.At];
            int styles = 0, colours = 0;
            foreach (int[] o in part.Options) { if (o[0] == now[0]) colours++; styles = Math.Max(styles, o[0] + 1); }
            int colour = 0;
            foreach (int[] o in part.Options) { if (o[0] == now[0] && o[1] < now[1]) colour++; }
            string line = PartNames[part.Comp] + "  style " + (now[0] + 1) + "/" + styles + (colours > 1 ? ", colour " + (colour + 1) + "/" + colours : "");
            Help("JASON'S LOOK  (" + (_lookPart + 1) + "/" + _lookParts.Count + ")~n~" + line
                 + "~n~~INPUT_CELLPHONE_UP~ ~INPUT_CELLPHONE_DOWN~ part   ~INPUT_CELLPHONE_LEFT~ ~INPUT_CELLPHONE_RIGHT~ change"
                 + "~n~~INPUT_FRONTEND_ACCEPT~ keep   ~INPUT_FRONTEND_CANCEL~ undo");
        }

        static bool Pressed(int control) => Function.Call<bool>(Hash.IS_DISABLED_CONTROL_JUST_PRESSED, 0, control);

        static void Help(string text)
        {
            Function.Call(Hash.BEGIN_TEXT_COMMAND_DISPLAY_HELP, "CELL_EMAIL_BCON");
            for (int i = 0; i < text.Length; i += 90)
                Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, text.Substring(i, Math.Min(90, text.Length - i)));
            Function.Call(Hash.END_TEXT_COMMAND_DISPLAY_HELP, 0, false, false, -1);
        }

        // ---- saved ----

        void SaveLook(int ped)
        {
            try
            {
                var lines = new List<string> { "# Jason's look, kept at a clothes store (ViSwitch). Delete to go back to his default look.", "model " + _jason };
                for (int c = 0; c < 12; c++)
                    lines.Add(c + " " + Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, ped, c) + " " + Function.Call<int>(Hash.GET_PED_TEXTURE_VARIATION, ped, c));
                Directory.CreateDirectory(Path.GetDirectoryName(LookFile));
                File.WriteAllLines(LookFile, lines);
            }
            catch (Exception ex) { Log("Jason's look: not saved (" + ex.Message + ")"); }
        }

        // His saved look on a ped just made; false when there's none for this model (the default look stays).
        bool ApplyLook(Ped ped)
        {
            try
            {
                if (!File.Exists(LookFile)) return false;
                bool model = false;
                int set = 0;
                foreach (string raw in File.ReadAllLines(LookFile))
                {
                    string[] p = raw.Trim().Split(' ');
                    if (p.Length == 2 && p[0] == "model") { model = p[1] == _jason.ToString(); continue; }
                    if (!model || p.Length != 3 || !int.TryParse(p[0], out int c) || !int.TryParse(p[1], out int d) || !int.TryParse(p[2], out int t)) continue;
                    if (c < 0 || c > 11 || !Function.Call<bool>(Hash.IS_PED_COMPONENT_VARIATION_VALID, ped, c, d, t)) continue;
                    Function.Call(Hash.SET_PED_COMPONENT_VARIATION, ped, c, d, t, 0);
                    set++;
                }
                return set > 0;
            }
            catch (Exception ex) { Log("Jason's look: not read (" + ex.Message + ")"); return false; }
        }
    }
}
