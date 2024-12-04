using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using SerialPort2NetPort;


class Program
{
    static void Main()
    {
        Console.WriteLine("读取配置");
        var currentDirectory = Directory.GetCurrentDirectory();
        var fileName = "config.json";
        var filePath = Path.Combine(currentDirectory, fileName);
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"{filePath}配置不存在，按任意键退出");
            Console.ReadKey();
            Environment.Exit(0);
        }
        else
        {
            StartAsync(filePath);
        }

        Console.ReadKey();
    }

    static async void StartAsync(string filePath)
    {
        try
        {
            var json = File.ReadAllText(filePath);
            var data = DeserializeFromJson<Data>(json);
            if (!string.IsNullOrEmpty(data.ServerIP))
            {
                foreach (var t in data.Outlets)
                {
                    var netPort2SerialPort = new NetPort2SerialPort(t.SerialPortName, t.Baud, data.ServerIP, t.TcpPort);
                    await Task.Run(netPort2SerialPort.ConnectServer);
                }
            }
            else
            {
                foreach (var t in data.Outlets)
                {
                    var serial2TcpServer = new Serial2TcpServer(t.SerialPortName, t.Baud, t.TcpPort);
                    _ = Task.Run(serial2TcpServer.StartAsync);
                }
            }

            while (true)
            {
                Console.WriteLine("输入exit退出");
                var exit = Console.ReadLine();
                if (exit == "exit")
                    Environment.Exit(0);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Console.WriteLine("按任意键退出");
            Console.ReadKey();
            Environment.Exit(0);
        }
    }

    public static string SerializeToJson<T>(T obj)
    {
        try
        {
            using (var memoryStream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                serializer.WriteObject(memoryStream, obj);
                memoryStream.Position = 0;

                using (var reader = new StreamReader(memoryStream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Serialization error: {ex.Message}");
            return null;
        }
    }

    private static T DeserializeFromJson<T>(string json)
    {
        try
        {
            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                return (T)serializer.ReadObject(memoryStream);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Deserialization error: {ex.Message}");
            return default(T);
        }
    }
}