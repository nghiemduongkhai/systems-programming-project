using System;
using System.Collections.Generic;

namespace InventoryKPI.Core.Models
{
    public class Invoice
    {
        public string Type { get; set; }

        public string InvoiceID { get; set; }

        public DateTime DateString { get; set; }

        public List<InvoiceItem> LineItems { get; set; }
    }
}