namespace Lyt.CoreMvvm
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Threading;

    public static class WpfExtensions
    {
        public static bool InDesignerMode()
            => Application.Current.MainWindow == null;

        public static void AssertDesignerMode()
        {
            if (!WpfExtensions.InDesignerMode())
            {
                throw new Exception("Not in design mode.");
            }
        }

        public static Visibility Visible(this bool flag)
            => flag ? Visibility.Visible : Visibility.Collapsed;

        public static Visibility Invisible(this bool flag)
            => !flag ? Visibility.Visible : Visibility.Collapsed;

        public static DispatcherTimer Start(this DispatcherTimer timer, int interval, EventHandler onTimerTick)
        {
            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }

            if (timer == null)
            {
                timer = new DispatcherTimer()
                {
                    Interval = TimeSpan.FromMilliseconds(interval),
                    IsEnabled = true,
                };

                timer.Tick += onTimerTick;
                timer.Start();
            }

            return timer;
        }

        public static void Stop(this DispatcherTimer timer, EventHandler onTimerTick)
        {
            if (timer != null)
            {
                timer.IsEnabled = false;
                timer.Tick -= onTimerTick;
                timer.Stop();
            }
        }

        public static void PreventMultipleClicks(this Button button)
        {
            button.IsEnabled = false;
            Dispatch.OnUiThread(
                async () =>
                {
                    await Task.Delay(200);
                    button.IsEnabled = true;
                },
                DispatcherPriority.Background);
        }

        public static string LoadTextResource(this string name, Assembly assembly)
        {
            if (assembly == null)
            {
                assembly = Assembly.GetExecutingAssembly();
            }

            string resourceName =
                assembly.GetManifestResourceNames().Single(str => str.EndsWith(name));
            if (string.IsNullOrWhiteSpace(resourceName))
            {
                return null;
            }

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static BitmapImage ToImageSource(this string name, string folder)
        {
            var uriSource = new Uri(string.Format("pack://application:,,,/{1}/{0}.png", name, folder));
            return uriSource.ToImageSource();
        }

        public static BitmapImage ToImageSource(this Uri uriSource)
        {
            try
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = uriSource;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (Debugger.IsAttached) { Debugger.Break(); }
                return null;
            }
        }

        public static BitmapImage ToImageSource(this byte[] bytes)
        {
            try
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream(bytes);
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (Debugger.IsAttached) { Debugger.Break(); }
                return null;
            }
        }

        public static UiType FindParent<UiType>(this DependencyObject uiElement)
            where UiType : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(uiElement);
            if (parent == null)
            {
                return null;
            }

            var parentAsUiType = parent as UiType;
            return parentAsUiType ?? parent.FindParent<UiType>();
        }

        public static T FindFirstVisualChild<T>(this DependencyObject dependencyObject)
            where T : DependencyObject
        {
            if (dependencyObject == null)
            {
                return null;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); ++i)
            {
                var child = VisualTreeHelper.GetChild(dependencyObject, i);
                var result = (child as T) ?? FindFirstVisualChild<T>(child);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject dependencyObject)
            where T : DependencyObject
        {
            if (dependencyObject != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); ++i)
                {
                    var child = VisualTreeHelper.GetChild(dependencyObject, i);
                    if (child != null && child is T t)
                    {
                        yield return t;
                    }

                    foreach (var childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}