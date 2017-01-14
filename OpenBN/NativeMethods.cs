using System;
using System.Runtime.InteropServices;

namespace OpenBN
{
    public class NativeMethods
    {

        public delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        public static bool initialized;
        public static IntPtr prevWndProc;
        public static WndProc hookProcDelegate;
        public static IntPtr hIMC;

        //various Win32 constants that we need
        public const int GWL_WNDPROC = -4;
        public const int WM_KEYDOWN = 0x100;
        public const int WM_KEYUP = 0x101;
        public const int WM_CHAR = 0x102;
        public const int WM_IME_SETCONTEXT = 0x0281;
        public const int WM_INPUTLANGCHANGE = 0x51;
        public const int WM_GETDLGCODE = 0x87;
        public const int WM_IME_COMPOSITION = 0x10f;
        public const int DLGC_WANTALLKEYS = 4;
        public const int WM_NCRBUTTONDOWN = 0x00A4;
        public const int WM_NCLBUTTONDOWN = 0x00A1;

        //Win32 functions that we're using
        [DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr ImmGetContext(IntPtr hWnd);

        [DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr hIMC);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    }
}