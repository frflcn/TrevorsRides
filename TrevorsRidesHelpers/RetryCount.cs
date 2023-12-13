using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrevorsRidesHelpers
{
    public class RetryCount
    {
        public short Count { get; private set; }
        public DateTime LastReset { get; private set; }
        public DateTime NextReset {  get; private set; }

        /// <summary>
        /// Maximum number of retry times between resets
        /// </summary>
        public static readonly short MaxCount = 10;

        /// <summary>
        /// The number of minutes between resets
        /// </summary>
        public static readonly short ResetTime = 10;


        public RetryCount()
        {
            Count = 0;
            LastReset = DateTime.UtcNow;
            NextReset = DateTime.UtcNow.AddMinutes(ResetTime);
        }
        public RetryCount(string retryCount)
        {
            string[] values = retryCount.Split(',');
        }

        private void Reset()
        {
            Count = 0;
            LastReset = DateTime.UtcNow;
        }
        public void Increment()
        {
            Count++;
        }
        public static RetryCount operator ++(RetryCount count)
        {
            count.Increment();
            return count;
        }
        public bool TryReset()
        {
            if (NextReset < DateTime.UtcNow)
            {
                Reset();
                return true;
            }
            return false;
        }
        public bool CanRetry()
        {
            TryReset();
            if (Count < MaxCount)
            {
                return true;
            }
            return false;
        }
        public bool Retry()
        {
            if (CanRetry())
            {
                Count++;
                return true;
            }
            return false;
        }
    }
}
