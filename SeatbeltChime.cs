using MSCLoader;
using System;
using UnityEngine;

namespace SeatbeltChime
{
    public class SeatbeltChime : Mod
    {
        public override string ID => "me.jfoster.SeatbeltChime"; // Your mod ID (unique)
        public override string Name => "SeatbeltChime"; // You mod name
        public override string Author => "Rave"; // Your Username
        public override string Version => "1.0"; // Version

        // Set this to true if you will be load custom assets from Assets folder.
        // This will create subfolder in Assets folder for your mod.
        public override bool UseAssetsFolder => false;

        private AudioSource chime;

        public override void OnLoad()
        {
            // Called once, when mod is loading after game is fully loaded
            try
            {
                var stream = Properties.Resources.chime;
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);

                chime = new GameObject(Name).AddComponent<AudioSource>();
                chime.clip = WavUtil.ToAudioClip(bytes);
                chime.spatialBlend = 0.75f;
                chime.volume = 0.25f;
                chime.maxDistance = 100.0f;
                chime.minDistance = 10.0f;
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
                GameObject dash = GameObject.Find("SATSUMA(557kg, 248)/Dashboard/pivot_dashboard/dashboard(Clone)");

                bool isDash = dash != null;
                bool isPowered = GameObject.Find("SATSUMA(557kg, 248)/Electricity/PowerON").activeSelf;

                bool isBelted = PlayMakerGlobals.Instance.Variables.FindFsmBool("PlayerSeatbeltsOn").Value;
                bool isDriving = PlayMakerGlobals.Instance.Variables.FindFsmString("PlayerCurrentVehicle").Value == "Satsuma";
                bool isSeated = PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIdrive").Value;

                if (chime.transform.parent == null)
                {
                    chime.transform.parent = dash.transform;
                    chime.transform.localPosition = Vector3.zero;
                }

                if (!isBelted && isPowered && isDash && (isSeated || isDriving))
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
