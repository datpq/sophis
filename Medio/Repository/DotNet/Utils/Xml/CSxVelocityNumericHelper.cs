// -----------------------------------------------------------------------
//  <copyright file="CSxVelocityNumericHelper.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/03/05</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.Utils.Xml
{
    using System.Globalization;

    public class CSxVelocityNumericHelper
    {
        public decimal ToDecimal(object value)
        {
            return decimal.Parse(value.ToString(), NumberStyles.Any);
        }

        public double ToDouble(object value)
        {
            return double.Parse(value.ToString(), NumberStyles.Any);
        }

        public double ToInt(object value)
        {
            return double.Parse(value.ToString(), NumberStyles.Any);
        }

        public decimal Multiply(decimal value1, decimal value2)
        {
            return value1 * value2;
        }

        public double Multiply(double value1, double value2)
        {
            return value1 * value2;
        }

        public int Multiply(int value1, int value2)
        {
            return value1 * value2;
        }

        public double Multiply(object value1, object value2)
        {
            return 0.0;
        }

        public override string ToString()
        {
            return "NumericHelper";
        }
    }
}