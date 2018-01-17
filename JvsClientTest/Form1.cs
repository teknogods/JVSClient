using System;
using System.Collections;
using System.Threading;
using System.Windows.Forms;
using JvsClientTest.Serial;

namespace JvsClientTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var serialHandler = new SerialHandler();
            serialHandler.InitSerial("COM3");
            var t = new Thread(() => serialHandler.RequestJvsInformation());
            t.Start();
            //serialHandler.CloseSerial();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            richTextBox1.Text += "JVS Information" + Environment.NewLine;
            richTextBox1.Text += "I/O ID:" + SerialHandler.JvsInformation.JvsIdentifier + Environment.NewLine;
            richTextBox1.Text += "Digitals:" + Environment.NewLine;

            for (int i = 0; i < SerialHandler.JvsInformation.DigitalBytes.Length; i++)
            {
                richTextBox1.Text += SerialHandler.JvsInformation.DigitalBytes[i] + Environment.NewLine;
                //richTextBox1.Text += SerialHandler.JvsInformation.DigitalBytes[i].ToString("X2") + Environment.NewLine;
            }

            richTextBox1.Text += "Analogs:" + Environment.NewLine;
            for (int i = 0; i < SerialHandler.JvsInformation.AnalogChannels.Length; i++)
            {
                richTextBox1.Text += SerialHandler.JvsInformation.AnalogChannels[i].ToString("X4") + Environment.NewLine;
            }
        }
    }
}
