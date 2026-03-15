using System;
using System.IO;

namespace InventoryKPI.Common
{
    public static class Config
    {
        public const string InvoiceFolder = @"../../../Data/invoices";
        public const string ProcessedFolder = @"../../../Data/processed";

        public const string ProductFile = @"../../../Data/product.txt";
        public const string ReportFile = @"../../../Data/report.json";
    }
}