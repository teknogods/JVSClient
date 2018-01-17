using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vJoyInjector
{
    public class JvsLimits
    {
        public List<JvsMinMax> JvsChannelLimits { get; set; }
    }

    public class JvsMinMax
    {
        public byte Minimum { get; set; }
        public byte Maximum { get; set; }
    }
}
