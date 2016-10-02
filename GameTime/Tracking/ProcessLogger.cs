using GameTime.Tracking.IO;
using GameTime.Tracking.Utility;
using System;
using System.Collections.Generic;
using System.Threading;

namespace GameTime.Tracking
{
    /// <summary>
    ///     ProcessLogger is responsible for tracking 3D applitacions and
    ///     saving the observations to the database.
    /// </summary>
    class ProcessLogger
    {

        public const int INITIAL_SLEEP_TIME = 5000;
        public const int RESCAN_INTERVAL = 5000;
        public const int AGGREGATE_INTERVAL = 2000;

        Storage storage;

        private volatile bool _shouldStopLogging = false;
        private volatile bool _shouldStopAggregating = false;

        private ProcessLogger() { }

        /// <summary>
        ///     Initialize the ProcessLogger with a Data intstance that will
        ///     be used to save the output of the ProcessChecker
        /// </summary>
        /// <param name="_storage">Data Store</param>
        public ProcessLogger(Storage _storage)
        {
            storage = _storage; 
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
            _shouldStopLogging = true;
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
                " seem during startup: {0}", String.Join(",", ignoreProcs));
#endif
            Thread.Sleep(RESCAN_INTERVAL);

            while (false == _shouldStopLogging)
            {
                try
                {
                    List<String> procNames =
                        ProcessChecker.Get3DProcessNames(ignoreProcs);
                    Console.WriteLine(String.Join(", ", procNames));
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
            while (false == _shouldStopAggregating)
            {
                Thread.Sleep(AGGREGATE_INTERVAL);
                try
                {
                    Console.WriteLine("AGGREGATING......");
                    List<DateTime> pingList = storage.getPings();
                    Console.WriteLine(String.Join(", ", pingList));

                    Transfer t = new Transfer(storage);
                    List<TimeSlice> alives = t.slicePings(pingList);
                    Console.WriteLine(String.Join(",", alives));

                    if (pingList.Count > 0)
                        storage.deletePingsUpTo(pingList[pingList.Count - 1]);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

    }
}
