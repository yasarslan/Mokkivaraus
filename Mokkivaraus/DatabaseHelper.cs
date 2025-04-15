using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mokkivaraus
{
    public class DatabaseHelper
    {
        private readonly string connectionString = "server=127.0.0.1;port=3306;database=vn;user=root;password=12345";
            public async Task<DataTable> GetDataAsync(string query)
            {
                        DataTable dt = new DataTable();
                        try
                        {
                            using (MySqlConnection conn = new MySqlConnection(connectionString))
                            {
                                await conn.OpenAsync();
                                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                                {
                                    using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                                    {
                                        dt.Load(reader);
                                        Console.WriteLine("Connected!");
                        }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Database Error: " + ex.Message);
                        }
                        return dt;

            }
    }
}
