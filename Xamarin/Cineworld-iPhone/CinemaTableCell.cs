using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace CineworldiPhone
{
	partial class CinemaTableCell : UITableViewCell
	{
		public CinemaTableCell (IntPtr handle) : base (handle)
		{
		}

		public CinemaInfo Cinema { get; private set; }

		public void UpdateCell(CinemaInfo cinema)
		{
			this.TextLabel.Text = cinema.Name;

			this.Cinema = cinema;
		}
	}
}
