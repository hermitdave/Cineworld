using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using CineWorld.ViewModels;
using Cineworld;

namespace CineWorld
{
    public partial class Search : PhoneApplicationPage
    {
        public SearchViewModel viewModel = null;

        public Search()
        {
            InitializeComponent();

            viewModel = new SearchViewModel();
            this.DataContext = this.viewModel;

            this.viewModel.PropertyChanged += viewModel_PropertyChanged;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!this.NavigationContext.QueryString.ContainsKey("Find"))
            {
                this.ptbSearch.Focus();
                return;
            }

            string search = this.NavigationContext.QueryString["Find"];
                    
            bool bError = false;
            try
            {
                if (App.Cinemas == null || App.Cinemas.Count == 0)
                {
                    LocalStorageHelper lsh = new LocalStorageHelper();
                    await lsh.DownloadFiles(false);

                    await lsh.DeserialiseObjects();
                }
            }
            catch(Exception ex)
            {
                bError = true;
            }

            if (bError)
            {
                MessageBox.Show("Error fetching details");
            }

            this.ptbSearch.Text = search;

            this.ExecuteSearch();
        }

        void viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SearchResults":
                    this.Focus();
                    break;
            }
        }

        private void PhoneTextBox_ActionIconTapped(object sender, EventArgs e)
        {
            ExecuteSearch();
        }

        private void ExecuteSearch()
        {
            if (String.IsNullOrWhiteSpace(this.ptbSearch.Text))
                return;

            this.viewModel.SearchString = this.ptbSearch.Text;

            this.viewModel.SearchAsync();
        }

        private void Grid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Grid g = sender as Grid;

            this.viewModel.HandleUserSelection(g.Tag, this.NavigationService);
        }

        private void ptbSearch_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                this.ExecuteSearch();
        }
    }
}