﻿using System;
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


        private const String CREATE_PING_TABLE =
                    "CREATE TABLE IF NOT EXISTS ping (" +
                    "id INTEGER PRIMARY KEY, " +
                    "time DEFAULT CURRENT_TIMESTAMP" +
                    ")";

        private const String CREATE_PROC_3D_TABLE =
            "CREATE TABLE IF NOT EXISTS proc3d (" +
            "id INTEGER PRIMARY KEY, " +
            "time DEFAULT CURRENT_TIMESTAMP, " +
            "programs TEXT)";


        private const String INSERT_PING_CMD =
            "INSERT INTO ping DEFAULT VALUES";

        private const String INSERT_PROC_3D_TEMPLATE =
            "INSERT INTO proc3d (programs) VALUES ('{0}');";

        private const String SELECT_ALL_PINGS =
            "SELECT time FROM ping";

        private const String DELETE_PINGS_UNTIL =
            "DELETE FROM ping WHERE time <= '{0}'";

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


                SQLiteCommand create_ping =
                    new SQLiteCommand(CREATE_PING_TABLE, sqlConn);

                SQLiteCommand create_proc3d =
                    new SQLiteCommand(CREATE_PROC_3D_TABLE, sqlConn);

                sqlConn.Open();
                create_ping.ExecuteNonQuery();
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
                
                var pingCmd = 
                    new SQLiteCommand(INSERT_PING_CMD, sqlConn);
                var procCmd = new SQLiteCommand(procQuery, sqlConn);

                sqlConn.Open();
                using (var transaction = sqlConn.BeginTransaction())
                {
                    pingCmd.ExecuteNonQuery();
                    procCmd.ExecuteNonQuery();
                    transaction.Commit();
                }

            }
        }

        public List<DateTime> getPings()
        {
            var pingTimes = new List<DateTime>();
            using (var sqlConn = 
                new SQLiteConnection(DATABASE_CONNECTION_STRING))
            {
                var getPingsCommand = 
                    new SQLiteCommand(SELECT_ALL_PINGS, sqlConn);

                sqlConn.Open();
                SQLiteDataReader r = getPingsCommand.ExecuteReader();

                while(r.Read())
                {
                    try
                    {
                        pingTimes.Add(
                            DateTime.SpecifyKind(
                                DateTime.Parse(Convert.ToString(r["time"])),
                                DateTimeKind.Utc
                                ));
                    } catch (Exception e)
                    {
                        Console.WriteLine(
                            "Error reading the following DateTime: {0} - {1}",
                            r["time"], e.Message);
                    }
                }

                return pingTimes;
            }
        }

        /// <summary>
        ///     Delete all the (processed) ping entries up to (and including)
        ///     the given DateTime
        /// </summary>
        /// <param name="to">inclusive end time of delete</param>
        public void deletePingsUpTo(DateTime to)
        {
            SQLiteCommand delCmd = new SQLiteCommand(String.Format(
                DELETE_PINGS_UNTIL,
                to.ToUniversalTime().ToString("yyyy'-'MM'-'dd HH':'mm':'ss")));
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