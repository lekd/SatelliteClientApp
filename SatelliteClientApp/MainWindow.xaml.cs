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
        CamRetriever satCam = null;
        StreamingServer satVidStreamer = null;
        public MainWindow()
        {
            InitializeComponent();
            panoStreamClient = new StreamingClient(Properties.Settings.Default.PanoServer, Properties.Settings.Default.PanoPort);
            panoStreamClient.NewFrameAvailableListener += Stream_NewFrameAvailableListener;
            panoStreamClient.Start();

            tableContentStreamClient = new StreamingClient(Properties.Settings.Default.PanoServer, Properties.Settings.Default.TablePort);
            tableContentStreamClient.NewFrameAvailableListener += Stream_NewFrameAvailableListener;
            tableContentStreamClient.Start();

            hubTableViewer.edgeFocusChangedEventHandler += HubTableViewer_edgeFocusChangedEventHandler;
            hubTableViewer.PanoFocusBoundary = panoViewer.FocusBoundary;
            hubTableViewer.readyToDisplaySatelliteEventHandler += HubTableViewer_readyToDisplaySatelliteEventHandler;
            panoViewer.panoFocusPosChangedHandler += hubTableViewer.panoFocusPosChangedHandler;

            if(Properties.Settings.Default.SatelliteRecording)
            {
                string[] camsList = CamRetriever.getCameraList();
                satCam = new CamRetriever(0);
                satCam.CropArea = new RectangleF(Properties.Settings.Default.AvatarCropLeft,
                                                    Properties.Settings.Default.AvatarCropTop,
                                                    Properties.Settings.Default.AvatarCropWidth,
                                                    Properties.Settings.Default.AvatarCropHeight);
                satCam.NewFrameAvailableEvent += SatCam_NewFrameAvailableEvent;
                
            }
        }

        private void HubTableViewer_readyToDisplaySatelliteEventHandler(bool isReady)
        {
            if(isReady)
            {
                if(satCam != null && !satCam.IsStarted)
                {
                    satCam.Start();
                    satVidStreamer = new StreamingServer(satCam.GrabFrames());
                    satVidStreamer.Start(Properties.Settings.Default.SatVideoPort);
                }
            }
        }

        private void SatCam_NewFrameAvailableEvent(int camIndex, Bitmap bmp)
        {
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
            Action displayAvatar = delegate
            {
                Bitmap downScale = Utilities.DownScaleBitmap(bmp, 8);
                hubTableViewer.updateSatelliteVideoFrame(downScale);
            };
            hubTableViewer.Dispatcher.Invoke(displayAvatar);
        }

        private void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            double w = grid_ViewsContainer.ActualWidth;
            double h = grid_ViewsContainer.RowDefinitions[1].ActualHeight;
            hubTableViewer.setWidth(w);
            hubTableViewer.setHeight(h);
            hubTableViewer.updateUIWithNewSize();
        }
        private void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            satCam?.Close();
            satCam = null;
            satVidStreamer?.Stop();
            satVidStreamer = null;
        }
        private void Stream_NewFrameAvailableListener(int port, BitmapImage frameSrc)
        {
            if(port == Properties.Settings.Default.PanoPort)
            {
                Action displayHubViewer = delegate
                {
                    Bitmap panoBmp = Utilities.BitmapImageToJpegBitmap(frameSrc.Clone());
                    panoBmp = Utilities.DownScaleBitmap(panoBmp, 8);
                    hubTableViewer.updateTableEdgesImages(panoBmp);
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
                    double h = grid_ViewsContainer.RowDefinitions[1].ActualHeight;
                    hubTableViewer.updateTableContent(bmp, w, h);
                };
                hubTableViewer.Dispatcher.Invoke(displayAction);
            }
        }


        #region children control events handler
        private void btn_Mode_Click(object sender, RoutedEventArgs e)
        {
            hubTableViewer.switchMode();
            if (hubTableViewer.ViewMode == HubTableViewerControl.TableViewMode.NORMAL)
            {
                hubTableViewer.Refresh();
                btn_Mode.Style = this.FindResource("AnchorButton") as Style;
            }
            else
            {
                btn_Mode.Style = this.FindResource("OkButton") as Style;
            }
        }
        private void HubTableViewer_edgeFocusChangedEventHandler(PointF relPos, double relAngularToSallite, double relativeW)
        {
            panoViewer.updateFocusWindow(relAngularToSallite, relativeW);
            hubTableViewer.displayPanoBubbleLink(panoViewer.FocusBoundary);
        }

        #endregion

        
    }
}
