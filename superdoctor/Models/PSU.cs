using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace superdoctor.Models
{
    public class PSU
    {
        public string Name { set; get; }
        public List<Device> Devices { set; get; }
    }
}
