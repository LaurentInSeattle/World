// #define VERBOSE

namespace Lyt.World.UserInterface
{
    using Lyt.CoreMvvm;
    using Lyt.World.Engine;
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

        // Those lists should go in the model classes 

        private List<string> debugVariables = new List<string>
        {
            //"industrialCapital", 
            //"industrialOutput", 
            //"lifeExpectancy",
            // "food",
        };

        private List<string> trackedVariables = new List<string>
        {
            // "lifeExpectancy",
        };

        #endregion VERBOSE and DEBUG variables 

        private List<Plot> Plots;
        private DispatcherTimer timer;
        private bool stopRequested;
        private bool isRunning;
        private bool runFast;

        private Simulator model;


        public void OnLoad()
        {
            this.StopCommand = new Command(this.OnStop);
            this.RunCommand = new Command(this.OnRun);

            // TODO: Provide some UI to pick the model we want to run 
            // 
            // this.model = new WorldModel();
            this.model = new FluModel();
            this.CreatePlots();
            this.OnReset();
        }

        private void CreatePlots()
        {
            var gridContent = this.View.PlotsGrid.Children;
            this.Plots = new List<Plot>();
            void PlaceAt(WpfPlot wpfPlot, int row, int col)
            {
                Grid.SetRow(wpfPlot, row);
                Grid.SetColumn(wpfPlot, col);
                gridContent.Add(wpfPlot);
                this.Plots.Add(new Plot(wpfPlot)); 
            }

            // TODO: For sure, improve layout 
            PlaceAt(new WpfPlot(), 0, 0);
            PlaceAt(new WpfPlot(), 0, 1);
            PlaceAt(new WpfPlot(), 0, 2);
            PlaceAt(new WpfPlot(), 1, 0);
            PlaceAt(new WpfPlot(), 1, 1);
            PlaceAt(new WpfPlot(), 1, 2);
            PlaceAt(new WpfPlot(), 2, 0);
            PlaceAt(new WpfPlot(), 2, 1);
            PlaceAt(new WpfPlot(), 2, 2);

            int index = 0; 
            var plotDefinitions = this.model.Plots();
            foreach( var plotDefinition in plotDefinitions)
            {
                this.Plots[index].SetDefinition ( plotDefinition) ;
                ++index;
            }
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
            this.model.Start(this.model.Parameters.Get("Delta Time"));
            var joined = new List<string>();
            foreach (var plot in this.Plots)
            {
                if ((plot == null) || !plot.IsValid)
                {
                    continue; 
                }

                if (plot.Equations != null)
                {
                    joined.AddRange(plot.Equations);
                }
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
                if (((int)this.model.Time) % 17 == 0)
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
            if (this.stopRequested || this.model.SimulationEnded())
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
            int length = (int)((this.model.Time - this.model.InitialTime()) / this.model.DeltaTime);
            double[] dataX = new double[length];
            for (int i = 0; i < length; ++i)
            {
                dataX[i] = this.model.InitialTime() + i * this.model.DeltaTime;
            }

            int locationIndex = 0;
            foreach (var plot in this.Plots)
            {
                if ( (plot == null) || !plot.IsValid) 
                {
                    break; 
                }

                this.UpdatePlot(plot.PlotHost, plot.Name, plot.Kind, plot.Equations, dataX);
                ++locationIndex;
            }
        }

        private void UpdatePlot(WpfPlot plot, string name, PlotKind kind, List<string> equations, double[] dataX)
        {
            var pPlot = plot.Plot;
            pPlot.Clear();
            pPlot.XAxis2.Label(name);
            int length = (int)((this.model.Time - this.model.InitialTime()) / this.model.DeltaTime);
            foreach (string equationName in equations)
            {
                var equation = this.model.EquationFromName(equationName);
                double[] dataY = new double[length];
                for (int i = 0; i < length; ++i)
                {
                    double value;
                    if (kind == PlotKind.Normalized)
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
            public Plot(WpfPlot plot)
            {
                this.PlotHost = plot;
            }

            public void SetDefinition (PlotDefinition plotDefinition)
            {
                this.PlotDefinition = plotDefinition;
                this.IsValid = true; 
            }


            public bool IsValid { get; private set; }

            public WpfPlot PlotHost { get; private set; }

            private PlotDefinition PlotDefinition { get; set; }

            public string Name => this.PlotDefinition.Name;

            public PlotKind Kind => this.PlotDefinition.Kind;

            public List<string> Equations => this.PlotDefinition.Equations;
        }
    }
}
