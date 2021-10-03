// #define VERBOSE

namespace Lyt.World.UserInterface
{
    using Lyt.CoreMvvm;
    using Lyt.World.Model;
    using Lyt.World.UserInterface.Controls;

    using ScottPlot;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Threading;

    public sealed class SimulationBindable : Bindable<SimulationControl>
    {
        #region VERBOSE and DEBUG variables 

        private List<string> debugVariables = new List<string>
        {
            //"industrialCapital", 
            //"industrialOutput", 
            //"lifeExpectancy",
            "food",
        };

        private List<string> trackedVariables = new List<string>
        {
            "lifeExpectancy",
        };

        #endregion VERBOSE and DEBUG variables 

        private List<Plot> Plots;
        private DispatcherTimer timer;
        private bool stopRequested;
        private bool isRunning;
        private bool runFast;
        private WorldModel model;


        public void OnLoad()
        {
            this.model = new WorldModel();
            this.StopCommand = new Command(this.OnStop);
            this.RunCommand = new Command(this.OnRun);
            this.CreatePlots();
            this.OnReset();
        }

        private void CreatePlots()
        {
            var gridContent = this.View.PlotsGrid.Children;

            void PlaceAt(WpfPlot wpfPlot, int row, int col)
            {
                Grid.SetRow(wpfPlot, row);
                Grid.SetColumn(wpfPlot, col);
                gridContent.Add(wpfPlot);
            }

            var plotTopLeft = new WpfPlot();
            PlaceAt(plotTopLeft, 0, 0);

            var plotTopMiddle = new WpfPlot();
            PlaceAt(plotTopMiddle, 0, 1);

            var plotTopRight = new WpfPlot();
            PlaceAt(plotTopRight, 0, 2);

            var plotMidLeft = new WpfPlot();
            PlaceAt(plotMidLeft, 1, 0);

            var plotMidMiddle = new WpfPlot();
            PlaceAt(plotMidMiddle, 1, 1);

            var plotMidRight = new WpfPlot();
            PlaceAt(plotMidRight, 1, 2);

            var plotBotLeft = new WpfPlot();
            PlaceAt(plotBotLeft, 2, 0);

            var plotBotMiddle = new WpfPlot();
            PlaceAt(plotBotMiddle, 2, 1);


            var plotBotRight = new WpfPlot();
            PlaceAt(plotBotRight, 2, 2);
            this.Plots = new List<Plot>
            {
                new Plot(plotTopLeft, "Population", Plot.PlotKind.Absolute, new List<string>
                    {
                        "population",
                        "population0To14",
                        "population0To44",
                        "population0To64",
                    }),
                new Plot(plotTopMiddle, "Industry", Plot.PlotKind.Absolute, new List<string>
                    {
                        "industrialOutput",
                    }),
                new Plot(plotMidMiddle, "Services", Plot.PlotKind.Absolute, new List<string>
                    {
                        "serviceOutput",
                    }),
                new Plot(plotBotMiddle, "Agriculture", Plot.PlotKind.Absolute, new List<string>
                    {
                        "food",
                    }),
                new Plot(plotTopRight, "Resources", Plot.PlotKind.Absolute, new List<string>
                    {
                        "nonrenewableResources",
                    }),
                new Plot(plotMidRight, "Pollution", Plot.PlotKind.Absolute, new List<string>
                    {
                        "persistentPollution",
                    }),
                new Plot(plotBotRight, "Arable Land", Plot.PlotKind.Absolute, new List<string>
                    {
                        "arableLand",
                    }),
                new Plot(plotMidLeft, "Life Expectancy", Plot.PlotKind.Absolute, new List<string>
                    {
                        "lifeExpectancy",
                    }),

                new Plot(plotBotLeft, "Food Per Capita", Plot.PlotKind.Absolute, new List<string>
                    {
                        "foodPerCapita",
                    }),
            };

        }

        private void OnReset()
        {
            if (this.model == null)
            {
                this.OnLoad();
                return;
            }

            this.LogText = string.Empty;
            this.TrackText = string.Empty;
            this.model.Start();
            var joined = new List<string>();
            foreach (var plot in this.Plots)
            {
                joined.AddRange(plot.Equations);
            }

            joined.AddRange(this.trackedVariables);
            this.model.Log(joined);

#if VERBOSE 
            this.DebugRowHeight = new GridLength(104, GridUnitType.Pixel);
#else
            this.DebugRowHeight = new GridLength(0, GridUnitType.Pixel);
#endif
            this.UpdateDebugLog();
            this.UpdateTrackingLog();
            this.UpdateYear();
        }

