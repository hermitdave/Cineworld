using System;
using Android.Support.V4.App;
using System.Collections.Generic;

namespace Cineworld_Android
{
	public class CinemaListPagerAdapter : FragmentPagerAdapter
	{
		ICollection<CinemaInfo> _cinemas;
		Fragment _listFragment;
		Fragment _mapFragment;

		public CinemaListPagerAdapter (Android.Support.V4.App.FragmentManager fm, ICollection<CinemaInfo> cinemas) : base (fm)
		{
			this._cinemas = cinemas;
		}

		#region implemented abstract members of PagerAdapter

		public override int Count {
			get {
				return this._cinemas.Count;
			}
		}

		#endregion

		#region implemented abstract members of FragmentPagerAdapter



		public override Fragment GetItem (int position)
		{
			return null;
//			if (position == 0) {
//				if (this._listFragment == null) {
//					this._listFragment = new CinemaListFragment (this._cinemas);
//				}
//
//				return this._listFragment;
//			} else {
//				if (this._mapFragment == null) {
//					this._mapFragment = new CinemaMapFragment (this._cinemas);
//				}
//
//				return this._mapFragment;
//			}
		}

		#endregion
	}
}

