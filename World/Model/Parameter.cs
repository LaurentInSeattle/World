namespace Lyt.World.Model
{
    public enum Widget
    {
        Slider,
        Switch,
    }

    public enum Format
    {
        Integer,
        Float,
    }

    public sealed class Parameter
    {
        public Parameter(
            string name, double defaultValue, double min, double max, double step,
            Widget widget = Widget.Slider, string equationName = null, Format format = Format.Float)
        {
            this.Name = name;
            this.DefaultValue = defaultValue;
            this.Min = min;
            this.Max = max;
            this.Step = step;
            this.Widget = widget;
            this.EquationName = equationName;
            this.Format = format;
        }

        public string Name { get; private set; }

        public Format Format { get; private set; }

        public double DefaultValue { get; private set; }

        public double Min { get; private set; }

        public double Max { get; private set; }

        public double Step { get; private set; }

        public Widget Widget { get; private set; }

        public string EquationName { get; private set; }

        public double CurrentValue { get; private set; }

        public double EditedValue { get; set; }

        public void Default()
        {
            this.CurrentValue = this.DefaultValue;
            this.EditedValue = this.DefaultValue;
        }

        public void CommitEdits() => this.CurrentValue = this.EditedValue;

        public void CancelEdits() => this.EditedValue = this.DefaultValue;
    }

}
