using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using InventoryKPI.Common;
using InventoryKPI.Core.Interfaces;
using InventoryKPI.Core.Models;

namespace InventoryKPI.Application.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly ConcurrentDictionary<string, decimal> _stock = new();
        private readonly ConcurrentDictionary<string, decimal> _avgCosts = new();

        private readonly HashSet<string> _skus = new();

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

            lock (_lock)
            {
                _skus.Add(itemCode);

                var oldStock = _stock.TryGetValue(itemCode, out var s) ? s : 0;
                var oldCost = _avgCosts.TryGetValue(itemCode, out var c) ? c : 0;

                var newStock = oldStock + quantity;

                _stock[itemCode] = newStock;

                var newAvgCost =
                    ((oldStock * oldCost) + (quantity * unitCost))
                    / newStock;

                _avgCosts[itemCode] = newAvgCost;

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

            lock (_lock)
            {
                _skus.Add(itemCode);

                var currentStock = _stock.TryGetValue(itemCode, out var s) ? s : 0;

                var newStock = currentStock - quantity;

                _stock[itemCode] = newStock;

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
            lock (_lock)
            {
                return _skus.ToDictionary(
                    sku => sku,
                    sku => _stock.TryGetValue(sku, out var qty) ? qty : 0
                );
            }
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

        public async Task SaveStateAsync(string path)
        {
            var directory = Path.GetDirectoryName(path);

            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var data = _skus.Select(sku => new
            {
                itemCode = sku,
                stock = _stock.TryGetValue(sku, out var s) ? s : 0,
                avgCost = _avgCosts.TryGetValue(sku, out var c) ? c : 0
            });

            var stateToSave = new
            {
                products = data,
                sales = _sales,
                purchases = _purchases
            };

            var json = JsonSerializer.Serialize(stateToSave, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(path, json);
        }

        public async Task LoadStateAsync(string path)
        {
            if (!File.Exists(path))
            {
                Logger.Info($"Inventory state file not found: {path}. Starting with empty inventory.");
                return;
            }

            var json = await File.ReadAllTextAsync(path);

            if (string.IsNullOrWhiteSpace(json))
            {
                Logger.Warning($"Inventory state file is empty: {path}");
                return;
            }

            var doc = JsonDocument.Parse(json);
            var count = 0;

            lock (_lock)
            {
                _stock.Clear();
                _avgCosts.Clear();
                _sales.Clear();
                _purchases.Clear();
                _skus.Clear();
            }

            if (doc.RootElement.TryGetProperty("products", out var productsElement))
            {
                foreach (var p in productsElement.EnumerateArray())
                {
                    var itemCode = p.GetProperty("itemCode").GetString();
                    var stock = p.GetProperty("stock").GetDecimal();
                    var cost = p.GetProperty("avgCost").GetDecimal();

                    if (string.IsNullOrWhiteSpace(itemCode))
                        continue;

                    _stock[itemCode] = stock;
                    _avgCosts[itemCode] = cost;

                    if (stock != 0 || cost != 0)
                    {
                        _skus.Add(itemCode);
                        count++;
                    }
                }
            }

            if (doc.RootElement.TryGetProperty("sales", out var salesElement))
            {
                var loadedSales = JsonSerializer.Deserialize<List<SaleRecord>>(salesElement.GetRawText());
                if (loadedSales != null)
                {
                    lock (_lock)
                    {
                        _sales.Clear();
                        _sales.AddRange(loadedSales);
                    }
                }
            }

            if (doc.RootElement.TryGetProperty("purchases", out var purchasesElement))
            {
                var loadedPurchases = JsonSerializer.Deserialize<List<PurchaseRecord>>(purchasesElement.GetRawText());
                if (loadedPurchases != null)
                {
                    lock (_lock)
                    {
                        _purchases.Clear();
                        _purchases.AddRange(loadedPurchases);
                    }
                }
            }

            Logger.Info($"Inventory state loaded successfully. {count} products restored.");
        }
    }
}