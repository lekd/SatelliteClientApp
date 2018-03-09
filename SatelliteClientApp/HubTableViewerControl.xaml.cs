using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
        public delegate void BoundaryChanged(double w, double h);
        public delegate void EdgeFocusChanged(PointF relPos, double relAngularToSallite, double relativeW);
        public delegate void TableFocusChanged(Bitmap tableFocus, double relPosX, double relPosY, double tableRotation);
        public delegate void TooltipControlRealeased();
        public delegate void TooltipInControlActivated();

        public enum TableViewMode { NORMAL, SATELLITE_ANCHOR}
        public enum TablePart { LEFT, RIGHT, TOP, BOTTOM, CENTER }
        const float MAX_RELATIVE_X = 0.5f;
        const float MAX_RELATIVE_Y = 0.5f;
        private System.Windows.Size defaultFocusWindowSize = new System.Windows.Size();
        private System.Windows.Size RelativeTableFocusSize = new System.Windows.Size(0.25f, 0.25f);

        Bitmap _tableContent;
        Bitmap _satAvatarBmp = new Bitmap(1, 1);
        PointF _satRelativePos = new PointF(-0.5f, -0.5f);
        public Boolean WasInitialized { get; set; }
        TableViewMode _viewMode = TableViewMode.NORMAL;
        public TableViewMode Mode
        {
            get { return _viewMode; }
        }

        double _avatarRotation = 0;
        double[] cornersAngles = new double[4];
        double _tableRotation = 0;
        PointF tableAbsoluteCenter = new PointF();

        public event BoundaryChanged boundaryChangeEventHandler = null;
        public event EdgeFocusChanged edgeFocusChangeEventHandler = null;
        public event TableFocusChanged tableFocusChangedEventHandler = null;
        public event TooltipControlRealeased tooltipControlReleasedEventHandler = null;
        public event TooltipInControlActivated tooltilControlledActivatedEventHandler = null;
        

        private Bitmap _circleMask;
        private Bitmap _highlightCircle;
        #region Properties & Field
        public double SatAvatarRotation
        {
            get
            {
                return _avatarRotation;
            }

            set
            {
                _avatarRotation = value;
                img_satAvatar.RenderTransform = new RotateTransform(_avatarRotation*180/Math.PI, img_satAvatar.Width / 2, img_satAvatar.Height / 2);
            }
        }
        public PointF SatRelativePosition
        {
            get
            {
                return _satRelativePos;
            }
            set
            {
                _satRelativePos = value;
                positionAvatar(_satRelativePos);
            }
        }
        public double SatPositionInPano
        {
            get
            {
                return getPositionInPanoFromRel2D(_satRelativePos);
            }
        }

        public PointF RelativeEdgeFocusCenter
        {
            get; set;
        }
        public PointF RelativeContentFocusCenter
        {
            get; set;
        }
        public PointF AbsFocusPosOnInView
        {
            get;set;
        }
        public double TableRotation
        {
            get
            {
                return _tableRotation;
            }

            set
            {
                _tableRotation = value;
                this.RenderTransform = new RotateTransform(_tableRotation*180/Math.PI, this.Width / 2, this.Height / 2);
            }
        }
        
        public Bitmap CircleMask
        {
            get
            {
                if(_circleMask == null)
                {
                    _circleMask = new Bitmap(300, 300);
                    using (var g = Graphics.FromImage(_circleMask))
                    {
                        SolidBrush whiteBrush = new SolidBrush(System.Drawing.Color.White);
                        SolidBrush transparentBrush = new SolidBrush(System.Drawing.Color.Transparent);
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        g.FillRectangle(transparentBrush, new RectangleF(0, 0, _circleMask.Width, _circleMask.Height));
                        g.FillEllipse(whiteBrush, new RectangleF(0, 0, _circleMask.Width, _circleMask.Height));
                    }
                }
                return _circleMask;
            }
            set
            {
                _circleMask = value;
            }
        }
        public Bitmap HighlightCircle
        {
            get
            {
                if(_highlightCircle == null)
                {
                    _highlightCircle = new Bitmap(200, 200);
                    using (var g = Graphics.FromImage(_highlightCircle))
                    {
                        SolidBrush transparentBrush = new SolidBrush(System.Drawing.Color.Transparent);
                        SolidBrush highlightBrush = new SolidBrush(System.Drawing.Color.Cyan);
                        SolidBrush semiWhite = new SolidBrush(System.Drawing.Color.FromArgb(50, 255, 255, 255));
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        g.FillRectangle(transparentBrush, new RectangleF(0, 0, _highlightCircle.Width, _highlightCircle.Height));
                        g.FillEllipse(semiWhite, new RectangleF(5, 5, _highlightCircle.Width - 10, _highlightCircle.Height-10));
                        g.DrawEllipse(new System.Drawing.Pen(highlightBrush, 3), new RectangleF(5, 5, _highlightCircle.Width-10, _highlightCircle.Height-10));
                    }
                }
                return _highlightCircle;
            }
        }
        public Rect TableBoundary
        {
            get; set;
        }
        #endregion
        public HubTableViewerControl()
        {
            InitializeComponent();
            _tableContent = new Bitmap(1, 1);
            WasInitialized = false;
            if(!Properties.Settings.Default.GazeRedirecting)
            {
                img_EdgeTop.Opacity = 0;
                img_EdgeBottom.Opacity = 0;
                img_EdgeLeft.Opacity = 0;
                img_EdgeRight.Opacity = 0;
            }
        }
        #region global command
        public void SwitchMode()
        {
            if(_viewMode == TableViewMode.NORMAL)
            {
                _viewMode = TableViewMode.SATELLITE_ANCHOR;
            }
            else
            {
                _viewMode = TableViewMode.NORMAL;
            }
        }
        public void Refresh()
        {
            TableRotation = getTableRotationAccordingToSatellitePosition(SatRelativePosition);
        }
        public void resetMouseState()
        {
            isMouseDownOnEdge = false;
            if(isMouseDownOnTable)
            {
                if(tooltipControlReleasedEventHandler != null)
                {
                    tooltipControlReleasedEventHandler();
                }
            }
            isMouseDownOnTable = false;
        }
        #endregion
        #region display information
        #region table content related
        public void updateTableContent(Bitmap tabContent, double maxW, double maxH)
        {
            if (tabContent.Width * 1.0 / tabContent.Height != _tableContent.Width * 1.0 / _tableContent.Height)
            {
                initTableBoundary(tabContent, maxW, maxH);
                img_satAvatar.Visibility = Visibility.Visible;
                string initAvatarFile = Directory.GetCurrentDirectory() + "\\Resources\\unknown_user.png";
                updateAvatarBitmap(new Bitmap(initAvatarFile));
                SatRelativePosition = new PointF(0f, 0.5f);
                
            }
            _tableContent = new Bitmap(tabContent);
            Bitmap downscaledContent = Utilities.DownScaleBitmap(_tableContent, 16);
            BitmapImage src = Utilities.ToBitmapImage(downscaledContent, ImageFormat.Jpeg);
            img_TableContent.Source = src;
            img_TableContent.Tag = downscaledContent;
            if(img_TableContentFocus.Opacity == 0)
            {
                updateContentFocus(RelativeContentFocusCenter, false);
            }
            else
            {
                updateContentFocus(RelativeContentFocusCenter, true);
            }
        }
        public void updateContentFocus(PointF relPos, bool show)
        {
            
            PointF displayCenter = new PointF();
            displayCenter.X = (float)(relPos.X * (img_TableContent.Width - img_TableContentFocus.Width) + tableAbsoluteCenter.X);
            displayCenter.Y = (float)(relPos.Y * (img_TableContent.Height - img_TableContentFocus.Height) + tableAbsoluteCenter.Y);
            double displayLeft = displayCenter.X - img_TableContentFocus.Width / 2;
            double displayTop = displayCenter.Y - img_TableContentFocus.Height / 2;

            double relLeft = (displayLeft - defaultFocusWindowSize.Height) / img_TableContent.Width;
            double relTop = (displayTop - defaultFocusWindowSize.Height) / img_TableContent.Height;

            Bitmap tableFocus = Utilities.CropBitmap(_tableContent, (float)relLeft, (float)relTop, (float)RelativeTableFocusSize.Width, (float)RelativeTableFocusSize.Height);
            tableFocus = Utilities.MaskBitmap(tableFocus, CircleMask);
            img_TableContentFocus.SetValue(Canvas.LeftProperty, displayLeft);
            img_TableContentFocus.SetValue(Canvas.TopProperty, displayTop);
            if (show)
            {
                img_TableContentFocus.Opacity = 1;
                if (tableFocusChangedEventHandler != null)
                {
                    PointF rotatedFocusCenter = Utilities.RotatePointAroundPoint(new PointF(0, 0), RelativeContentFocusCenter, TableRotation);
                    tableFocusChangedEventHandler(tableFocus, rotatedFocusCenter.X, rotatedFocusCenter.Y,TableRotation);
                }
            }
            try {
                img_TableContentFocus.Source = Utilities.ToBitmapImage(HighlightCircle,ImageFormat.Png);
                
            }
            catch(Exception ex)
            {
                String errorMsg = ex.Message;
            }


        }
        public PointF getRelativePosOnTable(PointF absPos)
        {
            PointF relPos = new PointF();
            double paddingX = img_TableContentFocus.Width;
            double paddingY = img_TableContentFocus.Height;
            relPos.X = (float)((absPos.X - tableAbsoluteCenter.X) / (img_TableContent.Width - paddingX));
            relPos.Y = (float)((absPos.Y - tableAbsoluteCenter.Y) / (img_TableContent.Height - paddingY));
            return relPos;
        }
        public void hideTableContentFocus()
        {
            Utilities.FadeControlOut(img_TableContentFocus, 0, 1, false);
        }
        #endregion

        #region edge related
        public void updateTableEdges(Bitmap panoImg)
        {
            if(!WasInitialized)
            {
                return;
            }
            Bitmap[] edgesBmps = segmentPanoIntoEdges(panoImg);
            //assign top edge
            BitmapImage topSrc = Utilities.ToBitmapImage(edgesBmps[0], ImageFormat.Png);
            img_EdgeTop.Source = topSrc;
            img_EdgeTop.Tag = edgesBmps[0];
            //assign right edge
            BitmapImage rightSrc = Utilities.ToBitmapImage(edgesBmps[1], ImageFormat.Png);
            img_EdgeRight.Source = rightSrc;
            img_EdgeRight.Tag = edgesBmps[1];
            //assign bottom edge
            BitmapImage bottomSrc = Utilities.ToBitmapImage(edgesBmps[2], ImageFormat.Png);
            img_EdgeBottom.Source = bottomSrc;
            img_EdgeBottom.Tag = edgesBmps[2];
            //assign left edge
            BitmapImage leftSrc = Utilities.ToBitmapImage(edgesBmps[3], ImageFormat.Png);
            img_EdgeLeft.Source = leftSrc;
            img_EdgeLeft.Tag = edgesBmps[3];

        }
        public void updateEdgeFocus(double angularDifToSatPos)
        {
            double angleOfSat = Utilities.AngleOfVector(new PointF(0, 0), SatRelativePosition, false);
            double angleOfEdgeFocus = angleOfSat + angularDifToSatPos;
            PointF relativePos = getRelativePositionFromAngle(angleOfEdgeFocus);
            RelativeEdgeFocusCenter = relativePos;
            updateEdgeFocus(relativePos);
            Utilities.FadeControlOut(img_EdgeFocus, 1, 2, false);
        }
        void updateEdgeFocus(PointF relativePos)
        {
            TablePart edge = belongToTableEdge(relativePos);
            Bitmap focusFrame = null;
            BendingDirection bendDirection = BendingDirection.None;
            double actualFocusHeight = 0;
            double actualFocusWidth = 0;
            PointF absFocusPos = new PointF();
            if (edge == TablePart.TOP)
            {
                double absX = (relativePos.X + 0.5) * this.Width;
                if(absX - defaultFocusWindowSize.Width/2 < 0)
                {
                    actualFocusWidth = absX + defaultFocusWindowSize.Width / 2;
                    bendDirection = BendingDirection.LeftDown;
                    absFocusPos.X = 0;
                    absFocusPos.Y = 0;
                }
                else if(absX + defaultFocusWindowSize.Width/2 > this.Width)
                {   
                    actualFocusWidth =  this.Width - (absX - defaultFocusWindowSize.Width / 2);
                    bendDirection = BendingDirection.RightDown;
                    absFocusPos.X = (float)(this.Width - actualFocusWidth);
                    absFocusPos.Y = 0;
                }
                else
                {
                    actualFocusWidth = defaultFocusWindowSize.Width;
                    absFocusPos.X = (float)(absX - actualFocusWidth / 2);
                    absFocusPos.Y = 0;
                }
                focusFrame = Utilities.DrawBendableRectangle(defaultFocusWindowSize.Width, defaultFocusWindowSize.Height,
                                                            actualFocusWidth, bendDirection);
            }
            else if(edge == TablePart.BOTTOM)
            {
                double absX = (relativePos.X + 0.5) * this.Width;
                if (absX - defaultFocusWindowSize.Width / 2 < 0)
                {
                    actualFocusWidth = absX + defaultFocusWindowSize.Width / 2;
                    actualFocusHeight = defaultFocusWindowSize.Width - actualFocusWidth;
                    actualFocusHeight = actualFocusHeight > defaultFocusWindowSize.Height ? actualFocusHeight : defaultFocusWindowSize.Height;
                    bendDirection = BendingDirection.LeftUp;
                    absFocusPos.X = 0;
                    absFocusPos.Y = (float)(this.Height - actualFocusHeight);
                }
                else if (absX + defaultFocusWindowSize.Width / 2 > this.Width)
                {
                    actualFocusWidth = this.Width - (absX - defaultFocusWindowSize.Width / 2);
                    actualFocusHeight = defaultFocusWindowSize.Width - actualFocusWidth;
                    actualFocusHeight = actualFocusHeight > defaultFocusWindowSize.Height ? actualFocusHeight : defaultFocusWindowSize.Height;
                    bendDirection = BendingDirection.RightUp;
                    absFocusPos.X = (float)(this.Width - actualFocusWidth);
                    absFocusPos.Y = (float)(this.Height - actualFocusHeight);
                }
                else
                {
                    actualFocusWidth = defaultFocusWindowSize.Width;
                    absFocusPos.Y = (float)(this.Height - defaultFocusWindowSize.Height);
                    absFocusPos.X = (float)(absX - defaultFocusWindowSize.Width / 2);
                }
                focusFrame = Utilities.DrawBendableRectangle(defaultFocusWindowSize.Width, defaultFocusWindowSize.Height,
                                                             actualFocusWidth, bendDirection);
            }
            else if(edge == TablePart.LEFT)
            {
                double absY = (relativePos.Y + 0.5) * this.Height;
                //here due to rotated 90 degree, the height of focus window is actually the width of the default size
                if (absY - defaultFocusWindowSize.Width/2 < 0)
                {
                    actualFocusHeight = absY + defaultFocusWindowSize.Width/2;
                    actualFocusWidth = defaultFocusWindowSize.Width - actualFocusHeight;
                    actualFocusWidth = actualFocusWidth > defaultFocusWindowSize.Height ? actualFocusWidth : defaultFocusWindowSize.Height;
                    bendDirection = BendingDirection.LeftDown;
                    focusFrame = Utilities.DrawBendableRectangle(defaultFocusWindowSize.Width, defaultFocusWindowSize.Height,
                                                             actualFocusWidth, bendDirection);
                    absFocusPos.X = 0;
                    absFocusPos.Y = 0;
                }
                else if(absY + defaultFocusWindowSize.Width/2 > this.Height)
                {
                    actualFocusHeight = this.Height - (absY - defaultFocusWindowSize.Width / 2);
                    actualFocusWidth = defaultFocusWindowSize.Width - actualFocusHeight;
                    actualFocusWidth = actualFocusWidth > defaultFocusWindowSize.Height ? actualFocusWidth : defaultFocusWindowSize.Height;
                    bendDirection = BendingDirection.LeftUp;
                    focusFrame = Utilities.DrawBendableRectangle(defaultFocusWindowSize.Width, defaultFocusWindowSize.Height,
                                                             actualFocusWidth, bendDirection);
                    absFocusPos.X = 0;
                    absFocusPos.Y = (float)(this.Height - actualFocusHeight);
                }
                else
                {
                    focusFrame = Utilities.DrawBendableRectangle(defaultFocusWindowSize.Height, defaultFocusWindowSize.Width,
                                                                   defaultFocusWindowSize.Height, BendingDirection.None);
                    absFocusPos.X = 0;
                    absFocusPos.Y = (float)(absY - defaultFocusWindowSize.Width / 2);
                }
            }
            else if(edge == TablePart.RIGHT)
            {
                double absY = (relativePos.Y + 0.5) * this.Height;
                //here due to rotated 90 degree, the height of focus window is actually the width of the default size
                if (absY - defaultFocusWindowSize.Width / 2 < 0)
                {
                    actualFocusHeight = absY + defaultFocusWindowSize.Width / 2;
                    actualFocusWidth = defaultFocusWindowSize.Width - actualFocusHeight;
                    actualFocusWidth = actualFocusWidth > defaultFocusWindowSize.Height ? actualFocusWidth : defaultFocusWindowSize.Height;
                    bendDirection = BendingDirection.RightDown;
                    focusFrame = Utilities.DrawBendableRectangle(defaultFocusWindowSize.Width, defaultFocusWindowSize.Height,
                                                             actualFocusWidth, bendDirection);
                    absFocusPos.Y = 0;
                    absFocusPos.X = (float)(this.Width - actualFocusWidth);
                }
                else if (absY + defaultFocusWindowSize.Width / 2 > this.Height)
                {
                    double oddY = absY + defaultFocusWindowSize.Width / 2 - this.Height;
                    actualFocusHeight = this.Height - (absY - defaultFocusWindowSize.Width / 2);
                    actualFocusWidth = defaultFocusWindowSize.Width - actualFocusHeight;
                    actualFocusWidth = actualFocusWidth > defaultFocusWindowSize.Height ? actualFocusWidth : defaultFocusWindowSize.Height;
                    bendDirection = BendingDirection.RightUp;
                    focusFrame = Utilities.DrawBendableRectangle(defaultFocusWindowSize.Width, defaultFocusWindowSize.Height,
                                                             actualFocusWidth, bendDirection);
                    absFocusPos.Y = (float)(this.Height - actualFocusHeight);
                    absFocusPos.X = (float)(this.Width - actualFocusWidth);
                }
                else
                {
                    focusFrame = Utilities.DrawBendableRectangle(defaultFocusWindowSize.Height, defaultFocusWindowSize.Width,
                                                                   defaultFocusWindowSize.Height, BendingDirection.None);
                    absFocusPos.X = (float)(this.Width - defaultFocusWindowSize.Height);
                    absFocusPos.Y = (float)(absY - defaultFocusWindowSize.Width / 2);
                }
            }
            if (focusFrame != null)
            {
                img_EdgeFocus.Source = Utilities.ToBitmapImage(focusFrame, ImageFormat.Png);
                img_EdgeFocus.Width = focusFrame.Width;
                img_EdgeFocus.Height = focusFrame.Height;
                img_EdgeFocus.SetValue(Canvas.LeftProperty, (double)absFocusPos.X);
                img_EdgeFocus.SetValue(Canvas.TopProperty, (double)absFocusPos.Y);
                img_EdgeFocus.BeginAnimation(UIElement.OpacityProperty, null);
                img_EdgeFocus.Opacity = 1.0;
            }
        }
        
        public void updateSatelliteVideoFrame(Bitmap satVidFrame)
        {
            Bitmap buf = new Bitmap(satVidFrame);
            double vidFrameRatio = buf.Width*1.0 / buf.Height;

            if(img_satAvatar.Width/img_satAvatar.Height != vidFrameRatio)
            {
                img_satAvatar.Width = img_satAvatar.Height * vidFrameRatio;
            }
            img_satAvatar.Source = Utilities.ToBitmapImage(buf, ImageFormat.Jpeg);
        }
        #endregion
        
        #region satellite avatar
        public void updateAvatarBitmap(Bitmap newAvatar)
        {
            
            if(_satAvatarBmp.Width != newAvatar.Width
                || _satAvatarBmp.Height != newAvatar.Height)
            {
                img_satAvatar.Height = (this.Height - img_TableContent.Height) / 2;
                img_satAvatar.Width = img_satAvatar.Height * newAvatar.Width / newAvatar.Height;
            }
            img_satAvatar.Source = Utilities.ToBitmapImage(new Bitmap(newAvatar), ImageFormat.Jpeg);
            _satAvatarBmp = newAvatar;
        }
        public void positionAvatar(PointF relativePos)
        {
            PointF avatarTopLeft = getDisplayTopLeftFromFromRelativePos(relativePos,img_satAvatar.Width, img_satAvatar.Height);
            img_satAvatar.SetValue(Canvas.LeftProperty, (double)avatarTopLeft.X);
            img_satAvatar.SetValue(Canvas.TopProperty, (double)avatarTopLeft.Y);
            SatAvatarRotation = getAvatarRotation(relativePos);
        }
        #endregion
        #endregion
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

            this.RenderTransform = new RotateTransform(0, this.Width / 2, this.Height / 2);
            img_TableContent.Width = tableDisplayW;
            img_TableContent.Height = tableDisplayH;

            tableAbsoluteCenter.X = (float)this.Width / 2;
            tableAbsoluteCenter.Y = (float)this.Height / 2;
            TableBoundary = new Rect(tableAbsoluteCenter.X - tableDisplayW / 2, tableAbsoluteCenter.Y - tableDisplayH / 2, tableDisplayW, tableDisplayH);

            //compute angles of corners
            //as angles go counter-clockwise with positive in the lower and negative in the upper part of table
            //bottom right
            PointF absBottomRight = new PointF((float)(tableAbsoluteCenter.X + tableDisplayW / 2), (float)(tableAbsoluteCenter.Y + tableDisplayH / 2));
            cornersAngles[0] = Utilities.AngleOfVector(tableAbsoluteCenter, absBottomRight, true);
            //bottom left
            PointF absBottomLeft = new PointF((float)(tableAbsoluteCenter.X - tableDisplayW/2), (float)(tableAbsoluteCenter.Y + tableDisplayH / 2));
            cornersAngles[1] = Utilities.AngleOfVector(tableAbsoluteCenter, absBottomLeft, true);
            //top left 
            PointF absTopLeft = new PointF((float)(tableAbsoluteCenter.X - tableDisplayW / 2), (float)(tableAbsoluteCenter.Y - tableDisplayH / 2));
            cornersAngles[2] = Utilities.AngleOfVector(tableAbsoluteCenter, absTopLeft, true);
            //top right
            PointF absTopRight = new PointF((float)(tableAbsoluteCenter.X + tableDisplayW / 2), (float)(tableAbsoluteCenter.Y - tableDisplayH / 2));
            cornersAngles[3] = Utilities.AngleOfVector(tableAbsoluteCenter, absTopRight, true);
            WasInitialized = true;

            if(boundaryChangeEventHandler != null)
            {
                boundaryChangeEventHandler(this.Width, this.Height);
            }

            defaultFocusWindowSize.Width = (this.Width + this.Height) * 2 / 8;
            defaultFocusWindowSize.Height = edgeThick;

            img_TableContentFocus.Width = img_TableContent.Width * RelativeTableFocusSize.Width;
            img_TableContentFocus.Height = img_TableContentFocus.Width;
            RelativeTableFocusSize.Height = img_TableContentFocus.Height / img_TableContent.Height;

            mainContainer.Width = this.Width;
            mainContainer.Height = this.Height;
        }
        Bitmap[] segmentPanoIntoEdges(Bitmap panoImg)
        {
            double edgeThick = grid_TableContainer.RowDefinitions[0].Height.Value;
            int downScaleFactor = 4;
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
            edges[0] = Utilities.SkewBitmap(edges[0], this.Width/downScaleFactor, img_TableContent.Width/downScaleFactor, edgeThick/downScaleFactor);
            //crop right edge
            relL = (tableSizeNormalized.X * 3 / 2 + tableSizeNormalized.Y) / tablePeriphery;
            relT = 0;
            relW = tableSizeNormalized.Y / tablePeriphery;
            relH = 1;
            edges[1] = Utilities.CropBitmap(panoImg, relL, relT, relW, relH);
            edges[1] = Utilities.SkewBitmap(edges[1], this.Height/downScaleFactor, img_TableContent.Height/downScaleFactor, edgeThick/downScaleFactor);
            edges[1] = Utilities.RotateBitmapQuadraticAngle(edges[1], 90);
            //crop left edge
            relL = tableSizeNormalized.X / (2*tablePeriphery);
            relT = 0;
            relW = tableSizeNormalized.Y / tablePeriphery;
            relH = 1;
            edges[3] = Utilities.CropBitmap(panoImg, relL, relT, relW, relH);
            edges[3] = Utilities.SkewBitmap(edges[3], this.Height/downScaleFactor, img_TableContent.Height/downScaleFactor, edgeThick/downScaleFactor);
            edges[3] = Utilities.RotateBitmapQuadraticAngle(edges[3], -90);
            //crop bottom edge
            relL = (tableSizeNormalized.X * 3 / 2 + 2 * tableSizeNormalized.Y) / tablePeriphery;
            relT = 0;
            relW = tableSizeNormalized.X/tablePeriphery;
            relH = 1;
            edges[2] = Utilities.CropBitmap(panoImg, relL, relT, relW, relH);
            edges[2] = Utilities.SkewBitmap(edges[2], this.Width/downScaleFactor, img_TableContent.Width/downScaleFactor, edgeThick/downScaleFactor);
            edges[2] = Utilities.RotateBitmapQuadraticAngle(edges[2], 180);
            return edges;
        }
        
        #region Vector-Angle conversion
        public double getAngularPositionRelativeToSatellite(PointF relativeP, PointF satelliteRelativePos)
        {
            double satAngle = Utilities.AngleOfVector(new PointF(0, 0), satelliteRelativePos, false);
            double targetAngle = Utilities.AngleOfVector(new PointF(0, 0), relativeP, false);
            return targetAngle - satAngle;
        }
        public double getPositionInPanoFromRel2D(PointF relPos)
        {
            double angle = Utilities.AngleOfVector(new PointF(0, 0), relPos, false);
            angle += Math.PI * 3 / 2;
            if(angle >= Math.PI*2)
            {
                angle -= Math.PI * 2;
            }
            double panoPortion = angle / (Math.PI * 2);
            return panoPortion;
        }
        public PointF getRelativePositionFromAngle(double angle)
        {
            PointF relativePos = new PointF();
            double angleIn360 = angle;
            if(angleIn360 <0)
            {
                angleIn360 += Math.PI * 2;
            }
            TablePart edge = belongToTableEdge(angleIn360, cornersAngles);
            if(edge == TablePart.TOP)
            {
                relativePos.Y = -0.5f;
                relativePos.X = (float)(Math.Cos(angle) * 0.5);
            }
            else if(edge == TablePart.BOTTOM)
            {
                relativePos.Y = 0.5f;
                relativePos.X = (float)(Math.Cos(angle) * 0.5);
            }
            else if(edge == TablePart.LEFT)
            {
                relativePos.X = -0.5f;
                relativePos.Y = (float)(Math.Sin(angle) * 0.5);
            }
            else if(edge == TablePart.RIGHT)
            {
                relativePos.X = 0.5f;
                relativePos.Y = (float)(Math.Sin(angle) * 0.5);
            }
            return relativePos;
        }
        public PointF getTableRelativePositionFromAbsoluteOne(PointF absPos)
        {
            PointF relativePos = new PointF();
            relativePos.X = (float)((absPos.X - tableAbsoluteCenter.X) / img_TableContent.Width);
            relativePos.Y = (float)((absPos.Y - tableAbsoluteCenter.Y) / img_TableContent.Height);
            if (Math.Abs(relativePos.X)>MAX_RELATIVE_X)
            {
                double sign = relativePos.X / Math.Abs(relativePos.X);
                relativePos.X = (float)(sign * MAX_RELATIVE_X);
            }
            if (Math.Abs(relativePos.Y) > MAX_RELATIVE_Y)
            {
                double sign = relativePos.Y / Math.Abs(relativePos.Y);
                relativePos.Y = (float)(sign * MAX_RELATIVE_Y);
            }
            return relativePos;
        }
        public PointF getTableAbsolutePositionFromRelativeOne(PointF relativePos)
        {
            PointF absolutePos = new PointF();
            absolutePos.X = (float)(relativePos.X * img_TableContent.Width + tableAbsoluteCenter.X);
            absolutePos.Y = (float)(relativePos.Y * img_TableContent.Height + tableAbsoluteCenter.Y);
            return absolutePos;
        }
        public PointF getDisplayTopLeftFromFromRelativePos(PointF relativePos,double displayW,double displayH)
        {
            TablePart edge = belongToTableEdge(relativePos);
            PointF absTopLeft = getTableAbsolutePositionFromRelativeOne(relativePos);
            double edgeThick = (this.Height - img_TableContent.Height) / 2;
            if(Math.Abs(relativePos.X)== MAX_RELATIVE_X && Math.Abs(relativePos.Y)== MAX_RELATIVE_Y)
            {
                PointF displayCenter = new PointF();
                double padding = Utilities.getRightEdgeFromHypotenuse(displayH / 2);
                displayCenter.X = (float)(absTopLeft.X + Math.Sign(relativePos.X) * padding);
                displayCenter.Y = (float)(absTopLeft.Y + Math.Sign(relativePos.Y) * padding);
                absTopLeft.X = (float)(displayCenter.X - displayW / 2);
                absTopLeft.Y = (float)(displayCenter.Y - displayH / 2);
                return absTopLeft;
            }
            if(edge == TablePart.TOP)
            {
                absTopLeft.Y = (float)(absTopLeft.Y - edgeThick / 2 - displayH / 2);
                absTopLeft.X = (float)(absTopLeft.X - displayW / 2);
                return absTopLeft;
            }
            if(edge == TablePart.BOTTOM)
            {
                absTopLeft.Y = (float)(absTopLeft.Y + edgeThick / 2 - displayH / 2);
                absTopLeft.X = (float)(absTopLeft.X - displayW / 2);
                return absTopLeft;
            }
            if(edge == TablePart.LEFT)
            {
                absTopLeft.X = (float)(absTopLeft.X - edgeThick/2 - displayW/2);
                absTopLeft.Y = (float)(absTopLeft.Y - displayH / 2);
                return absTopLeft;
            }
            if(edge == TablePart.RIGHT)
            {
                absTopLeft.X = (float)(absTopLeft.X + edgeThick / 2 - displayW / 2);
                absTopLeft.Y = (float)(absTopLeft.Y - displayH / 2);
                return absTopLeft;
            }
            absTopLeft.X -= (float)(displayW / 2);
            absTopLeft.Y -= (float)(displayH / 2);
            return absTopLeft;
        }
        double getAvatarRotation(PointF relativePos)
        {
            PointF directionVector = new PointF();
            directionVector.X = Math.Abs(relativePos.X) < MAX_RELATIVE_X ? 0 : relativePos.X;
            directionVector.Y = Math.Abs(relativePos.Y) < MAX_RELATIVE_Y ? 0 : relativePos.Y;
            double directVectorAngle = Utilities.AngleOfVector(new PointF(0, 0), directionVector, false);
            return directVectorAngle + Math.PI / 2;
        }
        double getTableRotationAccordingToSatellitePosition(PointF relativePos)
        {
            if(relativePos.Y == -MAX_RELATIVE_Y)
            {
                return Math.PI;
            }
            else if(relativePos.X == MAX_RELATIVE_X)
            {
                return Math.PI / 2;
            }
            else if(relativePos.X == -MAX_RELATIVE_X)
            {
                return -Math.PI / 2;
            }
            return 0;
        }
        public PointF getCenterPointOfEdgeFocus(PointF relativePos)
        {
            double midHorizontalBorderLen = (this.Width + img_TableContent.Width) / 2;
            double midVerticalBorderLen = (this.Height + img_TableContent.Height) / 2;
            PointF center = new PointF();
            center.X = (float)(tableAbsoluteCenter.X + relativePos.X * midHorizontalBorderLen);
            center.Y = (float)(tableAbsoluteCenter.Y + relativePos.Y * midVerticalBorderLen);
            return center;
        }
        #endregion
        #region logic checking
        public TablePart belongToTableEdge(double angleAroundTable, double[] cornersAngles)
        {
            if (angleAroundTable >= cornersAngles[0] && angleAroundTable < cornersAngles[1])
            {
                return TablePart.BOTTOM;
            }
            if (angleAroundTable >= cornersAngles[1] && angleAroundTable < cornersAngles[2])
            {
                return TablePart.LEFT;
            }
            if (angleAroundTable >= cornersAngles[2] && angleAroundTable < cornersAngles[3])
            {
                return TablePart.TOP;
            }
            return TablePart.RIGHT;
        }
        public TablePart belongToTableEdge(PointF relativePos)
        {
            if(relativePos.Y == -MAX_RELATIVE_Y)
            {
                return TablePart.TOP;
            }
            if(relativePos.Y == MAX_RELATIVE_Y)
            {
                return TablePart.BOTTOM;
            }
            if(relativePos.X == -MAX_RELATIVE_X)
            {
                return TablePart.LEFT;
            }
            if(relativePos.X == MAX_RELATIVE_X)
            {
                return TablePart.RIGHT;
            }
            return TablePart.CENTER;
        }
        public TablePart getTablePartFromUIElement(object UIElement)
        {
            if(UIElement == img_EdgeTop)
            {
                return TablePart.TOP;
            }
            if (UIElement == img_EdgeBottom)
            {
                return TablePart.BOTTOM;
            }
            if (UIElement == img_EdgeLeft)
            {
                return TablePart.LEFT;
            }
            if (UIElement == img_EdgeRight)
            {
                return TablePart.RIGHT;
            }
            return TablePart.CENTER;
        }
        System.Windows.Point ensurePointCompletelyInTable(System.Windows.Point absPos)
        {
            System.Windows.Point thresholdedP = new System.Windows.Point();
            thresholdedP.X = absPos.X;
            thresholdedP.Y = absPos.Y;
            double padding = 1;
            if(thresholdedP.X - img_TableContentFocus.Width/2< TableBoundary.Left + padding)
            {
                thresholdedP.X = TableBoundary.Left + img_TableContentFocus.Width/2 + padding;
            }
            if(thresholdedP.X + img_TableContentFocus.Width / 2 >TableBoundary.Right - padding)
            {
                thresholdedP.X = TableBoundary.Right - img_TableContentFocus.Width / 2 - padding;
            }
            if(thresholdedP.Y - img_TableContentFocus.Height/2 < TableBoundary.Top + padding)
            {
                thresholdedP.Y = TableBoundary.Top + img_TableContentFocus.Height / 2 + padding;
            }
            if(thresholdedP.Y + img_TableContentFocus.Height / 2 > TableBoundary.Bottom - padding)
            {
                thresholdedP.Y = TableBoundary.Bottom - img_TableContentFocus.Height / 2 - padding; 
            }
            return thresholdedP;
        }
        #endregion
        #region Mouse events handlers
        bool isMouseDownOnEdge = false;
        bool isMouseDownOnTable = false;
        private void TableMouseDownEventHandler(object sender,MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                TablePart touchedPart = getTablePartFromUIElement(sender);
                if (touchedPart != TablePart.CENTER)
                {
                    isMouseDownOnEdge = true;
                    System.Windows.Point globalMousePos = e.GetPosition(this);
                    PointF relativeToTable = getTableRelativePositionFromAbsoluteOne(new PointF((float)globalMousePos.X, (float)globalMousePos.Y));
                    if (_viewMode == TableViewMode.SATELLITE_ANCHOR)
                    {
                        positionAvatar(relativeToTable);
                    }
                    else
                    {
                        if(!Properties.Settings.Default.GazeRedirecting)
                        {
                            return;
                        }
                        RelativeEdgeFocusCenter = relativeToTable;
                        double relativeAngularDif = getAngularPositionRelativeToSatellite(relativeToTable, SatRelativePosition);
                        updateEdgeFocus(relativeToTable);
                        if (edgeFocusChangeEventHandler != null)
                        {
                            edgeFocusChangeEventHandler(relativeToTable, relativeAngularDif, 1.0 / 8);
                        }
                    }
                }
                else
                {
                    isMouseDownOnTable = true;
                    if(_viewMode == TableViewMode.NORMAL)
                    {
                        System.Windows.Point globalMousePos = e.GetPosition(this);
                        globalMousePos = ensurePointCompletelyInTable(globalMousePos);
                        AbsFocusPosOnInView = new PointF((float)globalMousePos.X, (float)globalMousePos.Y);
                        PointF relPos = getRelativePosOnTable(new PointF((float)globalMousePos.X, (float)globalMousePos.Y));
                        RelativeContentFocusCenter = relPos;
                        updateContentFocus(relPos,true);
                        isMouseDownOnTable = true;
                        if(tooltilControlledActivatedEventHandler != null)
                        {
                            tooltilControlledActivatedEventHandler();
                        }
                    }
                }
            }
        }
        private void TableMouseMoveEventHandler(object sender, MouseEventArgs e)
        {
            TablePart touchedPart = getTablePartFromUIElement(sender);
            if (touchedPart != TablePart.CENTER)
            {
                if (isMouseDownOnEdge && e.LeftButton == MouseButtonState.Pressed)
                { 
                    System.Windows.Point globalMousePos = e.GetPosition(this);
                    PointF relativeToTable = getTableRelativePositionFromAbsoluteOne(new PointF((float)globalMousePos.X, (float)globalMousePos.Y));
                    if (_viewMode == TableViewMode.SATELLITE_ANCHOR)
                    {
                        SatRelativePosition = relativeToTable;
                    }
                    else
                    {
                        if(!Properties.Settings.Default.GazeRedirecting)
                        {
                            return;
                        }
                        RelativeEdgeFocusCenter = relativeToTable;
                        double relativeAngularDif = getAngularPositionRelativeToSatellite(relativeToTable, SatRelativePosition);
                        updateEdgeFocus(relativeToTable);
                        if (edgeFocusChangeEventHandler != null)
                        {
                            edgeFocusChangeEventHandler(relativeToTable, relativeAngularDif, 1.0 / 8);
                        }
                    }
                }
            }
            else
            {
                if (_viewMode == TableViewMode.NORMAL && isMouseDownOnTable && e.LeftButton == MouseButtonState.Pressed)
                {
                    System.Windows.Point globalMousePos = e.GetPosition(this);
                    globalMousePos = ensurePointCompletelyInTable(globalMousePos);
                    AbsFocusPosOnInView = new PointF((float)globalMousePos.X, (float)globalMousePos.Y);
                    PointF relPos = getRelativePosOnTable(new PointF((float)globalMousePos.X, (float)globalMousePos.Y));
                    RelativeContentFocusCenter = relPos;
                    updateContentFocus(relPos,true);
                }
            }
        }
        private void TableMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //start fade-out animation
            if(isMouseDownOnEdge && Mode == TableViewMode.NORMAL)
            {
                
                Utilities.FadeControlOut(img_EdgeFocus, 1, 2, false);

            }
            if( isMouseDownOnTable)
            {
                if(tooltipControlReleasedEventHandler != null)
                {
                    tooltipControlReleasedEventHandler();
                }
            }
            resetMouseState();
        }

        #endregion


    }
}
