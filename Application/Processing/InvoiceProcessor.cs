using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InventoryKPI.Common;
using InventoryKPI.Core.Interfaces;
using InventoryKPI.Core.Models;

namespace InventoryKPI.Application.Processing
{
    public class InvoiceProcessor
    {
        private readonly IInventoryService _inventory;
        private readonly Dictionary<string, Product> _products;

        public InvoiceProcessor(
            IInventoryService inventory,
            Dictionary<string, Product> products)
        {
            _inventory = inventory;
            _products = products;
        }

        public void Process(List<Invoice> invoices)
        {
            if (invoices == null || invoices.Count == 0)
                return;

            Parallel.ForEach(
                invoices,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                },
                invoice =>
                {
                    try
                    {
                        if (invoice.LineItems == null)
                            return;

                        foreach (var item in invoice.LineItems)
                        {
                            if (!_products.TryGetValue(item.ItemCode, out var product))
                            {
                                Logger.Warning($"Unknown product code: {item.ItemCode}");
                                continue;
                            }

                            if (item.Quantity <= 0)
                            {
                                Logger.Warning($"Invalid quantity for {item.ItemCode}");
                                continue;
                            }

                            if (invoice.Type == InvoiceTypes.Purchase)
                            {
                                _inventory.AddPurchase(
                                    item.ItemCode,
                                    item.Quantity,
                                    item.UnitAmount,
                                    invoice.DateString);
                            }
                            else if (invoice.Type == InvoiceTypes.Sale)
                            {
                                _inventory.AddSale(
                                    item.ItemCode,
                                    item.Quantity,
                                    invoice.DateString);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Invoice processing error: {ex.Message}");
                    }
                });
        }
    }
}