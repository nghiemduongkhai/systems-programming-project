using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryKPI.Core.Models
{
	public class InvoiceItem
	{
		public string ItemCode { get; set; }

		public string Description { get; set; }

		public decimal Quantity { get; set; }

		public decimal UnitAmount { get; set; }

		public decimal LineAmount { get; set; }
	}
}