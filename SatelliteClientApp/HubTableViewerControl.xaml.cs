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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SatelliteClientApp
{
    /// <summary>
    /// Interaction logic for HubTableViewerControl.xaml
    /// </summary>
    public partial class HubTableViewerControl : UserControl
    {
        public enum TableViewMode { NORMAL, SATELLITE_ANCHOR}
        Bitmap _tableContent;
        Bitmap _panoContent;
        double _tableRotation = 90;
        PointF satellitePos = new PointF(0, 0.5f);
        public Boolean WasInitialized { get; set; }
        TableViewMode viewMode = TableViewMode.NORMAL;
        
        public HubTableViewerControl()
        {
            InitializeComponent();
            _tableContent = new Bitmap(1, 1);
            WasInitialized = false;
        }
        public void updateTableContent(Bitmap tabContent)
        {
            _tableContent = new Bitmap(tabContent);
            BitmapImage src = Utilities.ToBitmapImage(_tableContent, ImageFormat.Jpeg);
            img_TableContent.Source = src;
        }
        public void updateTableEdges(Bitmap panoImg)
        {
            Bitmap[] edgesBmps = segmentPanoIntoEdges(panoImg);
            //assign top edge
            BitmapImage topSrc = Utilities.ToBitmapImage(edgesBmps[0], ImageFormat.Jpeg);
            img_EdgeTop.Source = topSrc;
            img_EdgeTop.Tag = edgesBmps[0];
            //assign right edge
            BitmapImage rightSrc = Utilities.ToBitmapImage(edgesBmps[1], ImageFormat.Jpeg);
            img_EdgeRight.Source = rightSrc;
            img_EdgeRight.Tag = edgesBmps[1];
            //assign bottom edge
            BitmapImage bottomSrc = Utilities.ToBitmapImage(edgesBmps[2], ImageFormat.Jpeg);
            img_EdgeBottom.Source = bottomSrc;
            img_EdgeBottom.Tag = edgesBmps[2];
            //assign left edge
            //assign right edge
            BitmapImage leftSrc = Utilities.ToBitmapImage(edgesBmps[3], ImageFormat.Jpeg);
            img_EdgeLeft.Source = leftSrc;
            img_EdgeLeft.Tag = edgesBmps[3];
        }
        public void initTableBoundary(Bitmap tableContent, double parentW,double parentH)
        {
            double tableWHRatio = tableContent.Width * 1.0 / tableContent.Height;
            this.Height = parentH;
            double tableDisplayH = parentH * 0.8;
            double edgeThick = parentH * 0.1;
            double tableDisplayW = tableDisplayH * tableWHRatio;
            this.Width = tableDisplayW + 2 * edgeThick;

            grid_TableContainer.RowDefinitions[0].Height = new GridLength(edgeThick);
            grid_TableContainer.RowDefinitions[1].Height = new GridLength(tableDisplayH);
            grid_TableContainer.RowDefinitions[2].Height = new GridLength(edgeThick);

            grid_TableContainer.ColumnDefinitions[0].Width = new GridLength(edgeThick);
            grid_TableContainer.ColumnDefinitions[1].Width = new GridLength(tableDisplayW);
            grid_TableContainer.ColumnDefinitions[2].Width = new GridLength(edgeThick);

            this.RenderTransform = new RotateTransform(_tableRotation, this.Width / 2, this.Height / 2);
            WasInitialized = true;
        }
        Bitmap[] segmentPanoIntoEdges(Bitmap panoImg)
        {
            double edgeThick = grid_TableContainer.RowDefinitions[0].Height.Value;
            Bitmap[] edges = new Bitmap[4];
            PointF tableSizeNormalized = new PointF((float)(_tableContent.Width*1.0/ _tableContent.Height), 1);
            float tablePeriphery = 2 * (tableSizeNormalized.X + tableSizeNormalized.Y);
            float relL, relT, relW, relH;
            //crop top edge
            relL = (tableSizeNormalized.X / 2 + tableSizeNormalized.Y) / tablePeriphery;
            relT = 0;
            relW = tableSizeNormalized.X / tablePeriphery;
            relH = 1;
            edges[0] = Utilities.CropBitmap(panoImg, relL, relT, relW, relH);
            //edges[0] = Utilities.skewBitmap(edges[0], this.Width, grid_TableContainer.ColumnDefinitions[1].Width.Value, edgeThick);
            //crop right edge
            relL = (tableSizeNormalized.X * 3 / 2 + tableSizeNormalized.Y) / tablePeriphery;
            relT = 0;
            relW = tableSizeNormalized.Y / tablePeriphery;
            relH = 1;
            edges[1] = Utilities.CropBitmap(panoImg, relL, relT, relW, relH);
            //edges[1] = Utilities.skewBitmap(edges[1], this.Height, grid_TableContainer.RowDefinitions[1].Height.Value, edgeThick);
            edges[1] = Utilities.rotateBitmapQuadraticAngle(edges[1], 90);
            //crop left edge
            relL = tableSizeNormalized.X / (2*tablePeriphery);
            relT = 0;
            relW = tableSizeNormalized.Y / tablePeriphery;
            relH = 1;
            edges[3] = Utilities.CropBitmap(panoImg, relL, relT, relW, relH);
            //edges[3] = Utilities.skewBitmap(edges[3], this.Height, grid_TableContainer.RowDefinitions[1].Height.Value, edgeThick);
            edges[3] = Utilities.rotateBitmapQuadraticAngle(edges[3], -90);
            //crop bottom edge
            relL = (tableSizeNormalized.X * 3 / 2 + 2 * tableSizeNormalized.Y) / tablePeriphery;
            relT = 0;
            relW = tableSizeNormalized.X/tablePeriphery;
            relH = 1;
            edges[2] = Utilities.CropBitmap(panoImg, relL, relT, relW, relH);
            //edges[2] = Utilities.skewBitmap(edges[2], this.Width, grid_TableContainer.ColumnDefinitions[1].Width.Value, edgeThick);
            edges[2] = Utilities.rotateBitmapQuadraticAngle(edges[2], 180);
            return edges;
        }

    }
}
