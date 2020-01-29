using MSCLoader;
using System;

namespace SeatbeltChime
{
    class Settings
    {
        private Mod mod;

        public event Action Update;

        public MSCLoader.Settings volume;

        public Settings(Mod mod)
        {
            this.mod = mod;

            volume = new MSCLoader.Settings("Volume", "Chime Volume", 0.5f, OnUpdate);
        }

        public void OnUpdate()
        {
            Update?.Invoke();
        }

        public float GetVolume()
        {
            return Convert.ToSingle(volume.GetValue());
        }

        public void Load()
        {
            MSCLoader.Settings.AddSlider(mod, volume, 0.0f, 1.0f);
        }
    }
}
