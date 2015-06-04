using System;
using UIKit;
using CoreGraphics;

namespace CineworldiPhone
{
	public class PerformanceButton : UIButton
	{
		static UIColor tintColor;
		static PerformanceButton()
		{
			tintColor = UIColor.FromRGB(0, 122, 255);
		}

		public PerformanceInfo Performance { get; set; }

		public PerformanceButton (CGRect frame, PerformanceInfo perf) : base(UIButtonType.System)
		{
			this.Frame = frame;

			this.Layer.CornerRadius = 5f;
			this.Layer.MasksToBounds = true;
			this.Layer.RasterizationScale = UIScreen.MainScreen.Scale;
			this.Layer.Opaque = true;

			//this.BackgroundColor = UIColor.White;

			var topLabelRect = new CGRect (0, 3, frame.Width, (frame.Height / 2));

			var bottomLabelRect = new CGRect (0, frame.Height / 2, frame.Width, (frame.Height / 2) - 3);


			UILabel time = new UILabel (topLabelRect);
			time.Font = UIFont.FromName ("HelveticaNeue-Bold", 11f);
			time.Text = perf.TimeString;
			time.MinimumFontSize = 11f;
			time.TextAlignment = UITextAlignment.Center;
			this.AddSubview (time);

			UILabel type = new UILabel (bottomLabelRect);
			type.Font = UIFont.FromName ("HelveticaNeue", 10f);
			type.Text = perf.Type;
			type.MinimumFontSize = 10f;
			type.TextAlignment = UITextAlignment.Center;
			this.AddSubview (type);

			if (perf.AvailableFuture) 
			{
				this.BackgroundColor = UIColor.White;

				time.TextColor = type.TextColor = tintColor;

				this.Layer.BorderColor = tintColor.CGColor;
				this.Layer.BorderWidth = 1f;

				this.SetImage (Application.AvailableImagePressed, UIControlState.Highlighted);
			} 
			else
			{
				this.BackgroundColor = UIColor.LightGray;

				time.TextColor = UIColor.White;
				type.TextColor = UIColor.White;
			}


			//this.SetImage (Application.AvailableImageDefault, UIControlState.Normal);
			//this.SetImage (Application.UnavailableImage, UIControlState.Disabled);

			//this.Enabled = perf.AvailableFuture;

			this.Performance = perf;
		}
	}
}

