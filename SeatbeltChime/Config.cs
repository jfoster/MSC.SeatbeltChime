using MSCLoader;
using System;

namespace SeatbeltChime
{
    class Config
    {
        private Mod mod;

        private Settings satsumaChimeVolume;

        public float SatsumaChimeVolume
        {
            get
            {
                return Convert.ToSingle(satsumaChimeVolume.GetValue());
            }
        }

        public event Action Update;

        public Config(Mod mod)
        {
            this.mod = mod;

            satsumaChimeVolume = new Settings("SatsumaChimeVolume", "Satsuma Chime Volume", 0.5f, OnUpdate);
        }

        public void Load()
        {
            Settings.AddSlider(mod, satsumaChimeVolume, 0.0f, 1.0f);
        }

        public void OnUpdate()
        {
            Update?.Invoke();
        }
    }
}
