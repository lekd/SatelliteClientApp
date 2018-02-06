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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        StreamingClient panoStreamClient;
        StreamingClient tableContentStreamClient;
        public MainWindow()
        {
            InitializeComponent();
            panoStreamClient = new StreamingClient(Properties.Settings.Default.PanoServer, Properties.Settings.Default.PanoPort);
            panoStreamClient.NewFrameAvailableListener += Stream_NewFrameAvailableListener;
            panoStreamClient.Start();

            tableContentStreamClient = new StreamingClient(Properties.Settings.Default.PanoServer, Properties.Settings.Default.TablePort);
            tableContentStreamClient.NewFrameAvailableListener += Stream_NewFrameAvailableListener;
            tableContentStreamClient.Start();
        }
        private void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            

            //panoStreamClient.Start();
            //tableContentStreamClient.Start();
        }
        private void Stream_NewFrameAvailableListener(int port, BitmapImage frameSrc)
        {
            if(port == Properties.Settings.Default.PanoPort)
            {
                Action displayPanoViewer = delegate
                {
                    BitmapImage bmpSrc = frameSrc.Clone();
                    panoViewer.updatePanoImage(bmpSrc);
                    
                };
                panoViewer.Dispatcher.Invoke(displayPanoViewer);
                Action displayHubViewer = delegate
                {
                    Bitmap panoBmp = Utilities.BitmapImageToJpegBitmap(frameSrc.Clone());
                    panoBmp = Utilities.DownScaleBitmap(panoBmp, 8);
                    hubTableViewer.updateTableEdges(panoBmp);
                };
                hubTableViewer.Dispatcher.Invoke(displayHubViewer);
            }
            else if(port == Properties.Settings.Default.TablePort)
            {
                Action displayAction = delegate
                {
                    
                    Bitmap bmp = Utilities.BitmapImageToJpegBitmap(frameSrc);
                    if (!hubTableViewer.WasInitialized)
                    {
                        double w = grid_ViewsContainer.ActualWidth;
                        double h = grid_ViewsContainer.RowDefinitions[1].ActualHeight;
                        hubTableViewer.initTableBoundary(bmp, w, h);
                    }
                    hubTableViewer.updateTableContent(bmp);
                };
                hubTableViewer.Dispatcher.Invoke(displayAction);
            }
        }

        
    }
}
