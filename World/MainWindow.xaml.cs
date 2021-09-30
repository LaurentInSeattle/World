// #define VERBOSE 

namespace Lyt.World
{
    using Lyt.CoreMvvm;
    using Lyt.World.Model;

    using ScottPlot;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Threading;

    #region MIT License and more

    /* Original Work by Brian Hayes - under MIT Licence 
      
     
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

    /* My comments, circa 2021
        This code tries to bring to a larger population of software 'people' the scope of this work done now more than 50 years ago.
        Sadly: It is spot on.

     */
    #endregion MIT License 

    /// <summary> /// Interaction logic for MainWindow.xaml /// </summary>
    public partial class MainWindow : Window
    {
        #region VERBOSE and DEBUG variables 

        private List<string> debugVariables = new List<string>
        {
            //"industrialCapital", 
            //"industrialOutput", 
            //"lifeExpectancy",
            "food",
            //"landYield",
            //"landYieldFactor", 
            //"landFertility", 
            //"landYieldMultiplierFromCapital", 
            //"landYieldMultiplierFromAirPollution",
            //"landFertility",
            //"landFertilityRegeneration",
            //"landFertilityRegenerationTime", 
            //"landFertilityDegradation", 

        // "arableLand", 
        //"foodPerCapita",
        //"lifetimeMultiplierFromFood", 
        //"lifetimeMultiplierFromHealthServices", 
        //"lifetimeMultiplierFromPollution", 
        //"lifetimeMultiplierFromCrowding", 
        //"persistentPollution",
        //"persistenPollutionAppearanceRate",
        //"persistentPollutionGenerationRate",
        //"persistentPollutionGeneratedByIndustrialOutput", 
        //"persistentPollutionGeneratedByAgriculturalOutput", 
        //"persistentPollutionGenerationFactor",
        //"perCapitaResourceUsageMultiplier",
    };

        private List<string> trackedVariables = new List<string>
        {
            //"foodRatio",
            //"perceivedFoodRatio", 
            //"population",
            "lifeExpectancy",
            //"food",
            //"lifetimeMultiplierFromFood",
            //"lifetimeMultiplierFromHealthServices",
            //"lifetimeMultiplierFromPollution",
            //"lifetimeMultiplierFromCrowding",
        };

        #endregion VERBOSE and DEBUG variables 

        private List<Plot> Plots;
        private DispatcherTimer timer;
        private bool stopRequested;
        private bool isRunning;
        private Model.Model model;

        public MainWindow() => this.InitializeComponent();

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            this.model = new Model.Model();
            this.CreatePlots();
            this.OnReset(null, null);
        }

        private void CreatePlots()
        {
            this.Plots = new List<Plot>
            {
                new Plot(this.plotTopLeft, "Population", Plot.PlotKind.Absolute, new List<string>
                    {
                        "population",
                        "population0To14",
                        "population0To44",
                        "population0To64",
                    }),
                new Plot(this.plotTopMiddle, "Industry", Plot.PlotKind.Absolute, new List<string>
                    {
                        "industrialOutput",
                    }),
                new Plot(this.plotMidMiddle, "Services", Plot.PlotKind.Absolute, new List<string>
                    {
                        "serviceOutput",
                    }),
                new Plot(this.plotBotMiddle, "Agriculture", Plot.PlotKind.Absolute, new List<string>
                    {
                        "food",
                    }),
                new Plot(this.plotTopRight, "Resources", Plot.PlotKind.Absolute, new List<string>
                    {
                        "nonrenewableResources",
                    }),
                new Plot(this.plotMidRight, "Pollution", Plot.PlotKind.Absolute, new List<string>
                    {
                        "persistentPollution",
                    }),
                new Plot(this.plotBotRight, "Arable Land", Plot.PlotKind.Absolute, new List<string>
                    {
                        "arableLand",
                    }),
                new Plot(this.plotMidLeft, "Life Expectancy", Plot.PlotKind.Absolute, new List<string>
                    {
                        "lifeExpectancy",
                    }),

                new Plot(this.plotBotLeft, "Food Per Capita", Plot.PlotKind.Absolute, new List<string>
                    {
                        "foodPerCapita",
                    }),
            };

        }

        private void OnReset(object sender, RoutedEventArgs e)
        {
            if (this.model == null)
            {
                this.OnLoad(null, null);
                return;
            }

            this.log.Text = string.Empty;
            this.track.Text = string.Empty;
            this.model.Start();
            var joined = new List<string>();
            foreach (var plot in this.Plots)
            {
                joined.AddRange(plot.Equations);
            }

            joined.AddRange(this.trackedVariables);
            this.model.Log(joined);

#if VERBOSE 
            this.mainGrid.RowDefinitions[0].Height = new GridLength(104, GridUnitType.Pixel);
#else
            this.mainGrid.RowDefinitions[0].Height = new GridLength(0, GridUnitType.Pixel);
#endif
            this.UpdateDebugLog();
            this.UpdateTrackingLog();
            this.UpdateYear();
        }

