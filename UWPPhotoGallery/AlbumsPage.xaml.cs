using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UWPPhotoGallery.Model;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UWPPhotoGallery
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AlbumsPage : Page
    {
        public ObservableCollection<Album> Albums;

        private bool isselectbuttonclicked = false;
        public AlbumsPage()
        {
            this.InitializeComponent();
            Albums = new ObservableCollection<Album>();
        }

        /// <summary>
        /// This method will be async since it needs to read albums from files and display the photos that correspond only to that album
        /// </summary>
        /// <param name="e"></param>
        async protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // now do stuff to populate the grid
            bool retval = await PhotoManager.GetAllAlbums(Albums);
            if (retval && Albums.Count == 0)
            {

                //there are no pictures to display 
                //prompt the user to use the menu option to add pictures
                var messageDialog = new MessageDialog("No Albums to display. Use Add Option of Albums menu to add new albums");
                messageDialog.Commands.Add(new UICommand("OK"));
                await messageDialog.ShowAsync();

            }
            else if (!retval)
            {
                //something went wrong - tell the user to try again
                var messageDialog = new MessageDialog("OOPS!! Something went wrong, unable to load albums, Try Again or Contact App support");
                messageDialog.Commands.Add(new UICommand("OK"));
                await messageDialog.ShowAsync();

            }
            else
            {
                //everything went on well
                SelectAlbum.Visibility = Visibility.Visible;

            }
            
        }

       

        private void AlbumGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!isselectbuttonclicked)
            {
                // need to navigate to the page to display the photos of the album that is selected
                //set albumname as teh context
                Album selectedalbum = AlbumGrid.SelectedItem as Album;
                //set the context as album name to the photo display page
                this.Frame.Navigate(typeof(PhotosDisplayPage), selectedalbum.Name);
            }

        }

        private void SelectAlbum_Click(object sender, RoutedEventArgs e)
        {
            //enable the selection mode in the grid
            //AlbumGrid.SelectionMode = SelectionMode.Multiple;
            //enable delete and cancel buttons
            
            isselectbuttonclicked = true;
            DeleteAlbum.Visibility = Visibility.Visible;
            Cancel.Visibility = Visibility.Visible;
            SelectAlbum.Visibility = Visibility.Collapsed;
            AlbumGrid.SelectionMode = ListViewSelectionMode.Multiple;
            AlbumGrid.IsMultiSelectCheckBoxEnabled = true;

        }

        
        private async void DeleteAlbum_Click(object sender, RoutedEventArgs e)
        {
            //ask the user if he is sure of deleting the album
            var messageDialog = new MessageDialog("About to delete Album(s). Are you sure?");
            //create list of command and handlers
            messageDialog.Commands.Add(new UICommand("OK", new UICommandInvokedHandler(this.DeleteOK)));
            messageDialog.Commands.Add(new UICommand("Cancel", new UICommandInvokedHandler(this.DeleteCancel)));
            await messageDialog.ShowAsync();
            
        }

        private void DeleteOK(IUICommand command)
        {
            var selecteditems = AlbumGrid.SelectedItems.ToList();
            List<Album> TodeleteAlbums = new List<Album>();
            foreach (Album al in selecteditems) { TodeleteAlbums.Add(al); }
            PhotoManager.DeleteAlbums(TodeleteAlbums);
            this.Frame.Navigate(typeof(AlbumsPage));

        }

        private void DeleteCancel(IUICommand command)
        {
            this.Frame.Navigate(typeof(AlbumsPage));
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AlbumsPage));
        }
    }
}
