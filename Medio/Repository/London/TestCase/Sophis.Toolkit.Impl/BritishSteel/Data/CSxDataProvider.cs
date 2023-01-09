// -----------------------------------------------------------------------
//  <copyright file="CSxDataProvider.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/01/25</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.BritishSteel.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.IO;
    using DataAccess;
    using Extensions;
    using Model;

    public class CSxDataProvider : IDataProvider
    {
        public IList<BenchmarkUpdate> LoadIndexToBenchmarkMappings()
        {
            using (var reader = GetDbDataReader("select * from BS_BENCHMARK_WEIGHTS", new string[] {}, new object[] {}))
            {
                var listIndexToBench = new List<BenchmarkUpdate>();

                while (reader.Read())
                {
                    var benchmark = reader.GetActualValue<int>(reader.GetOrdinal("BENCHMARK"));
                    var index = reader.GetActualValue<int>(reader.GetOrdinal("BASKET"));
                    var isFloat = reader.GetActualValue<bool>(reader.GetOrdinal("IS_FLOAT"));

                    listIndexToBench.Add(new BenchmarkUpdate(benchmark, index, isFloat));
                }
                return listIndexToBench;
            }
        }

        public EquityHistoric LoadEquityHistoric(int sicovam, DateTime date)
        {
            const string query = "select SICOVAM, JOUR, D as LAST, FTSMCF, FTNOSH, FTSNOSH from historique where sicovam =:pSicovam and JOUR =:pDate order by jour desc";
            var paramNames = new[] {"pSicovam", "pDate"};
            var paramValues = new object[] {sicovam, date};

            using (var reader = GetDbDataReader(query, paramNames, paramValues))
            {
                if (reader.Read())
                {
                    return new EquityHistoric
                        {
                            Sicovam = reader.GetActualValue<int>(reader.GetOrdinal("SICOVAM")),
                            Date = reader.GetActualValue<DateTime>(reader.GetOrdinal("JOUR")),
                            Ftsnosh = reader.GetActualValue<decimal>(reader.GetOrdinal("FTSNOSH")),
                            Ftsmcf = reader.GetActualValue(reader.GetOrdinal("FTSMCF"), 1.0m),
                            Ftnosh = reader.GetActualValue<decimal>(reader.GetOrdinal("FTNOSH")),
                            Last = reader.GetActualValue<decimal>(reader.GetOrdinal("LAST"))
                        };
                }

                throw new InvalidDataException(string.Format("There's no historical data for the {0} for instrument : {1}", date.ToShortDateString(), sicovam));
            }
        }

        public virtual DbDataReader GetDbDataReader(string query, string[] paramNames, object[] paramValues)
        {
            using (var cmd = DBContext.Connection.CreateCommand())
            {
                cmd.CommandText = query;
                for (var i = 0; i < paramNames.Length; i++)
                {
                    cmd.Parameters.Add(paramNames[i], paramValues[i]);
                }
                return cmd.ExecuteReader();
            }
        }
    }
}