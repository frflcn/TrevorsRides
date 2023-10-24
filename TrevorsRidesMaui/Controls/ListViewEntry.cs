using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrevorsRidesHelpers.GoogleApiClasses;

namespace TrevorsRidesMaui.Controls
{
    public class ListViewEntry
    {
        public PlaceAutocompletePrediction Prediction { get; set; }
        public string MainText { get; set; }
        public string? SubText { get; set; }
        public ListViewEntry() { }
    }
}
