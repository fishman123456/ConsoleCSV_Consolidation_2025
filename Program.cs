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
        public string Column1 { get; set; }
        public string Column2 { get; set; }
        public string Column3 { get; set; }
        public string Column4 { get; set; }
        public string Column5 { get; set; }
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
                allRows.AddRange(records);
            }
        }

        string outputFilePath = folderPath+@"\output.csv";

        using (var writer = new StreamWriter(outputFilePath, false, Encoding.GetEncoding("Windows-1251")))
        using (var csv = new CsvWriter(writer, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
        {
            Delimiter = ";"
        }))
        {
            csv.WriteRecords(allRows);
        }

        Console.WriteLine($"Файл сохранен в {outputFilePath}");
    }
}