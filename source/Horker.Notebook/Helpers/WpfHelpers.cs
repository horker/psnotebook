using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Horker.Notebook.Helpers
{
    public static class WpfHelpers
    {
        // ref. https://stackoverflow.com/questions/1517743/in-wpf-how-can-i-determine-whether-a-control-is-visible-to-the-user

        public static bool IsUserVisible(this UIElement element, bool full)
        {
            if (!element.IsVisible)
                return false;

            var container = VisualTreeHelper.GetParent(element) as FrameworkElement;
            if (container == null)
                throw new ArgumentNullException("container");

            Rect bounds = element.TransformToAncestor(container).TransformBounds(new Rect(0.0, 0.0, element.RenderSize.Width, element.RenderSize.Height));
            Rect rect = new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);

            return full ? rect.Contains(bounds) : rect.IntersectsWith(bounds);
        }
    }
}
