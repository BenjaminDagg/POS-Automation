using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EGMSimulator.Core.Services.Implementations;
using EGMSimulator.Core.Models.EgmGame;
using Framework.Core.Logging;
using EGMSimulator.Core.Models.EgmGame;
using Framework.Core.Logging;

namespace POS_Automation.Model
{
    public class TransactionPortalService
    {
        public TransactionPortalClient tpClient;
        private int sequenceNumber;
        private BarcodeService BarcodeService;


        public TransactionPortalService(ILogService logService)
        {
            tpClient = new TransactionPortalClient(TestData.TransactionPortalIpAddress, TestData.TpPort);
            BarcodeService = new BarcodeService(logService);
        }


        public int SequenceNumber
        {
            get { return sequenceNumber; }
            set
            {
                sequenceNumber = value;
            }
        }


        public async Task Connect()
        {

            tpClient.Connect();
        }


        public void Disconnect()
        {
            tpClient.CLose();

        }

        private string ParseVoucherBarcode(string voucherText)
        {
            int startIndex = -1;
            int count = 0;
            for (int i = 0; i < voucherText.Length; i++)
            {
                if (voucherText[i] == '|')
                {
                    count++;
                    if (count == 3)
                    {
                        startIndex = i;
                    }
                }
            }

            if (startIndex != -1)
            {
                string vouhcer = voucherText.Substring(startIndex + 1, 24);
                vouhcer = vouhcer.Replace("-", "");
                return vouhcer;
            }

            return string.Empty;
        }


        public string TransL(int newBalanceCredits, int denom, int coinsBet, int linesBet, int dealNum, int ticketNum, string encryptedTicketBarcode)
        {


            var lTrans = $"{SequenceNumber},L,{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},{newBalanceCredits},0,{0},0,0,{denom},{coinsBet},{linesBet},{dealNum},0,{ticketNum},{encryptedTicketBarcode},0";
            var lTransResponse = tpClient.Execute(lTrans);

            SequenceNumber++;

            return lTransResponse;
        }


        public string TransW(int newBalance, int ticketNum, int denom, int coinsBet, int linesBet, int dealNum, string encryptedTicketBarcode, int barcodeTypeId, int coinsWon)
        {

            var barcode = BarcodeService.DecryptBarcode(barcodeTypeId, encryptedTicketBarcode);
            int tierLevel = barcode.DecryptedTier;

            var wTrans = $"{sequenceNumber},W,{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},{newBalance},0,,{coinsWon},{tierLevel},{denom},{coinsBet},{linesBet},{dealNum},0,{ticketNum},{encryptedTicketBarcode},0,0";
            var wTransResponse = tpClient.Execute(wTrans);

            sequenceNumber++;

            return wTransResponse;
        }


        public string TransM(int newBalance, int amountAddedCredits, int denom)
        {

            var MTrans = $"{SequenceNumber},M,{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},{newBalance},,{amountAddedCredits},{denom}";
            var response = tpClient.Execute(MTrans);

            SequenceNumber++;

            return response;
        }



        public string VoucherCreate(int currentBalance, int transAmount,bool jackpot = false)
        {
            int jackpotFlag = jackpot ? 1 : 0;
            int sessionAmount = 0;

            var voucherCreateTrans = $"{SequenceNumber},VoucherCreate,{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},{currentBalance},,{transAmount},{jackpotFlag},{sessionAmount}";
            var response = tpClient.Execute(voucherCreateTrans);

            SequenceNumber++;

            return response;
        }


        public string VoucherPrint(string voucherBarcode,int newBalance, int transAmount)
        {

            var voucherCreateTrans = $"{SequenceNumber},VoucherPrinted,{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},{newBalance},,{voucherBarcode},{transAmount}";
            var response = tpClient.Execute(voucherCreateTrans);

            sequenceNumber++;

            return response;
        }


        public string TransactionT(int denom, int dealNum, int ticketNum, int coinsBet, int linesBet)
        {

            var ttrans = $"{sequenceNumber},T,{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},{denom},{dealNum},{ticketNum},{coinsBet},{linesBet}";
            var response = tpClient.Execute(ttrans);

            SequenceNumber++;

            return response;
        }


        public string TransX(int statusCode)
        {
            string transX = $"{SequenceNumber},X,{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},{statusCode}";
            var response = tpClient.Execute(transX);

            SequenceNumber++;

            return response;
        }


        public string TransC()
        {
            string cTrans = $"{SequenceNumber},C,{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},";
            var response = tpClient.Execute(cTrans);

            SequenceNumber++;

            return response;
        }


        //Cash Drop
        public string TransD(POS_Automation.Model.GameplayParams gameParam)
        {
            string dTrans = $"{SequenceNumber},D,{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},DGE00177777OpKey,1900-01-01 00:00:00,{gameParam.DollarsInCredits},{gameParam.Count1Dollar},{gameParam.Count2Dollar},{gameParam.Count5Dollar},{gameParam.count10Dollar},{gameParam.Count20Dollar},{gameParam.Count50Dollar},{gameParam.Count100Dollar},{gameParam.TabsSold},{gameParam.WinTabs},{gameParam.LoseTabs},{gameParam.PayoutCredits},{gameParam.TicketDroppedValue},{gameParam.TicketCountDropped},0,0";
            var response = tpClient.Execute(dTrans);

            return response;
        }


        public string GetVoucher(int currentBalanceCredits, int voucherAmountCredits,bool jackpotFlag = false)
        {
            var newBalance = currentBalanceCredits - voucherAmountCredits;
            
            var response = VoucherCreate(currentBalanceCredits, voucherAmountCredits,jackpotFlag);
            var barcode = ParseVoucherBarcode(response);
            VoucherPrint(barcode,newBalance,voucherAmountCredits);

            return barcode;
        }

    }
}
