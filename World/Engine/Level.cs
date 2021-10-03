namespace Lyt.World.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public sealed class Level : Equation
    {
        private double initialValue; 

        public Level(Simulator model, string name, int number, string units, double initialValue) : base (model, name, number, units)
        {
            this.initialValue = initialValue;
            model.OnNewLevel(this);
        }

        public void ReInitialize(double initialValue)
        {
            this.initialValue = initialValue;
            this.Initialize(); 
        }

        public override void Initialize()
        {
            this.J = this.initialValue;
            this.K = this.initialValue;
        }
    }
}
