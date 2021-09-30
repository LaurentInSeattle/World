namespace Lyt.CoreMvvm
{
    using System;
    using System.Windows;
    using System.Windows.Media.Animation;

    public class GridLengthAnimation : AnimationTimeline
    {
        public static readonly DependencyProperty FromProperty;

        public static readonly DependencyProperty ToProperty;

        static GridLengthAnimation()
        {
            GridLengthAnimation.FromProperty =
                DependencyProperty.Register("From", typeof(GridLength), typeof(GridLengthAnimation));

            GridLengthAnimation.ToProperty =
                DependencyProperty.Register("To", typeof(GridLength), typeof(GridLengthAnimation));
        }

        /// <summary> Default just animates to a visible value. </summary>
        public GridLengthAnimation()
        {
            this.From = new GridLength(0, GridUnitType.Pixel);
            this.To = new GridLength(200, GridUnitType.Pixel);
            this.Duration = TimeSpan.FromMilliseconds(500);
        }

        /// <summary> Animates to a target value, using provided grid length as the start value. </summary>
        public GridLengthAnimation(GridLength gridLength, double to, int milliseconds)
        {
            if ((to < 0.0) || (milliseconds < 5) || (milliseconds > 1000 * 60))
            {
                throw new ArgumentOutOfRangeException("values");
            }

            if (gridLength.GridUnitType != GridUnitType.Pixel)
            {
                throw new Exception("Can only animate absolute pixels grid lengths.");
            }

            this.From = new GridLength(gridLength.Value, GridUnitType.Pixel);
            this.To = new GridLength(to, GridUnitType.Pixel);
            this.Duration = TimeSpan.FromMilliseconds(milliseconds);
        }

        public GridLengthAnimation(double from, double to, int milliseconds)
        {
            if ((to < 0.0) || (milliseconds < 5) || (milliseconds > 1000 * 60))
            {
                throw new ArgumentOutOfRangeException("values");
            }

            this.From = new GridLength(from, GridUnitType.Pixel);
            this.To = new GridLength(to, GridUnitType.Pixel);
            this.Duration = TimeSpan.FromMilliseconds(milliseconds);
        }

        public override Type TargetPropertyType => typeof(GridLength);

        protected override Freezable CreateInstanceCore() => new GridLengthAnimation();

        public GridLength From
        {
            get => (GridLength)this.GetValue(GridLengthAnimation.FromProperty);
            set => this.SetValue(GridLengthAnimation.FromProperty, value);
        }

        public GridLength To
        {
            get => (GridLength)this.GetValue(GridLengthAnimation.ToProperty);
            set => this.SetValue(GridLengthAnimation.ToProperty, value);
        }

        public override object GetCurrentValue(
            object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            double value = animationClock.CurrentProgress.Value;
            double fromValue = ((GridLength)this.GetValue(GridLengthAnimation.FromProperty)).Value;
            double toValue = ((GridLength)this.GetValue(GridLengthAnimation.ToProperty)).Value;

            // The only bits that do matter...
            double interpolated =
                fromValue > toValue ?
                    ((1 - value) * (fromValue - toValue)) + toValue :
                    (value * (toValue - fromValue)) + fromValue;

            return new GridLength(interpolated, GridUnitType.Pixel);
        }
    }
}
