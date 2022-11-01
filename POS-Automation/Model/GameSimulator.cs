using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using EGMSimulator.Core.Services.Implementations;
using EGMSimulator.Core.Models.EgmGame;
using Framework.Core.Logging;
using EGMSimulator.Core.Models.EgmGame;
using Framework.Core.Logging;

namespace POS_Automation.Model
{
    public class GameSimulator
    {
        public GameplayParams gameplayParams;
        public TransactionPortalService transactionPortalService;
        private SqlConnection LotteryRetailConnection;
        private SqlConnection DealDbConnection;
        private BarcodeService BarcodeService;
        private string ConnectionStringLR;
        private string ConnectionStringDeal;

        public GameSimulator(ILogService logService)
        {
            gameplayParams = new GameplayParams();
            transactionPortalService = new TransactionPortalService(logService);
            BarcodeService = new BarcodeService(logService);

            string ConnectionString = $"Server = {TestData.DbServer}; Database = {TestData.DbName}; User Id={TestData.DbUserName}; Password={TestData.DbPassword}; MultipleActiveResultSets=True";
            ConnectionStringLR = ConnectionString;
            LotteryRetailConnection = new SqlConnection(ConnectionString);
            LotteryRetailConnection.Open();

            string DealConnectionString = $"Server = {TestData.DbServer}; Database = eDeal; User Id={TestData.DbUserName}; Password={TestData.DbPassword}; MultipleActiveResultSets=True";
            ConnectionStringDeal = DealConnectionString;
            DealDbConnection = new SqlConnection(ConnectionString);
            DealDbConnection.Open();
        }



