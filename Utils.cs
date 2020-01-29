using UnityEngine;

namespace SeatbeltChime
{
    class Utils
    {
        public const string Version = ThisAssembly.Git.SemVer.Major + "." + ThisAssembly.Git.SemVer.Minor + "." + ThisAssembly.Git.SemVer.Patch;

        public static AudioClip GetChime()
        {
            var stream = Properties.Resources.chime;
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);

            return WavUtil.ToAudioClip(bytes);
        }
    }
}
