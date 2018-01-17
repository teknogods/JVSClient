using System;
using System.Collections;
using System.Threading;
using vJoyInterfaceWrap;

namespace vJoyInjector
{
    public class VJoyFeeder
    {
        public bool KillMe { get; set; }
        public BitArray Digitals { get; set; }
        public ushort[] AnalogChannels { get; set; }
        private byte _gasMinimum = 0x1F;
        private byte _gasMaximum = 0xD5;
        private byte _wheelMinimum = 0x1E;
        private byte _wheelMaximum = 0xDB;
        private byte _brakeMinimum = 0x1F;
        private byte _brakeMaximum = 0xC1;

        Int32 CalculateDiFromJvs(byte package, byte minimum, byte maximum)
        {
            if (package < minimum)
                return 0;
            if (package > maximum)
                return 0x8000 - 20;
            Int32 value = package - minimum;
            Int32 multiplier = 0x8000 / (maximum - minimum);
            return value * multiplier;
        }

        public void StartInjector()
        {
            // Create one joystick object and a position structure.
            var joystick = new vJoy();
            var iReport = new vJoy.JoystickState();

            uint id = 1;

            if (id <= 0 || id > 16)
            {
                Console.WriteLine("Illegal device ID {0}\nExit!", id);
                return;
            }

            // Get the driver attributes (Vendor ID, Product ID, Version Number)
            if (!joystick.vJoyEnabled())
            {
                Console.WriteLine("vJoy driver not enabled: Failed Getting vJoy attributes.\n");
                return;
            }

            Console.WriteLine("Vendor: {0}\nProduct :{1}\nVersion Number:{2}\n", joystick.GetvJoyManufacturerString(), joystick.GetvJoyProductString(), joystick.GetvJoySerialNumberString());

            // Get the state of the requested device
            VjdStat status = joystick.GetVJDStatus(id);
            switch (status)
            {
                case VjdStat.VJD_STAT_OWN:
                    Console.WriteLine("vJoy Device {0} is already owned by this feeder\n", id);
                    break;
                case VjdStat.VJD_STAT_FREE:
                    Console.WriteLine("vJoy Device {0} is free\n", id);
                    break;
                case VjdStat.VJD_STAT_BUSY:
                    Console.WriteLine("vJoy Device {0} is already owned by another feeder\nCannot continue\n", id);
                    return;
                case VjdStat.VJD_STAT_MISS:
                    Console.WriteLine("vJoy Device {0} is not installed or disabled\nCannot continue\n", id);
                    return;
                default:
                    Console.WriteLine("vJoy Device {0} general error\nCannot continue\n", id);
                    return;
            };

            // Check which axes are supported
            bool AxisX = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_X);
            bool AxisY = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_Y);
            bool AxisZ = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_Z);
            bool AxisRX = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_RX);
            bool AxisRZ = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_RZ);
            // Get the number of buttons and POV Hat switchessupported by this vJoy device
            int nButtons = joystick.GetVJDButtonNumber(id);

            // Test if DLL matches the driver
            UInt32 DllVer = 0, DrvVer = 0;
            bool match = joystick.DriverMatch(ref DllVer, ref DrvVer);
            if (match)
                Console.WriteLine("Version of Driver Matches DLL Version ({0:X})\n", DllVer);
            else
                Console.WriteLine("Version of Driver ({0:X}) does NOT match DLL Version ({1:X})\n", DrvVer, DllVer);


            // Acquire the target
            if ((status == VjdStat.VJD_STAT_OWN) || ((status == VjdStat.VJD_STAT_FREE) && (!joystick.AcquireVJD(id))))
            {
                Console.WriteLine("Failed to acquire vJoy device number {0}.\n", id);
                return;
            }

            Console.WriteLine("Acquired: vJoy device number {0}.\n", id);
            
            long maxval = 0;

            joystick.GetVJDAxisMax(id, HID_USAGES.HID_USAGE_X, ref maxval);
            
            // Reset this device to default values
            joystick.ResetVJD(id);

            // Feed the device in endless loop
            while (!KillMe)
            {
                if (Digitals != null && Digitals.Length != 0)
                {
                    for (var i = 0; i < Digitals.Length; i++)
                    {
                        joystick.SetBtn(Digitals[i], id, (uint) i);
                    }
                }

                if (AnalogChannels != null && AnalogChannels.Length != 0)
                {
                    int ptr = 48;
                    for (int i = 0; i < AnalogChannels.Length; i++)
                    {
                        if (i == 1)
                        {
                            joystick.SetAxis(CalculateDiFromJvs((byte)AnalogChannels[i], _gasMinimum, _gasMaximum), id, (HID_USAGES)ptr);
                        }
                        else if (i == 2)
                        {
                            joystick.SetAxis(CalculateDiFromJvs((byte)AnalogChannels[i], _brakeMinimum, _brakeMaximum), id, (HID_USAGES)ptr);
                        }
                        else if (i == 0)
                        {
                            joystick.SetAxis(CalculateDiFromJvs((byte)AnalogChannels[i], _wheelMinimum, _wheelMaximum), id, (HID_USAGES)ptr);
                        }
                        ptr++;
                    }
                }
                Thread.Sleep(10);
            }
        }

        public void StopInjector()
        {
            KillMe = true;
        }
    }
}
