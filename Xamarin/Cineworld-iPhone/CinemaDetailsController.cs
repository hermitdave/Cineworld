using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace CineworldiPhone
{
	partial class CinemaDetailsController : UIViewController
	{
		public CinemaDetailsController (IntPtr handle) : base (handle)
		{
		}

		public CinemaInfo Cinema { get; set; }
	}
}
