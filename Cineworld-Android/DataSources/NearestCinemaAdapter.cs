using System;
using Android.Widget;
using System.Collections.Generic;
using Android.Content;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Graphics;
using Cineworld;
using Android.Views;

namespace Cineworld_Android
{
	public class NearestCinemaAdapter : BaseAdapter
	{
		List<CinemaInfo> _cinemas = null;
		Context _context = null;
		Drawable _backgroundImage = null;
 
		public NearestCinemaAdapter (Context context, IEnumerable<CinemaInfo> cinemas, Drawable drawable)
		{
			this._context = context;
			this._cinemas = new List<CinemaInfo>(cinemas);
			this._backgroundImage = drawable;
		}

		#region implemented abstract members of BaseAdapter

		public override Java.Lang.Object GetItem (int position)
		{
			return null;
		}

		public override long GetItemId (int position)
		{
			return 0;
		}

		public override Android.Views.View GetView (int position, Android.Views.View convertView, Android.Views.ViewGroup parent)
		{
			View itemView = convertView ?? LayoutInflater.From(_context).Inflate(Resource.Layout.NearestCinemaButton, parent, false);

			Button btn = itemView as Button;

			var cinema = this._cinemas [position];

			if (CineworldApplication.UserLocation != null) 
			{
				double d = GeoMath.Distance (CineworldApplication.UserLocation.Latitude, CineworldApplication.UserLocation.Longitude, cinema.Latitude, cinema.Longitute, Config.Region == Config.RegionDef.UK ? GeoMath.MeasureUnits.Miles : GeoMath.MeasureUnits.Kilometers);
				btn.Text = String.Format("{0}\n{1:N1} {2}", cinema.Name, d, Config.Region == Config.RegionDef.UK ? "miles" : "kilometers");
			} else 
			{
				btn.Text = cinema.Name;
			}

			btn.Tag = cinema.ID;
			//btn.Alpha = 0.7f;

			return btn;
		}

		public override int Count 
		{
			get 
			{
				return this._cinemas.Count;
			}
		}

		#endregion
	}
}

