using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WindowCustomizeTest.Win32
{
    internal class ApiMsimg32
    {
        private const string Msimg32 = "msimg32.dll";


        #region AlphaBlend
        [DllImport(Msimg32)]
        [return:MarshalAs(UnmanagedType.Bool)]
        public static extern bool AlphaBlend(
            IntPtr hdcDest,
            int xoriginDest,
            int yoriginDest,
            int wDest,
            int hDest,
            IntPtr hdcSrc,
            int xoriginSrc,
            int yoriginSrc,
            int wSrc,
            int hSrc,
            BLENDFUNCTION ftn);
        #endregion
    }
}
