namespace Lyt.World.UserInterface.Controls
{
    using Lyt.CoreMvvm;
    using Lyt.CoreMvvm.Extensions;
    using Lyt.World.Model;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public sealed class RunBindable : DialogBindable<RunControl>
    {
        private WorldModel model; 

        public RunBindable(WorldModel model )
        {
            this.model = model;
            this.IsFastRun = true; 
            this.CancelCommand = new Command(this.ExecuteCancelCommand);
            this.RunCommand = new Command(this.ExecuteRunCommand);
            var sliders = new List<SliderParameterBindable>();
            foreach (var parameter in this.model.Parameters.All.Values)
            {
                sliders.Add(new SliderParameterBindable(parameter)); 
            }

            this.Sliders = sliders; 
        }

        private void ExecuteCancelCommand(object _)
        {
            this.model.Parameters.CancelEdits();
            this.Dismiss(); 
        }

        private void ExecuteRunCommand(object _)
        {
            this.model.Parameters.CommitEdits(); 
            this.Validate();
        }


        #region Bound Properties

        /// <summary> Gets or sets the IsFastRun property.</summary>
        public bool IsFastRun { get => this.Get<bool>(); set => this.Set(value); }

        /// <summary> Gets or sets the CancelCommand property.</summary>
        public ICommand CancelCommand { get => this.Get<ICommand>(); set => this.Set(value); }

        /// <summary> Gets or sets the RunCommand property.</summary>
        public ICommand RunCommand { get => this.Get<ICommand>(); set => this.Set(value); }

        /// <summary> Gets or sets the Sliders property.</summary>
        public List<SliderParameterBindable> Sliders { get => this.Get<List<SliderParameterBindable>>(); set => this.Set(value); }

        #endregion Bound Properties
    }
}
