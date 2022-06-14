using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;

namespace OutlookHTMLTest
{
    public class WinAPI
    {
        public const int GWL_EXSTYLE = -20;
        public const int LWA_ALPHA = 0x2;
        public const int LWA_COLORKEY = 0x1;
        public const int WH_GETMESSAGE = 3;
        public const int WS_EX_LAYERED = 0x80000;
        public const int ULW_COLORKEY = 0x00000001;
        public const int ULW_ALPHA = 0x00000002;
        public const int ULW_OPAQUE = 0x00000004;
        public const byte AC_SRC_OVER = 0x00;
        public const byte AC_SRC_ALPHA = 0x01;

        public delegate bool EnumChildWindowsCallback(IntPtr hWnd, IntPtr lParam);
        public delegate int HookProc(int code, IntPtr wParam, IntPtr lParam);


        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        public static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,     // x-coordinate of upper-left corner
            int nTopRect,      // y-coordinate of upper-left corner
            int nRightRect,    // x-coordinate of lower-right corner
            int nBottomRect,   // y-coordinate of lower-right corner
            int nWidthEllipse, // width of ellipse
            int nHeightEllipse // height of ellipse
        );




        [DllImport("gdi32.dll")] public static extern bool DeleteDC(IntPtr hdc);
        [DllImport("gdi32.dll")] public static extern bool DeleteObject(IntPtr hObject);
        [DllImport("gdi32.dll")] public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
        [DllImport("gdi32.dll")] public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        [DllImport("kernel32.dll")] public static extern int GetProcessId(IntPtr Process);
        [DllImport("kernel32.dll")] public static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("user32.dll")] public static extern bool EnumChildWindows(IntPtr hwndParent, EnumChildWindowsCallback callback, IntPtr lParam);
        [DllImport("user32.dll")] public static extern bool GetCaretPos(out Point lpPoint);
        [DllImport("user32.dll")] public static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
        [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);
        [DllImport("user32.dll")] public static extern bool IsWindow(IntPtr hWnd);
        [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll")] public static extern bool SetCaretPos(int x, int y);
        [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
        [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")] public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);
        [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int X, int Y, int cx, int cy, int flags);
        [DllImport("user32.dll")] public static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref Point pptDst, ref Size psize, IntPtr hdcSrc, ref Point pprSrc, Int32 crKey, ref BLENDFUNCTION pblend, Int32 dwFlags);
        [DllImport("user32.dll")] public static extern int CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")] public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
        [DllImport("User32.dll")] public static extern int GetDpiForWindow(IntPtr hwnd);
        [DllImport("user32.dll")] public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);
        [DllImport("user32.dll")] public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("user32.dll")] public static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
        [DllImport("user32.dll")] public static extern int UnhookWindowsHookEx(IntPtr idHook);
        [DllImport("user32.dll")] public static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")] public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")] public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);
        [DllImport("user32.dll")] public static extern int SendMessage(int hWnd, uint Msg, int wParam, int lParam);
        [DllImport("user32.dll")] public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("user32.dll")] public static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        [DllImport("user32.dll")] public static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);
        [DllImport("gdi32.dll")] public static extern IntPtr CreateBitmap(int nWidth, int nHeight, uint nPlanes, uint nBitCount, IntPtr lpBits);

        public static int GetWindowLng(IntPtr hWnd, int nIndex)
        {
            return Environment.Is64BitOperatingSystem
                ? GetWindowLongPtr(hWnd, nIndex).ToInt32()
                : GetWindowLong(hWnd, nIndex).ToInt32();
        }

        public static string GetWindowTextF(IntPtr wnd)
        {
            var sb = new StringBuilder(1024);
            var ln = GetWindowText(wnd, sb, 1024);
            if (ln > 0)
            {
                return sb.ToString().Substring(0, ln);
            }
            return "";
        }

        public static uint IDLEi => GetIdleTime();
        public static TimeSpan IDLEt => new TimeSpan(0, 0, 0, 0, (int)GetIdleTime());

        public static uint GetIdleTime()
        {
            var s = new LASTINPUTINFO();
            s.cbSize = (uint)Marshal.SizeOf(s);
            if (GetLastInputInfo(ref s))
            {
                return (uint)Environment.TickCount - (uint)s.dwTime;
            }
            return 0;
        }

        public static IntPtr FindChildWindowByClassName(IntPtr hwndParent, string className, bool findVisibleOnly = true)
        {
            var res = IntPtr.Zero;
            var cls = new StringBuilder(className.Length + 5);

            EnumChildWindows(hwndParent, delegate (IntPtr hwndChild, IntPtr lParam)
            {
                GetClassName(hwndChild, cls, cls.Capacity);
                var flag = IsWindowVisible(hwndChild);
                if ((!findVisibleOnly || flag) && cls.ToString() == className)
                {
                    res = hwndChild;
                    return false;
                }
                return true;
            }, IntPtr.Zero);
            return res;
        }

        public static IntPtr[] FindAllChildWindowByClassName(IntPtr hwndParent, string className, bool findVisibleOnly = true) => FindAllChildsWindowByClassName(hwndParent, className, findVisibleOnly);


        public static IntPtr[] FindAllChildsWindowByClassName(IntPtr hwndParent, string className, bool findVisibleOnly = false)
        {
            var res = new List<IntPtr>();
            var cls = new StringBuilder(className.Length + 5);

            WinAPI.EnumChildWindows(hwndParent, delegate (IntPtr hwndChild, IntPtr lParam)
            {
                WinAPI.GetClassName(hwndChild, cls, cls.Capacity);
                var flag = WinAPI.IsWindowVisible(hwndChild);
                if ((!findVisibleOnly || flag) && cls.ToString() == className)
                {
                    res.Add(hwndChild);
                    return true;
                }
                return true;
            }, IntPtr.Zero);
            return res.ToArray();
        }

        public static void ClickOnPoint(IntPtr wndHandle, Point clientPoint)
        {
            var oldPos = Cursor.Position;

            /// set cursor on coords, and press mouse
            Cursor.Position = new Point(clientPoint.X, clientPoint.Y);

            var inputMouseDown = new INPUT();
            inputMouseDown.Type = 0; /// input type mouse
            inputMouseDown.Data.Mouse.Flags = 0x0002; /// left button down

            var inputMouseUp = new INPUT();
            inputMouseUp.Type = 0; /// input type mouse
            inputMouseUp.Data.Mouse.Flags = 0x0004; /// left button up

            var inputs = new INPUT[] { inputMouseDown, inputMouseUp };
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));

            /// return mouse 
            Cursor.Position = oldPos;
        }

        public struct INPUT
        {
            public UInt32 Type;
            public MOUSEKEYBDHARDWAREINPUT Data;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct MOUSEKEYBDHARDWAREINPUT
        {
            [FieldOffset(0)]
            public MOUSEINPUT Mouse;
        }
        public struct MOUSEINPUT
        {
            public Int32 X;
            public Int32 Y;
            public UInt32 MouseData;
            public UInt32 Flags;
            public UInt32 Time;
            public IntPtr ExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BLENDFUNCTION
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LASTINPUTINFO
        {
            public static readonly int SizeOf = Marshal.SizeOf(typeof(LASTINPUTINFO));

            [MarshalAs(UnmanagedType.U4)] public uint cbSize;
            [MarshalAs(UnmanagedType.U4)] public uint dwTime;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CIEXYZ
        {
            [MarshalAs(UnmanagedType.U4)] public int ciexyzX;
            [MarshalAs(UnmanagedType.U4)] public int ciexyzY;
            [MarshalAs(UnmanagedType.U4)] public int ciexyzZ;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CIEXYZTRIPLE
        {
            [MarshalAs(UnmanagedType.Struct)] public CIEXYZ ciexyzRed;
            [MarshalAs(UnmanagedType.Struct)] public CIEXYZ ciexyzGreen;
            [MarshalAs(UnmanagedType.Struct)] public CIEXYZ ciexyzBlue;
        }

            [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFOHEADERV5
        {
            [MarshalAs(UnmanagedType.U4)] public uint biSize;
            [MarshalAs(UnmanagedType.U4)] public int biWidth;
            [MarshalAs(UnmanagedType.U4)] public int biHeight;
            [MarshalAs(UnmanagedType.U2)] public UInt16 biPlanes;
            [MarshalAs(UnmanagedType.U2)] public UInt16 biBitCount;
            [MarshalAs(UnmanagedType.U4)] public uint biCompression;
            [MarshalAs(UnmanagedType.U4)] public uint biSizeImage;
            [MarshalAs(UnmanagedType.U4)] public int biXPelsPerMeter;
            [MarshalAs(UnmanagedType.U4)] public int biYPelsPerMeter;
            [MarshalAs(UnmanagedType.U4)] public uint biClrUsed;
            [MarshalAs(UnmanagedType.U4)] public uint biClrImportant;
            [MarshalAs(UnmanagedType.U4)] public uint bV5RedMask;
            [MarshalAs(UnmanagedType.U4)] public uint bV5GreenMask;
            [MarshalAs(UnmanagedType.U4)] public uint bV5BlueMask;
            [MarshalAs(UnmanagedType.U4)] public uint bV5AlphaMask;
            [MarshalAs(UnmanagedType.U4)] public uint bV5CSType;
            [MarshalAs(UnmanagedType.Struct)] public CIEXYZTRIPLE bV5Endpoints;
            [MarshalAs(UnmanagedType.U4)] public uint bV5GammaRed;
            [MarshalAs(UnmanagedType.U4)] public uint bV5GammaGreen;
            [MarshalAs(UnmanagedType.U4)] public uint bV5GammaBlue;
            [MarshalAs(UnmanagedType.U4)] public uint bV5Intent;
            [MarshalAs(UnmanagedType.U4)] public uint bV5ProfileData;
            [MarshalAs(UnmanagedType.U4)] public uint bV5ProfileSize;
            [MarshalAs(UnmanagedType.U4)] public uint bV5Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RGBQUAD
        {
            [MarshalAs(UnmanagedType.U1)] public byte rgbBlue;
            [MarshalAs(UnmanagedType.U1)] public byte rgbGreen;
            [MarshalAs(UnmanagedType.U1)] public byte rgbRed;
            [MarshalAs(UnmanagedType.U1)] public byte rgbReserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFO
        {
            [MarshalAs(UnmanagedType.Struct)] public BITMAPINFOHEADERV5 bmiHeader;
            [MarshalAs(UnmanagedType.Struct)] public RGBQUAD bmiColors;
        }

        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        public static class SWP
        {
            public static readonly int
            NOSIZE = 0x0001,
            NOMOVE = 0x0002,
            NOZORDER = 0x0004,
            NOREDRAW = 0x0008,
            NOACTIVATE = 0x0010,
            DRAWFRAME = 0x0020,
            FRAMECHANGED = 0x0020,
            SHOWWINDOW = 0x0040,
            HIDEWINDOW = 0x0080,
            NOCOPYBITS = 0x0100,
            NOOWNERZORDER = 0x0200,
            NOREPOSITION = 0x0200,
            NOSENDCHANGING = 0x0400,
            DEFERERASE = 0x2000,
            ASYNCWINDOWPOS = 0x4000;
        }
    }
}
