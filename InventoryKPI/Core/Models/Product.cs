using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryKPI.Core.Models
{
    public class Product
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public bool IsSold { get; set; }

        public bool IsPurchased { get; set; }
    }
}