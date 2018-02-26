using System;
using System.Collections.Generic;
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

namespace HubVideoConferencingApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        StreamingClient satVideoConferenceClient;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            img_VideoDisplay.Width = this.Width;
            img_VideoDisplay.Height = this.Height;
            satVideoConferenceClient = new StreamingClient(Properties.Settings.Default.SatelliteIPAddress,
                                            Properties.Settings.Default.SatelliteVideoPort);
            satVideoConferenceClient.NewFrameAvailableListener += SatVideoConferenceClient_NewFrameAvailableListener;
            satVideoConferenceClient.Start();
                
        }

        private void SatVideoConferenceClient_NewFrameAvailableListener(int port, BitmapImage frameSrc)
        {
            Action showVideoFrame = delegate
            {
                BitmapImage buf = frameSrc.Clone();
                img_VideoDisplay.Source = buf;

            };
            img_VideoDisplay.Dispatcher.Invoke(showVideoFrame);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
    }
}
