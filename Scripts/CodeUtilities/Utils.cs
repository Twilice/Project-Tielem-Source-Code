using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.CodeUtilities
{
    public static class Utils
    {
        public static float RemapRange(this float val, float oldMin, float oldMax, float newMin, float newMax)
        {
            return (((val - oldMin) * (newMax - newMin)) / (oldMax - oldMin)) + newMin;
        }

        public static float RemapRangeClamped(this float val, float oldMin, float oldMax, float newMin, float newMax)
        {
            if (val < oldMin)
                return newMin;
            else if (oldMax < val)
                return newMax;
            else
                return (((val - oldMin) * (newMax - newMin)) / (oldMax - oldMin)) + newMin;
        }
    }
}
