using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WindowCustomizeTest.Win32
{
    internal class ApiUser32
    {
        private const string User32 = "user32.dll";


        #region API(public)
        /*---------------假装分割线------------------*/
        #region DefWindowProcW
        [DllImport(User32, CharSet = CharSet.Auto)]
        public static extern IntPtr DefWindowProc(
            IntPtr hWnd, 
            int msg, 
            IntPtr wParam, 
            IntPtr lParam);
        #endregion

        /*---------------假装分割线------------------*/
        #region RegisterClassExW
        [DllImport(User32, CharSet = CharSet.Auto, SetLastError = true, EntryPoint = "RegisterClassExW")]
        public static extern ushort RegisterClassEx(ref WNDCLASSEX lpWndClass);
        #endregion

        #region CreateWindowEx
        [DllImport(User32, CharSet = CharSet.Auto, SetLastError = true, EntryPoint = "CreateWindowExW")]
        public static extern IntPtr CreateWindowEx(
            uint dwExStyle,                      // 正在创建的窗口的扩展窗口样式。
            IntPtr classAtom,                    // 窗口类名或者 ATOM，是 WChar*，这里用 Atom，是一个ushort
            string lpWindowName,                 // 窗口名称
            uint dwStyle,                        // 窗口样式
            int x,                               // 窗口位置
            int y,                               // 窗口位置
            int nWidth,                          // 窗宽度
            int nHeight,                         // 窗口高度
            IntPtr hWndParent,                   // 父窗口句柄
            IntPtr hMenu,                        // 菜单句柄
            IntPtr hInstance,                    // 模块句柄
            IntPtr lpParam);                     // 指向要通过WEAT_CREATE消息的lParam参数所指向的CREATESTRUCT结构（lpCreateParams成员）传递给窗口的值的指针。 此消息在返回之前已通过此函数发送到创建的窗口。如果应用程序调用CreateWindow创建MDI客户端窗口，则lpParam应指向CLIENTCREATESTRUCT结构。 如果MDI客户端窗口调用CreateWindow创建MDI子窗口，则lpParam应该指向MDICREATESTRUCT结构。 如果不需要其他数据，则lpParam可以为NULL。
        #endregion

        #region DestroyWindow
        [DllImport(User32, CharSet = CharSet.Auto, EntryPoint = "DestroyWindow", SetLastError = true)]
        [return:MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyWindow(IntPtr hWnd);
        #endregion

        #region UnregisterClass
        [DllImport(User32, CharSet = CharSet.Auto, EntryPoint = "UnregisterClassW", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnregisterClass(IntPtr lpClassAtom, IntPtr hInstance);
        #endregion

        #region ShowWindow
        /// <summary>
        /// 设置指定窗口的显示状态。
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="nCmdShow"><see cref=""/></param>
        /// <returns></returns>
        [DllImport(User32)]
        [return:MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(
            IntPtr hWnd, 
            ShowWindowFlags nCmdShow);
        #endregion

        #region UpdateWindow
        [DllImport(User32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UpdateWindow(IntPtr hWnd);
        #endregion

        /*---------------假装分割线------------------*/
        #region GetWindowLongPtr
        /// <summary>
        /// 用在 32 位的情况下，64 位不能使用该函数，要用 SetWindowLongPtr 代替。
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="nIndex"></param>
        /// <param name="dwNewLong"></param>
        /// <returns></returns>
        /// /// <remarks>
        /// setWindowLong 的返回值 LONG_PTR 是定义好的类型，在32位下是32位，在64位下是64位，但是在.net中，唯一可以这样的
        /// 就是 IntPtr 了，所以使用 IntPtr。
        /// </remarks>
        [DllImport(User32, CharSet = CharSet.Auto, EntryPoint = "GetWindowLong")]
        private static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        /// <summary>
        /// 更改指定窗口的属性。函数还在额外的窗口内存中指定偏移量时设置一个值。
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="nIndex"></param>
        /// <param name="dwNewLong"></param>
        /// <returns></returns>
        /// <remarks>
        /// setWindowLong 的返回值 LONG_PTR 是定义好的类型，在32位下是32位，在64位下是64位，但是在.net中，唯一可以这样的
        /// 就是 IntPtr 了，所以使用 IntPtr。
        /// </remarks>
        [DllImport(User32, CharSet = CharSet.Auto, EntryPoint = "GetWindowLongPtrW")]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        public static IntPtr GetWindowLong(IntPtr hWnd, GWLIndex nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)                                       // 64 位
                return GetWindowLongPtr(hWnd, (int)nIndex, dwNewLong);
            else                                                        // 32 位
                return GetWindowLong(hWnd, (int)nIndex, dwNewLong);
        }
        #endregion

        #region SetWindowLongPtr
        /* 不同于Win32环境，C#没有宏定义，所以我们需要区分 32 位系统和 64 位系统，分别调用对应的 SetWindowLong 方法 */

        /// <summary>
        /// 用在 32 位的情况下，64 位不能使用该函数，要用 SetWindowLongPtr 代替。
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="nIndex"></param>
        /// <param name="dwNewLong"></param>
        /// <returns></returns>
        /// /// <remarks>
        /// setWindowLong 的返回值 LONG_PTR 是定义好的类型，在32位下是32位，在64位下是64位，但是在.net中，唯一可以这样的
        /// 就是 IntPtr 了，所以使用 IntPtr。
        /// </remarks>
        [DllImport(User32, CharSet = CharSet.Auto, EntryPoint = "SetWindowLong")]
        private static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        /// <summary>
        /// 更改指定窗口的属性。函数还在额外的窗口内存中指定偏移量时设置一个值。
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="nIndex"></param>
        /// <param name="dwNewLong"></param>
        /// <returns></returns>
        /// <remarks>
        /// setWindowLong 的返回值 LONG_PTR 是定义好的类型，在32位下是32位，在64位下是64位，但是在.net中，唯一可以这样的
        /// 就是 IntPtr 了，所以使用 IntPtr。
        /// </remarks>
        [DllImport(User32, CharSet = CharSet.Auto, EntryPoint = "SetWindowLongPtrW")]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        public static IntPtr SetWindowLong(IntPtr hWnd, GWLIndex nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)                                       // 64 位
                return SetWindowLongPtr(hWnd, (int)nIndex, dwNewLong);
            else                                                        // 32 位
                return SetWindowLong(hWnd, (int)nIndex, dwNewLong);
        }
        #endregion

        /*---------------假装分割线------------------*/
        #region SendMessend
        /// <summary>
        /// 将指定的消息发送到一个或多个窗口。 SendMessage函数调用指定窗口的窗口过程，直到该窗口过程处理完该消息后才返回。
        /// 
        /// 要发送消息并立即返回，请使用SendMessageCallback或SendNotifyMessage函数。 
        /// 要将消息发布到线程的消息队列中并立即返回，请使用PostMessage或PostThreadMessage函数。
        /// </summary>
        /// <param name="hWnd">窗口的句柄，其窗口过程将接收到该消息。 如果此参数为HWND_BROADCAST（（HWND）0xffff），则消息将发送到系统中的所有顶级窗口，包括禁用或不可见的无主窗口，重叠的窗口以及弹出窗口； 但是消息不会发送到子窗口。</param>
        /// <param name="nMsg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport(User32, CharSet = CharSet.Auto, EntryPoint = "SendMessageW", SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        #endregion

        /*---------------假装分割线------------------*/
        #region GetWindowRect
        [DllImport(User32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);
        #endregion

        #region SetWindowPos
        /// <summary>
        /// 更改子窗口，弹出窗口或顶级窗口的大小，位置和Z顺序。 这些窗口是根据其在屏幕上的外观排序的。 最顶部的窗口获得最高排名，并且是Z顺序中的第一个窗口。
        /// </summary>
        /// <param name="hWnd">窗口的句柄。</param>
        /// <param name="hWndInsertAfter">在Z顺序中位于定位的窗口之前的窗口的句柄。 此参数必须是窗口句柄或<see cref="WINDOWPOS"/>以下值之一。</param>
        /// <param name="x">窗口左侧的新位置，以客户坐标表示。</param>
        /// <param name="y">窗口顶部的新位置，以客户坐标表示。</param>
        /// <param name="cx">窗口的新宽度，以像素为单位。</param>
        /// <param name="cy">窗口的新高度，以像素为单位。</param>
        /// <param name="uFlags">窗口大小和位置标志。 此参数可以是以下值的组合。</param>
        /// <returns></returns>
        [DllImport(User32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int x,
            int y,
            int cx,
            int cy,
            SWPFlags uFlags);
        #endregion

        /*---------------假装分割线------------------*/
        #region GetDC
        /// <summary>
        /// 获取指定窗口句柄的客户区域DC或者整个屏幕的DC。
        /// </summary>
        /// <param name="ptr">需要获取DC的窗口的句柄，如果是NULL，就是获取整个屏幕的DC</param>
        /// <returns></returns>
        [DllImport(User32, CharSet = CharSet.Auto)]
        public static extern IntPtr GetDC(IntPtr ptr);
        #endregion

        #region GetWindowDC
        /// <summary>
        /// 获取整个窗口的DC，包括标题栏等
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        [DllImport(User32, SetLastError = true)]
        public static extern IntPtr GetWindowDC(IntPtr window);
        #endregion

        #region ReleaseDC
        /// <summary>
        /// 释放“设备上下文（DC）”，和DeleteDC不同的是，releaseDC并不一定会删除DC资源。
        /// </summary>
        /// <param name="hWnd">需要释放的 DC 的窗口句柄</param>
        /// <param name="dc">需要释放的 DC </param>
        /// <returns>如果DC被释放，返回1；如果没有被释放，返回0</returns>
        /// <remarks>
        /// 注意：GetDC 获得的 DC 不能使用 DeleteDC，必须使用 ReleaseDC。GetWindowDC 也是。
        /// 
        /// 总结：获取 DC 的函数是 Get 开始的，就用 ReleaseDC 释放；获取 DC 的函数是 Create 开始的，就用 DeleteDC 删除。
        /// Get 和 Release 需要在相同的线程中。
        /// </remarks>
        [DllImport(User32, SetLastError = true)]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr dc);
        #endregion

        /*---------------假装分割线------------------*/
        #region UpdateLayeredWindow
        /// <summary>
        /// https://docs.microsoft.com/zh-cn/windows/win32/api/winuser/nf-winuser-updatelayeredwindow?f1url=https%3A%2F%2Fmsdn.microsoft.com%2Fquery%2Fdev16.query%3FappId%3DDev16IDEF1%26l%3DZH-CN%26k%3Dk(WINUSER%2FUpdateLayeredWindow)%3Bk(UpdateLayeredWindow)%3Bk(DevLang-C%2B%2B)%3Bk(TargetOS-Windows)%26rd%3Dtrue
        /// 更新分层窗口的位置，大小，形状，内容和半透明。
        /// </summary>
        /// <param name="hwnd">分层窗口的句柄。 使用CreateWindowEx函数创建窗口时，可以通过指定WS_EX_LAYERED来创建分层窗口。</param>
        /// <param name="hdcDest">屏幕DC的句柄。 通过在调用函数时指定NULL可获得此句柄。 当窗口内容更新时，它用于调色板颜色匹配。 如果hdcDst为NULL，将使用默认调色板。如果hdcSrc为NULL，则hdcDst必须为NULL。</param>
        /// <param name="pptDest">指向指定分层窗口的新屏幕位置的结构的指针。 如果当前位置未更改，则pptDst可以为NULL。</param>
        /// <param name="psize">指向指定分层窗口新大小的结构的指针。 如果窗口的大小未更改，则psize可以为NULL。 如果hdcSrc为NULL，则psize必须为NULL。</param>
        /// <param name="hdcSrc">DC的句柄，用于定义分层窗口的表面。 可以通过调用CreateCompatibleDC函数获得此句柄。 如果窗口的形状和视觉上下文未更改，则hdcSrc可以为NULL。</param>
        /// <param name="pptSrc">指向指定设备上下文中层位置的结构的指针。 如果hdcSrc为NULL，则pptSrc应该为NULL。</param>
        /// <param name="crKey">一个结构，用于指定在组成分层窗口时要使用的颜色键。 要生成COLORREF，请使用RGB宏。</param>
        /// <param name="pblend"></param>
        /// <param name="dwFlags">此参数可以是下列值之一。</param>
        /// <returns></returns>
        [DllImport(User32, CharSet = CharSet.Auto, EntryPoint = "UpdateLayeredWindow")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UpdateLayeredWindow(
            IntPtr hwnd,
            IntPtr hdcDest,
            ref POINT pptDest,
            ref SIZE psize,
            IntPtr hdcSrc,
            ref POINT pptSrc,
            uint crKey,
            [In] ref BLENDFUNCTION pblend,
            uint dwFlags);
        #endregion

        #region RedrawWindow
        /// <summary>
        /// RedrawWindow函数更新窗口的工作区中的指定矩形或区域。
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="lprcUpdate"></param>
        /// <param name="hrgnUpdate"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        [DllImport(User32)]
        [return:MarshalAs(UnmanagedType.Bool)]
        public static extern bool RedrawWindow(
            IntPtr hWnd,
            IntPtr lprcUpdate,
            IntPtr hrgnUpdate,
            RedrawWindowFlags flags);
        #endregion


        #region EnumThreadWindows
        /// <summary>
        /// 通过将句柄传递给每个窗口，依次传递给应用程序定义的回调函数，可以枚举与线程关联的所有非子窗口。 
        /// EnumThreadWindows继续，直到枚举最后一个窗口或回调函数返回FALSE。 
        /// 要枚举特定窗口的子窗口，请使用 EnumChildWindows 函数。
        /// </summary>
        /// <param name="dwThreadID">要枚举其窗口的线程的标识符。</param>
        /// <param name="lpfn">指向应用程序定义的回调函数的指针。 有关更多信息，请参见<see cref="EnumThreadWndProc"/>。</param>
        /// <param name="lParam">应用程序定义的值，将传递给回调函数。</param>
        /// <returns>
        ///  如果回调函数对 dwThreadId 指定的线程中的所有窗口都返回 TRUE，则返回值为TRUE。 
        ///  如果回调函数在任何枚举窗口上返回 FALSE，或者在 dwThreadId 指定的线程中未找到任何窗口，则返回值为 FALSE。
        /// </returns>
        [DllImport(User32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumThreadWindows(
            int dwThreadID,
            EnumThreadWndProc lpfn,
            IntPtr lParam);
        #endregion
        #endregion


        internal static DefWndProc DefWndProc = DefWindowProc;
    }
}
