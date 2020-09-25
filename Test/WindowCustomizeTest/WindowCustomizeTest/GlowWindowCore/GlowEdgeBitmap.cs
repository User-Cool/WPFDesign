using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WindowCustomizeTest.Interop;
using WindowCustomizeTest.Win32;

/**
 * 计算机显示的内容，实际上是位图。也就是内容先渲染成位图，然后在来绘制（有的时候，位图渲染是由显卡来执行的）。
 * 
 * 要想自定义位图（BITMAP），需要以下内容：
 * BITMAPINFO，位图信息结构体，用来创建可以自定义的兼容位图 HBITMAP
 * CreateDIBSection，创建可以直接写入的位图
 * */
namespace WindowCustomizeTest.GlowWindowCore
{
    /// <summary>
    /// 创建作为边框的位图，供混合使用。
    /// 
    /// 位图利用图片（32位色真彩）设置 Alpha 通道，并且和设置的颜色一起形成最终的位图（自定义位图的数据）。
    /// 使用图片的原因是有 PhotoShop 等专业软件可以使用，效果更佳。
    /// </summary>
    /// <remarks>
    /// 利用 CreateDIBSection 创建可以写入数据的 BitMap（像素点可编辑），利用图片的alpha通道
    /// 和设置的颜色填写 BitMap 的数据区（void 指针），创建 BitMap。
    /// 
    /// 这里位图的内容来自设置的颜色，为了实现半透明的效果（边框的阴影）与 Alpha 通道进行混合，
    /// 形成了最终绘制需要的位图（设置颜色和带有Alpha通道的位图）。
    /// 
    /// 注意：
    ///     数据的顺序是：b、g、r、a 也就是 AARRGGBB，但是 Windows 是小端模式，所以需要反过来
    /// </remarks>
    internal class GlowEdgeBitmap : DisposableObject
    {
        #region 属性
        private const int GlowBitmapPartCount = 16;
        private const int BytesPerPixelBgra32 = 4;
        private static readonly CachedBitmapInfo[] _transparencyMasks = new CachedBitmapInfo[GlowBitmapPartCount];
        private readonly BITMAPINFO _bitmapInfo; // 边框位图结构体
        private readonly IntPtr _pbits;          // 边框位图数据
        #endregion


        private GlowEdgeBitmap(IntPtr screenDC, int width, int height)
        {
            _bitmapInfo.bmiHeader.biSize = Marshal.SizeOf(typeof(BITMAPINFOHEADER));
            _bitmapInfo.bmiHeader.biWidth = width;
            _bitmapInfo.bmiHeader.biHeight = -height;
            _bitmapInfo.bmiHeader.biPlanes = 1; // 必须是1
            _bitmapInfo.bmiHeader.biBitCount = 32;
            _bitmapInfo.bmiHeader.biCompression = BIC.BI_RGB;
            _bitmapInfo.bmiHeader.biSizeImage = 0; // 未压缩的 RGB
            _bitmapInfo.bmiHeader.biXPelsPerMeter = 0;
            _bitmapInfo.bmiHeader.biYPelsPerMeter = 0;
            _bitmapInfo.bmiHeader.biClrUsed = 0; /// <see cref="BITMAPINFO.bmiColors"/> 大小为0
            _bitmapInfo.bmiHeader.biClrImportant = 0;

            Handle = ApiGdi32.CreateDIBSection(
                screenDC,
                ref _bitmapInfo,
                0,
                out _pbits,
                IntPtr.Zero,
                0
                );
        }


        #region 公开的方法
        public IntPtr Handle { get; }
        public IntPtr DIBits => _pbits;
        public int Width => _bitmapInfo.bmiHeader.biWidth;
        public int Height => -_bitmapInfo.bmiHeader.biHeight;


