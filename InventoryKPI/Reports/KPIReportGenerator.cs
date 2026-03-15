using System;
using InventoryKPI.Core.Interfaces;

namespace InventoryKPI.Report
{
    public class KPIReportGenerator
    {
        private readonly IKPIService _kpi;

        public KPIReportGenerator(IKPIService kpi)
        {
            _kpi = kpi;
        }

        public void PrintSystemReport()
        {
            Console.WriteLine();
            Console.WriteLine("========= INVENTORY KPI REPORT =========");

            Console.WriteLine($"1. Total SKUs: {_kpi.CalculateTotalSKUs()}");

            Console.WriteLine($"2. Inventory Value: {_kpi.CalculateStockValue():F2}");

            Console.WriteLine($"3. Out Of Stock Items: {_kpi.CalculateOutOfStockItems()}");

            Console.WriteLine($"4. Average Daily Sales: {_kpi.CalculateAverageDailySales():F2}");

            Console.WriteLine($"5. Average Inventory Age: {_kpi.CalculateAverageInventoryAge():F2} days");

            Console.WriteLine("========================================");

            Console.WriteLine($"Report Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine();
        }
    }
}