namespace Lyt.World
{
    using Lyt.CoreMvvm;
    using Lyt.World.UserInterface;

    using System.Windows;

    #region MIT License and more

    /* Original Work by Brian Hayes - under MIT Licence 
      
     
        MIT License

        Copyright (c) 2016 Brian Hayes

        Permission is hereby granted, free of charge, to any person obtaining a copy
        of this software and associated documentation files (the "Software"), to deal
        in the Software without restriction, including without limitation the rights
        to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
        copies of the Software, and to permit persons to whom the Software is
        furnished to do so, subject to the following conditions:

        The above copyright notice and this permission notice shall be included in all
        copies or substantial portions of the Software.

        THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
        IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
        FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
        AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
        LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
        OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
        SOFTWARE.
     */

    /*
        Original Comments by Brian Hayes, circa 2012 

        The Limits to Growth_ by Meadows et al. (1972) presented a system dynamics model of the global ecosystem and economy, called World3.
        The original simulation was written in a language called DYNAMO. 
        The code in this repository, written in 2012, attempts to translate the DYNAMO World3 program into JavaScript.
        You can run the program in a web browser at [http://bit-player.org/extras/limits/ltg.html](http://bit-player.org/extras/limits/ltg.html).

        For background on the project see:

        * "Computation and the Human Predicament: _The Limits to Growth_ and the limits to computer modeling," by Brian Hayes, _American Scientist_ Volume 100, Number 3, May-June 2012, pp. 186–191. 
        * Available online in [HTML]
        * (http://www.americanscientist.org/issues/pub/computation-and-the-human-predicament) 
        * and [PDF]
        * (http://www.americanscientist.org/libraries/documents/2012491358139046-2012-05Hayes.pdf).
        * [World3, the public beta](http://bit-player.org/2012/world3-the-public-beta) (article posted on bit-player.org).    
     */

    /* Derivative Work, this code, by Laurent Yves Testud - under MIT Licence 


       MIT License

       Copyright (c) 2021 Laurent Yves Testud 

       Permission is hereby granted, free of charge, to any person obtaining a copy
       of this software and associated documentation files (the "Software"), to deal
       in the Software without restriction, including without limitation the rights
       to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
       copies of the Software, and to permit persons to whom the Software is
       furnished to do so, subject to the following conditions:

       The above copyright notice and this permission notice shall be included in all
       copies or substantial portions of the Software.

       THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
       IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
       FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
       AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
       LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
       OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
       SOFTWARE.
    */

    /* My comments, circa 2021
        This code tries to bring to a larger population of software 'people' the scope of this work done now more than 50 years ago.
        Sadly: It is spot on.

     */
    #endregion MIT License 

    /// <summary> /// Interaction logic for MainWindow.xaml /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.Loaded += (e, a) => 
            {
                var simulationBindable = Binder<SimulationControl, SimulationBindable>.Create() ;
                simulationBindable.OnLoad();
                this.Content = simulationBindable.View; 
            };
        } 
    }
}
