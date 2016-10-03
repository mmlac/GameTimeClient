using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;

namespace GameTimeClient.Tracking.Utility
{
    [JsonConverter(typeof(TimeSliceConverter))]
    class TimeSlice
    {
        public DateTime from { get; private set; }
        public DateTime to { get; private set; }

        public bool empty
        {
            get; private set;
        } = true;

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

        override public String ToString()
        {
            return String.Format("TimeSlice({0} - {1}) ({2} minutes)",
                from, to, (to - from).TotalMinutes);
        }

    }


    
    class TimeSliceConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (typeof(TimeSlice).IsAssignableFrom(objectType));
        }

        public override bool CanRead
        {
            get
            {
                return false;
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JObject jo = new JObject();
            JObject slice = new JObject();

            TimeSlice ts = (TimeSlice)value;
            slice.Add("from", ts.from);
            slice.Add("to",   ts.to);

            jo.Add("TimeSlice", slice);
           
            jo.WriteTo(writer);
        }
    }
}
