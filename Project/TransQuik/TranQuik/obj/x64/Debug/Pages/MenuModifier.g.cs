﻿#pragma checksum "..\..\..\..\Pages\MenuModifier.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "06921E6475A29D09B59B9596EAC6AFA251295CF156AC287718EBCAED2D922DB1"
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
    /// MenuModifier
    /// </summary>
    public partial class MenuModifier : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 38 "..\..\..\..\Pages\MenuModifier.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button addOnSave;
        
        #line default
        #line hidden
        
        
        #line 52 "..\..\..\..\Pages\MenuModifier.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button addOnReset;
        
        #line default
        #line hidden
        
        
        #line 72 "..\..\..\..\Pages\MenuModifier.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid quantityItem;
        
        #line default
        #line hidden
        
        
        #line 94 "..\..\..\..\Pages\MenuModifier.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox quantityDisplay;
        
        #line default
        #line hidden
        
        
        #line 125 "..\..\..\..\Pages\MenuModifier.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid GridModifierModeGroup;
        
        #line default
        #line hidden
        
        
        #line 127 "..\..\..\..\Pages\MenuModifier.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid GridModifierModeMenu;
        
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
            System.Uri resourceLocater = new System.Uri("/TranQuik;component/pages/menumodifier.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Pages\MenuModifier.xaml"
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
            this.addOnSave = ((System.Windows.Controls.Button)(target));
            return;
            case 2:
            this.addOnReset = ((System.Windows.Controls.Button)(target));
            return;
            case 3:
            this.quantityItem = ((System.Windows.Controls.Grid)(target));
            return;
            case 4:
            this.quantityDisplay = ((System.Windows.Controls.TextBox)(target));
            
            #line 94 "..\..\..\..\Pages\MenuModifier.xaml"
            this.quantityDisplay.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.quantityDisplay_TextChanged);
            
            #line default
            #line hidden
            return;
            case 5:
            
            #line 95 "..\..\..\..\Pages\MenuModifier.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.NumberButton_Click);
            
            #line default
            #line hidden
            
            #line 95 "..\..\..\..\Pages\MenuModifier.xaml"
            ((System.Windows.Controls.Button)(target)).TouchDown += new System.EventHandler<System.Windows.Input.TouchEventArgs>(this.NumberButton_TouchDown);
            
            #line default
            #line hidden
            return;
            case 6:
            
            #line 96 "..\..\..\..\Pages\MenuModifier.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.NumberButton_Click);
            
            #line default
            #line hidden
            
            #line 96 "..\..\..\..\Pages\MenuModifier.xaml"
            ((System.Windows.Controls.Button)(target)).TouchDown += new System.EventHandler<System.Windows.Input.TouchEventArgs>(this.NumberButton_TouchDown);
            
            #line default
            #line hidden
            return;
            case 7:
            
            #line 97 "..\..\..\..\Pages\MenuModifier.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.NumberButton_Click);
            
            #line default
            #line hidden
            
            #line 97 "..\..\..\..\Pages\MenuModifier.xaml"
            ((System.Windows.Controls.Button)(target)).TouchDown += new System.EventHandler<System.Windows.Input.TouchEventArgs>(this.NumberButton_TouchDown);
            
            #line default
            #line hidden
            return;
            case 8:
            
            #line 98 "..\..\..\..\Pages\MenuModifier.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.NumberButton_Click);
            
            #line default
            #line hidden
            
            #line 98 "..\..\..\..\Pages\MenuModifier.xaml"
            ((System.Windows.Controls.Button)(target)).TouchDown += new System.EventHandler<System.Windows.Input.TouchEventArgs>(this.NumberButton_TouchDown);
            
            #line default
            #line hidden
            return;
            case 9:
            
            #line 99 "..\..\..\..\Pages\MenuModifier.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.NumberButton_Click);
            
            #line default
            #line hidden
            
            #line 99 "..\..\..\..\Pages\MenuModifier.xaml"
            ((System.Windows.Controls.Button)(target)).TouchDown += new System.EventHandler<System.Windows.Input.TouchEventArgs>(this.NumberButton_TouchDown);
            
            #line default
            #line hidden
            return;
            case 10:
            
            #line 100 "..\..\..\..\Pages\MenuModifier.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.NumberButton_Click);
            
            #line default
            #line hidden
            
            #line 100 "..\..\..\..\Pages\MenuModifier.xaml"
            ((System.Windows.Controls.Button)(target)).TouchDown += new System.EventHandler<System.Windows.Input.TouchEventArgs>(this.NumberButton_TouchDown);
            
            #line default
            #line hidden
            return;
            case 11:
            
            #line 101 "..\..\..\..\Pages\MenuModifier.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.NumberButton_Click);
            
            #line default
            #line hidden
            
            #line 101 "..\..\..\..\Pages\MenuModifier.xaml"
            ((System.Windows.Controls.Button)(target)).TouchDown += new System.EventHandler<System.Windows.Input.TouchEventArgs>(this.NumberButton_TouchDown);
            
            #line default
            #line hidden
            return;
            case 12:
            
            #line 102 "..\..\..\..\Pages\MenuModifier.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.NumberButton_Click);
            
            #line default
            #line hidden
            
            #line 102 "..\..\..\..\Pages\MenuModifier.xaml"
            ((System.Windows.Controls.Button)(target)).TouchDown += new System.EventHandler<System.Windows.Input.TouchEventArgs>(this.NumberButton_TouchDown);
            
            #line default
            #line hidden
            return;
            case 13:
            
            #line 103 "..\..\..\..\Pages\MenuModifier.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.NumberButton_Click);
            
            #line default
            #line hidden
            
            #line 103 "..\..\..\..\Pages\MenuModifier.xaml"
            ((System.Windows.Controls.Button)(target)).TouchDown += new System.EventHandler<System.Windows.Input.TouchEventArgs>(this.NumberButton_TouchDown);
            
            #line default
            #line hidden
            return;
            case 14:
            
            #line 104 "..\..\..\..\Pages\MenuModifier.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.NumberButton_Click);
            
            #line default
            #line hidden
            
            #line 104 "..\..\..\..\Pages\MenuModifier.xaml"
            ((System.Windows.Controls.Button)(target)).TouchDown += new System.EventHandler<System.Windows.Input.TouchEventArgs>(this.NumberButton_TouchDown);
            
            #line default
            #line hidden
            return;
            case 15:
            
            #line 105 "..\..\..\..\Pages\MenuModifier.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.BackspaceButton_Click);
            
            #line default
            #line hidden
            
            #line 105 "..\..\..\..\Pages\MenuModifier.xaml"
            ((System.Windows.Controls.Button)(target)).TouchDown += new System.EventHandler<System.Windows.Input.TouchEventArgs>(this.BackspaceButton_TouchDown);
            
            #line default
            #line hidden
            return;
            case 16:
            
            #line 118 "..\..\..\..\Pages\MenuModifier.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.deleteItem_Click);
            
            #line default
            #line hidden
            
            #line 118 "..\..\..\..\Pages\MenuModifier.xaml"
            ((System.Windows.Controls.Button)(target)).TouchDown += new System.EventHandler<System.Windows.Input.TouchEventArgs>(this.deleteItem_TouchDown);
            
            #line default
            #line hidden
            return;
            case 17:
            this.GridModifierModeGroup = ((System.Windows.Controls.Grid)(target));
            return;
            case 18:
            this.GridModifierModeMenu = ((System.Windows.Controls.Grid)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

