namespace Lyt.CoreMvvm
{
    using System;

    public sealed class WeakAction<T> : WeakAction where T : class
    {
        /// <summary> Initializes a new instance of the <see cref="WeakAction" /> class. </summary>
        /// <param name="action">The action that will be associated to this instance.</param>
        public WeakAction(Action<T> action) : this(action?.Target, action) { }

        /// <summary> Initializes a new instance of the <see cref="WeakAction" /> class. </summary>
        /// <param name="target">The action's owner.</param>
        /// <param name="action">The action that will be associated to this instance.</param>
        public WeakAction(object target, Action<T> action) : base()
        {
            if (action.Method.IsStatic)
            {
                throw new NotSupportedException("no static actions");
            }

            this.Method = action.Method;
            this.ActionReference = new WeakReference(action.Target);
            this.Reference = new WeakReference(target);
        }

        /// <summary> Executes the action. This only happens if the action's owner is still alive.</summary>
        public new void Execute() => this.Execute(default(T));

        /// <summary> Executes the action. This only happens if the action's owner is still alive.</summary>
        public void Execute(T parameter)
        {
            object actionTarget = this.ActionTarget;
            if (this.IsAlive)
            {
                if ((this.Method != null) && (this.ActionReference != null) && (actionTarget != null))
                {
                    try
                    {
                        this.Method.Invoke(
                            actionTarget,
                            new object[]
                            {
                                parameter
                            });
                    }
                    catch
                    {
                        // do nothing. This can sometimes blow up if everything's not loaded yet.
                    }
                }
            }
        }
    }
}
