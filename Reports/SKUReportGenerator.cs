using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.IO;
using InventoryKPI.Core.Interfaces;
using InventoryKPI.Core.Models;

namespace InventoryKPI.Report
{
    public class SKUReportGenerator
    {
        private readonly IKPIService _kpi;
        private readonly IInventoryService _inventory;
        private readonly Dictionary<string, Product> _products;

        public SKUReportGenerator(IKPIService kpi, IInventoryService inventory, Dictionary<string, Product> products)
        {
            _kpi = kpi;
            _inventory = inventory;
            _products = products;
        }

        public void Generate(string outputFile)
        {
            var report = new List<object>();

            var stockLevels = _inventory.GetStockLevels();

            foreach (var sku in stockLevels.Keys)
            {
                if (!_products.TryGetValue(sku, out var product))
                    continue;

                var item = new
                {
                    ItemCode = product.Code,
                    Name = product.Name,

                    StockValue = _kpi.CalculateStockValue(product.Code),
                    OutOfStock = _kpi.IsOutOfStock(product.Code),
                    AverageDailySales = _kpi.CalculateAverageDailySales(product.Code),
                    AverageInventoryAge = _kpi.CalculateAverageInventoryAge(product.Code)
                };

                report.Add(item);
            }

            var json = JsonSerializer.Serialize(
                report,
                new JsonSerializerOptions { WriteIndented = true });

            var dir = Path.GetDirectoryName(outputFile);

            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(outputFile, json);
        }
    }
}