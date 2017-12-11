using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayFast.Integration.Model
{
    public class Settings
    {
        public bool IsLive { get; set; }
        public string MerchantID { get; set; }
        public string MerchantKey { get; set; }
        /// <summary>
        /// This is the URL that actually validates a successful sale and performs various logic
        /// </summary>
        public string URLNotify { get; set; }
        /// <summary>
        /// This is the URL that is called by PF when a transaction is cancelled
        /// </summary>
        public string URLCancel { get; set; }
        /// <summary>
        /// This is the URL that the user will be returned to upn successful sale
        /// </summary>
        public string URLReturn { get; set; }
    }
}
