using System;
using System.Collections;
using System.Collections.Generic;

namespace JvsProfessor.JvsHelp
{
    public class JvsInformation
    {
        public string JvsIdentifier { get; set; }
        public byte CmdFormatVersion { get; set; }
        public byte JvsVersion { get; set; }
        public byte CommVersion { get; set; }
        public SlaveInformation Features { get; set; }
        public BitArray DigitalBytes { get; set; }
        public ushort[] AnalogChannels { get; set; }
        public bool SyncOk { get; set; }
    }

    public class SlaveInformation
    {
        public byte DigitalPlayerCount { get; set; }
        public byte DigitalSwitchesPerPlayer { get; set; }
        public byte AnalogChannels { get; set; }
        public byte AnalogBitsPerChannel { get; set; }
        public byte CoinSlots { get; set; }
        public byte OutputCount { get; set; }

    }
    public static class JvsHelper
    {
        public const byte JVS_BROADCAST = 0xFF;
        public const byte JVS_OP_RESET = 0xF0;
        public const byte JVS_OP_ADDRESS = 0xF1;
        public const byte JVS_SYNC_CODE = 0xE0;
        public const byte JVS_TRUE = 0x01;
        public const byte JVS_REPORT_OK = 0x01;
        public const byte JVS_REPORT_ERROR1 = 0x02;
        public const byte JVS_REPORT_ERROR2 = 0x03;
        public const byte JVS_REPORT_DEVICE_BUSY = 0x04;
        public const byte JVS_STATUS_OK = 0x01;
        public const byte JVS_STATUS_UNKNOWN = 0x02;
        public const byte JVS_STATUS_CHECKSUM_FAIL = 0x03;
        public const byte JVS_STATUS_OVERFLOW = 0x04;
        public const byte JVS_ADDR_MASTER = 0x00;
        public const byte JVS_COMMAND_REV = 0x13;
        public const byte JVS_READID_DATA = 0x10;
        public const byte JVS_READ_DIGITAL = 0x20;
        public const byte JVS_READ_COIN = 0x21;
        public const byte JVS_READ_ANALOG = 0x22;
        public const byte JVS_READ_ROTATORY = 0x23;
        public const byte JVS_COIN_NORMAL_OPERATION = 0x00;
        public const byte JVS_COIN_COIN_JAM = 0x01;
        public const byte JVS_COIN_SYSTEM_DISCONNECTED = 0x02;
        public const byte JVS_COIN_SYSTEM_BUSY = 0x03;
        public const byte JVS_ESCAPE_CODE = 0xD0;
        public const byte JVS_ADDR_BROADCAST = 0xFF;
        public const byte JVS_OP_RESET_ARG = 0xD9;

        /// <summary>
        /// Calculates JVS checksum.
        /// </summary>
        /// <param name="dest">Destination node.</param>
        /// <param name="bytes">The data.</param>
        /// <param name="length">Length</param>
        /// <returns></returns>
        internal static byte CalcChecksum(int dest, byte[] bytes, int length)
        {
            var csum = dest + length + 1;

            for (var i = 0; i < length; i++)
                csum = (csum + bytes[i]) % 256;

            return (byte)csum;
        }

        internal static SlaveInformation ParseSlaveFeatures(List<byte> features)
        {
            var result = new SlaveInformation();

            for (var i = 0; i < features.Count; i += 4)
            {
                switch (features[i])
                {
                    case 0x00:
                        return result;
                    case 0x01:
                    {
                        result.DigitalPlayerCount = features[i + 1];
                        result.DigitalSwitchesPerPlayer = features[i + 2];
                        continue;
                    }
                    case 0x02:
                    {
                        result.CoinSlots = features[i + 1];
                        continue;
                    }
                    case 0x03:
                    {
                        result.AnalogChannels = features[i + 1];
                        result.AnalogBitsPerChannel = features[i + 2];
                        continue;
                    }
                    case 0x12:
                    {
                        result.OutputCount = features[i + 1];
                        continue;
                    }
                }
            }

            return result;
        }
    }
}
