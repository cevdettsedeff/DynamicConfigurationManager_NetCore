using ConfigurationReader.Library;
using Microsoft.Extensions.Configuration;

namespace test
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // ✅ Tek satır - 3 parametre
            var reader = new ConfigurationReader.Library.ConfigurationReader(
                applicationName: "SERVICE-A",
                connectionString: "Host=localhost;Port=5432;Database=ConfigurationDb;Username=postgres;Password=postgres",
                refreshTimerIntervalInMs: 5000  // 5 saniye
            );

            var maxItems = reader.GetValue<int>("MaxItemCount");
            Console.WriteLine($"MaxItemCount: {maxItems}");
            Console.ReadLine();
        }
    }
}