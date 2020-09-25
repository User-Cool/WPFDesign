using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using WindowCustomizeTest.Win32;
using HitTestResult = WindowCustomizeTest.Win32.HitTestResult;

/*
 * 自定义窗口的不错的文章 https://www.cnblogs.com/fabler/archive/2014/08/17/3918088.html Winform的窗体美化心酸路
 */

/**
 * 问：边框作为另一个窗口，它是怎么控制改变主窗口的大小的？
 * 答：在 WM_NCHITTEST 下进行命中测试，返回合适的测试结果，然后在 WM_NCLBUTTONDOWN 等消息下向目标窗口发送
 *     WM_NCHITTEST 消息，并且附带在 WM_NCHITTEST 的测试结果，最后将 WM_NCLBUTTONDOWN 消息发送给主窗口，
 *     这些欺骗了主窗口的处理函数，从而达到改变窗口的目的。
 *     同时，主窗口的变化会触发 WM_WINDOWPOSCHANGING 和 WM_WINDOWPOSCHANGED 消息，从而实现反回来移动分层
 *     窗口的目的。关于分层窗口的跟随，请看 <see cref="WindowCustomizeTest.GlowWindow"/>.
 * */
namespace WindowCustomizeTest.GlowWindowCore
{
    /// <summary>
    /// 作为边框的分层窗口。样式来自 Visual Studio。
    /// </summary>
    /// <remarks>
    /// 绘制使用的是 UpdateLayeredWindow。窗口样式一定要设置 WS_EX_LAYERED
    /// </remarks>
    internal class GlowEdge : HwndWrapper
    {
        #region 属性
        private GlowWindow _targetWindow;
        private IntPtr _targetWindowHandle;
        private static ushort _classAtom;
        private Dock _orientation; // 是左边框Left、还是右边框（right、上边框Top、下边框bottom
        private const int _glowEdgeBitmapMaxCount = 16;
        private static int _glowDepth = 9;
        private GlowEdgeBitmap[] _activeBitmaps = new GlowEdgeBitmap[_glowEdgeBitmapMaxCount];
        private GlowEdgeBitmap[] _inactiveBitmaps = new GlowEdgeBitmap[_glowEdgeBitmapMaxCount];
        private GlowEdgeDrawingContext _drawingContext = new GlowEdgeDrawingContext();
        
        private bool _isActive = true;
        private Color _activeColor;
        private Color _inactiveColor;
        #endregion


        internal GlowEdge(GlowWindow targetWindow, Dock orientation)
        {
            _targetWindow = targetWindow ?? throw new ArgumentNullException(nameof(targetWindow));
            _orientation = orientation;
            _targetWindowHandle = new WindowInteropHelper(_targetWindow).Handle;
        }


        #region 公开的方法
        /// <summary>
        /// 只有在 Active 发生变化的时候，Z 顺序才会发生变化。
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set 
            { 
                _isActive = value; 
                RenderZOrder(); 
                RenderLayeredWindow(); 
            }
        }

        public Color ActiveColor
        {
            get => _activeColor;
            set { _activeColor = value; }
        }

        public Color InactiveColor
        {
            get => _inactiveColor;
            set { _inactiveColor = value; }
        }

        public void UpdatePosition()
        {
            RenderPosition();
            RenderLayeredWindow();
        }

        public void UpdateState(ShowWindowFlags showWindowFlags)
        {
            ApiUser32.ShowWindow(Handle, showWindowFlags);
        }

        public void Visible(bool visible)
        {
            if (visible)
            {
                ApiUser32.SetWindowPos(
                Handle,
                _targetWindowHandle,
                0, 0, 0, 0,
                SWPFlags.SWP_NOSIZE | SWPFlags.SWP_NOMOVE | SWPFlags.SWP_NOACTIVATE | SWPFlags.SWP_SHOWWINDOW);
            }
            else
            {
                ApiUser32.SetWindowPos(
                Handle,
                _targetWindowHandle,
                0, 0, 0, 0,
                SWPFlags.SWP_NOSIZE | SWPFlags.SWP_NOMOVE | SWPFlags.SWP_NOACTIVATE | SWPFlags.SWP_HIDEWINDOW);
            }
        }
        #endregion


        #region 窗口创建
        #region 属性
        private const string GlowEdgeClassName = "GlowWindowEdge";
        private const uint GlowEdgeClassStyle = 0u;
        private const uint GlowEdgeWindowStyle = (uint)Win32.WindowStyle.WS_WDGLOWWINDOW;
        private const uint GlowEdgeWindowExStyle = (uint)WindowExStyle.WS_EX_WDGLOWWINDOW;
        #endregion

