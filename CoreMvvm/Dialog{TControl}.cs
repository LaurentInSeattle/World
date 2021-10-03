namespace Lyt.CoreMvvm
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;

    public partial class Dialog<TControl> : Window where TControl : UserControl, new()
    {
        public Dialog(
            string title,
            TControl control,
            DialogBindable<TControl> bindable,
            Dictionary<DependencyProperty, object> properties = null)
        {
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.SizeToContent = SizeToContent.WidthAndHeight;
            this.ResizeMode = ResizeMode.NoResize;
            this.ShowActivated = true;
            this.SnapsToDevicePixels = true;
            this.Title = title;
            this.WindowStyle = WindowStyle.SingleBorderWindow;
            if ((properties != null) && (properties.Count > 0))
            {
                this.SetupWindow(properties);
            }

            bindable.ParentDialog = this;
            bindable.Bind(control);
            this.Content = control;
        }

        private void SetupWindow(Dictionary<DependencyProperty, object> properties)
        {
            foreach (var property in properties)
            {
                try
                {
                    this.SetValue(property.Key, property.Value);
                }
                catch
                {
                    Debug.WriteLine("Failed to set " + property.Key.ToString());
                    if (Debugger.IsAttached) { Debugger.Break(); }
                }
            }
        }
    }
}