        private void OnTick(object sender, RoutedEventArgs e)
        {
            if (this.model == null)
            {
                this.OnLoad(null, null);
            }

            this.model.Tick();
            this.UpdateDebugLog();
            this.UpdateTrackingLog();
            this.UpdatePlots();
            this.UpdateYear();
        }

        private void OnRun(object sender, RoutedEventArgs e)
        {
            if (this.model == null)
            {
                this.OnLoad(null, null);
            }
            else
            {
                this.OnReset(null, null);
            }

            var runControl = new RunControl();
            var runBindable = new RunBindable(this.model);
            var dialog = new Dialog<RunControl>("Run the Model", runControl, runBindable) { ShowInTaskbar = true };
            bool? accepted = dialog.ShowDialog();
            if (!accepted.Value)
            {
                return;
            }

            this.model.Parametrize();
            this.timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200),
                IsEnabled = true,
            };

            this.stopRequested = false;
            this.timer.Tick += TimerTick;
            this.timer.Start();
            this.isRunning = true;
        }

        private void OnStop(object sender, RoutedEventArgs e)
        {
            this.stopRequested = true;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            this.OnTick(null, null);
            var durationYears = Parameters.Instance.FromName("Simulation Duration");
            if (this.stopRequested || (model.Time > Model.Model.StartYear + (int)durationYears.CurrentValue))
            {
                this.timer.Stop();
                this.timer.Tick -= TimerTick;
                this.timer = null;
                this.isRunning = false;
            }
        }

        [Conditional("VERBOSE")]
        private void UpdateDebugLog()
        {
            this.log.Text = this.log.Text + "\n~ Tick: " + this.model.TickCount.ToString() + "\n";
            foreach (string variableName in debugVariables)
            {
                var equation = model.EquationFromName(variableName);
                string line = string.Format("{0}: {1:0,0.000}\n", equation.FriendlyName, equation.K);
                this.log.Text = this.log.Text + line;
            }

            foreach (string variableName in trackedVariables)
            {
                var equation = model.EquationFromName(variableName);
                string line = string.Format("{0}: {1:0,0.000}\n", equation.FriendlyName, equation.K);
                this.log.Text = this.log.Text + line;
            }

            this.log.Text = this.log.Text + "\n";
        }

        [Conditional("VERBOSE")]
        private void UpdateTrackingLog()
        {
            this.track.Text = string.Empty;
            this.track.Text = string.Format("Tick: {0}:  Year: {1:D}  \n\n", this.model.TickCount, (int)this.model.Time);
            foreach (string variableName in trackedVariables)
            {
                var equation = model.EquationFromName(variableName);
                string line =
                    string.Format("{0}:\t\t{1:0,0.000}\n", equation.FriendlyName, equation.K);
                this.track.Text = this.track.Text + line;
            }
        }

        private void UpdatePlots()
        {
            int length = (int)((this.model.Time - Model.Model.StartYear) / this.model.DeltaTime);
            double[] dataX = new double[length];
            for (int i = 0; i < length; ++i)
            {
                dataX[i] = Model.Model.StartYear + i * this.model.DeltaTime;
            }

            int locationIndex = 0;
            foreach (var plot in this.Plots)
            {
                this.UpdatePlot(plot.PlotHost, plot.Name, plot.Kind, plot.Equations, dataX);
                ++locationIndex;
            }
        }

        private void UpdatePlot(WpfPlot plot, string name, Plot.PlotKind kind, List<string> equations, double[] dataX)
        {
            var pPlot = plot.Plot;
            pPlot.Clear();
            pPlot.XAxis2.Label(name);
            int length = (int)((this.model.Time - Model.Model.StartYear) / this.model.DeltaTime);
            foreach (string equationName in equations)
            {
                var equation = this.model.EquationFromName(equationName);
                double[] dataY = new double[length];
                for (int i = 0; i < length; ++i)
                {
                    double value;
                    if (kind == Plot.PlotKind.Normalized)
                    {
                        value = equation.NormalizedLoggedValue(i);
                    }
                    else
                    {
                        value = equation.Log[i];
                    }

                    dataY[i] = value;
                }

                pPlot.AddScatter(dataX, dataY);
            }
        }

        private void UpdateYear()
        {
            if (this.isRunning)
            {
                this.year.Text = string.Format("Year: {0} ", (int)this.model.Time);
            }
            else
            {
                this.year.Text = string.Empty;
            }
        }


        private class Plot
        {
            public enum PlotKind
            {
                Absolute,
                Normalized,
            }

            public Plot(WpfPlot plot, string name, PlotKind kind, List<string> equations)
            {
                this.PlotHost = plot;
                this.Name = name;
                this.Kind = kind;
                this.Equations = equations;
            }

            public WpfPlot PlotHost { get; private set; }

            public string Name { get; private set; }

            public PlotKind Kind { get; private set; }

            public List<string> Equations { get; private set; }
        }
    }
}
