using System;
using System.Collections.Generic;
using System.Drawing;
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
    /// Interaction logic for AugmentedHubTableViewer.xaml
    /// </summary>
    public partial class AugmentedHubTableViewer : UserControl
    {
        public event HubTableViewerControl.EdgeFocusChanged edgeFocusChangedEventHandler = null;
        Rect _panoFocusBoundary;
        public Rect PanoFocusBoundary
        {
            get
            {
                return _panoFocusBoundary;
            }

            set
            {
                _panoFocusBoundary = value;
            }
        }

        public AugmentedHubTableViewer()
        {
            InitializeComponent();
            hubTableViewer.boundaryChangeEventHandler += HubTableViewer_boundaryChangeEventHandler;
            hubTableViewer.edgeFocusChangeEventHandler += HubTableViewer_edgeFocusChangeEventHandler;
        }

        private void HubTableViewer_edgeFocusChangeEventHandler(PointF relPos, double relAngularToSallite, double relativeW)
        {
            if(edgeFocusChangedEventHandler != null)
            {
                edgeFocusChangedEventHandler(relPos, relAngularToSallite,relativeW);
                
            }
        }

        private void HubTableViewer_boundaryChangeEventHandler(double w, double h)
        {
            //set position of the hubviewer control in this augmented control
            double top = (this.Height - h) / 2;
            double left = (this.Width - w) / 2;
            hubTableViewer.SetValue(Canvas.LeftProperty, left);
            hubTableViewer.SetValue(Canvas.TopProperty, top);
        }

        public void setWidth(double w)
        {
            this.Width = w;
            canvasContainer.Width = w;
            blankCanvas.Width = w;
        }
        public void setHeight(double h)
        {
            this.Height = h;
            canvasContainer.Height = h;
            blankCanvas.Height = h;
        }
        public HubTableViewerControl.TableViewMode ViewMode
        {
            get
            {
                return hubTableViewer.Mode;
            }
        }
        public void updateTableContent(Bitmap tableContent,double maxW,double maxH)
        {
            
            hubTableViewer.updateTableContent(tableContent, maxW, maxH);
        }
        public void updateTableEdgesImages(Bitmap panoImage)
        {
            hubTableViewer.updateTableEdges(panoImage);
        }
        public void switchMode()
        {
            hubTableViewer.SwitchMode();
        }
        public double SatPositionInPano
        {
            get
            {
                return hubTableViewer.SatPositionInPano;
            }
        }

        
        public void Refresh()
        {
            hubTableViewer.Refresh();
        }
        #region mouse events
        private void blankCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            hubTableViewer.resetMouseState();
        }

        private void blankCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
        #endregion

        #region display bubble links
        public void displayPanoBubbleLink(Rect relPanoFocusBoundary)
        {
            PointF edgeFocusCenter = hubTableViewer.getCenterPointOfEdgeFocus(hubTableViewer.RelativeEdgeFocusCenter);
            System.Windows.Point centerInGlobal = hubTableViewer.TranslatePoint(new System.Windows.Point((int)edgeFocusCenter.X, (int)edgeFocusCenter.Y), canvasContainer);
            System.Drawing.Point panoFocusBottomLeft = new System.Drawing.Point((int)(relPanoFocusBoundary.Left * canvasContainer.Width), 0);
            System.Drawing.Point panoFocusBottomRight = new System.Drawing.Point((int)(relPanoFocusBoundary.Right * canvasContainer.Width), 0);
            System.Drawing.Point[] linkVertices = new System.Drawing.Point[3];
            linkVertices[0] = new System.Drawing.Point((int)centerInGlobal.X, (int)centerInGlobal.Y);
            linkVertices[1] = panoFocusBottomLeft;
            linkVertices[2] = panoFocusBottomRight;
            //offsetting vertices to the left & top
            double leftMost = Utilities.getLeftMostOfPolygon(linkVertices);
            double topMost = Utilities.getTopMostOfPolygon(linkVertices);
            for (int i = 0; i < linkVertices.Length; i++)
            {
                linkVertices[i].X -= (int)leftMost;
                linkVertices[i].Y -= (int)topMost;
            }
            Bitmap bmp = Utilities.DrawTriangle(linkVertices);
            img_EdgeFocusLink.Width = bmp.Width;
            img_EdgeFocusLink.Height = bmp.Height;
            try
            {
                img_EdgeFocusLink.Source = Utilities.ToBitmapImage(bmp, System.Drawing.Imaging.ImageFormat.Png);
            }catch
            { }
            img_EdgeFocusLink.BeginAnimation(UIElement.OpacityProperty, null);
            img_EdgeFocusLink.Opacity = 1;
            img_EdgeFocusLink.SetValue(Canvas.LeftProperty, leftMost);
            img_EdgeFocusLink.SetValue(Canvas.TopProperty, topMost);

            var fadeOutAnim = new DoubleAnimation
            {
                From = 1,
                To = 0,
                BeginTime = TimeSpan.FromSeconds(1),
                Duration = TimeSpan.FromSeconds(2),
                FillBehavior = FillBehavior.Stop
            };
            fadeOutAnim.Completed += (s, a) => img_EdgeFocusLink.Opacity = 0;
            img_EdgeFocusLink.BeginAnimation(UIElement.OpacityProperty, fadeOutAnim);
        }
        #endregion
    }
}
