namespace Lyt.CoreMvvm
{
    public sealed class Toast
    {
        public enum ToastEvent
        {
            Show,
            Dismiss,
        }

        public enum Severity
        {
            None,
            Info,
            Warning,
            Error,
        }

        public static void Show(Severity severity, string message, int duration = 12)
            => Messenger.Instance.Send(new Toast(Toast.ToastEvent.Show, severity, message, duration));

        public static void Dismiss(object dataContext)
            => Messenger.Instance.Send(new Toast(Toast.ToastEvent.Dismiss) { DataContext = dataContext });

        private Toast(ToastEvent evt) : this(evt, Severity.None, string.Empty, 0) { }

        private Toast(ToastEvent evt, Severity severity, string message, int duration)
        {
            this.Event = evt;
            this.Level = severity;
            this.Message = message;
            this.Duration = duration;
        }

        public ToastEvent Event { get; private set; }

        public Severity Level { get; private set; }

        public string Message { get; private set; }

        public int Duration { get; private set; }

        public object DataContext { get; private set; }
    }
}