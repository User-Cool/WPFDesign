using System.Windows;
using System.Windows.Media;

namespace CustomButtonTest.Assist
{
    public class ShadowEffectAssist
    {
        #region 颜色

        public static Color GetColor(DependencyObject obj)
        {
            return (Color)obj.GetValue(ColorProperty);
        }

        public static void SetColor(DependencyObject obj, Color value)
        {
            obj.SetValue(ColorProperty, value);
        }

        // Using a DependencyProperty as the backing store for Color.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.RegisterAttached("Color", typeof(Color), typeof(ShadowEffectAssist), new PropertyMetadata(new Color()));

        #endregion

        #region 深度
        public static double GetDepth(DependencyObject obj)
        {
            return (double)obj.GetValue(DepthProperty);
        }

        public static void SetDepth(DependencyObject obj, double value)
        {
            obj.SetValue(DepthProperty, value);
        }

        // Using a DependencyProperty as the backing store for Depth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DepthProperty =
            DependencyProperty.RegisterAttached("Depth", typeof(double), typeof(ShadowEffectAssist), new PropertyMetadata(4.0));
        #endregion

        #region 模糊

        public static double GetBlur(DependencyObject obj)
        {
            return (double)obj.GetValue(BlurProperty);
        }

        public static void SetBlur(DependencyObject obj, double value)
        {
            obj.SetValue(BlurProperty, value);
        }

        // Using a DependencyProperty as the backing store for Blur.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BlurProperty =
            DependencyProperty.RegisterAttached("Blur", typeof(double), typeof(ShadowEffectAssist), new PropertyMetadata(4.0));

        #endregion

        #region 透明度

        public static double GetOpacity(DependencyObject obj)
        {
            return (double)obj.GetValue(OpacityProperty);
        }

        public static void SetOpacity(DependencyObject obj, double value)
        {
            obj.SetValue(OpacityProperty, value);
        }

        // Using a DependencyProperty as the backing store for Opacity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OpacityProperty =
            DependencyProperty.RegisterAttached("Opacity", typeof(double), typeof(ShadowEffectAssist), new PropertyMetadata(0.41));

        #endregion

        #region 方向

        public static double GetDirection(DependencyObject obj)
        {
            return (double)obj.GetValue(DirectionProperty);
        }

        public static void SetDirection(DependencyObject obj, double value)
        {
            obj.SetValue(DirectionProperty, value);
        }

        // Using a DependencyProperty as the backing store for Direction.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DirectionProperty =
            DependencyProperty.RegisterAttached("Direction", typeof(double), typeof(ShadowEffectAssist), new PropertyMetadata(-90.0));

        #endregion

        #region Enable

        public static bool GetIsEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEnabledProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(ShadowEffectAssist), new PropertyMetadata(true));

        #endregion
    }
}
