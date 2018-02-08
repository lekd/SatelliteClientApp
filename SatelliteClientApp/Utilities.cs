using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using Emgu.CV.Structure;

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
            PointF[] srcCorners = new PointF[4];
            srcCorners[0] = new PointF(0, 0);
            srcCorners[1] = new PointF(origin.Width, 0);
            srcCorners[2] = new PointF(origin.Width, origin.Height);
            srcCorners[3] = new PointF(0, origin.Height);
            PointF[] dstCorners = new PointF[4];
            dstCorners[0] = new PointF(0, 0);
            dstCorners[1] = new PointF((float)dstBigEdge, 0);
            dstCorners[2] = new PointF((float)((dstBigEdge + dstSmallEdge) / 2), (float)dstHeight);
            dstCorners[3] = new PointF((float)((dstBigEdge - dstSmallEdge) / 2), (float)dstHeight);
            Emgu.CV.Image<Emgu.CV.Structure.Bgra, Byte> srcImg = new Emgu.CV.Image<Bgra, byte>(origin);
            Bitmap bufBmp = new Bitmap((int)dstBigEdge, (int)dstHeight);
            bufBmp.MakeTransparent();
            Emgu.CV.Image<Bgra, Byte> dstImg = new Emgu.CV.Image<Bgra, byte>(bufBmp);
            /*double[,] srcCorners = { { 0, 0 }, { origin.Width, 0 }, { origin.Width, origin.Height }, { origin.Width, 0 } };
            double[,] dstCorners = { { 0, 0 }, { dstBigEdge, 0 }, { (dstBigEdge + dstSmallEdge) / 2, dstHeight }, { (dstBigEdge - dstSmallEdge) / 2, dstHeight } };
            double[,] homog = new double[3, 3];

            Emgu.CV.Matrix<double> srcPM = new Emgu.CV.Matrix<double>(srcCorners);
            Emgu.CV.Matrix<double> dstPM = new Emgu.CV.Matrix<double>(dstCorners);
            Emgu.CV.Matrix<double> homogm = new Emgu.CV.Matrix<double>(homog);*/
            Emgu.CV.Mat transformMat = Emgu.CV.CvInvoke.FindHomography(srcCorners, dstCorners, Emgu.CV.CvEnum.HomographyMethod.Default);
            Emgu.CV.CvInvoke.WarpPerspective(srcImg, dstImg, transformMat, new Size((int)dstBigEdge, (int)dstHeight));
            return dstImg.Bitmap;
        }
    }
}
