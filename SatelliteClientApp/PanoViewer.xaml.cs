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
        public delegate void PanoFocusPositionChanged(double angularDifToSatPos, Rect panoFocusBoundary);
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
        public event PanoFocusPositionChanged panoFocusPosChangedHandler = null;
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
            rectHighlightSegment.Width = w;
            changeFocusWindowPosition(centerX);
            Utilities.FadeControlOut(rectHighlightSegment, 1, 2, false);
        }
        #region Mouse Events
        bool isMouseDown = false;
        private void imgPano_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                isMouseDown = true;
                System.Windows.Point mousePos = e.GetPosition(imgPano);
                changeFocusWindowPosition(mousePos.X);
                notifyPanoFocusChanged(mousePos.X);
            }
        }

        private void imgPano_MouseMove(object sender, MouseEventArgs e)
        {
            if(isMouseDown)
            {
                System.Windows.Point mousePos = e.GetPosition(imgPano);
                changeFocusWindowPosition(mousePos.X);
                notifyPanoFocusChanged(mousePos.X);
            }
        }
        private void imgPano_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = false;
            Utilities.FadeControlOut(rectHighlightSegment, 1, 2, false);
        }

        private void imgPano_MouseLeave(object sender, MouseEventArgs e)
        {
            isMouseDown = false;
            Utilities.FadeControlOut(rectHighlightSegment, 1, 2, false);
        }
        #endregion

        void changeFocusWindowPosition(double centerX, double centerY = 0)
        {
            double left = centerX - rectHighlightSegment.Width / 2;
            rectHighlightSegment.SetValue(Canvas.LeftProperty, left);
            rectHighlightSegment.SetValue(Canvas.TopProperty, (double)0);
            rectHighlightSegment.BeginAnimation(UIElement.OpacityProperty, null);
            rectHighlightSegment.Opacity = 1.0;

            focusBoundary.X = left / imgPano.Width;
            focusBoundary.Width = rectHighlightSegment.Width / imgPano.Width;
            focusBoundary.Y = 0;
            focusBoundary.Height = rectHighlightSegment.Height / imgPano.Width;
            
        }
        
        void notifyPanoFocusChanged(double centerX, double centerY=0)
        {
            double percentInPano = centerX / imgPano.Width;
            if (percentInPano > 0.5)
            {
                percentInPano -= 1;
            }
            double angularDifToSatPos = percentInPano * Math.PI * 2;
            if(panoFocusPosChangedHandler != null)
            {
                panoFocusPosChangedHandler(angularDifToSatPos, focusBoundary);
            }
        }
    }
}
