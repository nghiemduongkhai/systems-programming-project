using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using InventoryKPI.Common;

namespace InventoryKPI.Infrastructure.FileSystem
{
    public class FileService
    {
        public IEnumerable<string> GetInvoiceFiles(string folderPath)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                    return Enumerable.Empty<string>();

                return Directory.GetFiles(folderPath, "*.txt");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error reading invoice folder: {ex.Message}");
                return Enumerable.Empty<string>();
            }
        }

        public async Task<string> ReadFileAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return string.Empty;

                using FileStream fs = new FileStream(
                    filePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite,
                    4096,
                    true);

                using StreamReader reader = new StreamReader(fs);

                return await reader.ReadToEndAsync();
            }
            catch (Exception ex)
            {
                Logger.Error($"Error reading file {filePath}: {ex.Message}");
                return string.Empty;
            }
        }

        public void MoveToProcessed(string filePath)
        {
            try
            {
                if (!Directory.Exists(Config.ProcessedFolder))
                    Directory.CreateDirectory(Config.ProcessedFolder);

                var fileName = Path.GetFileName(filePath);

                var destination = Path.Combine(Config.ProcessedFolder, fileName);

                if (File.Exists(destination))
                    File.Delete(destination);

                File.Move(filePath, destination);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error moving file {filePath}: {ex.Message}");
            }
        }
    }
}