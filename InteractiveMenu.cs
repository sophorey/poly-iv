namespace App
{

    static public class InteractiveMenu
    {
        private static List<Option> options = new List<Option>();
        private static string value;
        enum OutputType
        {
            GeoJSON,
            Coordinates
        }

        public static string InputOutputType()
        {
            options = new List<Option>();
            var values = Enum.GetNames(typeof(OutputType));
            foreach (var item in values)
            {
                options.Add(new Option(item));
            }

            int index = 0;
            WriteMenu(options, options[index]);


            ConsoleKeyInfo keyInfo;
            do
            {
                keyInfo = Console.ReadKey();

                if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    if (index + 1 < options.Count)
                    {
                        index++;
                        WriteMenu(options, options[index]);
                    }
                }
                if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    if (index - 1 >= 0)
                    {
                        index--;
                        WriteMenu(options, options[index]);
                    }
                }
                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    value = options[index].Name;
                    break;
                }
            } while (keyInfo.Key != ConsoleKey.X);

            Console.WriteLine();
            return value;
        }


        private static void WriteMenu(List<Option> options, Option selectedOption)
        {
            Console.Clear();
            foreach (Option option in options)
            {
                if (option == selectedOption)
                {
                    Console.Write($">");
                }
                else
                {
                    Console.Write($" ");
                }
                Console.WriteLine(option.Name);
            }
        }
        private class Option
        {
            public string Name;
            public Option(string name)
            {
                Name = name;
            }
        }
    }
}