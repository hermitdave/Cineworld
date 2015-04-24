using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V4.View;
using Android.Util;

namespace Cineworld_Android
{
	[Activity(Label = "CinemasActivity", ParentActivity = typeof(MainActivity))]
    [MetaData("android.support.PARENT_ACTIVITY", Value = ".MainActivity")]
	public class CinemasActivity : BaseActivity
    {
		protected override int LayoutResource
        {
			get { return Resource.Layout.cinemas; }
        }
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

			this.SupportActionBar.Title = "Cinames";


			//tabs.OnTabReselectedListener = this;
			//tabs.OnPageChangeListener = this;

			//int count = Intent.GetIntExtra("clicks", 0);
            //var text = FindViewById<TextView>(Resource.Id.textView1);
            //text.Text = string.Format("You clicked {0} times!", count);
        }

//		
    }
}