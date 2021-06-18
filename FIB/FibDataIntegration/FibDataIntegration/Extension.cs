using System;

namespace FibDataIntegration
{
    public static class Extension
    {
        public static string ToStringStandardFormat(this TimeSpan ts)
        {
            return string.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds/10);
        }
    }
}
