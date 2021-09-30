namespace Lyt.CoreMvvm
{
    using System;
    using System.Windows;
    using System.Windows.Threading;

    public static class Dispatch
    {
        private static readonly Dispatcher uiDispatcher = Application.Current.Dispatcher;

        public static Dispatcher Dispatcher => Dispatch.uiDispatcher;

        // Sadly Action cannot be used as an extension method type...
        public static void OnUiThread(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
            => Dispatch.uiDispatcher.BeginInvoke(priority, action);

        public static void OnUiThread<TArgs>(
            Action<TArgs> action, TArgs args, DispatcherPriority priority = DispatcherPriority.Normal)
            => Dispatch.uiDispatcher.BeginInvoke(priority, (Action)delegate { action(args); });
    }
}