using GameTimeClient.Tracking.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace GameTimeClient.Tracking.IO
{
    class Transfer
    {

        Storage storage;

        private Transfer() { }

        public Transfer(Storage _storage)
        {
            storage = _storage;
        }



        public Dictionary<String, List<TimeSlice>> sliceProcs(
            List<Tuple<DateTime, String>> procList)
        {
            var processSlicesDict = new Dictionary<String, List<TimeSlice>>();
            var currentSlices     = new Dictionary<String, TimeSlice>();

            foreach (var procTrack in procList)
            {
                addDict(ref processSlicesDict, ref currentSlices, 
                    "ping", procTrack.Item1);

                if (false == procTrack.Item2.Equals(""))
                {
                    string[] procs = procTrack.Item2.Split(',');
                    foreach (string p in procs)
                    {
                        addDict(ref processSlicesDict, ref currentSlices,
                            p, procTrack.Item1);
                    }
                }
            }

            foreach(var c in currentSlices)
            {
                processSlicesDict[c.Key].Add(c.Value);
            }

            return processSlicesDict;
        }



        private void addDict(
            ref Dictionary<String, List<TimeSlice>> procSlices,
            ref Dictionary<String, TimeSlice> currentSlice,
            String p,
            DateTime dt)
        {
            if ( false == currentSlice.ContainsKey(p) )
                currentSlice.Add(p, new TimeSlice());

            if ( false == procSlices.ContainsKey(p) )
                procSlices.Add(p, new List<TimeSlice>());

            if ( false == currentSlice[p].add(dt) )
            {
                procSlices[p].Add(currentSlice[p]);
                currentSlice[p] = new TimeSlice();
                currentSlice[p].add(dt);
            }
        }


        



       
    }


    
}
