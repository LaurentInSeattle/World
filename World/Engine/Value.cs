namespace Lyt.World.Engine
{
    using Lyt.CoreMvvm.Extensions;

    using System;
    using System.Diagnostics;

    public class Value
    {
        public readonly Simulator Simulator;

        public readonly string Name;

        public readonly int Number;

        public readonly string Units;

        private double k;

        public Value(Simulator model, string name, int number, string units)
        {
            this.Simulator = model;
            this.Name = name;
            this.Number = number;
            this.Units = units;
        }

        public double K
        {
            get { return this.k; }
            set
            {
                if (this.CannotBeZero)
                {
                    this.CheckForZero(value);
                }

                this.CheckForNaNAndInfinityValue(value);
                if (this.CannotBeNegative)
                {
                    this.CheckForNegative(value);
                }

                this.k = value;
            }
        }

        public bool CannotBeZero { get; set; }

        public bool CannotBeNegative { get; set; }

        public string Sector { get; set; }

        public string SubSector { get; set; }

        public string FriendlyName => string.Format("{0} ({1})", this.Name.Capitalize().Wordify(), this.Number);

        public string FriendlyUnits => this.Units.Capitalize().Wordify();

        [Conditional("DEBUG")]
        public void CheckForNaNAndInfinity()
        {
            if (double.IsNaN(this.k) || double.IsInfinity(this.k))
            {
                Debug.WriteLine(this.FriendlyName + " is 'NaN' or infinite. ~ " + this.Name);
                if (Debugger.IsAttached) { Debugger.Break(); }
            }
        }

        [Conditional("DEBUG")]
        public void CheckForNaNAndInfinity(string equationName )
        {
            var equation = this.Simulator.EquationFromName(equationName);
            if (double.IsNaN(k) || double.IsInfinity(k))
            {
                Debug.WriteLine(equation.FriendlyName + " is 'NaN' or infinite. ~ " + equation.Name);
                if (Debugger.IsAttached) { Debugger.Break(); }
            }
        }

        [Conditional("DEBUG")]
        public void CheckForNaNAndInfinityValue(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                Debug.WriteLine("Value is 'NaN' or infinite." );
                if (Debugger.IsAttached) { Debugger.Break(); }
            }
        }

        [Conditional("DEBUG")]
        public void CheckForNegative(string equationName)
        {
            var equation = this.Simulator.EquationFromName(equationName);
            if ((equation.K < 0.0) && (this.Simulator.TickCount > 1))
            {
                Debug.WriteLine(equation.FriendlyName + " is negative. ~ " + equation.Name);
                if (Debugger.IsAttached) { Debugger.Break(); }
            }
        }

        [Conditional("DEBUG")]
        public void CheckForNegative()
        {
            if ((this.k < 0.0) && (this.Simulator.TickCount > 1))
            {
                Debug.WriteLine(this.FriendlyName + " is negative. ~ " + this.Name);
                if (Debugger.IsAttached) { Debugger.Break(); }
            }
        }

        [Conditional("DEBUG")]
        public void CheckForNegative(double value)
        {
            if ((value < 0.0) && ( this.Simulator.TickCount > 1 ))
            {
                Debug.WriteLine("Value is negative. ");
                if (Debugger.IsAttached) { Debugger.Break(); }
            }
        }

        public static bool IsAlmostZero(double value)
        {
            const double epsilon = 0.000_000_000_1;
            return (Math.Abs(value) < epsilon) ; 
        }

        [Conditional("DEBUG")]
        public void CheckForZero(double value)
        {
            const double epsilon = 0.000_000_000_1; 
            if ((Math.Abs(value) < epsilon) && (this.Simulator.TickCount > 1))
            {
                Debug.WriteLine("Value is zero. ");
                if (Debugger.IsAttached) { Debugger.Break(); }
            }
        }
    }
}
