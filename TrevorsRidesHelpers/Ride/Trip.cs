using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrevorsRidesHelpers.Ride
{
    public class Trip
    {
        public Guid Id { get; set; }
        public class DriverRequest
        {
            public Guid Id { get; set; }
            public bool HasRequested { get; set; }
            public bool HasAccepted { get; set; }
            public bool HasResponded { get; set; }
        }
    }
}
