using System;
using System.Runtime.InteropServices;
using System.Windows;

/// <summary>
/// C# 与非托管语言的相互操作。参考：https://docs.microsoft.com/zh-cn/dotnet/framework/interop/passing-structures
/// </summary>
/// <remarks>
/// 
/// 关于字符串
///   不同语言之间可能不能共享对象数据结构，所以字符串也是无法共享的，所以一般情况下，是c语言风格的字符串，也就是
///   以 null 结尾的字符串。这就需要注意了，C# 的 string（其他语言也是一样）没有null，所以在使用的时候要格外注意
///   用 [MarshalAs(UnmanagedType.LPWStr)] 可以将字符串转成带有 null 的。
/// 
/// 关于结构体和类对象
/// StructLayout：对类或者结构体的物理内存进行控制。
///         LayoutKind.Sequential 顺序的
///         LayoutKind.Explicit 控制每一个成员的位置（注意，是位置，大小时不能控制的）。关于类或者结构体的大小，百度
///         
/// 
/// </remarks>
namespace WindowCustomizeTest.Win32
{
    public delegate IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    public delegate IntPtr DefWndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    /// <summary>
    /// 与EnumThreadWindows函数一起使用的应用程序定义的回调函数。 
    /// 它接收与线程关联的窗口句柄。 
    /// WNDENUMPROC类型定义一个指向此回调函数的指针。 
    /// EnumThreadWndProc是应用程序定义的函数名称的占位符。
    /// </summary>
    /// <param name="hWnd">与 <see cref="ApiUser32.EnumThreadWindows"/> 函数中指定的线程关联的窗口的句柄。</param>
    /// <param name="lParam"><see cref="ApiUser32.EnumThreadWindows"/>函数中给定的应用程序定义的值。</param>
    /// <returns></returns>
    public delegate bool EnumThreadWndProc(IntPtr hWnd, IntPtr lParam);

    
    #region 结构体
    #region WNDCLASSEX(实际上是WNDCLASSEXW)
    /// <summary>
    /// 一般就是 LayoutKind.Sequential，C# 是UTF-16，所以 CharSet.Unicode
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct WNDCLASSEX
    {
        public uint    cbSize;       // 结构体的大小。用 (uint)Marshal.SizeOf(typeof(WNDCLASSEX))
        public uint    style;        // 窗口样式。看 WindowClassStyle
        public IntPtr  lpfnWndProc;  // 用 Marshal.GetFunctionPointerForDelegate
        public int     cbClsExtra;   // 根据窗口类结构分配的额外字节数。 系统将字节初始化为零。
        public int     cbWndExtra;   // 窗口实例之后要分配的额外字节数。 系统将字节初始化为零。 如果应用程序使用WNDCLASSEX在资源文件中注册使用CLASS指令创建的对话框，则它必须将此成员设置为DLGWINDOWEXTRA。
        public IntPtr  hInstance;    // 包含类的窗口过程的实例的句柄。
        public IntPtr  hIcon;        // 类图标的句柄。 该成员必须是图标资源的句柄。 如果该成员为NULL，则系统提供默认图标。
        public IntPtr  hCursor;      // 类光标的句柄。 该成员必须是游标资源的句柄。 如果此成员为NULL，则每当鼠标移入应用程序窗口时，应用程序必须显式设置光标形状。
        public IntPtr  hbrBackground;// 
        [MarshalAs(UnmanagedType.LPWStr)] // 这是必须的，因为 string 不是以 null 结尾的。表示以 NULL 结尾的宽字符字符串。
        public string  lpszMenuName; // 指向以空字符结尾的字符串的字符串，该字符串指定类菜单的资源名称，该名称出现在资源文件中。 如果使用整数标识菜单，请使用MAKEINTRESOURCE宏。 如果此成员为NULL，则属于此类的Windows没有默认菜单。
        [MarshalAs(UnmanagedType.LPWStr)] // 这是必须的。
        public string  lpszClassName;// 类名
        public IntPtr  hIconSm;      // 与窗口类关联的小图标的句柄。 如果此成员为NULL，则系统将在hIcon成员指定的图标资源中搜索适当大小的图标以用作小图标。
    }
    #endregion

    #region WNDCLASS
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct WNDCLASS
    {
        public uint style;
        public Delegate lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpszMenuName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpszClassName;
    }
    #endregion

    #region POINT
    [StructLayout(LayoutKind.Sequential)]
    internal struct POINT
    {
        public int x; // int 够用
        public int y;
    }
    #endregion

    #region SIZE
    [StructLayout(LayoutKind.Sequential)]
    internal struct SIZE
    {
        public int cx; // int 够用
        public int cy;
    }
    #endregion

    #region RECT
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public RECT(Rect rect)
        {
            Left = (int)rect.Left;
            Top = (int)rect.Top;
            Right = (int)rect.Right;
            Bottom = (int)rect.Bottom;
        }

