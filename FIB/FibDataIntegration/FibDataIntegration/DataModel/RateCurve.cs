using System;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FibDataIntegration.DataModel
{
    public class RateCurve
    {
        [JsonProperty(PropertyName = "dateEcheance")]
        [JsonConverter(typeof(DateTimeConverterRateCurve))]
        public DateTime Date { get; set; }

        [JsonProperty(PropertyName = "dateValeur")]
        [JsonConverter(typeof(DateTimeConverterRateCurve))]
        public DateTime ValueDate { get; set; }

        [JsonProperty(PropertyName = "tmp")]
        public double Rate { get; set; }
    }

    public class DateTimeConverterRateCurve : IsoDateTimeConverter
    {
        public DateTimeConverterRateCurve()
        {
            base.DateTimeFormat = "yyyy-MM-dd";
        }
    }
}
