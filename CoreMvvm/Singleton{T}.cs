namespace Lyt.CoreMvvm
{
    using System;
    using System.Diagnostics;
    using System.Reflection;

    public abstract class Singleton<TInstance> where TInstance : class
    {
        private static TInstance instance;

        public static TInstance Instance => Singleton<TInstance>.instance;

        static Singleton()
        {
            try
            {
                // Binding flags must include private constructors.
                var constructor = typeof(TInstance).GetConstructor(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    Type.EmptyTypes,
                    null);
                instance = (TInstance)constructor.Invoke(null);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
                if (Debugger.IsAttached) { Debugger.Break(); };
                throw;
            }
        }

        // In case, we need to be sure that the singleton is instanciated.
        public void Poke() { } 
    }
}