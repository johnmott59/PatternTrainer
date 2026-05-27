using System;
using System.Collections.Generic;
using System.Linq;
using SchwabLib;

namespace CandlePatternML
{
    public partial class Program
    {
        /// <summary>
        /// Counts the number of consecutive days the POC (Point of Control) has been trending up or down.
        /// Starting from the most recent day, counts how many days in a row the POC has moved in the same direction.
        /// </summary>
        /// <param name="volProfiles">List of volume profile results ordered chronologically</param>
        /// <returns>
        /// Positive integer if POC is trending up (e.g., +3 means 3 consecutive up days),
        /// Negative integer if POC is trending down (e.g., -2 means 2 consecutive down days),
        /// Zero if less than 2 data points or no clear trend
        /// </returns>
        public int CalculateConsecutivePOCDays(List<SchwabLib.Studies.VolumeProfileResult> volProfiles)
        {
            if (volProfiles == null || volProfiles.Count < 2)
                return 0;

            // Extract POC values
            List<double> POClist = volProfiles.Select(vp => vp.POC).ToList();

            // Reverse to start from most recent
            POClist.Reverse();

            int count = 0;

            // Compare most recent day (index 0) with previous day (index 1)
            if (POClist[0] > POClist[1])
            {
                // Trending up - start with 1 and count how many more consecutive up days
                count = 1;
                for (int n = 1; n < POClist.Count - 1; n++)
                {
                    if (POClist[n] > POClist[n + 1])
                        count++;
                    else
                        break;
                }
            }
            else if (POClist[0] < POClist[1])
            {
                // Trending down - start with -1 and count how many more consecutive down days
                count = -1;
                for (int n = 1; n < POClist.Count - 1; n++)
                {
                    if (POClist[n] < POClist[n + 1])
                        count--;
                    else
                        break;
                }
            }

            return count;
        }
    }
}
