using Microsoft.Maker.RemoteWiring;
using Microsoft.Maker.Serial;
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

// Документацию по шаблону элемента "Пустая страница" см. по адресу http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RemoteArduino
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        UsbSerial usbcomm;
        RemoteDevice arduino;

        DispatcherTimer dt;

        byte relay_pin = 7;

        bool auto_mode = false;

        public MainPage()
        {
            this.InitializeComponent();
            connect();
        }

        private async void Comm_ConnectionEstablished()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(() =>
            {
                arduino.pinMode(14, PinMode.ANALOG);
                arduino.pinMode(relay_pin, PinMode.OUTPUT);
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
            foreach (var x in dev)
            {
                System.Diagnostics.Debug.WriteLine("Found " + x.Name);
            }
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
                arduino.digitalWrite(relay_pin, on ? PinState.HIGH : PinState.LOW);
            }
        }

        private void on_Click(object sender, RoutedEventArgs e)
        {
            auto_mode = false;
            arduino.digitalWrite(relay_pin, PinState.HIGH);
        }

        private void off_Click(object sender, RoutedEventArgs e)
        {
            auto_mode = false;
            arduino.digitalWrite(relay_pin, PinState.LOW);
        }

        private void auto_Click(object sender, RoutedEventArgs e)
        {
            auto_mode = true;
        }


    }
}
