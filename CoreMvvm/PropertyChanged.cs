namespace Lyt.CoreMvvm
{
    using System.ComponentModel;

    public class NotifyPropertyChanged : INotifyPropertyChanged
    {
        /// <summary> Occurs when a property value changes. </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary> Raises the property changed event. </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
