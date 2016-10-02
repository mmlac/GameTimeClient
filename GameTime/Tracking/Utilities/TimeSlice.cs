using System;

namespace GameTime.Tracking.Utility
{
    class TimeSlice
    {
        DateTime from;
        DateTime to;
        bool empty = true;

        public bool add(DateTime dt)
        {
            if (true == empty)
            {
                from = dt;
                to = dt;
                empty = false;
            }
            else
            {
                if (to.AddMilliseconds(
                    ProcessLogger.RESCAN_INTERVAL * 1.7) > dt)
                    to = dt;
                else
                    return false;

            }
            return true;
        }

        public bool isEmpty()
        {
            return empty;
        }

        override public String ToString()
        {
            return String.Format("TimeSlice({0} - {1}) ({2} minutes)",
                from, to, (to - from).TotalMinutes);
        }

    }
}
