using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sophis.oms;
using System.Reflection;
using sophis.utils;
using sophis.instrument;
using MEDIO.CORE.Tools;
using sophis.static_data;
using MEDIO.OrderAutomation.net.Source.DataModel;
using sophis.oms.entry;
using sophis.DAL;
using Sophis.OrderBookCompliance;

namespace MEDIO.OrderAutomation.NET.Source.OrderCreationValidator
{
    class CSxOrderCreationBondValidator : IOrderCreationValidator
    {
        private const string STR_IS_UNIT_TRADED = "IS_UNIT_TRADED";

        private bool CheckSectorIfBondIsUnitTraded(CSMBond bond)
        {
            bool isUnitTraded = false;
            int sectorID = CSMSector.GetSectorIDFromName(STR_IS_UNIT_TRADED);
            if (sectorID == 0)
                return isUnitTraded;
            CSMSectorData sector = bond.GetSector(sectorID);
            if (sector == null)
                return isUnitTraded;
            CMString sectorName = "";
            CMString sectorCode = "";
            sectorName = sector.GetName();
            sectorCode = sector.GetCode();
            if (sectorName == "Y" && sectorCode == "Y")
                isUnitTraded = true;
            return isUnitTraded;
        }

        public ValidationResult Validate(IOrder order, bool creating)
        {
            using (var LOG = new CSMLog())
            {
                LOG.Begin(this.GetType().Name, MethodBase.GetCurrentMethod().Name);

                SingleOrder sOrder = (SingleOrder)order;
                if (sOrder != null)
                {
                    if (sOrder.Target.SecurityType != sophis.oms.ESecurityType.Bond)
                        return new ValidationResult() { IsValid = true };
                    CSMBond bond = CSMBond.GetInstance(sOrder.Target.SecurityID);
                    if (bond == null)
                    {
                        string msg = String.Format("Order #{0}. Failed to initialize Bond {1}.", sOrder.ID, sOrder.Target.SecurityID);
                        return new ValidationResult() { IsValid = false, ValidationMessages = new List<string>() { msg } };
                    }
                    if (sOrder.QuantityData.OrderedType != EQuantityType.Unit && CheckSectorIfBondIsUnitTraded(bond))
                    {
                        LOG.Write(CSMLog.eMVerbosity.M_debug, String.Format("Order #{0} is an order on the Bond.", order.ID));
                        double notional = bond.GetNotional();
                        if (notional == 0.0)
                        {
                            string msg = String.Format("Order #{0}. Invalid Notional {1} for Bond {2}.", sOrder.ID, notional, sOrder.Target.SecurityID);
                            return new ValidationResult() { IsValid = false, ValidationMessages = new List<string>() { msg } };
                        }
                        sOrder.QuantityData.OrderedType = EQuantityType.Unit;
                        sOrder.QuantityData.Model = QuantityPrototype.Unit;
                        double orderQtyInUnits = sOrder.QuantityData.OrderedQty / notional;
                        sOrder.QuantityData.OrderedQty = orderQtyInUnits;
                        sOrder.AllocationRulesSet.QuantityType = EQuantityType.Unit;
                        foreach (AllocationRule allocation in sOrder.AllocationRulesSet.Allocations)
                        {
                            if (allocation == null) continue;
                            double oldAllocatedQty = allocation.AllocatedQuantity;
                            double oldQty = allocation.Quantity;
                            int entityId = allocation.EntityID;
                            allocation.Quantity = oldQty / notional;
                            allocation.AllocatedQuantity = oldAllocatedQty / notional;
                        }
                    }
                }
            }
            return new ValidationResult() { IsValid = true };
        }

        // Use Sector instead to check if a bond IS_UNIT_TRADED=Y.
        private bool CheckExternalReferencesIfBondIsUnitTraded(int sicovam)
        {
            bool isUnitTraded = false;

            string query = String.Format(@"SELECT
                                                value
                                            FROM
                                                extrnl_references_instruments
                                            WHERE
                                                sophis_ident = {0}
                                                AND ref_ident = (
                                                    SELECT
                                                        ref_ident
                                                    FROM
                                                        extrnl_references_definition
                                                    WHERE
                                                        ref_name = '{1}'
                                                )",
                                                  sicovam,
                                                  STR_IS_UNIT_TRADED
                                                  );
            isUnitTraded = CSxDBHelper.GetOneRecord<string>(query).ToUpper().Equals("Y");
            return isUnitTraded;
        }
    }
}
