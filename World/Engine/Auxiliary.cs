namespace Lyt.World.Engine
{
    public class Auxiliary : Equation
    {
        public Auxiliary(Simulator model, string name, int number, string units) : base(model, name, number, units) 
            =>  model.OnNewAuxiliary(this);

        public Auxiliary(Simulator model, string name, int number) : base(model, name, number)
            => model.OnNewAuxiliary(this);

        public Auxiliary(string name, string units) : base(Simulator.Instance, name, 0, units)
            => Simulator.Instance.OnNewAuxiliary(this);

        public Auxiliary(string name) : base(Simulator.Instance, name, 0)
            => Simulator.Instance.OnNewAuxiliary(this);

        public int EvaluationOrder { get; set; }
    }
}
