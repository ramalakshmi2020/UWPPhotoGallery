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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UWPPhotoGallery
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddNewAlbumPage : Page
    {
        /// <summary>
        /// Collection of all photos
        /// </summary>
        public ObservableCollection<Photo> PhotoCollection;

        private string context;

        /// <summary>
        /// Collection of photos selected by user
        /// </summary>
        public ObservableCollection<Photo> SelectedPhotos;
        public AddNewAlbumPage()
        {
            this.InitializeComponent();
            PhotoCollection = new ObservableCollection<Photo>();
            SelectedPhotos = new ObservableCollection<Photo>();
        }

        async protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //load the collection with the photos from photomanager
            await PhotoManager.GetAllPhotos(PhotoCollection);
            // if ther is a context then that needs to be taken care too
            context = e.Parameter as string;
            bool retval = true;
            if (!String.IsNullOrEmpty(context))
            {
              
                AlbumName.Text = context;
                
                //the photo is in edit mode - get the seletec photos for this album name and keep it updated
                retval = PhotoManager.GetPhotosForAlbumName(SelectedPhotos, context);
                // set the selection of the 
                //create another list and copy it 
                if (retval)
                {
                    // let me create a new list and try
                    List<Photo> selphotos = new List<Photo>();
                    foreach (Photo p in SelectedPhotos)
                    {
                        selphotos.Add(p);
                    }
                    //set the item source again
                    PhotoSelectionGrid.ItemsSource = PhotoCollection;
                    foreach (Photo pic in selphotos)
                    {
                        PhotoSelectionGrid.SelectedItems.Add(pic);
                    }
                }
                else
                {
                    //display error message to user and go back to albums page
                    var messageDialog = new MessageDialog("OOPS Something went wrong, Try Again or Contact App Support");
                    messageDialog.Commands.Add(new UICommand("OK"));
                    await messageDialog.ShowAsync();
                    this.Frame.Navigate(typeof(AlbumsPage));

                }
            
            }
        }
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Photo coverphoto = PhotoSelectedGrid.SelectedItem as Photo;

            if (coverphoto == null)
            {
                //pick any image from the seelcted photos
                coverphoto = SelectedPhotos[0];
            }
            //album name
            string albumName = AlbumName.Text;
           
            //add this album
            Album newalbum = new Album
            {
                CoverPhotoFilePath = coverphoto.FilePath,
                Name = albumName,
                CoverPhotoThumbnail = (BitmapImage)coverphoto.Thumbnail


            };

            bool retval = true;
            //based on the context either add new album or modify
            retval = await PhotoManager.AddModifyAlbum(context, newalbum, SelectedPhotos);
            if (!retval)
            {
                //display error message to user and go back to albums page
                var messageDialog = new MessageDialog("OOPS Something went wrong, Try Again or Contact App Support");
                messageDialog.Commands.Add(new UICommand("OK"));
                await messageDialog.ShowAsync();

            }
            
            this.Frame.Navigate(typeof(AlbumsPage));

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            //simply load the albums page
            if (String.IsNullOrEmpty(context))
            {
                //load the albums page
                this.Frame.Navigate(typeof(AlbumsPage));
            }else
            {
                //load the photo display page witht he context
                this.Frame.Navigate(typeof(PhotosDisplayPage), context);
            }
           
        }

        private void PhotoSelectionGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //now update the grid on the other side with the selected photos and allow the user to complete the operation
            //just enbale add button
            SaveButton.Visibility = Visibility.Visible;
            AlbumName.Visibility = Visibility.Visible;
            CoverPhotoMsg.Visibility = Visibility.Visible;
            MsgTextBlock.Visibility = Visibility.Visible;
            var photos = PhotoSelectionGrid.SelectedItems.ToList();
            SelectedPhotos.Clear();
            foreach (Photo p in photos) { SelectedPhotos.Add(p); }
        }

        private void SelectedPhoto_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //just show the flyout
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }
    }
}
