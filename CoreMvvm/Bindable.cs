namespace Lyt.CoreMvvm
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Windows;


    public class Bindable : NotifyPropertyChanged
    {
        protected readonly Dictionary<string, object> properties;

        protected FrameworkElement frameworkElement;

        public Bindable() => this.properties = new Dictionary<string, object>();

        public void Bind(FrameworkElement frameworkElement)
        {
            if (frameworkElement != null)
            {
                this.frameworkElement = frameworkElement;
                this.OnDataBinding();
                if (this.frameworkElement.DataContext == null)
                {
                    this.frameworkElement.DataContext = this;
                }
            }
        }

        public virtual void OnDataBinding() { }

        /// <summary> Gets the value of a property </summary>
        protected T Get<T>([CallerMemberName] string name = null)
        {
            Debug.Assert(name != null, "name != null");
            return this.properties.TryGetValue(name, out object value) ? value == null ? default : (T)value : default;
        }

        /// <summary> Sets the value of a property </summary>
        protected void Set<T>(T value, [CallerMemberName] string name = null)
        {
            Debug.Assert(name != null, "name != null");
            if (Equals(value, this.Get<T>(name)))
            {
                return;
            }

            this.properties[name] = value;
            this.OnPropertyChanged(name);
        }
    }

    public class Bindable<TControl> : Bindable where TControl : FrameworkElement, new () 
    {
        public Bindable(TControl frameworkElement = null) => this.Bind(frameworkElement);

        public TControl View => this.frameworkElement as TControl;
    }

    public static class Binder<TControl, TBindable> 
        where TControl : FrameworkElement, new()
        where TBindable : Bindable<TControl>, new() 
    {
        public static TBindable Create()
        {
            var control = new TControl();
            var bindable = new TBindable();
            bindable.Bind(control);
            return bindable;
        }
    }
}
