using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryboarderUtility
{
    public static class MiscUtil
    {
        public static int RingShift(int val, int bits) {
            return (int)(((uint)val << bits) | ((uint)val >> (32 - bits)));
        }
    }
}
