using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

class KnapsackData
{
    
    private int n;              // Количество предметов
    private int m;              // Вместимость рюкзака
    private List<int> weights;  // Список масс предметов
    private List<int> values;   // Список стоимостей предметов
    public int N
    {
        get { return n; }
        set { n = value; }
    }
    public int M
    {
        get { return m; }
        set { m = value; }
    }
    public List<int> Weights
    {
        get { return weights; }
        set { weights = value; }
    }
    public List<int> Values
    {
        get { return values; }
        set { values = value; }
    }
    public KnapsackData(int n, int m, List<int> weights, List<int> values)
    {
        this.n = n;
        this.m = m;
        this.weights = weights;
        this.values = values;
    }
    public override string ToString()
    {
        return $"Количество предметов: {n}; вместимость рюкзака: {m};\n";
               //$"Массы предметов: {string.Join(", ", weights)};\n" +
               //$"Стоимость предметов: {string.Join(", ", values)};";
    }

    public (int maxCost, List<int> selectedItems) SolveBruteForce()
    /* Метод перебора всех подмножеств */
    {

        int maxCost = 0;                        // начальная максимальная стоимость
        List<int> selectedItems = new List<int>();  //список для хранения номеров предметов, дающих maxCost

        long totalCombinations = 1L << n;
        // 2^n - количество всех возможных подмножеств

        // перебираем все комбинации от 0 до 2^n-1
        for (int i = 0; i < totalCombinations; i++)
        {
            List<int> currentSet = new List<int>(); //список номеров предметов в текущем подмножестве
            int currentWeight = 0;                  //суммарный вес выбранных предметов
            int currentCost = 0;                    //суммарная стоимость выбранных предметов

            for (int j = 0; j < n; j++)
            {
                if ((i & (1 << j)) != 0)            // Проверяем, входит ли j-й предмет в текущее подмножество
                // Если i содержит 1 в позиции j, то это выражение не равно 0, значит, предмет j включён
                {
                    currentWeight += weights[j];
                    currentCost += values[j];
                    currentSet.Add(j);
                }
            }

            if (currentWeight <= m && currentCost > maxCost)
            {
                maxCost = currentCost;
                selectedItems = new List<int>(currentSet);
            }
        }

        return (maxCost, selectedItems);
    }

    public (int maxCost, List<int> selectedItems) SolveGreedy()
        // «Жадный» алгоритм
    {
        // Создаём список предметов с индексами, массой и стоимостью
        List<(int index, int weight, int value, double ratio)> items = new List<(int, int, int, double)>();

        for (int i = 0; i < n; i++)
        {
            double ratio = (double)values[i] / weights[i]; // Удельная ценность
            items.Add((i + 1, weights[i], values[i], ratio));
        }

        // Сортируем по убыванию удельной ценности (ценность/масса)
        items.Sort((a, b) => b.ratio.CompareTo(a.ratio));

        long currentWeight = 0;  // Текущий вес рюкзака
        int maxCost = 0;        // Итоговая стоимость
        List<int> selectedItems = new List<int>();

        foreach (var item in items)
        {
            if (currentWeight + item.weight <= m)  // Если предмет помещается
            {
                currentWeight += item.weight;
                maxCost += item.value;
                selectedItems.Add(item.index-1);  // Добавляем номер предмета в список
            }
        }
        return (maxCost, selectedItems);
    }

    // Метод динамического программирования
    public (int maxCost, List<int> selectedItems) SolveDP()
    {
        int[,] dp = new int[n + 1, m + 1];

        for (int i = 1; i <= n; i++)
        {
            int currentWeight = weights[i - 1];
            int currentValue = values[i - 1];

            for (int w = 0; w <= m; w++)
            {
                if (currentWeight > w)
                    dp[i, w] = dp[i - 1, w];
                else
                    dp[i, w] = Math.Max(dp[i - 1, w], currentValue + dp[i - 1, w - currentWeight]);
            }
        }

        //for (int i = 1; i <= n; i++)
        //{
        //    for (int j = 1; j <= m; j++)
        //    {
        //        Console.Write(dp[i, j]);
        //        Console.Write("  ");
        //    }
        //    Console.WriteLine();
        //}


        List<int> selectedItems = new List<int>();
        int remainingWeight = m;
        for (int i = n; i >= 1; i--)
        {
            if (dp[i, remainingWeight] != dp[i - 1, remainingWeight])
            {
                selectedItems.Add(i - 1);
                remainingWeight -= weights[i - 1];
            }
        }
        selectedItems.Reverse();

        int maxCost = dp[n, m];

        return (maxCost, selectedItems);
    }

    public static KnapsackData ReadDataFromConsole()
        // ввод данных с консоли
    {
        // кол-во предметов и вместимость рюкзака
        Console.WriteLine("Введите кол-во предметов и вместимость рюкзака");
        string[] firstLine = Console.ReadLine().Split();
        int n = int.Parse(firstLine[0]);
        int m = int.Parse(firstLine[1]);

        // массы предметов
        Console.WriteLine("Перечислете массы предметов");
        string[] weightLine = Console.ReadLine().Split();
        List<int> weights = new List<int>();
        foreach (string weight in weightLine)
        {
            weights.Add(int.Parse(weight));
        }

        // стоимости предметов
        Console.WriteLine("Перечислете стоимости предметов");
        string[] valueLine = Console.ReadLine().Split();
        List<int> values = new List<int>();
        foreach (string value in valueLine)
        {
            values.Add(int.Parse(value));
        }
        return new KnapsackData(n, m, weights, values);
    }

