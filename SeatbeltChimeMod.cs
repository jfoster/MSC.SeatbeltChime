using MSCLoader;
using System;
using UnityEngine;

namespace SeatbeltChime
{
    public class SeatbeltChimeMod : Mod
    {
        public override string ID => "Rave.SeatbeltChime"; // Your mod ID (unique)
        public override string Name => "SeatbeltChime"; // You mod name
        public override string Author => "Rave"; // Your Username
        public override string Version => "1.0"; // Version

        // Set this to true if you will be load custom assets from Assets folder.
        // This will create subfolder in Assets folder for your mod.
        public override bool UseAssetsFolder => true;

        private AudioSource chime;

        public override void OnLoad()
        {
            // Called once, when mod is loading after game is fully loaded
            try
            {
                WWW wav = new WWW("file:///" + System.IO.Path.Combine(ModLoader.GetModAssetsFolder(this), "chime.wav"));
                while (!wav.isDone)
                {
                }
                chime = new GameObject(Name).AddComponent<AudioSource>();
                chime.clip = wav.GetAudioClip(true);
                chime.spatialBlend = 0.75f;
                chime.volume = 0.25f;
                chime.maxDistance = 100.0f;
                chime.minDistance = 10.0f;
                chime.transform.parent = GameObject.Find("SATSUMA(557kg, 248)/Dashboard/pivot_dashboard/dashboard(Clone)").transform;
                chime.transform.localPosition = Vector3.zero;
            }
            catch (Exception ex)
            {
                ModConsole.Error(string.Format("Error: {0}\r\nStacktrace: {1}", ex.Source, ex.StackTrace));
            }
        }

        public override void Update()
        {
            // Update is called once per frame
            try
            {
                bool isBelted = PlayMakerGlobals.Instance.Variables.FindFsmBool("PlayerSeatbeltsOn").Value;
                bool isDriving = PlayMakerGlobals.Instance.Variables.FindFsmString("PlayerCurrentVehicle").Value == "Satsuma";
                bool isPowered = GameObject.Find("SATSUMA(557kg, 248)/Electricity/PowerON").activeSelf;
                bool isSeated = PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIdrive").Value;

                if (!isBelted && isPowered && (isSeated || isDriving))
                {
                    if (!chime.isPlaying)
                    {
                        chime.loop = true;
                        chime.Play();
                    }
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
    }
}