        private void OnTick()
        {
            if (this.model == null)
            {
                this.OnLoad();
            }

            this.model.Tick();
            if (!this.runFast)
            {
                this.UpdateDebugLog();
                this.UpdateTrackingLog();
                this.UpdatePlots();
            }
            else
            {
                if ( ((int) this.model.Time) % 20 == 0)
                {
                    this.UpdatePlots();
                }
            }

            this.UpdateYear();
        }

        private void OnRun(object _)
        {
            if (this.model == null)
            {
                this.OnLoad();
            }
            else
            {
                this.OnReset();
            }

            var runControl = new RunControl();
            var runBindable = new RunBindable(this.model);
            var dialog = new Dialog<RunControl>("Run the Model", runControl, runBindable) { ShowInTaskbar = true };
            bool? accepted = dialog.ShowDialog();
            if (!accepted.Value)
            {
                return;
            }

            this.runFast = runBindable.IsFastRun;
            this.model.Parametrize();
            double milliseconds = this.runFast ? 10 : 200;
            this.timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(milliseconds),
                IsEnabled = true,
            };

            this.stopRequested = false;
            this.timer.Tick += TimerTick;
            this.timer.Start();
            this.isRunning = true;
        }

        private void OnStop(object _)
        {
            this.stopRequested = true;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            this.OnTick();
            var durationYears = this.model.Parameters.FromName("Simulation Duration");
            if (this.stopRequested || (this.model.Time > Model.WorldModel.StartYear + (int)durationYears.CurrentValue))
            {
                this.timer.Stop();
                this.timer.Tick -= TimerTick;
                this.timer = null;
                this.isRunning = false;

                if (this.runFast)
                {
                    this.UpdatePlots();
                }
            }
        }

        [Conditional("VERBOSE")]
        private void UpdateDebugLog()
        {
            this.LogText = this.LogText + "\n~ Tick: " + this.model.TickCount.ToString() + "\n";
            foreach (string variableName in debugVariables)
            {
                var equation = model.EquationFromName(variableName);
                string line = string.Format("{0}: {1:0,0.000}\n", equation.FriendlyName, equation.K);
                this.LogText = this.LogText + line;
            }

            foreach (string variableName in trackedVariables)
            {
                var equation = model.EquationFromName(variableName);
                string line = string.Format("{0}: {1:0,0.000}\n", equation.FriendlyName, equation.K);
                this.LogText = this.LogText + line;
            }

            this.LogText = this.LogText + "\n";
        }

        [Conditional("VERBOSE")]
        private void UpdateTrackingLog()
        {
            this.TrackText = string.Empty;
            this.TrackText = string.Format("Tick: {0}:  Year: {1:D}  \n\n", this.model.TickCount, (int)this.model.Time);
            foreach (string variableName in trackedVariables)
            {
                var equation = model.EquationFromName(variableName);
                string line =
                    string.Format("{0}:\t\t{1:0,0.000}\n", equation.FriendlyName, equation.K);
                this.TrackText = this.TrackText + line;
            }
        }

        private void UpdatePlots()
        {
            int length = (int)((this.model.Time - WorldModel.StartYear) / this.model.DeltaTime);
            double[] dataX = new double[length];
            for (int i = 0; i < length; ++i)
            {
                dataX[i] = WorldModel.StartYear + i * this.model.DeltaTime;
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
            int length = (int)((this.model.Time - WorldModel.StartYear) / this.model.DeltaTime);
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
            => this.YearText = this.isRunning ? string.Format("Year: {0} ", (int)this.model.Time) : string.Empty;

        #region Bound Properties

        /// <summary> Gets or sets the LogText property.</summary>
        public string LogText { get => this.Get<string>(); set => this.Set(value); }

        /// <summary> Gets or sets the TrackText property.</summary>
        public string TrackText { get => this.Get<string>(); set => this.Set(value); }

        /// <summary> Gets or sets the YearText property.</summary>
        public string YearText { get => this.Get<string>(); set => this.Set(value); }

        /// <summary> Gets or sets the YearText property.</summary>
        public GridLength DebugRowHeight { get => this.Get<GridLength>(); set => this.Set(value); }

        /// <summary> Gets or sets the StopCommand property.</summary>
        public ICommand StopCommand { get => this.Get<ICommand>(); set => this.Set(value); }

        /// <summary> Gets or sets the RunCommand property.</summary>
        public ICommand RunCommand { get => this.Get<ICommand>(); set => this.Set(value); }

        #endregion Bound Properties

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
