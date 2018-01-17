using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using JvsClientTest.Serial;

namespace JvsToVjoy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool KillTheThread;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            KillTheThread = true;
            new Thread(HandleTraffic).Start();
        }

        private void HandleTraffic()
        {
            Thread.Sleep(1000);
            KillTheThread = false;
            var serialHandler = new SerialHandler();
            serialHandler.InitSerial("COM4");
            new Thread(() => serialHandler.RequestJvsInformation()).Start();
            var vJoyFeeder = new vJoyInjector.VJoyFeeder();
            new Thread(() => vJoyFeeder.StartInjector()).Start();
            var idSet = false;
            while (!KillTheThread)
            {
                if (!serialHandler.JvsInformation.SyncOk)
                {
                    Thread.Sleep(10);
                    continue;
                }
                vJoyFeeder.Digitals = serialHandler.JvsInformation.DigitalBytes;
                vJoyFeeder.AnalogChannels = serialHandler.JvsInformation.AnalogChannels;
                if (!idSet && serialHandler.JvsInformation.JvsIdentifier != "")
                {
                    idSet = true;
                    Dispatcher.Invoke(DispatcherPriority.Normal, (Action) (() =>
                    {
                        TxtJvsIo.Text += serialHandler.JvsInformation.JvsIdentifier;
                        TxtJvsCmd.Text += serialHandler.JvsInformation.CmdFormatVersion.ToString("X2");
                        TxtJvsVersion.Text += serialHandler.JvsInformation.JvsVersion.ToString("X2");
                        TxtJvsComm.Text += serialHandler.JvsInformation.CommVersion.ToString("X2");
                    }));
                }
                Thread.Sleep(10);
            }
            vJoyFeeder.KillMe = true;
            serialHandler.CloseSerial();
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            KillTheThread = true;
        }
    }
}
