using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using WindowCustomizeTest.Interop;
using WindowCustomizeTest.Win32;

/// <summary>
/// 自定义窗口的不错的文章 https://www.cnblogs.com/fabler/archive/2014/08/17/3918088.html Winform的窗体美化心酸路
/// </summary>
/// 

/**
 * 问：（子类）为什么在创建窗口之后，要将窗口过程进行替换？ 
 * 答：这样可以在一定程度上提升效率，毕竟窗口过程是 switch 语句。当然，提升与否，看实际情况。
 * 
 * 问：<see cref="WindowCustomizeTest.GlowWindowCore.HwndWrapper.SubclassWndProc"/> 是否没有意义
 * 答：否，这样不仅可以保证窗口过程回到指定的方法上，还有上面提到的好处。
 * */
namespace WindowCustomizeTest.GlowWindowCore
{
    /// <summary>
    /// Win32 窗口的封装。
    /// </summary>
    /// 
    /// <remarks>
    /// 
    /// 设计模式：工厂方法。
    ///     
    /// 注意：
    ///     1、对象创建之后并没有创建 Win32 窗口。要想真正创建 Win32 窗口，需要调用 <see cref="EnsureHandle()"/> 方法
    ///     2、在创建窗口的时候，可以使用窗口类的名称（<see cref="WNDCLASS.lpszClassName"/>），也可以使用窗口类的 Atom，这个变量由窗口类注册函数返回
    ///     3、不要随意调用 <see cref="RegisterClass(string)"/>
    ///     4、<see cref="DestroyWindowCore()"/> 内存的自动释放不是在主线程（UI线程）里面的。
    ///     
    /// </remarks>
    /// 
    /// <example>
    /// 在使用上
    /// 
    ///  1、子类继承之后，设置 <see cref="IsWindowSubclassed"/> 为true；
    ///  2、如果子类想要自己实现窗口类的继承，需要自己配置窗口类，然后重写<see cref="CreateWindowClassCore"/> 注册自己的窗口类；
    ///  3、重写 <see cref="CreateWindowCore"/> 创建窗口类，通过调用 <see cref="WindowClassAtom"/> 注册窗口类；
    ///  4、调用 <see cref="EnsureHandle"/> 或者 <see cref="Handle"/> 创建对象。
    /// </example>
    internal abstract class HwndWrapper : DisposableObject
    {
        #region 属性
        private IntPtr _handle;          // 保存窗口句柄。
        private ushort _wndClassAtom;    // 保存窗口的类的 Atom 值。
        private WndProc _wndProc;
        private bool _isHandleCreationAllowed = true; // 是否允许创建窗口。1、在释放资源之后，不能在创建了，在创建过之后，也不能在创建了
        #endregion


        #region 公开的方法 (工厂方法)
        /// <summary>
        /// 入口1、获取窗口的句柄。
        /// </summary>
        /// <remarks></remarks>
        public IntPtr Handle
        {
            get
            {
                // 对句柄进行检查
                EnsureHandle();
                return _handle;
            }
        }

        /// <summary>
        /// 入口2、检查句柄。并在合适的情况下通过 Win32 Api 创建窗口。该方法可以用来创建win32窗口。
        /// </summary>
        public void EnsureHandle()
        {
            // 如果句柄是 nullptr，可能是没有创建，也可能是已经释放了。
            if (_handle == IntPtr.Zero)
            {
                if (_isHandleCreationAllowed == false) return;
                _isHandleCreationAllowed = false;
                _handle = CreateWindowCore();
                if (IsWindowSubclassed) SubclassWndProc(); // 为什么这么做？为了提高窗口过程的执行效率，毕竟是 Switch 语句，能减少判断，就减少判断
            }
        }
        #endregion


        #region 受保护的方法
        /// <summary>
        /// 一个标记，该类是不是子类。
        /// </summary>
        /// <remarks>
        /// 如果为 True，<see cref="EnsureHandle()"/> 会调用 <see cref="SubclassWndProc()"/>，
        /// 将窗口过程重新设置为 <see cref="WndProc"/>
        /// </remarks>
        protected virtual bool IsWindowSubclassed => false;

        #region 获取窗口类的 Atom
        /// <summary>
        /// 获取窗口类的 Atom。在创建创建时使用，用来创建窗口。
        /// </summary>
        protected ushort WindowClassAtom
        {
            get
            {
                if (_wndClassAtom == 0) _wndClassAtom = CreateWindowClassCore(); // 如果没有注册，进行注册。避免重复注册
                return _wndClassAtom;
            }
        }
        #endregion

