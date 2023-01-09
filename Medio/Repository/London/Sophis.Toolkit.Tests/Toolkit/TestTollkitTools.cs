// -----------------------------------------------------------------------
//  <copyright file="TestTollkitTools.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/02/19</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.Tests.Toolkit
{
    using System;
    using Extensions;
    using NUnit.Framework;

    [TestFixture]
    public class TestTollkitTools
    {
        [Test]
        [TestCase(18, 17, Description = "Tuesday")]
        [TestCase(17, 14, Description = "monday")]
        [TestCase(16, 14, Description = "sunday")]
        [TestCase(15, 14, Description = "saturday")]
        [TestCase(14, 13, Description = "friday")]
        [TestCase(13, 12, Description = "thusday")]
        [TestCase(12, 11, Description = "Wednesday")]
        public void TestDateTimePreviousBusinessDayExtension(int day, int expectedDay)
        {
            DateTime d1 = new DateTime(2012, 12, day).PreviousBusinessDay();
            Assert.AreEqual(new DateTime(2012, 12, expectedDay), d1);
        }
    }
}