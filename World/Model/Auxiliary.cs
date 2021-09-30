namespace Lyt.World.Model
{
    public class Auxiliary : Equation
    {
        public Auxiliary(Model model, string name, int number, string units) : base(model, name, number, units) 
            =>  model.OnNewAuxiliary(this);

        public Auxiliary(Model model, string name, int number) : base(model, name, number)
            => model.OnNewAuxiliary(this);

        public int EvaluationOrder { get; set; }
    }
}
