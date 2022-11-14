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
        public int DollarsInCredits { get; set; }
        public int Count1Dollar { get; set; }
        public int Count2Dollar { get; set; }
        public int Count5Dollar { get; set; }
        public int count10Dollar { get; set; }
        public int Count20Dollar { get; set; }
        public int Count50Dollar { get; set; }
        public int Count100Dollar { get; set; }
        public int TabsSold { get; set; }
        public int WinTabs { get; set; }
        public int LoseTabs { get; set; }
        public int PayoutCredits { get; set; }
        public int TicketDroppedValue { get; set; }
        public int TicketCountDropped { get; set; }


        public GameplayParams()
        {
            BalanceCredits = 500;
            TicketNumber = 0;
            SequenceNumber = 0;
            Denomination = 0;
        }
    }
}
