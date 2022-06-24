using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bpa_Praktikum
{
    internal class TimeTracker
    {
        private DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public TimeTracker()
        {

        }

        public DateTime Start()
        {
            StartTime = DateTime.Now;
            return StartTime;
        }

        public DateTime Stop()
        {
            EndTime = DateTime.Now;
            return EndTime;
        }

        public TimeSpan GetDifference()
        {
            return EndTime - StartTime;
        }
    }
}
