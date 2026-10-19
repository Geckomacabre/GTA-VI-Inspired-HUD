using System;
using System.Windows.Forms;
using GTA;
using GTA.Native;
using ViExtras.NpcMemory;

namespace ViExtras
{
    /// <summary>
    /// The FiveM server's um_memory, for single player: RDR2's Greet / Antagonize
    /// / Defuse as body language, people who notice you without being spoken to,
    /// people and neighbourhoods who remember you, and witnesses who run to
    /// report what you did. [Memory] in ViExtras.ini.
    ///
    /// The police half of um_memory (LSPD / Sheriff notoriety, officers who
    /// recognise you, fenix-police dispatch) is not here: GTA6Hud's own police
    /// memory and police brain do that in single player. A witness's report,
    /// and anything else here that calls the police, only ever raises the
    /// vanilla wanted level.
    ///
    /// GTA6Hud has a stranger conversation of its own (hold aim on someone, then
    /// GREET / TAUNT; GTA6Hud.ini [Interactions] NpcConversations). Turn that off
    /// while this is on; if it stays on, this prompt stands aside while aim is held.
    /// </summary>
    public sealed class Memory : Script
    {
        private readonly bool _enabled;
        private bool _standingRequested;
        private bool _suspended;
        private bool _wasDown;
        private bool _preloaded;
        private int _nextSave, _nextPrune;

        public Memory()
        {
            Paths.Init(BaseDirectory);
            MemCfg.Load();
            _enabled = MemCfg.Enabled;
            if (!_enabled) return;

            Tick += OnTick;
            // Honor's deeds reach the neighbourhood here, as um_memory's server
            // heard qbx_honor:server:hookApplied. Queued: the event fires in the
            // Honor script's turn, and the deed is handled in this one's.
            HonorApi.HookApplied += hook => { lock (_deeds) _deeds.Enqueue(hook); };
            KeyDown += (s, e) =>
            {
                Interact.OnKeyDown(e.KeyCode);
                if (MemCfg.StandingKey != Keys.None && e.KeyCode == MemCfg.StandingKey) _standingRequested = true;
            };
            KeyUp += (s, e) => Interact.OnKeyUp(e.KeyCode);
            Aborted += (s, e) =>
            {
                Witness.Cancel();
                Mem.Flush();
            };
        }

        private static void Safely(string what, Action a)
        {
            try { a(); }
            catch (Exception ex) { Log.Write("Memory", what + ": " + ex); }
        }

        /// <summary>Everything stops for a story mission (unless DuringMissions), a cutscene or a switch.</summary>
        private static bool ShouldSuspend(Ped player) =>
            (Game.IsMissionActive && !MemCfg.DuringMissions) || HudBridge.Busy || player == null || !player.Exists();

        private readonly System.Collections.Generic.Queue<string> _deeds = new System.Collections.Generic.Queue<string>();

        private void OnTick(object sender, EventArgs e)
        {
            if (!GameReady.Now) return;
            Ped player = Game.Player.Character;
            if (player == null || !player.Exists()) return;
            while (true)
            {
                string deed;
                lock (_deeds)
                {
                    if (_deeds.Count == 0) break;
                    deed = _deeds.Dequeue();
                }
                Safely("deed " + deed, () => MemoryApi.ReportDeed(deed));
            }
            if (!_preloaded)
            {
                _preloaded = true;
                Safely("preload", Anims.Preload);
            }

            // Each story character remembers for themselves; what was going on
            // around the last one goes with them (qbx_relog's unload on FiveM).
            if (!HudBridge.Busy)
            {
                string before = Mem.Character;
                Safely("character", () => Mem.Switch(Mem.CharacterOf(player)));
                if (before != null && before != Mem.Character)
                {
                    Witness.Cancel();
                    Crimes.Reset();
                    Ambient.Reset();
                    Recognition.Reset();
                    Interact.Hide();
                }
            }

            // Death or arrest wipes what the police had going, as GTA6Hud does:
            // a witness still running is called off and unfound bodies forgotten.
            bool down = player.IsDead || Function.Call<bool>(Hash.IS_PLAYER_BEING_ARRESTED, Game.Player, true);
            if (down && !_wasDown)
            {
                Witness.Cancel();
                Crimes.Reset();
                Interact.Hide();
            }
            _wasDown = down;

            if (ShouldSuspend(player))
            {
                if (!_suspended)
                {
                    _suspended = true;
                    Witness.Cancel();
                    Later.Clear();
                    Anims.Clear();
                }
                Interact.Hide();   // and no key press waits for afterwards
                SaveNow();
                return;
            }
            _suspended = false;

            Safely("later", Later.Tick);
            Safely("anims", Anims.Tick);
            Safely("crimes", () =>
            {
                Crimes.Frame(player);
                Crimes.Tick(player);
            });
            Safely("witness", Witness.Tick);
            Safely("ambient", () => Ambient.Tick(player));
            Safely("recognition", () => Recognition.Tick(player));
            Safely("interact", () => Interact.Frame(player));

            if (_standingRequested)
            {
                _standingRequested = false;
                Safely("standing", ShowStanding);
            }

            int now = UM.Now;
            if (now >= _nextPrune)
            {
                _nextPrune = now + 10000;
                Safely("prune", Mem.Prune);
            }
            SaveNow();
        }

