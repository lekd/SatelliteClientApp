﻿#pragma checksum "..\..\HubTableViewerControl.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "573D2F567F702412EBA932CC41BD6273E8186937"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using SatelliteClientApp;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace SatelliteClientApp {
    
    
    /// <summary>
    /// HubTableViewerControl
    /// </summary>
    public partial class HubTableViewerControl : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 8 "..\..\HubTableViewerControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal SatelliteClientApp.HubTableViewerControl mainView;
        
        #line default
        #line hidden
        
        
        #line 11 "..\..\HubTableViewerControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Canvas mainContainer;
        
        #line default
        #line hidden
        
        
        #line 14 "..\..\HubTableViewerControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid grid_TableContainer;
        
        #line default
        #line hidden
        
        
        #line 28 "..\..\HubTableViewerControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image img_TableContent;
        
        #line default
        #line hidden
        
        
        #line 33 "..\..\HubTableViewerControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image img_EdgeTop;
        
        #line default
        #line hidden
        
        
        #line 37 "..\..\HubTableViewerControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image img_EdgeLeft;
        
        #line default
        #line hidden
        
        
        #line 41 "..\..\HubTableViewerControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image img_EdgeRight;
        
        #line default
        #line hidden
        
        
        #line 45 "..\..\HubTableViewerControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image img_EdgeBottom;
        
        #line default
        #line hidden
        
        
        #line 50 "..\..\HubTableViewerControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image img_EdgeFocus;
        
        #line default
        #line hidden
        
        
        #line 53 "..\..\HubTableViewerControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image img_TableContentFocus;
        
        #line default
        #line hidden
        
        
        #line 56 "..\..\HubTableViewerControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image img_satAvatar;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/SatelliteClientApp;component/hubtableviewercontrol.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\HubTableViewerControl.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.mainView = ((SatelliteClientApp.HubTableViewerControl)(target));
            return;
            case 2:
            this.mainContainer = ((System.Windows.Controls.Canvas)(target));
            return;
            case 3:
            this.grid_TableContainer = ((System.Windows.Controls.Grid)(target));
            return;
            case 4:
            this.img_TableContent = ((System.Windows.Controls.Image)(target));
            
            #line 30 "..\..\HubTableViewerControl.xaml"
            this.img_TableContent.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.TableMouseUpDownEventHandler);
            
            #line default
            #line hidden
            
            #line 31 "..\..\HubTableViewerControl.xaml"
            this.img_TableContent.MouseMove += new System.Windows.Input.MouseEventHandler(this.TableMouseMoveEventHandler);
            
            #line default
            #line hidden
            
            #line 32 "..\..\HubTableViewerControl.xaml"
            this.img_TableContent.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.TableMouseLeftButtonUp);
            
            #line default
            #line hidden
            return;
            case 5:
            this.img_EdgeTop = ((System.Windows.Controls.Image)(target));
            
            #line 34 "..\..\HubTableViewerControl.xaml"
            this.img_EdgeTop.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.TableMouseUpDownEventHandler);
            
            #line default
            #line hidden
            
            #line 35 "..\..\HubTableViewerControl.xaml"
            this.img_EdgeTop.MouseMove += new System.Windows.Input.MouseEventHandler(this.TableMouseMoveEventHandler);
            
            #line default
            #line hidden
            
            #line 36 "..\..\HubTableViewerControl.xaml"
            this.img_EdgeTop.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.TableMouseLeftButtonUp);
            
            #line default
            #line hidden
            return;
            case 6:
            this.img_EdgeLeft = ((System.Windows.Controls.Image)(target));
            
            #line 38 "..\..\HubTableViewerControl.xaml"
            this.img_EdgeLeft.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.TableMouseUpDownEventHandler);
            
            #line default
            #line hidden
            
            #line 39 "..\..\HubTableViewerControl.xaml"
            this.img_EdgeLeft.MouseMove += new System.Windows.Input.MouseEventHandler(this.TableMouseMoveEventHandler);
            
            #line default
            #line hidden
            
            #line 40 "..\..\HubTableViewerControl.xaml"
            this.img_EdgeLeft.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.TableMouseLeftButtonUp);
            
            #line default
            #line hidden
            return;
            case 7:
            this.img_EdgeRight = ((System.Windows.Controls.Image)(target));
            
            #line 42 "..\..\HubTableViewerControl.xaml"
            this.img_EdgeRight.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.TableMouseUpDownEventHandler);
            
            #line default
            #line hidden
            
            #line 43 "..\..\HubTableViewerControl.xaml"
            this.img_EdgeRight.MouseMove += new System.Windows.Input.MouseEventHandler(this.TableMouseMoveEventHandler);
            
            #line default
            #line hidden
            
            #line 44 "..\..\HubTableViewerControl.xaml"
            this.img_EdgeRight.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.TableMouseLeftButtonUp);
            
            #line default
            #line hidden
            return;
            case 8:
            this.img_EdgeBottom = ((System.Windows.Controls.Image)(target));
            
            #line 46 "..\..\HubTableViewerControl.xaml"
            this.img_EdgeBottom.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.TableMouseUpDownEventHandler);
            
            #line default
            #line hidden
            
            #line 47 "..\..\HubTableViewerControl.xaml"
            this.img_EdgeBottom.MouseMove += new System.Windows.Input.MouseEventHandler(this.TableMouseMoveEventHandler);
            
            #line default
            #line hidden
            
            #line 48 "..\..\HubTableViewerControl.xaml"
            this.img_EdgeBottom.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.TableMouseLeftButtonUp);
            
            #line default
            #line hidden
            return;
            case 9:
            this.img_EdgeFocus = ((System.Windows.Controls.Image)(target));
            return;
            case 10:
            this.img_TableContentFocus = ((System.Windows.Controls.Image)(target));
            return;
            case 11:
            this.img_satAvatar = ((System.Windows.Controls.Image)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

