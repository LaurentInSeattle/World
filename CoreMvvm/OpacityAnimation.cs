namespace Lyt.CoreMvvm
{
    using System;
    using System.Windows;
    using System.Windows.Media.Animation;

    public static class OpacityAnimations
    {
        public delegate void OnAnimationCompleted(object sender, EventArgs e);

        public static void FadeIn(this FrameworkElement element, int milliseconds, double opacity = 1.0)
        {
            element.Opacity = 0.0;
            element.Visibility = Visibility.Visible;
            element.Fade(
                opacity,
                milliseconds,
                (s, e) =>
                    // In some rare cases we are on the wrong thread.
                    element.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Render,
                        (Action)delegate
                        {
                            // Prevent the Animation System to maintain a live reference on the UI
                            // object, thus preventing a view/model to get disposed
                            element.BeginAnimation(UIElement.OpacityProperty, null);

                            // Prevent the opacity to return to zero at the end of the animation
                            element.Opacity = opacity;
                        }),
                isFadeOut: false);
        }

        public static void FadeOut(this FrameworkElement element, int milliseconds)
            => element.Fade(
                0.0,
                milliseconds,
                (s, e) =>
                    // See comments above for FadeIn...
                    element.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Render,
                        (Action)delegate
                        {
                            element.BeginAnimation(UIElement.OpacityProperty, null);
                            element.Opacity = 0.0;
                            element.Visibility = Visibility.Hidden;
                        }));

        public static void TransitionTo(this FrameworkElement element, FrameworkElement toElement)
        {
            element.Visibility = Visibility.Collapsed;
            toElement.Visibility = Visibility.Visible;
        }

        private static void Fade(this FrameworkElement element, double fadeInOpacity, int milliseconds, OnAnimationCompleted onAnimationCompleted, bool isFadeOut = true)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            if ((milliseconds < 5) || (milliseconds > 1000 * 60))
            {
                throw new ArgumentOutOfRangeException(nameof(milliseconds));
            }

            // Ensure no other animation is running
            element.BeginAnimation(UIElement.OpacityProperty, null);

            double from = isFadeOut ? 1.0 : 0.0;
            double to = isFadeOut ? 0.0 : fadeInOpacity;
            var animation =
                new DoubleAnimation
                {
                    From = from,
                    To = to,
                    Duration = TimeSpan.FromMilliseconds(milliseconds),
                    FillBehavior = FillBehavior.Stop, // Avoid sticky animation
                    AutoReverse = false,
                };

            // IMPORTANT
            //  => The Completed event *** MUST *** be hooked up *** BEFORE *** the animation is begun.
            animation.Completed += (s, e) => onAnimationCompleted(s, e);
            element.BeginAnimation(UIElement.OpacityProperty, animation, HandoffBehavior.Compose);
        }

        public static void Resize(this FrameworkElement element, int milliseconds, double height, double width)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            if ((milliseconds < 5) || (milliseconds > 1000 * 60))
            {
                throw new ArgumentOutOfRangeException(nameof(milliseconds));
            }

            // Ensure no other animations are running
            element.BeginAnimation(FrameworkElement.HeightProperty, null);
            element.BeginAnimation(FrameworkElement.WidthProperty, null);

            double fromH = element.ActualHeight;
            double toH = height;
            var animationHeight =
                new DoubleAnimation
                {
                    From = fromH,
                    To = toH,
                    Duration = TimeSpan.FromMilliseconds(milliseconds),
                    FillBehavior = FillBehavior.HoldEnd,
                    AutoReverse = false,
                };

            double fromW = element.ActualWidth;
            double toW = width;
            var animationWidth =
                new DoubleAnimation
                {
                    From = fromW,
                    To = toW,
                    Duration = TimeSpan.FromMilliseconds(milliseconds),
                    FillBehavior = FillBehavior.HoldEnd,
                    AutoReverse = false,
                };

            // IMPORTANT
            //  => The Completed event *** MUST *** be hooked up *** BEFORE *** the animation is begun.
            animationHeight.Completed +=
                (s, e) =>
                {
                    element.Height = height;
                    element.BeginAnimation(FrameworkElement.HeightProperty, null);
                };
            animationWidth.Completed += (s, e) =>
            {
                element.Width = width;
                element.BeginAnimation(FrameworkElement.WidthProperty, null);
            };
            element.BeginAnimation(FrameworkElement.HeightProperty, animationHeight, HandoffBehavior.Compose);
            element.BeginAnimation(FrameworkElement.WidthProperty, animationWidth, HandoffBehavior.Compose);
        }
    }
}