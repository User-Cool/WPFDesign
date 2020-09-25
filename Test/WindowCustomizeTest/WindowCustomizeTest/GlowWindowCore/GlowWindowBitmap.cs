using System;
using System.Runtime.InteropServices;
using WindowCustomizeTest.Interop;
using WindowCustomizeTest.Win32;

namespace WindowCustomizeTest.GlowWindowCore
{
    internal class GlowWindowBitmap : DisposableObject
    {
        private readonly BITMAPINFO _bitmapInfo; // 边框位图结构体
        private readonly IntPtr _pbits;          // 边框位图数据


        public GlowWindowBitmap(IntPtr screenDC, int width, int height)
        {
            _bitmapInfo.bmiHeader.biSize = Marshal.SizeOf(typeof(BITMAPINFOHEADER));
            _bitmapInfo.bmiHeader.biWidth = width;
            _bitmapInfo.bmiHeader.biHeight = -height;
            _bitmapInfo.bmiHeader.biPlanes = 1; // 必须是1
            _bitmapInfo.bmiHeader.biBitCount = 32;
            _bitmapInfo.bmiHeader.biCompression = BIC.BI_RGB;
            _bitmapInfo.bmiHeader.biSizeImage = 0; // 未压缩的 RGB
            _bitmapInfo.bmiHeader.biXPelsPerMeter = 0;
            _bitmapInfo.bmiHeader.biYPelsPerMeter = 0;
            _bitmapInfo.bmiHeader.biClrUsed = 0; /// <see cref="BITMAPINFO.bmiColors"/> 大小为0
            _bitmapInfo.bmiHeader.biClrImportant = 0;

            Handle = ApiGdi32.CreateDIBSection(
                screenDC,
                ref _bitmapInfo,
                0,
                out _pbits,
                IntPtr.Zero,
                0
                );
        }


        public IntPtr Handle { get; }
        public IntPtr DIBits => _pbits;
        public int Width => _bitmapInfo.bmiHeader.biWidth;
        public int Height => -_bitmapInfo.bmiHeader.biHeight;


        #region 重写 DisposableObject 的方法
        /// <summary>
        /// 释放 HBITMAP。CreateXXX 创建的内容要是用 DeleteXXX 进行释放。
        /// </summary>
        protected override void DisposeNativeResources()
        {
            ApiGdi32.DeleteObject(Handle);
        }
        #endregion
    }
}
