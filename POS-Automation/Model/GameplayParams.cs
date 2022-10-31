using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_Automation.Model
{
    public class GameplayParams
    {
        public int TicketNumber { get; set; }
        public int SequenceNumber { get; set; }
        public int BalanceCredits { get; set; }
        public int Denomination { get; set; }
        public int BetAmount
        {
            get
            {
                return (LinesBet * CoinsBet) * Denomination;
            }
        }
        public int LinesBet { get; set; }
        public int CoinsBet { get; set; }
        public int DealNumber { get; set; }
        public int BarcodeTypeId { get; set; }


        public GameplayParams()
        {
            BalanceCredits = 500;
            TicketNumber = 0;
            SequenceNumber = 0;
            Denomination = 0;
        }
    }
}
