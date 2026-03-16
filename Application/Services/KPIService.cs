using System;
using System.Collections.Generic;
using System.Linq;
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
            return _inventory.GetStockLevels().Count;
        }

        // KPI 2 (per SKU)
        public decimal CalculateStockValue(string? itemCode = null)
        {
            var stock = _inventory.GetStockLevels();

            if (!string.IsNullOrEmpty(itemCode))
            {
                return stock.TryGetValue(itemCode, out var qty)
                    ? qty * _inventory.GetUnitCost(itemCode)
                    : 0;
            }

            decimal total = 0;

            foreach (var item in stock)
            {
                if (item.Value <= 0)
                    continue;

                var cost = _inventory.GetUnitCost(item.Key);

                total += item.Value * cost;
            }

            return total;
        }

        // KPI 3
        public int CalculateOutOfStockItems()
        {
            var stock = _inventory.GetStockLevels();

            return stock.Values.Count(v => v <= 0);
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
        public decimal CalculateAverageDailySales(string? itemCode = null)
        {
            IEnumerable<SaleRecord> sales = _inventory.GetSales();

            if (!string.IsNullOrEmpty(itemCode))
                sales = sales.Where(x => x.ItemCode == itemCode);

            var salesList = sales.ToList();

            if (salesList.Count == 0)
                return 0;

            var totalSold = salesList.Sum(x => x.Quantity);

            var start = salesList.Min(x => x.Date).Date;
            var end = salesList.Max(x => x.Date).Date;

            var days = Math.Max((end - start).Days + 1, 1);

            return (decimal)totalSold / days;
        }

        // KPI 5
        public double CalculateAverageInventoryAge(string? itemCode = null)
        {
            var purchases = _inventory.GetPurchases()
                .OrderByDescending(p => p.Date)
                .ToList();

            var stock = _inventory.GetStockLevels();

            if (purchases.Count == 0)
                return 0;

            var purchaseGroups = purchases
                .GroupBy(p => p.ItemCode)
                .ToDictionary(g => g.Key, g => g);

            var now = DateTime.UtcNow;

            double weightedAge = 0;
            double totalQty = 0;

            IEnumerable<string> targetSkus =
                string.IsNullOrEmpty(itemCode)
                ? stock.Keys
                : new[] { itemCode };

            foreach (var sku in targetSkus)
            {
                if (!stock.TryGetValue(sku, out var remainingStock) || remainingStock <= 0)
                    continue;

                if (!purchaseGroups.TryGetValue(sku, out var skuPurchases))
                    continue;

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
    }
}