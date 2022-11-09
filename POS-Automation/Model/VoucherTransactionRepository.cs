using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_Automation.Model
{
    public class VoucherTransactionRepository : DatabaseManager
    {
        public VoucherTransactionRepository() : base()
        {

        }

        //sets AUTOCASHDRAWERUSEDFLG in CASINO_SYSTEM_PARAMETERS to true or false. This settings determines if cash drawer prompt opens
        public void SetAutoCashDrawerFlag(bool isEnabled)
        {

            var query = "update CASINO_SYSTEM_PARAMETERS set VALUE1 = @Value where PAR_NAME = 'AUTOCASHDRAWERUSEDFLG'";

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@Value", System.Data.SqlDbType.Bit).Value = isEnabled;

                    cmd.ExecuteNonQuery();

                }
            }
        }

        public void SetVoucherExpired(string barcode)
        {

            var expireDate = DateTime.Now.AddDays((TestData.VoucherExpirationDays + 1) * -1);

            var query = "update VOUCHER set CREATE_DATE = @Date where BARCODE = @Barcode";

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@Date", System.Data.SqlDbType.DateTime).Value = expireDate;
                    cmd.Parameters.Add("@Barcode", System.Data.SqlDbType.VarChar).Value = barcode;

                    cmd.ExecuteNonQuery();

                }
            }
        }

        public void SetVoucherRedeemedState(string barcode, bool isRedeemed)
        {

            var expireDate = DateTime.Now.AddDays((TestData.VoucherExpirationDays + 1) * -1);

            var query = "update VOUCHER set REDEEMED_STATE = @IsRedeemed where BARCODE = @Barcode";

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@IsRedeemed", System.Data.SqlDbType.Bit).Value = isRedeemed;
                    cmd.Parameters.Add("@Barcode", System.Data.SqlDbType.VarChar).Value = barcode;

                    cmd.ExecuteNonQuery();

                }
            }
        }

        public void SetReceiptReprintEnabled(bool isEnabled)
        {

            var query = "update CASINO_SYSTEM_PARAMETERS set VALUE1 = @Value where PAR_NAME = 'ALLOW_RECEIPT_REPRINT'";

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@Value", System.Data.SqlDbType.Bit).Value = isEnabled;

                    cmd.ExecuteNonQuery();

                }
            }
        }

        public string GetCurrentUserSession(string username)
        {

            var query = "select top(1) SESSION_ID from CASHIER_TRANS  where CREATED_BY = @UserName order by CASHIER_TRANS_ID desc";
            string sessionId = string.Empty;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@UserName", System.Data.SqlDbType.VarChar).Value = username;

                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        sessionId = reader.GetString(0);
                    }

                }
            }

            return sessionId;
        }
    }
}
