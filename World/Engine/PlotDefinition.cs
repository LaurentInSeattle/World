namespace Lyt.World.Engine
{
    using System.Collections.Generic;

    public enum PlotKind
    {
        Absolute,
        Normalized,
    }

    public class PlotDefinition
    {
        public PlotDefinition(string name, PlotKind kind, List<string> equations)
        {
            this.Name = name;
            this.Kind = kind;
            this.Equations = equations;
        }


        public string Name { get; private set; }

        public PlotKind Kind { get; private set; }

        public List<string> Equations { get; private set; }
    }
}
