namespace Lyt.World.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    public sealed class Table : Auxiliary
    {
        public readonly double[] Data;
        public readonly double Min;
        public readonly double Max;
        public readonly double Delta;
        public readonly List<double> Indices;

        public Table(
            Simulator model, string name, int number, string units, double[] data, double min, double max, double delta)
            : base(model, name, number, units)
        {
            this.Data = data;
            this.Min = min;
            this.Max = max;
            this.Delta = delta;
            this.Indices = new List<double>();
            for (double i = this.Min; i <= this.Max; i += this.Delta)
            {
                this.Indices.Add(i);
            }
        }

        public Table(
            string name, string units, double[] data, double min, double max, double delta)
            : base(Simulator.Instance, name, 0, units)
        {
            this.Data = data;
            this.Min = min;
            this.Max = max;
            this.Delta = delta;
            this.Indices = new List<double>();
            for (double i = this.Min; i <= this.Max; i += this.Delta)
            {
                this.Indices.Add(i);
            }
        }

        public override void Update()
        {
            if (this.UpdateFunction != null)
            {
                double source = this.UpdateFunction.Invoke();
                this.CheckForNaNAndInfinityValue(source); 
                double value = this.Lookup(source);
                this.CheckRange(value); 

                this.K = value;
            }
        }

        private double Lookup(double source)
        {
            if (source <= this.Min)
            {
                return this.Data[0];
            }
            else if (source >= this.Max)
            {
                return this.Data[this.Data.Length - 1];
            }
            else
            {
                int j = 0;
                for (double i = this.Min; i <= this.Max; i += this.Delta, j++)
                {
                    if (i >= source)
                    {
                        double lowerVal = this.Data[j - 1];
                        double upperVal = this.Data[j];
                        double fraction = (source - (i - this.Delta)) / this.Delta;
                        this.CheckInterpolate(fraction, source);
                        return lowerVal + (fraction * (upperVal - lowerVal));
                    }
                }
            }

            throw new Exception("Table lookup failed to find a value: " + this.Name + " Index: " + source.ToString("D"));
        }

        [Conditional("DEBUG")]
        private void CheckRange(double value)
        {
            double first = this.Data[0];
            double last = this.Data[this.Data.Length - 1];
            double min = Math.Min(first, last);
            double max = Math.Max(first, last);
            if ((value < min) && (value > max))
            {
                throw new Exception("Table lookup out of range: " + this.Name + " Source: " + value.ToString("F"));
            }
        }

        [Conditional("DEBUG")]
        private void CheckInterpolate(double fraction, double source)
        {
            if ((fraction < 0.0) || (fraction > 1.0))
            {
                throw new Exception("Table lookup failed to interpolate: " + this.Name + " Source: " + source.ToString("D"));
            }
        }
    }
}