        public Point Position => new Point(Left, Top);
        public Size Size => new Size(Width, Height);

        public int Height
        {
            get => Bottom - Top;
            set => Bottom = Top + value;
        }

        public int Width
        {
            get => Right - Left;
            set => Right = Left + value;
        }
    }
    #endregion

    #region BITMAPINFO
    /// <summary>
    /// BITMAPINFO结构最后是调色板数据,包含若干个RGBQUAD条目,一般来说最大为256个,定义为长度为1的数组是c语言里常用的一个技巧,用来定义可变长度的结构.你在程序里需要分配够保存调色板的大小,最好把mBitmapInfo定义为一个指针
    /// mBitmapInfo = (BITMAPINFO*) malloc(sizeof(BITMAPINFO)+256*sizeof(RGBQUAD));
    /// https://docs.microsoft.com/zh-cn/windows/win32/api/wingdi/ns-wingdi-bitmapinfo
    /// </summary>
    /// <remarks>
    /// public RGBQUAD[] bmiColors（原定义为 RGBQUAD bmiColors[1]）是位图的颜色表或者颜色蒙版数据，并不是一定会有数据的。
    /// 其功能和指针类似，因为颜色蒙版数据大小是未知的，因此难以给出固定的大小。结构体的内存是连续的，也就是 BITMAPINFO 如果
    /// 存在 RGBQUAD 数据，那么就是紧跟 BITMAPINFOHEADER 之后的，也就是首地址即是 BITMAPINFO 的地址，也是 BITMAPINFOHEADER
    /// 的地址，如果不存在 RGBQUAD 的数据，可以将 BITMAPINFOHEADER 直接当做 BITMAPINFO 使用。
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct BITMAPINFO
    {
        public BITMAPINFOHEADER bmiHeader;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1, ArraySubType = UnmanagedType.Struct)]
        public RGBQUAD[] bmiColors; // 这个是调色板，真彩不需要调色板。
    }
    #endregion

    #region BITMAPINFOHEADER
    /// <summary>
    /// https://docs.microsoft.com/zh-cn/windows/win32/api/wingdi/ns-wingdi-bitmapinfoheader?f1url=https%3A%2F%2Fmsdn.microsoft.com%2Fquery%2Fdev16.query%3FappId%3DDev16IDEF1%26l%3DZH-CN%26k%3Dk(WINGDI%2FBITMAPINFOHEADER)%3Bk(BITMAPINFOHEADER)%3Bk(DevLang-C%2B%2B)%3Bk(TargetOS-Windows)%26rd%3Dtrue
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct BITMAPINFOHEADER
    {
        /// <summary>
        /// 结构所需的字节数。
        /// </summary>
        public int biSize;
        /// <summary>
        /// 位图的宽度，以像素为单位。
        /// 
        /// 如果biCompression为BI_JPEG或BI_PNG，则biWidth成员分别指定解压缩的JPEG或PNG图像文件的宽度。
        /// </summary>
        public int biWidth;
        /// <summary>
        /// 位图的高度，以像素为单位。 如果biHeight为正，则位图是自下而上的DIB，其原点是左下角。 如果biHeight为负，则位图是自上而下的DIB，其原点是左上角。
        /// 
        /// 如果biHeight为负，表示自上而下的DIB，则biCompression必须为BI_RGB或BI_BITFIELDS。 自上而下的DIB无法压缩。
        /// 
        /// 如果biCompression为BI_JPEG或BI_PNG，则biHeight成员分别指定解压缩的JPEG或PNG图像文件的高度。
        /// </summary>
        public int biHeight; // 全彩是负数
        /// <summary>
        /// 目标设备的平面数。 此值必须设置为1。
        /// </summary>
        public ushort biPlanes;
        /// <summary>
        /// 每像素位数。
        /// biBitCount成员确定定义每个像素的位数以及位图中的最大颜色数。 
        /// 取值为：0、1、4、8、16、24、32
        /// </summary>
        public ushort biBitCount;
        /// <summary>
        /// 自底向上的压缩位图的压缩类型（自顶向下的DIB无法压缩）。 该成员可以是以下值之一。
        /// </summary>
        public BIC biCompression;
        /// <summary>
        /// 指定图像的大小（以字节为单位）。 对于未压缩的RGB位图，可以将其设置为0。
        /// </summary>
        public int biSizeImage;
        /// <summary>
        /// 
        /// </summary>
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        /// <summary>
        /// 指定颜色表中位图实际使用的颜色索引数。指出 BITMAPINFO 的 RGBQUAD 的大小。
        /// </summary>
        public int biClrUsed;
        /// <summary>
        /// 指定被认为对显示位图很重要的颜色索引数。 如果该值为零，则所有颜色都很重要。
        /// </summary>
        public int biClrImportant;
    }
    #endregion

    #region RGBQUAD
    [StructLayout(LayoutKind.Sequential)]
    internal struct RGBQUAD
    {
        public byte rgbBlue;
        public byte rgbGreen;
        public byte rgbRed;
        public byte rgbReserved;
    }
    #endregion

    #region BLENDFUNCTION
    [StructLayout(LayoutKind.Sequential)]
    internal struct BLENDFUNCTION
    {
        /// <summary>
        /// 源混合操作，目前唯一能指定的的值是 <see cref="AlphaBlendControl.AC_SRC_OVER"/> (0x00)
        /// </summary>
        public byte BlendOp;
        /// <summary>
        /// 必须是 0
        /// </summary>
        public byte BlendFlags;
        /// <summary>
        /// 指定在整个源位图上使用的 Alpha 透明度值。这个值和源的透明度值结合在一起，
        /// 也就是在源的透明度的基础上，在透明上这个值。因此，如果这个值是0，那么就假
        /// 定源是完全透明的，如果使用源的透明度，需要将这个值设为255
        /// </summary>
        public byte SourceConstantAlpha;
        /// <summary>
        /// 控制源位图和目标位图的混合方式，目前只有 <see cref="AlphaBlendControl.AC_SRC_ALPHA"/> (0x01)。
        /// 
        /// 作用：
        ///     1、设置 AC_SRC_ALPHA 后，源的 Alpha 和 <see cref="SourceConstantAlpha"/> 一起生效
        ///     2、设置为 0，将忽略源的 alpha，只有 <see cref="SourceConstantAlpha"/> 生效。
        /// </summary>
        public byte AlphaFormat;
    }
    #endregion

    #region WINDOWPOS窗口位置
    [StructLayout(LayoutKind.Sequential)]
    internal struct WINDOWPOS
    {
        /// <summary>
        /// 窗口在Z顺序中的位置（前后位置）。 
        /// 该成员可以是放置该窗口的窗口的句柄，也可以是 
        ///     <see cref="ApiUser32.SetWindowPos(IntPtr, IntPtr, int, int, int, int, SWPFlags)"/> 
        /// 函数列出的特殊值之一。<see cref="HWNDPos"/>
        /// </summary>
        public IntPtr hwndInsertAfter;
        public IntPtr hwnd;
        public int x;
        public int y;
        public int cx;
        public int cy;
        public SWPFlags flags;
    }
    #endregion
    #endregion


    #region 窗口类样式
    internal enum WndClassStyle : uint
    {
        CS_VREDRAW         = 0x0001, // 如果移动或大小调整改变了客户区的高度，则重新绘制整个窗口。
        CS_HREDRAW         = 0x0002, // 如果移动或大小调整改变了客户区域的宽度，则重新绘制整个窗口。
        CS_DBLCLKS         = 0x0008, // 当用户在属于类的窗口中双击鼠标时，向窗口过程发送一个双击消息。
        CS_NOCLOSE         = 0x0200, // 禁用窗口菜单上的关闭。
        CS_DROPSHADOW      = 0x00020000, // 在窗口上启用投影效果。这个效果是通过SPI_SETDROPSHADOW打开和关闭的。通常，这是为一些短期存在的小窗口(如菜单)启用的，以强调它们与其他窗口的z顺序关系。从具有这种样式的类创建的窗口必须是顶级窗口;它们可能不是子窗口。
        CS_BYTEALIGNCLIENT = 0x1000, // 按字节边界对齐窗口的工作区(x方向)。此样式影响窗口的宽度及其在显示器上的水平位置。
        CS_BYTEALIGNWINDOW = 0x2000, // 按字节边界对齐窗口(x方向)。此样式影响窗口的宽度及其在显示器上的水平位置。
        CS_CLASSDC         = 0x0040, // 分配一个设备上下文让类中的所有窗口共享。因为窗口类是特定于进程的，所以应用程序的多个线程可以创建同一个类的窗口。线程也可以尝试同时使用关联设备。当这种情况发生时，系统只允许一个线程成功地完成绘图操作。
        CS_GLOBALCLASS     = 0x4000, // 指示窗口类是应用程序全局类。有关更多信息，请参见关于窗口类的“应用程序全局类”部分。
        CS_OWNDC           = 0x0020, // 为类中的每个窗口分配唯一的关联设备。
        CS_PARENTDC        = 0x0080, // 将子窗口的剪切矩形设置为父窗口的剪切矩形，以便子窗口可以在父窗口上绘制。一个带有CS PARENTDC样式的窗口从设备上下文的系统缓存中接收一个常规的设备上下文。它不给子程序父程序的关联设备或关联设备设置。指定CS PARENTDC可以增强应用程序的性能。
        CS_SAVEBITS        = 0x0800, // 将屏幕图像被此类窗口遮盖的部分另存为位图。 删除窗口后，系统将使用保存的位图来还原屏幕图像，包括其他被遮盖的窗口。 因此，如果位图所使用的内存尚未被丢弃，并且其他屏幕操作并未使存储的图像无效，则系统不会将WM_PAINT消息发送到被遮盖的窗口。此样式对于短暂显示的小窗口（例如菜单或对话框）很有用，然后在进行其他屏幕活动之前将其删除。 这种样式增加了显示窗口所需的时间，因为系统必须首先分配内存来存储位图。

        CS_DEFAULT = CS_VREDRAW | CS_HREDRAW,
        CS_NULL = 0
    }
    #endregion

    #region 窗口样式
    internal enum WindowStyle : uint
    {
        WS_BORDER       = 0x00800000, // 窗口具有细线边框。
        WS_CAPTION      = 0x00C00000, // 该窗口具有标题栏（包括WS_BORDER样式）。
        WS_CHILD        = 0x40000000, // 该窗口是子窗口。 具有这种样式的窗口不能具有菜单栏。 此样式不能与WS_POPUP样式一起使用。
        WS_CHILDWINDOW  = 0x40000000, // 与WS_CHILD样式相同。
        WS_CLIPCHILDREN = 0x02000000, // 在父窗口内进行绘制时，不包括子窗口所占的区域。 创建父窗口时使用此样式。
        WS_CLIPSIBLINGS = 0x04000000, // 相对于彼此剪辑子窗口； 也就是说，当特定的子窗口接收到WM_PAINT消息时，WS_CLIPSIBLINGS样式会将所有其他重叠的子窗口剪切到要更新的子窗口区域之外。 如果未指定WS_CLIPSIBLINGS并且子窗口重叠，则在子窗口的客户区域内进行绘制时，可以在相邻子窗口的客户区域内进行绘制。
        WS_DISABLED     = 0x08000000, // 该窗口最初被禁用。 禁用的窗口无法接收来自用户的输入。 要在创建窗口后更改此设置，请使用EnableWindow函数。
        WS_DLGFRAME     = 0x00400000, // 窗口具有通常用于对话框的样式的边框。 具有这种样式的窗口不能具有标题栏。
        WS_GROUP        = 0x00020000, // 该窗口是一组控件中的第一个控件。 该组由该第一个控件和在其后定义的所有控件组成，直到下一个具有WS_GROUP样式的下一个控件。 每个组中的第一个控件通常具有WS_TABSTOP样式，以便用户可以在组之间移动。 用户随后可以使用方向键将键盘焦点从组中的一个控件更改为组中的下一个控件。您可以打开和关闭此样式以更改对话框导航。 若要在创建窗口后更改此样式，请使用SetWindowLong函数。
        WS_HSCROLL      = 0x00100000, // 该窗口具有水平滚动条。
        WS_ICONIC       = 0x20000000, // 最初将窗口最小化。 与WS_MINIMIZE样式相同。
        WS_MAXIMIZE     = 0x01000000, // 窗口最初是最大化的。
        WS_MAXIMIZEBOX  = 0x00010000, // 该窗口具有最大化按钮。 不能与WS_EX_CONTEXTHELP样式结合使用。 还必须指定WS_SYSMENU样式。
        WS_MINIMIZE     = 0x20000000, // 最初将窗口最小化。 与WS_ICONIC样式相同。
        WS_MINIMIZEBOX  = 0x00020000, // 该窗口有一个最小化按钮。 不能与WS_EX_CONTEXTHELP样式结合使用。 还必须指定WS_SYSMENU样式。
        WS_OVERLAPPED   = 0x00000000, // 该窗口是一个重叠的窗口。 重叠的窗口具有标题栏和边框。 与WS_TILED样式相同。
        WS_POPUP        = 0x80000000, // 窗口是一个弹出窗口。 此样式不能与WS_CHILD样式一起使用。
        WS_SIZEBOX      = 0x00040000, // 窗口具有大小调整边框。 与WS_THICKFRAME样式相同。
        WS_SYSMENU      = 0x00080000, // 该窗口的标题栏上有一个窗口菜单。 还必须指定WS_CAPTION样式。
        WS_TABSTOP      = 0x00010000, // 该窗口是一个控件，当用户按下TAB键时可以接收键盘焦点。 按下TAB键将键盘焦点更改为WS_TABSTOP样式的下一个控件。您可以打开和关闭此样式以更改对话框导航。 若要在创建窗口后更改此样式，请使用SetWindowLong函数。 为了使用户创建的窗口和无模式对话框可与制表符一起使用，请更改消息循环以调用IsDialogMessage函数。
        WS_THICKFRAME   = 0x00040000, // 窗口具有大小调整边框。 与WS_SIZEBOX样式相同。
        WS_TILED        = 0x00000000, // 该窗口是一个重叠的窗口。 重叠的窗口具有标题栏和边框。 与WS_OVERLAPPED样式相同。
        WS_VISIBLE      = 0x10000000, // 该窗口最初是可见的。可以使用ShowWindow或SetWindowPos函数打开和关闭此样式。
        WS_VSCROLL      = 0x00200000, // 该窗口具有垂直滚动条。

        WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
        WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
        WS_TILEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,

        WS_WDGLOWWINDOW = WS_POPUP | WS_VISIBLE | WS_CLIPSIBLINGS | WS_CLIPCHILDREN
    }
    #endregion

    #region 窗口扩展样式
    internal enum WindowExStyle : uint
    {
        WS_EX_ACCEPTFILES         = 0x00000010, // 窗口接受拖放文件。
        WS_EX_APPWINDOW           = 0x00040000, // 当窗口可见时，将顶级窗口强制放到任务栏上。
        WS_EX_CLIENTEDGE          = 0x00000200, // 窗户的边缘是下陷的。
        WS_EX_COMPOSITED          = 0x02000000, // 使用双缓冲按从下到上的绘制顺序绘制窗口的所有后代。 从下到上的绘画顺序允许后代窗口具有半透明（alpha）和透明（color-key）效果，但前提是后代窗口还设置了WS_EX_TRANSPARENT位。 双缓冲允许绘制窗口及其后代，而不会闪烁。 如果窗口的类样式为CS_OWNDC或CS_CLASSDC，则不能使用此方法。
        WS_EX_CONTEXTHELP         = 0x00000400, // 窗口的标题栏包含一个问号。 当用户单击问号时，光标将变为带有指针的问号。 如果用户然后单击子窗口，则该子窗口会收到WM_HELP消息。 子窗口应将消息传递给父窗口过程，该过程应使用HELP_WM_HELP命令调用WinHelp函数。 帮助应用程序显示一个弹出窗口，通常包含子窗口的帮助。WS_EX_CONTEXTHELP不能与WS_MAXIMIZEBOX或WS_MINIMIZEBOX样式一起使用。
        WS_EX_CONTROLPARENT       = 0x00010000, // 窗口本身包含应该参与对话框导航的子窗口。如果指定了此样式，则在执行导航操作(如处理TAB键、箭头键或键盘助记符)时，对话管理器将递归到此窗口的子窗口中。
        WS_EX_DLGMODALFRAME       = 0x00000001, // 窗口有一个双边框。 可以通过在dwStyle参数中指定WS_CAPTION样式来创建带有标题栏的窗口。
        WS_EX_LAYERED             = 0x00080000, // 该窗口是分层窗口。 如果窗口的类样式为CS_OWNDC或CS_CLASSDC，则不能使用此样式。Windows 8：顶级窗口和子窗口支持WS_EX_LAYERED样式。 以前的Windows版本仅对顶级窗口支持WS_EX_LAYERED。
        WS_EX_LAYOUTRTL           = 0x00400000, // 如果外壳语言是希伯来语，阿拉伯语或其他支持阅读顺序对齐的语言，则窗口的水平原点在右边缘。 水平值增加到左侧。
        WS_EX_LEFT                = 0x00000000, // 窗口具有左对齐的通用属性。这是默认值。
        WS_EX_LEFTSCROLLBAR       = 0x00004000, // 如果外壳语言是希伯来语，阿拉伯语或其他支持阅读顺序对齐的语言，则垂直滚动条（如果有）位于客户区域的左侧。 对于其他语言，样式将被忽略。
        WS_EX_LTRREADING          = 0x00000000, // 使用从左到右的阅读顺序属性显示窗口文本。 这是默认值。
        WS_EX_MDICHILD            = 0x00000040, // 该窗口是MDI子窗口。
        WS_EX_NOACTIVATE          = 0x08000000, // 当用户单击它时，以这种样式创建的顶级窗口不会成为前台窗口。 当用户最小化或关闭前景窗口时，系统不会将此窗口置于前景。不应通过程序访问或使用讲述人等可访问技术通过键盘导航来激活该窗口。要激活窗口，使用SetActiveWindow或SetForegroundWindow函数。默认情况下，该窗口不显示在任务栏上。 要强制窗口显示在任务栏上，请使用WS_EX_APPWINDOW样式。
        WS_EX_NOINHERITLAYOUT     = 0x00100000, // 该窗口不会将其窗口布局传递给其子窗口。
        WS_EX_NOPARENTNOTIFY      = 0x00000004, // 使用此样式创建的子窗口在创建或销毁时不会将WM_PARENTNOTIFY消息发送到其父窗口。
        WS_EX_NOREDIRECTIONBITMAP = 0x00200000, // 窗口不渲染到重定向表面。 这适用于没有可见内容或使用表面以外的机制提供视觉效果的窗口。
        WS_EX_RIGHT               = 0x00001000, // 该窗口具有通用的“右对齐”属性。 这取决于窗口类。 仅当外壳语言是希伯来语，阿拉伯语或其他支持阅读顺序对齐的语言时，此样式才有效。 否则，样式将被忽略。
        WS_EX_RIGHTSCROLLBAR      = 0x00000000, // 垂直滚动条（如果有）在客户区域的右侧。 这是默认值。
        WS_EX_RTLREADING          = 0x00002000, // 如果外壳语言是希伯来语，阿拉伯语或其他支持阅读顺序对齐的语言，则使用从右到左的阅读顺序属性显示窗口文本。 对于其他语言，样式将被忽略。
        WS_EX_STATICEDGE          = 0x00020000, // 该窗口具有三维边框样式，旨在用于不接受用户输入的项目。
        WS_EX_TOOLWINDOW          = 0x00000080, // 该窗口旨在用作浮动工具栏。 工具窗口的标题栏比普通标题栏短，并且窗口标题使用较小的字体绘制。 当用户按下ALT + TAB时，工具窗口不会出现在任务栏或对话框中。 如果工具窗口具有系统菜单，则其图标不会显示在标题栏上。 但是，您可以通过右键单击或键入ALT + SPACE来显示系统菜单。
        WS_EX_TOPMOST             = 0x00000008, // 该窗口应放置在所有非最上面的窗口之上，并且即使在停用该窗口的情况下也应保持在它们之上。 若要添加或删除此样式，请使用SetWindowPos函数。
        WS_EX_TRANSPARENT         = 0x00000020, // 在绘制窗口下方的兄弟姐妹（由同一线程创建）之前，不应绘制窗口。 该窗口显示为透明，因为基础同级窗口的位已被绘制。
        WS_EX_WINDOWEDGE          = 0x00000100, // 窗口的边框带有凸起的边缘。

        WS_EX_OVERLAPPEDWINDOW  = WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE,
        WS_EX_PALETTEWINDOW     = WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST,

        // GlowEdge 窗口的扩展样式
        WS_EX_WDGLOWWINDOW      = WS_EX_LEFT | WS_EX_LTRREADING | WS_EX_RIGHTSCROLLBAR | WS_EX_TOOLWINDOW | WS_EX_LAYERED
    }
    #endregion



    #region HitTestResult
    internal enum HitTestResult : int
    {
        HTBORDER      = 18, // 在没有尺寸边框的窗口的边框中。
        HTBOTTOM      = 15, // 在可调整大小的窗口的水平下边框中（用户可以单击鼠标以垂直调整窗口大小）。
        HTBOTTOMLEFT  = 16, // 在可调整大小的窗口的边框的左下角（用户可以单击鼠标以对角地调整窗口的大小）。
        HTBOTTOMRIGHT = 17, // 在可调整大小的窗口的边框的右下角（用户可以单击鼠标以对角地调整窗口的大小）。
        HTCAPTION     = 2,  // 在标题栏中。
        HTCLIENT      = 1,  // 在客户区。
        HTCLOSE       = 20, // 在关闭按钮。
        HTERROR       = -2, // 在屏幕背景上或窗口之间的分隔线上（与HTNOWHERE相同，但DefWindowProc函数会产生系统提示音以指示错误）。
        HTGROWBOX     = 4,  // 在尺寸框中（与HTSIZE相同）。
        HTHELP        = 21, // 在“帮助”按钮中。
        HTHSCROLL     = 6,  // 在水平滚动条中。
        HTLEFT        = 10, // 在可调整大小的窗口的左边框中（用户可以单击鼠标以水平调整窗口大小）。
        HTMENU        = 5,  // 在菜单中。
        HTMAXBUTTON   = 9,  // 在最大化按钮中。
        HTMINBUTTON   = 8,  // 在最小化按钮中。
        HTNOWHERE     = 0,  // 在屏幕背景上或窗口之间的分隔线上。
        HTREDUCE      = 8,  // 在最小化按钮中。
        HTRIGHT       = 11, // 在可调整大小的窗口的右边框中（用户可以单击鼠标以水平调整窗口大小）。
        HTSIZE        = 4,  // 在大小框（与HTGROWBOX相同）中。
        HTSYSMENU     = 3,  // 在窗口菜单或子窗口的关闭按钮中。
        HTTOP         = 12, // 在窗口的水平上边界中。
        HTTOPLEFT     = 13, // 在窗口边框的左上角。
        HTTOPRIGHT    = 14, // 在窗口边框的右上角。
        HTTRANSPARENT = -1, // 在当前由同一线程中的另一个窗口覆盖的窗口中（消息将发送到同一线程中的基础窗口，直到其中一个返回非HTTRANSPARENT的代码为止）。
        HTVSCROLL     = 7,  // 在垂直滚动条中。
        HTZOOM        = 9,  // 在最大化按钮中。
    }
    #endregion

    #region WM_ACTIVE 的wParam值
    internal enum ActiveMsgValue
    {
        WA_INACTIVE = 0,
        WA_ACTIVE = 1,
        WA_CLICKACTIVE = 2
    }
    #endregion

    #region WM_SIZE 的 wParam 值
    internal enum SIZEMsgValue
    {
        SIZE_RESTORED = 0,
        SIZE_MINIMIZED = 1,
        SIZE_MAXIMIZED = 2,
        SIZE_MAXSHOW = 3,
        SIZE_MAXHIDE = 4
    }
    #endregion

    #region GetWindowLong 和 SetWindowLong 的 nIndex
    internal enum GWLIndex : int
    {
        GWL_EXSTYLE     = -20,        // 设置新的扩展窗口样式。
        GWLP_HINSTANCE  = -6,         // 设置新的应用程序实例句柄。
        GWLP_ID         = -12,        // 设置子窗口的新标识符。 该窗口不能是顶级窗口。
        GWL_STYLE       = -16,        // 设置一个新的窗口样式。
        GWLP_USERDATA   = -21,        // 设置与窗口关联的用户数据。 该数据供创建该窗口的应用程序使用。 其值最初为零。
        GWLP_WNDPROC    = -4,         // 设置窗口过程的新地址。
        GWLP_HWNDPARENT = -8          // 设置父窗口的句柄（如果有）。
    }
    #endregion

    #region RedrawWindowFlags
    [Flags]
    internal enum RedrawWindowFlags : uint
    {
        /// <summary>
        /// 使 lprcUpdate 或 hrgnUpdate 无效（只有一个可以为非NULL）。 如果两个均为NULL，则整个窗口将无效。
        /// </summary>
        RDW_INVALIDATE = 0x0001,
        /// <summary>
        /// 导致WM_PAINT消息被发布到窗口，而不管窗口的任何部分是否无效。
        /// </summary>
        RDW_INTERNALPAINT = 0x0002,
        /// <summary>
        /// 使窗口重新绘制时使窗口接收WM_ERASEBKGND消息。 还必须指定RDW_INVALIDATE标志。 否则，RDW_ERASE无效。
        /// </summary>
        RDW_ERASE = 0x0004,

        /// <summary>
        /// 
        /// </summary>
        RDW_VALIDATE        = 0x0008,
        /// <summary>
        /// 禁止所有未决的内部WM_PAINT消息。 此标志不影响由非NULL更新区域产生的WM_PAINT消息。
        /// </summary>
        RDW_NOINTERNALPAINT = 0x0010,
        /// <summary>
        /// 禁止所有未决的WM_ERASEBKGND消息。
        /// </summary>
        RDW_NOERASE = 0x0020,
        RDW_NOCHILDREN      = 0x0040,
        RDW_ALLCHILDREN     = 0x0080,
        RDW_UPDATENOW       = 0x0100,
        RDW_ERASENOW        = 0x0200,

        RDW_FRAME           = 0x0400, // 使窗口的非客户区与更新区域相交的任何部分接收WM_NCPAINT消息。 还必须指定RDW_INVALIDATE标志。 否则，RDW_FRAME不起作用。 除非指定了RDW_UPDATENOW或RDW_ERASENOW，否则通常不会在RedrawWindow执行期间发送WM_NCPAINT消息。

        RDW_NOFRAME         = 0x0800 // 禁止所有未决的WM_NCPAINT消息。 此标志必须与RDW_VALIDATE一起使用，并且通常与RDW_NOCHILDREN一起使用。 应谨慎使用RDW_NOFRAME，因为它可能导致窗口的某些部分绘制不正确。
    }
    #endregion

    #region ShowWindow Flags
    internal enum ShowWindowFlags
    {
        SW_HIDE = 0,
        SW_SHOWNORMAL = 1,
        SW_SHOWMINIMIZED = 2,
        SW_MAXIMIZE = 3,
        SW_SHOWMAXIMIZED = 3,
        SW_SHOWNOACTIVATE = 4,
        SW_SHOW = 5,
        SW_MINIMIZE = 6,
        SW_SHOWMINNOACTIVE = 7,
        SW_SHOWNA = 8,
        SW_RESTORE = 9,
        SW_SHOWDEFAULT = 10,
        SW_FORCEMINIMIZE = 11,
    }
    #endregion

    #region BLENDFUNCTION 的属性的取值
    internal enum AlphaBlendControl : byte
    {
        AC_SRC_OVER  = 0x00,
        AC_SRC_ALPHA = 0x01
    }
    #endregion

    #region BITMAPINFO 需要的值
    /// <summary>
    /// https://docs.microsoft.com/en-us/previous-versions/dd183376(v=vs.85)
    /// </summary>
    internal enum BIC : int
    {
        BI_RGB       = 0,
        BI_RLE8      = 1,
        BI_RLE4      = 2,
        BI_BITFIELDS = 3,
        BI_JPEG      = 4,
        BI_PNG       = 5
    }
    #endregion

    #region CreateDIBSection usage 的值
    internal enum DIBColors
    {
        DIB_PAL_COLORS,
        DIB_RGB_COLORS
    }
    #endregion

    #region UpdateLayeredWindow 的 Flags
    internal enum ULWFlags : uint
    {
        ULW_COLORKEY = 0x00000001,
        ULW_ALPHA = 0x00000002,
        ULW_OPAQUE = 0x00000004,
        ULW_EX_NORESIZE = 0x00000008,
    }
    #endregion

    #region SetWindowPos、WINDOWPOS Flags
    /// <summary>
    /// https://docs.microsoft.com/zh-cn/windows/win32/api/winuser/nf-winuser-setwindowpos?f1url=https%3A%2F%2Fmsdn.microsoft.com%2Fquery%2Fdev16.query%3FappId%3DDev16IDEF1%26l%3DZH-CN%26k%3Dk(WINUSER%2FSetWindowPos)%3Bk(SetWindowPos)%3Bk(DevLang-C%2B%2B)%3Bk(TargetOS-Windows)%26rd%3Dtrue
    /// </summary>
    internal enum SWPFlags : uint
    {
        /// <summary>
        /// 保留当前大小（忽略cx和cy参数）。
        /// </summary>
        SWP_NOSIZE = 0x0001,
        /// <summary>
        /// 保留当前位置（忽略X和Y参数）。
        /// </summary>
        SWP_NOMOVE = 0x0002,
        /// <summary>
        /// 保留当前的Z顺序（忽略hWndInsertAfter参数）。
        /// </summary>
        SWP_NOZORDER = 0x0004,
        /// <summary>
        /// 不重绘更改。 
        /// 如果设置了此标志，则不会进行任何重绘。 
        /// 这适用于工作区，非工作区（包括标题栏和滚动条）以及由于移动窗口而未显示的父窗口的任何部分。 
        /// 设置此标志后，应用程序必须明确使窗口和父窗口中需要重绘的任何部分无效或重绘。
        /// </summary>
        SWP_NOREDRAW = 0x0008,
        /// <summary>
        /// 不激活窗口。 
        /// 如果未设置此标志，则激活窗口并将其移到最顶层或非顶层组的顶部（取决于hWndInsertAfter参数的设置）。
        /// </summary>
        SWP_NOACTIVATE = 0x0010,
        /// <summary>
        /// 应用使用SetWindowLong函数设置的新框架样式。 
        /// 即使未更改窗口的大小，也向该窗口发送WM_NCCALCSIZE消息。 
        /// 如果未指定此标志，则仅在更改窗口大小时才发送 WM_NCCALCSIZE。
        /// </summary>
        SWP_FRAMECHANGED = 0x0020,
        /// <summary>
        /// 在窗口周围绘制框架（在窗口的类描述中定义）。
        /// </summary>
        SWP_DRAWFRAME = 0x0020,
        /// <summary>
        /// 显示窗口。
        /// </summary>
        SWP_SHOWWINDOW = 0x0040,
        /// <summary>
        /// 隐藏窗口。
        /// </summary>
        SWP_HIDEWINDOW = 0x0080,
        /// <summary>
        /// 丢弃客户区的全部内容。 
        /// 如果未指定此标志，则在调整窗口大小或位置后，将保存客户区的有效内容并将其复制回客户区。
        /// </summary>
        SWP_NOCOPYBITS = 0x0100,
        /// <summary>
        /// 不更改所有者窗口在Z顺序中的位置。
        /// </summary>
        SWP_NOOWNERZORDER = 0x0200,
        /// <summary>
        /// 与SWP_NOOWNERZORDER标志相同。
        /// </summary>
        SWP_NOREPOSITION = 0x0200,
        /// <summary>
        /// 阻止窗口接收WM_WINDOWPOSCHANGING消息。
        /// </summary>
        SWP_NOSENDCHANGING = 0x0400,
        /// <summary>
        /// 防止生成WM_SYNCPAINT消息。
        /// </summary>
        SWP_DEFERERASE = 0x2000,
        /// <summary>
        /// 如果调用线程和拥有窗口的线程连接到不同的输入队列，则系统会将请求发布到拥有窗口的线程。 
        /// 这样可以防止在其他线程处理请求时调用线程阻塞其执行。
        /// </summary>
        SWP_ASYNCWINDOWPOS = 0x4000
    }
    #endregion

    #region SetWindowPos hWndInsertAfter 的值
    /// <summary>
    /// <see cref="ApiUser32.SetWindowPos(IntPtr, IntPtr, int, int, int, int, SWPFlags)"/> 或者
    /// <see cref="WINDOWPOS"/>
    /// 的 hWndInsertAfter 的值之一。
    /// </summary>
    internal enum HWNDPos:int
    {
        HWND_NOTOPMOST = -2,
        HWND_TOPMOST = -1,
        HWND_TOP = 0,
        HWND_BOTTOM = 1,
    }
    #endregion
}
