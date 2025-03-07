using System;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Net.Sockets;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinUI;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using SkiaSharp;
using LiveCharts;
using System.IO.Ports;
using System.Threading;
using System.Windows;
using System.Net;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using System.Reflection.Metadata;
using System.Text;



namespace App2;

public sealed partial class SplashScreenWindow : Window
{
    public SplashScreenWindow()
    {
        this.InitializeComponent();
        Task.Delay(4000).ContinueWith(_ => DispatcherQueue.TryEnqueue(Close));
    }

    
}

