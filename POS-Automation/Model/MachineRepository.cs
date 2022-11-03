using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_Automation.Model
{
    public class MachineRepository : DatabaseManager
    {
        public MachineRepository() : base()
        {

        }

        public List<Machine> GetAllMachines()
        {
            var query = "SELECT MACH_NO, CASINO_MACH_NO, MODEL_DESC, IP_ADDRESS, REMOVED_FLAG, BALANCE, LAST_ACTIVITY, ACTIVE_FLAG FROM [dbo].[MACH_SETUP] WHERE REMOVED_FLAG = 0";

            List<Machine> machines = new List<Machine>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var machine = new Machine();

                            machine.Id = reader.GetString(0);
                            machine.MachNo = reader.GetString(1);
                            machine.Description = reader.GetString(2);

                            if (!reader.IsDBNull(3))
                            {
                                machine.IpAddress = reader.GetString(3);
                            }
                            
                            machine.Balance = reader.GetDecimal(5);

                            if (!reader.IsDBNull(6))
                            {
                                machine.LastPlayed = reader.GetDateTime(6);
                            }
                            
                            machine.Status = reader.GetByte(7) == 1 ? true : false;

                            machines.Add(machine);
                        }

                        return machines;
                    }

                }
            }
        }
    }
}
