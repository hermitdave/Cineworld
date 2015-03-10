using System;
using UIKit;
using Foundation;
using System.Threading.Tasks;

namespace CineworldiPhone
{
	public class ImageHelper
	{
		public ImageHelper ()
		{
		}

		public static async Task<UIImage> ImageFromUrl(string uri)
		{
			UIImage img = null;

			await Task.Run (() => {
				using (var url = new NSUrl (uri)) {
					using (var data = NSData.FromUrl (url)) {
						img = UIImage.LoadFromData (data);
					}
				}
			});

			return img;
		}
	}
}

