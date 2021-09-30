namespace Lyt.World.Model
{
    using Lyt.CoreMvvm;
    using Lyt.CoreMvvm.Extensions;

    using System;
    using System.Collections.Generic;
    using System.Linq;

    public sealed class Parameters : Singleton<Parameters>
    {
        Dictionary<string, Parameter> parameters;

        public void Create()
        {
            var parameterList = new List<Parameter>()
            {
                new Parameter("Simulation Duration", 220, 180, 420, 20, Widget.Slider, null, Format.Integer),
                new Parameter("Delta Time", 1.0, 0.2, 1.0, 0.1),
                new Parameter("Resources Multiplier", 1.0, 0.5, 2.5, 0.1),
                new Parameter("Output Consumed", 0.43, 0.37, 0.47, 0.01),
            };

            this.parameters = parameterList.ToDictionary(parameter => parameter.Name, parameter => parameter);
        }

        public Dictionary<string, Parameter> All => this.parameters; 

        public double Get(string parameterName) => this.FromName(parameterName).CurrentValue;

        public Parameter FromName(string parameterName)
        {
            if (this.parameters.TryGetValue(parameterName, out var parameter))
            {
                return parameter;
            }
            else
            {
                throw new Exception("Parameter not found: " + parameterName);
            }
        }

        public void ToDefaults()
            => this.parameters.Values.ForEach<Parameter>((parameter) => parameter.Default());

        public void CommitEdits()
            => this.parameters.Values.ForEach<Parameter>((parameter) => parameter.CommitEdits());

        public void CancelEdits()
            => this.parameters.Values.ForEach<Parameter>((parameter) => parameter.CancelEdits());
    }
}
