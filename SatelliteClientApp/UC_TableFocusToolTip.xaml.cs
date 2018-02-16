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
        public UC_TableFocusToolTip()
        {
            InitializeComponent();
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
            handleShape_clipPoly.StartPoint = new System.Windows.Point(this.Width / 3, 0);
            handleShape_clipLine1.Point = new System.Windows.Point(this.Width / 2, this.Height / 2);
            handleShape_clipLine2.Point = new System.Windows.Point(this.Width*2 / 3, 0);
            handleShape_clipLine3.Point = new System.Windows.Point(this.Width, 0);
            handleShape_clipLine4.Point = new System.Windows.Point(this.Width , this.Height);
            handleShape_clipLine5.Point = new System.Windows.Point(0, this.Height);
            handleShape_clipLine6.Point = new System.Windows.Point(0, 0);
            handleShape.SetValue(Canvas.LeftProperty, (double)0);
            handleShape.SetValue(Canvas.TopProperty, (double)0);
            //update tooltip
            
            toolTipContent.Width = Diameter * (RelativeInnerRingSizeToOuterRing - 0.024);
            toolTipContent.Height = Diameter * (RelativeInnerRingSizeToOuterRing - 0.024);
            double toolTipLeft = (Diameter - toolTipContent.Width) / 2;
            double toolTipTop = (Diameter - toolTipContent.Height) / 2;
            toolTipContent.SetValue(Canvas.LeftProperty, toolTipLeft);
            toolTipContent.SetValue(Canvas.TopProperty, toolTipTop);

            
        }
        public void updateToolTipContent(Bitmap content)
        {
            toolTipContent.Source = Utilities.ToBitmapImage(content, System.Drawing.Imaging.ImageFormat.Png);
        }
        private void handle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
            
        }

        private void handle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void handle_MouseMove(object sender, MouseEventArgs e)
        {
            
        }

        private void handle_MouseEnter(object sender, MouseEventArgs e)
        {
            
        }

        private void handle_MouseLeave(object sender, MouseEventArgs e)
        {
            
        }
        public void AllowClickThrough(bool allow)
        {
            if(allow)
            {
                handleContainer.IsHitTestVisible = false;
            }
            else
            {
                handleContainer.IsHitTestVisible = true;
            }
        }
    }
}
