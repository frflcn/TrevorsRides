using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrevorsRidesHelpers
{
    
    public class VersionControl
    {
        public Version MinimumVersion { get; set; }
        public Version RecommendedVersion { get; set; }
        public Version LatestVersion { get; set; }
        public VersionControl(Version minimumVersion, Version recommendedVersion, Version latestVersion)
        {
            MinimumVersion = minimumVersion;
            RecommendedVersion = recommendedVersion;
            LatestVersion = latestVersion;
        }
        public VersionControl()
        {

        }
    }
}
