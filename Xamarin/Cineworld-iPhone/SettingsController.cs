using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using System.Collections.Generic;
using Cineworld;

namespace CineworldiPhone
{
	partial class SettingsController : UIViewController
	{
		public Cineworld_iPhoneViewController MainViewController { get; set; }

		List<string> regions = null;

		public SettingsController (IntPtr handle) : base (handle)
		{
			regions = new List<string> ();
			regions.Add ("United Kingdom");
			regions.Add ("Ireland");
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			RegionPickerViewModel regionPickerVM = new RegionPickerViewModel (this.regions,  MainViewController);

			this.RegionPicker.Model = regionPickerVM;

			this.RegionPicker.Select (Config.Region == Config.RegionDef.UK ? 0 : 1, 0, true);

			regionPickerVM.InitialSelected = true;
		}

//		public override void ViewDidAppear (bool animated)
//		{
//			var vm = this.RegionPicker.Model as RegionPickerViewModel;
//
//			vm.Selected (null, Config.Region == Config.RegionDef.UK ? 0 : 1, 0);
//
//			vm.InitialSelected = true;
//
//			this.RegionPicker.ReloadAllComponents ();
//		}
	}
}
