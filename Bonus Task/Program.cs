using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace testWeather2;

public class Coord
{
    public double? lat { get; set; }
    public double? lon { get; set; }
}

public class City
{
    public int? id { get; set; }
    public string? name { get; set; }
    public Coord? coord { get; set; }
    public string? country { get; set; }
    public int? population { get; set; }
    public int? timezone { get; set; }
    public int? sunrise { get; set; }
    public int? sunset { get; set; }
}

public class WeatherInfoMain
{
    public double? temp { get; set; }
    public double? feels_like { get; set; }
    public double? temp_min { get; set; }
    public double? temp_max { get; set; }
    public int? pressure { get; set; }
    public int? sea_level { get; set; }
    public int? grnd_level { get; set; }
    public int? humidity { get; set; }
    public double? temp_kf { get; set; }
}

public class WeatherInfoWeatherItem
{
    public int? id { get; set; }
    public string? main { get; set; }
    public string? description { get; set; }
    public string? icon { get; set; }
}

public class WeatherInfoWind
{
    public double? speed { get; set; }
    public int? deg { get; set; }
    public double? gust { get; set; }
}

public class WeatherInfo
{
    public int? dt { get; set; }
    public WeatherInfoMain? main { get; set; }
    public WeatherInfoWind? wind { get; set; }
    public IList<WeatherInfoWeatherItem>? weather { get; set; }
    public string? dt_txt { get; set; }

}

public class WeatherForecast
{
    public string? cod { get; set; }
    public int? message { get; set; }
    public int? cnt { get; set; }
    public City? city { get; set; }
    public IList<WeatherInfo>? list { get; set; }
}

class Program
{
    private static readonly HttpClient client = new HttpClient();
    private static string url = "https://api.openweathermap.org/data/2.5/forecast?appid=24c2ee8a94ccda72df909dd2b821099a&q=Cherkasy&cnt=5";

    async static Task Main(string[] args)
    {

        SqliteConnection sqlite_conn;
        sqlite_conn = CreateConnection();
        CreateTable(sqlite_conn);

        await getWeather();
        Console.WriteLine("Hello, World!");

        async Task getWeather()
        {
            Console.WriteLine("Getting JSON...");
            var responseString = await client.GetStringAsync(url);
            Console.WriteLine("Parsing JSON...");
            WeatherForecast? weatherForecast =
               JsonSerializer.Deserialize<WeatherForecast>(responseString);
            Console.WriteLine($"cod: {weatherForecast?.cod}");
            Console.WriteLine($"City: {weatherForecast?.city?.name}");
            Console.WriteLine($"list count: {weatherForecast?.list?.Count}");
            foreach (var weatherInfo in weatherForecast?.list)
            {
                Console.WriteLine($"weather temp : {weatherInfo?.main?.temp}");
                Console.WriteLine($"weather humidity : {weatherInfo?.main?.humidity}");

                foreach (var weatherInfoWeather in weatherInfo?.weather)
                {
                    Console.WriteLine($"  weather main : {weatherInfoWeather?.main}");
                    Console.WriteLine($"  weather description : {weatherInfoWeather?.description}");
                }
            }

            InsertData(sqlite_conn, weatherForecast?.list);

        }

        static SqliteConnection CreateConnection()
        {

            SqliteConnection sqlite_conn;
            // Create a new database connection:
            sqlite_conn = new SqliteConnection("Data Source=database_weather.db;");
            // Open the connection:
            try
            {
                sqlite_conn.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return sqlite_conn;
        }

        static void CreateTable(SqliteConnection conn)
        {
            SqliteCommand sqlite_cmd;
            string Createsql = "CREATE TABLE WeatherTable (dt INT, temp VARCHAR(32), humidity VARCHAR(32))";
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = Createsql;
            sqlite_cmd.ExecuteNonQuery();
        }

        static void InsertData(SqliteConnection conn, IList<WeatherInfo> weatherInfoList)
        {
            SqliteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            foreach (var weatherInfo in weatherInfoList)
            {
                sqlite_cmd.CommandText = $"INSERT INTO WeatherTable (dt, temp, humidity) VALUES({weatherInfo.dt}, '{weatherInfo?.main?.temp}', '{weatherInfo?.main?.humidity}'); ";
                sqlite_cmd.ExecuteNonQuery();
            }
            conn.Close();
        }
    }
}