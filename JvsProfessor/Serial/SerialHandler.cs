using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using JvsProfessor.JvsHelp;

namespace JvsClientTest.Serial
{
    public class SerialHandler
    {
        private IntPtr _hPort;
        private Thread _rxThread;
        private bool _online;
        private bool _auto;
        private bool _checkSends = true;
        private Exception _rxException;
        private bool _rxExceptionReported;
        private int _writeCount;
        private int _stateRts = 2;
        private int _stateDtr = 2;
        private int _stateBrk = 2;
        //private SerialPort _port;
        public static bool KillMe { get; set; }
        private bool _jvsInitialized = false;

        public JvsInformation JvsInformation = new JvsInformation()
        {
            SyncOk = false
        };

        public void InitSerial(string port)
        {
            var portDcb = new DCB();
            COMSTAT cs;
            uint er;
            _hPort = Win32Com.CreateFile(port, Win32Com.GENERIC_READ | Win32Com.GENERIC_WRITE, 0, IntPtr.Zero,
                Win32Com.OPEN_EXISTING, 0, IntPtr.Zero);
            if (_hPort == (IntPtr)Win32Com.INVALID_HANDLE_VALUE)
            {
                if (Marshal.GetLastWin32Error() == Win32Com.ERROR_ACCESS_DENIED)
                {
                    return;
                }
                throw new CommPortException("Port Open Failure");
            }

            bool isError = Win32Com.ClearCommError(_hPort, out er, out cs);

            Win32Com.SetupComm(_hPort, 516, 516);
            Win32Com.GetCommState(_hPort, ref portDcb);
            //portDcb.Init(false, false, false, 0, false, false, false, false, 0);
            portDcb.BaudRate = 115200;
            portDcb.DCBlength = Marshal.SizeOf(typeof(DCB));
            portDcb.ByteSize = 8;
            portDcb.Parity = 0;
            portDcb.StopBits = 0;

            Win32Com.SetCommState(_hPort, ref portDcb);
            Win32Com.EscapeCommFunction(_hPort, Win32Com.CLRRTS);
            Win32Com.EscapeCommFunction(_hPort, Win32Com.SETDTR);
            Win32Com.SetCommMask(_hPort, Win32Com.EV_RXCHAR);
            Win32Com.GetCommTimeouts(_hPort, out var commTimeouts);

            commTimeouts.ReadTotalTimeoutConstant = 0;
            commTimeouts.ReadTotalTimeoutMultiplier = 0;

            Win32Com.SetCommTimeouts(_hPort, ref commTimeouts);
            JvsClient.HPort = _hPort;
        }

        public void CloseSerial()
        {
            try
            {
                KillMe = true;
                Thread.Sleep(1000);
                Win32Com.CloseHandle(_hPort);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public void RequestJvsInformation()
        {
            try
            {
                while (!JvsClient.send_init_to_master())
                {
                    // wait until finished
                }
                var ioId = JvsClient.GetJvsReply(1, new byte[] {0x10});
                JvsInformation.JvsIdentifier = System.Text.Encoding.Default.GetString(ioId.ToArray()).Replace("\0", " ");
                JvsInformation.CmdFormatVersion = JvsClient.GetJvsReply(1, new byte[] { 0x11 }).Single();
                JvsInformation.JvsVersion = JvsClient.GetJvsReply(1, new byte[] { 0x12 }).Single();
                JvsInformation.CommVersion = JvsClient.GetJvsReply(1, new byte[] { 0x13 }).Single();
                var slaveFeatures = JvsClient.GetJvsReply(1, new byte[] { 0x14 });
                JvsInformation.Features = JvsHelper.ParseSlaveFeatures(slaveFeatures);

                JvsInformation.DigitalBytes = new BitArray((JvsInformation.Features.DigitalPlayerCount * (JvsInformation.Features.DigitalSwitchesPerPlayer / (JvsInformation.Features.DigitalSwitchesPerPlayer / 2)) * 8) + 8);
                JvsInformation.AnalogChannels = new ushort[JvsInformation.Features.AnalogChannels];
                JvsInformation.SyncOk = true;
                while (!KillMe)
                {
                    var digitalBytes = new BitArray(JvsClient.GetJvsReply(1, new byte[] { 0x20, JvsInformation.Features.DigitalPlayerCount, (byte) (JvsInformation.Features.DigitalSwitchesPerPlayer / (JvsInformation.Features.DigitalSwitchesPerPlayer / 2)) }).ToArray());
                    for (var i = 0; i < digitalBytes.Count; i++)
                    {
                        JvsInformation.DigitalBytes[i] = digitalBytes[i];
                    }

                    var analogBytes = JvsClient.GetJvsReply(1, new byte[] { 0x22, JvsInformation.Features.AnalogChannels });
                    for (var i = 0; i < JvsInformation.AnalogChannels.Length; i++)
                    {
                        JvsInformation.AnalogChannels[i] = (ushort) (analogBytes[i * 2] + analogBytes[(i * 2) + 1] * 0x1000);
                    }
                    Thread.Sleep(10);
                }
                }
                catch (Exception e)
                {

                }
        }
    }
}
