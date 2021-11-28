namespace Lyt.World.Model
{
    using Lyt.World.Engine;

    using System.Collections.Generic;

    #region MIT License 

    /* this code, by Laurent Yves Testud - under MIT Licence 


       MIT License

       Copyright (c) 2021 Laurent Yves Testud 

       Permission is hereby granted, free of charge, to any person obtaining a copy
       of this software and associated documentation files (the "Software"), to deal
       in the Software without restriction, including without limitation the rights
       to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
       copies of the Software, and to permit persons to whom the Software is
       furnished to do so, subject to the following conditions:

       The above copyright notice and this permission notice shall be included in all
       copies or substantial portions of the Software.

       THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
       IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
       FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
       AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
       LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
       OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
       SOFTWARE.
    */

    #endregion MIT License 

    public sealed partial class FluModel : Simulator
    {
        private Parameter[] parameters = new Parameter[]
        {
            new Parameter("Simulation Duration", 200, 150, 420, 10, Widget.Slider, null, Format.Integer),
            new Parameter("Delta Time", 1.0, 1.0, 2.0, 1, Widget.Slider, null, Format.Integer),
        };

        public FluModel() : base()
        {
            Simulator.Instance = this; 
            this.Parameters = new Parameters(parameters);
            this.Parameters.ToDefaults();
            this.CreateModel();
            base.FinalizeConstruction(this.dependencies, null);
        }

        public override bool SimulationEnded()
        {
            var duration = this.Parameters.FromName("Simulation Duration");
            return (this.Time > (int)duration.CurrentValue);
        }

        public override void Parametrize()
        {
        }

        public override List<PlotDefinition> Plots()
        {
            var list = new List<PlotDefinition>
            {
                new PlotDefinition("Population", PlotKind.Absolute, new List<string>
                {
                        "susceptible", 
                        "infected", 
                        "sick", 
                        "recovered", 
                        "dead", 
                }),
            };

            return list; 
        }
    }
}
