﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surge.Util
{
    internal static class UnixHelpers
    {
        public static DateTime FromUnixTime(this long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }
    }
}