        /// <summary>
        /// 工厂方法：用设定的颜色和位置创建一个 GlowBitmap 对象。
        /// </summary>
        /// <param name="drawingContext"></param>
        /// <param name="bitmapPart"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static GlowEdgeBitmap Create(GlowEdgeDrawingContext dc, GlowEdgeBitmapParts part, Color color)
        {
            CachedBitmapInfo mast = GetOrCreateAlphaMast(part);

            GlowEdgeBitmap glowEdgeBitmap = new GlowEdgeBitmap(dc.ScreenDC, mast.Width, mast.Height);

            // 创建数据
            for (int i = 0; i < mast.DIBits.Length; i += BytesPerPixelBgra32)
            {
                byte a = mast.DIBits[i + 3];
                byte r = PremultiplyAlpha(color.R, a);
                byte g = PremultiplyAlpha(color.G, a);
                byte b = PremultiplyAlpha(color.B, a);
                Marshal.WriteByte(glowEdgeBitmap.DIBits, i, b);
                Marshal.WriteByte(glowEdgeBitmap.DIBits, i + 1, g);
                Marshal.WriteByte(glowEdgeBitmap.DIBits, i + 2, r);
                Marshal.WriteByte(glowEdgeBitmap.DIBits, i + 3, a);
            }

            return glowEdgeBitmap;
        }
        #endregion


        #region 重写 DisposableObject 的方法
        /// <summary>
        /// 释放 HBITMAP。CreateXXX 创建的内容要是用 DeleteXXX 进行释放。
        /// </summary>
        protected override void DisposeNativeResources()
        {
            ApiGdi32.DeleteObject(Handle);
        }
        #endregion


        #region 私有的方法
        /// <summary>
        /// 读取图片的 Alpha 通道作为阴影的 Alpha 通道
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        private static CachedBitmapInfo GetOrCreateAlphaMast(GlowEdgeBitmapParts part)
        {
            int index = (int)part;

            if (_transparencyMasks[index] == null)
            {
                BitmapImage mastImage =
                    new BitmapImage(
                        new Uri($"pack://application:,,,/WindowCustomizeTest;Component/GlowWindowCore/Resource/{part}.png"/*, UriKind.Relative*/));

                byte[] pixels = new byte[BytesPerPixelBgra32 * mastImage.PixelWidth * mastImage.PixelHeight];
                int stride = BytesPerPixelBgra32 * mastImage.PixelWidth;
                mastImage.CopyPixels(pixels, stride, 0);
                mastImage.Freeze();

                _transparencyMasks[index] =
                    new CachedBitmapInfo(pixels, mastImage.PixelWidth, mastImage.PixelHeight);
            }

            return _transparencyMasks[index];
        }

        /// <summary>
        /// 对颜色通道进行 Alpha 混合计算。这里就是忽略原来的 Alpha 值，使用设定的 Alpha 值进行混合计算。
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="alpha"></param>
        /// <returns>返回与 Alpha 通道混合之后的颜色</returns>
        /// <remarks>
        /// 我们的目的是阴影边框，并且边框能够设置颜色。为了让阴影设计更加方便，我们采用带有Alpha通道的位图的方式，
        /// 利用位图的 Alpha 通道与我们设定的颜色进行混合计算，从而得到具有设计位图的alpha通道和我们定义的颜色的新的颜色。
        /// 
        /// 需要注意的是，我们这里得到的，在整体上，还是属于源。至于源和目标的混合，我们可以将源当做我们窗口，目标当做屏幕，我们的窗口
        /// 需要“贴”到屏幕上，如果我们的窗口是透明的，那么就需要屏幕的颜色与我们的颜色进行混合，从而得到最终颜色的目的。这就是
        /// 需要源和目标的原因。
        /// </remarks>
        private static byte PremultiplyAlpha(byte channel, byte alpha)
        {
            return (byte)(channel * alpha / 255.0); // alpha 混合计算公式。
        }
        #endregion



        /// <summary>
        /// 暂存来自图片的位（ARGB）数据。该图片被用来辅助创建边框的位图。主要使用该图片的 Alpha 通道。
        /// </summary>
        /// <remarks>
        /// DIBites：位图的像素点数据；
        /// Height：位图的高度；
        /// Width：位图的宽度；
        /// </remarks>
        private sealed class CachedBitmapInfo
        {
            internal readonly byte[] DIBits;
            internal readonly int Height;
            internal readonly int Width;

            internal CachedBitmapInfo(byte[] diBits, int width, int height)
            {
                Width = width;
                Height = height;
                DIBits = diBits;
            }
        }
    }
}
