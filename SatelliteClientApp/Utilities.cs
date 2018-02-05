using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;

namespace SatelliteClientApp
{
    public class Utilities
    {
        public static BitmapImage ToBitmapImage(Bitmap bitmap, ImageFormat imgFormat)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, imgFormat);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                //bitmapImage.Freeze();
                return bitmapImage;
            }
        }
    }
}
