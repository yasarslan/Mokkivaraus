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
        private readonly string connectionString = "server=;port=;database=;user=;password=";
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

            public async Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object> parameters)
            {
                int affectedRows = 0;
                try
                {
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        await conn.OpenAsync();
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            if (parameters != null)
                            {
                                foreach (var param in parameters)
                                {
                                    cmd.Parameters.AddWithValue(param.Key, param.Value);

                                   Console.WriteLine($"Parameter added: {param.Key} = {param.Value}");
                                }
                            }

                        // Log the full query for debugging
                        Console.WriteLine("Executing query: " + query);

                        affectedRows = await cmd.ExecuteNonQueryAsync();

                        // Log number of affected rows
                        Console.WriteLine($"Rows affected: {affectedRows}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Database Error: " + ex.Message);
                }
                return affectedRows;
            }

        public async Task<object?> ExecuteScalarAsync(string query, Dictionary<string, object> parameters)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand(query, connection))
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }

                    var result = await command.ExecuteScalarAsync();
                    return result;
                }
            }
        }
    }
}
