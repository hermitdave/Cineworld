using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Xna.Framework.Media;

namespace CineWorld
{
    public partial class PhotoViewer : PhoneApplicationPage
    {
        public static Uri photoUri { get; set; }

        BitmapImage bitmap;

        private bool _busy = false;

        private const double _maxWidth = 2048;
        private const double _maxHeight = 2048;

        private double _width = 0;
        private double _height = 0;

        private double _scale = 1.0;

        private bool _highQuality = false;

        private bool _pinching = false;

        private Point _relativeCenter;

        private bool _loaded = false;

        private bool Busy
        {
            get
            {
                return _busy;
            }

            set
            {
                if (_busy != value)
                {
                    _busy = value;

                    ProgressBar.Visibility = _busy ? Visibility.Visible : Visibility.Collapsed;
                    ProgressBar.IsIndeterminate = _busy;
                }
            }
        }

        private bool HighQuality
        {
            get
            {
                return _highQuality;
            }

            set
            {
                if (_highQuality != value)
                {
                    _highQuality = value;

                    if (_highQuality)
                    {
                        var task = RenderAsync();
                    }
                }
            }
        }

        public PhotoViewer()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.bitmap = new BitmapImage() { UriSource = photoUri, CreateOptions = BitmapCreateOptions.BackgroundCreation };
            this.bitmap.ImageOpened += bitmap_ImageOpened;
            
            //this.img.ImageOpened += img_ImageOpened;
            //this.img.Source = new BitmapImage() { UriSource = photoUri, CreateOptions = BitmapCreateOptions.BackgroundCreation };
        }

        async void bitmap_ImageOpened(object sender, RoutedEventArgs e)
        {
            await SetupAsync();
            await RenderAsync();
        }

        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            base.OnOrientationChanged(e);

            ConfigureViewport();
        }

        private void Viewport_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Image.Width < Viewport.ActualWidth)
            {
                ConfigureViewport();
            }
        }

        private async Task SetupAsync()
        {
            try
            {
                Windows.Foundation.Size dimensions = new Windows.Foundation.Size(this.bitmap.PixelWidth, this.bitmap.PixelHeight);

                double scale;

                if (dimensions.Width > dimensions.Height)
                {
                    scale = _maxWidth / dimensions.Width;
                }
                else
                {
                    scale = _maxHeight / dimensions.Height;
                }

                _width = dimensions.Width * scale;
                _height = dimensions.Height * scale;

                HighQuality = false;

                ConfigureViewport();
            }
            catch (Exception)
            {
            }
        }

        private async Task RenderAsync()
        {
            if (!Busy)
            {
                Busy = true;

                bool hq;

                do
                {
                    hq = _highQuality;

                    double w = hq ? _width : _width * _scale;
                    double h = hq ? _height : _height * _scale;

                    //WriteableBitmap writeableBitmap = new WriteableBitmap(this.bitmap);
                    //writeableBitmap.SetSource(this.bitmap);
                    //await App.PhotoModel.RenderBitmapAsync(writeableBitmap);
                    Image.Width = w;
                    Image.Height = h;
                    Image.Source = this.bitmap;
                }
                while (hq != _highQuality);

                Busy = false;
            }
        }

        private void ConfigureViewport()
        {
            if (_width < _height)
            {
                _scale = Viewport.ActualHeight / _height;
            }
            else
            {
                _scale = Viewport.ActualWidth / _width;
            }

            Image.Width = _width * _scale;
            Image.Height = _height * _scale;

            Viewport.Bounds = new Rect(0, 0, Image.Width, Image.Height);
            Viewport.SetViewportOrigin(new Point(
                Image.Width / 2 - Viewport.Viewport.Width / 2,
                Image.Height / 2 - Viewport.Viewport.Height / 2));
        }

        private void Viewport_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            if (_pinching)
            {
                e.Handled = true;

                CompletePinching();
            }
        }

        private void Viewport_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (e.PinchManipulation != null)
            {
                e.Handled = true;

                if (!_pinching)
                {
                    _pinching = true;

                    _relativeCenter = new Point(
                        e.PinchManipulation.Original.Center.X / Image.Width,
                        e.PinchManipulation.Original.Center.Y / Image.Height);
                }

                //System.Diagnostics.Debug.WriteLine("X={0} Y={1}", e.PinchManipulation.Original.Center.X, e.PinchManipulation.Original.Center.Y);

                double w, h;

                if (_width < _height)
                {
                    h = _height * _scale * e.PinchManipulation.CumulativeScale;
                    h = Math.Max(Viewport.ActualHeight, h);
                    h = Math.Min(h, _height);

                    w = h * _width / _height;
                }
                else
                {
                    w = _width * _scale * e.PinchManipulation.CumulativeScale;
                    w = Math.Max(Viewport.ActualWidth, w);
                    w = Math.Min(w, _width);

                    h = w * _height / _width;
                }

                Image.Width = w;
                Image.Height = h;

                Viewport.Bounds = new Rect(0, 0, w, h);

                GeneralTransform transform = Image.TransformToVisual(Viewport);
                Point p = transform.Transform(e.PinchManipulation.Original.Center);

                double x = _relativeCenter.X * w - p.X;
                double y = _relativeCenter.Y * h - p.Y;

                if (w < _width && h < _height)
                {
                    Viewport.SetViewportOrigin(new Point(x, y));
                }
            }
            else if (_pinching)
            {
                e.Handled = true;

                CompletePinching();
            }
        }

        private void Viewport_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (_pinching)
            {
                e.Handled = true;

                CompletePinching();
            }
        }

        private void CompletePinching()
        {
            _pinching = false;

            double sw = Image.Width / _width;
            double sh = Image.Height / _height;

            _scale = Math.Min(sw, sh);

            WriteableBitmap bitmap = Image.Source as WriteableBitmap;

            if (Image.Width > _width / 2)
            {
                HighQuality = true;
            }
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            if(this.bitmap != null)
            {
                WriteableBitmap wb = new WriteableBitmap(this.bitmap);
                wb.SaveToMediaLibrary(System.IO.Path.GetFileName(photoUri.OriginalString));

                this.ApplicationBar.IsVisible = false;
            }
        }

    }
}