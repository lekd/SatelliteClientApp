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
        
        public static Bitmap BitmapImageToJpegBitmap(BitmapImage bmpImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new JpegBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bmpImage));
                enc.Save(outStream);
                Bitmap bmp = new Bitmap(outStream);
                return bmp;
            }
        }
        public static Bitmap DownScaleBitmap(Bitmap origin, float downScaleFactor)
        {
            Bitmap resized = new Bitmap(origin, new Size((int)(origin.Width * 1.0f / downScaleFactor),
                                                           (int)(origin.Height * 1.0f / downScaleFactor)));
            return resized;
        }
        public static Bitmap CropBitmap(Bitmap origin, float percentL, float percentT, float percentW, float percentH)
        {
            Bitmap crop = new Bitmap((int)(percentW * origin.Width), (int)(percentH * origin.Height));
            Graphics g = Graphics.FromImage(crop);
            if (percentL >= 0 && percentL + percentW <= 1)
            {
                Rectangle cropRect = new Rectangle();
                cropRect.X = (int)(percentL * origin.Width);
                cropRect.Y = (int)(percentT * origin.Height);
                cropRect.Width = (int)(percentW * origin.Width);
                cropRect.Height = (int)(percentH * origin.Height);
                g.DrawImage(origin, new Rectangle(0, 0, crop.Width, crop.Height), cropRect, GraphicsUnit.Pixel);
            }
            else
            {
                Rectangle cropLeftHalf = new Rectangle();
                Rectangle cropRightHalf = new Rectangle();
                if (percentL < 0)
                {
                    cropRightHalf.X = 0;
                    cropRightHalf.Y = (int)(percentT * origin.Height);
                    cropRightHalf.Width = (int)((percentW + percentL) * origin.Width);
                    cropRightHalf.Height = (int)(percentH * origin.Height);

                    cropLeftHalf.X = (int)((1 + percentL) * origin.Width);
                    cropLeftHalf.Y = (int)(percentT * origin.Height);
                    cropLeftHalf.Width = (int)(-percentL * origin.Width);
                    cropLeftHalf.Height = (int)(percentH * origin.Height);
                }
                else
                {
                    cropLeftHalf.X = (int)(percentL * origin.Width);
                    cropLeftHalf.Y = (int)(percentT * origin.Height);
                    cropLeftHalf.Width = (int)((1 - percentL) * origin.Width);
                    cropLeftHalf.Height = (int)(percentH * origin.Height);

                    cropRightHalf.X = 0;
                    cropRightHalf.Y = (int)(percentT * origin.Height);
                    cropRightHalf.Width = (int)((percentL + percentW - 1) * origin.Width);
                    cropRightHalf.Height = (int)(percentH * origin.Height);
                }
                g.DrawImage(origin, new Rectangle(0, 0, cropLeftHalf.Width, cropLeftHalf.Height), cropLeftHalf, GraphicsUnit.Pixel);
                g.DrawImage(origin, new Rectangle(cropLeftHalf.Width, 0, cropRightHalf.Width, cropRightHalf.Height), cropRightHalf, GraphicsUnit.Pixel);
            }
            return crop;
        }
        static public Bitmap rotateBitmapQuadraticAngle(Bitmap origin, double angle)
        {
            if (angle == 180 || angle == -180)
            {
                origin.RotateFlip(RotateFlipType.Rotate180FlipNone);
            }
            else if (angle == 90 || angle == -270)
            {
                origin.RotateFlip(RotateFlipType.Rotate90FlipNone);
            }
            else if (angle == -90 || angle == 270)
            {
                origin.RotateFlip(RotateFlipType.Rotate270FlipNone);
            }
            return origin;
        }
        static public Bitmap skewBitmap(Bitmap origin, double dstBigEdge,double dstSmallEdge,double dstHeight)
        {
            Point topLeft = new Point(0, 0);
            Point topRight = new Point((int)dstBigEdge, 0);
            Point botLeft = new Point((int)((dstBigEdge - dstSmallEdge) / 2), (int)dstHeight);
            Point botRight = new Point((int)((dstBigEdge + dstSmallEdge) / 2), (int)dstHeight);
            return QuadrilateralDistortion.QuadDistort.Distort(origin, topLeft, topRight, botLeft, botRight);
        }
    }
}
