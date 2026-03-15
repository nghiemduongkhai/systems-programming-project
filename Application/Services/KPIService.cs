using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InventoryKPI.Core.Interfaces;
using InventoryKPI.Core.Models;

namespace InventoryKPI.Application.Services
{
    public class KPIService : IKPIService
    {
        private readonly IInventoryService _inventory;

        public KPIService(IInventoryService inventory)
        {
            _inventory = inventory;
        }

        // KPI 1
        public int CalculateTotalSKUs()
        {
            var stock = _inventory.GetStockLevels();
            return stock.Count;
        }

        // KPI 2
        public decimal CalculateStockValue()
        {
            var stock = _inventory.GetStockLevels();

            decimal total = 0;

            foreach (var item in stock)
            {
                var cost = _inventory.GetUnitCost(item.Key);
                total += item.Value * cost;
            }

            return total;
        }

        // KPI 2 (per SKU)
        public decimal CalculateStockValue(string itemCode)
        {
            var stock = _inventory.GetStockLevels();

            if (!stock.TryGetValue(itemCode, out var qty))
                return 0;

            var cost = _inventory.GetUnitCost(itemCode);

            return qty * cost;
        }

        // KPI 3 
        public int CalculateOutOfStockItems()
        {
            var stock = _inventory.GetStockLevels();

            int count = 0;

            foreach (var item in stock)
            {
                if (item.Value <= 0)
                    count++;
            }

            return count;
        }

        // KPI 3 (per SKU)
        public bool IsOutOfStock(string itemCode)
        {
            var stock = _inventory.GetStockLevels();

            if (!stock.TryGetValue(itemCode, out var qty))
                return true;

            return qty <= 0;
        }

        // KPI 4 
        public decimal CalculateAverageDailySales()
        {
            var sales = _inventory.GetSales();

            if (!sales.Any())
                return 0;

            var totalSold = sales.Sum(x => x.Quantity);

            var start = sales.Min(x => x.Date).Date;
            var end = sales.Max(x => x.Date).Date;

            var days = Math.Max((end - start).Days + 1, 1);

            return totalSold / days;
        }

        // KPI 4 (per SKU)
        public decimal CalculateAverageDailySales(string itemCode)
        {
            var sales = _inventory.GetSales()
                .Where(x => x.ItemCode == itemCode);

            if (!sales.Any())
                return 0;

            var totalSold = sales.Sum(x => x.Quantity);

            var start = sales.Min(x => x.Date).Date;
            var end = sales.Max(x => x.Date).Date;

            var days = Math.Max((end - start).Days + 1, 1);

            return totalSold / days;
        }

        // KPI 5 
        public double CalculateAverageInventoryAge()
        {
            var purchases = _inventory.GetPurchases();
            var stock = _inventory.GetStockLevels();

            if (!purchases.Any())
                return 0;

            var now = DateTime.UtcNow;

            double weightedAge = 0;
            double totalQty = 0;

            foreach (var sku in stock.Keys)
            {
                var remainingStock = stock[sku];

                if (remainingStock <= 0)
                    continue;

                var skuPurchases = purchases
                    .Where(p => p.ItemCode == sku)
                    .OrderByDescending(p => p.Date);

                foreach (var p in skuPurchases)
                {
                    if (remainingStock <= 0)
                        break;

                    var takeQty = Math.Min((double)p.Quantity, (double)remainingStock);

                    var ageDays = (now - p.Date).TotalDays;

                    weightedAge += ageDays * takeQty;
                    totalQty += takeQty;

                    remainingStock -= (decimal)takeQty;
                }
            }

            if (totalQty == 0)
                return 0;

            return weightedAge / totalQty;
        }

        // KPI 5 (per SKU)
        public double CalculateAverageInventoryAge(string itemCode)
        {
            var purchases = _inventory.GetPurchases()
                .Where(x => x.ItemCode == itemCode)
                .OrderByDescending(x => x.Date)
                .ToList();

            if (!purchases.Any())
                return 0;

            var stock = _inventory.GetStockLevels();

            if (!stock.TryGetValue(itemCode, out var remainingStock))
                return 0;

            if (remainingStock <= 0)
                return 0;

            var now = DateTime.UtcNow;

            double weightedAge = 0;
            double countedQty = 0;

            foreach (var p in purchases)
            {
                if (remainingStock <= 0)
                    break;

                var takeQty = Math.Min((double)p.Quantity, (double)remainingStock);

                var ageDays = (now - p.Date).TotalDays;

                weightedAge += ageDays * takeQty;
                countedQty += takeQty;

                remainingStock -= (decimal)takeQty;
            }

            if (countedQty == 0)
                return 0;

            return weightedAge / countedQty;
        }
    }
}