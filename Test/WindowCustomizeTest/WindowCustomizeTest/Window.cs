using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shell;
using WindowCustomizeTest.Win32;

namespace WindowCustomizeTest
{
    /// <summary>
    /// 自定义窗口。
    /// </summary>
    /// <remarks>
    /// 重点：
    ///     1、窗口过程的捕获。使用 HwndSource 进行窗口过程捕获；
    ///     2、C# 扩展方法。static 类的 static 方法；
    ///     3、怎么使用系统命令；
    /// </remarks>
    public class Window : System.Windows.Window
    {
        #region ElementName
        // private const string RootElementName = "PART_Root";
        private const string NonClientAreaElementName = "PART_NonClientArea";
        private const string TitleElementName = "PART_Title";
        private const string IconElementName = "PART_IconButton";
        private const string CloseButtonElementName = "PART_CloseButton";
        private const string MaxiButtonElementName = "PART_MaxiButton";
        private const string MiniButtonElementName = "PART_MiniButton";
        private const string RestoreButtonElementName = "PART_RestoreButton";
        #endregion

        #region 属性
        //private Control _root;
        private UIElement _nonClientArea;
        private Button _icon;
        private TextBlock _title;
        #endregion


        #region 构造函数
        /// <summary>
        /// 重新定位默认样式。
        /// </summary>
        static Window()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata(typeof(Window)));
        }

        public Window()
        {
            // 去除窗口非客户区域
            InitializeChrome();
            // 绑定窗口完成事件（一般情况下，这是最后一个事件）。
            Loaded += (o, e) => OnLoaded(e);
        }
        #endregion


        #region 可重写的方法
        /* 这些方法的调用顺序，是从上往下 */
        #region 初始化默认的非客户区
        /// <summary>
        /// 设置 WindowChrome，去除窗口原有的非客户区框架。
        /// </summary>
        /// <remarks>
        /// 推荐使用该方法，这种方式可以保留系统特性。
        /// </remarks>
        protected virtual void InitializeChrome()
        {
            WindowChrome chrome = new WindowChrome
            {
                CornerRadius = new CornerRadius(0),
                GlassFrameThickness = new Thickness(-1),   // 完全不要非客户区
                UseAeroCaptionButtons = false,             // 不进行按钮的命中测试
                ResizeBorderThickness = new Thickness(8),  // 大小调整识别区域
                CaptionHeight = 0                          // 标题栏高度 0
            };

            WindowChrome.SetWindowChrome(this, chrome);
        }
        #endregion

        #region 在加载完成之后
        /// <summary>
        /// 当对元素进行布局、呈现，且可将其用于交互时发生。通常是在元素初始化序列中引发的最后一个事件。
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnLoaded(RoutedEventArgs args)
        {
            /* 系统提供的命令、事件只是定义，没有实现，因此需要我们提供实现。PS：SystemCommands 继承自 RoutedCommand，没有实现各个函数 */
            CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand,
                (s, e) => WindowState = WindowState.Minimized));
            CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand,
                (s, e) => WindowState = WindowState.Maximized));
            CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand,
                (s, e) => WindowState = WindowState.Normal));
            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, (s, e) => Close()));
            CommandBindings.Add(new CommandBinding(SystemCommands.ShowSystemMenuCommand, ShowSystemMenu));
        }
        #endregion

        #region 窗口过程
        protected virtual IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case (int)WindowsMessage.WM_NCHITTEST:
                    return HitTest(lParam, ref handled);
                // 下面两个消息，是上古时代的，自定义窗口需要将他们屏蔽
                case (int)WindowsMessage.WM_NCUAHDRAWCAPTION:
                case (int)WindowsMessage.WM_NCUAHDRAWFRAME:
                    handled = true;
                    break;
            } // 记得删除
            
            return IntPtr.Zero; // 不要返回默认的 窗口过程函数，会使模板样式失效。
        }
        #endregion
        #endregion


        #region 重写的方法
        /* 这些方法的调用顺序，是从上往下 */
        /// <summary>
        /// 在完成模板的应用之后，我们从模板中获取需要的控件。
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            /* 从模板中查找需要的控件 */
            InitializeElements();
        }

        /// <summary>
        /// 如果需要与 win32 交互，可以在这个函数中进行准备工作。
        /// *注意：事实证明，在 base.OnSourceInitialized(e) 之前设置钩子，会导致部分消息拦截失败！
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            this.GetHwndSource()?.AddHook(WndProc);
        }

        protected override void OnStateChanged(EventArgs e)
        {
            // 保证窗口在窗体变换时位于最顶层，以呈现动画。
            bool top = Topmost;
            Topmost = true;
            
            base.OnStateChanged(e);

            //OnStateChanged();
            Topmost = top;
        }
        #endregion


        #region 私有、保护的方法
        #region 初始化需要的控件
        /// <summary>
        /// 查找窗口需要的控件，绑定控件事件。
        /// </summary>
        /// <remarks>
        /// 注意：在 OnApplyTemplate() 之后调用。
        /// </remarks>
        private void InitializeElements()
        {
            // 非客户区，用来相应移动等系统命令
            _nonClientArea = GetTemplateChild(NonClientAreaElementName) as UIElement;
            
            // 标题栏，如果标题栏设置了颜色，那么会影响移动命令（在标题栏上无法移动了）
            if (GetTemplateChild(TitleElementName) is TextBlock t)
                _title = t;

            // 图标实际上可以点击，会显示菜单。
            if (GetTemplateChild(IconElementName) is Button ib)
            {
                ib.Command = SystemCommands.ShowSystemMenuCommand;
                _icon = ib;
            }

            if (GetTemplateChild(CloseButtonElementName) is Button cb)
                cb.Command = SystemCommands.CloseWindowCommand;

            if (GetTemplateChild(MaxiButtonElementName) is Button mab)
                mab.Command = SystemCommands.MaximizeWindowCommand;
            
            if (GetTemplateChild(MiniButtonElementName) is Button mib)
                mib.Command = SystemCommands.MinimizeWindowCommand;

            if (GetTemplateChild(RestoreButtonElementName) is Button rb)
                rb.Command = SystemCommands.RestoreWindowCommand;

            // _root = GetTemplateChild(RootElementName) as Control;
        }
        #endregion

        #region 显示系统菜单
        private void ShowSystemMenu(object sender, ExecutedRoutedEventArgs e)
        {
            var point = new Point(0, _icon.ActualHeight);
            SystemCommands.ShowSystemMenu(this, PointToScreen(point));
        }
        #endregion

        #region 命中测试
        protected IntPtr HitTest(IntPtr lParam, ref bool handled)
        {
            // 获取鼠标坐标（相对于屏幕）
            var mousePointX = Win32x.GET_X_LPARAM((int)lParam);
            var mousePointY = Win32x.GET_Y_LPARAM((int)lParam);
            // 转换到窗口，并且获取命中的控件（窗口子元素）
            var point = _nonClientArea.PointFromScreen(new Point(mousePointX, mousePointY));
            var inputElement = _nonClientArea.InputHitTest(point);
            // 如果是非客户区或者是标题，就允许移动
            if (inputElement != null && (inputElement == _nonClientArea || inputElement == _title))
            {
                handled = true;
                return (IntPtr)HitTestResult.HTCAPTION;
            }

            return IntPtr.Zero;
        }
        #endregion

        #region 窗口状态发生变化
        /*
        private void OnStateChanged()
        {
            if (WindowState == WindowState.Maximized)
            {
                if (_maxiButton != null) _maxiButton.Visibility = Visibility.Collapsed;
                if (_restoreButton != null) _restoreButton.Visibility = Visibility.Visible;
            }
            else if (WindowState == WindowState.Normal)
            {
                if (_maxiButton != null) _maxiButton.Visibility = Visibility.Visible;
                if (_restoreButton != null) _restoreButton.Visibility = Visibility.Collapsed;
            }
        }*/
        #endregion
        #endregion
    }
}
