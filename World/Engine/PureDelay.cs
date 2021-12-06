namespace Lyt.World.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;


    public sealed class PureDelay : Auxiliary
    {
        private readonly string inputEquationName;
        private readonly double delay;
        private int stageCount;
        private List<double> stages;
        private Equation Input;

        public PureDelay(Simulator model, string name, int number, string units, double delay, string inputEquationName)
            : base(model, name, number, units)
        {
            this.delay = delay;
            this.inputEquationName = inputEquationName;
        }

        public PureDelay(string name, string units, double delay, string inputEquationName)
            : base(Simulator.Instance, name, 0, units)
        {
            this.delay = delay;
            this.inputEquationName = inputEquationName;
        }

        public override void Reset()
        {
            this.Input = this.Simulator.EquationFromName(this.inputEquationName);
            base.Reset();
        }

        public override void Initialize()
        {
            this.stageCount = (int)(delay / Simulator.Instance.DeltaTime);
            this.stages = new List<double>(stageCount);

            this.J = this.K = this.Input.K;
            for (int i = 0; i < this.stageCount; ++i)
            {
                this.stages.Add(this.Input.K); 
            }
        }

        public override void Update()
        {
            this.J = this.K = this.stages[this.stageCount -1 ];
            this.stages.Insert(0,this.Input.K);
            this.stages.RemoveAt(this.stageCount);
        }
    }

}
