using MSCLoader;
using System;
using UnityEngine;

namespace SeatbeltChime
{
    public class SeatbeltChime : Mod
    {
        private Settings settings;

        private AudioSource chime;

        public override void ModSettings()
        {
            settings = new Settings(this);
            settings.Update += OnSettingsUpdate;
            settings.Load();
        }

        private void OnSettingsUpdate()
        {
            chime.volume = settings.GetVolume();
        }

        public override void OnLoad()
        {
            try
            {
                chime = new GameObject(Name).AddComponent<AudioSource>();
                chime.clip = Utils.GetChime();
                chime.spatialBlend = 0.75f;
                chime.maxDistance = 100.0f;
                chime.minDistance = 10.0f;
                chime.volume = settings.GetVolume();
            }
            catch (Exception ex)
            {
                ModConsole.Error(string.Format("Error: {0}\r\nStacktrace: {1}", ex.Source, ex.StackTrace));
            }
        }

        public override void Update()
        {
            try
            {
                GameObject dash = GameObject.Find("SATSUMA(557kg, 248)/Dashboard/pivot_dashboard/dashboard(Clone)");

                bool isDashInstalled = dash != null;
                bool isCarPowered = GameObject.Find("SATSUMA(557kg, 248)/Electricity/PowerON").activeSelf;

                bool isBelted = PlayMakerGlobals.Instance.Variables.FindFsmBool("PlayerSeatbeltsOn").Value;
                bool isDriving = PlayMakerGlobals.Instance.Variables.FindFsmString("PlayerCurrentVehicle").Value == "Satsuma";

                if (chime.transform.parent == null)
                {
                    chime.transform.parent = dash.transform;
                    chime.transform.localPosition = Vector3.zero;
                }

                if (isDashInstalled && isCarPowered && isDriving && !isBelted && !chime.isPlaying)
                {
                    chime.loop = true;
                    chime.Play();
                }
                else if (chime.isPlaying)
                {
                    chime.loop = false;
                }
            }
            catch (Exception ex)
            {
                ModConsole.Error(string.Format("Error: {0}\r\nStacktrace: {1}", ex.Source, ex.StackTrace));
            }
        }

        public override string ID => "xyz.jfoster.SeatbeltChime"; // Your mod ID (unique)
        public override string Name => "SeatbeltChime"; // You mod name
        public override string Author => "Rave"; // Your Username
        public override string Version => Utils.Version; // Version

        // Set this to true if you will be load custom assets from Assets folder.
        // This will create subfolder in Assets folder for your mod.
        public override bool UseAssetsFolder => false;
    }
}
