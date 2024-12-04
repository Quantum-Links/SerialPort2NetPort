namespace SerialPort2NetPort
{
	public class TransPortConfig
	{
		public S2T[] S2Ts;
		public T2S[] T2Ss;
	}
	public abstract class S2T
	{
		public string SerialName;
		public int TcpServerPort;
	}
	public abstract class T2S:S2T
	{
		public string TcpServerIp;
	}
}
