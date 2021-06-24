using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace UWPPhotoGallery.Model
{

    /// <summary>
    /// Basic model of the photo that will be displayed using the application 
    /// </summary>
    public class Photo
    {
      
        /// <summary>
        /// File path will contain the full path to the location of the actual image 
        /// </summary>
        
        public string FilePath { get; set; }

        /// <summary>
        /// Image is the loaded bitmap that has the full image and 
        /// can be mapped to any image control in xaml
        /// </summary>
        public ImageSource Image { get; set; }

        /// <summary>
        /// Thumbnail is the loaded bitmap that has the thumbnail of the photo
        /// and can be mapped to any image control in xaml
        /// </summary>
        public ImageSource Thumbnail { get; set; }
        
    }
}
