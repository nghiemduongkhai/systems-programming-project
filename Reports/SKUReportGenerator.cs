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
        private readonly Dictionary<string, Product> _products;

        public SKUReportGenerator(IKPIService kpi, Dictionary<string, Product> products)
        {
            _kpi = kpi;
            _products = products;
        }

        public void Generate(string outputFile)
        {
            var report = new List<object>();

            foreach (var product in _products.Values)
            {
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
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            Directory.CreateDirectory(Path.GetDirectoryName(outputFile)!);
            File.WriteAllText(outputFile, json);
        }
    }
}