using System;
using System.Runtime.InteropServices;

namespace JvsClientTest.Serial
{
    public class Win32Com
    {
        /// <summary>
        /// Opening Testing and Closing the Port Handle.
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateFile(String lpFileName, UInt32 dwDesiredAccess, UInt32 dwShareMode,
            IntPtr lpSecurityAttributes, UInt32 dwCreationDisposition, UInt32 dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        //Constants for errors:
        public const UInt32 ERROR_FILE_NOT_FOUND = 2;
        public const UInt32 ERROR_INVALID_NAME = 123;
        public const UInt32 ERROR_ACCESS_DENIED = 5;
        public const UInt32 ERROR_IO_PENDING = 997;

        //Constants for return value:
        public const Int32 INVALID_HANDLE_VALUE = -1;

        //Constants for dwFlagsAndAttributes:
        public const UInt32 FILE_FLAG_OVERLAPPED = 0x40000000;

        //Constants for dwCreationDisposition:
        public const UInt32 OPEN_EXISTING = 3;

        //Constants for dwDesiredAccess:
        public const UInt32 GENERIC_READ = 0x80000000;
        public const UInt32 GENERIC_WRITE = 0x40000000;

        [DllImport("kernel32.dll")]
        public static extern Boolean CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        public static extern Boolean GetHandleInformation(IntPtr hObject, out UInt32 lpdwFlags);


        /// <summary>
        /// Manipulating the communications settings.
        /// </summary>

        [DllImport("kernel32.dll")]
        public static extern Boolean GetCommState(IntPtr hFile, ref DCB lpDCB);

        [DllImport("kernel32.dll")]
        public static extern Boolean GetCommTimeouts(IntPtr hFile, out COMMTIMEOUTS lpCommTimeouts);

        [DllImport("kernel32.dll")]
        public static extern Boolean BuildCommDCBAndTimeouts(String lpDef, ref DCB lpDCB, ref COMMTIMEOUTS lpCommTimeouts);

        [DllImport("kernel32.dll")]
        public static extern Boolean SetCommState(IntPtr hFile, [In] ref DCB lpDCB);

        [DllImport("kernel32.dll")]
        public static extern Boolean SetCommTimeouts(IntPtr hFile, [In] ref COMMTIMEOUTS lpCommTimeouts);

        [DllImport("kernel32.dll")]
        public static extern Boolean SetupComm(IntPtr hFile, UInt32 dwInQueue, UInt32 dwOutQueue);

        /// <summary>
        /// Reading and writing.
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern Boolean WriteFile(IntPtr fFile, Byte[] lpBuffer, UInt32 nNumberOfBytesToWrite,
            out UInt32 lpNumberOfBytesWritten, IntPtr lpOverlapped);

        [DllImport("kernel32.dll")]
        public static extern Boolean SetCommMask(IntPtr hFile, UInt32 dwEvtMask);

        // Constants for dwEvtMask:
        public const UInt32 EV_RXCHAR = 0x0001;
        public const UInt32 EV_RXFLAG = 0x0002;
        public const UInt32 EV_TXEMPTY = 0x0004;
        public const UInt32 EV_CTS = 0x0008;
        public const UInt32 EV_DSR = 0x0010;
        public const UInt32 EV_RLSD = 0x0020;
        public const UInt32 EV_BREAK = 0x0040;
        public const UInt32 EV_ERR = 0x0080;
        public const UInt32 EV_RING = 0x0100;
        public const UInt32 EV_PERR = 0x0200;
        public const UInt32 EV_RX80FULL = 0x0400;
        public const UInt32 EV_EVENT1 = 0x0800;
        public const UInt32 EV_EVENT2 = 0x1000;

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern Boolean WaitCommEvent(IntPtr hFile, IntPtr lpEvtMask, IntPtr lpOverlapped);

        [DllImport("kernel32.dll")]
        public static extern Boolean CancelIo(IntPtr hFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern Boolean ReadFile(IntPtr hFile, [Out] Byte[] lpBuffer, UInt32 nNumberOfBytesToRead,
            out UInt32 nNumberOfBytesRead, IntPtr lpOverlapped);

        [DllImport("kernel32.dll")]
        public static extern Boolean TransmitCommChar(IntPtr hFile, Byte cChar);

        /// <summary>
        /// Control port functions.
        /// </summary>
        [DllImport("kernel32.dll")]
        public static extern Boolean EscapeCommFunction(IntPtr hFile, UInt32 dwFunc);

        // Constants for dwFunc:
        public const UInt32 SETXOFF = 1;
        public const UInt32 SETXON = 2;
        public const UInt32 SETRTS = 3;
        public const UInt32 CLRRTS = 4;
        public const UInt32 SETDTR = 5;
        public const UInt32 CLRDTR = 6;
        public const UInt32 RESETDEV = 7;
        public const UInt32 SETBREAK = 8;
        public const UInt32 CLRBREAK = 9;

        [DllImport("kernel32.dll")]
        public static extern Boolean GetCommModemStatus(IntPtr hFile, out UInt32 lpModemStat);

        // Constants for lpModemStat:
        public const UInt32 MS_CTS_ON = 0x0010;
        public const UInt32 MS_DSR_ON = 0x0020;
        public const UInt32 MS_RING_ON = 0x0040;
        public const UInt32 MS_RLSD_ON = 0x0080;

        /// <summary>
        /// Status Functions.
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern Boolean GetOverlappedResult(IntPtr hFile, IntPtr lpOverlapped,
            out UInt32 nNumberOfBytesTransferred, Boolean bWait);

        [DllImport("kernel32.dll")]
        public static extern Boolean ClearCommError(IntPtr hFile, out UInt32 lpErrors, IntPtr lpStat);
        [DllImport("kernel32.dll")]
        public static extern Boolean ClearCommError(IntPtr hFile, out UInt32 lpErrors, out COMSTAT cs);

        //Constants for lpErrors:
        public const UInt32 CE_RXOVER = 0x0001;
        public const UInt32 CE_OVERRUN = 0x0002;
        public const UInt32 CE_RXPARITY = 0x0004;
        public const UInt32 CE_FRAME = 0x0008;
        public const UInt32 CE_BREAK = 0x0010;
        public const UInt32 CE_TXFULL = 0x0100;
        public const UInt32 CE_PTO = 0x0200;
        public const UInt32 CE_IOE = 0x0400;
        public const UInt32 CE_DNS = 0x0800;
        public const UInt32 CE_OOP = 0x1000;
        public const UInt32 CE_MODE = 0x8000;
        [DllImport("kernel32.dll")]
        public static extern Boolean GetCommProperties(IntPtr hFile, out COMMPROP cp);
    }
}