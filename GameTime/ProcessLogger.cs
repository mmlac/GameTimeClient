using System;
using System.Collections.Generic;
using System.Threading;

namespace GameTime
{
    /// <summary>
    ///     ProcessLogger is responsible for tracking 3D applitacions and
    ///     saving the observations to the database.
    /// </summary>
    class ProcessLogger
    {

        Data data;

        /// <summary>
        ///     Initialize the ProcessLogger with a Data intstance that will
        ///     be used to save the output of the ProcessChecker
        /// </summary>
        /// <param name="_data">Data Store</param>
        public ProcessLogger(Data _data)
        {
            data = _data; 
        }

        private volatile bool _shouldStop = false;

        /// <summary>
        ///     Ask the ProcessLogger loop to stop
        /// </summary>
        public void requestStop()
        {
            _shouldStop = true;
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
            Thread.Sleep(15000);
            List<String> ignoreProcs = 
                ProcessChecker.Get3DProcessNames(new List<String>());

#if DEBUG
            Console.WriteLine("Ignoring the following processes as they were" +
                " seem during startup: {0}", String.Join(",", ignoreProcs));
#endif

            while (false == _shouldStop)
            {
                try
                {
                    List<String> procNames = 
                        ProcessChecker.Get3DProcessNames(ignoreProcs);
                    Console.WriteLine(String.Join(", ", procNames));
                    data.save3DProcessNames(procNames);
                    Thread.Sleep(60000);
                }
                catch (Exception e)
                {
                    Console.WriteLine(
                        "ERROR in 3D Proc Logger Thread: {0}", e.Message);
                }
            }
        }

    }
}
