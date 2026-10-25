using System;
using System.Collections.Generic;
using System.Drawing;
using GTA;
using GTA.Math;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// Petting a dog, single player's own: walk up to a dog that's minding its
    /// own business, and PET comes up on it on vice_hud's world prompt (the one
    /// Greet uses), on the context button (E / d-pad right). The player kneels
    /// and pets it with Franklin's petting of Chop; a big dog (Chop's skeleton:
    /// rottweiler, shepherd, retriever, husky) plays Chop's side of it, a small
    /// one stands still for it. Honor is paid through the pet_dog hook; the same
    /// dog pays again only after PetSameDogMs.
    ///
    /// A dog a script owns (Chop at Franklin's, a mission's dog) is left alone,
    /// as Memory leaves script-owned people alone.
    /// </summary>
    internal sealed class HonorDogs
    {
        private const string Dict = "creatures@rottweiler@tricks@";
        private const string PlayerClip = "petting_franklin";
        private const string DogClip = "petting_chop";
        private const int Context = 51;                  // INPUT_CONTEXT: E / d-pad right
        private const float Reach = 1.8f;
        private const int ScanMs = 250;
        private const int TurnMs = 700;
        private const int HeadBone = 31086;
        private static readonly Vector3 HeadOffset = new Vector3(0f, 0f, 0.3f);
        private const float ButtonToWord = 0.8f * 10.8f * 0.7f;  // as MemoryInteract's world prompt

        private static readonly HashSet<int> BigDogs = Hashes("a_c_chop", "a_c_rottweiler", "a_c_shepherd", "a_c_retriever", "a_c_husky");
        private static readonly HashSet<int> SmallDogs = Hashes("a_c_poodle", "a_c_pug", "a_c_westy");

        private readonly Func<string, int?> _report;
        private readonly Action<string> _trace;
        private readonly int _sameDogMs;

        private readonly Dictionary<int, int> _pettedAt = new Dictionary<int, int>();
        private Ped _dog;
        private int _scanAt;

        // the petting itself: 0 none, 1 turning to each other, 2 playing
        private int _phase;
        private Ped _petting;
        private int _phaseUntil;
        private bool _dictMissing;

        public HonorDogs(Func<string, int?> report, Action<string> trace)
        {
            _report = report;
            _trace = trace;
            _sameDogMs = Config.Get("Honor", "PetSameDogMs", 300000);
        }

        private static HashSet<int> Hashes(params string[] models)
        {
            var set = new HashSet<int>();
            foreach (string m in models) set.Add(Game.GenerateHash(m));
            return set;
        }

        /// <summary>A dog is being petted: the player is busy.</summary>
        public bool Busy => _phase != 0;

        public void Tick(Ped player, int now, bool hidden)
        {
            if (_phase != 0)
            {
                Petting(player, now);
                return;
            }

            if (now >= _scanAt)
            {
                _scanAt = now + ScanMs;
                _dog = CanPet(player) ? Nearest(player, now) : null;
            }
            if (_dog == null || !_dog.Exists() || hidden) return;

            Draw(player, _dog);
            if (Function.Call<bool>(Hash.IS_CONTROL_JUST_PRESSED, 0, Context)) Start(player, _dog, now);
        }

        private static bool CanPet(Ped player)
        {
            if (player.IsDead || Function.Call<bool>(Hash.IS_PED_IN_ANY_VEHICLE, player, false)) return false;
            if (Game.Player.WantedLevel > 0) return false;
            if (Function.Call<bool>(Hash.IS_PED_RAGDOLL, player) || Function.Call<bool>(Hash.IS_PED_SWIMMING, player)) return false;
            if (Function.Call<bool>(Hash.IS_PED_CLIMBING, player) || Function.Call<bool>(Hash.IS_PED_CUFFED, player)) return false;
            if (Function.Call<bool>(Hash.IS_PLAYER_FREE_AIMING, Game.Player)) return false;
            if (Function.Call<bool>(Hash.IS_PED_IN_COMBAT, player, 0)) return false;
            if (HudBridge.Paused || HudBridge.Busy || HudBridge.HudMenuOpen) return false;
            return true;
        }

        private Ped Nearest(Ped player, int now)
        {
            Ped best = null;
            float bestD = Reach;
            foreach (Ped ped in World.GetNearbyPeds(player, Reach + 0.5f))
            {
                if (ped == null || !ped.Exists() || ped.IsDead) continue;
                int model = ped.Model.Hash;
                if (!BigDogs.Contains(model) && !SmallDogs.Contains(model)) continue;
                if (Function.Call<bool>(Hash.IS_ENTITY_A_MISSION_ENTITY, ped)) continue;
                if (Function.Call<bool>(Hash.IS_PED_IN_COMBAT, ped, 0) || Function.Call<bool>(Hash.IS_PED_FLEEING, ped)) continue;
                if (_pettedAt.TryGetValue(ped.Handle, out int at) && now - at < 15000) continue;   // just petted: let it be
                float d = ped.Position.DistanceTo(player.Position);
                if (d < bestD) { best = ped; bestD = d; }
            }
            return best;
        }

        private void Start(Ped player, Ped dog, int now)
        {
            if (_dictMissing || !Function.Call<bool>(Hash.DOES_ANIM_DICT_EXIST, Dict))
            {
                if (!_dictMissing) Log.Write("Honor", "no " + Dict + " in this game: petting is off");
                _dictMissing = true;
                return;
            }
            Function.Call(Hash.REQUEST_ANIM_DICT, Dict);
            Function.Call(Hash.TASK_TURN_PED_TO_FACE_ENTITY, player, dog, TurnMs);
            Function.Call(Hash.TASK_TURN_PED_TO_FACE_ENTITY, dog, player, TurnMs);
            _petting = dog;
            _phase = 1;
            _phaseUntil = now + TurnMs;
            _dog = null;
        }

        private void Petting(Ped player, int now)
        {
            Ped dog = _petting;
            bool gone = dog == null || !dog.Exists() || dog.IsDead || player.IsDead;
            if (_phase == 1)
            {
                if (gone) { Stop(player, dog); return; }
                if (now < _phaseUntil || !Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, Dict))
                {
                    if (now > _phaseUntil + 2000) Stop(player, dog);   // the dictionary never came
                    return;
                }
                float length = Function.Call<float>(Hash.GET_ANIM_DURATION, Dict, PlayerClip);
                int ms = length > 0.5f ? (int)(length * 1000f) : 6000;
                Function.Call(Hash.TASK_PLAY_ANIM, player, Dict, PlayerClip, 8f, -8f, ms, 0, 0f, false, false, false);
                if (BigDogs.Contains(dog.Model.Hash))
                    Function.Call(Hash.TASK_PLAY_ANIM, dog, Dict, DogClip, 8f, -8f, ms, 0, 0f, false, false, false);
                else
                    Function.Call(Hash.TASK_STAND_STILL, dog, ms);
                _phase = 2;
                _phaseUntil = now + ms;
                return;
            }

            // Playing: the player's moves wait; it's over when the clip is.
            Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, 24, true);   // attack
            Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, 25, true);   // aim
            Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, 22, true);   // jump
            if (gone || now >= _phaseUntil)
            {
                bool done = !gone;
                Stop(player, dog);
                if (!done) return;
                // The same dog pays once every PetSameDogMs; petting it again is still allowed.
                bool pays = !_pettedAt.TryGetValue(dog.Handle, out int last) || now - last >= _sameDogMs;
                _pettedAt[dog.Handle] = now;
                _trace("petted dog " + dog.Handle + (pays ? "" : " (this dog already paid)"));
                if (pays) _report("pet_dog");
            }
        }

        private void Stop(Ped player, Ped dog)
        {
            _phase = 0;
            _petting = null;
            if (player != null && player.Exists() && Function.Call<bool>(Hash.IS_ENTITY_PLAYING_ANIM, player, Dict, PlayerClip, 3))
                Function.Call(Hash.STOP_ANIM_TASK, player, Dict, PlayerClip, -4f);
            if (dog != null && dog.Exists() && Function.Call<bool>(Hash.IS_ENTITY_PLAYING_ANIM, dog, Dict, DogClip, 3))
                Function.Call(Hash.STOP_ANIM_TASK, dog, Dict, DogClip, -4f);
            Function.Call(Hash.REMOVE_ANIM_DICT, Dict);
        }

        /// <summary>The script is stopping: no one is left kneeling.</summary>
        public void Shutdown()
        {
            if (_phase != 0) Stop(Game.Player.Character, _petting);
        }

        // vice_hud's world prompt, one row: the button, then PET (MemoryInteract.Draw).
        private static void Draw(Ped player, Ped dog)
        {
            Vector3 head = Function.Call<Vector3>(Hash.GET_PED_BONE_COORDS, dog, HeadBone, 0f, 0f, 0f) + HeadOffset;
            var sx = new OutputArgument();
            var sy = new OutputArgument();
            if (!Function.Call<bool>(Hash.GET_SCREEN_COORD_FROM_WORLD_COORD, head.X, head.Y, head.Z, sx, sy)) return;
            float x = sx.GetResult<float>() * Canvas.W;
            float y = sy.GetResult<float>() * Canvas.H;

            Art.Entry shared = Glyphs.ForControl(Context);
            Art.Entry button = shared != null ? Art.Get("wa_" + System.IO.Path.GetFileNameWithoutExtension(shared.Path)) : null;
            Art.Entry word = Art.Get("wa_label_pet");
            float bh = button != null ? button.BoxH : 0f;
            float wh = word != null ? word.BoxH : 0f;
            float rowH = Math.Max(bh, wh);
            if (button != null)
            {
                Art.Draw(button, x, y + (rowH - bh) * 0.5f, Color.White);
                x += button.BoxW + ButtonToWord;
            }
            if (word != null) Art.Draw(word, x, y + (rowH - wh) * 0.5f, Color.White);
        }
    }
}
