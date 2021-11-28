namespace Lyt.World.Model
{
    using Lyt.CoreMvvm;
    using Lyt.CoreMvvm.Extensions;
    using Lyt.World.Engine;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    //  Limits to Growth: This is a re-implementation in C# of World3, the social-economic-environmental model created by
    //  Dennis and Donella Meadows and others circa 1970. The results of the modeling exercise were published in The Limits to Growth
    //  in 1972, and the model itself was more fully documented in Dynamics of Growth in a Finite World in 1974. 

    #region MIT License and more

    /* Original Work by Brian Hayes - under MIT Licence - Can be found in the JavaScript project folder
      
     
        MIT License

        Copyright (c) 2016 Brian Hayes

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

    /*
        Original Comments by Brian Hayes, circa 2012 

        The Limits to Growth_ by Meadows et al. (1972) presented a system dynamics model of the global ecosystem and economy, called World3.
        The original simulation was written in a language called DYNAMO. 
        The code in this repository, written in 2012, attempts to translate the DYNAMO World3 program into JavaScript.
        You can run the program in a web browser at [http://bit-player.org/extras/limits/ltg.html](http://bit-player.org/extras/limits/ltg.html).

        For background on the project see:

        * "Computation and the Human Predicament: _The Limits to Growth_ and the limits to computer modeling," by Brian Hayes, _American Scientist_ Volume 100, Number 3, May-June 2012, pp. 186–191. 
        * Available online in [HTML]
        * (http://www.americanscientist.org/issues/pub/computation-and-the-human-predicament) 
        * and [PDF]
        * (http://www.americanscientist.org/libraries/documents/2012491358139046-2012-05Hayes.pdf).
        * [World3, the public beta](http://bit-player.org/2012/world3-the-public-beta) (article posted on bit-player.org).    
     */

    /* Derivative Work, this code, by Laurent Yves Testud - under MIT Licence 


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

    /* Laurent's comments, circa 2021
        This project tries to bring to a larger population of software 'people' the scope of this work done now more than 50 years ago.
        Sadly: It is spot on.

     */
    #endregion MIT License 

    public sealed partial class WorldModel : Simulator
    {
        public const int StartYear = 1900;
        public const int PolicyYear = 1975; // eqn 150.1


        private Parameter[] parameters = new Parameter[]
        {
            new Parameter("Simulation Duration", 220, 180, 420, 20, Widget.Slider, null, Format.Integer),
            new Parameter("Delta Time", 1.0, 0.2, 1.0, 0.1),
            new Parameter("Resources Multiplier", 1.0, 0.5, 2.5, 0.1),
            new Parameter("Output Consumed", 0.43, 0.31, 0.53, 0.02),
        };

        public WorldModel() : base()
        {
            this.Parameters = new Parameters(parameters);
            this.Parameters.ToDefaults();
            this.CreatePopulationSector();
            this.CreateCapitalSector();
            this.CreateAgriculturalSector();
            this.CreateOtherSectors();
            this.AdjustForPersistenPollutionAppearanceRate();
            base.FinalizeConstruction(this.auxSequence, this.CustomUpdate);
        }

        public override double InitialTime() => WorldModel.StartYear; 

        public override bool SimulationEnded()
        {
            var durationYears = this.Parameters.FromName("Simulation Duration");
            return (this.Time > this.InitialTime() + (int)durationYears.CurrentValue);
        }

        public override void Parametrize()
        {
            this.SetInitialResources(this.Parameters.Get("Resources Multiplier"));
            this.SetOutputConsumed(this.Parameters.Get("Output Consumed"));
        }


        public override List<PlotDefinition> Plots ()
        {
            return new List<PlotDefinition>
            {
                new PlotDefinition("Population", PlotKind.Absolute, new List<string>
                    {
                        "population",
                        "population0To14",
                        "population0To44",
                        "population0To64",
                    }),
                new PlotDefinition("Industry", PlotKind.Absolute, new List<string>
                    {
                        "industrialOutput",
                    }),
                new PlotDefinition("Services", PlotKind.Absolute, new List<string>
                    {
                        "serviceOutput",
                    }),
                new PlotDefinition("Agriculture", PlotKind.Absolute, new List<string>
                    {
                        "food",
                    }),
                new PlotDefinition("Resources", PlotKind.Absolute, new List<string>
                    {
                        "nonrenewableResources",
                    }),
                new PlotDefinition("Pollution", PlotKind.Absolute, new List<string>
                    {
                        "persistentPollution",
                    }),
                new PlotDefinition("Arable Land", PlotKind.Absolute, new List<string>
                    {
                        "arableLand",
                    }),
                new PlotDefinition("Life Expectancy", PlotKind.Absolute, new List<string>
                    {
                        "lifeExpectancy",
                    }),

                new PlotDefinition("Food Per Capita", PlotKind.Absolute, new List<string>
                    {
                        "foodPerCapita",
                    }),
            };
        }

        private void CustomUpdate() => this.persistenPollutionAppearanceRate.Update();

        private void AdjustForPersistenPollutionAppearanceRate() =>
            this.Auxiliaries.Remove("persistenPollutionAppearanceRate");
    }
}
