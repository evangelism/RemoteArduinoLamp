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
        BluetoothSerial btcomm;
        RemoteDevice arduino;

        DispatcherTimer dt;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Comm_ConnectionEstablished()
        {
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,new Windows.UI.Core.DispatchedHandler(() =>
            {
                arduino.pinMode(2, PinMode.INPUT);
                arduino.pinMode(14, PinMode.ANALOG);
                arduino.pinMode(15, PinMode.ANALOG);
                arduino.pinMode(13, PinMode.OUTPUT);
                arduino.AnalogPinUpdatedEvent += Arduino_AnalogPinUpdatedEvent;
                arduino.DigitalPinUpdatedEvent += Arduino_DigitalPinUpdatedEvent;
                dt.Start();
                on.IsEnabled = true;
                off.IsEnabled = true;
            }));
        }

        private async void go_Click(object sender, RoutedEventArgs e)
        {
            dt = new DispatcherTimer() { Interval = new TimeSpan(500) };
            dt.Tick += Dt_Tick;
            var usb = sender == serial;
            var dev = usb ? await UsbSerial.listAvailableDevicesAsync() : await BluetoothSerial.listAvailableDevicesAsync();
            lst.Items.Clear();
            foreach (var x in dev)
            {
                lst.Items.Add(x.Name);
            }
            if (usb)
            {
                usbcomm = new UsbSerial(dev[0]);
                arduino = new RemoteDevice(usbcomm);
                usbcomm.ConnectionEstablished += Comm_ConnectionEstablished;
                usbcomm.begin(57600, SerialConfig.SERIAL_8N1);
            }
            else
            {
                btcomm = new BluetoothSerial(dev[0]);
                arduino = new RemoteDevice(btcomm);
                btcomm.ConnectionEstablished += Comm_ConnectionEstablished;
                btcomm.begin(38400, SerialConfig.SERIAL_8N1);
            }
        }

        private void Arduino_DigitalPinUpdatedEvent(byte pin, PinState state)
        {
            txt.Text = string.Format("Pin={0}, Value={1}", pin, state);
        }

        private void Arduino_AnalogPinUpdatedEvent(byte pin, ushort value)
        {
            txt.Text = string.Format("Pin={0}, Value={1}", pin, value);
        }

        private void Dt_Tick(object sender, object e)
        {
            arduino.pinMode(14, PinMode.ANALOG);
            arduino.pinMode(2, PinMode.INPUT);
            txt.Text = string.Format("A0={0}, D2={1}, TIME={2}", arduino.analogRead(0), arduino.digitalRead(2),DateTime.Now);
        }

        private void on_Click(object sender, RoutedEventArgs e)
        {
            arduino.digitalWrite(13, PinState.HIGH);
        }

        private void off_Click(object sender, RoutedEventArgs e)
        {
            arduino.digitalWrite(13, PinState.LOW);
        }
    }
}
