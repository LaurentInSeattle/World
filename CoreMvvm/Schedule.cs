namespace Lyt.CoreMvvm
{
    using System;
    using System.Threading.Tasks;
    using System.Windows.Threading;

    public static class Schedule
    {
        public static void OnUiThread(int delay, Action action, DispatcherPriority priority = DispatcherPriority.Normal)
            => Task.Run(async () =>
            {
                await Task.Delay(delay);
                _ = Dispatch.Dispatcher.BeginInvoke(priority, action);
            });

        public static void OnUiThread<TArgs>(
            int delay, Action<TArgs> action, TArgs args, DispatcherPriority priority = DispatcherPriority.Normal)
            => Task.Run(async () =>
               {
                   await Task.Delay(delay);
                   _ = Dispatch.Dispatcher.BeginInvoke(priority, (Action)delegate { action(args); });
               });
    }
}