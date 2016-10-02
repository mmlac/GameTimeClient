using GameTime.Tracking.Utility;
using System;
using System.Collections.Generic;

namespace GameTime.Tracking.IO
{
    class Transfer
    {

        Storage storage;

        private Transfer() { }

        public Transfer(Storage _storage)
        {
            storage = _storage;
        }

        public List<TimeSlice> slicePings(List<DateTime> pingList)
        {
            List<TimeSlice> slices = new List<TimeSlice>();        
            TimeSlice ts = new TimeSlice();

            foreach (DateTime p in pingList)
            {    
                bool add = ts.add(p);
                if (false == add)
                {
                    slices.Add(ts);
                    ts = new TimeSlice();
                    ts.add(p);
                }
            }

            if (!ts.isEmpty())
                slices.Add(ts);

            return slices;
        }

        public void getGameTime()
        {

        } 
    }


    
}
