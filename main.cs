using System;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;


namespace Lab_06
{

    class Program
    {
        
        static string API_KEY = "6876035b49da589210a0c76608d5da29";
        public struct Weather
        {
            public string Country { get; set; }
            public string Name { get; set; }
            public double Temp { get; set; }
            public string Description { get; set; }
        }

        public class API_call
        {
            public async Task<Weather> GetWeather(double shirota, double dolgota) //для многопоточности
            {

                var url = $"https://api.openweathermap.org/data/2.5/weather";
                var parameters = $"?lat={shirota}&lon={dolgota}&appid={API_KEY}";

                HttpClient client = new HttpClient(); //класс для API запросов
                client.BaseAddress = new Uri(url);

                HttpResponseMessage response = await client.GetAsync(parameters).ConfigureAwait(false); //асинхорнно ожидаем ответ от API
                Weather res = new Weather();

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    
                    //Console.WriteLine(jsonString);//////

                    Regex rx = new Regex("(?<=\"country\":\")[^\"]+(?=\")");
                    res.Country = rx.Match(jsonString).ToString();
                    rx = new Regex("(?<=\"name\":\")[^\"]+(?=\")");
                    res.Name = rx.Match(jsonString).ToString();
                    rx = new Regex("(?<=\"temp\":)[^\"]+(?=,)");
                    res.Temp = Math.Round(Convert.ToDouble(rx.Match(jsonString).ToString().Replace('.', ',')) - 273);
                    rx = new Regex("(?<=\"description\":\")[^\"]+(?=\")");
                    res.Description = rx.Match(jsonString).ToString();
                    
                    //Console.WriteLine($"\n||{res.Country}, {res.Name}: {res.Temp}, {res.Description}||\n"); ////

                }

                return res;
            }
        }

        static void Main()
        {
            API_call test_call = new API_call();
            

            Weather[] weatherList = new Weather[10];

            for (int i = 0; i < weatherList.Length; i++)
            {
                Weather temp = new Weather();
                Random rng = new Random();
                do
                {
                    temp = test_call.GetWeather(
                        rng.Next(-90, 89) + rng.NextDouble(),
                        rng.Next(-180, 179) + rng.NextDouble()
                    ).GetAwaiter().GetResult();
                } while (temp.Country.Length == 0 || temp.Name.Length == 0);

                Console.WriteLine($"{temp.Country}, {temp.Name}: {temp.Temp} degrees, {temp.Description}");
                weatherList[i] = temp;
            }



            Console.WriteLine("\nCountry with Min temp:");
            Weather outputData = (from data in weatherList
                                  select data
                                  ).OrderBy(data => data.Temp).First();
            Console.WriteLine($"    {outputData.Country}, {outputData.Name}: {outputData.Temp} degrees, {outputData.Description}");

            Console.WriteLine("Country with Max temp:\n");
            outputData = (from data in weatherList
                          select data
                          ).OrderBy(data => data.Temp).Last();
            Console.WriteLine($"    {outputData.Country}, {outputData.Name}: {outputData.Temp} degrees, {outputData.Description}");


            double res = weatherList.Average(data => data.Temp);
            Console.WriteLine($"\nAverage world temperature:\n    {res} degrees");

            int countryCount = weatherList.Select(data => data.Country).Distinct().Count(); //выбрать страны, убрать копии, посчитать количество
            Console.WriteLine($"\nCountry count:\n    {countryCount}");

            try
            {
                var firstRes = (from data in weatherList
                                     where data.Description == "rain" ||
                                             data.Description == "clear sky" ||
                                             data.Description == "few clouds"
                                     select data).Take(1).First(); //берём только 1 и преобразуем из получившегося массива с 1 эл в эл
                Console.WriteLine("Firs suitile data with rain, clear sky ot few clouds:");
                Console.WriteLine($"    {firstRes.Country}, {firstRes.Name}: {firstRes.Temp} degrees, {firstRes.Description}");
            }
            catch { Console.WriteLine("\nERROR: No suitable data found"); }

            Console.ReadLine();
        }
    }
}
