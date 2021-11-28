namespace Lyt.World.Engine
{
    public sealed class Smooth : Auxiliary
    {
        public delegate double InitializeDelegate();

        public readonly double delay;
        private bool firstCall;

        private readonly double initialValue;
        private readonly string inputEquationName;
        private Equation input;

        public Smooth(Simulator model, string name, int number, string units, double delay, string inputEquationName, double initialValue) 
            : base(model, name, number, units)
        {
            this.firstCall = true;
            this.inputEquationName = inputEquationName;
            this.delay = delay;
            this.initialValue = initialValue; 
        }

        public Smooth(string name, string units, double delay, string inputEquationName, double initialValue)
            : base(Simulator.Instance, name, 0, units)
        {
            this.firstCall = true;
            this.inputEquationName = inputEquationName;
            this.delay = delay;
            this.initialValue = initialValue;
        }

        public InitializeDelegate InitializeFunction { get; set; }

        public override void Reset()
        {
            this.firstCall = true;
            this.input = this.Simulator.EquationFromName(this.inputEquationName);
            base.Reset();
        }

        public override void Initialize()
        {
            double startValue; 
            if (this.InitializeFunction != null)
            {
                startValue = this.InitializeFunction.Invoke(); 
            }
            else
            {
                startValue = this.initialValue;
            }

            this.J = this.K = startValue; 
        }

        public override void Update()
        {
            if (this.firstCall)
            {
                this.Initialize();
                this.firstCall = false;
            }
            else
            {
                this.K = this.J + this.Simulator.DeltaTime * (this.input.J - this.J) / this.delay;
            }
        }
    }
}
