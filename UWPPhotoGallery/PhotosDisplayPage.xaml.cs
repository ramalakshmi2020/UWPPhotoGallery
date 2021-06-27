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
    public sealed partial class PhotosDisplayPage : Page
    {
        public ObservableCollection<Photo> Photos;

        private bool isSelectButtonClicked = false;

        private string context;
        public PhotosDisplayPage()
        {
            this.InitializeComponent();
            Photos = new ObservableCollection<Photo>();
            //this.Loaded += PhotosDisplayPage_Loaded;
        }

        /// <summary>
        /// This method will actually query for all the pictures present in the my pictures folder and display it to the user
        /// We have subscribed for this because the loading of pics is async and cannot be done in the constructor of the page.
        /// If there are no pictures it will display a pop up box asking the user to add pics to the folder.
        /// Added context tot he method - the page is used to display all photos or photos pertaining to an album with the albumname contexr
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        
        async protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            //now do whatever that needs to be done
            LoadingRing.IsActive = true;
            context = e.Parameter as string;
            //now if there is no context - ( i.e all photos should be loaded)
            //based on the context either load all photos or the photos of an album
            bool loaded = false;
            if (String.IsNullOrEmpty(context) || context == "Photos")
            {
                DisplayTypeTextBlock.Text = "Photos";
                //No need for edit button here in this context
                EditAlbumButton.Visibility = Visibility.Collapsed;
                //enable select button
                SelectPhotosButton.Visibility = Visibility.Visible;
                loaded = await PhotoManager.GetAllPhotos(Photos);
            }
            else
            {
                DisplayTypeTextBlock.Text = context;
                EditAlbumButton.Visibility = Visibility.Visible;
                SelectPhotosButton.Visibility = Visibility.Collapsed;

                loaded = PhotoManager.GetPhotosForAlbumName(Photos, context);
                
            }
             
            //if retval is false and photos collection is empty - tell the user something is wrong
            if(loaded && Photos.Count == 0)
            {
                //there are no pictures to display 
                //prompt the user to use the menu option to add pictures
                var messageDialog = new MessageDialog("No Photos found in PicturesLibrary. Use Add Option of AllPhotos menu to add pictures to the collection");
                messageDialog.Commands.Add(new UICommand("OK"));
                await messageDialog.ShowAsync();

            } else if (!loaded){
                //something went wrong - tell the user to try again
                var messageDialog = new MessageDialog("Something went wrong, Contact App Support");
                messageDialog.Commands.Add(new UICommand("OK"));
                await messageDialog.ShowAsync();

            }
            LoadingRing.IsActive = false;

        }

        private void PhotoImage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!isSelectButtonClicked)
            {
                //disable edit button
                EditAlbumButton.Visibility = Visibility.Collapsed;
                DisplayTypeTextBlock.Visibility = Visibility.Collapsed;
                //on tapping this image, load the flipview control with the cureent image as the default one
                Photo selectedphoto = PhotoGrid.SelectedItem as Photo;
                PhotoFlipView.SelectedItem = selectedphoto;
                //enable flipview

                PhotoFlipView.Visibility = Visibility.Visible;
            }

        }
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            //just load the addalbum page with mode as edit
            string context = DisplayTypeTextBlock.Text;
            this.Frame.Navigate(typeof(AddNewAlbumPage), context);

        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var messageDialog = new MessageDialog("About to delete Photo(s). Are you sure?");
            //create list of command and handlers
            messageDialog.Commands.Add(new UICommand("OK", new UICommandInvokedHandler(this.DeleteOK)));
            messageDialog.Commands.Add(new UICommand("Cancel", new UICommandInvokedHandler(this.DeleteCancel)));
            await messageDialog.ShowAsync();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            //load the same page again with 
            this.Frame.Navigate(typeof(PhotosDisplayPage));
        }

        private void SelectPhotosButton_Click(object sender, RoutedEventArgs e)
        {
            //select is clicked - make the photogrid multiselectmode and enable delete and cancelbuttons
            isSelectButtonClicked = true;
            PhotoGrid.SelectionMode = ListViewSelectionMode.Multiple;
            PhotoGrid.IsMultiSelectCheckBoxEnabled = true;
            DeleteButton.Visibility = Visibility.Visible;
            CancelButton.Visibility = Visibility.Visible;
            SelectPhotosButton.Visibility = Visibility.Collapsed;


        }

        private async void DeleteOK(IUICommand command)
        {
            var selecteditems = PhotoGrid.SelectedItems.ToList();
            List<Photo> TodeletePhotos = new List<Photo>();
            foreach (Photo al in selecteditems) { TodeletePhotos.Add(al); }
            bool retval = await PhotoManager.DeletePhotos(TodeletePhotos);
            if (!retval)
            {
                //display message to the user and move on to display the page
                var messageDialog = new MessageDialog("OOPS! Something happened, Unable to delete Photo(s). Please Try Again or Contact App Support");
                messageDialog.Commands.Add(new UICommand("OK"));
                await messageDialog.ShowAsync();
            }
            this.Frame.Navigate(typeof(PhotosDisplayPage));

        }

        private void DeleteCancel(IUICommand command)
        {
            this.Frame.Navigate(typeof(PhotosDisplayPage));
        }
    }
}
