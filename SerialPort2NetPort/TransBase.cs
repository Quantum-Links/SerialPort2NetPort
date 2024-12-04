using System;
using System.IO.Ports;

namespace SerialPort2NetPort
{
	public abstract class TransBase
	{
		protected readonly SerialPort SerialPort;
		protected static void LogMessage(string message)
		{
			Console.WriteLine($"{DateTime.Now}: {message}{Environment.NewLine}");
		}
		protected TransBase(string serialPortName,int baudRate)
		{
			SerialPort = new SerialPort(serialPortName, baudRate, Parity.None, 8, StopBits.One);
			SerialPort.Open();
			SerialPort.DataReceived += SerialPort_DataReceived;
		}
		protected abstract void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e);
	}
}
