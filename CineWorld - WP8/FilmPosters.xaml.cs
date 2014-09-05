using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections.ObjectModel;
using CineWorld;
using System.Windows.Media;
using Microsoft.Xna.Framework.Media;
using Windows.Phone.System.UserProfile;
using System.IO;
using System.Windows.Media.Imaging;
using WP8_BackgroundTask;
using System.IO.IsolatedStorage;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using CineWorld.ViewModels;
using Windows.Storage;

namespace Cineworld
{
    public partial class FilmPosters : PhoneApplicationPage
    {
        private FilmPosterViewModel posterViewModel = new FilmPosterViewModel();

        MediaLibrary mediaLib = new MediaLibrary();

        public FilmPosters()
        {
            InitializeComponent();

            this.DataContext = this.posterViewModel;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!Config.ShowCleanBackground)
            {
                this.LayoutRoot.Background = new ImageBrush()
                {
                    ImageSource = new BitmapImage(new Uri("SplashScreenImage-WVGA.jpg", UriKind.Relative)),
                    Opacity = 0.2,
                    Stretch = Stretch.UniformToFill
                };
            }

            await this.posterViewModel.LoadPosters();
        }

        private void AllowedImage_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Image img = sender as Image;
            StackPanel gParent = img.Parent as StackPanel;

            ContextMenu contextMenu = new ContextMenu();
            contextMenu.Tag = img.Tag;

            MenuItem miLockscreen = new MenuItem() { Background = Application.Current.Resources["PhoneAccentBrush"] as SolidColorBrush, Foreground = new SolidColorBrush(Colors.White), Header = "set as lockscreen" };
            miLockscreen.Click += miLockscreen_Click;
            contextMenu.Items.Add(miLockscreen);

            MenuItem miSave = new MenuItem() { Background = Application.Current.Resources["PhoneAccentBrush"] as SolidColorBrush, Foreground = new SolidColorBrush(Colors.White), Header = "save to pictures hub" };
            miSave.Click += miSave_Click;
            contextMenu.Items.Add(miSave);

            MenuItem miIgnore = new MenuItem() { Background = Application.Current.Resources["PhoneAccentBrush"] as SolidColorBrush, Foreground = new SolidColorBrush(Colors.White), Header = "ignore" };
            miIgnore.Click += miIgnore_Click;
            contextMenu.Items.Add(miIgnore);

            ContextMenuService.SetContextMenu(gParent, contextMenu);
            contextMenu.IsOpen = true;
        }

        async void miLockscreen_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            if (mi != null)
            {
                ContextMenu cm = mi.Parent as ContextMenu;

                FilmInfo fi = (FilmInfo)cm.Tag;

                if (Config.AnimateLockscreen && !LockScreenManager.IsProvidedByCurrentApplication)
                {
                    // If you're not the provider, this call will prompt the user for permission.
                    // Calling RequestAccessAsync from a background agent is not allowed.
                    await LockScreenManager.RequestAccessAsync();
                }

                Config.AnimateLockscreen = LockScreenManager.IsProvidedByCurrentApplication;

                await ScheduledAgent.SetPoster(fi.MediumPosterUrl);
            }
        }

        void miIgnore_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            if (mi != null)
            {
                ContextMenu cm = mi.Parent as ContextMenu;

                FilmInfo fi = (FilmInfo)cm.Tag;

                this.posterViewModel.IgnoreFilm(fi);
            }
        }

        async void miSave_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            if (mi != null)
            {
                ContextMenu cm = mi.Parent as ContextMenu;

                FilmInfo fi = (FilmInfo)cm.Tag;

                string original = fi.MediumPosterUrl.OriginalString.Replace("w500", "original");

                using (MemoryStream stream = new MemoryStream())
                {
                    await new AsyncWebClient().SavePictureLocally(original, stream);

                    stream.Position = 0;

                    string filename = Path.GetFileName(original);

                    mediaLib.SavePicture(filename, stream);
                }

                //StorageFolder folder = KnownFolders.SavedPictures;
                //StorageFile file = await folder.CreateFileAsync(String.Format("{0}.jpg", fi.Title), CreationCollisionOption.ReplaceExisting);
                //using (Stream s = await file.OpenStreamForWriteAsync())
                //{
                //    s.Write(data, 0, data.Length);
                //}

                
            }
        }

        private void IgnoredImage_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Image img = sender as Image;
            StackPanel gParent = img.Parent as StackPanel;

            ContextMenu contextMenu = new ContextMenu();
            contextMenu.Tag = img.Tag;

            MenuItem miAllow = new MenuItem() { Background = Application.Current.Resources["PhoneAccentBrush"] as SolidColorBrush, Foreground = new SolidColorBrush(Colors.White), Header = "allow" };
            miAllow.Click += miAllow_Click;
            contextMenu.Items.Add(miAllow);

            ContextMenuService.SetContextMenu(gParent, contextMenu);
            contextMenu.IsOpen = true;
        }

        void miAllow_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            if (mi != null)
            {
                ContextMenu cm = mi.Parent as ContextMenu;

                FilmInfo fi = (FilmInfo)cm.Tag;

                this.posterViewModel.AllowFilm(fi);
            }
        }
    }
}