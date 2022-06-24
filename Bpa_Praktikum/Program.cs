namespace Bpa_Praktikum
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var client = new OpcUaClient(Timeout.Infinite, 10, "opc.tcp://192.168.8.1:4840", true);
            client.Run();
        }
    }
}