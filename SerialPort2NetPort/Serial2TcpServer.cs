using System;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SerialPort2NetPort
{
	public class Serial2TcpServer : TransBase
	{
		private TcpListener _tcpListener;
		private TcpClient _tcpClient;
		public Serial2TcpServer(string serialPortName, int baudRate,int tcpServerPort) : base(serialPortName,baudRate)
		{
			_tcpListener = new TcpListener(IPAddress.Any, tcpServerPort);
			_tcpListener.Start();
			var localEndPoint = (IPEndPoint)_tcpListener.LocalEndpoint;
			Console.WriteLine($"开启串口转发{serialPortName}=>{localEndPoint.Address}:{localEndPoint.Port}");
			// _ = Task.Run(StartAsync);
		}
		protected override async void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			try
			{
				var bytesToRead = SerialPort.BytesToRead;
				var buffer = new byte[bytesToRead];
				var readAsync = await SerialPort.BaseStream.ReadAsync(buffer, 0, bytesToRead);
				if (_tcpClient?.Connected != true) return;
				await _tcpClient.GetStream().WriteAsync(buffer, 0, bytesToRead);
				LogMessage($"已发送--{BitConverter.ToString(buffer)} 到{_tcpClient.Client.RemoteEndPoint}");
			}
			catch (Exception ex)
			{
				LogMessage(ex.Message);
			}
		}

		public async Task StartAsync()
		{
			while (_tcpListener!=null)
			{
				_tcpClient = await _tcpListener.AcceptTcpClientAsync();
				LogMessage($"客户端--{_tcpClient.Client.RemoteEndPoint}已连接");
				_ = Task.Run(ForwardNetMessages);
			}
		}
		async Task ForwardNetMessages()
		{
			var buffer = new byte[1024];
			while (_tcpListener != null)
			{
				try
				{
					if (_tcpClient?.Connected != true) continue;
					var bytesRead = await _tcpClient.GetStream().ReadAsync(buffer, 0, buffer.Length);
					if (bytesRead <= 0 || !SerialPort.IsOpen) continue;
					var sendData = new byte[bytesRead];
					Array.Copy(buffer, sendData, bytesRead);
					SerialPort.Write(sendData, 0, sendData.Length);
					LogMessage($"已发送--{BitConverter.ToString(sendData)} 到{SerialPort.PortName}");
				}
				catch (Exception ex)
				{
					LogMessage($"{ex.Message}");
					break;
				}
			}
		}
		public void Stop()
		{
			SerialPort.Close();
			SerialPort.Dispose();
			_tcpClient?.Close();
			_tcpListener.Stop();
			LogMessage("转发已停止");
		}
	}
}
