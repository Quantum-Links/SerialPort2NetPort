using System;
using System.IO.Ports;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SerialPort2NetPort
{
	public class NetPort2SerialPort : TransBase
	{
		readonly TcpClient _tcpClient;
		private readonly string _ip;
		private readonly int _port;
		public NetPort2SerialPort(string serialPortName,int baudRate, string ip, int port):base(serialPortName,baudRate)
		{
			_tcpClient = new TcpClient();
			_ip = ip;
			_port = port;
		}
		protected override async void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			try
			{
				var bytesToRead = SerialPort.BytesToRead;
				var buffer = new byte[bytesToRead];
				SerialPort.Read(buffer, 0, bytesToRead);
				if (!_tcpClient.Connected) return;
				await _tcpClient.GetStream().WriteAsync(buffer, 0, bytesToRead);
				LogMessage($"已发送到 TCP 客户端: {BitConverter.ToString(buffer)}");
			}
			catch (Exception ex)
			{
				LogMessage($"串口读取错误: {ex.Message}");
			}
		}
		private async Task ForwardNetMessages()
		{
			var buffer = new byte[1024];
			while (_tcpClient.Connected)
			{
				try
				{
					var bytesRead = await _tcpClient.GetStream().ReadAsync(buffer, 0, buffer.Length);
					if (bytesRead <= 0 || !SerialPort.IsOpen) continue;
					SerialPort.Write(buffer, 0, bytesRead);
					LogMessage($"已发送到串口: {BitConverter.ToString(buffer, 0, bytesRead)}");
				}
				catch (Exception ex)
				{
					LogMessage($"TCP 读取错误: {ex.Message}");
					break;
				}
			}
		}

		public async Task ConnectServer()
		{
			int i = 0;
			while (i<3)
			{
				try
				{
					await _tcpClient.ConnectAsync(_ip, _port);
					Console.WriteLine($"开启网口转发{_ip}=>{_port}");
					LogMessage($"已连接到服务器 {_ip}:{_port}");
					_ = Task.Run(ForwardNetMessages);
					break;
				}
				catch (Exception ex)
				{
					i++;
					LogMessage($"连接失败: {ex.Message},正在尝试第{i}次重连");
					if (i >= 3)
						throw;
				}	
			}
		}
	}
}
