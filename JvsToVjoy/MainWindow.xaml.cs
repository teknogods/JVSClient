using System;
using System.Collections;
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
            serialHandler.InitSerial("COM3");
            var jvsThread = new Thread(() => serialHandler.RequestJvsInformation());
            jvsThread.Start();
            var keyb = new KeyboardInjector.KeyboardInject();
            var idSet = false;
            var vJoyFeeder = new vJoyInjector.VJoyFeeder();
            while (!KillTheThread)
            {
                if (!serialHandler.JvsInformation.SyncOk)
                {
                    Thread.Sleep(10);
                    continue;
                }

                if (!jvsThread.IsAlive)
                {
                    Thread.Sleep(100);
                    jvsThread = new Thread(() => serialHandler.RequestJvsInformation());
                    jvsThread.Start();
                    Thread.Sleep(100);
                }
                vJoyFeeder.Digitals = serialHandler.JvsInformation.DigitalBytes;
                vJoyFeeder.AnalogChannels = serialHandler.JvsInformation.AnalogChannels;
                if (idSet)
                {
                    keyb.SendInputs(serialHandler.JvsInformation.DigitalBytes);
                }
                if (!idSet && serialHandler.JvsInformation.JvsIdentifier != "")
                {
                    idSet = true;
                    //keyb.Initialize();
                    new Thread(vJoyFeeder.StartInjector).Start();
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            new Thread(() =>
            {
                Thread.Sleep(5000);
                KillTheThread = true;
                new Thread(HandleTraffic).Start();
            }
            ).Start();
        }
    }
}
