using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_Automation.Model.Payout
{
    public class VoucherListRecord
    {
        public string Barcode { get; set; }
        public decimal Amount { get; set; }
        public int VoucherId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Location { get; set; }
        public bool NeedsApproval { get; set; }
    }
}
