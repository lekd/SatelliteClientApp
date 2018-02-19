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
        const double tablePadding = 20;
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
            hubTableViewer.tableFocusChangedEventHandler += HubTableViewer_tableFocusChangedEventHandler;
            hubTableViewer.tooltipControlReleasedEventHandler += HubTableViewer_tooltipControlReleasedEventHandler;
        }

        

        private void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
        public void updateUIWithNewSize()
        {
            tableFocusTooltip.SetSize(this.Height * 0.4);
        }
        #region children control events handlers
        private void HubTableViewer_tableFocusChangedEventHandler(Bitmap tableFocus, double relPosX, double relPosY)
        {
            double windowCenterX = this.Width / 2 ;
            double windowCenterY = this.Height / 2;
            tableFocusTooltip.updateToolTipContent(tableFocus);
            double absCenterX = relPosX * (this.Width - tableFocusTooltip.Width) + windowCenterX;
            double absCenterY = relPosY * (this.Height - tableFocusTooltip.Height) + windowCenterY;
            double tooltipLeft = absCenterX - tableFocusTooltip.Width / 2;
            double tooltipTop = absCenterY - tableFocusTooltip.Width / 2;
            tableFocusTooltip.SetValue(Canvas.LeftProperty, tooltipLeft);
            tableFocusTooltip.SetValue(Canvas.TopProperty, tooltipTop);
            tableFocusTooltip.Visibility = Visibility.Visible;
            tableFocusTooltip.Opacity = 1;
            tableFocusTooltip.updateRelativePos(new PointF((float)relPosX, (float)relPosY));
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
        private void HubTableViewer_tooltipControlReleasedEventHandler()
        {
            tableFocusTooltip.enableTouch(true);
        }
        #endregion
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
            
            hubTableViewer.updateTableContent(tableContent, maxW, maxH- 2*tablePadding);
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
            hubTableViewer.hideTableContentFocus();
            Utilities.FadeControlOut(tableFocusTooltip, 0, 1, true);
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
            positionBubbleLink(img_EdgeFocusLink, leftMost, topMost);
            Utilities.FadeControlOut(img_EdgeFocusLink, 1, 2, false);
        }
        
        void positionBubbleLink(UIElement bubbleLink,double left, double top)
        {
            bubbleLink.SetValue(Canvas.LeftProperty, left);
            bubbleLink.SetValue(Canvas.TopProperty, top);
            bubbleLink.BeginAnimation(UIElement.OpacityProperty, null);
            bubbleLink.Opacity = 1;
        }
        public void panoFocusPosChangedHandler(double angularDifToSatPos, Rect panoFocusBoundary)
        {
            hubTableViewer.updateEdgeFocus(angularDifToSatPos);
            displayPanoBubbleLink(panoFocusBoundary);
        }
        #endregion

        
    }
}
