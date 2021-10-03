namespace Lyt.World.Engine
{
    public sealed class Rate : Equation
    {
        public Rate(Simulator model, string name, int number, string units) 
            : base(model, name, number, units) 
            => model.OnNewRate(this);
    }
}
