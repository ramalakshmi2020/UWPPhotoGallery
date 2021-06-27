using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
        /// The collecetion of all the albums created by the user
        /// </summary>
        private static List<Album> AllAlbums = new List<Album>();

        /// <summary>
        /// This variable is used for the onetime initialization of the albums collection
        /// </summary>
        private static bool isAllAlbumsInitialised = false;
        /// <summary>
        /// This variable is used to initialise the photo collection only once, and
        /// then after that it will the already initialised collection
        ///
        /// </summary>
        private static bool isAllPhotosInitialised = false;
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
            if (!isAllPhotosInitialised)
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
                            ImageProperties prop = await file.Properties.GetImagePropertiesAsync();
                            newphoto.DateTaken = prop.DateTaken.DateTime;
                            AllPhotos.Add(newphoto);
                        }
                        
                    }
                    isAllPhotosInitialised = true;
                    //add an album file to this folder


                }
                catch (Exception)
                {
                    retval = false;
                }
            }

            //recreate new collection by sorting them
            List<Photo> sortedlist;
            sortedlist = new  List<Photo>(AllPhotos.OrderByDescending(item => item.DateTaken));
            sortedlist.ForEach(item => photos.Add(item));
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
                    var myPictures = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Pictures);
                    string libraryPath = myPictures.SaveFolder.Path;
                    //first of all we need to check if the file already exists - if so replace the exisiting at the same time remove the listing from the photocollection
                    StorageFolder folder = KnownFolders.PicturesLibrary;
                    string filename = Path.GetFileName(selfile.Path);
                    if (await folder.TryGetItemAsync(filename) != null){

                        string path = $"{libraryPath}\\{filename}";
                        //remove this from the photo collection and replace with the new file
                        AllPhotos.RemoveAll(item => item.FilePath == path);
                       
                    }
                    StorageFile file = await selfile.CopyAsync(KnownFolders.PicturesLibrary, filename, NameCollisionOption.ReplaceExisting);
                    //this will make sure the file remains there for future too...
                    Photo newphoto = new Photo { FilePath = file.Path };

                    IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read);
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(fileStream);
                   
                    newphoto.Image = bitmapImage;


                   var thumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem) ;

                        BitmapImage thumbnailimage = new BitmapImage();
                    await thumbnailimage.SetSourceAsync(thumbnail);
                    newphoto.Thumbnail = thumbnailimage;
                    //image properties
                    ImageProperties prop = await file.Properties.GetImagePropertiesAsync();
                    newphoto.DateTaken = prop.DateTaken.DateTime;
                    AllPhotos.Add(newphoto);
                }
            }
        }
       
        
        private async static Task<bool> AddNewAlbum(Album album, ObservableCollection<Photo> SelectedPhotos)
        {
            StorageFolder storageFolder =
                    Windows.Storage.ApplicationData.Current.LocalFolder;
            StorageFolder albumsFolder;
            bool retval = true;
            try
            {
                //if it exists it will open else it will create the folder and open it 
                albumsFolder = await storageFolder.CreateFolderAsync("Albums", CreationCollisionOption.OpenIfExists);
                //add an album file to this folder
                //check if the album name is empty - if so give it a name
                if (String.IsNullOrEmpty(album.Name))
                {
                    int count = AllAlbums.Count + 1;
                    album.Name = $"Album{count}"; 
                }
                string path = $"{albumsFolder.Path}//{album.Name}.txt";

                using (StreamWriter sw = new StreamWriter(path, false))
                {
                    //First write the path of the coverphotoimage
                    sw.WriteLine(album.CoverPhotoFilePath);
                    foreach (Photo ph in SelectedPhotos)
                    {
                        sw.WriteLine(ph.FilePath);
                    }
                    sw.Close();

                }
                AllAlbums.Add(album);
            }
            catch (Exception)
            {
                retval = false;

            }
            return retval;
            

        }
        /// <summary>
        /// This method will initialize the list of albums that have been created previously if any!!
        /// Since it picks stuff from file, we will maintain a static collection and return it everytime the user queries
        /// </summary>
        /// <param name="albums"></param>
        /// <returns></returns>
        public async static Task<bool> GetAllAlbums(ObservableCollection<Album> albums)
        {
            bool retval = true;
            if (!isAllAlbumsInitialised) { 
                StorageFolder storageFolder =
                    Windows.Storage.ApplicationData.Current.LocalFolder;
                StorageFolder albumsFolder;
                try
                {
                    albumsFolder = await storageFolder.CreateFolderAsync("Albums", CreationCollisionOption.OpenIfExists);
                    //albums are there start getting them one by one and creat
                    if (albumsFolder != null)
                    {

                        var files = await albumsFolder.GetFilesAsync();
                        if (files != null)
                        {
                            foreach (StorageFile file in files)
                            {
                                //get the index of the wextensiona nd trim it to get the album name only
                                //int index = file.Name.IndexOf(".", file.Name.Length - 1);
                                string albumname = Path.GetFileNameWithoutExtension(file.Name);

                                //for each existing album - create an album record and load it in memory
                                // now for read the file and update the coverphoto and create an album record
                                string coverfilePath;

                                using (StreamReader sr = new StreamReader(file.Path))
                                {
                                    //First write the path of the coverphotoimage
                                    coverfilePath = sr.ReadLine();

                                    sr.Close();

                                }
                                //load the thumbnailasync
                                //load the file with the path
                                StorageFolder picturesFolder = KnownFolders.PicturesLibrary;
                                StorageFile picfile = await picturesFolder.GetFileAsync(Path.GetFileName(coverfilePath));

                                var thumbnail = await picfile.GetThumbnailAsync(ThumbnailMode.SingleItem);
                                BitmapImage thumbnailimage = new BitmapImage();
                                await thumbnailimage.SetSourceAsync(thumbnail);


                                Album newalbum = new Album { Name = albumname, CoverPhotoFilePath = coverfilePath, CoverPhotoThumbnail = thumbnailimage };


                                AllAlbums.Add(newalbum);
                            }
                            //Read the files and get the album deltails
                            isAllAlbumsInitialised = true;
                        }
                        else
                        {
                            retval = false;
                        }
                    }
                    else
                    {
                        retval = false;
                    }
                } catch (Exception )
                {
                    retval = false;
                }




            }
            AllAlbums.ForEach(item => albums.Add(item));
            //copy the albums to the colletcion
            return retval;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="photos"></param>
        /// <param name="albumName"></param>
        public static bool GetPhotosForAlbumName(ObservableCollection<Photo> photos, string albumName)
        {
            bool retval = true;
            try
            {
                photos.Clear();
                List<Photo> albumphotos = new List<Photo>();
                //open the file and get the contents
                string path = $"{Windows.Storage.ApplicationData.Current.LocalFolder.Path}\\Albums\\{albumName}.txt";

                StreamReader sr = new StreamReader(path);
                string line;
                //Read the first line of text
                line = sr.ReadLine();
                //Continue to read until you reach end of file
                line = sr.ReadLine();
                while (line != null)
                {
                    //first line is the coverphoto for the album
                    //write the lie to console window

                    //Read the next line

                    //this is the first selected photo
                    //check if any of the photocollection matches with this, if so add it tot he list
                    foreach (Photo ph in AllPhotos)
                    {
                        if (line == ph.FilePath)
                        {
                            //add it to the selected photos/observable collection
                            albumphotos.Add(ph);
                        }
                    }
                    line = sr.ReadLine();
                }
                //close the file
                sr.Close();
                //sort photos by date time
                List<Photo> sortedlist;
                sortedlist = new List<Photo>(albumphotos.OrderByDescending(item => item.DateTaken));
                sortedlist.ForEach(item => photos.Add(item));
            }
            catch (Exception )
            {
                retval = false;
            }

            return retval;
        }

        /// <summary>
        /// This method will add a new album it doesnot exist or modify the existing one
        /// </summary>
        /// <param name="oldAlbumName"></param>
        /// <param name="newalbum"></param>
        /// <param name="photostoadd"></param>
        /// <returns></returns>
        public static async Task<bool> AddModifyAlbum(string oldAlbumName, Album newalbum, ObservableCollection<Photo> photostoadd)
        {
            bool retval = true;
            try
            {
                //the album that was there before has got modified. This needs to be updated. First let us remove the oldalbum from the collection
                // add the new album.
                if (!String.IsNullOrEmpty(oldAlbumName))
                {
                    retval = DeleteAlbum(oldAlbumName);
                }
                retval = await AddNewAlbum(newalbum, photostoadd);
            }catch (Exception)
            {
                retval = false;
            }
            return retval;
        }

        private static bool DeleteAlbum (string albumName)
        {
            bool retval = true;
            try
            {
                //First delete the album.txt file
                string path = $"{Windows.Storage.ApplicationData.Current.LocalFolder.Path}\\Albums\\{albumName}.txt";
                File.Delete(path);
                //Also the album collection needs to be updated
                AllAlbums.RemoveAll(item => albumName == item.Name);


            }
            catch
            {
                retval = false;

            }
            return retval;
        }

        /// <summary>
        /// Deletion of photos 
        /// </summary>
        /// <param name="photos"></param>
        public static async Task<bool> DeletePhotos(List<Photo> photos)
        {
            bool retval = true;
            try
            {
                foreach (Photo ph in photos)
                {
                    StorageFolder picturesFolder = KnownFolders.PicturesLibrary;

                    StorageFile manifestFile = await picturesFolder.GetFileAsync(Path.GetFileName(ph.FilePath));
                    await manifestFile.DeleteAsync();
                    
                    //Need to remove the photo from static collection too.
                    AllPhotos.Remove(ph);

                    //also need to remove references from the albums if any
                    foreach (Album al in AllAlbums)

                    {
                        ObservableCollection<Photo> albumphotos = new ObservableCollection<Photo>();
                       

                        string path = $"{Windows.Storage.ApplicationData.Current.LocalFolder.Path}\\Albums\\{al.Name}.txt";

                        //open the file and check for occurances of this pic file
                        StreamReader sr = new StreamReader(path);
                        string currentcontents = sr.ReadToEnd();
                        sr.Close();
                        List<string> listofPhotos = currentcontents.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                        
                        //check if this photo belongs to an album
                        //if so the album file and the collection needs to be changed
                        if (listofPhotos.Contains(ph.FilePath))
                        {
                            listofPhotos.RemoveAll(x => x == ph.FilePath);

                            //newstring to write to the file now
                            
                            //does the coverphoto match?
                            if (al.CoverPhotoFilePath == ph.FilePath)
                            {
                                //change the coverphoto to any other photo
                                al.CoverPhotoFilePath = listofPhotos[0];
                                //add the first line to be the coverphoto
                                listofPhotos.Insert(0, al.CoverPhotoFilePath);
                                //we need to also change the thumbnail
                                StorageFile picfile = await picturesFolder.GetFileAsync(Path.GetFileName(al.CoverPhotoFilePath));

                                var thumbnail = await picfile.GetThumbnailAsync(ThumbnailMode.SingleItem);
                                BitmapImage thumbnailimage = new BitmapImage();
                                await thumbnailimage.SetSourceAsync(thumbnail);
                                al.CoverPhotoThumbnail = thumbnailimage;

                            }
                            StreamWriter sw = new StreamWriter(path);
                            foreach (string s in listofPhotos)
                            {
                                sw.WriteLine(s);
                            }
                            sw.Close();
                        }

                        // if that is
                    }
                }
                
            }
            catch
            {
                retval = false;
            }
            return retval;
        }

        /// <summary>
        /// Deletion of albums
        /// </summary>
        /// <param name="albums"></param>
        /// <returns></returns>
        public static bool DeleteAlbums(List<Album> albums)
        {
            bool retval = true;
           foreach(Album item in albums)
            {
                retval = DeleteAlbum(item.Name);
            }
            return retval;
                
        }
    }
}