        /// <summary>
        /// 是 HwndSource 的子类。
        /// </summary>
        protected override bool IsWindowSubclassed => true;

        #region 构建窗口类
        private ushort RegisterClass()
        {
            if (_classAtom == 0)
            {
                var wndClassEx = default(WNDCLASSEX);
                wndClassEx.cbSize = (uint)Marshal.SizeOf(typeof(WNDCLASSEX));
                wndClassEx.style = GlowEdgeClassStyle; // 0u
                wndClassEx.cbClsExtra = 0; // 默认0就行
                wndClassEx.cbWndExtra = 0; // 同上
                wndClassEx.hInstance = ApiKernel32.GetModuleHandle(null); // 程序的句柄
                wndClassEx.hIcon = IntPtr.Zero;  // 默认 0 就行 
                wndClassEx.hCursor = IntPtr.Zero; // 默认 0 就行
                wndClassEx.hbrBackground = IntPtr.Zero; // 默认 0 就行
                wndClassEx.lpfnWndProc = Marshal.GetFunctionPointerForDelegate(ApiUser32.DefWndProc);
                wndClassEx.lpszClassName = GlowEdgeClassName;
                wndClassEx.lpszMenuName = null;

                _classAtom = ApiUser32.RegisterClassEx(ref wndClassEx);
            }
            
            return _classAtom;
        }
        #endregion

        #region 注册窗口类
        protected override ushort CreateWindowClassCore() => RegisterClass();
        #endregion

        #region 创建窗口
        protected override IntPtr CreateWindowCore()
        {
            return ApiUser32.CreateWindowEx(
                GlowEdgeWindowExStyle,
                new IntPtr(WindowClassAtom),
                string.Empty,
                GlowEdgeWindowStyle,
                0,
                0,
                0,
                0,
                /// 将分层窗口设置为目标窗口的父窗口的子对象。这在解释上是合理的（分层窗口和目标窗口是同级的）。并且，这样可以解决当目标窗口是其他窗口的子窗口的时候，分层窗口在获得焦点之后，在目标窗口的父窗口之下的问题。
                new WindowInteropHelper(_targetWindow).Owner,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero
                );
        }
        #endregion

        #region 销毁窗口类
        protected override void DestroyWindowClassCore() { }
        #endregion

