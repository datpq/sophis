namespace ExpressoReporting.DataModel
{
    public class Parameter
    {
        public string Name { get; set; }
        public ParameterType Type { get; set; }
        public string Value { get; set; }
        //public Report Report { get; set; }
    }

    public enum ParameterType
    {
        String,
        Integer,
        Date,
        Folio,
        Instrument,
        Unknown
    }
}
