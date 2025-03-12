using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO.Ports;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using MUXC = Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls;
using CommunityToolkit.WinUI.Controls;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.ApplicationModel.Core;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml.Media;
using CommunityToolkit.WinUI.Behaviors;

namespace App2
{
    // LiveCharts i�in ViewModel
    public class ViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<ObservableValue> _values;
        private ObservableCollection<ISeries> _series;
        private int _maxDataPoints = 100;  // G�sterilecek maksimum veri noktas�
        private int _currentTimeIndex = 0;

        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModel()
        {
            _values = new ObservableCollection<ObservableValue>();
            // Ba�lang��ta bo� veri ekleyelim
            for (int i = 0; i < _maxDataPoints; i++)
            {
                _values.Add(new ObservableValue(0));
            }

            _series = new ObservableCollection<ISeries>
            {
                
                new LineSeries<ObservableValue>
                {
                    Values = _values,
                    Fill = null,
                    GeometrySize = 0,
                    Stroke = new SolidColorPaint(SKColors.Orange) { StrokeThickness = 2 },
                    Name = "LDR De�eri"
                }
            };
        }

        public ObservableCollection<ISeries> Series
        {
            get => _series;
            set
            {
                _series = value;
                OnPropertyChanged(nameof(Series));
            }
        }

        // X ekseni i�in ayarlar (�rne�in, zaman)
        public Axis[] XAxes { get; set; } = new Axis[]
        {
            new Axis
            {
                Name = "Zaman (sn)",
                NamePaint = new SolidColorPaint(SKColors.Black),
                LabelsPaint = new SolidColorPaint(SKColors.Blue),
                TextSize = 10,
                SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) { StrokeThickness = 1 },
                MinLimit = 0,
                MaxLimit = 100
            }
        };

        // Y ekseni i�in ayarlar (LDR sens�r de�eri: 0-1023)
        public Axis[] YAxes { get; set; } = new Axis[]
        {
            new Axis
            {
                Name = "LDR De�eri",
                NamePaint = new SolidColorPaint(SKColors.Red),
                LabelsPaint = new SolidColorPaint(SKColors.Green),
                TextSize = 12,
                SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray)
                {
                    StrokeThickness = 1,
                    PathEffect = new DashEffect(new float[] { 3, 3 })
                },
                MinLimit = 0,
                MaxLimit = 1023
            }
        };

        // Seri porttan gelen yeni veriyi ekle
        public void AddDataPoint(double value)
        {
            // Rolling window y�ntemiyle verileri g�ncelliyoruz
            _values[_currentTimeIndex % _maxDataPoints].Value = value;
            _currentTimeIndex++;

            // LiveCharts bazen sadece de�er g�ncellemesinde de�i�ikli�i alg�lamayabilir, 
            // bu y�zden Series koleksiyonundaki de�i�ikli�i bildiriyoruz.
            OnPropertyChanged(nameof(Series));
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // MainWindow kodlar�: Seri porttan veriyi al�p ViewModel �zerinden grafi�e g�nderiyoruz.
    public partial class MainWindow : Window
    {
        SerialPort serialPort;
        private bool isPortOpen = false;
        private readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        private ViewModel chartViewModel;

        public MainWindow()
        {
            InitializeComponent();
            
            if (MicaController.IsSupported())
            {
                this.SystemBackdrop = new MicaBackdrop()
                {
                    Kind = MicaKind.BaseAlt
                };
            }
            else if (DesktopAcrylicController.IsSupported())
            {
                this.SystemBackdrop = new DesktopAcrylicBackdrop();
            }
        
        Title = "TITAN ROCKET PROJECT";
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(TitleBar);

            // COM portlar�n� listele
            string[] ports = SerialPort.GetPortNames();
            comboBoxPorts.ItemsSource = ports;
            if (ports.Length > 0)
                comboBoxPorts.SelectedIndex = 0;
            else
                alici.Text = "A��k COM port bulunamad�.";

            // Baud rate se�imleri
            int[] baudRates = { 300, 600, 1200, 2400, 4800, 9600, 14400, 19200, 38400, 57600, 115200 };
            comboBoxBaudRates.ItemsSource = baudRates;
            

            // ViewModel olu�turup chart kontrol�ne DataContext atamas� yap�yoruz.
            chartViewModel = new ViewModel();
            chartControl.DataContext = chartViewModel;
        }

        // Port a�ma butonunun click metodu
        private void buttonOpenPort_Click(object sender, RoutedEventArgs e)
        {
            if (comboBoxBaudRates.SelectedItem == null)
            {
                var notification = new Notification
                {
                    Title = "Baud Rate Hatas�!!",
                    Message = "Baud rate se�ilmedi!!",
                    Severity = MUXC.InfoBarSeverity.Informational,
                };
                NotificationQueue.Show(notification);
            }



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
            if (!isPortOpen)
            {
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
            else
            {
                alici.Text = "Port zaten a��k.";
            }
        }

        // Port kapama butonunun click metodu
        private void buttonClosePort_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort == null)
            {
                alici.Text = "Port daha �nce a��lmam��.";
            }
            else
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
                    alici.Text = "Port kapat�lamad�. Uygulamay� yeniden ba�lat�n�z.";
                }
            }
        }

        // Seri porttan gelen veriyi okuma metodu
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = serialPort.ReadLine().Trim();
                System.Diagnostics.Debug.WriteLine("Gelen veri: " + data);

                // Arduino'dan gelen LDR sens�r verisinin, 0-1023 aras� bir say� oldu�unu varsay�yoruz.
                if (double.TryParse(data, NumberStyles.Any, CultureInfo.InvariantCulture, out double sensorValue))
                {
                    dispatcherQueue.TryEnqueue(() =>
                    {
                        chartViewModel.AddDataPoint(sensorValue);
                    });
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Veri parse edilemedi: " + data);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Veri okuma hatas�: " + ex.Message);
            }
        }
    }
}
