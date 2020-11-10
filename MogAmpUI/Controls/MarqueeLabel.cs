using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MogAmpUI.Controls
{
	public class MarqueeLabel : Label
	{
        /*
         * Statics, constants, enums, magic, etc.
         */

        private static List<double> AllPossibleFontSizes { get; } =
            new List<double>() {
                72.0, 48.0, 36.0, 24.0,
                22.0, 20.0, 18.0, 16.0, 14.0, 12.0,
                11.0, 10.0, 9.0, 8.0, 7.0, 6.0
            };

        /*
         * Constructors, destructors, overrides, etc.
         */

		protected override void OnContentChanged(object oldContent, object newContent)
		{
			base.OnContentChanged(oldContent, newContent);
            FitTextToLabelHeight();
		}

		/*
         * Text dimension handling
         */

		public bool IgnoreParentWidth { set; private get; } = true;

        public void FitTextToLabelHeight()
        {
            string inputString = this.Content as string;
            if (inputString.Length > 0)
            {
                double fontSize = 100.0;

                // see how much room we have,
                int targetWidth = Convert.ToInt32(Math.Round(this.ActualWidth));
                int targetHeight = Convert.ToInt32(Math.Round(this.ActualHeight));

                // should only happen on first launch, because Windows is stupid
                // all of the dimension properties on elements won't be set at that time
                if (targetWidth == 0 || targetHeight == 0) return;

                // iterate over all possible font sizes
                // TODO: see if variable fonts make this unnecessary
                foreach (double fs in AllPossibleFontSizes)
                {
                    fontSize = fs;

                    // yay windows bloat
                    var formattedText = new FormattedText(
                        inputString, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                        new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch),
                        fontSize, Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip
                    );

                    // find the generated font that just fits within the element dimensions
                    double actualTextWidth = formattedText.Width;
                    double actualTextHeight = (formattedText.Height + formattedText.OverhangLeading + formattedText.OverhangAfter) - formattedText.OverhangTrailing;
                    if ((this.IgnoreParentWidth || actualTextWidth <= targetWidth) && actualTextHeight <= targetHeight)
                    {
                        // we've found our font size, use it
                        this.FontSize = fontSize;
                        this.MarqueeTextWidth = actualTextWidth;
                        ReleaseMarquee();
                        InitializeMarquee();
                        break;
                    }
                }
            }
        }

        /*
         * Marquee (scrolling text) animation management
         */

        private Storyboard MarqueeStoryboard;
        private double MarqueeTextWidth;

        private void InitializeMarquee()
		{
            // only init marquee if we're expecting to ignore the width
            if (this.IgnoreParentWidth)
            {
                // NOTE: you might ask yourself why i chose to do this in code, and not in the xaml
                //       it's because i hate xaml, and this is easier to read

                // we need the width of the parent canvas so we know how far to travel
                double parentWidth = (this.Parent as Canvas).ActualWidth;

                // how long should we wait after load to begin moving text
                TimeSpan initialDelay = TimeSpan.FromSeconds(2.5);

                // one second for every 100 total pixels of travel
                TimeSpan firstAnimLength = TimeSpan.FromSeconds(this.MarqueeTextWidth / 100.0);
                TimeSpan secondAnimLength = TimeSpan.FromSeconds((this.MarqueeTextWidth + parentWidth) / 100.0);

                // it's really stupid that this is needed, but otherwise, the position won't be reset
                DoubleAnimation resetAnimation = new DoubleAnimation()
                {
                    From = 0,
                    To = 0,
                    RepeatBehavior = new RepeatBehavior(1),
                    Duration = new Duration(TimeSpan.FromMilliseconds(10)),
                };

                // start at default position for the text and move left
                DoubleAnimation firstAnimation = new DoubleAnimation()
                {
                    From = 0,
                    To = -this.MarqueeTextWidth,
                    RepeatBehavior = new RepeatBehavior(1),
                    BeginTime = initialDelay,
                    Duration = new Duration(firstAnimLength),
                };

                // actually marquee and start far right, moving far left
                DoubleAnimation secondAnimation = new DoubleAnimation()
                {
                    From = (this.Parent as Canvas).ActualWidth,
                    To = -this.MarqueeTextWidth,
                    RepeatBehavior = RepeatBehavior.Forever,
                    BeginTime = initialDelay + firstAnimLength,
                    Duration = new Duration(secondAnimLength),
                };

                // assign animation targets to this label
                Storyboard.SetTarget(resetAnimation, this);
                Storyboard.SetTargetProperty(resetAnimation, new PropertyPath(Canvas.LeftProperty));
                Storyboard.SetTargetName(resetAnimation, "ResetAnimation");
                Storyboard.SetTarget(firstAnimation, this);
                Storyboard.SetTargetProperty(firstAnimation, new PropertyPath(Canvas.LeftProperty));
                Storyboard.SetTargetName(firstAnimation, "FirstAnimation");
                Storyboard.SetTarget(secondAnimation, this);
                Storyboard.SetTargetProperty(secondAnimation, new PropertyPath(Canvas.LeftProperty));
                Storyboard.SetTargetName(secondAnimation, "SecondAnimation");

                // initialize the storyboard and add all the children
                this.MarqueeStoryboard = new Storyboard() { Name = "MarqueeAnimation" };
                this.MarqueeStoryboard.Children.Add(resetAnimation);
                this.MarqueeStoryboard.Children.Add(firstAnimation);
                this.MarqueeStoryboard.Children.Add(secondAnimation);
                this.MarqueeStoryboard.Begin(this, true);
            }
        }

        private void ReleaseMarquee()
		{
            // AFAICT: this isn't needed and it adds a noticible jump.
            // this.MarqueeStoryboard?.Stop(this);
            // this.MarqueeStoryboard?.Remove(this);
        }
    }
}
