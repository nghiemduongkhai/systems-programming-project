using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InventoryKPI.Core.Models;

namespace InventoryKPI.Core.Interfaces
{
    public interface IKPIService
    {
        // KPI 1 
        int CalculateTotalSKUs();

        // KPI 2
        decimal CalculateStockValue();
        decimal CalculateStockValue(string itemCode);

        // KPI 3
        int CalculateOutOfStockItems();
        bool IsOutOfStock(string itemCode);

        // KPI 4
        decimal CalculateAverageDailySales();
        decimal CalculateAverageDailySales(string itemCode);

        // KPI 5
        double CalculateAverageInventoryAge();
        double CalculateAverageInventoryAge(string itemCode);
    }
}