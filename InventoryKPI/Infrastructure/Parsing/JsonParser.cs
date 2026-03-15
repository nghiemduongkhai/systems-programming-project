using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using InventoryKPI.Core.Models;
using InventoryKPI.Common;

namespace InventoryKPI.Infrastructure.Parsing
{
    public class JsonParser
    {
        private readonly JsonSerializerOptions _options = new()
        {
            PropertyNameCaseInsensitive = true
        };

        // Load product.txt
        public Dictionary<string, Product> ParseProducts(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return new Dictionary<string, Product>();

                var json = File.ReadAllText(filePath);

                var response = JsonSerializer.Deserialize<ProductResponse>(json, _options);

                var products = response?.Items ?? new List<Product>();

                var productDict = new Dictionary<string, Product>();

                foreach (var product in products)
                {
                    if (string.IsNullOrWhiteSpace(product.Code))
                        continue;

                    productDict[product.Code] = product;
                }

                return productDict;
            }
            catch (Exception ex)
            {
                Logger.Error($"Product parse error: {ex.Message}");
                return new Dictionary<string, Product>();
            }
        }

        // Load invoice files
        public List<Invoice> ParseInvoices(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return new List<Invoice>();

                var json = File.ReadAllText(filePath);

                var response = JsonSerializer.Deserialize<InvoiceResponse>(json, _options);

                var invoices = response?.Invoices ?? new List<Invoice>();

                var result = new List<Invoice>();

                foreach (var invoice in invoices)
                {
                    if (invoice == null)
                        continue;

                    if (invoice.Type != InvoiceTypes.Sale &&
                        invoice.Type != InvoiceTypes.Purchase)
                        continue;

                    if (invoice.LineItems == null)
                        continue;

                    invoice.LineItems = invoice.LineItems
                        .Where(i =>
                            !string.IsNullOrWhiteSpace(i.ItemCode) &&
                            i.Quantity > 0)
                        .ToList();

                    if (invoice.LineItems.Count > 0)
                        result.Add(invoice);
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.Error($"JSON parse error: {ex.Message}");
                return new List<Invoice>();
            }
        }
    }
}