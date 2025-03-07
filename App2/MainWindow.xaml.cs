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
using System.Collections.Generic;
using System.Windows;
using System.Net;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using System.Reflection.Metadata;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace App2
{
    public class ViewModel
    {
        public ISeries[] Series { get; set; } = new ISeries[]
        {
            new ColumnSeries<int>(new int[] { 3, 4, 2 }),
            new ColumnSeries<int>(new int[] { 4, 2, 6 }),
            new ColumnSeries<double, DiamondGeometry>(new double[] { 4, 3, 4 })
        };
    }



    public partial class MainWindow : Window
    {

        SerialPort serialPort;
        private bool isPortOpen=false;
        
        private readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        
       

        public MainWindow()
        {
            InitializeComponent();
         

            Title = "TITAN ROCKET PROJECT";
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(TitleBar);//üst barýn seçim kodu

            string[] ports = SerialPort.GetPortNames();
            comboBoxPorts.ItemsSource = ports;
            if (ports.Length > 0)
                comboBoxPorts.SelectedIndex = 0;
            else
                alici.Text = "Açýk COM port bulunamadý.";

            int[] baudRates = { 300, 600, 1200, 2400, 4800, 9600, 14400, 19200, 38400, 57600, 115200 };
            comboBoxBaudRates.ItemsSource = baudRates;
            comboBoxBaudRates.SelectedItem = 9600;
        }
        //open tuþu kodu
        private void buttonOpenPort_Click(object sender, RoutedEventArgs e)
        {

            if (comboBoxPorts.SelectedItem == null)
            {
                alici.Text = "Lütfen bir COM port seçin.";
                return;
            }
            if (comboBoxBaudRates.SelectedItem == null)
            {
                alici.Text = "Lütfen bir baud rate seçin.";
                return;
            }

            string portName = comboBoxPorts.SelectedItem.ToString();
            int baudRate = Convert.ToInt32(comboBoxBaudRates.SelectedItem);

            serialPort = new SerialPort(portName, baudRate);
            serialPort.DataReceived += SerialPort_DataReceived;
            if (isPortOpen)
            {
                try
                {
                    serialPort.Open();
                    alici.Text = $"{portName} portu, {baudRate} baud ile açýldý.";
                    buttonOpenPort.IsEnabled = false;
                    buttonClosePort.IsEnabled = true;
                    isPortOpen = true;


                }
                catch (Exception ex)
                {
                    alici.Text = $"Port açýlýrken hata: {ex.Message}";
                }

            }
            else
            {
                alici.Text = "Port zaten açýk.";
            }
        }
        //close tuþu kodu
        private async void buttonClosePort_Click(Object sender, RoutedEventArgs e)
        {

            if (serialPort==null)
            {
                alici.Text = "Daha portu açmadýn amk";
              
            }
            else
            {
                try
                {
                    serialPort.Close();

                    alici.Text = "Port kapandý";
                    buttonClosePort.IsEnabled = false;
                    buttonOpenPort.IsEnabled = true;
                    isPortOpen = false;


                }
                catch
                {
                    alici.Text = "Port Kapatýlamadý Uygulamayý yeniden baþlatýnýz";

                }
            }
            
            


        }
        //serial porttan gelen kodu okuma kodu(deðiþik bir cümle oldu) 
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string data = serialPort.ReadExisting();

            dispatcherQueue.TryEnqueue(() =>
            {
                textBlockStatus.Text += "\nAlýnan veri: " + data;
            });
        }
    }

    
}