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
using System.IO.Ports;
using System.Threading;
using System.Windows;
using System.Net;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace App2
{



    public partial class MainWindow : Window
    {

        SerialPort serialPort;
        private bool isPortOpen = false;
        private bool _isPortOpen;
        private readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        public MainWindow()
        {
            InitializeComponent();
            Title = "TITAN ROCKET PROJECT";
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(TitleBar);//�st bar�n se�im kodu

            string[] ports = SerialPort.GetPortNames();
            comboBoxPorts.ItemsSource = ports;
            if (ports.Length > 0)
                comboBoxPorts.SelectedIndex = 0;
            else
                alici.Text = "A��k COM port bulunamad�.";

            int[] baudRates = { 300, 600, 1200, 2400, 4800, 9600, 14400, 19200, 38400, 57600, 115200 };
            comboBoxBaudRates.ItemsSource = baudRates;
            comboBoxBaudRates.SelectedItem = 9600;
        }
        //open tu�u kodu
        private void buttonOpenPort_Click(object sender, RoutedEventArgs e)
        {

            if (comboBoxPorts.SelectedItem == null)
            {
                alici.Text = "L�tfen bir COM port se�in.";
                return;
            }
            if (comboBoxBaudRates.SelectedItem == null)
            {
                alici.Text = "L�tfen bir baud rate se�in.";
                return;
            }

            string portName = comboBoxPorts.SelectedItem.ToString();
            int baudRate = Convert.ToInt32(comboBoxBaudRates.SelectedItem);

            serialPort = new SerialPort(portName, baudRate);
            serialPort.DataReceived += SerialPort_DataReceived;

            try
            {
                serialPort.Open();
                alici.Text = $"{portName} portu, {baudRate} baud ile a��ld�.";
                buttonOpenPort.IsEnabled = false;
                buttonClosePort.IsEnabled = true;
                isPortOpen = true;

            }
            catch (Exception ex)
            {
                alici.Text = $"Port a��l�rken hata: {ex.Message}";
            }
        }
        //close tu�u kodu
        private async void buttonClosePort_Click(Object sender, RoutedEventArgs e)
        {

            if (isPortOpen = false)
            {

                try
                {
                    serialPort.Close();

                    alici.Text = "Port kapand�";
                    buttonClosePort.IsEnabled = false;
                    buttonOpenPort.IsEnabled = true;
                    isPortOpen = false;

                }
                catch
                {
                    alici.Text = "Port Kapat�lamad� Uygulamay� yeniden ba�lat�n�z";

                }
            }
            else
            {
                alici.Text = "Daha portu a�mad�n nas�l kapat�can";
                return;
            }


        }
        //serial porttan gelen kodu okuma kodu(de�i�ik bir c�mle oldu) 
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string data = serialPort.ReadExisting();

            dispatcherQueue.TryEnqueue(() =>
            {
                textBlockStatus.Text += "\nAl�nan veri: " + data;
            });
        }
    }
}