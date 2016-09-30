using System;
using System.Collections.Generic;

namespace GameTimeClient
{
    /// <summary>
    ///     ProcessLogger is responsible for tracking 3D applitacions and
    ///     saving the observations to the database.
    /// </summary>
    class ProcessLogger
    {

        public ProcessLogger()
        {
            // init the DB connection

            // start Timer to run update3DProcesses()
            
        }

        /// <summary>
        ///     Callback called by a Timer to get 3D processes and write
        ///     them to the database.
        /// </summary>
        /// <param name="state">Timer state</param>
        private void update3DProcesses(object state)
        {
            List<String> procNames = ProcessChecker.Get3DProcessNames();
            // save to DB

            // save a ping to DB (when did we last check) to make sure we don't
            // count time when PC is sleeping / turned off before we could track
            // a shutdown of the game
        }

    }
}