        private void SaveNow()
        {
            int now = UM.Now;
            if (now < _nextSave) return;
            _nextSave = now + MemCfg.SaveMs;
            Safely("save", Mem.Flush);
        }

        private static string ZoneStanding(float rep)
        {
            if (rep >= 60f) return "beloved";
            if (rep >= 25f) return "well liked";
            if (rep > -25f) return "a stranger here";
            if (rep > -60f) return "disliked";
            return "despised";
        }

        /// <summary>
        /// The FiveM /standing command, on a key. The LSPD and Sheriff halves were
        /// um_memory's police notoriety, which isn't kept here.
        /// </summary>
        private static void ShowStanding()
        {
            string code = UM.ZoneAt(UM.Player.Position, out string name, out _);
            MemoryUi.Notify("In **" + name + "** you are {" + ZoneStanding(Mem.ZoneRep(code)) + "}. People who know your face: " + Mem.FaceCount + ".");
        }
    }

    /// <summary>
    /// um_memory's client exports, for other scripts (commands.lua). The
    /// notoriety ones went with the police side.
    /// </summary>
    public static class MemoryApi
    {
        /// <summary>How `ped` feels about the player, -100..100, and the band: warm, polite, cold or hostile.</summary>
        public static float GetAttitude(Ped ped, out string band)
        {
            band = Attitude.Polite;
            if (ped == null || !UM.IsPerson(ped)) return 0f;
            return Attitude.Of(ped, out band, out _, out _);
        }

        public static float GetZoneReputation(string zoneCode = null) => Mem.ZoneRep(zoneCode ?? UM.ZoneCode(UM.Player.Position));

        public static void AddZoneReputation(float delta, string zoneCode = null) => Mem.AddZone(zoneCode ?? UM.ZoneCode(UM.Player.Position), delta);

        /// <summary>Report a crime from another script: witnesses, memory, the wanted level. `kind` is assault, shooting, carjack, kill, kill_cop, robbery, brandish or petty.</summary>
        public static void ReportCrime(string kind, Ped victim = null)
        {
            if (MemCfg.Enabled && kind != null && MemCfg.Crimes.ContainsKey(kind)) Crimes.Commit(kind, victim);
        }

        /// <summary>
        /// An honor hook that was applied elsewhere (store_robbery, honest_work, ...):
        /// it leaves its mark on the neighbourhood, as um_memory's server did with
        /// qbx_honor:server:hookApplied. The Memory script calls this for every hook
        /// Honor applies (HonorApi.HookApplied).
        /// </summary>
        public static void ReportDeed(string hook)
        {
            if (MemCfg.Enabled) Crimes.Deed(hook);
        }

        /// <summary>
        /// True while Memory's Greet / Antagonize prompt is up on someone. A port
        /// of vice_hud honor's own Greet / Antagonize should stand down while
        /// Memory is enabled, as um_memory's patch to vice_hud did on FiveM.
        /// </summary>
        public static bool PromptUp => Interact.PromptUp;

        /// <summary>[Memory] Enabled and Interact: the greet verbs are Memory's.</summary>
        public static bool OwnsGreeting => MemCfg.Enabled && MemCfg.Interact;
    }
}
