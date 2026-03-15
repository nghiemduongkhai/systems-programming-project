using System;
using System.Threading.Tasks;
using InventoryKPI.Common;
using InventoryKPI.Infrastructure.Parsing;
using InventoryKPI.Infrastructure.FileSystem;
using InventoryKPI.Infrastructure.Monitoring;
using InventoryKPI.Application.Services;
using InventoryKPI.Application.Processing;
using InventoryKPI.Report;

internal class Program
{
    static async Task Main(string[] args)
    {
        Logger.Info("Inventory KPI System Starting...");

        var parser = new JsonParser();
        var fileService = new FileService();

        var products = parser.ParseProducts(Config.ProductFile);

        var inventory = new InventoryService();
        inventory.LoadProducts(products.Values);

        var processor = new InvoiceProcessor(inventory, products);

        var queue = new InvoiceQueueService(parser, fileService, processor);

        // Load existing invoice files
        foreach (var file in fileService.GetInvoiceFiles(Config.InvoiceFolder))
        {
            await queue.Enqueue(file);
        }

        // Start watcher
        var watcher = new FileWatcherService(queue);
        watcher.Start();

        Logger.Info("System ready. Watching for new invoice files...");

        var kpiService = new KPIService(inventory);
        var consoleReport = new KPIReportGenerator(kpiService);
        var skuReport = new SKUReportGenerator(kpiService, products);

        // Refresh KPI mỗi 2 giây
        while (true)
        {
            Console.Clear();

            consoleReport.PrintSystemReport();

            skuReport.Generate(Config.ReportFile);

            await Task.Delay(2500);
        }
    }
}