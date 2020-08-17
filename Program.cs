using System;
using Npgsql;
using System.Text.RegularExpressions;

namespace KonturTest
{
    class Program
    {
        static void ExecuteQuery(NpgsqlCommand cmd, string output, string query){
            Console.WriteLine(output);
            cmd.CommandText = query;
            cmd.ExecuteNonQuery();
            Console.WriteLine("ok.");
        }

        static void Main(string[] args)
        {
            var PosrgresUser = "superuser";
            var PostgresPass = "SuperUser";
            var PosgresDB = "superdb";
            var PostgresPort = "5430";
            var runVersion = Environment.Version;

            try{
                Console.WriteLine($"\nHello from {Environment.MachineName}\nRuntime version - {runVersion}");
                if (runVersion != new Version(3, 0, 0) && runVersion != new Version(3, 1, 0))
                {
                    throw new System.Exception("Program requires runtime version 3.0.0");
                }

                var cs = $"Host=MyServer;port={PostgresPort};Username={PosrgresUser};Password={PostgresPass};Database={PosgresDB}";
                using var con = new NpgsqlConnection(cs);

                Console.WriteLine($"Try to connect to PostgreSQL with connection string:\n{cs}");
                con.Open();
                using var cmd = new NpgsqlCommand("SELECT version()", con);

                var rawVersion = Regex.Match(cmd.ExecuteScalar().ToString(), @"[0-9]{1,}\.[0-9]{1,}").ToString();
                var pgVersion = new Version(rawVersion);

                if (pgVersion != new Version(11, 7))
                {
                    throw new System.Exception($"Requires PostgreSQL version 11.7 but current {pgVersion}");
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"PostgreSQL version: {cmd.ExecuteScalar().ToString()}");
                
                //add new database ultradb
                ExecuteQuery(cmd,"\nTry to add database ultradb", "CREATE DATABASE ultradb OWNER postgres;");

                //New connection to ultradb
                Console.WriteLine("\nTry to connect to ultradb");
                cs = $"Host=MyServer;port={PostgresPort};Username={PosrgresUser};Password={PostgresPass};Database=ultradb";
                using var newcon = new NpgsqlConnection(cs);
                newcon.Open();
                using var newcmd = new NpgsqlCommand("", newcon);

                //add table
                ExecuteQuery(newcmd, "\nTry to add table test_table in ultradb", @"DROP TABLE IF EXISTS test_table;
                    CREATE TABLE test_table(id SERIAL PRIMARY KEY, 
                    name VARCHAR(255), price INT);");

                //drop table
                ExecuteQuery(newcmd, "\nTry to drop table test_table in ultradb", "DROP TABLE IF EXISTS test_table;");
                newcon.Close();

                //drop database ultradb
                ExecuteQuery(cmd, "\nTry to drop database ultradb", @"SELECT pg_terminate_backend(pid) 
                            FROM pg_stat_activity 
                            WHERE pid <> pg_backend_pid()
                            AND datname = 'ultradb';
                            DROP DATABASE ultradb");

                con.Close();
                Console.WriteLine("\nDone.");

            }catch (Exception e){
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nError:\n{e.GetBaseException()}");
            }
            finally{
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}
