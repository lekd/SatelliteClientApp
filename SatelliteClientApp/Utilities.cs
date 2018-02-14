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
    public enum BendingDirection { None, LeftUp, LeftDown, RightUp, RightDown}
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
        static public double AngleOfVector(PointF origin, PointF vectorTop,bool isIn360)
        {
            PointF vector = new PointF(vectorTop.X - origin.X, vectorTop.Y - origin.Y);
            double len = Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
            double angle = Math.Acos(vector.X / len);
            if(vector.Y < 0)
            {
                angle = -angle;
                if(isIn360)
                {
                    angle += Math.PI * 2;
                }
            }
            return angle;
        }
        static public double getRightEdgeFromHypotenuse(double hypotenuse)
        {
            return hypotenuse / Math.Sqrt(2);
        }
        static public Bitmap DrawBendableRectangle(double defWidth,double defHeight,double actualWidth,BendingDirection bendDirection)
        {
            double actualHeight = (defWidth - actualWidth) ;
            actualHeight = actualHeight >= defHeight ? actualHeight : defHeight;
            Bitmap bmp = new Bitmap((int)Math.Ceiling(actualWidth), (int)Math.Ceiling(actualHeight));
            using (Graphics g = Graphics.FromImage(bmp))
            {
                SolidBrush transparentBrush = new SolidBrush(Color.Transparent);
                SolidBrush semiOpaqueBrush = new SolidBrush(Color.FromArgb(50, Color.White));
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                g.FillRectangle(transparentBrush, new Rectangle(0, 0, bmp.Width, bmp.Height));
                Pen linePen = new Pen(new SolidBrush(Color.Cyan), 2.0f);
                
                g.FillRectangle(semiOpaqueBrush, new Rectangle(0, 0, bmp.Width, bmp.Height));
                g.DrawRectangle(linePen, new Rectangle(0, 0, bmp.Width, bmp.Height));
                Pen transparentPen = new Pen(transparentBrush, 2.0f);
                RectangleF missingArea = new RectangleF();
                if (bendDirection == BendingDirection.LeftUp)
                {
                    missingArea.X = (float)defHeight;
                    missingArea.Y = 0;
                    missingArea.Width = (float)(actualWidth - defHeight);
                    missingArea.Height = (float)(actualHeight - defHeight);
                    g.FillRectangle(transparentBrush, missingArea.Left, missingArea.Top, missingArea.Width, missingArea.Height);
                    g.DrawRectangle(linePen, missingArea.X, missingArea.Y, missingArea.Width, missingArea.Height);
                    //cover unnecessary edges with transparent
                    if (missingArea.Width > 0 && missingArea.Height > 0)
                    {
                        g.DrawLine(transparentPen, missingArea.Left, missingArea.Top, missingArea.Right, missingArea.Top);
                        g.DrawLine(transparentPen, missingArea.Right, missingArea.Top, missingArea.Right, missingArea.Bottom);
                    }
                    return bmp;
                }
                else if(bendDirection == BendingDirection.LeftDown)
                {
                    missingArea.X = (float)defHeight;
                    missingArea.Y = (float)defHeight;
                    missingArea.Width = (float)(actualWidth - missingArea.X);
                    missingArea.Height = (float)(actualHeight - missingArea.Y);
                    g.FillRectangle(transparentBrush, missingArea.Left, missingArea.Top, missingArea.Width, missingArea.Height);
                    g.DrawRectangle(linePen, missingArea.X, missingArea.Y, missingArea.Width, missingArea.Height);
                    //cover unnecessary edges with transparent
                    if (missingArea.Width > 0 && missingArea.Height > 0)
                    {
                        g.DrawLine(transparentPen, missingArea.Left, missingArea.Bottom, missingArea.Right, missingArea.Bottom);
                        g.DrawLine(transparentPen, missingArea.Right, missingArea.Top, missingArea.Right, missingArea.Bottom);
                    }
                    return bmp;
                }
                else if(bendDirection == BendingDirection.RightUp)
                {
                    missingArea.X = 0;
                    missingArea.Y = 0;
                    missingArea.Width = (float)(actualWidth - defHeight);
                    missingArea.Height = (float)(actualHeight - defHeight);
                    g.FillRectangle(transparentBrush, missingArea.Left, missingArea.Top, missingArea.Width, missingArea.Height);
                    g.DrawRectangle(linePen, missingArea.X, missingArea.Y, missingArea.Width, missingArea.Height);
                    //cover unnecessary edges with transparent
                    if (missingArea.Width > 0 && missingArea.Height > 0)
                    {
                        g.DrawLine(transparentPen, missingArea.Left, missingArea.Top, missingArea.Right, missingArea.Top);
                        g.DrawLine(transparentPen, missingArea.Left, missingArea.Top, missingArea.Left, missingArea.Bottom);
                    }
                    return bmp;
                }
                else if(bendDirection == BendingDirection.RightDown)
                {
                    missingArea.X = 0;
                    missingArea.Y = (float)(defHeight);
                    missingArea.Width = (float)(actualWidth - defHeight);
                    missingArea.Height = (float)(actualHeight - defHeight);
                    g.FillRectangle(transparentBrush, missingArea.Left, missingArea.Top, missingArea.Width, missingArea.Height);
                    g.DrawRectangle(linePen, missingArea.X, missingArea.Y, missingArea.Width, missingArea.Height);
                    //cover unnecessary edges with transparent
                    if (missingArea.Width > 0 && missingArea.Height > 0)
                    {
                        g.DrawLine(transparentPen, missingArea.Left, missingArea.Bottom, missingArea.Right, missingArea.Bottom);
                        g.DrawLine(transparentPen, missingArea.Left, missingArea.Top, missingArea.Left, missingArea.Bottom);
                    }
                    return bmp;
                }

            }
            return bmp;
        }
        public static double getLeftMostOfPolygon(Point[] polygonVertices)
        {
            double minX = polygonVertices[0].X;
            for(int i=0; i< polygonVertices.Length; i++)
            {
                if(polygonVertices[i].X < minX)
                {
                    minX = polygonVertices[i].X;
                }
            }
            return minX;
        }
        public static double getRightMostOfPolygon(Point[] polygonVertices)
        {
            double maxX = polygonVertices[0].X;
            for (int i = 0; i < polygonVertices.Length; i++)
            {
                if (polygonVertices[i].X > maxX)
                {
                    maxX = polygonVertices[i].X;
                }
            }
            return maxX;
        }
        public static double getTopMostOfPolygon(Point[] polygonVertices)
        {
            double minY = polygonVertices[0].Y;
            for (int i = 0; i < polygonVertices.Length; i++)
            {
                if (polygonVertices[i].Y < minY)
                {
                    minY= polygonVertices[i].Y;
                }
            }
            return minY;
        }
        public static double getBottomMostOfPolygon(Point[] polygonVertices)
        {
            double maxY = polygonVertices[0].Y;
            for (int i = 0; i < polygonVertices.Length; i++)
            {
                if (polygonVertices[i].Y > maxY)
                {
                    maxY = polygonVertices[i].Y;
                }
            }
            return maxY;
        }
        public static Bitmap DrawTriangle(Point[] vertices)
        {
            double left = getLeftMostOfPolygon(vertices);
            double top = getTopMostOfPolygon(vertices);
            double right = getRightMostOfPolygon(vertices);
            double bottom = getBottomMostOfPolygon(vertices);
            double w = right - left;
            double h = bottom - top;
            Bitmap bmp = new Bitmap((int)Math.Ceiling(w), (int)Math.Ceiling(h));
            SolidBrush transparentBrush = new SolidBrush(Color.Transparent);
            SolidBrush semiCyanBrush = new SolidBrush(Color.FromArgb(100, Color.Cyan));
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.FillRectangle(transparentBrush, new Rectangle(0, 0, bmp.Width, bmp.Height));
                g.FillPolygon(semiCyanBrush, vertices);
            }
            return bmp;
        }
    }
}
