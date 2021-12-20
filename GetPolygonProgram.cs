using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace App
{
    public class GetPolygonProgram
    {

        public GetPolygonProgram()
        {
            StartAction().Wait();
        }


        public static async Task StartAction()
        {
            Resource osm = new OpenStreetMapResource();
            try
            {
                Input.Start();
                dynamic netObject = await osm.Request();
                object result = osm.Simplify(netObject);

                if (Input.OutputFormat == "GeoJSON")
                {
                    result = new GeoJSON()
                    {
                        type = osm.PolygoneType,
                        coordinates = result
                    };
                }

                string jsonstring = JsonConvert.SerializeObject(result);

                await osm.WriteToFile(jsonstring);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine();
            }

        }
    }






    abstract class Resource
    {
        public abstract string Url { get; }
        public static HttpClient client = new HttpClient();
        public abstract string PolygoneType { get; set; }
        static Resource()
        {
            client.DefaultRequestHeaders.Add("User-Agent", "C# App");
        }

        public async Task<dynamic> Request()
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(Url);
                HttpContent responseContent = response.Content;
                if (responseContent.Headers.ContentLength <= 2)
                {
                    throw new Exception("Адрес не найден");
                }

                string responseStringJson = await responseContent.ReadAsStringAsync();
                dynamic netObject = JsonConvert.DeserializeObject(responseStringJson);
                return netObject;
            }
            catch( Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
                
        }

        public abstract JArray Simplify(dynamic netObject);


        public async Task WriteToFile(string jsonstring)
        {


            await File.WriteAllTextAsync(Input.FileName, jsonstring);
            Console.WriteLine();
            Console.WriteLine($"Запись файла выполнена успешно - {Input.GetFullPath(Input.FileName)}\n");
        }
    }



    class OpenStreetMapResource : Resource
    {
        public override string Url => $"https://nominatim.openstreetmap.org/search?q={Input.Adress}&format=json&addressdetails=1&limit=1&polygon_geojson=1";
        public override string PolygoneType { get; set; }

        public override JArray Simplify(dynamic netObject)
        {
            JArray result = new JArray();
            PolygoneType = netObject[0].geojson.type;

            if (netObject[0].geojson.type == "Polygon")
            {
                var afterSimplification = new JArray();
                afterSimplification.Add(netObject[0].geojson.coordinates[0][0]);
                afterSimplification.Add(netObject[0].geojson.coordinates[0][1]);

                for (int i = 2; i < netObject[0].geojson.coordinates[0].Count - 2; i++)
                {
                    if (i % Input.Divider == 0)
                    {
                        afterSimplification.Add(netObject[0].geojson.coordinates[0][i]);
                    }
                }

                afterSimplification.Add(netObject[0].geojson.coordinates[0][netObject[0].geojson.coordinates[0].Count - 2]);
                afterSimplification.Add(netObject[0].geojson.coordinates[0][netObject[0].geojson.coordinates[0].Count - 1]);
                result.Add(afterSimplification);

                Console.WriteLine($"\nКоличество точек до оптимизации {netObject[0].geojson.coordinates[0].Count.ToString()}\nКоличество точек после оптимизации {afterSimplification.Count}");
            }
            else if (netObject[0].geojson.type == "MultiPolygon")
            {
                var afterSimplification = new List<JArray>();

                //[0] - внешний слой, [0] слой внутри которого группы, [0] слой первой группы, [0] первый элемент первой группы <float>
                int totalPointsBefore = 0;
                int totalPointsAfter = 0;
                for (int i = 0; i < netObject[0].geojson.coordinates.Count; i++)
                {
                    afterSimplification.Add(new JArray());

                    // geojson с группой-полигоном меньше 4 элементов невалиден
                    afterSimplification[i].Add(netObject[0].geojson.coordinates[i][0][0]);
                    afterSimplification[i].Add(netObject[0].geojson.coordinates[i][0][1]);
                    totalPointsBefore += netObject[0].geojson.coordinates[i][0].Count;
                    for (int j = 2; j < netObject[0].geojson.coordinates[i][0].Count - 2; j++)
                    {
                        if (j % Input.Divider == 0)
                        {
                            afterSimplification[i].Add(netObject[0].geojson.coordinates[i][0][j]);
                        }
                    }
                    afterSimplification[i].Add(netObject[0].geojson.coordinates[i][0][netObject[0].geojson.coordinates[i][0].Count - 2]);
                    afterSimplification[i].Add(netObject[0].geojson.coordinates[i][0][netObject[0].geojson.coordinates[i][0].Count - 1]);
                    totalPointsAfter += afterSimplification[i].Count;

                }

                result.Add(afterSimplification);

                Console.WriteLine($"\nКоличество точек до оптимизации {totalPointsBefore}\nКоличество точек после оптимизации {totalPointsAfter}");
            }
            else if (netObject[0].geojson.type == "Point")
            {
                for (int i = 0; i < netObject[0].geojson.coordinates.Count; i++)
                {
                    result.Add(netObject[0].geojson.coordinates[i]);
                }

            }

            return result;

        }

    }

    class GoogleResource : Resource
    {
        public override string Url => "https://google.com";

        public override string PolygoneType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override JArray Simplify(dynamic netObject)
        {
            throw new NotImplementedException();
        }
    }

    class YandexResource : Resource
    {
        public override string Url => "https://yandex.ru";

        public override string PolygoneType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override JArray Simplify(dynamic netObject)
        {
            throw new NotImplementedException();
        }
    }




    class Input
    {
        public static string FileName;
        public static string Adress;
        public static int Divider;
        public static string OutputFormat;

        public static void Start()
        {
            SetDefault();
            InputFormat();
            InputAdress();
            InputFilename();
            InputDivider();
        }

        private static void SetDefault()
        {
            FileName = "rndname.json";
            Adress = "москва";
            Divider = 1;
            OutputFormat = "GeoJSON";
        }

        private static void InputFilename()
        {
            Console.WriteLine($"Введите имя файла (по-умолчанию {FileName}): ");
            string str = Console.ReadLine();
            if (str.Length > 0)
            {
                FileName = str;
            }
            Console.WriteLine();
            Console.WriteLine($"Имя файла - {FileName}");
            Console.WriteLine();
        }
        private static void InputAdress()
        {
            Console.WriteLine($"Введите адрес (по-умолчанию {Adress}): ");
            string place = Console.ReadLine();
            if (place.Length > 0)
            {
                Adress = place;
            }
            Console.WriteLine();
            Console.WriteLine($"Текущий адрес - {Adress}");
            Console.WriteLine();
        }

        private static void InputDivider()
        {
            Console.WriteLine($"Введите значение упрощения от 1 до 100 включительно: \n");
            try
            {
                string line = Console.ReadLine();
                int input = int.Parse(line);
                if (Divider > 0 && Divider <= 100)
                {
                    Divider = input;
                    Console.WriteLine();
                    Console.WriteLine($"Значение упрощения - {Divider}\n");
                    Console.WriteLine();
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: введите число от 1 до 100 включительно");
                Console.WriteLine();
                InputDivider();
            }
        }

        private static void InputFormat()
        {
            OutputFormat = InteractiveMenu.InputOutputType();
            Console.WriteLine($"Формат вывода - {OutputFormat}\n");
            Console.WriteLine();
        }

        static public string GetFullPath(string fileName)
        {
            string pathString = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            return pathString;
        }
    }
}

public class GeoJSON
{
    public string type { get; set; }
    public object coordinates { get; set; }

}