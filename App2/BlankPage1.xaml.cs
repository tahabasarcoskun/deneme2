using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using MUXC = Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls;
using CommunityToolkit.WinUI.Behaviors;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace App2
{


    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BlankPage1 : Page
    {
        public BlankPage1()
        {
            this.InitializeComponent();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var notification = new Notification
            {
                Title = $"Notification {DateTimeOffset.Now}",
                Message = GetRandomText(),
                Severity = MUXC.InfoBarSeverity.Informational,
            };

            NotificationQueue.Show(notification);
        }

        private static int _current = 0;

        private static string GetRandomText() => (_current++ % 4) switch
        {
            1 => "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec sollicitudin bibendum enim at tincidunt. Praesent egestas ipsum ligula, nec tincidunt lacus semper non.",
            2 => "Pellentesque in risus eget leo rhoncus ultricies nec id ante.",
            3 => "Sed quis nisi quis nunc condimentum varius id consectetur metus. Duis mauris sapien, commodo eget erat ac, efficitur iaculis magna. Morbi eu velit nec massa pharetra cursus.",
            _ => "Fusce non quam egestas leo finibus interdum eu ac massa. Quisque nec justo leo. Aenean scelerisque placerat ultrices. Sed accumsan lorem at arcu commodo tristique.",
        };
    }
}