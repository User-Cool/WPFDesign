using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WindowCustomizeTest.Win32
{
    internal class ApiGdi32
    {
        private const string Gdi32 = "gdi32.dll";


        #region API
        #region CreateCompatibleDC
        [DllImport(Gdi32)]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        #endregion

        #region CreateDIBSection
        /// <summary>
        /// 创建可以直接写入内容的兼容 BitMap
        /// </summary>
        /// <param name="hdc">BitMap的设备</param>
        /// <param name="pbim"><see cref="BITMAPINFO"/></param>
        /// <param name="usage"><see cref="DIBColors.DIB_PAL_COLORS"/>或者<see cref="DIBColors.DIB_RGB_COLORS"/></param>
        /// <param name="ppvBits">位图的数据所在位置，不需要人工释放</param>
        /// <param name="hSection">函数将用于创建DIB的文件映射对象的句柄。 此参数可以为NULL。</param>
        /// <param name="offset">从hSection引用的文件映射对象的开头开始的偏移量，其中开始存储位图位值。 如果hSection为NULL，则忽略此值。 位图位的值在双字边界上对齐，因此dwOffset必须是DWORD大小的倍数。</param>
        /// <returns></returns>
        [DllImport(Gdi32)]
        public static extern IntPtr CreateDIBSection(
            IntPtr hdc,
            ref BITMAPINFO pbim,
            uint usage,
            out IntPtr ppvBits,
            IntPtr hSection,
            uint offset
            );
        #endregion

        #region SelectObject
        [DllImport(Gdi32)]
        public static extern IntPtr SelectObject(
            IntPtr hdc,
            IntPtr h);
        #endregion

        #region DeleteDC
        [DllImport(Gdi32)]
        [return:MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteDC(IntPtr hdc);
        #endregion

        #region DeleteObject
        [DllImport(Gdi32)]
        [return:MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject(IntPtr ho);
        #endregion

        #region GetDeviceCaps 
        /// <summary>
        /// GetDeviceCaps函数检索指定设备的设备特定信息。
        /// https://docs.microsoft.com/zh-cn/windows/win32/api/wingdi/nf-wingdi-getdevicecaps?f1url=https%3A%2F%2Fmsdn.microsoft.com%2Fquery%2Fdev16.query%3FappId%3DDev16IDEF1%26l%3DZH-CN%26k%3Dk(WINGDI%2FGetDeviceCaps)%3Bk(GetDeviceCaps)%3Bk(DevLang-C%2B%2B)%3Bk(TargetOS-Windows)%26rd%3Dtrue
        /// </summary>
        /// <param name="hdc"></param>
        /// <param name="index">预定义值</param>
        /// <returns></returns>
        [DllImport(Gdi32)]
        public static extern int GetDeviceCaps(IntPtr hdc, int index);
        #endregion
        #endregion
    }
}
