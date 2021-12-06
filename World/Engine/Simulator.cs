namespace Lyt.World.Engine
{
    using Lyt.CoreMvvm;
    using Lyt.CoreMvvm.Extensions;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public abstract class Simulator
    {
        public static Simulator Instance; 
            
        protected readonly List<Equation> EquationsList;
        protected readonly List<Level> Levels;
        protected readonly List<Rate> Rates;

        protected Dictionary<string, Auxiliary> Auxiliaries;
        protected List<Auxiliary> OrderedAuxiliaries;
        protected Dictionary<string, Equation> EquationsDictionary;

        protected string sector;
        protected string subSector;

        private Action customUpdate;

        private Action customStart;

        public Simulator()
        {
            this.EquationsList = new List<Equation>(256) { null };
            this.Levels = new List<Level>(32);
            this.Rates = new List<Rate>(32);
            this.Auxiliaries = new Dictionary<string, Auxiliary>(128);
            this.OrderedAuxiliaries = new List<Auxiliary>(128);
        }

        public abstract void Parametrize ();

        public abstract bool SimulationEnded();

        public abstract List<PlotDefinition> Plots();

        public virtual double InitialTime() => 0.0;

        public virtual string TimeUnit => string.Empty;

        public virtual int PlotRows => 1;

        public virtual int PlotCols => 1;


        protected void FinalizeConstruction (IEnumerable<string> auxSequence, Action customUpdate = null, Action customStart = null)
        {
            this.customUpdate = customUpdate;
            this.customStart = customStart;
            this.SortAuxiliaryEquations(auxSequence);
            this.EquationsDictionary = this.EquationsList.Where(equ => equ != null).ToDictionary(equ => equ.Name, equ => equ);
            this.Reset();
        }

        public Parameters Parameters { get; protected set; }

        public int TickCount { get; private set; }

        public double Time { get; private set; }

        public double DeltaTime { get; private set; }

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

        public void Tick()
        {
            ++this.TickCount;
            this.UpdateLevels();
            this.UpdateAuxiliaries();
            this.UpdateRates();
            this.customUpdate?.Invoke();
            this.TickEquations();
            this.CheckForNaNsAndInfinities();
            this.Time += this.DeltaTime;
        }

        public void Log(IEnumerable<string> equationNames) 
            => equationNames.ForEach<string>((equationName) => this.EquationFromName(equationName).LogData());

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

        public void Start(double deltaTime)
        {
            this.DeltaTime = deltaTime;
            this.Reset();
            this.InitializeLevels();
            this.InitializeSmoothAndDelays();
            this.TickCount = 0;
            this.Time = this.InitialTime();

            for (int i = 1; i <= 3; ++i)
            {
                this.UpdateAuxiliaries();
                this.UpdateRates();
                this.customUpdate?.Invoke();
                this.TickEquations();
            }

            this.InitializeLevels();
            this.TickCount = 0;
            this.Time = this.InitialTime();
            this.customStart?.Invoke();
        }

        protected static double Clip(double a, double b, double x, double y) => x >= y ? a : b;

        protected static double Positive (double x) => x < 0.0 ? 0.0 : x;

        protected static double AsInt(double x) => Math.Round(x, MidpointRounding.AwayFromZero);

        protected void CheckForNaNsAndInfinities()
            => this.EquationsDictionary.Values.ForEach<Equation>((e) => e.CheckForNaNAndInfinity());

        private void SortAuxiliaryEquations(IEnumerable<string> auxSequence)
        {
            int orderIndex = 0;
            foreach (string auxiliaryName in auxSequence)
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
                    if (equation is PureDelay pureDelay)
                    {
                        pureDelay.Initialize();
                    }

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

        private void InitializeLevels() => this.Levels.ForEach<Level>((level) => level.Initialize());

        private void UpdateLevels() => this.Levels.ForEach<Level>((level) => level.Update());

        private void UpdateAuxiliaries() => this.OrderedAuxiliaries.ForEach<Auxiliary>((aux) => aux.Update());

        private void UpdateRates() => this.Rates.ForEach<Rate>((rate) => rate.Update());

        private void Reset() => this.EquationsList.ForEach<Equation>((equation) => equation.Reset());

        private void TickEquations() => this.EquationsList.ForEach<Equation>((equation) => equation.Tick());

    }
}
