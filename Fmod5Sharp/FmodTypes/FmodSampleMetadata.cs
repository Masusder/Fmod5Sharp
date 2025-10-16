using Fmod5Sharp.Util;
using System.Collections.Generic;
using System.IO;

namespace Fmod5Sharp.FmodTypes
{
    public class FmodSampleMetadata : IBinaryReadable
    {
        internal bool HasAnyChunks;
        internal uint FrequencyId;
        internal ulong DataOffset;
        internal List<FmodSampleChunk> Chunks = new();
        internal int NumChannels;

        public bool IsStereo;
        public ulong SampleCount;

        public int Frequency => FsbLoader.Frequencies.TryGetValue(FrequencyId, out var actualFrequency) ? actualFrequency : (int)FrequencyId; // If set by FREQUENCY chunk, id is actual frequency
        public uint Channels => (uint)NumChannels;

        void IBinaryReadable.Read(BinaryReader reader)
        {
            var sampleMode = reader.ReadUInt64();

            HasAnyChunks = (sampleMode & 1) == 1; // Bit 0
            FrequencyId = (uint)sampleMode.Bits(1, 4); // Bits 1-4

            int channelBits = (int)sampleMode.Bits(5, 2); // Bits 5-6
            NumChannels = channelBits switch
            {
                0 => 1,
                1 => 2,
                2 => 6,
                3 => 8,
                _ => throw new("Number of channels not supported"),
            };
            IsStereo = NumChannels == 2;

            DataOffset = sampleMode.Bits(7, 27) * 32;
            SampleCount = sampleMode.Bits(34, 30);
        }
    }
}