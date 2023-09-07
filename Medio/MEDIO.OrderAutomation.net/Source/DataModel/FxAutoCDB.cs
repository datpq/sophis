
using MEDIO.OrderAutomation.net.Source.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace MEDIO.OrderAutomation.NET.Source.DataModel
{
    public class FxAutoCDB
    {
        [ReadOnly(true)]
        public string ParentID { get; set; }

        [ReadOnly(true)]
        public string ID { get; set; }

        [ReadOnly(true)]
        public string Name { get; set; }

        [ReadOnly(true)]
        public double? Balance { get; set; }

        [ReadOnly(true)]
        public double? BalanceRounded { get; set; }

        public string Currency { get; set; }

        public int NodeType { get; set; }

        [ReadOnly(true)]
        public double? Amount { get; set; }

        [ReadOnly(true)]
        public double? AmountCurGlb { get; set; }

        [ReadOnly(true)]
        public double? MedioMarketValueCurGlb { get; set; }

        [ReadOnly(true)]
        public double? MedioMarketValue { get; set; }

        [ReadOnly(true)]
        public string CurrencyOri { get; set; }

        public int BPS { get; set; }

        public double? Threshold { get; set; }

        [ReadOnly(true)]
        public double? AmountRounded { get; set; }

        [ReadOnly(true)]
        public double? AmountRaised { get; set; }

        [ReadOnly(true)]
        public double? WeightNav { get; set; }

        [ReadOnly(true)]
        public DateTime? DateSettlement { get; set; }

        public static ICollection<FxAutoCDB> FetchData(DateTime dateTime, int[] arrSleeveUds, string[] arrCurrencies)
        {
            CSxUtils.GenerateFXSleeves(dateTime, arrSleeveUds, arrCurrencies);
            return @"
SELECT PARENTID, ID, NAME, BALANCE, CURRENCY, BPS, THRESHOLD, AMOUNT, WEIGHTNAV, DATESETTLEMENT, NODETYPE,
    CASE WHEN T.ROUNDING_RULE=1 OR NODETYPE=2 THEN ROUND(BALANCE) ELSE BALANCE END BALANCEROUNDED,
    CASE WHEN T.ROUNDING_RULE=1 OR NODETYPE=2 THEN ROUND(AMOUNT) ELSE AMOUNT END AMOUNTROUNDED
FROM MEDIO_FX_SLEEVES FX
    LEFT JOIN TITRES T ON TO_CHAR(T.SICOVAM) = FX.ID
ORDER BY PARENTID, NAME".BindFromQuery((IDataReader r) =>
                new FxAutoCDB
                {
                    ParentID = r["PARENTID"] == DBNull.Value ? null : r["PARENTID"].ToString(),
                    ID = r["ID"] == DBNull.Value ? null : r["ID"].ToString(),
                    Name = r["NAME"].ToString(),
                    Balance = r["BALANCE"] == DBNull.Value ? null : (double?)Convert.ToDouble(r["BALANCE"]),
                    Currency = r["CURRENCY"] == DBNull.Value ? null : r["CURRENCY"].ToString(),
                    BPS = r["BPS"] == DBNull.Value ? 0 : Convert.ToInt32(r["BPS"]),
                    Threshold = r["THRESHOLD"] == DBNull.Value ? null : (double?)Convert.ToDouble(r["THRESHOLD"]),
                    NodeType = Convert.ToInt32(r["NODETYPE"]),
                    Amount = r["AMOUNT"] == DBNull.Value ? null : (double?)Convert.ToDouble(r["AMOUNT"]),
                    WeightNav = r["WEIGHTNAV"] == DBNull.Value ? null : (double?)Convert.ToDouble(r["WEIGHTNAV"]),
                    DateSettlement = r["DATESETTLEMENT"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(r["DATESETTLEMENT"]),
                    BalanceRounded = r["BALANCEROUNDED"] == DBNull.Value ? null : (double?)Convert.ToDouble(r["BALANCEROUNDED"]),
                    AmountRounded = r["AMOUNTROUNDED"] == DBNull.Value ? null : (double?)Convert.ToDouble(r["AMOUNTROUNDED"]),
                }
            );
        }
    }
}
