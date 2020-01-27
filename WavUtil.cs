using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace SeatbeltChime
{
    public static class WavUtil
    {
        const int BlockSize_16Bit = 2;

        public static byte[] FromAudioClip(AudioClip audioClip, int offsetSamples = 0, Boolean includeWavFileHeader = true)
        {
            int samples = (offsetSamples == 0) ? audioClip.samples : offsetSamples;
            // Copy float[] data from AudioClip
            float[] data = new float[samples * audioClip.channels];
            audioClip.GetData(data, 0);
            return FromAudioClipData(data, samples, audioClip.channels, audioClip.frequency, includeWavFileHeader);
        }

        public static byte[] FromAudioClip(AudioClip audioClip, int currentPosition, int lastPosition = 0, Boolean includeWavFileHeader = true)
        {
            int samples = currentPosition - lastPosition;
            // Copy float[] data from AudioClip
            float[] data = new float[samples * audioClip.channels];
            audioClip.GetData(data, lastPosition);
            return FromAudioClipData(data, samples, audioClip.channels, audioClip.frequency, includeWavFileHeader);
        }

        public static byte[] FromAudioClipData(float[] audioClipData, int samples, int channels, int frequency, Boolean includeWavFileHeader)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                int headerSize = includeWavFileHeader ? 44 : 0;
                UInt16 bitDepth = 16; // bit depth from audio clip must be 16 bit!

                // total file size = 44 bytes for header format and audioClip.samples * factor due to float to Int16 / sbyte conversion
                int fileSize = samples * BlockSize_16Bit + headerSize;

                if (includeWavFileHeader)
                {
                    // chunk descriptor (riff)
                    WriteWavFileHeader(stream, fileSize);
                    // file header (fmt)
                    WriteWavFileFormat(stream, channels, frequency, bitDepth);
                }

                // wav data chunks (data)
                WriteWavData(stream, audioClipData, samples, bitDepth, includeWavFileHeader);

                byte[] bytes = stream.ToArray();

                // Output wav bytes
                return bytes;
            }
        }

        private static int WriteWavFileHeader(MemoryStream stream, int fileSize)
        {
            int count = 0;

            // riff chunk id
            byte[] riff = Encoding.ASCII.GetBytes("RIFF");
            count += WriteBytesToMemoryStream(stream, riff, "ID");

            // riff chunk size
            int chunkSize = fileSize - 8; // total size - 8 for the other two fields in the header
            count += WriteBytesToMemoryStream(stream, BitConverter.GetBytes(chunkSize), "CHUNK_SIZE");

            byte[] wave = Encoding.ASCII.GetBytes("WAVE");
            count += WriteBytesToMemoryStream(stream, wave, "FORMAT");

            return count;
        }

        private static int WriteWavFileFormat(MemoryStream stream, int channels, int sampleRate, UInt16 bitDepth)
        {
            int count = 0;

            byte[] id = Encoding.ASCII.GetBytes("fmt ");
            count += WriteBytesToMemoryStream(stream, id, "FMT_ID");

            int subchunk1Size = 16; // 24 - 8
            count += WriteBytesToMemoryStream(stream, BitConverter.GetBytes(subchunk1Size), "SUBCHUNK_SIZE");

            UInt16 audioFormat = 1;
            count += WriteBytesToMemoryStream(stream, BitConverter.GetBytes(audioFormat), "AUDIO_FORMAT");

            UInt16 numChannels = Convert.ToUInt16(channels);
            count += WriteBytesToMemoryStream(stream, BitConverter.GetBytes(numChannels), "CHANNELS");

            count += WriteBytesToMemoryStream(stream, BitConverter.GetBytes(sampleRate), "SAMPLE_RATE");

            int byteRate = sampleRate * channels * BytesPerSample(bitDepth);
            count += WriteBytesToMemoryStream(stream, BitConverter.GetBytes(byteRate), "BYTE_RATE");

            UInt16 blockAlign = Convert.ToUInt16(channels * BytesPerSample(bitDepth));
            count += WriteBytesToMemoryStream(stream, BitConverter.GetBytes(blockAlign), "BLOCK_ALIGN");

            count += WriteBytesToMemoryStream(stream, BitConverter.GetBytes(bitDepth), "BITS_PER_SAMPLE");

            return count;
        }

        private static int WriteWavData(MemoryStream stream, float[] audioClipData, int samples, UInt16 bitDepth, Boolean includeWavFileHeader)
        {
            int count = 0;

            byte[] bytes = ConvertAudioClipDataToInt16ByteArray(audioClipData);
            int subchunk2Size = Convert.ToInt32(samples * BlockSize_16Bit);

            if (includeWavFileHeader)
            {
                byte[] id = Encoding.ASCII.GetBytes("data");
                count += WriteBytesToMemoryStream(stream, id, "DATA_ID");
                count += WriteBytesToMemoryStream(stream, BitConverter.GetBytes(subchunk2Size), "SAMPLES");
            }

            // Write bytes to stream
            count += WriteBytesToMemoryStream(stream, bytes, "DATA");

            return count;
        }

        private static int WriteBytesToMemoryStream(MemoryStream stream, byte[] bytes, string tag = "")
        {
            int count = bytes.Length;
            stream.Write(bytes, 0, count);
            //Debug.LogFormat ("WAV:{0} wrote {1} bytes.", tag, count);
            return count;
        }

        private static byte[] ConvertAudioClipDataToInt16ByteArray(float[] data)
        {
            using (MemoryStream dataStream = new MemoryStream())
            {
                int x = sizeof(Int16);

                Int16 maxValue = Int16.MaxValue;

                int i = 0;
                while (i < data.Length)
                {
                    dataStream.Write(BitConverter.GetBytes(Convert.ToInt16(data[i] * maxValue)), 0, x);
                    ++i;
                }
                byte[] bytes = dataStream.ToArray();

                return bytes;
            }
        }

        private static int BytesPerSample(UInt16 bitDepth)
        {
            return bitDepth / 8;
        }

        #region Wav bytes to Unity AudioClip

        public static AudioClip ToAudioClip(byte[] fileBytes, string name = "wav")
        {
            int headerOffset = 0;
            Int32 sampleRate = 16000;
            UInt16 channels = 1;
            Int32 subchunk2 = fileBytes.Length;

            // check for RIF header
            Boolean includeWavFileHeader = true;
            byte[] fileHeaderChars = new byte[4];
            Array.Copy(fileBytes, 0, fileHeaderChars, 0, 4);
            string fileHeader = Encoding.ASCII.GetString(fileHeaderChars);
            if (!fileHeader.Equals("RIFF"))
            {
                includeWavFileHeader = false;
            }

            if (includeWavFileHeader)
            {
                Int32 subchunk1 = BitConverter.ToInt32(fileBytes, 16);

                // NB: Only uncompressed PCM wav files are supported.
                UInt16 audioFormat = BitConverter.ToUInt16(fileBytes, 20);

                channels = BitConverter.ToUInt16(fileBytes, 22);
                sampleRate = BitConverter.ToInt32(fileBytes, 24);
                UInt16 bitDepth = BitConverter.ToUInt16(fileBytes, 34);

                headerOffset = 16 + 4 + subchunk1 + 4;
                subchunk2 = BitConverter.ToInt32(fileBytes, headerOffset);
            }

            float[] data;
            data = Convert16BitByteArrayToAudioClipData(fileBytes, headerOffset, subchunk2);

            AudioClip audioClip = AudioClip.Create(name, data.Length, (int)channels, sampleRate, false);
            audioClip.SetData(data, 0);
            return audioClip;
        }

        private static float[] Convert16BitByteArrayToAudioClipData(byte[] source, int headerOffset, int dataSize)
        {
            int wavSize = dataSize;

            // only required if there is a header
            if (headerOffset != 0)
            {
                wavSize = BitConverter.ToInt32(source, headerOffset);
                headerOffset += sizeof(int);
            }

            int x = sizeof(Int16); // block size = 2
            int convertedSize = wavSize / x;

            //Debug.Log ("Convert header offset: " + headerOffset + " dataSize: " + dataSize + " size: " + convertedSize);

            float[] data = new float[convertedSize];

            Int16 maxValue = Int16.MaxValue;
            int i = 0;
            while (i < convertedSize)
            {
                int offset = i * x + headerOffset;
                data[i] = (float)BitConverter.ToInt16(source, offset) / maxValue;
                ++i;
            }

            return data;
        }

        #endregion

    }
}