using System;
using System.IO;

namespace InventoryKPI.Common
{
    public static class Config
    {
        public const string InvoiceFolder = @"../../../Data/invoices";
        public const string ProcessedFolder = @"../../../Data/processed";
        public const string ProductFile = @"../../../Data/product.txt";

        public const string ReportFile = @"../../../InventoryData/report.json";
        public const string InventoryStateFile = @"../../../InventoryData/inventory_data.json";
    }
}