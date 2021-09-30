using System.Diagnostics;

namespace Lyt.World.Model
{
    public sealed class Delay : Auxiliary
    {
        private readonly string inputEquationName;
        private bool firstCall;
        private double delayPerStage;
        private Equation Input;
        private Stage alpha;
        private Stage beta;
        private Stage gamma;

        public Delay(Model model, string name, int number, string units, double delay, string inputEquationName)
            : base(model, name, number, units)
        {
            this.firstCall = true;
            this.delayPerStage = delay / 3.0;
            this.inputEquationName = inputEquationName;
        }

        public override void Reset()
        {
            this.firstCall = true;
            this.Input = this.Model.EquationFromName(this.inputEquationName);
            this.alpha = new Stage();
            this.beta = new Stage();
            this.gamma = new Stage();
            base.Reset();
        }

        public override void Initialize()
        {
            this.J = this.K = this.Input.K;
            this.alpha.J = this.alpha.K = this.Input.J;
            this.beta.J = this.beta.K = this.Input.J;
            this.gamma.J = this.gamma.K = this.Input.J;
        }

        public override void Update()
        {
            if (this.firstCall)
            {
                this.J = this.K = this.Input.K;
                this.alpha.J = this.alpha.K = this.K;
                this.beta.J = this.beta.K = this.K;
                this.gamma.J = this.gamma.K = this.K;
                this.firstCall = false;
            }
            else
            {
                double dt = this.Model.DeltaTime;
                this.alpha.K = this.alpha.J + dt * (this.Input.J - this.alpha.J) / this.delayPerStage;
                this.beta.K = this.beta.J + dt * (this.alpha.J - this.beta.J) / this.delayPerStage;
                this.gamma.K = this.gamma.J + dt * (this.beta.J - this.gamma.J) / this.delayPerStage;
                this.alpha.J = this.alpha.K;
                this.beta.J = this.beta.K;
                this.gamma.J = this.gamma.K;
                this.K = this.gamma.K;
            }
        }

        private class Stage
        {
            public double J;
            public double K;
        }
    }
}
