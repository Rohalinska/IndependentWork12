using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace IndependentWork12
{
    class Program
    {
        static void Main(string[] args)
        {
            // Розміри колекцій для тесту
            int[] sizes = { 1_000_000, 5_000_000, 10_000_000 };

            // Виконуємо експерименти для кожного розміру
            foreach (int size in sizes)
            {
                Console.WriteLine($"\n=== Тест з {size} елементами ===");

                // 1. Створюємо колекцію випадкових чисел
                List<int> numbers = GenerateRandomList(size);

                // 2. LINQ
                Stopwatch sw = Stopwatch.StartNew();
                var linqResult = numbers
                    .Where(n => IsPrime(n % 1000))  // обчислювально інтенсивна операція
                    .Select(n => Math.Sqrt(n))
                    .ToList();
                sw.Stop();
                Console.WriteLine($"LINQ: {sw.ElapsedMilliseconds} ms");

                // 3. PLINQ
                sw.Restart();
                var plinqResult = numbers
                    .AsParallel()
                    .Where(n => IsPrime(n % 1000))
                    .Select(n => Math.Sqrt(n))
                    .ToList();
                sw.Stop();
                Console.WriteLine($"PLINQ: {sw.ElapsedMilliseconds} ms");
            }

            // 4. Демонстрація проблем побічних ефектів
            Console.WriteLine("\n=== Демонстрація побічних ефектів ===");
            int counter = 0;

            try
            {
                List<int> smallList = GenerateRandomList(1000);

                // PLINQ без потокобезпеки
                smallList.AsParallel().ForAll(n =>
                {
                    counter++; // некоректна операція
                });

                Console.WriteLine($"Некоректний результат без lock: {counter}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            // Виправлено з lock
            counter = 0;
            object lockObj = new object();
            List<int> safeList = GenerateRandomList(1000);

            safeList.AsParallel().ForAll(n =>
            {
                lock (lockObj)
                {
                    counter++; // тепер потокобезпечно
                }
            });
            Console.WriteLine($"Коректний результат з lock: {counter}");
        }

        // Генерація випадкового списку
        static List<int> GenerateRandomList(int size)
        {
            Random rnd = new Random();
            List<int> list = new List<int>(size);
            for (int i = 0; i < size; i++)
            {
                list.Add(rnd.Next(1, 10_000_000));
            }
            return list;
        }

        // Перевірка на просте число (обчислювально інтенсивна)
        static bool IsPrime(int n)
        {
            if (n <= 1) return false;
            for (int i = 2; i <= Math.Sqrt(n); i++)
            {
                if (n % i == 0) return false;
            }
            return true;
        }
    }
}
