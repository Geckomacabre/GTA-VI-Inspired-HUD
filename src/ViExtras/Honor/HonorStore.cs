using System;
using GTA;

namespace ViExtras
{
    /// <summary>
    /// Where honor is kept between sessions: what the FiveM version kept in
    /// the character's metadata (metadata.honor and metadata.honorBroken), here
    /// one section per story character in Save\Honor.ini, keyed by the player
    /// model. Michael, Franklin, Trevor, Lucia and Jason (ViSwitch's fifth character) each have their own.
    /// </summary>
    internal static class HonorStore
    {
        private static ScriptSettings _file;

        private static ScriptSettings File
        {
            get
            {
                if (_file == null) _file = ScriptSettings.Load(Paths.Save("Honor.ini"));
                return _file;
            }
        }

        // What the story characters' sections are called, in the order of Models().
        private static readonly string[] Names = { "Michael", "Franklin", "Trevor", "Lucia", "Jason" };
        private static int[] _hashes;

        /// <summary>The save section for a player model.</summary>
        public static string KeyFor(int modelHash)
        {
            if (_hashes == null) _hashes = Array.ConvertAll(Models(), Game.GenerateHash);
            for (int i = 0; i < _hashes.Length; i++)
                if (_hashes[i] == modelHash) return Names[i];
            return "Model_" + ((uint)modelHash).ToString("X8");
        }

        // The story characters' models. Lucia's and Jason's are the player's own downloads, named in GTA6Hud.ini
        // ([Lucia] Model, [Jason] Model) as GTA6Hud and ViSwitch read them, so a model under another name keeps
        // that character's honor.
        private static string[] Models()
        {
            string lucia = "lucia_s", jason = "jdfinal";
            try
            {
                ScriptSettings hud = ScriptSettings.Load(System.IO.Path.Combine(Paths.Scripts ?? ".", "GTA6Hud.ini"));
                lucia = hud.GetValue("Lucia", "Model", lucia);
                jason = hud.GetValue("Jason", "Model", jason);
            }
            catch (Exception ex)
            {
                Log.Write("Honor", "GTA6Hud.ini: " + ex.Message);
            }
            return new[]
            {
                "player_zero", "player_one", "player_two",
                string.IsNullOrWhiteSpace(lucia) ? "lucia_s" : lucia.Trim(),
                string.IsNullOrWhiteSpace(jason) ? "jdfinal" : jason.Trim(),
            };
        }

        public static void Load(string key, int fallback, out int value, out bool broken)
        {
            value = fallback;
            broken = false;
            try
            {
                value = File.GetValue(key, "Honor", fallback);
                broken = File.GetValue(key, "Broken", false);
            }
            catch (Exception ex)
            {
                Log.Write("Honor", "load " + key + ": " + ex.Message);
            }
        }

        public static void Save(string key, int value, bool broken)
        {
            try
            {
                File.SetValue(key, "Honor", value);
                File.SetValue(key, "Broken", broken);
                // The file as ScriptSettings.Save writes it, taken now and written off the
                // script thread (SaveFile.WriteLater): a deed doesn't wait on the disk.
                var text = new System.Text.StringBuilder();
                foreach (string section in File.GetAllSectionNames())
                {
                    text.AppendLine("[" + section + "]");
                    foreach (string name in File.GetAllKeyNames(section))
                        text.AppendLine(name + " = " + File.GetValue(section, name, ""));
                    text.AppendLine();
                }
                SaveFile.WriteLater(Paths.Save("Honor.ini"), text.ToString(), "Honor");
            }
            catch (Exception ex)
            {
                Log.Write("Honor", "save " + key + ": " + ex.Message);
            }
        }
    }
}
