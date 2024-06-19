﻿#pragma checksum "..\..\..\Pages\StartUpSelector.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "846D6D7B3DC7A16875350D13A080DB9A955D3EE0030013D3D01B37422835E3F4"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Material.Icons.WPF;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
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
using TranQuik.Pages;


namespace TranQuik.Pages {
    
    
    /// <summary>
    /// StartUpSelector
    /// </summary>
    public partial class StartUpSelector : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 17 "..\..\..\Pages\StartUpSelector.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid Selector;
        
        #line default
        #line hidden
        
        
        #line 27 "..\..\..\Pages\StartUpSelector.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button POS;
        
        #line default
        #line hidden
        
        
        #line 33 "..\..\..\Pages\StartUpSelector.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Sync;
        
        #line default
        #line hidden
        
        
        #line 39 "..\..\..\Pages\StartUpSelector.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ExitButton;
        
        #line default
        #line hidden
        
        
        #line 54 "..\..\..\Pages\StartUpSelector.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock LastSync;
        
        #line default
        #line hidden
        
        
        #line 58 "..\..\..\Pages\StartUpSelector.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock AutoSync;
        
        #line default
        #line hidden
        
        
        #line 62 "..\..\..\Pages\StartUpSelector.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock deviceType;
        
        #line default
        #line hidden
        
        
        #line 65 "..\..\..\Pages\StartUpSelector.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ProgressBar ProgressBarx;
        
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
            System.Uri resourceLocater = new System.Uri("/TranQuik;component/pages/startupselector.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Pages\StartUpSelector.xaml"
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
            this.Selector = ((System.Windows.Controls.Grid)(target));
            return;
            case 2:
            this.POS = ((System.Windows.Controls.Button)(target));
            
            #line 27 "..\..\..\Pages\StartUpSelector.xaml"
            this.POS.Click += new System.Windows.RoutedEventHandler(this.POS_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.Sync = ((System.Windows.Controls.Button)(target));
            
            #line 33 "..\..\..\Pages\StartUpSelector.xaml"
            this.Sync.Click += new System.Windows.RoutedEventHandler(this.SyncButton_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.ExitButton = ((System.Windows.Controls.Button)(target));
            
            #line 39 "..\..\..\Pages\StartUpSelector.xaml"
            this.ExitButton.Click += new System.Windows.RoutedEventHandler(this.ExitButton_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.LastSync = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 6:
            this.AutoSync = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 7:
            this.deviceType = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 8:
            this.ProgressBarx = ((System.Windows.Controls.ProgressBar)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

