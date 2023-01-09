using System;
using System.Globalization;

public class Program
{
    public static void Main(string[] args)
    {
        var colVal = args != null && args.Length > 0 ? args[0] : "";
        var lineVal = args != null && args.Length > 1 ? args[1] : "";
        solve(colVal, lineVal);
    }

    private static DateTime LastDayOfWeek(DateTime dt)
    {
        if (dt.DayOfWeek == DayOfWeek.Sunday) return dt.AddDays(-2);
        if (dt.DayOfWeek == DayOfWeek.Saturday) return dt.AddDays(-1);
        return dt;
    }

    private static void solve(string colVal, string lineVal)
    {
        Console.Out.Write(123456789);
    }
}