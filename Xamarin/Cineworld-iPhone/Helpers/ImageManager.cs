﻿using System;
using MonoTouch.UrlImageStore;
using UIKit;

namespace CineworldiPhone
{
	public class ImageManager : IUrlImageUpdated
	{
		public delegate void ImageLoadedDelegate(string id, UIImage image);
		public event ImageLoadedDelegate ImageLoaded;
		UrlImageStore imageStore;

		private ImageManager()
		{
			imageStore = new UrlImageStore ("myImageStore", processImage);						            
		}

		private static ImageManager instance;

		public static ImageManager Instance
		{
			get
			{
				if (instance == null)
					instance = new ImageManager ();

				return instance;
			}	
		}

		// this is the actual entrypoint you call
		public UIImage GetImage(string imageUrl)
		{
			if (String.IsNullOrWhiteSpace (imageUrl))
				return null;

			return imageStore.RequestImage (imageUrl, imageUrl, this);
		}

		public void UrlImageUpdated (string id, UIImage image)
		{
			// just propagate to upper level
			if (this.ImageLoaded != null)
				this.ImageLoaded(id, image);
		}

		// This handles our ProcessImageDelegate
		// just a simple way for us to be able to do whatever we want to our image
		// before it gets cached, so here's where you want to resize, etc.
		UIImage processImage(string id, UIImage image)
		{
			return image;
		}		
	}
}

