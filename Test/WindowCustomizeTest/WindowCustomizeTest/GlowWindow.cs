using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shell;
using System.Windows.Threading;
using WindowCustomizeTest.GlowWindowCore;
using WindowCustomizeTest.Win32;

namespace WindowCustomizeTest
{
    #region read me
    /// <summary>
    /// 另一种实现窗口边框自定义的方法。
    /// 
    /// 自定义边框比较简单的一种实现是去掉窗口边框，然后通过控件绘制，但是这种方式只是大体上使人满意。
    /// 还有另一种方式，使用这种方式可以保留系统的特性（如动画）。
    /// 
    /// # 思路：
    /// 利用分层窗口计数，创建窗口的分层窗口，在分层窗口上绘制边框。
    /// 这些窗口必须进行特殊处理，才能达到“看上出好像没有额外的窗口”一样的效果。
    /// 
    /// # 技术点
    /// 1、GetWindowPtr、SetWindowPtr：已创建窗口的数据的改变
    /// 2、GetDC：回去设备上下文
    /// 3、CreateCompatibleDC：双缓冲绘图技术，设备上下文的内存对象（兼容的内存DC）
    /// 4、BITMAPINFO：位图信息
    /// 5、BITMAPINFOHEADER：位图头信息
    /// 6、CreateDIBSection：创建可直接写入数据的位图。
    /// 7、SelecteObject：连接 位图 和 设备上下文
    /// 8、BLENDFUNCTION：目标（DC）和源（DC）的混合控制
    /// 9、AlphaBlend：混合函数
    /// 10、UpdateLayeredWindow：分层窗口刷新。
    /// 11、WM_WINDOWPOSCHANGING、WM_WINDOWPOSCHANGED：在目标窗口的这些消息中更新分层窗口的位置。
    /// 12、WM_NCHITTIST：命中测试，在分层窗口中进行命中测试。
    /// 13、WM_NCLMOUSEDOWN等：在分层窗口中处理该消息，将测试结果和该消息发送给目标窗口，实现大小调整。
    /// 14、双缓冲。
    /// 15、双内存设备。
    /// 
    /// 
    /// # 调用非托管方法
    /// 
    /// 
    /// # 注意
    /// 1、窗口的位置、大小、Z 顺序等内容的处理，在消息 WM_WINDOWPOSCHANGED 中。
    /// 2、WPF 下处理 Windows 消息，重写方法 OnSourceInitialized，在消息中添加窗口过程钩子。
    /// 3、利用 HwndSource 可以为 WPF 创建的窗口设置窗口过程钩子
    /// 4、利用 WindowInteropHelper 可以获得 WPF 创建的窗口的句柄
    /// 5、分层窗口设置为目标窗口的父窗口的子窗口，否则在设置分层窗口的 Z 顺序的时候，分层创建将会在目标窗口的父窗口下面。
    /// 6、在构造函数里面时，窗口还没有创建，无法获得窗口的 Win32 句柄 HWND。
    /// 7、利用 HwndSource 设置钩子截取 Windows 消息的方式无法截获 WM_CREATE 消息（具体哪些没有测试）。
    /// 8、在 OnApplyTemplate 之前，依赖属性的值变化方法就已经触发了。
    /// 9、GC 的内存释放是在UI线程之外的线程中进行的（这点很重要）。
    /// 
    /// 
    /// # 缺陷：
    /// 1、分层窗口不是创建了一个，而是四个；
    /// 2、各个方法的调用时机非常重要；
    /// 3、暂时不支持圆角；
    /// </summary>
    #endregion

    public class GlowWindow : Window
    {
        private readonly GlowEdge[] _glowEdges = new GlowEdge[4];
        private DispatcherTimer _makeGlowVisibleTimer;


