﻿#pragma checksum "..\..\..\Pages\ProductComponent - Copy.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "C42A7E7927BE8F2B8342AF3C6630C4FFA2726B612370A5D6AF7A785EEA408E75"
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
    /// ProductComponent
    /// </summary>
    public partial class ProductComponent : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 39 "..\..\..\Pages\ProductComponent - Copy.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid ProductComponentGroupButtonGrid;
        
        #line default
        #line hidden
        
        
        #line 60 "..\..\..\Pages\ProductComponent - Copy.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button NextComponentGroup_Button;
        
        #line default
        #line hidden
        
        
        #line 76 "..\..\..\Pages\ProductComponent - Copy.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button NextComponentButton;
        
        #line default
        #line hidden
        
        
        #line 87 "..\..\..\Pages\ProductComponent - Copy.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid quantityItem;
        
        #line default
        #line hidden
        
        
        #line 108 "..\..\..\Pages\ProductComponent - Copy.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox quantityDisplay;
        
        #line default
        #line hidden
        
        
        #line 129 "..\..\..\Pages\ProductComponent - Copy.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid ProductComponentButtonGrid;
        
        #line default
        #line hidden
        
        
        #line 151 "..\..\..\Pages\ProductComponent - Copy.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button SubmitButton;
        
        #line default
        #line hidden
        
        
        #line 165 "..\..\..\Pages\ProductComponent - Copy.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button CancelButton;
        
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
            System.Uri resourceLocater = new System.Uri("/TranQuik;component/pages/productcomponent%20-%20copy.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Pages\ProductComponent - Copy.xaml"
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
            this.ProductComponentGroupButtonGrid = ((System.Windows.Controls.Grid)(target));
            return;
            case 2:
            
            #line 55 "..\..\..\Pages\ProductComponent - Copy.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.PrevComponentGroup_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.NextComponentGroup_Button = ((System.Windows.Controls.Button)(target));
            
            #line 60 "..\..\..\Pages\ProductComponent - Copy.xaml"
            this.NextComponentGroup_Button.Click += new System.Windows.RoutedEventHandler(this.NextComponentGroup_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            
            #line 71 "..\..\..\Pages\ProductComponent - Copy.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.PrevComponent_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.NextComponentButton = ((System.Windows.Controls.Button)(target));
            
            #line 76 "..\..\..\Pages\ProductComponent - Copy.xaml"
            this.NextComponentButton.Click += new System.Windows.RoutedEventHandler(this.NextComponent_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.quantityItem = ((System.Windows.Controls.Grid)(target));
            return;
            case 7:
            this.quantityDisplay = ((System.Windows.Controls.TextBox)(target));
            
            #line 108 "..\..\..\Pages\ProductComponent - Copy.xaml"
            this.quantityDisplay.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.quantityDisplay_TextChanged);
            
            #line default
            #line hidden
            return;
            case 8:
            
            #line 109 "..\..\..\Pages\ProductComponent - Copy.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.NumberButton_Click);
            
            #line default
            #line hidden
            return;
            case 9:
            
            #line 110 "..\..\..\Pages\ProductComponent - Copy.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.NumberButton_Click);
            
            #line default
            #line hidden
            return;
            case 10:
            
            #line 111 "..\..\..\Pages\ProductComponent - Copy.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.NumberButton_Click);
            
            #line default
            #line hidden
            return;
            case 11:
            
            #line 112 "..\..\..\Pages\ProductComponent - Copy.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.NumberButton_Click);
            
            #line default
            #line hidden
            return;
            case 12:
            
            #line 113 "..\..\..\Pages\ProductComponent - Copy.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.NumberButton_Click);
            
            #line default
            #line hidden
            return;
            case 13:
            
            #line 114 "..\..\..\Pages\ProductComponent - Copy.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.NumberButton_Click);
            
            #line default
            #line hidden
            return;
            case 14:
            
            #line 115 "..\..\..\Pages\ProductComponent - Copy.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.NumberButton_Click);
            
            #line default
            #line hidden
            return;
            case 15:
            
            #line 116 "..\..\..\Pages\ProductComponent - Copy.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.NumberButton_Click);
            
            #line default
            #line hidden
            return;
            case 16:
            
            #line 117 "..\..\..\Pages\ProductComponent - Copy.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.NumberButton_Click);
            
            #line default
            #line hidden
            return;
            case 17:
            
            #line 118 "..\..\..\Pages\ProductComponent - Copy.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.NumberButton_Click);
            
            #line default
            #line hidden
            return;
            case 18:
            
            #line 119 "..\..\..\Pages\ProductComponent - Copy.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.BackspaceButton_Click);
            
            #line default
            #line hidden
            return;
            case 19:
            this.ProductComponentButtonGrid = ((System.Windows.Controls.Grid)(target));
            return;
            case 20:
            this.SubmitButton = ((System.Windows.Controls.Button)(target));
            
            #line 159 "..\..\..\Pages\ProductComponent - Copy.xaml"
            this.SubmitButton.Click += new System.Windows.RoutedEventHandler(this.SubmitButton_Click);
            
            #line default
            #line hidden
            return;
            case 21:
            this.CancelButton = ((System.Windows.Controls.Button)(target));
            
            #line 173 "..\..\..\Pages\ProductComponent - Copy.xaml"
            this.CancelButton.Click += new System.Windows.RoutedEventHandler(this.CancelButton_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

