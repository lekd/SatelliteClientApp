using System;
using System.Collections.Generic;
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
        public MainWindow()
        {
            InitializeComponent();
            panoStreamClient = new StreamingClient(Properties.Settings.Default.PanoServer, Properties.Settings.Default.PanoPort);
            panoStreamClient.NewFrameAvailableListener += Stream_NewFrameAvailableListener;
            panoStreamClient.Start();
        }

        private void Stream_NewFrameAvailableListener(int port, BitmapImage frameSrc)
        {
            if(port == Properties.Settings.Default.PanoPort)
            {
                Action displayAction = delegate
                {
                    BitmapImage bmpSrc = frameSrc.Clone();
                    imgTestShower.Source = bmpSrc;
                };
                imgTestShower.Dispatcher.Invoke(displayAction);
            }
        }
    }
}
