﻿using DirectShowLib;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatelliteClientApp
{
    public delegate void CamNewFrameEventHandler(object sender, Bitmap bmp);
    public class CamRetriever
    {
        const int UPDATE_INTERVAL = 80;
        public event CamNewFrameEventHandler NewFrameAvailableEvent;
        protected VideoCapture camCapture;
        private DateTime lastUpdate;
        public int CamIndex { get; set; }
        public Bitmap CurrentFrame { get; set; }
        public RectangleF CropArea { get; set; }
        private bool isRunning = false;
        int frameWidth = 640;
        int frameHeight = 480;
        public bool IsStarted
        {
            get
            {
                return isRunning;
            }
        }
        public CamRetriever(int camIndex, int frameW=640,int frameH=480)
        {
            CamIndex = camIndex;
            lastUpdate = DateTime.Now;
            CropArea = new RectangleF(0, 0, 1, 1);
            frameWidth = frameW;
            frameHeight = frameH;
        }
        public void Start()
        {
            if (CamIndex >= 0)
            {
                camCapture = new VideoCapture(CamIndex);
                camCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, frameWidth);
                camCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, frameHeight);
                if (camCapture != null && camCapture.Ptr != IntPtr.Zero )
                {
                    camCapture.ImageGrabbed += ProcessFrame;
                    camCapture.Start();
                    isRunning = true;
                }
            }
        }
        public void Close()
        {
            isRunning = false;
            if (camCapture != null)
            {
                camCapture.Stop();
                camCapture.Dispose();
            }
        }
        Mat originFrame = new Mat();
        private void ProcessFrame(object sender, EventArgs arg)
        {
            if (camCapture != null && camCapture.Ptr != IntPtr.Zero)
            {
                try
                {
                    camCapture.Retrieve(originFrame, 0);
                    Bitmap bmp = originFrame.Bitmap;
                    if (CropArea != null)
                    {
                        bmp = Utilities.CropBitmap(bmp, CropArea.Left, CropArea.Top, CropArea.Width, CropArea.Height);
                    }
                    CurrentFrame = bmp;
                    if (NewFrameAvailableEvent != null)
                    {
                        NewFrameAvailableEvent(this, bmp);
                    }
                }
                catch(Exception ex)
                {
                    string msg = ex.Message;
                }

            }
        }
        public IEnumerable<Image> GrabFrames()
        {
            while (isRunning)
            {
                yield return CurrentFrame;
            }
            yield break;
        }
        public static string[] getCameraList()
        {
            DsDevice[] capDevices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            string[] cameraNames = new string[capDevices.Length];
            for (int i = 0; i < capDevices.Length; i++)
            {
                cameraNames[i] = capDevices[i].Name;
            }
            return cameraNames;
        }
    }
}
