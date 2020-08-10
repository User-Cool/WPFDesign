using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace CustomScrollBarTest
{
    /// <summary>
    /// 利用附加属性，与 ScrollViewer 的事件建立绑定。绑定在 <see cref="ScrollAnimationBehavior.OnIsEnabledChanged(DependencyObject, DependencyPropertyChangedEventArgs)"/> 下进行。
    /// </summary>
    class ScrollAnimationBehavior
    {
        private static ScrollViewer _scrollViewer = new ScrollViewer();


        #region 附加属性

        #region VerticalOffset

        public static double GetVerticalOffset(DependencyObject obj)
        {
            return (double)obj.GetValue(VerticalOffsetProperty);
        }

        public static void SetVerticalOffset(DependencyObject obj, double value)
        {
            obj.SetValue(VerticalOffsetProperty, value);
        }

        public static readonly DependencyProperty VerticalOffsetProperty =
            DependencyProperty.RegisterAttached("VerticalOffset", typeof(double), typeof(ScrollAnimationBehavior), new PropertyMetadata(0.0, OnVerticalOffsetChanged));

        
        private static void OnVerticalOffsetChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            if (target is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToVerticalOffset((double)args.NewValue);
            }
        }

        #endregion

        #region 持续时间

        public static TimeSpan GetTimeDuration(DependencyObject obj)
        {
            return (TimeSpan)obj.GetValue(TimeDurationProperty);
        }

        public static void SetTimeDuration(DependencyObject obj, TimeSpan value)
        {
            obj.SetValue(TimeDurationProperty, value);
        }

        // Using a DependencyProperty as the backing store for TimeDuration.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeDurationProperty =
            DependencyProperty.RegisterAttached("TimeDuration", typeof(TimeSpan), typeof(ScrollAnimationBehavior), new PropertyMetadata(new TimeSpan(0, 0, 0, 0, 0)));

        #endregion

        #region PointsToScroll

        public static double GetPointsToScroll(DependencyObject obj)
        {
            return (double)obj.GetValue(PointsToScrollProperty);
        }

        public static void SetPointsToScroll(DependencyObject obj, double value)
        {
            obj.SetValue(PointsToScrollProperty, value);
        }

        // Using a DependencyProperty as the backing store for PointsToScroll.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PointsToScrollProperty =
            DependencyProperty.RegisterAttached("PointsToScroll", typeof(double), typeof(ScrollAnimationBehavior), new PropertyMetadata(0.0));

        #endregion

        #region IsEnabled Property

        public static bool GetIsEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEnabledProperty, value);
        }

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(ScrollAnimationBehavior), new PropertyMetadata(false, OnIsEnabledChanged));

        /// <summary>
        /// 当 IsEnabled 发生变化的时候调用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <remarks>
        /// 在该方法下，与 ScrollViewer 的事件建立绑定。原理和 Behavior 框架相似，属于精简版的。
        /// </remarks>
        private static void OnIsEnabledChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var target = sender;

            if (target != null && target is ScrollViewer)
            {
                ScrollViewer scroller = target as ScrollViewer;
                scroller.Loaded += new RoutedEventHandler(ScrollViewerLoaded);
            }

            if (target != null && target is ListBox)
            {
                ListBox listbox = target as ListBox;
                //listbox.Loaded += new RoutedEventHandler(listboxLoaded);
            }
        }

        #endregion

        #endregion

        /// <summary>
        /// 动画显示实现。
        /// </summary>
        /// <param name="scrollViewer"></param>
        /// <param name="ToValue"></param>
        private static void AnimateScroll(ScrollViewer scrollViewer, double ToValue)
        {
            DoubleAnimation verticalAnimation = new DoubleAnimation
            {
                From = scrollViewer.VerticalOffset,
                To = ToValue,
                Duration = new Duration(GetTimeDuration(scrollViewer))
            };

            Storyboard storyboard = new Storyboard();

            storyboard.Children.Add(verticalAnimation);
            Storyboard.SetTarget(verticalAnimation, scrollViewer);
            Storyboard.SetTargetProperty(verticalAnimation, new PropertyPath(ScrollAnimationBehavior.VerticalOffsetProperty));
            storyboard.Begin();
        }

        private static double NormalizeScrollPos(ScrollViewer scroll, double scrollChange, Orientation o)
        {
            double returnValue = scrollChange;

            if (scrollChange < 0)
            {
                returnValue = 0;
            }

            if (o == Orientation.Vertical && scrollChange > scroll.ScrollableHeight)
            {
                returnValue = scroll.ScrollableHeight;
            }
            else if (o == Orientation.Horizontal && scrollChange > scroll.ScrollableWidth)
            {
                returnValue = scroll.ScrollableWidth;
            }

            return returnValue;
        }

        private static void SetEventHandlersForScrollViewer(ScrollViewer scroller)
        {
            scroller.PreviewMouseWheel += new MouseWheelEventHandler(ScrollViewerPreviewMouseWheel);
            scroller.PreviewKeyDown += new KeyEventHandler(ScrollViewerPreviewKeyDown);
        }


        private static void ScrollViewerPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double mouseWheelChange = (double)e.Delta;
            ScrollViewer scroller = (ScrollViewer)sender;
            double newVOffset = GetVerticalOffset(scroller) - (mouseWheelChange / 3);

            if (newVOffset < 0)
            {
                AnimateScroll(scroller, 0);
            }
            else if (newVOffset > scroller.ScrollableHeight)
            {
                AnimateScroll(scroller, scroller.ScrollableHeight);
            }
            else
            {
                AnimateScroll(scroller, newVOffset);
            }

            e.Handled = true;
        }

        private static void ScrollViewerPreviewKeyDown(object sender, KeyEventArgs e)
        {
            ScrollViewer scroller = (ScrollViewer)sender;

            Key keyPressed = e.Key;
            double newVerticalPos = GetVerticalOffset(scroller);
            bool isKeyHandled = false;

            if (keyPressed == Key.Down)
            {
                newVerticalPos = NormalizeScrollPos(scroller, (newVerticalPos + GetPointsToScroll(scroller)), Orientation.Vertical);
                isKeyHandled = true;
            }
            else if (keyPressed == Key.PageDown)
            {
                newVerticalPos = NormalizeScrollPos(scroller, (newVerticalPos + scroller.ViewportHeight), Orientation.Vertical);
                isKeyHandled = true;
            }
            else if (keyPressed == Key.Up)
            {
                newVerticalPos = NormalizeScrollPos(scroller, (newVerticalPos - GetPointsToScroll(scroller)), Orientation.Vertical);
                isKeyHandled = true;
            }
            else if (keyPressed == Key.PageUp)
            {
                newVerticalPos = NormalizeScrollPos(scroller, (newVerticalPos - scroller.ViewportHeight), Orientation.Vertical);
                isKeyHandled = true;
            }

            if (newVerticalPos != GetVerticalOffset(scroller))
            {
                AnimateScroll(scroller, newVerticalPos);
            }

            e.Handled = isKeyHandled;
        }


        #region Event Handle

        private static void ScrollViewerLoaded(object sender, RoutedEventArgs e)
        {
            ScrollViewer scroller = sender as ScrollViewer;

            SetEventHandlersForScrollViewer(scroller);
        }

        /*
        private static void listboxLoaded(object sender, RoutedEventArgs e)
        {
            ListBox listbox = sender as ListBox;

            _scrollViewer = FindVisualChildHelper.GetFirstChildOfType<ScrollViewer>(listbox);
            SetEventHandlersForScrollViewer(_scrollViewer);

            SetTimeDuration(_scrollViewer, new TimeSpan(0, 0, 0, 0, 200));
            SetPointsToScroll(_scrollViewer, 16.0);

            listbox.SelectionChanged += new SelectionChangedEventHandler(ListBoxSelectionChanged);
            listbox.Loaded += new RoutedEventHandler(ListBoxLoaded);
            listbox.LayoutUpdated += new EventHandler(ListBoxLayoutUpdated);
        }*/

        #endregion


        private static void ListBoxLayoutUpdated(object sender, EventArgs e)
        {
            UpdateScrollPosition(sender);
        }

        private static void ListBoxLoaded(object sender, RoutedEventArgs e)
        {
            UpdateScrollPosition(sender);
        }

        private static void ListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateScrollPosition(sender);
        }

        private static void UpdateScrollPosition(object sender)
        {
            ListBox listbox = sender as ListBox;

            if (listbox != null)
            {
                double scrollTo = 0;

                for (int i = 0; i < (listbox.SelectedIndex); i++)
                {
                    ListBoxItem tempItem = listbox.ItemContainerGenerator.ContainerFromItem(listbox.Items[i]) as ListBoxItem;

                    if (tempItem != null)
                    {
                        scrollTo += tempItem.ActualHeight;
                    }
                }

                AnimateScroll(_scrollViewer, scrollTo);
            }
        }
    }
}
