using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

public static class ExtensionMethods
{
    // Deep clone
    public static T DeepClone<T>(this T a)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, a);
            stream.Position = 0;
            return (T) formatter.Deserialize(stream);
        }
    }
    
    private static Random rng = new Random();  

    public static void Shuffle<T>(this IList<T> list)  
    {  
        int n = list.Count;  
        while (n > 1) {  
            n--;  
            int k = rng.Next(n + 1);  
            T value = list[k];  
            list[k] = list[n];  
            list[n] = value;  
        }  
    }
}


namespace Mushrooms
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            // Чтение данных из файла
            var data = File.ReadAllLines("data.txt").Select(raw => String.Join("", raw.Split(',')) ).ToArray();
            double dataSize = data.Length;
            var trainingSize = (int)(2 * dataSize / 3); 
            
            //Коллекция для хранения условных вероятностей
            var counts = new Dictionary<char, Dictionary<char, double>[]>();
            var parForE = new[]
            {
                new []{'b', 'c', 'x', 'f', 'k', 's'}.ToDictionary(x => x, x => 0.0),
                new []{'f', 'g', 'y', 's'}.ToDictionary(x => x, x => 0.0),
                new []{'n', 'b', 'c', 'g', 'r', 'p', 'u', 'e', 'w', 'y'}.ToDictionary(x => x, x => 0.0),
                new []{'t', 'f'}.ToDictionary(x => x, x => 0.0),
                new []{'a', 'l', 'c', 'y', 'f', 'm', 'n', 'p', 's'}.ToDictionary(x => x, x => 0.0),
                new []{'a', 'd', 'f', 'n'}.ToDictionary(x => x, x => 0.0),
                new []{'c', 'w', 'd'}.ToDictionary(x => x, x => 0.0),
                new []{'b', 'n'}.ToDictionary(x => x, x => 0.0),
                new []{'k', 'n', 'b', 'h', 'g', 'r', 'o', 'p', 'u', 'e', 'w', 'y'}.ToDictionary(x => x, x => 0.0),
                new []{'e', 't'}.ToDictionary(x => x, x => 0.0),
                new []{'b', 'c', 'u', 'e', 'z', 'r', '?'}.ToDictionary(x => x, x => 0.0),
                new []{'f', 'y', 'k', 's'}.ToDictionary(x => x, x => 0.0),
                new []{'f', 'y', 'k', 's'}.ToDictionary(x => x, x => 0.0),
                new []{'n', 'b', 'c', 'g', 'o', 'p', 'e', 'w', 'y'}.ToDictionary(x => x, x => 0.0),
                new []{'n', 'b', 'c', 'g', 'o', 'p', 'e', 'w', 'y'}.ToDictionary(x => x, x => 0.0),
                new []{'p', 'u'}.ToDictionary(x => x, x => 0.0),
                new []{'n', 'o', 'w', 'y'}.ToDictionary(x => x, x => 0.0),
                new []{'n', 'o', 't'}.ToDictionary(x => x, x => 0.0),
                new []{'c', 'e', 'f', 'l', 'n', 'p', 's', 'z'}.ToDictionary(x => x, x => 0.0),
                new []{'k', 'n', 'b', 'h', 'r', 'o', 'u', 'w', 'y'}.ToDictionary(x => x, x => 0.0),
                new []{'a', 'c', 'n', 's', 'v', 'y'}.ToDictionary(x => x, x => 0.0),
                new []{'g', 'l', 'm', 'p', 'u', 'w', 'd'}.ToDictionary(x => x, x => 0.0)
            };
            var parForP = parForE.DeepClone();
            counts.Add('e', parForE);
            counts.Add('p', parForP);
            
            // Перемешиваем массив с данными
            data.Shuffle();

            var poisonCount = 0;
            for (var i = 0; i < trainingSize; i++)
            {
                if (data[i][0] == 'p')
                    poisonCount++;
            }
            var edibleCount = trainingSize - poisonCount;
            var classCount = new Dictionary<char, double>
            {
                {'p', poisonCount},
                {'e', edibleCount}
            };

            var poisonProb = (double) poisonCount / trainingSize;
            var edibleProb = (double) edibleCount / trainingSize;
            
            // Обучаем Наивный Баесовский классификатор
            for (var i = 0; i < trainingSize; i++)
            {
                var mushroomClass = data[i][0];
                for (var j = 1; j < 23; j++)
                {
                    var currentParam = data[i][j];
                    counts[mushroomClass][j - 1][currentParam] += 1.0 / classCount[mushroomClass];
                }
            }

            //Работа с тестовой выборкой
            double correct = 0;
            for (var i = trainingSize; i < dataSize; i++)
            {
                var pp = poisonProb;
                var pe = edibleProb;
                for (var j = 1; j < 23; j++)
                {
                    var currentParam = data[i][j];
                    pp *= counts['p'][j - 1][currentParam];
                    pe *= counts['e'][j - 1][currentParam];
                }
                var mushroomClass = pp >= pe ? 'p' : 'e';
                if (data[i][0] == mushroomClass)
                    correct++;
            }

            //Вывод результатов
            var testSize = dataSize - trainingSize;
            var p = 1.0*(dataSize / 3 - correct + 0.5*Math.Pow(1.96, 2)) / (dataSize / 3 + Math.Pow(1.96, 2));
            var w = 1.96 * Math.Sqrt(p * (1 - p) / (dataSize / 3 + Math.Pow(1.96, 2)));
            Console.WriteLine("Верно распознано {0} грибов из {1}", correct, testSize);
            Console.WriteLine("Вероятность ошибки: {0}", 1 - correct / testSize);
            Console.WriteLine("Доверительный интервал вероятности ошибки: [{0}, {1}]", p - w, p + w);
        }
    }
}