using System;
using Npgsql;

namespace Version
{
    class Program
    {
        static void Main(string[] args)
        {
            var PosrgresUser = "superuser";
            var PostgresPass = "SuperUser";
            var PosgresDB    = "SuperDB";
            var PostgresPort = "5432";

            Console.WriteLine($"Hello from {Environment.MachineName}");

            var cs = $"Host=MyServer;Username={PosrgresUser}:{PostgresPort};Password={PostgresPass};Database={PosgresDB}";

            Console.WriteLine($"Try to connect to PostgreSQL with connection string:\n{cs}");

            using var con = new NpgsqlConnection(cs);
            try
            {
                con.Open();
                var sql = "SELECT version()";
                using var cmd = new NpgsqlCommand(sql, con);
                var version = cmd.ExecuteScalar().ToString();
                Console.WriteLine($"PostgreSQL version: {version}");
            }
            catch (Npgsql.NpgsqlException e)
            {
                Console.WriteLine($"Error!\n{e.InnerException}");
                
            }
            finally
            {
                con.Close();
            }

            Console.ReadKey();            
        }
    }
}
