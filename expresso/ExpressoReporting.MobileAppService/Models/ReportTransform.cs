using System.Collections.Generic;

namespace ExpressoReporting.MobileAppService.Models
{
    public class ReportTransform
    {
        public const string TransformTypeStyleSheet = "Style Sheet";
        public const string TransformTypeCrystalReport = "Crystal Reports";
        public const string TransformTypeExternalCommand = "External Command";

        public ReportTransform()
        {
            Params = new List<ReportTransformParam>();
        }

        public int Step { get; set; }
        public string TransformType { get; set; }

        public List<ReportTransformParam> Params { get; set; }
    }
}
