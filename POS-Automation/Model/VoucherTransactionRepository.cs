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
    }
}
