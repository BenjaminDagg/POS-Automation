using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Transactions;
using System.Globalization;

namespace POS_Automation.Model
{
    public class DatabaseManager
    {
        private string ConnectionString;

        public DatabaseManager()
        {
            string connString = $"Server = tcp:{TestData.DbServer}; Database = {TestData.DbName}; User Id={TestData.DbUserName}; Password={TestData.DbPassword}; MultipleActiveResultSets=True;TrustServerCertificate=true";
            ConnectionString = connString;
        }

        public void AddTestMachine()
        {
            var dateString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var Date = DateTime.ParseExact(dateString, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);

            var query = "BEGIN " +
                            "if not exists " +
                            "(select * from [LotteryRetail].[dbo].[MACH_SETUP] where MACH_NO = @MachNo) " +
                            "begin " +
                                "insert into [LotteryRetail].[dbo].[MACH_SETUP] " +
                                "([MACH_NO] , " +
                                "[LOCATION_ID] ," +
                                "[MODEL_DESC] ," +
                                "[TYPE_ID] ," +
                                "[GAME_CODE] , " +
                                "[BANK_NO] , " +
                                "[CASINO_MACH_NO] ," +
                                "[IP_ADDRESS] ," +
                                "[PLAY_STATUS] ," +
                                "[PLAY_STATUS_CHANGED] ," +
                                "[ACTIVE_FLAG] ," +
                                "[REMOVED_FLAG] ," +
                                "[BALANCE] ," +
                                "[PROMO_BALANCE] ," +
                                "[LAST_ACTIVITY] ," +
                                "[LAST_CONNECT] ," +
                                "[LAST_DISCONNECT] ," +
                                "[MACH_SERIAL_NO] ," +
                                "[VOUCHER_PRINTING] ," +
                                "[METER_TRANS_NO] ," +
                                "[SD_RS_FLAG] ," +
                                "[GAME_RELEASE] ," +
                                "[GAME_CORE_LIB_VERSION] ," +
                                "[GAME_LIB_VERSION] ," +
                                "[MATH_DLL_VERSION] ," +
                                "[MATH_LIB_VERSION] ," +
                                "[OS_VERSION] ," +
                                "[SYSTEM_VERSION] ," +
                                "[SYSTEM_LIB_A_VERSION] ," +
                                "[SYSTEM_CORE_LIB_VERSION] ," +
                                "[InstallDate] ," +
                                "[RemoveDate] ," +
                                "[MultiGameEnabled] ," +
                                "[TransactionPortalIpAddress] ," +
                                "[TransactionPortalControlPort] ," +
                                "[ModifiedDate]) " +
                                "select " +
                                "@MachNo," +
                                "@LocationId," +
                                "B.BANK_DESCR," +
                                "'S'," +
                                "@GameCode," +
                                "@BankNo," +
                                "@CasinoMachNo," +
                                "@IpAddress," +
                                "1," +
                                "@Date," +
                                "1," +
                                "0," +
                                "5.00," +
                                "0.00," +
                                "@Date," +
                                "NULL," +
                                "NULL," +
                                "@SerialNo," +
                                "0," +
                                "0," +
                                "0," +
                                "'S410MOL-8.00.221'," +
                                "'S410MOL-8.00.221'," +
                                "'S410MOL-8.00.221'," +
                                "'S410MOL-8.00.221'," +
                                "'S410MOL-8.00.221'," +
                                "'3.1.2-PCIE'," +
                                "'S410MOL-8.00.221'," +
                                "'S410MOL-8.00.221'," +
                                "'S410MOL-8.00.221'," +
                                "@Date," +
                                "NULL," +
                                "0," +
                                "NULL," +
                                "NULL," +
                                "@Date " +
                                "from [LotteryRetail].[dbo].[CASINO] LOC " +
                                "inner join [LotteryRetail].[dbo].[BANK] B on B.GAME_TYPE_CODE = @GameTypeCode " +
                                "end " +
                                "end";

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@MachNo", System.Data.SqlDbType.VarChar).Value = TestData.DefaultMachineNumber;
                    cmd.Parameters.Add("@GameCode", System.Data.SqlDbType.VarChar).Value = TestData.TestGameCode;
                    cmd.Parameters.Add("@BankNo", System.Data.SqlDbType.Int).Value = TestData.TestBankNumber;
                    cmd.Parameters.Add("@CasinoMachNo", System.Data.SqlDbType.VarChar).Value = TestData.TestLocationMachineNumber;
                    cmd.Parameters.Add("@IpAddress", System.Data.SqlDbType.VarChar).Value = TestData.TestMachineIpAddress;
                    cmd.Parameters.Add("@Date", System.Data.SqlDbType.DateTime).Value = Date;
                    cmd.Parameters.Add("@SerialNo", System.Data.SqlDbType.VarChar).Value = TestData.TestMachineSerialNumber;
                    cmd.Parameters.Add("@GameTypeCode", System.Data.SqlDbType.VarChar).Value = TestData.TestGameTypeCode;
                    cmd.Parameters.Add("@LocationId", System.Data.SqlDbType.Int).Value = TestData.LocationId;

                    cmd.ExecuteNonQuery();
                }
            }
        }


        //Resets test machine data back to original state after a test
        public void ResetTestMachine()
        {
            AddTestMachine();

            //delete games other than default from MACH_SETUP_GAMES
            var removeGamesQuery = "delete from MACH_SETUP_GAMES where (MACH_NO = @MachNo and GAME_CODE <> @GameCode) or (MACH_NO = @MachNo and BANK_NO <> @BankId)";

            //set MACH_SETUP fields back to default
            var restMachineQuery = "update MACH_SETUP " +
                            "set " +
                            "REMOVED_FLAG = 0, " +
                            "ACTIVE_FLAG = 1, " +
                            "PLAY_STATUS = 1, " +
                            "CASINO_MACH_NO = @LocationMachineNumber ," +
                            "MultiGameEnabled = 0, " +
                            "MACH_SERIAL_NO = @SerialNumber, " +
                            "IP_ADDRESS = @IPAddress, " +
                            "BANK_NO = @BankId, " +
                            "GAME_CODE = @GameCode ," +
                            "Balance = 5.00" +
                        "where MACH_NO = @MachNo";

            //Assign default game to machine if not already assigned
            var assignDefaultGameQuery = "begin " +
                                            "if not exists " +
                                                "(select * from MACH_SETUP_GAMES " +
                                                "where GAME_CODE = @GameCode " +
                                                "and BANK_NO = @BankId " +
                                                "and MACH_NO = @MachNo) " +
                                            "begin " +
                                                "insert into MACH_SETUP_GAMES " +
                                                "([MACH_NO],[LOCATION_ID],[GAME_CODE],[BANK_NO],[TYPE_ID]," +
                                                "[GAME_RELEASE],[MATH_DLL_VERSION],[MATH_LIB_VERSION],[IsEnabled]) " +
                                                "values" +
                                                "(@MachNo,@LocationId,@GameCode,@BankId,'S',NULL,NULL,NULL,1) " +
                                            "end " +
                                         "end";

            var setGamesEnabledQuery = "update MACH_SETUP_GAMES set IsEnabled = 1 where MACH_NO = @MachNo";

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(restMachineQuery, conn))
                {
                    cmd.Parameters.Add("@MachNo", System.Data.SqlDbType.VarChar).Value = TestData.DefaultMachineNumber;
                    cmd.Parameters.Add("@LocationMachineNumber", System.Data.SqlDbType.VarChar).Value = TestData.DefaultLocationMachineNumber;
                    cmd.Parameters.Add("@SerialNumber", System.Data.SqlDbType.VarChar).Value = TestData.DefaultSerialNumber;
                    cmd.Parameters.Add("@IPAddress", System.Data.SqlDbType.VarChar).Value = TestData.DefaultIPAddress;
                    cmd.Parameters.Add("@BankId", System.Data.SqlDbType.VarChar).Value = TestData.TestBankNumber;
                    cmd.Parameters.Add("@GameCode", System.Data.SqlDbType.VarChar).Value = TestData.TestGameCode;
                    cmd.Parameters.Add("@LocationId", System.Data.SqlDbType.VarChar).Value = TestData.LocationId;

                    cmd.ExecuteNonQuery();

                    cmd.CommandText = removeGamesQuery;
                    var result = cmd.ExecuteNonQuery();

                    cmd.CommandText = assignDefaultGameQuery;
                    result = cmd.ExecuteNonQuery();

                    cmd.CommandText = setGamesEnabledQuery;
                    result = cmd.ExecuteNonQuery();
                }
            }

            UpdateMachineLastPlay(TestData.DefaultMachineNumber);
        }

        public void UpdateMachineLastPlay(string machNo)
        {

            string currentDateString = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff");
            DateTime currentDate = DateTime.ParseExact(currentDateString, "yyyy-MM-dd HH-mm-ss.fff", CultureInfo.InvariantCulture);

            var query = "BEGIN " +
                            "IF NOT EXISTS " +
                            "(select * from MACH_LAST_PLAY " +
                            "where MACH_NO = @MachNo) " +
                            "BEGIN " +
                                "insert into MACH_LAST_PLAY " +
                                    "([MACH_NO] " +
                                    ",[DEAL_NO] " +
                                    ",[ROLL_NO] " +
                                    ",[TICKET_NO] " +
                                    ",[BARCODE_SCAN] " +
                                    ",[COINS_BET] " +
                                    ",[LINES_BET] " +
                                    ",[ERROR_NO] " +
                                    ",[SEQUENCE_NO] " +
                                    ",[BALANCE] " +
                                    ",[DTIMESTAMP]) " +
                            "values " +
                                    "(@MachNo " +
                                    ",@DealNo " +
                                    ",1 " +
                                    ",260 " +
                                    ",'08CeUXXDD3F1EKQgN88BKMQTK72eXQ5Z' " +
                                    ",2 " +
                                    ",20 " +
                                    ",119 " +
                                    ",7 " +
                                    ",100 " +
                                    ",@TimeStamp) " +
                            "END " +
                         "END";

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@MachNo", System.Data.SqlDbType.VarChar).Value = machNo;
                    cmd.Parameters.Add("@DealNo", System.Data.SqlDbType.Decimal).Value = TestData.GameplayDealNumber;
                    cmd.Parameters.Add("@TimeStamp", System.Data.SqlDbType.DateTime).Value = currentDate;

                    cmd.ExecuteNonQuery();

                }
            }
        }
    }
}
