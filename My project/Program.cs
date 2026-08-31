using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace DictionariesApp
{
    // хранения данных словаря
    public class CustomDictionary
    {
        public string Type { get; set; }
        public Dictionary<string, List<string>> Words { get; set; } = new Dictionary<string, List<string>>();
    }

    class Program
    {
        static string dataFolder = "DictionariesData";

        static void Main(string[] args)
        {
            if (!Directory.Exists(dataFolder))
                Directory.CreateDirectory(dataFolder);

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== ДОДАТОК «СЛОВНИКИ» ===");
                Console.WriteLine("1. Створити словник");
                Console.WriteLine("2. Відкрити словник");
                Console.WriteLine("0. Вихід");
                Console.Write("Виберіть дію: ");

                string choice = Console.ReadLine();
                switch (choice)
                {
                    case "1": CreateDictionary(); break;
                    case "2": OpenDictionary(); break;
                    case "0": return;
                    default: Console.WriteLine("Невірний ввід."); break;
                }
            }
        }

        static void CreateDictionary()
        {
            Console.Clear();
            Console.Write("Введіть тип словника (наприклад, Англо-російський): ");
            string type = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(type)) return;

            string filePath = Path.Combine(dataFolder, $"{type}.json");
            if (File.Exists(filePath))
            {
                Console.WriteLine("Такий словник вже існує!");
                Console.ReadLine();
                return;
            }

            var dict = new CustomDictionary { Type = type };
            SaveDictionary(dict, filePath);
            Console.WriteLine("Словник успішно створено!");
            Console.ReadLine();
        }

        static void OpenDictionary()
        {
            Console.Clear();
            var files = Directory.GetFiles(dataFolder, "*.json");
            if (files.Length == 0)
            {
                Console.WriteLine("Немає жодного словника. Створіть новий.");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Доступні словники:");
            for (int i = 0; i < files.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {Path.GetFileNameWithoutExtension(files[i])}");
            }

            Console.Write("Виберіть номер словника: ");
            if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= files.Length)
            {
                string filePath = files[index - 1];
                WorkWithDictionary(filePath);
            }
        }

        static void WorkWithDictionary(string filePath)
        {
            string json = File.ReadAllText(filePath);
            CustomDictionary dict = JsonSerializer.Deserialize<CustomDictionary>(json);

            while (true)
            {
                Console.Clear();
                Console.WriteLine($"=== Словник: {dict.Type} ===");
                Console.WriteLine("1. Додати слово і переклад");
                Console.WriteLine("2. Замінити слово або переклад");
                Console.WriteLine("3. Видалити слово або переклад");
                Console.WriteLine("4. Шукати переклад");
                Console.WriteLine("5. Експортувати слово у файл");
                Console.WriteLine("0. Повернутися назад");
                Console.Write("Виберіть дію: ");

                string choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        Console.Write("Введіть слово: ");
                        string word = Console.ReadLine().ToLower();
                        Console.Write("Введіть переклад: ");
                        string translation = Console.ReadLine().ToLower();

                        if (!dict.Words.ContainsKey(word))
                            dict.Words[word] = new List<string>();

                        if (!dict.Words[word].Contains(translation))
                            dict.Words[word].Add(translation);
                        
                        SaveDictionary(dict, filePath);
                        Console.WriteLine("Додано!");
                        break;

                    case "2": // Замена
                        Console.Write("Введіть слово, яке хочете змінити: ");
                        string oldWord = Console.ReadLine().ToLower();
                        if (dict.Words.ContainsKey(oldWord))
                        {
                            Console.WriteLine("1 - Змінити саме слово, 2 - Змінити один з перекладів");
                            string subChoice = Console.ReadLine();
                            if (subChoice == "1")
                            {
                                Console.Write("Введіть нове слово: ");
                                string newWord = Console.ReadLine().ToLower();
                                dict.Words[newWord] = dict.Words[oldWord];
                                dict.Words.Remove(oldWord);
                                Console.WriteLine("Слово змінено!");
                            }
                            else if (subChoice == "2")
                            {
                                Console.Write("Який переклад замінити? ");
                                string oldTr = Console.ReadLine().ToLower();
                                if (dict.Words[oldWord].Contains(oldTr))
                                {
                                    Console.Write("Новий переклад: ");
                                    string newTr = Console.ReadLine().ToLower();
                                    dict.Words[oldWord].Remove(oldTr);
                                    dict.Words[oldWord].Add(newTr);
                                    Console.WriteLine("Переклад змінено!");
                                }
                            }
                            SaveDictionary(dict, filePath);
                        }
                        else Console.WriteLine("Слово не знайдено.");
                        break;

                    case "3": // Удаление
                        Console.Write("Введіть слово для видалення (його чи його перекладу): ");
                        string wordDel = Console.ReadLine().ToLower();
                        if (dict.Words.ContainsKey(wordDel))
                        {
                            Console.WriteLine("1 - Видалити слово повністю, 2 - Видалити конкретний переклад");
                            string subChoice = Console.ReadLine();
                            if (subChoice == "1")
                            {
                                dict.Words.Remove(wordDel);
                                Console.WriteLine("Слово та всі його переклади видалено!");
                            }
                            else if (subChoice == "2")
                            {
                                Console.Write("Введіть переклад для видалення: ");
                                string trDel = Console.ReadLine().ToLower();
                                if (dict.Words[wordDel].Count > 1)
                                {
                                    dict.Words[wordDel].Remove(trDel);
                                    Console.WriteLine("Переклад видалено!");
                                }
                                else
                                {
                                    Console.WriteLine("Помилка! Це останній варіант перекладу. Видалити неможливо.");
                                }
                            }
                            SaveDictionary(dict, filePath);
                        }
                        else Console.WriteLine("Слово не знайдено.");
                        break;

                    case "4": // Поиск
                        Console.Write("Введіть слово для пошуку: ");
                        string searchWord = Console.ReadLine().ToLower();
                        if (dict.Words.ContainsKey(searchWord))
                        {
                            Console.WriteLine($"Переклади: {string.Join(", ", dict.Words[searchWord])}");
                        }
                        else Console.WriteLine("Слово не знайдено.");
                        break;

                    case "5": // Экспорт
                        Console.Write("Введіть слово для експорту: ");
                        string expWord = Console.ReadLine().ToLower();
                        if (dict.Words.ContainsKey(expWord))
                        {
                            string exportData = $"{expWord} - {string.Join(", ", dict.Words[expWord])}";
                            File.WriteAllText($"{expWord}_export.txt", exportData);
                            Console.WriteLine("Експортовано у файл " + $"{expWord}_export.txt");
                        }
                        break;

                    case "0":
                        return; // Возврат в предыдущее меню
                }
                Console.ReadLine(); // Пауза
            }
        }

        static void SaveDictionary(CustomDictionary dict, string filePath)
        {
            string json = JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }
    }
}