using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SatelliteClientApp
{
    /// <summary>
    /// Interaction logic for PanoViewer.xaml
    /// </summary>
    public partial class PanoViewer : UserControl
    {
        Bitmap panoFrame;
        bool isInitialized = false;
        private Rect focusBoundary = new Rect();
        public Rect FocusBoundary
        {
            get
            {
                return focusBoundary;
            }
        }
        public PanoViewer()
        {
            InitializeComponent();
        }
        public void updatePanoImage(BitmapImage panoBmpImg, double satPosInPano)
        {
            initControls();
            Bitmap temp = Utilities.BitmapImageToJpegBitmap(panoBmpImg);
            Bitmap rearrangedPano = rearrangePano(temp, satPosInPano);
            panoFrame = new Bitmap(rearrangedPano);
            imgPano.Source = Utilities.ToBitmapImage(panoFrame,ImageFormat.Jpeg);
        }
        void initControls()
        {
            if(!isInitialized)
            {
                rectHighlightSegment.Width = imgPano.Width / 8;
            }
            isInitialized = true;
        }
        Bitmap rearrangePano(Bitmap pano,double dividerPos)
        {
            double leftHalfWidth = pano.Width * dividerPos;
            if(((int)leftHalfWidth)==0 || ((int)leftHalfWidth)==pano.Width)
            {
                return pano;
            }
            Bitmap rearrangedPano = new Bitmap(pano.Width, pano.Height);
            Bitmap leftHalf = Utilities.CropBitmap(pano, 0, 0, (float)dividerPos, 1);
            Bitmap rightHalf = Utilities.CropBitmap(pano, (float)dividerPos, 0, (float)(1 - dividerPos), 1);
            using (Graphics g = Graphics.FromImage(rearrangedPano))
            {
                g.DrawImage(rightHalf, new PointF(0, 0));
                g.DrawImage(leftHalf, new PointF((float)rightHalf.Width, 0));
                return rearrangedPano;
            }
        }
        public void updateFocusWindow(double relativeAngularToSatellite,double relativeW)
        {
            double relativeAngularInPercent = relativeAngularToSatellite /( Math.PI * 2);
            if(relativeAngularInPercent < 0)
            {
                relativeAngularInPercent += 1;
            }
            double centerX = relativeAngularInPercent * imgPano.Width;
            double w = relativeW * imgPano.Width;
            double left = centerX - w / 2;
            rectHighlightSegment.SetValue(Canvas.LeftProperty, left);
            rectHighlightSegment.SetValue(Canvas.TopProperty, (double)0);
            rectHighlightSegment.Width = w;
            rectHighlightSegment.BeginAnimation(UIElement.OpacityProperty, null);
            rectHighlightSegment.Opacity = 1.0;

            //start fade-out animation
            var fadeOutAnim = new DoubleAnimation
            {
                From = 1,
                To = 0,
                BeginTime = TimeSpan.FromSeconds(1),
                Duration = TimeSpan.FromSeconds(2),
                FillBehavior = FillBehavior.Stop
            };
            fadeOutAnim.Completed += (s, a) => rectHighlightSegment.Opacity = 0;
            rectHighlightSegment.BeginAnimation(UIElement.OpacityProperty, fadeOutAnim);

            focusBoundary.X = left/imgPano.Width;
            focusBoundary.Width = w/imgPano.Width;
            focusBoundary.Y = 0;
            focusBoundary.Height = rectHighlightSegment.Height/imgPano.Width;
        }
    }
}
