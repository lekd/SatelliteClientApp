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
    /// Interaction logic for PanoViewer.xaml
    /// </summary>
    public partial class PanoViewer : UserControl
    {
        Bitmap panoFrame;
        bool isInitialized = false;
        public PanoViewer()
        {
            InitializeComponent();
        }
        public void updatePanoImage(BitmapImage panoBmpImg)
        {
            initControls();
            imgPano.Source = panoBmpImg;
            panoFrame = Utilities.BitmapImageToJpegBitmap(panoBmpImg);
        }
        void initControls()
        {
            if(!isInitialized)
            {
                rectHighlightSegment.Width = imgPano.Width / 8;
            }
            isInitialized = true;
        }
    }
}
