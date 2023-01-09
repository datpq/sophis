using System;
using System.Collections.Generic;
using System.Text;

using SophisETL.Common;
using SophisETL.Common.Logger;
using SophisETL.Common.Tools;

using SophisETL.Transform.PrintDebug.Xml;



namespace SophisETL.Transform.PrintDebug
{
    [SettingsType(typeof(Settings), "_Settings")]
    public class PrintDebug : AbstractBasicTransformTemplate
    {
        //// Internal Members
        private Settings _Settings { get; set; }

        public override void Init()
        {
            base.Init();
            // No specific checks can be done at this point
        }

        protected override Record Transform( Record record )
        {
            // basic print of the record, no modification in the record
            System.Console.Out.WriteLine(record.ToString());

            return record;
        }
    }
}
