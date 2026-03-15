using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InventoryKPI.Common;
using InventoryKPI.Core.Models;

namespace InventoryKPI.Core.Interfaces
{
    public interface IInventoryService
    {
        void LoadProducts(IEnumerable<Product> products);

        // ACCPAY invoice
        void AddPurchase(string itemCode, decimal quantity, decimal unitCost, DateTime date);

        // ACCREC invoice
        void AddSale(string itemCode, decimal quantity, DateTime date);

        Dictionary<string, decimal> GetStockLevels();

        decimal GetUnitCost(string itemCode);

        List<SaleRecord> GetSales();

        List<PurchaseRecord> GetPurchases();
    }
}