    public static KnapsackData ReadDataFromFile(string filePath)
        // ввод данных из файла
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Файл не найден.");
        }
        string[] lines = File.ReadAllLines(filePath);

        // кол-во предметов и вместимость рюкзака
        string[] firstLine = lines[0].Split();
        int n = int.Parse(firstLine[0]);  // кол-во предметов
        int m = int.Parse(firstLine[1]);  // вместимость рюкзака

        // массы предметов
        string[] weightLine = lines[1].Split();
        List<int> weights = new List<int>();
        foreach (string weight in weightLine)
        {
            weights.Add(int.Parse(weight));
        }

        // стоимости предметов
        string[] valueLine = lines[2].Split();
        List<int> values = new List<int>();
        foreach (string value in valueLine)
        {
            values.Add(int.Parse(value));
        }

        // Возвращаем объект KnapsackData
        return new KnapsackData(n, m, weights, values);
    }

    public static KnapsackData GenerateRandom(
       int n,
       int minWeight = 1,
       int maxWeight = 50,
       int minValue = 1,
       int maxValue = 50
   )
    {
        if (n <= 0)
            throw new ArgumentException("Количество предметов должно быть больше 0.");
        if (minWeight > maxWeight || minValue > maxValue)
            throw new ArgumentException("Некорректные диапазоны для веса или стоимости.");

        Random rand = new Random();
        List<int> weights = new List<int>();
        List<int> values = new List<int>();

        for (int i = 0; i < n; i++)
        {
            weights.Add(rand.Next(minWeight, maxWeight + 1));
            values.Add(rand.Next(minValue, maxValue + 1));
        }

        int totalWeight = weights.Sum();
        int m = (int)(totalWeight * (rand.Next(30, 71) / 100.0));

        //m = Math.Max(m, weights.Max());

        return new KnapsackData(n, m, weights, values);
    }
}

class Program
{
    static void Main()
    {

        Console.WriteLine("Выберите способ ввода данных:");
        Console.WriteLine("1 - Ввод с консоли");
        Console.WriteLine("2 - Ввод из файла");
        Console.WriteLine("3 - Сгенерировать случайные данные");

        int choice = int.Parse(Console.ReadLine());

        KnapsackData? knapsackData = null;

        switch (choice)
        {
            case 1: // Ввод данных с консоли

                knapsackData = KnapsackData.ReadDataFromConsole();
                break;

             case 2:    // Ввод данных с файла
                Console.WriteLine("Введите путь к файлу:");
                string filePath = Console.ReadLine();
                try
                {
                    knapsackData = KnapsackData.ReadDataFromFile(filePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при загрузке данных из файла: {ex.Message}");
                }                
                break;
            case 3:
                Console.WriteLine("Введите кол-во вещей");
                int n = Convert.ToInt32(Console.ReadLine());
                knapsackData = KnapsackData.GenerateRandom(n, 5, 40, 1, 30);
                break;
        }

        if (knapsackData != null)
        {
            Console.WriteLine("\nДанные задачи:");
            Console.WriteLine(knapsackData);

            // Метод перебора
            Stopwatch swBrute = Stopwatch.StartNew();
            var (maxCost, selectedItems) = knapsackData.SolveBruteForce();
            swBrute.Stop();
            Console.WriteLine("\nРешение задачи методом перебора:");
            Console.WriteLine($"Время выполнения: {swBrute.Elapsed}");
            Console.WriteLine($"Максимальная стоимость: {maxCost}");
            int totalWeight = selectedItems.Sum(itemIndex => knapsackData.Weights[itemIndex]);
            Console.WriteLine($"Суммарный вес: {totalWeight}");
            //Console.WriteLine($"Выбранные предметы: {string.Join(", ", selectedItems)}");

            // Жадный алгоритм
            Stopwatch swGreedy = Stopwatch.StartNew();
            var (maxCost1, selectedItems1) = knapsackData.SolveGreedy();
            swGreedy.Stop();
            Console.WriteLine("\nРешение задачи «жадным» алгоритмом:");
            Console.WriteLine($"Время выполнения: {swGreedy.Elapsed}");
            Console.WriteLine($"Максимальная стоимость: {maxCost1}");
            int totalWeight1 = selectedItems1.Sum(itemIndex => knapsackData.Weights[itemIndex]);
            Console.WriteLine($"Суммарный вес: {totalWeight1}");
            //Console.WriteLine($"Выбранные предметы: {string.Join(", ", selectedItems1)}");

            // Динамическое программирование
            Stopwatch swDP = Stopwatch.StartNew();
            var (maxCost2, selectedItems2) = knapsackData.SolveDP();
            swDP.Stop();
            Console.WriteLine("\nРешение задачи методом динамического программирования:");
            Console.WriteLine($"Время выполнения: {swDP.Elapsed}");
            Console.WriteLine($"Максимальная стоимость: {maxCost2}");
            int totalWeight2 = selectedItems2.Sum(itemIndex => knapsackData.Weights[itemIndex]);
            Console.WriteLine($"Суммарный вес: {totalWeight2}");
            //Console.WriteLine($"Выбранные предметы: {string.Join(", ", selectedItems2)}");
        }
    }
}
