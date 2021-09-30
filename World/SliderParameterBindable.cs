namespace Lyt.World
{
    using Lyt.CoreMvvm;
    using Lyt.CoreMvvm.Extensions;
    using Lyt.World.Model;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Input;


    public sealed class SliderParameterBindable : Bindable<SliderParameterControl>
    {
        private const string floatFormat = "{0:F2}";
        private const string intFormat = "{0:D}";

        private Parameter parameter;

        public SliderParameterBindable(Parameter parameter) => this.parameter = parameter;

        public override void OnDataBinding()
        {
            this.Name = this.parameter.Name;
            this.Minimum = this.parameter.Min;
            this.Min = 
                this.parameter.Format == Format.Integer ?
                    string.Format(intFormat, (int)this.Minimum) :
                    string.Format(floatFormat, this.Minimum);
            this.Maximum = this.parameter.Max;
            this.Max =
                this.parameter.Format == Format.Integer ?
                    string.Format(intFormat, (int) this.Maximum):
                    string.Format(floatFormat, this.Maximum);
            this.Frequency = this.parameter.Step;
            this.SmallChange = this.parameter.Step;
            this.LargeChange = this.parameter.Step * 2.0;
            this.Value = this.parameter.CurrentValue;
            this.Current = 
                this.parameter.Format == Format.Integer ?
                    string.Format(intFormat, (int)this.Value) :
                    string.Format(floatFormat, this.Value);
        }

        #region Bound Properties

        /// <summary> Gets or sets the Name property.</summary>
        public string Name { get => this.Get<string>(); set => this.Set(value); }

        /// <summary> Gets or sets the Min property.</summary>
        public string Min { get => this.Get<string>(); set => this.Set(value); }

        /// <summary> Gets or sets the Max property.</summary>
        public string Max { get => this.Get<string>(); set => this.Set(value); }

        /// <summary> Gets or sets the Current property.</summary>
        public string Current { get => this.Get<string>(); set => this.Set(value); }

        /// <summary> Gets or sets the Minimum property.</summary>
        public double Minimum { get => this.Get<double>(); set => this.Set(value); }

        /// <summary> Gets or sets the Maximum property.</summary>
        public double Maximum { get => this.Get<double>(); set => this.Set(value); }

        /// <summary> Gets or sets the Frequency property.</summary>
        public double Frequency { get => this.Get<double>(); set => this.Set(value); }

        /// <summary> Gets or sets the SmallChange property.</summary>
        public double SmallChange { get => this.Get<double>(); set => this.Set(value); }

        /// <summary> Gets or sets the LargeChange property.</summary>
        public double LargeChange { get => this.Get<double>(); set => this.Set(value); }

        /// <summary> Gets or sets the Value property.</summary>
        public double Value
        {
            get => this.Get<double>();
            set
            {
                this.Set(value);
                this.parameter.EditedValue = value; 
                this.Current =
                    this.parameter.Format == Format.Integer ?
                        string.Format(intFormat, (int)value) :
                        string.Format(floatFormat, value);
            }
        }
    
        #endregion Bound Properties
    }
}
