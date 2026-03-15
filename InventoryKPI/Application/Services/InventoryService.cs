using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using InventoryKPI.Common;
using InventoryKPI.Core.Interfaces;
using InventoryKPI.Core.Models;

namespace InventoryKPI.Application.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly ConcurrentDictionary<string, decimal> _stock = new();
        private readonly ConcurrentDictionary<string, decimal> _avgCosts = new();

        private readonly List<SaleRecord> _sales = new();
        private readonly List<PurchaseRecord> _purchases = new();

        private readonly object _lock = new();

        public void LoadProducts(IEnumerable<Product> products)
        {
            if (products == null)
                throw new ArgumentNullException(nameof(products));

            foreach (var product in products)
            {
                if (string.IsNullOrWhiteSpace(product.Code))
                    continue;

                _stock.TryAdd(product.Code, 0);
                _avgCosts.TryAdd(product.Code, 0);
            }
        }

        public void AddPurchase(string itemCode, decimal quantity, decimal unitCost, DateTime date)
        {
            if (string.IsNullOrWhiteSpace(itemCode))
                throw new ArgumentException("Invalid item code", nameof(itemCode));

            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));

            if (unitCost < 0)
                throw new ArgumentException("Unit cost cannot be negative", nameof(unitCost));

            var newStock = _stock.AddOrUpdate(
                itemCode,
                quantity,
                (_, currentStock) => currentStock + quantity);

            var oldStock = newStock - quantity;

            var oldCost = _avgCosts.TryGetValue(itemCode, out var existingCost)
                ? existingCost
                : 0;

            var totalStock = oldStock + quantity;

            if (totalStock > 0)
            {
                var newAvgCost =
                    ((oldStock * oldCost) + (quantity * unitCost))
                    / totalStock;

                _avgCosts[itemCode] = newAvgCost;
            }

            lock (_lock)
            {
                _purchases.Add(new PurchaseRecord
                {
                    ItemCode = itemCode,
                    Quantity = quantity,
                    UnitCost = unitCost,
                    Date = date
                });
            }
        }

        public void AddSale(string itemCode, decimal quantity, DateTime date)
        {
            if (string.IsNullOrWhiteSpace(itemCode))
                throw new ArgumentException("Invalid item code", nameof(itemCode));

            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));

            while (true)
            {
                if (!_stock.TryGetValue(itemCode, out var currentStock))
                {
                    Logger.Warning($"Unknown SKU: {itemCode}");
                    return;
                }

                if (currentStock < quantity)
                {
                    Logger.Warning($"Sale exceeds stock for {itemCode}");
                    return;
                }

                var newStock = currentStock - quantity;

                if (_stock.TryUpdate(itemCode, newStock, currentStock))
                    break;
            }

            lock (_lock)
            {
                _sales.Add(new SaleRecord
                {
                    ItemCode = itemCode,
                    Quantity = quantity,
                    Date = date
                });
            }
        }

        public Dictionary<string, decimal> GetStockLevels()
        {
            return new Dictionary<string, decimal>(_stock);
        }

        public decimal GetUnitCost(string itemCode)
        {
            if (string.IsNullOrWhiteSpace(itemCode))
                return 0;

            return _avgCosts.TryGetValue(itemCode, out var cost)
                ? cost
                : 0;
        }

        public List<SaleRecord> GetSales()
        {
            lock (_lock)
            {
                return _sales.ToList();
            }
        }

        public List<PurchaseRecord> GetPurchases()
        {
            lock (_lock)
            {
                return _purchases.ToList();
            }
        }
    }
}