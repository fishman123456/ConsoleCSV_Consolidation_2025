using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using System.Text;

public class Program
{
    public class Row
    {
        public string Number { get; set; }
        public string EquipmentCode { get; set; }
        public string Name { get; set; }
        public string Quantity { get; set; }
        public string Weight { get; set; }
    }

    public class GroupedRow
    {
        public string Number { get; set; }
        public string EquipmentCode { get; set; }
        public decimal TotalQuantity { get; set; }
        public string Weight { get; set; } // Изменено на string, так как не суммируем
    }

    public static void Main()
    {
        Console.Write("Введите путь к папке с CSV-файлами: ");
        string folderPath = Console.ReadLine();

        var files = Directory.GetFiles(folderPath, "*.csv");

        var allRows = new List<Row>();

        foreach (var file in files)
        {
            using (var reader = new StreamReader(file, Encoding.GetEncoding("Windows-1251")))
            using (var csv = new CsvReader(reader, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = false
            }))
            {
                var records = csv.GetRecords<Row>();
                foreach (var record in records)
                {
                    // Проверка и обработка Quantity
                    if (string.IsNullOrEmpty(record.Quantity))
                    {
                        record.Quantity = "0";
                    }
                    else
                    {
                        decimal quantity;
                        if (!decimal.TryParse(record.Quantity, out quantity))
                        {
                            Console.WriteLine($"Пропускаем строку из файла {file} из-за некорректного значения в Quantity: {record.Quantity}");
                            continue;
                        }
                    }

                    // Проверка и обработка Weight
                    if (string.IsNullOrEmpty(record.Weight))
                    {
                        record.Weight = "0";
                    }
                    else
                    {
                        decimal weight;
                        if (!decimal.TryParse(record.Weight, out weight))
                        {
                            Console.WriteLine($"Пропускаем строку из файла {file} из-за некорректного значения в Weight: {record.Weight}");
                            continue;
                        }
                    }

                    allRows.Add(record);
                }
            }
        }

        // Запись объединенных данных в файл
        string combinedOutputFilePath = folderPath + @"\combined_output.csv";

        using (var writer = new StreamWriter(combinedOutputFilePath, false, Encoding.GetEncoding("Windows-1251")))
        using (var csv = new CsvWriter(writer, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
        {
            Delimiter = ";"
        }))
        {
            csv.WriteRecords(allRows);
        }

        Console.WriteLine($"Объединенные данные сохранены в {combinedOutputFilePath}");

        // Группировка и суммирование
        var groupedRows = allRows.GroupBy(r => r.EquipmentCode)
                               .Select(g => new GroupedRow
                               {
                                   Number = g.First().Number,
                                   EquipmentCode = g.Key,
                                   TotalQuantity = g.Sum(x => decimal.Parse(x.Quantity)),
                                   Weight = g.First().Weight // Берем значение Weight из первой строки
                               });

        // Запись сгруппированных данных в файл с заменой разделителя на запятую
        string groupedOutputFilePath = folderPath + @"\grouped_output.csv";

        using (var writer = new StreamWriter(groupedOutputFilePath, false, Encoding.GetEncoding("Windows-1251")))
        using (var csv = new CsvWriter(writer, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
        {
            Delimiter = ";"
        }))
        {
            foreach (var row in groupedRows)
            {
                csv.WriteField(row.Number);
                csv.WriteField(row.EquipmentCode);
                csv.WriteField(row.TotalQuantity.ToString("0.00", new System.Globalization.CultureInfo("ru-RU")));
                csv.WriteField(row.Weight); // Записываем Weight как есть
                csv.NextRecord();
            }
        }

        Console.WriteLine($"Сгруппированные данные сохранены в {groupedOutputFilePath}");

        // Запись объединенных данных с названиями файлов в один файл
        string combinedWithFileNamesOutputFilePath = folderPath + @"\combined_with_file_names.csv";

        using (var writer = new StreamWriter(combinedWithFileNamesOutputFilePath, false, Encoding.GetEncoding("Windows-1251")))
        using (var csv = new CsvWriter(writer, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
        {
            Delimiter = ";"
        }))
        {
            foreach (var file in files)
            {
                csv.WriteField("Файл: " + Path.GetFileName(file));
                csv.NextRecord();

                using (var reader = new StreamReader(file, Encoding.GetEncoding("Windows-1251")))
                using (var csvReader = new CsvReader(reader, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",
                    HasHeaderRecord = false
                }))
                {
                    var records = csvReader.GetRecords<Row>();
                    foreach (var record in records)
                    {
                        csv.WriteField(record.Number);
                        csv.WriteField(record.EquipmentCode);
                        csv.WriteField(record.Name);
                        csv.WriteField(record.Quantity);
                        csv.WriteField(record.Weight);
                        csv.NextRecord();
                    }
                }
            }
        }

        Console.WriteLine($"Объединенные данные с названиями файлов сохранены в {combinedWithFileNamesOutputFilePath}");
    }
}