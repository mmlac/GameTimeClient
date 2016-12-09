using GameTime.IO;
using GameTimeClient.Tracking.IO;
using GameTimeClient.Tracking.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace GameTimeClient.Tracking
{
    /// <summary>
    ///     ProcessLogger is responsible for tracking 3D applitacions and
    ///     saving the observations to the database.
    /// </summary>
    class ProcessLogger
    {
        // Intervals for the logging and aggregating / transfering of
        // running 3D processes
        public const int INITIAL_SLEEP_TIME = 15000;
        public const int RESCAN_INTERVAL = 60000;
        public const int AGGREGATE_INTERVAL = 90000;

        // Variables to stop the process logging and aggregating/transfering
        // threads
        private volatile bool _shouldStopLogging = false;
        private volatile bool _shouldStopAggregating = false;


        Storage storage;
        GameTimeConnection gtconn;

        /// <summary>
        ///     Disabled empty constructor. We need to get a Storage supplied
        /// </summary>
        private ProcessLogger() { }

        /// <summary>
        ///     Initialize the ProcessLogger with a Data intstance that will
        ///     be used to save the output of the ProcessChecker
        /// </summary>
        /// <param name="_storage">Data Store</param>
        public ProcessLogger(Storage _storage, GameTimeConnection _gtconn)
        {
            storage = _storage;
            gtconn  = _gtconn;
        }

        

        /// <summary>
        ///     Ask the ProcessLogger loop to stop
        /// </summary>
        public void requestStopLogging()
        {
            _shouldStopLogging = true;
        }


        /// <summary>
        ///     Ask the Aggregation loop to stop
        /// </summary>
        public void requestStopAggregating()
        {
            _shouldStopAggregating = true;
        }


        /// <summary>
        ///     Callback called by a Timer to get 3D processes and write
        ///     them to the database.
        /// </summary>
        /// <param name="state">Timer state</param>
        public void log3DProcesses()
        {
            // initial break to let other programs start up for a good
            // initial reading of processes using D3D or OpenGL that we don't
            // want to track
            Thread.Sleep(INITIAL_SLEEP_TIME);

            List<String> ignoreProcs =
                ProcessChecker.Get3DProcessNames(new List<String>());

#if DEBUG
            Console.WriteLine("Ignoring the following processes as they were" +
                " seen during startup: {0}", String.Join(",", ignoreProcs));
#endif

            Thread.Sleep(RESCAN_INTERVAL);


            while (false == _shouldStopLogging)
            {
                try
                {
                    List<String> procNames =
                        ProcessChecker.Get3DProcessNames(ignoreProcs);
#if DEBUG
                    Console.WriteLine(String.Join(", ", procNames));
#endif
                    storage.save3DProcessNames(procNames);

                    Thread.Sleep(RESCAN_INTERVAL);
                }
                catch (Exception e)
                {
                    Console.WriteLine(
                        "ERROR in 3D Proc Logger Thread: {0}", e.Message);
                }
            }
        }



        public void aggregate()
        {
            //NOT THREAD SAFE...  so don't call it outside of this thread or 
            // it is not guaranteed to be right

            while (false == _shouldStopAggregating)
            {
                Thread.Sleep(AGGREGATE_INTERVAL);
                try
                {
                    Console.WriteLine("AGGREGATING......");
                    var procs = storage.get3DProcesses();

                    Transfer t = new Transfer(storage);
                    Dictionary<String, List<TimeSlice>> slicedProcs =
                        t.sliceProcs(procs);

#if DEBUG
                    foreach(var k in slicedProcs)
                    {
                        logToFile(slicedProcs);
                        Console.WriteLine("Key: {0} - Slices: {1}",
                            k.Key, String.Join(",", k.Value));
                    }
#endif

                    if ( gtconn.sendSlices(slicedProcs) )
                    {
                        // delete the data just sent.
                        // until procs[procs.Count - 1].Item1
                    }

                    //if (pingList.Count > 0)
                    //    storage.deletePingsUpTo(pingList[pingList.Count - 1]);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

#if DEBUG
        private void logToFile(Dictionary<String, List<TimeSlice>> slicedProcs)
        {
            string path = "AggregateLog.txt";
            // This text is added only once to the file.
            if (!File.Exists(path))
                File.Create(path).Close();

            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(DateTime.Now);
                    foreach( var kv in slicedProcs)
                    {
                        sw.WriteLine(String.Format("{0}: {1}", 
                            kv.Key, kv.Value));
                    }
                sw.WriteLine("----------------------------------------------");
            }
        }
#endif

    }
}
