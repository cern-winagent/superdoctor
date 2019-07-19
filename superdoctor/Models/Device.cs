using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace superdoctor.Models
{
    public class Device
    {
        public string Name { set; get; }
        public float? HighLimit { set; get; }
        public float? LowLimit { set; get; }
        public float Value { set; get; }
        public string Unit { set; get; }
    }
}
