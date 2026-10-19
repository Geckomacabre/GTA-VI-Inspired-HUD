using System;
using GTA;

namespace ViExtras
{
    /// <summary>
    /// Where honor is kept between sessions: what the FiveM version kept in
    /// the character's metadata (metadata.honor and metadata.honorBroken), here
    /// one section per story character in Save\Honor.ini, keyed by the player
    /// model. Michael, Franklin, Trevor and Lucia each have their own.
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

        // The story characters' models, and what their sections are called.
        private static readonly string[] Models = { "player_zero", "player_one", "player_two", "lucia_s" };
        private static readonly string[] Names = { "Michael", "Franklin", "Trevor", "Lucia" };
        private static int[] _hashes;

        /// <summary>The save section for a player model.</summary>
        public static string KeyFor(int modelHash)
        {
            if (_hashes == null) _hashes = Array.ConvertAll(Models, Game.GenerateHash);
            for (int i = 0; i < _hashes.Length; i++)
                if (_hashes[i] == modelHash) return Names[i];
            return "Model_" + ((uint)modelHash).ToString("X8");
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
                File.Save();
            }
            catch (Exception ex)
            {
                Log.Write("Honor", "save " + key + ": " + ex.Message);
            }
        }
    }
}
