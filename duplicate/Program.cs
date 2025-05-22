using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;


class Program
{
    static void Main(string[] args)
    {
        Console.Write("Klasör yolunu girin: ");
        string folderPath = Console.ReadLine();

        if (!Directory.Exists(folderPath))
        {
            Console.WriteLine("Geçersiz klasör yolu.");
            return;
        }

        var duplicateFiles = FindDuplicateFiles(folderPath);

        ExportToTextFile(duplicateFiles, "duplicates.txt");

        Console.WriteLine("İşlem tamamlandı. duplicates.txt dosyası oluşturuldu.");
    }

    static Dictionary<string, List<string>> FindDuplicateFiles(string folderPath)
    {
        var hashDict = new Dictionary<string, List<string>>();

        // Sadece .3dm ve .stl dosyalarını al
        var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
                             .Where(f => f.EndsWith(".3dm", StringComparison.OrdinalIgnoreCase) ||
                                         f.EndsWith(".stl", StringComparison.OrdinalIgnoreCase))
                             .ToArray();

        int totalFiles = files.Length;
        int processedFiles = 0;

        foreach (var file in files)
        {
            try
            {
                string hash = ComputeFileHash(file);
                if (!hashDict.ContainsKey(hash))
                {
                    hashDict[hash] = new List<string>();
                }
                hashDict[hash].Add(file);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata oluştu: {file} - {ex.Message}");
            }

            processedFiles++;
            int percentComplete = (int)((processedFiles / (double)totalFiles) * 100);
            Console.Write($"\rİşlem durumu: %{percentComplete} ({processedFiles}/{totalFiles})");
        }

        Console.WriteLine(); // Yüzde satırı sonrası bir alt satıra geçmek için

        // Sadece tekrar eden dosyaları döndür
        var duplicates = new Dictionary<string, List<string>>();
        foreach (var pair in hashDict)
        {
            if (pair.Value.Count > 1)
                duplicates[pair.Key] = pair.Value;
        }

        return duplicates;
    }



    static string ComputeFileHash(string filePath)
    {
        using (var sha256 = SHA256.Create())
        using (var stream = File.OpenRead(filePath))
        {
            var hashBytes = sha256.ComputeHash(stream);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }

    static void ExportToTextFile(Dictionary<string, List<string>> duplicates, string textFilePath)
    {
        using (var writer = new StreamWriter(textFilePath))
        {
            foreach (var pair in duplicates)
            {
                writer.WriteLine($"Hash: {pair.Key}");
                foreach (var file in pair.Value)
                {
                    writer.WriteLine($"  - {file}");
                }
                writer.WriteLine();
            }
        }
    }
}
