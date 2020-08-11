using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace CustomScrollBarTest
{
    public class VisualChildrenHelper
    {
        public static List<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            List<T> children = new List<T>();
            return FindVisualChildren(parent, children);
        }

        protected static List<T> FindVisualChildren<T>(DependencyObject parent, List<T> children) where T : DependencyObject
        {
            var visualCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < visualCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                var count = VisualTreeHelper.GetChildrenCount(child);
                if (count > 0)
                {
                    if (child is T t)
                        children.Add(t);

                    FindVisualChildren(child, children);
                }
            }

            return children;
        }
    }
}
