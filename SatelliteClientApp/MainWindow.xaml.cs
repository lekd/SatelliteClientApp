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
                Action displayHubViewer = delegate
                {
                    Bitmap panoBmp = Utilities.BitmapImageToJpegBitmap(frameSrc.Clone());
                    panoBmp = Utilities.DownScaleBitmap(panoBmp, 8);
                    hubTableViewer.updateTableEdges(panoBmp);
                };
                hubTableViewer.Dispatcher.Invoke(displayHubViewer);
                Action displayPanoViewer = delegate
                {
                    BitmapImage bmpSrc = frameSrc.Clone();
                    panoViewer.updatePanoImage(bmpSrc,hubTableViewer.SatPositionInPano);
                    
                };
                panoViewer.Dispatcher.Invoke(displayPanoViewer);
                
            }
            else if(port == Properties.Settings.Default.TablePort)
            {
                Action displayAction = delegate
                {
                    
                    Bitmap bmp = Utilities.BitmapImageToJpegBitmap(frameSrc);
                    double w = grid_ViewsContainer.ActualWidth;
                    double h = grid_ViewsContainer.RowDefinitions[2].ActualHeight;
                    hubTableViewer.updateTableContent(bmp, w, h);
                };
                hubTableViewer.Dispatcher.Invoke(displayAction);
            }
        }

        private void btn_Mode_Click(object sender, RoutedEventArgs e)
        {
            hubTableViewer.SwitchMode();
            if(hubTableViewer.Mode == HubTableViewerControl.TableViewMode.NORMAL)
            {
                hubTableViewer.Refresh();
                btn_Mode.Content = "ANCHOR";
            }
            else
            {
                btn_Mode.Content = "SAVE";
            }
        }
    }
}
