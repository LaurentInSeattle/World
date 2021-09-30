namespace Lyt.World.Model
{
    using Lyt.CoreMvvm.Extensions;

    using System.Collections.Generic;
    using System.Diagnostics;

    public class Equation : Value
    {
        public delegate double UpdateDelegate();

        private bool logData;

        public Equation(Model model, string name, int number, string units) : base(model, name, number, units)
        {
            this.Model.OnNewEquation(this);
        }

        public Equation(Model model, string name, int number) : base(model, name, number, "dimensionless")
        {
            this.Model.OnNewEquation(this);
        }

        public UpdateDelegate UpdateFunction { get; set; }

        public double J { get; set; }

        public double Minimum { get; private set; }

        public double Maximum { get; private set; }

        public double NormalizedValue => (this.K - this.Minimum) / (this.Maximum - this.Minimum);

        public double NormalizedLoggedValue(int index)
        {
            if ( this.Maximum == double.MinValue)
            {
                return 0.0;
            }

            if ( this.Minimum == double.MaxValue)
            {
                return 0.0;
            }

            if (Value.IsAlmostZero(this.Maximum - this.Minimum))
            {
                return 0.0;
            }

            return (this.Log[index] - this.Minimum) / (this.Maximum - this.Minimum);
        }

        public List<double> Log { get; set; }

        public void LogData()
        {
            this.logData = true;
            this.Log = new List<double>(512);
            this.Maximum = double.MinValue;
            this.Minimum = double.MaxValue;
        }

        public virtual void Reset()
        {
            this.logData = false;
            this.Log = null;
        }

        public virtual void Initialize() { }

        public virtual void Update()
        {
            if (this.UpdateFunction != null)
            {
                this.K = this.UpdateFunction.Invoke();
                this.CheckForNaNAndInfinity();
                if (this.logData)
                {
                    if (this.K > this.Maximum)
                    {
                        this.Maximum = this.K;
                    }

                    if (this.K < this.Minimum)
                    {
                        this.Minimum = this.K;
                    }
                }
            }
        }

        public virtual void Tick()
        {
            if (this.logData)
            {
                if (this.Log != null)
                {
                    this.Log.Add(this.K);
                }
                else
                {
                    Debug.WriteLine(this.FriendlyName + " has not logging support. ~ " + this.Name);
                    if (Debugger.IsAttached) { Debugger.Break(); }
                }
            }

            this.J = this.K;
        }
    }
}