        #region 构造函数
        static GlowWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GlowWindow), new FrameworkPropertyMetadata(typeof(GlowWindow)));
        }

        public GlowWindow()
        {
            
        }
        #endregion


        #region 依赖属性
        #region 依赖属性
        public Color ActiveGlowColor
        {
            get => (Color)GetValue(ActiveGlowColorProperty);
            set => SetValue(ActiveGlowColorProperty, value);
        }

        public static readonly DependencyProperty ActiveGlowColorProperty = 
            DependencyProperty.Register( 
                "ActiveGlowColor",
                typeof(Color),
                typeof(GlowWindow),
                new PropertyMetadata(
                    SystemColors.ActiveBorderColor,
                    OnGlowColorChanged));

        public Color InactiveGlowColor
        {
            get => (Color)GetValue(InactiveGlowColorProperty);
            set => SetValue(InactiveGlowColorProperty, value);
        }

        public static readonly DependencyProperty InactiveGlowColorProperty = 
            DependencyProperty.Register(
                "InactiveGlowColor",
                typeof(Color),
                typeof(GlowWindow),
                new PropertyMetadata(
                    SystemColors.InactiveBorderColor, 
                    OnGlowColorChanged));
        #endregion

        #region 依赖属性相关方法
        /// <summary>
        /// 在辉光颜色依赖属性发生变换的时候被调用。第一次触发比 OnApplyTemplate() 早。
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        private static void OnGlowColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ((GlowWindow)obj).UpdateGlowEdgeColor();
        }
        #endregion
        #endregion


        #region 重写的方法
        #region 窗口非客户区 WindowChrome
        protected override void InitializeChrome()
        {
            WindowChrome chrome = new WindowChrome
            {
                CornerRadius = new CornerRadius(0),
                GlassFrameThickness = new Thickness(-1),   // 完全不要非客户区
                UseAeroCaptionButtons = false,             // 不进行按钮的命中测试
                ResizeBorderThickness = new Thickness(0),  // 大小调整识别区域
                CaptionHeight = 0                          // 标题栏高度 0
            };

            WindowChrome.SetWindowChrome(this, chrome);
        }
        #endregion

        #region 在窗口创建完成之后，创建 GlowEdge
        /// <summary>
        /// 在窗口加载完成之后，创建 GlowEdge，设置颜色。
        /// </summary>
        /// <param name="args"></param>
        protected override void OnLoaded(RoutedEventArgs args)
        {
            base.OnLoaded(args);

            // 要在属性设置之前创建。
            CreateGlowWindowHandles();
            UpdateGlowEdgeColor();
        }
        #endregion

        #region 重写事件
        #region 释放 GlowEdge 的资源
        /// <summary>
        /// 在关闭之后，释放 GlowEdge 对象的资源专用。
        /// </summary>
        /// <param name="e"></param>
        /// <remarks>
        /// 注意，GlowEdge 的释放需要调用 DestroyWindow 方法，该方法无法在其他线程下成功执行。
        /// </remarks>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // 释放掉 GlowEdge 对象。一定要在 UI 线程中释放
            foreach (var c in GlowEdges)
                c.Dispose();
        }
        #endregion

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
        }
        #endregion

        #region 窗口过程
        /// <summary>
        /// 处理 Windows 的消息。并不是所有消息都有对应的事件。
        /// </summary>
        /// <param name="handled">如果处理了，设置为 TRUE，没有处理设置为 false</param>
        /// <returns>
        /// 1、WM_ACTIVATE 是在获得 Activate 之后，要想控制 Activate，使用 SetWindowPos 或者 WM_WINDOWPOSCHANGING。
        /// 2、窗口作为对话框时的闪烁，是连续的 WM_NCACTIVATE 消息，不是 WM_ACTIVATE。WM_ACTIVATE 是客户区的。
        /// 3、不管是位置变化、还是Z顺序变化，还是大小变化，都是 WM_WINDOWPOSCHANGED 和 WM_WINDOWPOSCHANGING 消息，是 SetWindowPos 和 WINDOWPOS
        /// </returns>
        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg <= (int)WindowsMessage.WM_WINDOWPOSCHANGED)
            {
                if (msg != (int)WindowsMessage.WM_QUIT)
                {
                    switch (msg)
                    {
                        case (int)WindowsMessage.WM_ACTIVATE:
                            break;
                        case (int)WindowsMessage.WM_SIZE:
                            UpdateGlowEdgeVisibility(wParam.ToInt32());
                            break;
                        // 让 GlowEdge 保持相对位置的不变。
                        case (int)WindowsMessage.WM_WINDOWPOSCHANGED:
                            OnPositionChanged();
                            break;
                    }
                }
            }
            else
            {
                // 模态窗口在点击其父窗口的时候，会闪烁，只需要处理一下 WM_NCACTIVATE 消息就可以了。
                if (msg == (int)WindowsMessage.WM_NCACTIVATE)
                {
                    OnActiveChanged((Win32x.LOWORD(wParam.ToInt32())) != ((int)ActiveMsgValue.WA_INACTIVE));
                    return base.WndProc(hwnd, msg, wParam, lParam, ref handled);
                }
            }
            return base.WndProc(hwnd, msg, wParam, lParam, ref handled);
        }
        #endregion
        #endregion


        #region 私有的方法
        #region 创建 GlowEdge
        /* 可以避免重复创建不假，但是是不是有点故意找麻烦的感觉？ */
        /// <summary>
        /// 窗口 GlowEdge。
        /// </summary>
        /// <remarks>
        /// 调用 <see cref="WindowCustomizeTest.GlowWindowCore.HwndWrapper.EnsureHandle"/> 才能真正创建对象（win32窗口）
        /// </remarks>
        private void CreateGlowWindowHandles()
        {
            for (int i = 0; i < _glowEdges.Length; i++)
                GetOrCreateGlowEdge(i).EnsureHandle();
        }

        /// <summary>
        /// 返回需要的 GlowEdge 对象。
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        private GlowEdge GetOrCreateGlowEdge(int direction)
        {
            return _glowEdges[direction] ?? (_glowEdges[direction] = new GlowEdge(this, (Dock)direction));
        }
        #endregion

        #region 可用的 GlowEdge （不是 null 的 GlowEdge 对象）
        private IEnumerable<GlowEdge> GlowEdges => from w in _glowEdges where w != null select w;
        #endregion

        #region 在窗口的位置、大小等发生变化之后
        private void OnPositionChanged()
        {
            foreach (var e in GlowEdges)
            {
                e.UpdatePosition();
            }
        }
        #endregion

        #region 活动状态改变的时候
        private void OnActiveChanged(bool active)
        {
            foreach (var c in GlowEdges)
                c.IsActive = active;
        }
        #endregion

        #region 可见状态发生变化的时候
        private bool show = true;
        private void UpdateGlowEdgeVisibility(int wParam)
        {
            if (wParam == (int)SIZEMsgValue.SIZE_MINIMIZED)
                show = false;
            else
                show = true;
            /*
            foreach (var c in GlowEdges)
                c.Visible(false);
            else if (wParam == (int)SIZEMsgValue.SIZE_RESTORED)
                Dispatcher.InvokeAsync(
                    new Action(() =>
                    {
                        foreach (var c in GlowEdges)
                            c.Visible(true);
                    }));*/
            
            

            if (_makeGlowVisibleTimer != null)
            {
                _makeGlowVisibleTimer.Stop();
            }
            _makeGlowVisibleTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(200.0)
            };
            _makeGlowVisibleTimer.Tick += new EventHandler((o,e)=> {
                foreach (var c in GlowEdges)
                    c.Visible(show);
                _makeGlowVisibleTimer.Stop();
            });
            _makeGlowVisibleTimer.Start();
        }

        private void UpdateGlowEdgeVisibility()
        {
            foreach (var c in GlowEdges)
                c.Visible(show);
        }
        #endregion

        #region 框颜色发生变化的时候
        private void UpdateGlowEdgeColor()
        {
            foreach (var current in GlowEdges)
            {
                current.ActiveColor = ActiveGlowColor;
                current.InactiveColor = InactiveGlowColor;
            }
        }
        #endregion
        #endregion
    }
}
