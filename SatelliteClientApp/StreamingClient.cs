using MjpegProcessor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SatelliteClientApp
{
    public delegate void NewFrameAvailableHandler(int port, BitmapImage frameSrc);
    public class StreamingClient
    {
        MjpegDecoder _videoDecoder;
        int _port;
        string videoUri;
        public event NewFrameAvailableHandler NewFrameAvailableListener;
        public StreamingClient(string serverIP,int port)
        {
            videoUri = string.Format("http://{0}:{1}", serverIP, port);
            _port = port;
            _videoDecoder = new MjpegDecoder();
            _videoDecoder.FrameReady += videoDecoder_FrameReady;
        }
        public void Start()
        {
            _videoDecoder.ParseStream(new Uri(videoUri));
        }
        private void videoDecoder_FrameReady(object sender, FrameReadyEventArgs e)
        {
            
            if(NewFrameAvailableListener != null)
            {
                NewFrameAvailableListener(_port, e.BitmapImage);
            }
        }
    }
}
