using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Maker.Serial;
using Microsoft.Maker.RemoteWiring;
using Microsoft.Maker.Firmata;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RemoteArduino
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        UsbSerial usbcomm;
        RemoteDevice arduino;

        DispatcherTimer dt;

        bool auto_mode = false;

        public MainPage()
        {
            this.InitializeComponent();
            connect();
        }

        private void Comm_ConnectionEstablished()
        {
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,new Windows.UI.Core.DispatchedHandler(() =>
            {
                arduino.pinMode(14, PinMode.ANALOG);
                arduino.pinMode(13, PinMode.OUTPUT);
                dt.Start();
                on.IsEnabled = true;
                off.IsEnabled = true;
                auto.IsEnabled = true;
            }));
        }

        private async void connect()
        {
            dt = new DispatcherTimer() { Interval = new TimeSpan(500) };
            dt.Tick += loop;
            var dev = await UsbSerial.listAvailableDevicesAsync();
            usbcomm = new UsbSerial(dev[0]);
            arduino = new RemoteDevice(usbcomm);
            usbcomm.ConnectionEstablished += Comm_ConnectionEstablished;
            usbcomm.begin(57600, SerialConfig.SERIAL_8N1);
        }

        private void loop(object sender, object e)
        {
            if (auto_mode)
            {
                arduino.pinMode(14, PinMode.ANALOG);
                var on = arduino.analogRead(0) > 512;
                arduino.digitalWrite(13, on ? PinState.HIGH : PinState.LOW);
            }
        }

        private void on_Click(object sender, RoutedEventArgs e)
        {
            auto_mode = false;
            arduino.digitalWrite(13, PinState.HIGH);
        }

        private void off_Click(object sender, RoutedEventArgs e)
        {
            auto_mode = false;
            arduino.digitalWrite(13, PinState.LOW);
        }

        private void auto_Click(object sender, RoutedEventArgs e)
        {
            auto_mode = true;
        }
    }
}
