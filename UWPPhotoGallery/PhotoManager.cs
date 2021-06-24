using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWPPhotoGallery.Model;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace UWPPhotoGallery
{
    public static class PhotoManager
    {
        /// <summary>
        /// The collection of all the photos in the mypictures folder 
        /// </summary>
        private static List<Photo> AllPhotos = new List<Photo>();
        /// <summary>
        /// This variable is used to initialise the photo collection only once, and
        /// then after that it will the already initialised collection
        ///
        /// </summary>
        private static bool isinitialised = false;
        /// <summary>
        /// This method will initilaze the Allphotos static member variable, the first time it is caled.
        /// Once initialised, it will always return AllPhotos collection.
        /// </summary>
        /// <param name="photos"></param>
        /// <returns></returns>
        public static async Task<bool> GetAllPhotos(ObservableCollection<Photo> photos)
        {
            //how to get absolute path
            photos.Clear();
            bool retval = true;
            //PhotoCollection.Clear();
            if (!isinitialised)
            {
                try
                {
                    //Check if there are pictures in the picture library
                    StorageFolder picturesFolder = KnownFolders.PicturesLibrary;
                    IReadOnlyList<StorageFile> fileList = await picturesFolder.GetFilesAsync();
                    if (fileList != null)
                    {

                        foreach (StorageFile file in fileList)
                        {
                            //Create new photo with file path
                            Photo newphoto = new Photo { FilePath = file.Path };
                            //read the file and create a bitmap image with the file stream
                            IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read);
                            BitmapImage bitmapImage = new BitmapImage();
                            bitmapImage.SetSource(fileStream);

                            //set this bitmap image to the photo image
                            newphoto.Image = bitmapImage;

                            //Get the thumbnail for this image and set it in photo element
                            var thumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem);

                            BitmapImage thumbnailimage = new BitmapImage();
                            await thumbnailimage.SetSourceAsync(thumbnail);
                            newphoto.Thumbnail = thumbnailimage;

                            AllPhotos.Add(newphoto);
                        }
                        
                    }
                    isinitialised = true;
                    //add an album file to this folder


                }
                catch (Exception e)
                {
                    retval = false;
                }
            }
            AllPhotos.ForEach(elem => photos.Add(elem));
            return retval;
        }


        /// <summary>
        /// This method will use the file picker class and ask the user to pick files that needs to be added to 
        /// the photo collection. It will also update the Allphotos static collection with the newly added photos
        /// </summary>
        /// <returns></returns>
        public static async Task AddNewPhoto()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            var files = await picker.PickMultipleFilesAsync();
            if (files != null)
            {
                foreach (StorageFile selfile in files)
                {
                    StorageFile file = await selfile.CopyAsync(KnownFolders.PicturesLibrary);
                    //this will make sure the file remains there for future too...
                    Photo newphoto = new Photo { FilePath = file.Path };

                    IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read);
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(fileStream);

                    newphoto.Image = bitmapImage;


                    var thumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem);

                    BitmapImage thumbnailimage = new BitmapImage();
                    await thumbnailimage.SetSourceAsync(thumbnail);
                    newphoto.Thumbnail = thumbnailimage;
                    
                    AllPhotos.Add(newphoto);
                }
            }
        }
    }
}
