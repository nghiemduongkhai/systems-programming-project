using System;
using System.IO;
using System.Threading.Tasks;
using InventoryKPI.Common;

namespace InventoryKPI.Infrastructure.Monitoring
{
    public class FileWatcherService
    {
        private readonly FileSystemWatcher _watcher;
        private readonly InvoiceQueueService _queue;

        public FileWatcherService(InvoiceQueueService queue)
        {
            _queue = queue;

            if (!Directory.Exists(Config.InvoiceFolder))
                Directory.CreateDirectory(Config.InvoiceFolder);

            _watcher = new FileSystemWatcher(Config.InvoiceFolder, "*.txt")
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
            };

            _watcher.Created += OnCreated;
        }

        public void Start()
        {
            _watcher.EnableRaisingEvents = true;
            Logger.Info($"Watching folder: {Config.InvoiceFolder}");
        }

        private async void OnCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                await Task.Delay(500);

                if (!File.Exists(e.FullPath))
                    return;

                Logger.Info($"Detected new invoice file: {e.FullPath}");

                await _queue.Enqueue(e.FullPath);
            }
            catch (Exception ex)
            {
                Logger.Error($"Watcher error: {ex.Message}");
            }
        }
    }
}