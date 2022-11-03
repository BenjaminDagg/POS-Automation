using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_Automation.Model
{
    public class PromoTicketRepository : DatabaseManager
    {
        public PromoTicketRepository() : base()
        {

        }

        public bool PrintPromoTicketsEnabled()
        {
            var query = "SELECT PRINT_PROMO_TICKETS from CASINO where LOCATION_ID = @LocId";

            bool isEnabled;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@LocId", System.Data.SqlDbType.VarChar).Value = TestData.LocationId;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var value = reader.GetBoolean(0);
                            return value;
                        }

                        return false;
                    }

                }
            }
        }
    }
}
