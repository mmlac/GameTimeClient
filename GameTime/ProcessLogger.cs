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

        public ProcessLogger(Data _data)
        {
            data = _data;

            Timer timer = new Timer(log3DProcesses, 1, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(8));
        }

        /// <summary>
        ///     Callback called by a Timer to get 3D processes and write
        ///     them to the database.
        /// </summary>
        /// <param name="state">Timer state</param>
        private void log3DProcesses(object state)
        {
            Console.WriteLine("Called Timer");
            List<String> procNames = ProcessChecker.Get3DProcessNames();
            Console.WriteLine(String.Join(", ", procNames));
            //data.save3DProcessNames(procNames);
        }

    }
}
