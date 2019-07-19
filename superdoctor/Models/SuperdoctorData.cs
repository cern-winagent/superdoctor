using System;
using System.Collections.Generic;

namespace superdoctor.Models
{
    public class SuperdoctorData
    {
        public string HostName { set; get; }
        public DateTime Date { set; get; }
        public List<Device> Devices { set; get; }
        public List<PSU> PSUs { set; get; }

        public String UnprocessedOutput { set; get; }

    }
}
