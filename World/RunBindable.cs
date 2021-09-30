
namespace Lyt.World
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
        private Model.Model model; 

        public RunBindable(Model.Model model )
        {
            this.model = model;
            this.CancelCommand = new Command(this.ExecuteCancelCommand);
            this.RunCommand = new Command(this.ExecuteRunCommand);
            var sliders = new List<SliderParameterBindable>();
            foreach (var parameter in Parameters.Instance.All.Values)
            {
                sliders.Add(new SliderParameterBindable(parameter)); 
            }

            this.Sliders = sliders; 
        }

        private void ExecuteCancelCommand(object _)
        {
            Parameters.Instance.CancelEdits();
            this.Dismiss(); 
        }

        private void ExecuteRunCommand(object _)
        {
            Parameters.Instance.CommitEdits(); 
            this.Validate();
        }


        #region Bound Properties

        /// <summary> Gets or sets the CancelCommand property.</summary>
        public ICommand CancelCommand { get => this.Get<ICommand>(); set => this.Set(value); }

        /// <summary> Gets or sets the RunCommand property.</summary>
        public ICommand RunCommand { get => this.Get<ICommand>(); set => this.Set(value); }

        /// <summary> Gets or sets the Sliders property.</summary>
        public List<SliderParameterBindable> Sliders { get => this.Get<List<SliderParameterBindable>>(); set => this.Set(value); }

        #endregion Bound Properties
    }
}
