﻿#pragma checksum "..\..\..\..\Pages\ChildPage\SalesTypeReports.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "7C9D1F481588EC3122A11B0490403DC22AA2AA64F0F686F8B9936065D4A6ED19"
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
using TranQuik.Pages.ChildPage;


namespace TranQuik.Pages.ChildPage {
    
    
    /// <summary>
    /// SalesTypeReports
    /// </summary>
    public partial class SalesTypeReports : System.Windows.Controls.Page, System.Windows.Markup.IComponentConnector {
        
        
        #line 25 "..\..\..\..\Pages\ChildPage\SalesTypeReports.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock sessionReportsShopName;
        
        #line default
        #line hidden
        
        
        #line 29 "..\..\..\..\Pages\ChildPage\SalesTypeReports.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock sessionReportsPageName;
        
        #line default
        #line hidden
        
        
        #line 52 "..\..\..\..\Pages\ChildPage\SalesTypeReports.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DatePicker startDatePicker;
        
        #line default
        #line hidden
        
        
        #line 56 "..\..\..\..\Pages\ChildPage\SalesTypeReports.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DatePicker endDatePicker;
        
        #line default
        #line hidden
        
        
        #line 66 "..\..\..\..\Pages\ChildPage\SalesTypeReports.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox selectPOS;
        
        #line default
        #line hidden
        
        
        #line 70 "..\..\..\..\Pages\ChildPage\SalesTypeReports.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox selectSaleMode;
        
        #line default
        #line hidden
        
        
        #line 81 "..\..\..\..\Pages\ChildPage\SalesTypeReports.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button showReports;
        
        #line default
        #line hidden
        
        
        #line 87 "..\..\..\..\Pages\ChildPage\SalesTypeReports.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button printReports;
        
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
            System.Uri resourceLocater = new System.Uri("/TranQuik;component/pages/childpage/salestypereports.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Pages\ChildPage\SalesTypeReports.xaml"
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
            this.sessionReportsShopName = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 2:
            this.sessionReportsPageName = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 3:
            this.startDatePicker = ((System.Windows.Controls.DatePicker)(target));
            return;
            case 4:
            this.endDatePicker = ((System.Windows.Controls.DatePicker)(target));
            return;
            case 5:
            this.selectPOS = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 6:
            this.selectSaleMode = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 7:
            this.showReports = ((System.Windows.Controls.Button)(target));
            
            #line 81 "..\..\..\..\Pages\ChildPage\SalesTypeReports.xaml"
            this.showReports.Click += new System.Windows.RoutedEventHandler(this.showReports_Click);
            
            #line default
            #line hidden
            return;
            case 8:
            this.printReports = ((System.Windows.Controls.Button)(target));
            
            #line 88 "..\..\..\..\Pages\ChildPage\SalesTypeReports.xaml"
            this.printReports.Click += new System.Windows.RoutedEventHandler(this.printReports_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
