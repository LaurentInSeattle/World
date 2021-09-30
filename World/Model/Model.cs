namespace Lyt.World.Model
{
    using Lyt.CoreMvvm;
    using Lyt.CoreMvvm.Extensions;

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

    public sealed partial class Model
    {
        public const int StartYear = 1900;
        public const int PolicyYear = 1975; // eqn 150.1

        private readonly List<Equation> EquationsList;
        private readonly Dictionary<string, Equation> EquationsDictionary;
        private readonly List<Level> Levels;
        private readonly List<Rate> Rates;

        private Dictionary<string, Auxiliary> Auxiliaries;
        private List<Auxiliary> OrderedAuxiliaries;

        public int TickCount { get; private set; }
        public double Time { get; private set; }
        public double DeltaTime { get; private set; }

        private string sector;
        private string subSector;

        public Model()
        {
            Parameters.Instance.Create();
            this.EquationsList = new List<Equation>(256) { null };
            this.Levels = new List<Level>(32);
            this.Rates = new List<Rate>(32);
            this.Auxiliaries = new Dictionary<string, Auxiliary>(128);
            this.OrderedAuxiliaries = new List<Auxiliary>(128);

            this.CreatePopulationSector();
            this.CreateCapitalSector();
            this.CreateAgriculturalSector();
            this.CreateOtherSectors();
            Parameters.Instance.ToDefaults();
            this.AdjustForPersistenPollutionAppearanceRate();
            this.SortAuxiliaryEquations();
            this.EquationsDictionary = this.EquationsList.Where(equ => equ != null).ToDictionary(equ => equ.Name, equ => equ);
            this.Reset();
        }

        private void Reset()
        {
            this.TickCount = 0;
            this.Time = StartYear;
            this.EquationsList.ForEach<Equation>((e)=> e.Reset());
        }

        public void Start()
        {
            this.Reset();
            this.InitializeLevels();
            this.InitializeSmoothAndDelays();
            this.TickCount = 0;
            this.Time = StartYear;

            for (int i = 1; i <= 3; ++i)
            {
                this.UpdateAuxiliaries();
                this.UpdateRates();
                this.persistenPollutionAppearanceRate.Update();
                this.TickEquations();
            }

            this.InitializeLevels();
            this.TickCount = 0;
            this.DeltaTime = Parameters.Instance.Get("Delta Time");
            this.Time = StartYear;
        }

        public void Parametrize()
        {
            this.SetInitialResources(Parameters.Instance.Get("Resources Multiplier"));
            this.SetOutputConsumed(Parameters.Instance.Get("Output Consumed"));
        }

        public void Tick()
        {
            ++this.TickCount;
            this.UpdateLevels();
            this.UpdateAuxiliaries();
            this.UpdateRates();
            this.persistenPollutionAppearanceRate.Update();
            this.TickEquations();
            this.CheckForNaNsAndInfinities();

            this.Time += this.DeltaTime;
        }

        public void Log(IEnumerable<string> equationNames)
        {
            foreach (string equationName in equationNames)
            {
                this.EquationFromName(equationName).LogData();
            }
        }

        public Dictionary<string, List<double>> GetLogs(IEnumerable<string> equationNames)
        {
            var logs = new Dictionary<string, List<double>>();
            foreach (string equationName in equationNames)
            {
                var equation = this.EquationFromName(equationName);
                var log = equation.Log;
                if (!log.IsNullOrEmpty())
                {
                    logs.Add(equationName, log);
                }
                else
                {
                    Debug.WriteLine("No data collected for: " + equationName);
                    continue;
                }
            }

            return logs;
        }

        public void OnNewLevel(Level level) => this.Levels.Add(level);

        public void OnNewRate(Rate rate) => this.Rates.Add(rate);

        public void OnNewAuxiliary(Auxiliary auxiliary) => this.Auxiliaries.Add(auxiliary.Name, auxiliary);

        public void OnNewEquation(Equation equation)
        {
            equation.Sector = this.sector;
            equation.SubSector = this.subSector;
            this.EquationsList.Add(equation);
        }

        public Equation EquationFromName(string equationName)
        {
            if (this.EquationsDictionary.TryGetValue(equationName, out var equation))
            {
                return equation;
            }
            else
            {
                throw new Exception("Equation not found: " + equationName);
            }
        }

        private void AdjustForPersistenPollutionAppearanceRate()
        {
            this.Auxiliaries.Remove("persistenPollutionAppearanceRate");
        }

        private static double Clip(double a, double b, double x, double y) => x >= y ? a : b;

        private void CheckForNaNsAndInfinities() 
            => this.EquationsDictionary.Values.ForEach<Equation>((e)=>e.CheckForNaNAndInfinity());

        private void SortAuxiliaryEquations()
        {
            int orderIndex = 0;
            foreach (string auxiliaryName in this.auxSequence)
            {
                if (this.Auxiliaries.TryGetValue(auxiliaryName, out var auxiliary))
                {
                    auxiliary.EvaluationOrder = orderIndex;
                    ++orderIndex;
                }
            }

            this.OrderedAuxiliaries =
                (from aux in this.Auxiliaries.Values orderby aux.EvaluationOrder ascending select aux).ToList();
            this.Auxiliaries.Clear();
            this.Auxiliaries = null;
        }

        private void InitializeSmoothAndDelays()
        {
            this.EquationsList.ForEach<Equation>(
                (equation) => 
                {
                    if (equation is Smooth smooth)
                    {
                        smooth.Initialize();
                    }

                    if (equation is Delay delay)
                    {
                        delay.Initialize();
                    }
                });
        }

        private void InitializeLevels() => this.Levels.ForEach<Level>((l)=> l.Initialize());

        private void UpdateAuxiliaries() => this.OrderedAuxiliaries.ForEach<Auxiliary>((a) => a.Update());

        private void UpdateRates() => this.Rates.ForEach<Rate>((r) => r.Update());

        private void UpdateLevels() => this.Levels.ForEach<Level>((l) => l.Update());

        private void TickEquations() => this.EquationsList.ForEach<Equation>((e) => e.Tick());
    }
}
