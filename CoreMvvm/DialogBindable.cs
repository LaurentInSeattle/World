namespace Lyt.CoreMvvm
{
    using System.Windows.Controls;

    public class DialogBindable<TControl> : Bindable<TControl> where TControl : UserControl, new ()
    {
        public Dialog<TControl> ParentDialog { get; set; }

        public void Dismiss() => this.ParentDialog.DialogResult = false;

        public void Validate() => this.ParentDialog.DialogResult = true;
    }
}
