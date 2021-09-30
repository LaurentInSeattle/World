namespace Lyt.World.Model
{
    public sealed class Rate : Equation
    {
        public Rate(Model model, string name, int number, string units) 
            : base(model, name, number, units) 
            => model.OnNewRate(this);
    }
}
