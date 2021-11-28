namespace Lyt.World.Engine
{
    public sealed class Rate : Equation
    {
        public Rate(Simulator model, string name, int number, string units) 
            : base(model, name, number, units) 
            => model.OnNewRate(this);

        public Rate(string name, string units)
            : base(Simulator.Instance, name, 0, units)
            => Simulator.Instance.OnNewRate(this);
    }
}
