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
            var joystick2 = new vJoy();

            uint id = 1;
            uint id2 = 2;

            if (id <= 0 || id > 16)
            {
                Console.WriteLine("Illegal device ID {0}\nExit!", id);
                return;
            }

            // Get the driver attributes (Vendor ID, Product ID, Version Number)
            if (!joystick.vJoyEnabled() || !joystick2.vJoyEnabled())
            {
                Console.WriteLine("vJoy driver not enabled: Failed Getting vJoy attributes.\n");
                return;
            }

            Console.WriteLine("Vendor: {0}\nProduct :{1}\nVersion Number:{2}\n", joystick.GetvJoyManufacturerString(), joystick.GetvJoyProductString(), joystick.GetvJoySerialNumberString());
            Console.WriteLine("Vendor: {0}\nProduct :{1}\nVersion Number:{2}\n", joystick2.GetvJoyManufacturerString(), joystick2.GetvJoyProductString(), joystick2.GetvJoySerialNumberString());
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

            // Get the state of the requested device
            var status2 = joystick2.GetVJDStatus(id2);
            switch (status2)
            {
                case VjdStat.VJD_STAT_OWN:
                    Console.WriteLine("vJoy Device {0} is already owned by this feeder\n", id2);
                    break;
                case VjdStat.VJD_STAT_FREE:
                    Console.WriteLine("vJoy Device {0} is free\n", id2);
                    break;
                case VjdStat.VJD_STAT_BUSY:
                    Console.WriteLine("vJoy Device {0} is already owned by another feeder\nCannot continue\n", id2);
                    return;
                case VjdStat.VJD_STAT_MISS:
                    Console.WriteLine("vJoy Device {0} is not installed or disabled\nCannot continue\n", id2);
                    return;
                default:
                    Console.WriteLine("vJoy Device {0} general error\nCannot continue\n", id2);
                    return;
            };

            // Check which axes are supported
            bool AxisX = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_X);
            bool AxisY = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_Y);
            bool AxisZ = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_Z);
            bool AxisRX = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_RX);
            bool AxisRZ = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_RZ);

            // Check which axes are supported
            bool AxisX2 = joystick2.GetVJDAxisExist(id2, HID_USAGES.HID_USAGE_X);
            bool AxisY2 = joystick2.GetVJDAxisExist(id2, HID_USAGES.HID_USAGE_Y);
            bool AxisZ2 = joystick2.GetVJDAxisExist(id2, HID_USAGES.HID_USAGE_Z);
            bool AxisRX2 = joystick2.GetVJDAxisExist(id2, HID_USAGES.HID_USAGE_RX);
            bool AxisRZ2 = joystick2.GetVJDAxisExist(id2, HID_USAGES.HID_USAGE_RZ);
            // Get the number of buttons and POV Hat switchessupported by this vJoy device
            int nButtons = joystick.GetVJDButtonNumber(id);
            int nButtons2 = joystick2.GetVJDButtonNumber(id2);

            // Test if DLL matches the driver
            UInt32 DllVer = 0, DrvVer = 0;
            bool match = joystick.DriverMatch(ref DllVer, ref DrvVer);
            if (match)
                Console.WriteLine("Version of Driver Matches DLL Version ({0:X})\n", DllVer);
            else
                Console.WriteLine("Version of Driver ({0:X}) does NOT match DLL Version ({1:X})\n", DrvVer, DllVer);

            bool match2 = joystick2.DriverMatch(ref DllVer, ref DrvVer);
            if (match2)
                Console.WriteLine("Version of Driver Matches DLL Version ({0:X})\n", DllVer);
            else
                Console.WriteLine("Version of Driver ({0:X}) does NOT match DLL Version ({1:X})\n", DrvVer, DllVer);

            // Acquire the target 
            if ((status == VjdStat.VJD_STAT_OWN) || ((status == VjdStat.VJD_STAT_FREE) && !joystick.AcquireVJD(id)))
            {
                Console.WriteLine("Failed to acquire vJoy device number {0}.\n", id);
                return;
            }

            // Acquire the target 
            if ((status2 == VjdStat.VJD_STAT_OWN) || ((status == VjdStat.VJD_STAT_FREE) && !joystick2.AcquireVJD(id2)))
            {
                Console.WriteLine("Failed to acquire vJoy device number {0}.\n", id2);
                return;
            }

            Console.WriteLine("Acquired: vJoy device number {0}.\n", id2);
            
            long maxval = 0;

            joystick.GetVJDAxisMax(id, HID_USAGES.HID_USAGE_X, ref maxval);
            joystick2.GetVJDAxisMax(id2, HID_USAGES.HID_USAGE_X, ref maxval);

            // Reset this device to default values
            joystick.ResetVJD(id);
            joystick2.ResetVJD(id2);

            // Feed the device in endless loop
            joystick2.SetBtn(false, 2, 10 );
            while (!KillMe)
            {
                if (Digitals != null && Digitals.Length != 0)
                {
                    for(int i = 0; i < Digitals.Length; i++)
                    {
                        //if(i >= Digitals.Length / 2)
                        //{
                        //    joystick2.SetBtn(Digitals[i], id + 1, (uint)(i - (Digitals.Length / 2)));
                        //}
                        //else
                        //{
                        if(Digitals[i])
                        {
                            // Start
                            SetButton(joystick, joystick2, true, i);
                        }
                        else
                        {
                            SetButton(joystick, joystick2, false, i);
                        }
                        //}
                    }
                    //for (var i = 0; i < Digitals.Length; i++)
                    //{
                    //    joystick.SetBtn(Digitals[i], id, (uint) i);
                    //}
                }

                //if (AnalogChannels != null && AnalogChannels.Length != 0)
                //{
                //    int ptr = 48;
                //    for (int i = 0; i < AnalogChannels.Length; i++)
                //    {
                //        if (i == 1)
                //        {
                //            joystick.SetAxis(CalculateDiFromJvs((byte)AnalogChannels[i], _gasMinimum, _gasMaximum), id, (HID_USAGES)ptr);
                //        }
                //        else if (i == 2)
                //        {
                //            joystick.SetAxis(CalculateDiFromJvs((byte)AnalogChannels[i], _brakeMinimum, _brakeMaximum), id, (HID_USAGES)ptr);
                //        }
                //        else if (i == 0)
                //        {
                //            joystick.SetAxis(CalculateDiFromJvs((byte)AnalogChannels[i], _wheelMinimum, _wheelMaximum), id, (HID_USAGES)ptr);
                //        }
                //        ptr++;
                //    }
                //}
                Thread.Sleep(10);
            }
        }

        private void SetButton(vJoy joystick1, vJoy joystick2, bool active, int index)
        {
            if (active)
                Console.WriteLine("Pressed: " + index.ToString("X2"));
            switch (index)
            {
                // P1 Up
                case 0x0D:
                    if (active)
                        joystick1.SetAxis(0, 1, HID_USAGES.HID_USAGE_Y);
                    break;
                case 0x0C:
                    if (active)
                        joystick1.SetAxis(0x8000, 1, HID_USAGES.HID_USAGE_Y);
                    else
                        joystick1.SetAxis(0x4000, 1, HID_USAGES.HID_USAGE_Y);
                    break;
                case 0x0B:
                    if (active)
                        joystick1.SetAxis(0, 1, HID_USAGES.HID_USAGE_X);
                    break;
                case 0x0A:
                    if (active)
                        joystick1.SetAxis(0x8000, 1, HID_USAGES.HID_USAGE_X);
                    else
                        joystick1.SetAxis(0x4000, 1, HID_USAGES.HID_USAGE_X);
                    break;
                case 0x09:
                    joystick1.SetBtn(active, 1, 1);
                    break;
                case 0x08:
                    joystick1.SetBtn(active, 1, 2);
                    break;
                case 0x17:
                    joystick1.SetBtn(active, 1, 3);
                    break;
                case 0x16:
                    joystick1.SetBtn(active, 1, 4);
                    break;
                case 0x15:
                    joystick1.SetBtn(active, 1, 5);
                    break;
                case 0x14:
                    joystick1.SetBtn(active, 1, 6);
                    break;
                case 0x0F:
                    joystick1.SetBtn(active, 1, 7);
                    break;
                case 0x07:
                    joystick1.SetBtn(active, 1, 8);
                    break;
                case 0x0E:
                    joystick1.SetBtn(active, 1, 9);
                    break;

                // P2
                case 0x1D:
                    if (active)
                        joystick2.SetAxis(0, 2, HID_USAGES.HID_USAGE_Y);
                    break;
                case 0x1C:
                    if (active)
                        joystick2.SetAxis(0x8000, 2, HID_USAGES.HID_USAGE_Y);
                    else
                        joystick2.SetAxis(0x4000, 2, HID_USAGES.HID_USAGE_Y);
                    break;
                case 0x1B:
                    if (active)
                        joystick2.SetAxis(0, 2, HID_USAGES.HID_USAGE_X);
                    break;
                case 0x1A:
                    if (active)
                        joystick2.SetAxis(0x8000, 2, HID_USAGES.HID_USAGE_X);
                    else
                        joystick2.SetAxis(0x4000, 2, HID_USAGES.HID_USAGE_X);
                    break;
                case 0x19:
                    joystick2.SetBtn(active, 2, 1);
                    break;
                case 0x18:
                    joystick2.SetBtn(active, 2, 2);
                    break;
                case 0x27:
                    joystick2.SetBtn(active, 2, 3);
                    break;
                case 0x26:
                    joystick2.SetBtn(active, 2, 4);
                    break;
                case 0x25:
                    joystick2.SetBtn(active, 2, 5);
                    break;
                case 0x24:
                    joystick2.SetBtn(active, 2, 6);
                    break;
                case 0x1F:
                    joystick2.SetBtn(active, 2, 7);
                    break;
                default:
                    if (active)
                        Console.WriteLine("Pressed: " + index.ToString("X2"));
                    break;
            }
        }

        public void StopInjector()
        {
            KillMe = true;
        }
    }
}
