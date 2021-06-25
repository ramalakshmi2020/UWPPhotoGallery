using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace UWPPhotoGallery.Model
{
    public class Album
    {
        /// <summary>
        /// Name of the Album
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Thumbail image source of the coverphoto that needs to be displayed
        /// </summary>
        public ImageSource CoverPhotoThumbnail { get; set; }

        public string CoverPhotoFilePath { get; set; }
    }
}
