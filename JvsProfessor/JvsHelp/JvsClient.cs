using System;
using System.Collections.Generic;
using System.Threading;
using JvsClientTest.Serial;
using static JvsProfessor.JvsHelp.JvsHelper;

namespace JvsProfessor.JvsHelp
{
    public static class JvsClient
    {

        private static byte JVS_DEVICE_COUNT = 0;
        public static IntPtr HPort;
        internal static bool send_init_to_master()
        {
            uint retval = 0;
            COMSTAT cs;
            uint er;
            writePacket(JVS_ADDR_BROADCAST, new[] { JVS_OP_RESET, JVS_OP_RESET_ARG });
            if (!Win32Com.ClearCommError(HPort, out er, out cs))
                return false;
            if (!Win32Com.EscapeCommFunction(HPort, Win32Com.SETRTS))
                return false;
            Thread.Sleep(1000);
            JVS_DEVICE_COUNT = 0;
            while (JVS_DEVICE_COUNT != 1)
            {
                JVS_DEVICE_COUNT++;
                //info(true, "Sending reset to JVS %08d", JVS_DEVICE_COUNT);
                send_init_to_JVS_board(JVS_DEVICE_COUNT);
            }
            return true;
        }

        private static bool send_init_to_JVS_board(byte node)
        {
            writePacket(JVS_ADDR_BROADCAST, new[] { JVS_OP_ADDRESS, node });
            Thread.Sleep(1000);
            return ReadTheComData();
        }

        private static void sendByte(byte b)
        {
            uint written = 0;
            Win32Com.WriteFile(HPort, new[] { b }, 1, out written, IntPtr.Zero);
        }

        private static byte readByte(out uint bytesRead)
        {
            byte[] c = new byte[256];
            Win32Com.ReadFile(HPort, c, 1, out bytesRead, IntPtr.Zero);
            return c[0];
        }

        private static void writePacket(byte node, byte[] data)
        {
            var bytes = new List<byte>();
            bytes.AddRange(data);
            bytes.Add(CalcChecksum(node, data, data.Length));

            sendByte(JVS_SYNC_CODE);
            sendByte(node);
            sendByte((byte)bytes.Count);
            for (int i = 0; i < bytes.Count; i++)
            {
                if (bytes[i] == JVS_SYNC_CODE || bytes[i] == JVS_ESCAPE_CODE)
                {
                    sendByte(JVS_ESCAPE_CODE);
                    sendByte((byte)(bytes[i] - 1));
                }
                else
                {
                    sendByte(bytes[i]);
                }
            }
        }

        private static bool wait_for_read_event()
        {
            //return WaitCommEvent(hPort, &event, NULL);
            return true;
        }

        private static bool ReadTheComData()
        {
            COMSTAT cs;
            uint er;
            if (!Win32Com.ClearCommError(HPort, out er, out cs))
                return false;
            if (!Win32Com.EscapeCommFunction(HPort, Win32Com.CLRRTS))
                return false;
            Thread.Sleep(1000);

            uint count = 0;
            uint Err;
            COMSTAT CST;

            while (true)
            {
                Win32Com.ClearCommError(HPort, out Err, out CST);

                if ((CST.cbInQue > 0) || (count > 1000000))
                    break;
                count++;
            }

            var result = readPacket();

            if (result[0] != JVS_STATUS_OK)
            {
                return false;
            }
            if (result[1] != JVS_REPORT_OK)
            {
                return false;
            }

            return true;

        }

        private static List<byte> readPacket()
        {
            uint c, dest, data_length, csum, csum_rcvd;
            var result = new List<byte>();
            uint read = 0;

            if (wait_for_read_event())
            {
                while (true)
                {
                    c = readByte(out read);
                    if (read == 0)
                    {
                        Console.WriteLine("the server did not answer. bytes read from com port == 0");
                        break;
                    }
                    if (c == JVS_SYNC_CODE)
                    {

                        dest = readByte(out read);
                        data_length = (uint)(readByte(out read) - 1);
                        for (int i = 0; i < data_length; i++)
                        {
                            var value = readByte(out read);
                            if (value == JVS_ESCAPE_CODE)
                                value = (byte)(readByte(out read) + 1);
                            result.Add(value);
                        }
                        csum_rcvd = readByte(out read);
                        csum = CalcChecksum((int)dest, result.ToArray(), result.Count);
                        if (csum != csum_rcvd)
                        {
                            Console.WriteLine($"Packet checksum mismatch: {csum.ToString("X2")}  (calculated) != {csum_rcvd.ToString("X2")}  (received)");
                        }

                        break;
                    }

                }
            }
            else
            {
                Console.WriteLine("no read event occurred");
            }
            return result;
        }

        internal static List<byte> GetJvsReply(byte node, byte[] package)
        {
            List<byte> result = new List<byte>();

            writePacket(node, package);

            //if (!ClearCommError(hPort, 0, 0))
            //return false;
            if (!Win32Com.EscapeCommFunction(HPort, Win32Com.SETRTS))
                return result;

            uint count = 0;
            uint Err;
            COMSTAT CST;

            while (true)
            {
                Win32Com.ClearCommError(HPort, out Err, out CST);

                if ((CST.cbInQue > 0) || (count > 1000000))
                    break;
                count++;
            }
            result.AddRange(readPacket());

            if (result[0] != JVS_STATUS_OK)
            {
                Console.WriteLine("JVS Status not ok!");
                return null;
            }
            if (result[1] != JVS_REPORT_OK)
            {
                Console.WriteLine("JVS Report not ok!");
                return null;
            }

            //Console.WriteLine("Reply ok!");

            //info(true, "first eight bytes from reply buffer: %02x %02x %02x %02x %02x %02x %02x %02x", readData[0], readData[1], readData[2], readData[3], readData[4], readData[5], readData[6], readData[7]);

            // We remove status and report.
            result.RemoveAt(0);
            result.RemoveAt(0);

            return result;
        }
    }
}
