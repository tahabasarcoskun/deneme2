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
using System;
using System.Windows;
using System.Net;
using System.Threading.Tasks;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace App2
{



        public partial class MainWindow : Window
        {
            SerialPort serialPort;

            public MainWindow()
            {
                InitializeComponent();
                // Sistemdeki COM portlarýný listele
                string[] ports = SerialPort.GetPortNames();
                comboBoxPorts.ItemsSource = ports;
                if (ports.Length > 0)
                    comboBoxPorts.SelectedIndex = 0;
                else
                    textBlockStatus.Text = "Açýk COM port bulunamadý.";

                // Yaygýn kullanýlan baud rate seçeneklerini ekle
                int[] baudRates = { 300, 600, 1200, 2400, 4800, 9600, 14400, 19200, 38400, 57600, 115200 };
                comboBoxBaudRates.ItemsSource = baudRates;
                comboBoxBaudRates.SelectedItem = 9600;
            }

            private void buttonOpenPort_Click(object sender, RoutedEventArgs e)
            {
                if (comboBoxPorts.SelectedItem == null)
                {
                    textBlockStatus.Text = "Lütfen bir COM port seçin.";
                    return;
                }
                if (comboBoxBaudRates.SelectedItem == null)
                {
                    textBlockStatus.Text = "Lütfen bir baud rate seçin.";
                    return;
                }

                string portName = comboBoxPorts.SelectedItem.ToString();
                int baudRate = Convert.ToInt32(comboBoxBaudRates.SelectedItem);

                // Seri portu belirtilen port ve baud rate ile yapýlandýr
                serialPort = new SerialPort(portName, baudRate);
                try
                {
                    serialPort.Open();
                    textBlockStatus.Text = $"{portName} portu, {baudRate} baud ile açýldý.";
                }
                catch (Exception ex)
                {
                    textBlockStatus.Text = $"Port açýlýrken hata: {ex.Message}";
                }
            }
        }
    }