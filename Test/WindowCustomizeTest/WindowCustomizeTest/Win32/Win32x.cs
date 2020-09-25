namespace WindowCustomizeTest.Win32
{
    internal class Win32x
    {
        public static int LOWORD(int value) => (short)(value & 0xFFFF);
        public static int HIWORD(int value) => (short)((value >> 16) & 0xFFFF);

        public static int GET_X_LPARAM(int lParam) => LOWORD(lParam);
        public static int GET_Y_LPARAM(int lParam) => HIWORD(lParam);
    }
}
