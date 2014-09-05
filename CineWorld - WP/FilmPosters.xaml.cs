using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Cineworld;
using Newtonsoft.Json.Linq;
using WP7Helpers.Common;
using Microsoft.Xna.Framework.Media;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;

namespace CineWorld
{
    public partial class FilmPosters : PhoneApplicationPage
    {
        ObservableCollection<FilmInfo> AllowFilms = new ObservableCollection<FilmInfo>();
        ObservableCollection<FilmInfo> IgnoredFilms = new ObservableCollection<FilmInfo>();
        
        public FilmPosters()
        {
            InitializeComponent();

            this.lstAllowed.ItemsSource = AllowFilms;
            this.lstIgnored.ItemsSource = IgnoredFilms;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            LoadPosters();
        }

        private void LoadPosters()
        {
            HashSet<int> ignored = new HashSet<int>(Config.FilmPostersToIgnore);

            HashSet<string> uniqueposters = new HashSet<string>();

            this.AllowFilms.Clear();
            this.IgnoredFilms.Clear();

            foreach (var item in App.Films.Values)
            {
                if (item.MediumPosterUrl == null)
                    continue;

                if (uniqueposters.Contains(item.MediumPosterUrl.OriginalString))
                    continue;

                uniqueposters.Add(item.MediumPosterUrl.OriginalString);

                if (ignored.Contains(item.EDI))
                    IgnoredFilms.Add(item);
                else
                    AllowFilms.Add(item);
            }

        }

        private void AllowedImage_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Image img = sender as Image;
            Grid gParent = img.Parent as Grid;

            ContextMenu contextMenu = new ContextMenu();
            contextMenu.Tag = img.Tag;

            MenuItem miSave = new MenuItem() { Background = Application.Current.Resources["PhoneAccentBrush"] as SolidColorBrush, Foreground = new SolidColorBrush(Colors.White), Header = "save to pictures hub" };
            miSave.Click += miSave_Click;
            contextMenu.Items.Add(miSave);

            MenuItem miIgnore = new MenuItem() { Background = Application.Current.Resources["PhoneAccentBrush"] as SolidColorBrush, Foreground = new SolidColorBrush(Colors.White), Header = "ignore" };
            miIgnore.Click += miIgnore_Click;
            contextMenu.Items.Add(miIgnore);

            ContextMenuService.SetContextMenu(gParent, contextMenu);
            contextMenu.IsOpen = true;
        }

        void miIgnore_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            if (mi != null)
            {
                ContextMenu cm = mi.Parent as ContextMenu;

                FilmInfo fi = (FilmInfo)cm.Tag;

                IEnumerable<FilmInfo> filmsToIgnore = App.Films.Values.Where(f => f.MediumPosterUrl != null && String.Compare(fi.MediumPosterUrl.OriginalString, f.MediumPosterUrl.OriginalString, StringComparison.OrdinalIgnoreCase) == 0);

                if (filmsToIgnore != null)
                {
                    List<int> films = new List<int>(Config.FilmPostersToIgnore);
                    foreach (var film in filmsToIgnore)
                        films.Add(film.EDI);

                    Config.FilmPostersToIgnore = films;
                }

                this.LoadPosters();
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

                byte[] data = await new AsyncWebClient().SavePictureLocally(original);

                new MediaLibrary().SavePicture(fi.Title, data);
            }
        }

        private void IgnoredImage_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Image img = sender as Image;
            Grid gParent = img.Parent as Grid;

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

                IEnumerable<FilmInfo> filmsToIgnore = App.Films.Values.Where(f => f.MediumPosterUrl != null && String.Compare(fi.MediumPosterUrl.OriginalString, f.MediumPosterUrl.OriginalString, StringComparison.OrdinalIgnoreCase) == 0);

                if (filmsToIgnore != null)
                {
                    List<int> films = new List<int>(Config.FilmPostersToIgnore);
                    foreach (var film in filmsToIgnore)
                        films.Remove(film.EDI);

                    Config.FilmPostersToIgnore = films;
                }

                this.LoadPosters();
            }
        }
    }
}