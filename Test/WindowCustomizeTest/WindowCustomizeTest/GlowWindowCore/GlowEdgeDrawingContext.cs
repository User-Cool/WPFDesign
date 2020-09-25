using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowCustomizeTest.Interop;
using WindowCustomizeTest.Win32;

namespace WindowCustomizeTest.GlowWindowCore
{
    /// <summary>
    /// 封装 Win32（GDI32）绘制需要的 DC（设备上下文）
    /// </summary>
    internal class GlowEdgeDrawingContext : DisposableObject
    {
        private GlowWindowBitmap _windowBitmap;
        private  BLENDFUNCTION _blend;
        private  IntPtr _screenDC;
        private  IntPtr _windowDC;
        private  IntPtr _backgroundDC;


        public GlowEdgeDrawingContext(/*int width, int height*/)
        {
            _screenDC = ApiUser32.GetDC(IntPtr.Zero);
            if (ScreenDC == IntPtr.Zero) throw new Exception(nameof(ScreenDC));

            _windowDC = ApiGdi32.CreateCompatibleDC(ScreenDC);
            if (WindowDC == IntPtr.Zero) throw new Exception(nameof(WindowDC));

            _backgroundDC = ApiGdi32.CreateCompatibleDC(ScreenDC);
            if (BackgroundDC == IntPtr.Zero) throw new Exception(nameof(BackgroundDC));

            _blend.BlendOp = (byte)AlphaBlendControl.AC_SRC_OVER;         // 唯一值
            _blend.BlendFlags = 0;                                        // 必须 0
            _blend.SourceConstantAlpha = 0xFF;                            // 不增加额外的 Alpha 值，一切以源为主
            _blend.AlphaFormat = (byte)AlphaBlendControl.AC_SRC_ALPHA;    // 使用源的 Alpha 通道

            //_windowBitmap = new GlowWindowBitmap(ScreenDC, width, height);
            //ApiGdi32.SelectObject(_windowDC, _windowBitmap.Handle);
        }


        public IntPtr ScreenDC => _screenDC;
        public IntPtr WindowDC => _windowDC;
        public IntPtr BackgroundDC => _backgroundDC;
        public BLENDFUNCTION Blend => _blend;
        public int Width => _windowBitmap.Width;
        public int Height => _windowBitmap.Height;

        public void Resize(int width, int height)
        {
            if (_windowBitmap != null) _windowBitmap.Dispose();
            _windowBitmap = new GlowWindowBitmap(ScreenDC, width, height);
            ApiGdi32.SelectObject(_windowDC, _windowBitmap.Handle);
        }


        #region 重写的 DisposableObject 的方法
        protected override void DisposeManagedResources()
        {
            _windowBitmap.Dispose();
        }

        protected override void DisposeNativeResources()
        {
            if (BackgroundDC != IntPtr.Zero) ApiGdi32.DeleteDC(BackgroundDC);
            if (WindowDC != IntPtr.Zero) ApiGdi32.DeleteDC(WindowDC);
            if (ScreenDC != IntPtr.Zero) ApiUser32.ReleaseDC(IntPtr.Zero, ScreenDC);
        }
        #endregion
    }
}