        #region 窗口过程
        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg <= (int)WindowsMessage.WM_WINDOWPOSCHANGING)
            {
                
            }
            else
            {
                if (msg != (int)WindowsMessage.WM_DISPLAYCHANGE)
                {
                    if (msg == (int)WindowsMessage.WM_NCHITTEST)
                        return new IntPtr(HistTest(lParam));
                    switch (msg)
                    {
                        case (int)WindowsMessage.WM_NCLBUTTONDOWN:
                        case (int)WindowsMessage.WM_NCLBUTTONDBLCLK:
                        case (int)WindowsMessage.WM_NCRBUTTONDOWN:
                        case (int)WindowsMessage.WM_NCRBUTTONDBLCLK:
                        case (int)WindowsMessage.WM_NCMBUTTONDOWN:
                        case (int)WindowsMessage.WM_NCMBUTTONDBLCLK:
                        case (int)WindowsMessage.WM_NCXBUTTONDOWN:
                        case (int)WindowsMessage.WM_NCXBUTTONDBLCLK:
                            {
                                var targetWindowHandle = _targetWindowHandle;
                                ApiUser32.SendMessage(
                                    targetWindowHandle,
                                    (int)WindowsMessage.WM_ACTIVATE,
                                    new IntPtr((int)ActiveMsgValue.WA_CLICKACTIVE),
                                    IntPtr.Zero);
                                ApiUser32.SendMessage(
                                    targetWindowHandle, 
                                    msg, 
                                    wParam, 
                                    IntPtr.Zero);
                                return IntPtr.Zero;
                            }
                    }
                }
                else
                {

                }
            }
            return base.WndProc(hwnd, msg, wParam, lParam);
        }
        #endregion
        #endregion


        #region 重写的方法
        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
        }

        protected override void DisposeNativeResources()
        {
            base.DisposeNativeResources();
        }
        #endregion


        #region 私有的方法
        #region 创建 GlowEdgeBitmap
        private GlowEdgeBitmap GetOrCreateBitmap(GlowEdgeDrawingContext dc, GlowEdgeBitmapParts part)
        {
            GlowEdgeBitmap[] glowEdgeBitmaps;
            Color color;

            if (IsActive)
            {
                glowEdgeBitmaps = _activeBitmaps;
                color = ActiveColor;
            }
            else
            {
                glowEdgeBitmaps = _inactiveBitmaps;
                color = InactiveColor;
            }

            return glowEdgeBitmaps[(int)part] ?? 
                (glowEdgeBitmaps[(int)part] = GlowEdgeBitmap.Create(dc, part, color));
        }
        #endregion

        /// <summary>
        /// UpdateLayeredWindow，更新窗口
        /// </summary>
        private void RenderLayeredWindow()
        {
            // var drawingContext = new GlowEdgeDrawingContext(Width, Height);
            _drawingContext.Resize(Width, Height);
            var drawingContext = _drawingContext;

            POINT point = new POINT { x = Left, y = Top };
            SIZE size = new SIZE { cx = Width, cy = Height };
            POINT pointSrc = new POINT { x = 0, y = 0 };
            BLENDFUNCTION blend = drawingContext.Blend;

            switch (_orientation)
            {
                case Dock.Left:
                    DrawLeft(drawingContext);
                    break;
                case Dock.Top:
                    DrawTop(drawingContext);
                    break;
                case Dock.Right:
                    DrawRight(drawingContext);
                    break;
                case Dock.Bottom:
                    DrawBottom(drawingContext);
                    break;
            }

            ApiUser32.UpdateLayeredWindow(
                Handle,
                drawingContext.ScreenDC,
                ref point,
                ref size,
                drawingContext.WindowDC,
                ref pointSrc,
                0,
                ref blend,
                (uint)ULWFlags.ULW_ALPHA);

            //drawingContext.Dispose();
        }

        /// <summary>
        /// 计算窗口的大小和位置
        /// </summary>
        private void RenderPosition()
        {
            var handle = _targetWindowHandle;
            ApiUser32.GetWindowRect(handle, out var rect);

            switch (_orientation)
            {
                case Dock.Left:
                    Left = rect.Left - _glowDepth;
                    Top = rect.Top - _glowDepth;
                    Width = _glowDepth;
                    Height = rect.Height + _glowDepth + _glowDepth;
                    break;
                case Dock.Top:
                    Left = rect.Left;
                    Top = rect.Top - _glowDepth;
                    Width = rect.Width;
                    Height = _glowDepth;
                    break;
                case Dock.Right:
                    Left = rect.Right;
                    Top = rect.Top - _glowDepth;
                    Width = _glowDepth;
                    Height = rect.Height + _glowDepth + _glowDepth;
                    break;
                case Dock.Bottom:
                    Left = rect.Left;
                    Top = rect.Bottom;
                    Width = rect.Width;
                    Height = _glowDepth;
                    break;
            }

            //ApiUser32.SetWindowPos(Handle, _targetWindowHandle, 0, 0, 0, 0, 19);
        }

        /// <summary>
        /// Z 顺序
        /// </summary>
        private void RenderZOrder()
        {
            ApiUser32.SetWindowPos(
                Handle,
                _targetWindowHandle,
                0, 0, 0, 0,
                SWPFlags.SWP_NOSIZE | SWPFlags.SWP_NOMOVE | SWPFlags.SWP_NOACTIVATE);
        }

        #region 命中测试
        /// <summary>
        /// 进行命中测试
        /// </summary>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private int HistTest(IntPtr lParam)
        {
            int x = Win32x.GET_X_LPARAM(lParam.ToInt32());
            int y = Win32x.GET_Y_LPARAM(lParam.ToInt32());
            ApiUser32.GetWindowRect(Handle, out var rect);
            switch (_orientation)
            {
                case Dock.Left:
                    if (y < rect.Top + _glowDepth) return (int)HitTestResult.HTTOPLEFT;
                    if (y > rect.Bottom - _glowDepth) return (int)HitTestResult.HTBOTTOMLEFT;
                    return (int)HitTestResult.HTLEFT;
                case Dock.Top:
                    if (x  < rect.Left + _glowDepth) return 13;
                    if (x > rect.Right - _glowDepth) return 14;
                    return 12;
                case Dock.Right:
                    if (y < rect.Top + _glowDepth) return 14;
                    if (y > rect.Bottom - _glowDepth) return 17;
                    return 11;
                default:
                    if (x < rect.Left + _glowDepth) return 16;
                    if (x > rect.Right - _glowDepth) return 17;
                    return 15;
            }
        }
        #endregion

        #region 位置
        private int Left { get; set; }
        private int Top { get; set; }
        private int Width { get; set; }
        private int Height { get; set; }
        #endregion

        #region 绘制
        private void DrawLeft(GlowEdgeDrawingContext drawingContext)
        {
            // 创建或者拿到边框各个位置的源位图
            GlowEdgeBitmap CTL = GetOrCreateBitmap(drawingContext, GlowEdgeBitmapParts.CornerTopLeft);
            GlowEdgeBitmap LT = GetOrCreateBitmap(drawingContext, GlowEdgeBitmapParts.LeftTop);
            GlowEdgeBitmap L = GetOrCreateBitmap(drawingContext, GlowEdgeBitmapParts.Left);
            GlowEdgeBitmap LB = GetOrCreateBitmap(drawingContext, GlowEdgeBitmapParts.LeftBottom);
            GlowEdgeBitmap CBL = GetOrCreateBitmap(drawingContext, GlowEdgeBitmapParts.CornerBottomLeft);

            int LT_y = CTL.Height;
            int L_y = LT_y + LT.Height;
            int CBL_y = drawingContext.Height - CBL.Height;
            int LB_y = CBL_y - LB.Height;
            int L_h = LB_y - L_y;

            ApiGdi32.SelectObject(drawingContext.BackgroundDC, CTL.Handle); // 
            ApiMsimg32.AlphaBlend(
                drawingContext.WindowDC, 0, 0, CTL.Width, CTL.Height, // 就是00，因为是 BackgroundDC 和 WindowDC 的混合，不是最终的 ScreenDC。
                drawingContext.BackgroundDC, 0, 0, CTL.Width, CTL.Height,
                drawingContext.Blend);

            ApiGdi32.SelectObject(drawingContext.BackgroundDC, LT.Handle);
            ApiMsimg32.AlphaBlend(
                drawingContext.WindowDC, 0, LT_y, LT.Width, LT.Height,
                drawingContext.BackgroundDC, 0, 0, LT.Width, LT.Height,
                drawingContext.Blend);

            if (L_h > 0)
            {
                ApiGdi32.SelectObject(drawingContext.BackgroundDC, L.Handle);
                ApiMsimg32.AlphaBlend(
                    drawingContext.WindowDC, 0, L_y, L.Width, L_h,
                    drawingContext.BackgroundDC, 0, 0, L.Width, L.Height,
                    drawingContext.Blend);
            }

            ApiGdi32.SelectObject(drawingContext.BackgroundDC, LB.Handle);
            ApiMsimg32.AlphaBlend(
                drawingContext.WindowDC, 0, LB_y, LB.Width, LB.Height,
                drawingContext.BackgroundDC, 0, 0, LB.Width, LB.Height,
                drawingContext.Blend);

            ApiGdi32.SelectObject(drawingContext.BackgroundDC, CBL.Handle);
            ApiMsimg32.AlphaBlend(
                drawingContext.WindowDC, 0, CBL_y, CBL.Width, CBL.Height,
                drawingContext.BackgroundDC, 0, 0, CBL.Width, CBL.Height,
                drawingContext.Blend);
        }

        private void DrawRight(GlowEdgeDrawingContext drawingContext)
        {
            // 创建或者拿到边框各个位置的源位图
            GlowEdgeBitmap CTR = GetOrCreateBitmap(drawingContext, GlowEdgeBitmapParts.CornerTopRight);
            GlowEdgeBitmap RT = GetOrCreateBitmap(drawingContext, GlowEdgeBitmapParts.RightTop);
            GlowEdgeBitmap R = GetOrCreateBitmap(drawingContext, GlowEdgeBitmapParts.Right);
            GlowEdgeBitmap RB = GetOrCreateBitmap(drawingContext, GlowEdgeBitmapParts.RightBottom);
            GlowEdgeBitmap CBR = GetOrCreateBitmap(drawingContext, GlowEdgeBitmapParts.CornerBottomRight);

            int RT_y = CTR.Height;
            int R_y = RT_y + RT.Height;
            int CBR_y = drawingContext.Height - CBR.Height;
            int RB_y = CBR_y - RB.Height;
            int R_h = RB_y - R_y;

            ApiGdi32.SelectObject(drawingContext.BackgroundDC, CTR.Handle);
            ApiMsimg32.AlphaBlend(
                drawingContext.WindowDC, 0, 0, CTR.Width, CTR.Height,
                drawingContext.BackgroundDC, 0, 0, CTR.Width, CTR.Height,
                drawingContext.Blend);

            ApiGdi32.SelectObject(drawingContext.BackgroundDC, RT.Handle);
            ApiMsimg32.AlphaBlend(
                drawingContext.WindowDC, 0, RT_y, RT.Width, RT.Height,
                drawingContext.BackgroundDC, 0, 0, RT.Width, RT.Height,
                drawingContext.Blend);

            if (R_h > 0)
            {
                ApiGdi32.SelectObject(drawingContext.BackgroundDC, R.Handle);
                ApiMsimg32.AlphaBlend(
                    drawingContext.WindowDC, 0, R_y, R.Width, R_h,
                    drawingContext.BackgroundDC, 0, 0, R.Width, R.Height,
                    drawingContext.Blend);
            }

            ApiGdi32.SelectObject(drawingContext.BackgroundDC, RB.Handle);
            ApiMsimg32.AlphaBlend(
                drawingContext.WindowDC, 0, RB_y, RB.Width, RB.Height,
                drawingContext.BackgroundDC, 0, 0, RB.Width, RB.Height,
                drawingContext.Blend);

            ApiGdi32.SelectObject(drawingContext.BackgroundDC, CBR.Handle);
            ApiMsimg32.AlphaBlend(
                drawingContext.WindowDC, 0, CBR_y, CBR.Width, CBR.Height,
                drawingContext.BackgroundDC, 0, 0, CBR.Width, CBR.Height,
                drawingContext.Blend);
        }

        private void DrawTop(GlowEdgeDrawingContext drawingContext)
        {
            // 创建或者拿到边框各个位置的源位图
            GlowEdgeBitmap TL = GetOrCreateBitmap(drawingContext, GlowEdgeBitmapParts.TopLeft);
            GlowEdgeBitmap T = GetOrCreateBitmap(drawingContext, GlowEdgeBitmapParts.Top);
            GlowEdgeBitmap TR = GetOrCreateBitmap(drawingContext, GlowEdgeBitmapParts.TopRight);

            int T_x = TL.Width;
            int TR_x = drawingContext.Width - TR.Width;
            int T_W = TR_x - T_x;

            ApiGdi32.SelectObject(drawingContext.BackgroundDC, TL.Handle);
            ApiMsimg32.AlphaBlend(
                drawingContext.WindowDC, 0, 0, TL.Width, TL.Height,
                drawingContext.BackgroundDC, 0, 0, TL.Width, TL.Height,
                drawingContext.Blend);

            if (T_W > 0)
            {
                ApiGdi32.SelectObject(drawingContext.BackgroundDC, T.Handle);
                ApiMsimg32.AlphaBlend(
                    drawingContext.WindowDC, T_x, 0, T_W, T.Height,
                    drawingContext.BackgroundDC, 0, 0, T.Width, T.Height,
                    drawingContext.Blend);
            }

            ApiGdi32.SelectObject(drawingContext.BackgroundDC, TR.Handle);
            ApiMsimg32.AlphaBlend(
                drawingContext.WindowDC, TR_x, 0, TR.Width, TR.Height,
                drawingContext.BackgroundDC, 0, 0, TR.Width, TR.Height,
                drawingContext.Blend);
        }

        private void DrawBottom(GlowEdgeDrawingContext drawingContext)
        {
            // 创建或者拿到边框各个位置的源位图
            GlowEdgeBitmap BL = GetOrCreateBitmap(drawingContext, GlowEdgeBitmapParts.BottomLeft);
            GlowEdgeBitmap B = GetOrCreateBitmap(drawingContext, GlowEdgeBitmapParts.Bottom);
            GlowEdgeBitmap BR = GetOrCreateBitmap(drawingContext, GlowEdgeBitmapParts.BottomRight);

            int B_x = BL.Width;
            int BR_x = drawingContext.Width - BR.Width;
            int B_W = BR_x - B_x;

            ApiGdi32.SelectObject(drawingContext.BackgroundDC, BL.Handle);
            ApiMsimg32.AlphaBlend(
                drawingContext.WindowDC, 0, 0, BL.Width, BL.Height,
                drawingContext.BackgroundDC, 0, 0, BL.Width, BL.Height,
                drawingContext.Blend);

            if (B_W > 0)
            {
                ApiGdi32.SelectObject(drawingContext.BackgroundDC, B.Handle);
                ApiMsimg32.AlphaBlend(
                    drawingContext.WindowDC, B_x, 0, B_W, B.Height,
                    drawingContext.BackgroundDC, 0, 0, B.Width, B.Height,
                    drawingContext.Blend);
            }

            ApiGdi32.SelectObject(drawingContext.BackgroundDC, BR.Handle);
            ApiMsimg32.AlphaBlend(
                drawingContext.WindowDC, BR_x, 0, BR.Width, BR.Height,
                drawingContext.BackgroundDC, 0, 0, BR.Width, BR.Height,
                drawingContext.Blend);
        }
        #endregion
        #endregion
    }
}
