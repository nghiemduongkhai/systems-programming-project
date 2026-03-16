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
        decimal CalculateStockValue(string? itemCode = null);

        // KPI 3
        int CalculateOutOfStockItems();
        bool IsOutOfStock(string itemCode);

        // KPI 4
        decimal CalculateAverageDailySales(string? itemCode = null);

        // KPI 5
        double CalculateAverageInventoryAge(string? itemCode = null);
    }
}