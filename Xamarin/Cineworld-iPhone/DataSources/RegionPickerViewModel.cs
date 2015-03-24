using System;
using System.Collections.Generic;
using Cineworld;

namespace CineworldiPhone
{
	public class RegionPickerViewModel : ListPickerViewModel<string>
	{
		Cineworld_iPhoneViewController MainVC;

		public bool InitialSelected { get; set; }

		public RegionPickerViewModel (List<string> regions, Cineworld_iPhoneViewController vc) : base(regions)
		{
			this.InitialSelected = false;
			this.MainVC = vc;

			//this.Selected (null, Config.Region == Config.RegionDef.UK ? 0 : 1, 0);
		}

		public override void Selected (UIKit.UIPickerView pickerView, nint row, nint component)
		{
			base.Selected (pickerView, row, component);

			if (!InitialSelected) 
			{
				return;
			}

			var selectedVal = base.SelectedItem;

			Config.RegionDef region = selectedVal.Equals ("Ireland") ? Config.RegionDef.Ireland : Config.RegionDef.UK;

			if (region != Config.Region) 
			{
				Config.Region = region;

				this.MainVC.RegionSelectionComplete (true);
				return;
			}

			this.MainVC.RegionSelectionComplete (false);
		}
	}
}

