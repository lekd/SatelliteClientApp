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
        public enum TableViewMode { NORMAL, SATELLITE_ANCHOR}
        public enum TablePart { LEFT, RIGHT, TOP, BOTTOM, CENTER }
        const float MAX_RELATIVE_X = 0.5f;
        const float MAX_RELATIVE_Y = 0.5f;
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
        
        #endregion
        public HubTableViewerControl()
        {
            InitializeComponent();
            _tableContent = new Bitmap(1, 1);
            WasInitialized = false;
            
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
        #endregion
        #region display information
        #region table related
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
            BitmapImage src = Utilities.ToBitmapImage(_tableContent, ImageFormat.Jpeg);
            img_TableContent.Source = src;
        }
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
            edges[0] = Utilities.skewBitmap(edges[0], this.Width/downScaleFactor, img_TableContent.Width/downScaleFactor, edgeThick/downScaleFactor);
            //crop right edge
            relL = (tableSizeNormalized.X * 3 / 2 + tableSizeNormalized.Y) / tablePeriphery;
            relT = 0;
            relW = tableSizeNormalized.Y / tablePeriphery;
            relH = 1;
            edges[1] = Utilities.CropBitmap(panoImg, relL, relT, relW, relH);
            edges[1] = Utilities.skewBitmap(edges[1], this.Height/downScaleFactor, img_TableContent.Height/downScaleFactor, edgeThick/downScaleFactor);
            edges[1] = Utilities.rotateBitmapQuadraticAngle(edges[1], 90);
            //crop left edge
            relL = tableSizeNormalized.X / (2*tablePeriphery);
            relT = 0;
            relW = tableSizeNormalized.Y / tablePeriphery;
            relH = 1;
            edges[3] = Utilities.CropBitmap(panoImg, relL, relT, relW, relH);
            edges[3] = Utilities.skewBitmap(edges[3], this.Height/downScaleFactor, img_TableContent.Height/downScaleFactor, edgeThick/downScaleFactor);
            edges[3] = Utilities.rotateBitmapQuadraticAngle(edges[3], -90);
            //crop bottom edge
            relL = (tableSizeNormalized.X * 3 / 2 + 2 * tableSizeNormalized.Y) / tablePeriphery;
            relT = 0;
            relW = tableSizeNormalized.X/tablePeriphery;
            relH = 1;
            edges[2] = Utilities.CropBitmap(panoImg, relL, relT, relW, relH);
            edges[2] = Utilities.skewBitmap(edges[2], this.Width/downScaleFactor, img_TableContent.Width/downScaleFactor, edgeThick/downScaleFactor);
            edges[2] = Utilities.rotateBitmapQuadraticAngle(edges[2], 180);
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
        #endregion
        #region Mouse events handlers
        bool isMouseDownOnEdge = false;
        private void TableMouseUpDownEventHandler(object sender,MouseButtonEventArgs e)
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
                        double relativeAngularDif = getAngularPositionRelativeToSatellite(relativeToTable, SatRelativePosition);
                        if(edgeFocusChangeEventHandler != null)
                        {
                            edgeFocusChangeEventHandler(relativeToTable, relativeAngularDif, 1.0/8);
                        }
                    }
                }
            }
            else
            {
                isMouseDownOnEdge = false;
            }
        }
        private void TableMouseMoveEventHandler(object sender, MouseEventArgs e)
        {
            TablePart touchedPart = getTablePartFromUIElement(sender);
            if (touchedPart != TablePart.CENTER)
            {
                if (isMouseDownOnEdge)
                { 
                    System.Windows.Point globalMousePos = e.GetPosition(this);
                    PointF relativeToTable = getTableRelativePositionFromAbsoluteOne(new PointF((float)globalMousePos.X, (float)globalMousePos.Y));
                    if (_viewMode == TableViewMode.SATELLITE_ANCHOR)
                    {
                        SatRelativePosition = relativeToTable;
                    }
                    else
                    {
                        double relativeAngularDif = getAngularPositionRelativeToSatellite(relativeToTable, SatRelativePosition);
                        if (edgeFocusChangeEventHandler != null)
                        {
                            edgeFocusChangeEventHandler(relativeToTable, relativeAngularDif, 1.0/8);
                        }
                    }
                }
            }
            
        }
        #endregion

    }
}
