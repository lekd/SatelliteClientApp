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
        }
        public void setHeight(double h)
        {
            this.Height = h;
            canvasContainer.Height = h;
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
    }
}
