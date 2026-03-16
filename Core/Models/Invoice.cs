using System;
using System.Collections.Generic;

namespace InventoryKPI.Core.Models
{
    public class Invoice
    {
        public string Type { get; set; } = string.Empty;

        public string InvoiceID { get; set; } = string.Empty;

        public DateTime DateString { get; set; }

        public List<InvoiceItem> LineItems { get; set; } = new List<InvoiceItem>();
    }
}