        private async Task LoadData()
        {
            var query = "select " +
                        "DS.DEAL_NO, " +
                        "DS.TAB_AMT, " +
                        "DSEQ.NEXT_TICKET, " +
                        "DSEQ.DENOMINATION, " +
                        "DSEQ.COINS_BET, " +
                        "DSEQ.LINES_BET, " +
                        "GT.BARCODE_TYPE_ID, " +
                        "MS.BALANCE, " +
                        "MLP.SEQUENCE_NO " +
                        "from DEAL_SETUP as DS " +
                        "inner join DEAL_STATS DSTATS on DS.DEAL_NO = DSTATS.DEAL_NO " +
                        "inner join DEAL_SEQUENCE DSEQ on DSEQ.DEAL_NO = DS.DEAL_NO " +
                        "inner join GAME_SETUP GS on GS.GAME_CODE = DS.GAME_CODE " +
                        "INNER JOIN GAME_TYPE GT on GT.GAME_TYPE_CODE = GS.GAME_TYPE_CODE " +
                        "inner join MACH_SETUP MS on ms.MACH_NO = @MachNo " +
                        "inner join MACH_LAST_PLAY MLP on MLP.MACH_NO = MS.MACH_NO " +
                        "where DS.DEAL_NO = @DealNo";

            using (SqlConnection conn = new SqlConnection(ConnectionStringLR))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@DealNo", System.Data.SqlDbType.Int).Value = TestData.GameplayDealNumber;
                    cmd.Parameters.Add("@MachNo", System.Data.SqlDbType.VarChar).Value = TestData.DefaultMachineNumber;

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            gameplayParams.DealNumber = reader.GetInt32(0);
                            gameplayParams.TicketNumber = reader.GetInt32(2);
                            gameplayParams.Denomination = reader.GetInt32(3);
                            gameplayParams.CoinsBet = reader.GetInt16(4);
                            gameplayParams.LinesBet = reader.GetByte(5);
                            gameplayParams.BarcodeTypeId = reader.GetInt16(6);

                            decimal balanceDollars = reader.GetDecimal(7);
                            int balanceCredits = (int)(balanceDollars * 100);
                            gameplayParams.BalanceCredits = balanceCredits;

                            gameplayParams.SequenceNumber = reader.GetInt32(8) + 1;
                            transactionPortalService.SequenceNumber = gameplayParams.SequenceNumber;
                        }
                    }
                }
            }
        }


        public async Task StartUp()
        {
            await LoadData();
            transactionPortalService.Connect();
        }


        private string ParseBarcode(string transResponse)
        {
            int commaIndex = transResponse.LastIndexOf(',');

            var barcodeRs = transResponse.Substring(commaIndex + 1).Trim();
            string barcode = barcodeRs;

            return barcode;
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


        public async Task ShutDown()
        {
            transactionPortalService.Disconnect();


            await ResetMachine();
            await ResetDeal();
        }


        private async Task ResetMachine()
        {
            var query = "update MACH_LAST_PLAY set SEQUENCE_NO = @SeqNo where MACH_NO = @MachNo";

            using (SqlConnection conn = new SqlConnection(ConnectionStringLR))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@SeqNo", System.Data.SqlDbType.Int).Value = transactionPortalService.SequenceNumber;
                    cmd.Parameters.Add("@MachNo", System.Data.SqlDbType.VarChar).Value = TestData.DefaultMachineNumber;

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }


        private async Task ResetDeal()
        {
            var updateTicketNumQuery = "update DEAL_SEQUENCE set NEXT_TICKET = @TicketNum where DEAL_NO = @DealNo";

            SqlCommand command = new SqlCommand(updateTicketNumQuery, LotteryRetailConnection);
            command.Parameters.Add("@TicketNum", System.Data.SqlDbType.Int).Value = gameplayParams.TicketNumber;
            command.Parameters.Add("@DealNo", System.Data.SqlDbType.VarChar).Value = TestData.GameplayDealNumber;

            var reader = await command.ExecuteNonQueryAsync();

            var setTicketsActive = $"update  [eDeal].[dbo].[Deal{gameplayParams.DealNumber}] set IsActive = 1";
            SqlCommand dealCommand = new SqlCommand(setTicketsActive, DealDbConnection);

            reader = await dealCommand.ExecuteNonQueryAsync();
        }


        public string Play()
        {

            string tTransResponse = transactionPortalService.TransactionT(gameplayParams.Denomination, gameplayParams.DealNumber, gameplayParams.TicketNumber, gameplayParams.CoinsBet, gameplayParams.LinesBet);

            if (!string.IsNullOrEmpty(tTransResponse))
            {
                string barcodeString = ParseBarcode(tTransResponse);
                var barcode = BarcodeService.DecryptBarcode(gameplayParams.BarcodeTypeId, barcodeString);
                string response;

                if (barcode.DecryptedCreditsWon > 0)
                {
                    var creditsWon = barcode.DecryptedCreditsWon * gameplayParams.CoinsBet;
                    var tierLevel = barcode.DecryptedTier;
                    var newBalance = gameplayParams.BalanceCredits - gameplayParams.BetAmount;
                    newBalance += creditsWon;

                    gameplayParams.BalanceCredits = newBalance;

                    response = transactionPortalService.TransW(newBalance, gameplayParams.TicketNumber, gameplayParams.Denomination, gameplayParams.CoinsBet, gameplayParams.LinesBet, gameplayParams.DealNumber, barcodeString, gameplayParams.BarcodeTypeId, creditsWon);

                    gameplayParams.TicketNumber++;
                }
                else
                {
                    var newBalance = gameplayParams.BalanceCredits - gameplayParams.BetAmount;
                    gameplayParams.BalanceCredits = newBalance;

                    response = transactionPortalService.TransL(newBalance, gameplayParams.Denomination, gameplayParams.CoinsBet, gameplayParams.LinesBet, gameplayParams.DealNumber, gameplayParams.TicketNumber, barcodeString);

                    gameplayParams.TicketNumber++;
                }

                return response;
            }

            return string.Empty;
        }


        public string Win(out int winAmountCredits)
        {
            string tTransResponse = transactionPortalService.TransactionT(gameplayParams.Denomination, gameplayParams.DealNumber, gameplayParams.TicketNumber, gameplayParams.CoinsBet, gameplayParams.LinesBet);
            string barcodeString = ParseBarcode(tTransResponse);
            var barcode = BarcodeService.DecryptBarcode(gameplayParams.BarcodeTypeId, barcodeString);
            string response;

            while (barcode.DecryptedCreditsWon < 1)
            {
                tTransResponse = transactionPortalService.TransactionT(gameplayParams.Denomination, gameplayParams.DealNumber, gameplayParams.TicketNumber, gameplayParams.CoinsBet, gameplayParams.LinesBet);
                barcodeString = ParseBarcode(tTransResponse);
                barcode = BarcodeService.DecryptBarcode(gameplayParams.BarcodeTypeId, barcodeString);

                gameplayParams.TicketNumber++;
            }

            var creditsWon = barcode.DecryptedCreditsWon * gameplayParams.CoinsBet;
            var tierLevel = barcode.DecryptedTier;
            var newBalance = gameplayParams.BalanceCredits - gameplayParams.BetAmount;
            newBalance += creditsWon;

            gameplayParams.BalanceCredits = newBalance;

            response = transactionPortalService.TransW(newBalance, gameplayParams.TicketNumber, gameplayParams.Denomination, gameplayParams.CoinsBet, gameplayParams.LinesBet, gameplayParams.DealNumber, barcodeString, gameplayParams.BarcodeTypeId, creditsWon);

            gameplayParams.TicketNumber++;

            winAmountCredits = creditsWon;

            return response;
        }


        public string Loss()
        {
            string tTransResponse = transactionPortalService.TransactionT(gameplayParams.Denomination, gameplayParams.DealNumber, gameplayParams.TicketNumber, gameplayParams.CoinsBet, gameplayParams.LinesBet);

            string barcodeString = ParseBarcode(tTransResponse);

            var newBalance = gameplayParams.BalanceCredits - gameplayParams.BetAmount;
            gameplayParams.BalanceCredits = newBalance;

            string response = transactionPortalService.TransL(newBalance, gameplayParams.Denomination, gameplayParams.CoinsBet, gameplayParams.LinesBet, gameplayParams.DealNumber, gameplayParams.TicketNumber, barcodeString);

            gameplayParams.TicketNumber++;

            return response;
        }


        public string BillIn(double amount)
        {
            int currentBalance = gameplayParams.BalanceCredits;
            int denom = gameplayParams.Denomination;
            int amountAddedCredits = (int)(amount * 100) * denom;
            int newBalance = currentBalance + amountAddedCredits;

            string response = transactionPortalService.TransM(newBalance, amountAddedCredits, denom);

            gameplayParams.BalanceCredits = newBalance;

            return response;
        }


        public string CashOut()
        {
            string createVoucherResponse = transactionPortalService.VoucherCreate(gameplayParams.BalanceCredits, gameplayParams.BalanceCredits);
            string voucher = ParseVoucherBarcode(createVoucherResponse);

            string printVoucherResponse = transactionPortalService.VoucherPrint(voucher);

            gameplayParams.BalanceCredits = 0;

            return printVoucherResponse;
        }


        public string SetOffline()
        {
            string response = transactionPortalService.TransX(300);
            gameplayParams.SequenceNumber++;

            return response;
        }


        public string SetOnline()
        {
            string response = transactionPortalService.TransX(0);
            gameplayParams.SequenceNumber++;

            return response;
        }

    }
}
