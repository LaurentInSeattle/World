namespace Lyt.World.Model
{
    using Lyt.World.Engine;

    using System.Collections.Generic;

    public sealed partial class CovidModel : Simulator
    {
        private Parameter[] parameters = new Parameter[]
        {
            new Parameter("Simulation Duration", 100, 50, 520, 10, Widget.Slider, null, Format.Integer),
            new Parameter("Delta Time", 1.0, 1.0, 2.0, 1, Widget.Slider, null, Format.Integer),
        };

        public CovidModel() : base()
        {
            Simulator.Instance = this; 
            this.Parameters = new Parameters(parameters);
            this.Parameters.ToDefaults();
            this.CreateModel();
            base.FinalizeConstruction(this.dependencies, null, this.OnStart);
        }

        public override bool SimulationEnded()
        {
            var duration = this.Parameters.FromName("Simulation Duration");
            return (this.Time > (int)duration.CurrentValue);
        }

        public override string TimeUnit => "Days" ;

        public override void Parametrize()
        {
        }

        public void OnStart ( )
        {
            this.infected.J = 
            this.infected.K = 100.0;
        }

        public override int PlotRows => 2;

        public override int PlotCols => 2;

        public override List<PlotDefinition> Plots()
        {
            var list = new List<PlotDefinition>
            {
                new PlotDefinition("Susceptible - Recovered", PlotKind.Absolute, new List<string>
                {
                    "susceptible",
                    "recovered",
                }),
                new PlotDefinition("Infected - Sick - Dead", PlotKind.Absolute, new List<string>
                {
                    "infected",
                    "sick",
                    "dead",
                }),
                new PlotDefinition("New: Infected - Sick ", PlotKind.Absolute, new List<string>
                {
                    "infectedPerDay",
                    "sickPerDay",
                }),
                new PlotDefinition("New: Recoveries - Deaths - Vulnerable", PlotKind.Absolute, new List<string>
                {
                    "recoveryPerDay",
                    "deathPerDay",
                    "vulnerablePerDay",
                }),
            };

            return list; 
        }
    }
}
