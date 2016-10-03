using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace GameTimeClient.Tracking.IO
{
    /// <summary>
    ///     Class handling all SQL related tasks    
    /// </summary>
    class Storage
    {

        private const String DATABASE_FILENAME = "GameTime.sqlite";

        private readonly String DATABASE_CONNECTION_STRING =
            String.Format("Data Source={0};Version=3;Pooling=True;" +
                "Max Pool Size=10;", DATABASE_FILENAME);

        private const String CREATE_PROC_3D_TABLE =
            "CREATE TABLE IF NOT EXISTS proc3d (" +
            "id INTEGER PRIMARY KEY, " +
            "time DEFAULT CURRENT_TIMESTAMP, " +
            "programs TEXT)";

        private const String INSERT_PROC_3D_TEMPLATE =
            "INSERT INTO proc3d (programs) VALUES ('{0}');";

        private const String SELECT_ALL_3D_PROCS =
            "SELECT time, programs FROM proc3D";


        /// <summary>
        ///     Initializes the database and creates the tables if they
        ///     are not present
        /// </summary>
        public Storage()
        {
            if (false == File.Exists(DATABASE_FILENAME))
            {
                SQLiteConnection.CreateFile(DATABASE_FILENAME);
            }

            using (var sqlConn =
                new SQLiteConnection(DATABASE_CONNECTION_STRING))
            {
                SQLiteCommand create_proc3d =
                    new SQLiteCommand(CREATE_PROC_3D_TABLE, sqlConn);

                sqlConn.Open();
                create_proc3d.ExecuteNonQuery();
            };
        }



        /// <summary>
        ///     Stores the result of the ProcChecker in the database and also
        ///     saves a ping that helps to make sure that the PC was not
        ///     sleeping and we were tracking even though there were no 
        ///     matching processes.
        /// </summary>
        /// <param name="procNames"></param>
        public void save3DProcessNames(List<String> procNames)
        {
            using (var sqlConn =
                new SQLiteConnection(DATABASE_CONNECTION_STRING))
            {
                String procString = String.Join(",", procNames);
                String procQuery = 
                    String.Format(INSERT_PROC_3D_TEMPLATE, procString);
                var procCmd = new SQLiteCommand(procQuery, sqlConn);

                sqlConn.Open();
                procCmd.ExecuteNonQuery();
                

            }
        }



        public List<Tuple<DateTime, String>> get3DProcesses()
        {
            var procList = new List<Tuple<DateTime, String>>();
            using (var sqlConn =
                new SQLiteConnection(DATABASE_CONNECTION_STRING))
            {
                var getPingsCommand =
                    new SQLiteCommand(SELECT_ALL_3D_PROCS, sqlConn);

                sqlConn.Open();
                SQLiteDataReader r = getPingsCommand.ExecuteReader();

                while (r.Read())
                {
                    try
                    {
                        procList.Add(new Tuple<DateTime, string>(
                            DateTime.SpecifyKind(
                                DateTime.Parse(Convert.ToString(r["time"])),
                            DateTimeKind.Utc
                            ),
                            Convert.ToString(r["programs"])));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(
                            "Error reading the following DateTime: {0} - {1}",
                            r["time"], e.Message);
                    }
                }

                return procList;
            }
        }
    }

}
