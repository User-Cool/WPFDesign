using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowCustomizeTest.Win32
{
    internal enum WindowsMessage : int
    {
        WM_NULL = 0x0000,
        WM_CREATE = 0x0001,
        WM_DESTROY = 0x0002,
        WM_MOVE = 0x0003,

        /// <summary>
        /// 更改大小后发送到窗口。
        /// wParam : 请求的调整大小类型。 此参数可以是下列值之一。
        /// lParam : lParam的低位字指定客户区的新宽度。lParam的高位字指定客户区的新高度。
        /// </summary>
        /// <remarks>
        /// DefWindowProc函数在处理WM_WINDOWPOSCHANGED消息时发送WM_SIZE和WM_MOVE消息。 
        /// 如果应用程序在不调用DefWindowProc的情况下处理WM_WINDOWPOSCHANGED消息，则不会发送WM_SIZE和WM_MOVE消息。
        /// </remarks>
        WM_SIZE = 0x0005,

        // 发送到激活的窗口和禁用的窗口。如果窗口使用相同的输入队列，则消息将同步发送，首先发送到停用的顶层窗口的窗口过程，然后发送到激活的顶层窗口的窗口过程。 如果窗口使用不同的输入队列，则消息将异步发送，因此将立即激活窗口。
        // wParam : 低位字指定窗口是处于激活状态还是处于禁用状态。 此参数可以是下列值之一。 高位字指定了激活或停用的窗口的最小化状态。 非零值表示窗口已最小化。
        // lParam : 根据wParam参数的值，激活或停用窗口的句柄。 如果wParam的低位字是WA_INACTIVE，则lParam是要激活的窗口的句柄。 如果wParam的低位字是WA_ACTIVE或WA_CLICKACTIVE，则lParam是要停用的窗口的句柄。 该句柄可以为NULL。
        // 返回值：如果应用程序处理此消息，则应返回 0。
        // 
        // remark: 如果窗口正在激活并且没有最小化，则DefWindowProc函数会将键盘焦点设置到窗口。 如果通过鼠标单击激活了窗口，则它还会收到WM_MOUSEACTIVATE消息。
        WM_ACTIVATE = 0x0006,
        WM_SETFOCUS = 0x0007,
        WM_KILLFOCUS = 0x0008,
        WM_ENABLE = 0x000A,
        WM_SETREDRAW = 0x000B,
        WM_SETTEXT = 0x000C,
        WM_GETTEXT = 0x000D,
        WM_GETTEXTLENGTH = 0x000E,
        WM_PAINT = 0x000F,
        WM_CLOSE = 0x0010,
        #region WCE
        WM_QUERYENDSESSION = 0x0011,
        WM_QUERYOPEN = 0x0013,
        WM_ENDSESSION = 0x0016,
        #endregion
        WM_QUIT = 0x0012,
        WM_ERASEBKGND = 0x0014,
        WM_SYSCOLORCHANGE = 0x0015,
        WM_SHOWWINDOW = 0x0018,
        WM_WININICHANGE = 0x001A,

        WM_DEVMODECHANGE = 0x001B,

        WM_ACTIVATEAPP = 0x001C,
        WM_FONTCHANGE = 0x001D,
        WM_TIMECHANGE = 0x001E,
        WM_CANCELMODE = 0x001F,
        WM_SETCURSOR = 0x0020,
        WM_MOUSEACTIVATE = 0x0021,
        WM_CHILDACTIVATE = 0x0022,
        WM_QUEUESYNC = 0x0023,


        // 当窗口的大小或位置即将更改时发送给窗口。应用程序可以使用此消息来覆盖窗口默认的最大大小和位置，或其默认的最小或最大跟踪大小。
        // wParam : 没有使用
        // lParam : 一个指向 MINMAXINFO 结构的指针，该结构包含默认的最大位置和尺寸，以及默认的最小和最大跟踪大小。应用程序可以通过设置该结构的成员来覆盖默认值。
        // 返回值：如果窗口过程处理了这个函数，返回 0。
        //
        // remark: "最大跟踪大小"是使用边框来调整窗口大小所能产生的最大窗口大小。"最小跟踪大小"是使用边界来调整窗口大小所能产生的最小窗口大小。
        WM_GETMINMAXINFO = 0x0024, // MINMAXINFO

        WM_PAINTICON = 0x0026,
        WM_ICONERASEBKGND = 0x0027,
        WM_NEXTDLGCTL = 0x0028,
        WM_SPOOLERSTATUS = 0x002A,
        WM_DRAWITEM = 0x002B,
        WM_MEASUREITEM = 0x002C,
        WM_DELETEITEM = 0x002D,
        WM_VKEYTOITEM = 0x002E,
        WM_CHARTOITEM = 0x002F,
        WM_SETFONT = 0x0030,
        WM_GETFONT = 0x0031,
        WM_SETHOTKEY = 0x0032,
        WM_GETHOTKEY = 0x0033,
        WM_QUERYDRAGICON = 0x0037,
        WM_COMPAREITEM = 0x0039,
        WM_GETOBJECT = 0x003D,
        WM_COMPACTING = 0x0041,
        /// WM_COMMNOTIFY = 0x0044, 不在支持

        // 由于调用SetWindowPos函数或另一个窗口管理函数而被发送到大小，位置或Z顺序要更改的窗口。
        // wParam : 没有使用
        // lParam : 指向WINDOWPOS结构的指针，该结构包含有关窗口的新大小和位置的信息。
        // 返回值：如果应用程序处理此消息，则应返回 0。
        // remark: 在处理此消息时，修改 WINDOWPOS 中的任何值都会影响窗口的新大小，位置或Z顺序。 应用程序可以通过设置或清除WINDOWPOS的flags成员中的适当位来防止更改窗口。
        WM_WINDOWPOSCHANGING = 0x0046,

        // 窗口的大小、位置或Z顺序中的位置由于调用SetWindowPos函数或另一个窗口管理函数而发生了变化。
        // wParam : 没有使用
        // lParam : 一个指向 WINDOWPOS 结构的指针，它包含有关窗口的新大小和位置的信息。
        // 返回值 : 如果窗口过程处理了这个函数，返回 0。
        // 
        // remark : 默认情况下，DefWindowProc函数发送WM_SIZE和WM_MOVE消息到窗口。如果应用程序不调用DefWindowProc处理WM_WINDOWPOSCHANGED消息，则WM_SIZE和WM_MOVE消息不会被发送。在WM_WINDOWPOSCHANGED消息期间执行任何移动或大小变化处理，而不调用DefWindowProc，效率更高。
        WM_WINDOWPOSCHANGED = 0x0047,

        WM_COMMAND = 0x0111,

        // 当用户从“窗口”菜单（以前称为系统或控制菜单）中选择命令时，或者当用户选择“最大化”按钮，“最小化”按钮，“还原”按钮或“关闭”按钮时，窗口就会收到此消息。
        // wParam : 请求的系统命令的类型。 此参数可以是下列值之一。
        // lParam : 如果使用鼠标选择了窗口菜单命令，则低位字以屏幕坐标指定光标的水平位置。 否则，将不使用此参数。如果使用鼠标选择了窗口菜单命令，则高阶单词将以屏幕坐标指定光标的垂直位置。 如果使用系统加速器选择命令，则此参数为1；如果使用助记符，则此参数为零。
        // return : 如果应用程序处理此消息，则应返回零。
        // 
        // https://docs.microsoft.com/en-us/windows/win32/menurc/wm-syscommand
        WM_SYSCOMMAND = 0x0112,

        // 显示分辨率更改后，WM_DISPLAYCHANGE消息将发送到所有窗口。
        // wParam : 显示器的新图像深度，以每像素位数为单位。
        // lParam : 低位字指定屏幕的水平分辨率。高位字指定屏幕的垂直分辨率。
        // 返回值：没有说怎么处理。
        // 
        // 此消息仅发送到顶级窗口。 对于所有其他窗口，将其过帐。
        WM_DISPLAYCHANGE = 0x007E,

        WM_SETICON = 0x0080,

        // 发送到窗口，以确定窗口的哪一部分对应于特定的屏幕坐标。例如，当光标移动时，当鼠标按钮被按下或释放时，或者在响应对一个函数(如WindowFromPoint)的调用时，就会发生这种情况。如果没有捕获鼠标，则消息将发送到光标下方的窗口。否则，消息将发送到捕获鼠标的窗口。
        // wParam : 没有使用
        // lParam : 低位字指定光标的x坐标。坐标相对于屏幕的左上角。高阶字指定光标的y坐标。坐标相对于屏幕的左上角。
        // 返回值 : DefWindowProc函数的返回值是 “定义 2”中 值之一，指示光标热点的位置。
        WM_NCHITTEST = 0x0084,

        WM_NCCREATE = 0x0081,
        WM_NCDESTROY = 0x0082,

        // 在必须计算窗口工作区的大小和位置时发送。 通过处理此消息，当窗口的大小或位置改变时，应用程序可以控制窗口的客户区域的内容。
        // wParam : 如果wParam为TRUE，则它指定应用程序应指示客户区的哪一部分包含有效信息。 系统将有效信息复制到新客户区域内的指定区域。
        //          如果wParam为FALSE，则应用程序无需指示客户区的有效部分。
        // lParam : 如果wParam为TRUE，则lParam指向NCCALCSIZE_PARAMS结构，该结构包含应用程序可用来计算客户端矩形的新大小和位置的信息。
        //          如果wParam为FALSE，则lParam指向RECT结构。 在输入时，结构包含建议的窗口矩形。 在退出时，该结构应包含相应窗口工作区的屏幕坐标。
        // 返回值：如果wParam参数为FALSE，则应用程序应返回零。
        //        如果wParam为TRUE，则应用程序应返回零或以下值的组合。
        //        如果wParam为TRUE，并且应用程序返回零，则旧的客户区将保留并与新客户区的左上角对齐。
        // https://docs.microsoft.com/zh-cn/windows/win32/winmsg/wm-nccalcsize?redirectedfrom=MSDN
        WM_NCCALCSIZE = 0x0083,

        // 当需要更改其非客户区以指示活动状态或非活动状态时，发送到窗口。
        // wParam : 指示何时需要更改标题栏或图标以指示活动状态或非活动状态。 
        //          如果要绘制活动的标题栏或图标，则wParam参数为TRUE。 如果要绘制无效的标题栏或图标，则wParam为FALSE。
        // lParam : 当此窗口的视觉样式处于活动状态时，不使用此参数。
        //          当该窗口的视觉样式无效时，此参数是该窗口非客户区域的可选更新区域的句柄。 如果此参数设置为-1，则DefWindowProc不会重新绘制非客户区以反映状态更改。
        // 返回值：当wParam参数为FALSE时，应用程序应返回TRUE以指示系统应继续默认处理，或者应返回FALSE以防止更改。 
        //        当wParam为TRUE时，返回值将被忽略。
        // 
        // 不建议处理与标准窗口的非客户区相关的消息，因为应用程序必须能够绘制该窗口非客户区的所有必需部分。 
        // 如果应用程序确实处理了此消息，则它必须返回TRUE以指示系统完成活动窗口的更改。 
        // 如果在收到此消息时窗口最小化，则应用程序应将消息传递给DefWindowProc函数。
        // 当wParam参数为TRUE时，DefWindowProc函数以其活动颜色绘制标题栏或图标标题，而在wParam为FALSE时以其非活动颜色绘制标题栏或图标标题。
        WM_NCACTIVATE = 0x0086,

        WM_NCMOUSEMOVE = 0x00A0,

        // 当光标在窗口的非客户区域内时，用户按下鼠标左键该消息被发送。 此消息将发布到包含鼠标光标的窗口中。 如果一个窗口捕获了鼠标，则不会发布此消息。
        // wParam : 窗口过程处理 WM_NCHITTEST 命中测试返回的命中结果。
        // lParam : 一个 POINTS 结构，其中包含光标的x和y坐标。 坐标是相对于屏幕的左上角的。
        // 返回值：如果程序处理了这个消息，返回0
        // 
        // DefWindowProc(默认的窗口消息处理函数)测试指定的点，去找到光标所在的位置，并且执行适当的动作。如果合适，DefWindowProc发送 WM_SYSCOMMAND 消息到窗口。
        // 通俗的说，就是  DefWindowProc 函数计算鼠标在窗口中所在的位置，并且执行一些适当的操作。如果合适，还会发送 WM_SYSCOMMAND 消息。
        WM_NCLBUTTONDOWN = 0x00A1,
        WM_NCLBUTTONUP = 0x00A2,
        WM_NCLBUTTONDBLCLK = 0x00A3,
        WM_NCRBUTTONDOWN = 0x00A4,
        WM_NCRBUTTONUP = 0x00A5,
        WM_NCRBUTTONDBLCLK = 0x00A6,

        // 鼠标中键
        WM_NCMBUTTONDOWN = 0x00A7,
        WM_NCMBUTTONUP = 0x00A8,
        WM_NCMBUTTONDBLCLK = 0x00A9,

        // 特殊鼠标的按键
        WM_NCXBUTTONDOWN = 0x00AB,
        WM_NCXBUTTONUP = 0x00AC,
        WM_NCXBUTTONDBLCLK = 0x00AD,

        WM_NCUAHDRAWCAPTION = 0x00AE, // 未文档化的消息，可能是古老的消息，不用处理，进行屏蔽就好
        WM_NCUAHDRAWFRAME = 0x00AF,

        // DPI
        WM_DPICHANGED = 0x02E0,
        WM_DPICHANGED_BEFOREPARENT = 0x02E2,
        WM_DPICHANGED_AFTERPARENT = 0x02E3
    }
}
