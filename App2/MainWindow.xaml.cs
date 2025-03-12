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
    // LiveCharts için ViewModel
    public class ViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<ObservableValue> _values;
        private ObservableCollection<ISeries> _series;
        private int _maxDataPoints = 100;  // Gösterilecek maksimum veri noktasý
        private int _currentTimeIndex = 0;

        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModel()
        {
            _values = new ObservableCollection<ObservableValue>();
            // Baþlangýçta boþ veri ekleyelim
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
                    Name = "LDR Deðeri"
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

        // X ekseni için ayarlar (örneðin, zaman)
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

        // Y ekseni için ayarlar (LDR sensör deðeri: 0-1023)
        public Axis[] YAxes { get; set; } = new Axis[]
        {
            new Axis
            {
                Name = "LDR Deðeri",
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
            // Rolling window yöntemiyle verileri güncelliyoruz
            _values[_currentTimeIndex % _maxDataPoints].Value = value;
            _currentTimeIndex++;

            // LiveCharts bazen sadece deðer güncellemesinde deðiþikliði algýlamayabilir, 
            // bu yüzden Series koleksiyonundaki deðiþikliði bildiriyoruz.
            OnPropertyChanged(nameof(Series));
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // MainWindow kodlarý: Seri porttan veriyi alýp ViewModel üzerinden grafiðe gönderiyoruz.
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

            // COM portlarýný listele
            string[] ports = SerialPort.GetPortNames();
            comboBoxPorts.ItemsSource = ports;
            if (ports.Length > 0)
                comboBoxPorts.SelectedIndex = 0;
            else
                alici.Text = "Açýk COM port bulunamadý.";

            // Baud rate seçimleri
            int[] baudRates = { 300, 600, 1200, 2400, 4800, 9600, 14400, 19200, 38400, 57600, 115200 };
            comboBoxBaudRates.ItemsSource = baudRates;
            

            // ViewModel oluþturup chart kontrolüne DataContext atamasý yapýyoruz.
            chartViewModel = new ViewModel();
            chartControl.DataContext = chartViewModel;
        }

        // Port açma butonunun click metodu
        private void buttonOpenPort_Click(object sender, RoutedEventArgs e)
        {
            if (comboBoxBaudRates.SelectedItem == null)
            {
                var notification = new Notification
                {
                    Title = "Baud Rate Hatasý!!",
                    Message = "Baud rate seçilmedi!!",
                    Severity = MUXC.InfoBarSeverity.Informational,
                };
                NotificationQueue.Show(notification);
            }



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
            if (!isPortOpen)
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

        // Port kapama butonunun click metodu
        private void buttonClosePort_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort == null)
            {
                alici.Text = "Port daha önce açýlmamýþ.";
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
                    alici.Text = "Port kapatýlamadý. Uygulamayý yeniden baþlatýnýz.";
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

                // Arduino'dan gelen LDR sensör verisinin, 0-1023 arasý bir sayý olduðunu varsayýyoruz.
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
                System.Diagnostics.Debug.WriteLine("Veri okuma hatasý: " + ex.Message);
            }
        }
    }
}
