namespace SerialPort2NetPort
{
	public class Data
	{
		public string ServerIP;
		public Outlet[] Outlets;
	}
	public class Outlet
	{
		public string SerialPortName;
		public int TcpPort;
		public int Baud;
	}
}
