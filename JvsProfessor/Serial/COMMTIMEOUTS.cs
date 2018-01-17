using System;
using System.Runtime.InteropServices;

namespace JvsClientTest.Serial
{
    [StructLayout(LayoutKind.Sequential)]
    public struct COMMTIMEOUTS
    {
        internal Int32 ReadIntervalTimeout;
        internal Int32 ReadTotalTimeoutMultiplier;
        internal Int32 ReadTotalTimeoutConstant;
        internal Int32 WriteTotalTimeoutMultiplier;
        internal Int32 WriteTotalTimeoutConstant;
    }
}