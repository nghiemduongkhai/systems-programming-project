using System;
using System.Collections.Generic;

namespace InventoryKPI.Core.Models
{
    public class InvoiceResponse
    {
        public List<Invoice> Invoices { get; set; } = new List<Invoice>();
    }

    public class SaleRecord
    {
        public string ItemCode { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public DateTime Date { get; set; }
    }

    public class PurchaseRecord
    {
        public string ItemCode { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public DateTime Date { get; set; }
    }
}