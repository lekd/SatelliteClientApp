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
    /// Interaction logic for UC_TableFocusToolTip.xaml
    /// </summary>
    public partial class UC_TableFocusToolTip : UserControl
    {
        const double RelativeInnerRingSizeToOuterRing = 0.84;
        double tooltipRotationAngle = 0;
        PointF _relativePosInContainer = new PointF();
        PointF absoluteCenter = new PointF();
        public UC_TableFocusToolTip()
        {
            InitializeComponent();
        }
        public void updateRelativePos(PointF pos)
        {
            if(_relativePosInContainer.X != pos.X || _relativePosInContainer.Y != pos.Y)
            {
                _relativePosInContainer.X = pos.X;
                _relativePosInContainer.Y = pos.Y;
                enableTouch(false);
            }
        }
        public void SetSize(double Diameter)
        {
            this.Width = mainContainer.Width = Diameter;
            this.Height = mainContainer.Height =  Diameter;
            double handle_OuterRingDiameter = Diameter;
            double handle_InnerRingDiameter = Diameter * RelativeInnerRingSizeToOuterRing;
            //update handle container - outer-most ring
            handleContainer_OuterRing.RadiusX = Diameter / 2;
            handleContainer_OuterRing.RadiusY = Diameter / 2;
            handleContainer_OuterRing.Center = new System.Windows.Point(this.Width / 2, this.Height / 2);
            handleContainer_InnerRing.RadiusX = handle_InnerRingDiameter / 2;
            handleContainer_InnerRing.RadiusY = handle_InnerRingDiameter / 2;
            handleContainer_InnerRing.Center = new System.Windows.Point(this.Width / 2, this.Height / 2);
            //update ring - border of tooltip
            ring.Width = handle_InnerRingDiameter;
            ring.Height = handle_InnerRingDiameter;
            double ringLeft = (Diameter - ring.Width) / 2;
            double ringTop = (Diameter - ring.Height) / 2;
            ring.StrokeThickness = Diameter * 0.03;
            ring.SetValue(Canvas.LeftProperty, ringLeft);
            ring.SetValue(Canvas.TopProperty, ringTop);
            //update handle
            handleShape_OuterRing.RadiusX = handleShape_OuterRing.RadiusY = handle_OuterRingDiameter/2;
            handleShape_OuterRing.Center = new System.Windows.Point(this.Width / 2, this.Height / 2);
            handleShape_InnerRing.RadiusX = handleShape_InnerRing.RadiusY = (handle_InnerRingDiameter / 2) - 5;
            handleShape_InnerRing.Center = new System.Windows.Point(this.Width / 2, this.Height / 2);
            handleShape_clipPoly.StartPoint = new System.Windows.Point(0, 0);
            handleShape_clipLine1.Point = new System.Windows.Point(this.Width / 2, this.Height / 2);
            handleShape_clipLine2.Point = new System.Windows.Point(this.Width, 0);
            handleShape_clipLine3.Point = new System.Windows.Point(this.Width , this.Height);
            handleShape_clipLine4.Point = new System.Windows.Point(0, this.Height);
            handleShape.SetValue(Canvas.LeftProperty, (double)0);
            handleShape.SetValue(Canvas.TopProperty, (double)0);
            //update tooltip
            
            toolTipContent.Width = Diameter * (RelativeInnerRingSizeToOuterRing - 0.03);
            toolTipContent.Height = Diameter * (RelativeInnerRingSizeToOuterRing - 0.03);
            double toolTipLeft = (Diameter - toolTipContent.Width) / 2;
            double toolTipTop = (Diameter - toolTipContent.Height) / 2;
            toolTipContent.SetValue(Canvas.LeftProperty, toolTipLeft);
            toolTipContent.SetValue(Canvas.TopProperty, toolTipTop);

            absoluteCenter.X = (float)this.Width / 2;
            absoluteCenter.Y = (float)this.Height / 2;
            mainContainer.RenderTransform = new RotateTransform(0, absoluteCenter.X, absoluteCenter.Y);

        }
        public void updateToolTipContent(Bitmap content)
        {
            toolTipContent.Source = Utilities.ToBitmapImage(content, System.Drawing.Imaging.ImageFormat.Png);
        }
        public void enableTouch(bool isTouchEnabled)
        {
            handleContainer.IsHitTestVisible = isTouchEnabled;
            handleShape.IsHitTestVisible = isTouchEnabled;
        }
        #region mouse events
        private void handleContainer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                
                System.Windows.Point curMousePos = e.GetPosition(this);
                rotateToPoint(curMousePos);
            }
        }
        bool isMousePressedOnHandle = false;
        System.Windows.Point prevMousePosOnHandle = new System.Windows.Point();
        private void handleShape_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                isMousePressedOnHandle = true;
                prevMousePosOnHandle = e.GetPosition(this);
            }
        }

        private void handleShape_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed && isMousePressedOnHandle)
            {
                System.Windows.Point curMousePosOnHandle = e.GetPosition(this);
                Vector prevToCenter = new Vector(prevMousePosOnHandle.X - absoluteCenter.X, prevMousePosOnHandle.Y - absoluteCenter.Y);
                Vector curToCenter = new Vector(curMousePosOnHandle.X - absoluteCenter.X, curMousePosOnHandle.Y - absoluteCenter.Y);
                double rotAngle = Vector.AngleBetween(prevToCenter, curToCenter);
                tooltipRotationAngle += rotAngle;
                if (tooltipRotationAngle >= 360)
                {
                    tooltipRotationAngle -= 360;
                }
                if (tooltipRotationAngle < 0)
                {
                    tooltipRotationAngle += 360;
                }
                (mainContainer.RenderTransform as RotateTransform).Angle = tooltipRotationAngle;
            }
        }
        private void handleShape_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isMousePressedOnHandle = false;
        }

        private void handleShape_MouseLeave(object sender, MouseEventArgs e)
        {
            isMousePressedOnHandle = false;
        }
        #endregion
        #region rotate tooltip
        void rotateToPoint(System.Windows.Point newTouchPoint)
        {
            Vector baseVector = new Vector( absoluteCenter.X - absoluteCenter.X, 0 - absoluteCenter.Y);
            Vector targetVector = new Vector(newTouchPoint.X - absoluteCenter.X, newTouchPoint.Y - absoluteCenter.Y);
            tooltipRotationAngle = Vector.AngleBetween(baseVector, targetVector);
            if (tooltipRotationAngle >= 360)
            {
                tooltipRotationAngle -= 360;
            }
            if (tooltipRotationAngle < 0)
            {
                tooltipRotationAngle += 360;
            }
            (mainContainer.RenderTransform as RotateTransform).Angle = tooltipRotationAngle;
        }
        
        #endregion


    }
}
