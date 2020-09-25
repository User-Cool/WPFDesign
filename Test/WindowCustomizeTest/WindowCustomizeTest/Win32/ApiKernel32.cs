using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WindowCustomizeTest.Win32
{
    internal class ApiKernel32
    {
        private const string Kernel32 = "kernel32.dll";


        #region HMODULE GetModuleHandle(LPCSTR lpModuleName);
        /// <summary>
        /// 获取运行中模块的句柄。
        /// </summary>
        /// <param name="lpModuleName"></param>
        /// <returns></returns>
        [DllImport(Kernel32, CharSet = CharSet.Auto, SetLastError = true, EntryPoint = "GetModuleHandleW")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
        #endregion

        [DllImport(Kernel32)]
        public static extern uint GetLastError();
    }
}