        #region 创建窗口类（visual 可重写，用来实现自定义窗口类）
        /// <summary>
        /// 注册窗口类。这里提供了默认的。
        /// </summary>
        /// <returns> 窗口类的 Atom </returns>
        protected virtual ushort CreateWindowClassCore() =>
            RegisterClass(Guid.NewGuid().ToString()); // 类名好像关系不大，就是用来创建的
        #endregion

        #region 创建窗口(abstract 必须重写)
        /// <summary>
        /// 重写该方法，创建窗口。
        /// </summary>
        /// <returns> 创建窗口的过程 </returns>
        protected abstract IntPtr CreateWindowCore();
        #endregion

        #region 窗口过程（visual 可重写）
        /// <summary>
        /// 窗口过程。默认。
        /// </summary>
        protected virtual IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam) =>
            ApiUser32.DefWindowProc(hwnd, msg, wParam, lParam);
        #endregion

        #region 销毁窗口类（visual 可重写）
        /// <summary>
        /// 取消窗口类的注册，删除内存
        /// </summary>
        protected virtual void DestroyWindowClassCore()
        {
            if (_wndClassAtom != 0)
            {
                var moduleHandle = ApiKernel32.GetModuleHandle(null);
                ApiUser32.UnregisterClass(new IntPtr(_wndClassAtom), moduleHandle);
                _wndClassAtom = 0;
            }
        }
        #endregion

        #region 销毁窗口对象（visual 可重写）
        /// <summary>
        /// 销毁窗口，删除内存。
        /// </summary>
        protected virtual void DestroyWindowCore()
        {
            if (_handle != IntPtr.Zero)
            {
                bool result = false;

                /* 
                if (Application.Current != null)
                Application.Current.Dispatcher.Invoke(
                    new Action(() => { 
                        result = ApiUser32.DestroyWindow(_handle); }));
                */

                result = ApiUser32.DestroyWindow(_handle);
                _handle = IntPtr.Zero;
            }
        }
        #endregion

        #region 默认的窗口类注册方法
        /// <summary>
        /// 注册窗口类。默认的。
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        /// <remarks>
        /// 抽象的记录提供的默认的窗口注册方法。在我们的项目中，我们需要注册的是窗口是分层窗口，用来实现边框的
        /// </remarks>
        protected ushort RegisterClass(string className)
        {
            var wndClassEx = default(WNDCLASSEX);
            wndClassEx.cbSize = (uint)Marshal.SizeOf(typeof(WNDCLASSEX));
            wndClassEx.style = (uint)WndClassStyle.CS_DEFAULT; // 原来是 0u
            wndClassEx.cbClsExtra = 0; // 默认0就行
            wndClassEx.cbWndExtra = 0; // 同上
            wndClassEx.hInstance = ApiKernel32.GetModuleHandle(null); // 程序的句柄
            wndClassEx.hIcon = IntPtr.Zero;  // 默认 0 就行 
            wndClassEx.hCursor = IntPtr.Zero; // 默认 0 就行
            wndClassEx.hbrBackground = IntPtr.Zero; // 默认 0 就行
            wndClassEx.lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProc = new WndProc(WndProc));
            wndClassEx.lpszClassName = className;
            wndClassEx.lpszMenuName = null;

            return ApiUser32.RegisterClassEx(ref wndClassEx);
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
            _isHandleCreationAllowed = false;
            DestroyWindowCore();
            DestroyWindowClassCore();
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 重新设置窗口的窗口过程，保证窗口过程是 <see cref="WndProc(IntPtr, uint, IntPtr, IntPtr)"/>
        /// </summary>
        /// <remarks>
        /// 该函数并非无意义，我们定义 <see cref="WndProc(IntPtr, uint, IntPtr, IntPtr)"/> 为窗口过程，子类通过重写该过程实现
        /// 窗口消息的处理。但是，如果子类要自己定义窗口类，那么必然存在将窗口过程设置为其他的情况，为了保证正常，所有才会有这个方法；
        /// 另外，窗口过程是switch，如果switch的判断内容过多，会造成性能的下降，该方法也可以将创建将创建时的窗口过程和实际执行的窗口
        /// 过程进行分离，从而实现减少实际窗口过程的switch判断的目的。
        /// </remarks>
        private void SubclassWndProc()
        {
            _wndProc = new WndProc(WndProc);
            ApiUser32.SetWindowLong(
                _handle,
                GWLIndex.GWLP_WNDPROC,
                Marshal.GetFunctionPointerForDelegate(_wndProc));
        }
        #endregion
    }
}
