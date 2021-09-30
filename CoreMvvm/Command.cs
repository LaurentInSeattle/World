namespace Lyt.CoreMvvm
{
    using System;
    using System.Windows.Input;

    public class Command : ICommand
    {
        private readonly Predicate<object> canExecute;
        private readonly Action<object> execute;

        public Command(Predicate<object> canExecute, Action<object> execute)
        {
            this.canExecute = canExecute;
            this.execute = execute;
        }

        public Command(Action<object> execute)
        {
            this.canExecute = (o) => true;
            this.execute = execute;
        }

#pragma warning disable 0067 // Never used 
        public event EventHandler CanExecuteChanged; // For interface compliance 
#pragma warning restore 0067

        public bool CanExecute(object parameter) => this.canExecute == null || this.canExecute(parameter);

        public void Execute(object parameter) => this.execute?.Invoke(parameter);
    }
}
