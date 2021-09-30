namespace Lyt.CoreMvvm
{
    using System.Windows;
    using System.Windows.Controls;

    public class BindableControl : UserControl
    {
        public BindableControl() => this.DataContextChanged += OnDataContextChanged;

        protected void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext is Bindable bindable)
            {
                bindable.Bind(this);
            }
        }
    }
}
