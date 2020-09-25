using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace WindowCustomizeTest
{
    /// <summary>
    /// Window的扩展方法。
    /// </summary>
    internal static class WindowExtension
    {
        /// <summary>
        /// 返回 WPF 窗口的 Win32 句柄。
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        internal static IntPtr GetHandle(this System.Windows.Window window) => new WindowInteropHelper(window).Handle;

        /// <summary>
        /// 返回窗口的 HwndSource 对象。
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        //internal static HwndSource GetHwndSource(this System.Windows.Window window) => HwndSource.FromHwnd(GetHandle(window));

        /* 本着学习的态度，下面是获取 HwndSource 的第二种方法 */
        internal static HwndSource GetHwndSource(this System.Windows.Window window) => 
            HwndSource.FromVisual(window) is HwndSource hwndSource ? hwndSource : null;
    }
}
