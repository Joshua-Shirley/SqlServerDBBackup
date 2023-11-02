using Microsoft.Data.SqlClient;
using System.Data;

namespace Backup
{
    internal class Procedure
    {
        private readonly static string connString = "Server=.;Database=master;Trusted_Connection=True;TrustServerCertificate=True";
        internal static void BackupAll ()
        {
            // To change the databases included in the backup alter the procedure.
            try
            {
                using SqlConnection connection = new(connString);
                using SqlCommand backup = new("BackupAllDatabases", connection);
                connection.Open();
                backup.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        internal static void Backup(string dbName, string path)
        {
            DateTime now = DateTime.Now;
            string timestamp = now.ToString("s").Replace(":", "");
            string queryStatement = $"BACKUP DATABASE [{dbName}] TO DISK = '{path}\\{dbName}-{timestamp}.bak';";

            Console.Write($"\t{queryStatement}\n");
            try
            {
                using SqlConnection connection = new(connString);
                using SqlCommand query = new SqlCommand(queryStatement, connection);
                connection.Open();
                query.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        internal static void BackupDataBases()
        {
            string queryStatement = "SELECT [name]" +
                ", [database_id]" +
                ", [create_date]" +
                ", [physical_database_name] " +
                "FROM sys.databases " +
                "WHERE Cast(CASE WHEN name IN ('master', 'model', 'msdb', 'tempdb') THEN 1 " +
                "ELSE is_distributor END As bit) = 0";
            try
            {
                using SqlConnection connection = new(connString);
                using SqlCommand query = new SqlCommand(queryStatement, connection);
                connection.Open();
                SqlDataReader reader = query.ExecuteReader();
                while (reader.Read()) 
                {
                    Console.WriteLine($"\n{reader[0]}");
                    // Does folder exist
                    // if false create
                    var path = CreateDirectory((string)reader[0]);
                    // run back up
                    Backup(reader[0].ToString(), path);
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
    
        }

        private static string CreateDirectory(string folder)
        {
            string root = $"D:\\\\SQL Backup\\{folder}";
            if( !Directory.Exists(root) )
            {
                Directory.CreateDirectory(root);
            }
            return root;
        }
    }
}

