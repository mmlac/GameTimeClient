using GameTimeClient.Tracking.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameTime.IO
{
    /// <summary>
    ///     Class handling all state and communication between client and 
    ///     server.
    /// </summary>
    class GameTimeConnection
    {

        private JsonSerializerSettings jsonSettings =
            new JsonSerializerSettings
            {
                DateFormatHandling    = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling  = DateTimeZoneHandling.Utc
            };

        public bool sendSlices(Dictionary<String, List<TimeSlice>> slices)
        {
            try
            {
                String jsonSlices = JsonConvert.SerializeObject(slices);
                Console.WriteLine(jsonSlices);

                //TODO: * build HTTPS url with auth token
                //      * deal with token renewal, etc.
                //      -> This should be handled in a separate wrapper
                //         class.
                // GameTimeConnection.upload(jsonSlices)

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to upload slices: {0}", e.Message);
            }
            return false;
        }

    }
}
