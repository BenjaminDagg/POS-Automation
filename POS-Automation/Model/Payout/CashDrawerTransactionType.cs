using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_Automation.Model.Payout
{
    public enum CashDrawerTransactionType : int
    {
        StartingBalance = 0,
        CashAdded = 'A',
        CashRemoved = 'R',
        Payout = 'P'
    };
}
