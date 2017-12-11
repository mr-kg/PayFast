using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayFast.Integration.Model
{
    public class Transaction
    {
        public decimal Amount { get; set; }
        public string OrderId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
