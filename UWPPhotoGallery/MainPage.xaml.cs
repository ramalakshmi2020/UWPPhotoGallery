using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWPPhotoGallery
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {

            string context = "Photos";
            //by default always display the photos collection
            this.MainFrame.Navigate(typeof(PhotosDisplayPage), context);
        }

        private void PhotosMenuItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            

        }

        private async void PhotosNewFlyout_Click(object sender, RoutedEventArgs e)
        {
            //The user wants to add a few photos from other places to this collection in pictures library
            await PhotoManager.AddNewPhoto();
            //reload the page to reflect the change
            
            this.MainFrame.Navigate(typeof(PhotosDisplayPage));
        }

        
        private void AlbumsNewFlyout_Click(object sender, RoutedEventArgs e)
        {
            //user wAants ot create a new album
            //take him to create new albums page
            this.MainFrame.Navigate(typeof(AddNewAlbumPage));
        }

       
        private void PhotosViewFlyout_Click(object sender, RoutedEventArgs e)
        {
            string context = "Photos";
            this.MainFrame.Navigate(typeof(PhotosDisplayPage), context);
        }

        private void AlbumsViewFlyout_Click(object sender, RoutedEventArgs e)
        {
            this.MainFrame.Navigate(typeof(AlbumsPage));

        }
    }
}
