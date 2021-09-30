namespace Lyt.CoreMvvm
{
    using System.Windows.Controls;

    public sealed class ModalDialog
    {
        public enum ModalEvent
        {
            Dismiss,
            Run,
            Dismissed,
            RunBlocking,
            Update,
        }

        public static void Show(bool show, UserControl control)
        {
            if (show)
            {
                ModalDialog.Run(control);
            }
            else
            {
                ModalDialog.Dismiss();
            }
        }

        public static void Run(UserControl control)
            => Messenger.Instance.Send(new ModalDialog(ModalDialog.ModalEvent.Run, control));

        public static void RunBlocking(UserControl control)
            => Messenger.Instance.Send(new ModalDialog(ModalDialog.ModalEvent.RunBlocking, control));

        public static void Dismiss()
            => Messenger.Instance.Send(new ModalDialog(ModalDialog.ModalEvent.Dismiss));

        public ModalDialog(ModalEvent evt)
        {
            this.Event = evt;
            this.Control = null;
        }

        public ModalDialog(ModalEvent evt, UserControl control)
        {
            this.Event = evt;
            this.Control = control;
        }

        public ModalDialog(ModalEvent evt, string tag)
        {
            this.Event = evt;
            this.Tag = tag;
        }

        public ModalEvent Event { get; private set; }

        public UserControl Control { get; private set; }

        public string Tag { get; private set; }
    }
}