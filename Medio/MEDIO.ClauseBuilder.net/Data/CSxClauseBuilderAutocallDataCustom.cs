using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Sophis;
using Sophis.ClauseBuilder;
using sophis.static_data;
using sophis.instrument;
using Sophis.Instrument;
using Sophis.Utils;
using Sophis.Windows;
using Sophis.Windows.Converters;
using Sophis.Windows.Data;
using Sophis.Windows.UserControls;


namespace MEDIO.ClauseBuilder.net.Data
{
    public class CSxClauseBuilderAutocallWizard : ClauseBuilderExoticMaskWizardBase<CSxClauseBuilderAutocallData>
    {
        public CSxClauseBuilderAutocallWizard()
        {
        }

        /// <summary>
        /// If this methods returns true, GenerateOption will call GenerateSchedule (which calls FillGeneratedDates) at the beginning.
        /// </summary>
        /// <param name="exoticMaskData">The Data object</param>
        /// <returns>true if GenerateSchedule must be called at the beginning of GenerateOption</returns>
        protected override bool NeedGenerateSchedule(CSxClauseBuilderAutocallData exoticMaskData)
        {
            return (exoticMaskData != null && exoticMaskData.GeneratedDateList.Count == 0);
        }

        /// <summary>
        /// Step 4 - Generate Schedule callback
        /// </summary>
        /// <param name="exoticMaskData"></param>
        /// <param name="defaultCalendar"></param>
        protected override void FillGeneratedDates(CSxClauseBuilderAutocallData exoticMaskData, CSMClauseBuilderExoticMaskSchedulerData.CalendarInfo defaultCalendar)
        {
            exoticMaskData.GeneratedDateList.Clear();
            var schedulerData = exoticMaskData.ExoticMaskSchedulerData;
            if (schedulerData.CalendarList.Count() == 0 && defaultCalendar != null)
                schedulerData.CalendarList.Add(defaultCalendar);
            CSMCalendar calendarForRoll = ClauseBuilderUtils.GetCalendar(schedulerData, ClauseBuilderUtils.eMAvailabilityType.M_iRolling);

            int issueDate = exoticMaskData.BasicData.IssueDate;
            int endDate = exoticMaskData.BasicData.PeriodEndDate;
            int observationFrequency = exoticMaskData.ExoticMaskSchedulerData.ObservationFrequency;
            int couponFrequency = exoticMaskData.CouponFrequency;
            int brokenDateRefIndex = exoticMaskData.ExoticMaskSchedulerData.BrokenDateRefIndex;
            long brokenDate = exoticMaskData.ExoticMaskSchedulerData.BrokenDate;

            if (observationFrequency == 0 || couponFrequency == 0)
                throw new System.Exception("Autocall frequency is missing, no period will be generated.");

            eMHolidayAdjustmentType adjustmentType = (eMHolidayAdjustmentType)exoticMaskData.ExoticMaskSchedulerData.HolidayAdjustmentTypeIndex;

            //First get all coupon dates
            List<int> couponDateList = ClauseBuilderUtils.GenerateCouponDateList(issueDate,
                endDate,
                couponFrequency,
                calendarForRoll,
                adjustmentType,
                brokenDateRefIndex,
                brokenDate);

            //Then fill all generated dates fields

            CSMMarket market = CSMMarket.GetCSRMarket(exoticMaskData.BasicData.CurrencyCode, exoticMaskData.BasicData.MarketCode);
            eMDayCountBasisType dayCountBasis = eMDayCountBasisType.M_dcb30_360;
            SSMDayCountCalculation calc = null;
            if (market != null)
            {
                dayCountBasis = market.GetBondDayCountBasisType();
                calc = new SSMDayCountCalculation(market.GetBondPeriodicityType(), calendarForRoll);
            }
            else
                calc = new SSMDayCountCalculation(calendarForRoll);
            CSMDayCountBasis basis = CSMDayCountBasis.GetCSRDayCountBasis(dayCountBasis);
            double couponPerAnnum = exoticMaskData.MaxCouponIfKO;
            double periodConditionalCoupon = 0;
            bool useFixedPeriodCoupon = false;
            double fixedPeriodCoupon = 0;
            double periodCoupon = 0;
            int periodCount = 1;

            //SRQ-24912
            //When frequency is in month or year, use a simple fraction to calculate a fixed period coupon
            //(for instance, if coupon p.a is 12, and frequency is 3m, coupons will be 3, 6, 9, 12)
            //The aim is to get rounded values when the ratio between coupon p.a and frequency is obvious
            //In other cases, the fixed period coupon will be calculated using basis.GetEquivalentYearCount
            if (sophisTools.CSMDay.IsARelativeDate(couponFrequency))
            {
                DateInterval couponInterval = ClauseBuilderUtils.GetDateIntervalFromInteger(couponFrequency);
                if (couponInterval.Multiplier > 0)
                {
                    if (couponInterval.Period == 'm')
                    {
                        fixedPeriodCoupon = couponPerAnnum * (double)couponInterval.Multiplier / 12.0;
                        periodConditionalCoupon = exoticMaskData.ConditionalCoupon * (double)couponInterval.Multiplier / 12.0;
                        useFixedPeriodCoupon = true;
                    }
                    else if (couponInterval.Period == 'y')
                    {
                        fixedPeriodCoupon = couponPerAnnum * (double)couponInterval.Multiplier;
                        periodConditionalCoupon = exoticMaskData.ConditionalCoupon * (double) couponInterval.Multiplier;
                        useFixedPeriodCoupon = true;
                    }
                    else if (couponDateList.Count > 1)
                    {
                        //if stub forward, use the first period as reference to calculate fixed period coupon, else use last period
                        if (brokenDateRefIndex == 2)//2 == efgForward
                            fixedPeriodCoupon = couponPerAnnum * basis.GetEquivalentYearCount(couponDateList[0], couponDateList[1], calc);
                        else
                            fixedPeriodCoupon = couponPerAnnum * basis.GetEquivalentYearCount(couponDateList[couponDateList.Count - 2], couponDateList[couponDateList.Count - 1], calc);
                        useFixedPeriodCoupon = true;
                    }
                }
            }

            List<int> obsvDateList = ClauseBuilderUtils.GenerateCouponDateList(issueDate,
                endDate,
                observationFrequency,
                calendarForRoll,
                adjustmentType,
                brokenDateRefIndex,
                brokenDate);

            foreach (var date in couponDateList)
            {
                //OR: not sure which method to use to calculate payDate...there are several ways for swaps in jambe_DonneFluxImpl
                //int paymentGap = clauseBuilder.GetSettlementShift();
                //int payDate = calendar.MatchingBusinessDay(date + paymentGap, (eMHolidayAdjustmentType)exoticMaskData.ExoticMaskSchedulerData.HolidayAdjustmentTypeIndex);
                //int payDate = calendar.AddNumberOfDays(date, paymentGap);
                //int payDate = clauseBuilder.GetSettlementDate(date);
                int payDate = date;
                if (market != null)
                    payDate = market.GetSettlementDate(date, issueDate);

                if (useFixedPeriodCoupon)
                {
                    periodCoupon = (double)periodCount * fixedPeriodCoupon;
                }
                else
                {
                    periodCoupon = couponPerAnnum * basis.GetEquivalentYearCount(issueDate, date, calc);
                }

                exoticMaskData.GeneratedDateList.Add(new CSxClauseBuilderAutocallData.GeneratedDates
                {
                    CouponDate = date,
                    CouponPayDate = payDate,
                    AutocallLevel = obsvDateList.Contains(date) ? exoticMaskData.AutocallLevel : 9999999.99,
                    AutocallCoupon = periodCoupon,
                    CouponLevel = exoticMaskData.ConditionalCouponLevel,
                    CouponLastDate = payDate,
                    ConCoupon = periodConditionalCoupon
                });
                periodCount++;
            }

            exoticMaskData.OnPropertyChanged("GeneratedDateList");
        }

        /// <summary>
        /// Fill the Clause Builder "Underlyings" tab
        /// </summary>
        /// <param name="exoticMaskData">The specific data object</param>
        /// <param name="clauseBuilderUserData">The clause builder data object, containing the underlying list to be filled</param>
        protected override void GenerateUnderlyings(CSxClauseBuilderAutocallData exoticMaskData, CSMOptionClauseBuilderUserData clauseBuilderUserData)
        {
            clauseBuilderUserData.UnderlyingList.Clear();

            IEnumerable<int> inputUnderlyingList = from cu in exoticMaskData.UnderlyingList select cu.UnderlyingCode;
            IEnumerable<int> inputDateList = from gd in exoticMaskData.GeneratedDateList select gd.CouponDate;
            ClauseBuilderUtils.GenerateUnderlyings(clauseBuilderUserData.UnderlyingList, inputUnderlyingList, inputDateList);
        }

        /// <summary>
        /// Fill the Clause Builder "Baskets" tab
        /// </summary>
        /// <param name="exoticMaskData">The specific data object</param>
        /// <param name="clauseBuilderUserData">The clause builder data object, containing the basket list to be filled</param>
        protected override void GenerateBaskets(CSxClauseBuilderAutocallData exoticMaskData, CSMOptionClauseBuilderUserData clauseBuilderUserData)
        {
            int packageId = 0;
            string components = "";
            IEnumerable<int> inputUnderlyingList = from cu in exoticMaskData.UnderlyingList select cu.UnderlyingCode;
            List<int> uniqueDateList = new List<int>();
            foreach (var fixing in clauseBuilderUserData.FixingClauseList)
                if (!uniqueDateList.Contains(fixing.Date))
                    uniqueDateList.Add(fixing.Date);
            ClauseBuilderUtils.GenerateBaskets(inputUnderlyingList, uniqueDateList, ref packageId, ref components, clauseBuilderUserData);

            if (exoticMaskData.UnderlyingList.Count > 1)
            {
                ClauseBuilderUtils.AddBasket(packageId, components, exoticMaskData.BasketType, clauseBuilderUserData);
                packageId++;
                if (exoticMaskData.IsAccumulatedCoupon)
                {
                    ClauseBuilderUtils.AddBasket(packageId, components, "Standard", clauseBuilderUserData);
                    packageId++;
                }
            }
        }

        /// <summary>
        /// Fill the Clause Builder "Fixings" and "Payoff" tab
        /// </summary>
        /// <param name="exoticMaskData">The specific data object</param>
        /// <param name="clauseBuilderUserData">The clause builder data object, containing the fixing list and clause list to be filled</param>
        protected override void GenerateUnderlyingsAndFixingsAndClauses(CSxClauseBuilderAutocallData exoticMaskData, CSMOptionClauseBuilderUserData clauseBuilderUserData)
        {
            GenerateFixings(exoticMaskData, clauseBuilderUserData); //fixing must be generating before underlying because of commo first nearby generation.
            GenerateUnderlyings(exoticMaskData, clauseBuilderUserData);
            GenerateBaskets(exoticMaskData, clauseBuilderUserData);
            GenerateClauses(exoticMaskData, clauseBuilderUserData);
            IEnumerable<int> inputUnderlyingList = from cu in exoticMaskData.UnderlyingList select cu.UnderlyingCode;
            ClauseBuilderUtils.CompleteBasketForBarrier(clauseBuilderUserData, inputUnderlyingList);
            ClauseBuilderUtils.InitBasketClauseDataAll(clauseBuilderUserData);

            if (exoticMaskData.CouponChecked)
            {
                GenerateCouponFixings(exoticMaskData, clauseBuilderUserData);
                GenerateCouponCluases(exoticMaskData, clauseBuilderUserData);
                ModifyCouponClauses(exoticMaskData, clauseBuilderUserData);
            }
        }

        private void ShiftIndexesForClause(int clauseIdx, int nbShift, int shiftStartIndex, CustomBindingList<CSMClauseBuilderOptionClauseData> OptionClauseList)
        {
            if (clauseIdx >= OptionClauseList.Count) return;
            OptionClauseList[clauseIdx].Properties = OptionClauseBuilderDataManager.ShiftIndexes(OptionClauseBuilderDataManager.GetIndexList(OptionClauseList[clauseIdx].Properties), nbShift, shiftStartIndex);
            OptionClauseList[clauseIdx].Child = OptionClauseBuilderDataManager.ShiftIndexes(OptionClauseBuilderDataManager.GetIndexList(OptionClauseList[clauseIdx].Child), nbShift, shiftStartIndex);
            OptionClauseList[clauseIdx].Continuation = OptionClauseBuilderDataManager.ShiftIndexes(OptionClauseBuilderDataManager.GetIndexList(OptionClauseList[clauseIdx].Continuation), nbShift, shiftStartIndex);
        }

        private void GenerateCouponFixings(CSxClauseBuilderAutocallData exoticMaskData,
                CSMOptionClauseBuilderUserData clauseBuilderUserData)
        {
            int minFixingType = exoticMaskData.BasicData.FixingType;
            Int32 couponFreq = exoticMaskData.CouponFrequency;
            CSMCalendar calendarForRoll = ClauseBuilderUtils.GetCalendar(exoticMaskData.ExoticMaskSchedulerData, ClauseBuilderUtils.eMAvailabilityType.M_iRolling);
            DateInterval couponInterval = ClauseBuilderUtils.GetDateIntervalFromInteger(couponFreq);
            int issueDate = exoticMaskData.BasicData.IssueDate;
            int endDate = exoticMaskData.BasicData.PeriodEndDate;
            int brokenDateRefIndex = exoticMaskData.ExoticMaskSchedulerData.BrokenDateRefIndex;
            long brokenDate = exoticMaskData.ExoticMaskSchedulerData.BrokenDate;
            eMHolidayAdjustmentType adjustmentType = (eMHolidayAdjustmentType)exoticMaskData.ExoticMaskSchedulerData.HolidayAdjustmentTypeIndex;

            // First regenerate coupon dates
            List<int> couponDateList = exoticMaskData.GeneratedDateList.Select(x => x.CouponLastDate).ToList();
            int couponDatesCount = couponDateList.Count;

            if (couponInterval.Multiplier > 0)
            {
                Int32 index = -1;
                foreach (var fixingClause in clauseBuilderUserData.FixingClauseList)
                {
                    index++;
                    if (index < 1)
                        continue;
                    fixingClause.Id = fixingClause.Id + couponDatesCount;
                }

                for (int i = 0; i < couponDatesCount; i++)
                {
                    List<int> allDateList = ClauseBuilderUtils.GenerateCouponDateList(issueDate,
                        couponDateList[i],
                        732, //1 day
                        calendarForRoll,
                        adjustmentType,
                        brokenDateRefIndex,
                        brokenDate);

                    foreach (var oneDate in allDateList.OrderByDescending(x => x))
                    {
                        CSMClauseBuilderFixingClauseData fixingClauseDate = new CSMClauseBuilderFixingClauseData { Id = i + 2, Type = "Min", Date = oneDate, FixingType = minFixingType };
                        if (!clauseBuilderUserData.FixingClauseList.Contains(fixingClauseDate))
                            clauseBuilderUserData.FixingClauseList.Insert(i + 1, fixingClauseDate);
                    }
                }
            }
        }

        private void GenerateCouponCluases(CSxClauseBuilderAutocallData exoticMaskData,
                CSMOptionClauseBuilderUserData clauseBuilderUserData)
        {
            // delete last exitup and continuation in case of KI
            if (exoticMaskData.KiIsChecked)
            {
                var lastExitup = clauseBuilderUserData.OptionClauseList.Where(x => x.Name == "ExitUp")
                    .LastOrDefault();
                if (clauseBuilderUserData.OptionClauseList.Contains(lastExitup))
                {
                    clauseBuilderUserData.OptionClauseList.Remove(lastExitup);
                    Int32 startIndex = lastExitup.Id - 1;
                    //for (int i = lastExitup.Id + 1; i < clauseBuilderUserData.OptionClauseList.Count+1; i++)
                    //{
                    //    ShiftIndexesForClause(i, -1, startIndex, clauseBuilderUserData.OptionClauseList);
                    //}
                    int index = 0;
                    foreach (CSMClauseBuilderOptionClauseData option in clauseBuilderUserData.OptionClauseList)
                    {
                        index++;
                        if (index < startIndex)
                            continue;
                        ShiftIndexesForClause(index, -1, startIndex, clauseBuilderUserData.OptionClauseList);
                    }
                }

                var lastContinuation = clauseBuilderUserData.OptionClauseList.Where(x => x.Name == "Continuation")
                    .LastOrDefault();
                if (clauseBuilderUserData.OptionClauseList.Contains(lastContinuation))
                {
                    clauseBuilderUserData.OptionClauseList.Remove(lastContinuation);
                    Int32 startIndex = lastContinuation.Id - 1;
                    //for (int i = lastContinuation.Id + 1; i < clauseBuilderUserData.OptionClauseList.Count + 1; i++)
                    //{
                    //    ShiftIndexesForClause(i, -1, startIndex, clauseBuilderUserData.OptionClauseList);
                    //}
                    int index = 0;
                    foreach (CSMClauseBuilderOptionClauseData option in clauseBuilderUserData.OptionClauseList)
                    {
                        index++;
                        if (index < startIndex)
                            continue;
                        ShiftIndexesForClause(index, -1, startIndex, clauseBuilderUserData.OptionClauseList);
                    }
                }
            }

            foreach (var clause in clauseBuilderUserData.OptionClauseList.ToList())
            {
                Int32 startIndex = clause.Id;
                if (clause.Name == "Nothing")
                {
                    CSMClauseBuilderOptionClauseData digitUp = new CSMClauseBuilderOptionClauseData()
                    {
                        Id = startIndex + 1,
                        Name = "DigitalUp",
                        Coupon = exoticMaskData.ConditionalCoupon,
                        Strike = exoticMaskData.ConditionalCouponLevel,
                        Factor = 1,
                        Underlying = clauseBuilderUserData.BasketCompositionList[0].Basket
                    };

                    clauseBuilderUserData.OptionClauseList.Insert(startIndex, digitUp);
                    Int32 index = -1;

                    foreach (CSMClauseBuilderOptionClauseData data in clauseBuilderUserData.OptionClauseList)
                    {
                        index++;
                        if (index < startIndex + 1)
                            continue;
                        data.Id = index + 1;
                        ShiftIndexesForClause(index, 1, startIndex, clauseBuilderUserData.OptionClauseList);
                    }
                }
                if (clause.Name == "ExitUp")
                {
                    CSMClauseBuilderOptionClauseData continuation = new CSMClauseBuilderOptionClauseData()
                    {
                        Id = startIndex + 1,
                        Name = "Continuation",
                        Coupon = 0,
                        Factor = 1,
                        Comments = "execution of both next clauses : checking autocallibility AND coupon"
                    };
                    CSMClauseBuilderOptionClauseData digitUp = new CSMClauseBuilderOptionClauseData()
                    {
                        Id = startIndex + 2,
                        Name = "DigitalUp",
                        Coupon = exoticMaskData.ConditionalCoupon,
                        Strike = exoticMaskData.ConditionalCouponLevel,
                        Factor = 1,
                        Underlying = clause.Underlying
                    };
                    continuation.Child = String.Format("{0},{1}", continuation.Id + 1, continuation.Id + 2);
                    clauseBuilderUserData.OptionClauseList.Insert(startIndex, continuation);
                    clauseBuilderUserData.OptionClauseList.Insert(startIndex + 1, digitUp);

                    Int32 index = -1;
                    foreach (CSMClauseBuilderOptionClauseData option in clauseBuilderUserData.OptionClauseList)
                    {
                        index++;
                        if (index < startIndex + 2)
                            continue;
                        option.Id = index + 1;
                        ShiftIndexesForClause(index, 2, startIndex, clauseBuilderUserData.OptionClauseList);
                    }
                }
            }
        }

        private void ModifyCouponClauses(CSxClauseBuilderAutocallData exoticMaskData,
            CSMOptionClauseBuilderUserData clauseBuilderUserData)
        {
            var minList = clauseBuilderUserData.FixingClauseList.Where(y => y.Type == "Min").ToList();
            var fixingList = clauseBuilderUserData.FixingClauseList.Where(x => x.Type == "Fixing").ToList();

            int digitalUpCount = 0;
            int exitUpCount = 0;

            foreach (var clauseData in clauseBuilderUserData.OptionClauseList.ToList())
            {
                if (clauseData.Name == "DigitalUp")
                {
                    clauseData.StartId = 1.ToString();
                    clauseData.EndIds = minList.ElementAt(digitalUpCount) != null ? minList[digitalUpCount].Id.ToString() : "";
                    CSxClauseBuilderAutocallData.GeneratedDates date = exoticMaskData.GeneratedDateList[digitalUpCount];
                    clauseData.PaymentDate = date.CouponPayDate;
                    clauseData.Strike = date.CouponLevel;
                    clauseData.Coupon = date.ConCoupon;
                    digitalUpCount++;
                }
                else if (clauseData.Name == "ExitUp")
                {
                    exitUpCount++;
                    /// TEST
                    //CSxClauseBuilderAutocallData.GeneratedDates date = exoticMaskData.GeneratedDateList[digitalUpCount];
                    clauseData.StartId = 1.ToString();
                    clauseData.EndIds = fixingList.ElementAt(exitUpCount) != null ? fixingList[exitUpCount].Id.ToString() : "";
                    //clauseData.PaymentDate = date.CouponPayDate;
                    //fixingList.ElementAt(exitUpCount) != null ? fixingList[exitUpCount].Date : 0;
                }
                else if (clauseData.Name == "Cliquet")
                {
                    clauseData.Floor = -10000000000.0; // Min value 
                    clauseData.Cap = -clauseData.Floor; // Max value
                    clauseData.EndIds = minList.Last() != null ? minList.Last().Id.ToString() : "";
                }
                else if (clauseData.Name == "Switch")
                {
                    clauseData.EndIds = minList.Last() != null ? minList.Last().Id.ToString() : "";
                }
            }
        }

        private void GenerateFixings(CSxClauseBuilderAutocallData exoticMaskData, CSMOptionClauseBuilderUserData clauseBuilderUserData)
        {
            int fixingType = exoticMaskData.BasicData.FixingType;

            if (exoticMaskData.GeneratedDateList.Count == 0)
                return;
            int issueDate = exoticMaskData.BasicData.IssueDate;
            int endDate = exoticMaskData.GeneratedDateList.Last().CouponDate;

            int fixingId = 1;
            //Add IssueDate As Fixing
            CSMClauseBuilderFixingClauseData firstfixingClauseDate = new CSMClauseBuilderFixingClauseData { Id = fixingId, Type = "Fixing", Date = issueDate, FixingType = fixingType };
            clauseBuilderUserData.FixingClauseList.Add(firstfixingClauseDate);

            fixingId++;

            exoticMaskData.fixingKIId = -1;
            exoticMaskData.MemoryFixingsMap = null;

            //Only for memory : add Fixing for each memory date
            if (exoticMaskData.IsMemoryCoupon)
            {
                exoticMaskData.MemoryFixingsMap = new Dictionary<int, int>();
                CSMCalendar calendarForRoll = ClauseBuilderUtils.GetCalendar(exoticMaskData.ExoticMaskSchedulerData, ClauseBuilderUtils.eMAvailabilityType.M_iRolling);
                Int32 memoryFreq = exoticMaskData.MemoryFrequency;
                List<int> memoryDates = new List<int>();
                for (int obsdateId = 0; obsdateId < exoticMaskData.GeneratedDateList.Count; obsdateId++)
                {
                    int memStartDate = 0 < obsdateId ? exoticMaskData.GeneratedDateList[obsdateId - 1].CouponDate : issueDate;
                    int memEndDate = exoticMaskData.GeneratedDateList[obsdateId].CouponDate;
                    int memDate = memEndDate;

                    if (sophisTools.CSMDay.IsARelativeDate(memoryFreq))
                    {
                        DateInterval memoryInterval = ClauseBuilderUtils.GetDateIntervalFromInteger(memoryFreq);
                        if (memoryInterval.Multiplier > 0)
                        {
                            int memCount = 0;
                            while (memStartDate < memDate)
                            {
                                memoryDates.Add(memDate);
                                memCount++;
                                memDate = ClauseBuilderUtils.ShiftDate(calendarForRoll, memEndDate, -memoryInterval.Multiplier, memoryInterval.Period, memCount,
                                    (eMHolidayAdjustmentType)exoticMaskData.ExoticMaskSchedulerData.HolidayAdjustmentTypeIndex);
                            }
                        }
                    }
                    else if (memoryFreq == -1 || memoryFreq == 0)//Final
                    {
                        memoryDates.Add(memEndDate);
                    }
                    else if (sophisTools.CSMDay.IsAFixedDate(memoryFreq) && memStartDate < memoryFreq && memoryFreq <= memEndDate)
                    {
                        memoryDates.Add(memoryFreq);
                    }
                }

                memoryDates.Sort();

                int lastPeriodEndfixing = exoticMaskData.GeneratedDateList.Last().CouponDate;
                exoticMaskData.LastPeriodEndfixingId = -1;
                foreach (var date in memoryDates)
                {
                    if (!clauseBuilderUserData.FixingClauseList.Any(f => f.Type == "Fixing" && f.Date == date))
                    {
                        clauseBuilderUserData.FixingClauseList.Add(new CSMClauseBuilderFixingClauseData
                        {
                            Id = fixingId,
                            Type = "Fixing",
                            Date = date,
                            FixingType = fixingType
                        });
                        if (lastPeriodEndfixing == date)
                            exoticMaskData.LastPeriodEndfixingId = fixingId;
                        exoticMaskData.MemoryFixingsMap.Add(date, fixingId);
                        fixingId++;
                    }
                }

                if (-1 == exoticMaskData.LastPeriodEndfixingId)
                {
                    clauseBuilderUserData.FixingClauseList.Add(new CSMClauseBuilderFixingClauseData
                    {
                        Id = fixingId,
                        Type = "Fixing",
                        Date = lastPeriodEndfixing,
                        FixingType = fixingType
                    });
                    exoticMaskData.LastPeriodEndfixingId = fixingId;
                    fixingId++;
                }
            }
            else
            {
                //Add all observation date as Fixing
                foreach (CSxClauseBuilderAutocallData.GeneratedDates date in exoticMaskData.GeneratedDateList)
                {
                    CSMClauseBuilderFixingClauseData fixingClauseDate = new CSMClauseBuilderFixingClauseData { Id = fixingId, Type = "Fixing", Date = date.CouponDate, FixingType = fixingType };
                    date.FixingClauseId = fixingClauseDate.Id;
                    clauseBuilderUserData.FixingClauseList.Add(fixingClauseDate);
                    fixingId++;
                }
                exoticMaskData.LastPeriodEndfixingId = fixingId - 1;
            }

            //Add Min Fixing for KI switch
            if (exoticMaskData.KiIsChecked == true)
            {
                int minFixingType = fixingType;
                if (exoticMaskData.KiIntraday)//if intraday, use Low as fixing type
                    minFixingType = TypeLow;
                // ClauseBuilderExoticMaskDataAutocall.GeneratedDates lastDate = exoticMaskData.GeneratedDateList.Last();

                Int32 kiFreq = exoticMaskData.KIFrequency;
                int minFixingDate;
                if (sophisTools.CSMDay.IsARelativeDate(kiFreq))
                {
                    CSMCalendar calendarForRoll = ClauseBuilderUtils.GetCalendar(exoticMaskData.ExoticMaskSchedulerData, ClauseBuilderUtils.eMAvailabilityType.M_iRolling);
                    minFixingDate = issueDate;
                    DateInterval couponInterval = ClauseBuilderUtils.GetDateIntervalFromInteger(kiFreq);
                    if (couponInterval.Multiplier > 0)
                    {
                        int minFixingCount = 0;
                        while (minFixingDate <= endDate)
                        {
                            CSMClauseBuilderFixingClauseData fixingClauseDate = new CSMClauseBuilderFixingClauseData { Id = fixingId, Type = "Min", Date = minFixingDate, FixingType = minFixingType };
                            exoticMaskData.fixingKIId = fixingClauseDate.Id;
                            if (!clauseBuilderUserData.FixingClauseList.Contains(fixingClauseDate))
                                clauseBuilderUserData.FixingClauseList.Add(fixingClauseDate);
                            minFixingCount++;
                            minFixingDate = ClauseBuilderUtils.ShiftDate(calendarForRoll, issueDate, couponInterval.Multiplier, couponInterval.Period,
                                minFixingCount, (eMHolidayAdjustmentType)exoticMaskData.ExoticMaskSchedulerData.HolidayAdjustmentTypeIndex);
                        }
                    }

                    if (clauseBuilderUserData.FixingClauseList.Last().Date < endDate)
                    {
                        minFixingDate = endDate;
                        CSMClauseBuilderFixingClauseData fixingClauseDate = new CSMClauseBuilderFixingClauseData { Id = fixingId, Type = "Min", Date = minFixingDate, FixingType = minFixingType };
                        exoticMaskData.fixingKIId = fixingClauseDate.Id;
                        if (!clauseBuilderUserData.FixingClauseList.Contains(fixingClauseDate))
                            clauseBuilderUserData.FixingClauseList.Add(fixingClauseDate);
                    }
                }
                else if (kiFreq == -1)//Final
                {
                    minFixingDate = endDate;
                    CSMClauseBuilderFixingClauseData fixingClauseDate = new CSMClauseBuilderFixingClauseData { Id = fixingId, Type = "Min", Date = minFixingDate, FixingType = minFixingType };
                    exoticMaskData.fixingKIId = fixingClauseDate.Id;
                    if (!clauseBuilderUserData.FixingClauseList.Contains(fixingClauseDate))
                        clauseBuilderUserData.FixingClauseList.Add(fixingClauseDate);
                }
                else if (sophisTools.CSMDay.IsAFixedDate(kiFreq))
                {
                    minFixingDate = kiFreq;
                    CSMClauseBuilderFixingClauseData fixingClauseDate = new CSMClauseBuilderFixingClauseData { Id = fixingId, Type = "Min", Date = minFixingDate, FixingType = minFixingType };
                    exoticMaskData.fixingKIId = fixingClauseDate.Id;
                    if (!clauseBuilderUserData.FixingClauseList.Contains(fixingClauseDate))
                        clauseBuilderUserData.FixingClauseList.Add(fixingClauseDate);
                }
            }
        }

        protected void GenerateClauses(CSxClauseBuilderAutocallData exoticMaskData, CSMOptionClauseBuilderUserData clauseBuilderUserData)
        {
            if (exoticMaskData.GeneratedDateList.Count == 0)
            {
                throw new System.Exception("Autocall periods is empty, no clause will be generated");
            }

            if (exoticMaskData.IsFixedCoupon)
                GenerateFixedCouponClauses(exoticMaskData, clauseBuilderUserData);
            else if (exoticMaskData.IsAccumulatedCoupon)
                GenerateAccumulatedCouponClauses(exoticMaskData, clauseBuilderUserData);
            else
                GenerateMemoryCouponClauses(exoticMaskData, clauseBuilderUserData);
        }

        private string GetFormatedString(double num, int dec)
        {
            return Math.Round(num, dec).ToString();
        }

        private void GenerateFixedCouponClauses(CSxClauseBuilderAutocallData exoticMaskData, CSMOptionClauseBuilderUserData clauseBuilderUserData)
        {
            bool isSwappedOption = exoticMaskData.BasicData.IsSwappedOption;

            int idClause = 1;

            int lastDateCount = exoticMaskData.GenerateLastKO ? exoticMaskData.GeneratedDateList.Count : exoticMaskData.GeneratedDateList.Count - 1;

            //1. Create CashFlow
            if (0 < lastDateCount)
            {
                clauseBuilderUserData.OptionClauseList.Add(new Sophis.Instrument.CSMClauseBuilderOptionClauseData
                {
                    Id = idClause,
                    Name = "Nothing" //,
                    //Coupon = 0
                });
                ++idClause;
            }

            //2.ExitUp  Clauses for all Dates including last one

            string lastUnderlying = clauseBuilderUserData.UnderlyingList.Last().LineId.ToString();
            if (clauseBuilderUserData.BasketCompositionList.Count > 0)
                lastUnderlying = clauseBuilderUserData.BasketCompositionList.Last().Basket;

            for (int i = 0; i < lastDateCount; i++)
            {
                int paymentDate = exoticMaskData.GeneratedDateList[i].CouponPayDate;

                CSxClauseBuilderAutocallData.GeneratedDates date = exoticMaskData.GeneratedDateList[i];

                if (clauseBuilderUserData.UnderlyingList.Count == 0)
                    continue;

                String truncatedCouponStr = GetFormatedString(date.AutocallCoupon * 100, 2);

                clauseBuilderUserData.OptionClauseList.Add(
                    new Sophis.Instrument.CSMClauseBuilderOptionClauseData
                    {
                        Id = idClause,
                        Name = "ExitUp",
                        Underlying = lastUnderlying,
                        PaymentDate = paymentDate,
                        Coupon = (isSwappedOption ? 0 : 1) + date.AutocallCoupon,
                        Strike = date.AutocallLevel,
                        StartId = "1",
                        EndIds = date.FixingClauseId.ToString(),
                        Comments = "Early redemption if perf > Autocall level with cash payment of 100%*Notional *1I(ELN) + " + truncatedCouponStr + "% * Notional, else go to the next early redemption date"
                        //Properties and Child updated later, see end of method
                    });

                idClause++;
            }

            //IF KI Option

            CSxClauseBuilderAutocallData.GeneratedDates lastDate = exoticMaskData.GeneratedDateList.Last();
            if (exoticMaskData.KiIsChecked == true)
            {
                //3. Add Switch Clause
                var KISwitchClause = new Sophis.Instrument.CSMClauseBuilderOptionClauseData
                {
                    Id = idClause,
                    Name = "Switch",
                    //Coupon = 0
                    PaymentDate = lastDate.CouponPayDate,
                    StartId = "1",
                    EndIds = (exoticMaskData.fixingKIId).ToString(), //"Min" Fixing of the KI Clause
                    Strike = exoticMaskData.KIBarrierLevel, // Strike = KI Level
                    Underlying = lastUnderlying,
                    Comments = "At Maturity, KI Switch is done"
                    //Child updated later, see end of method
                };
                clauseBuilderUserData.OptionClauseList.Add(KISwitchClause);
                idClause++;

                //4. Create "If KI" clause(s)
                AddSwitchChildClause(clauseBuilderUserData.OptionClauseList, ref idClause, exoticMaskData.PayoffTypeKI, exoticMaskData.FactorKI,
                    exoticMaskData.PerfCapKI, exoticMaskData.PerfFloorKI, exoticMaskData.fixingKIId, lastUnderlying, isSwappedOption, "If KI down crossed");
                //5. Create "If no KI" clause(s)
                int KISwitch2ndChildId = idClause;
                AddSwitchChildClause(clauseBuilderUserData.OptionClauseList, ref idClause, exoticMaskData.PayoffTypeNoKI, exoticMaskData.FactorNoKI,
                    exoticMaskData.PerfCapNoKI, exoticMaskData.PerfFloorNoKI, exoticMaskData.fixingKIId, lastUnderlying, isSwappedOption, "If KI not down crossed");

                KISwitchClause.Child = (KISwitchClause.Id + 1).ToString() + "," + KISwitch2ndChildId.ToString();
            }
            else //Add a CashFlow so that last ExitUp has 2 children
            {
                if (isSwappedOption)
                    clauseBuilderUserData.OptionClauseList.Add(new Sophis.Instrument.CSMClauseBuilderOptionClauseData
                    {
                        Id = idClause,
                        Name = "Nothing"
                    });
                else
                    clauseBuilderUserData.OptionClauseList.Add(new Sophis.Instrument.CSMClauseBuilderOptionClauseData
                    {
                        Id = idClause,
                        Name = "CashFlow",
                        Coupon = 1,
                        PaymentDate = lastDate.CouponPayDate,
                        Comments = "Final redemption."
                    });
                idClause++;
            }

            //6. Rate leg clause if swapped option
            if (isSwappedOption)
            {
                clauseBuilderUserData.OptionClauseList.Add(new Sophis.Instrument.CSMClauseBuilderOptionClauseData
                {
                    Id = idClause,
                    Name = "RateLeg",
                    Value = "Accrued",
                    Comments = "Depends on the ExitUp clause"
                });
                idClause++;
            }
            int rateLegClauseId = clauseBuilderUserData.OptionClauseList.Last().Id;

            //7. Update Children and Properties
            for (int i = 1; i < clauseBuilderUserData.OptionClauseList.Count - 1; i++)
            {
                if (clauseBuilderUserData.OptionClauseList[i].Name == "ExitUp")
                {
                    clauseBuilderUserData.OptionClauseList[i].Child = "1," + (clauseBuilderUserData.OptionClauseList[i].Id + 1).ToString();
                    if (isSwappedOption)
                        clauseBuilderUserData.OptionClauseList[i].Properties = rateLegClauseId.ToString();
                }
            }
        }

        private void GenerateAccumulatedCouponClauses(CSxClauseBuilderAutocallData exoticMaskData, CSMOptionClauseBuilderUserData clauseBuilderUserData)
        {
            bool isSwappedOption = exoticMaskData.BasicData.IsSwappedOption;

            //1. All observation dates except the last one
            int idClause = 1;
            int basketsCount = clauseBuilderUserData.BasketCompositionList.Count;
            //the first basket could have been created for commo (first Nearby baskets) 
            //last basket : basketCount - 1 -> basket standard
            //basketCount -2 && underlyingList count in wizard > 1 : basket of type specified in wizard
            string basketUser = clauseBuilderUserData.UnderlyingList.Last().LineId.ToString();
            if (exoticMaskData.UnderlyingList.Count > 1) //in this case there is at least two baskets.
                basketUser = clauseBuilderUserData.BasketCompositionList[basketsCount - 2].Basket;
            else if (clauseBuilderUserData.BasketCompositionList.Count > 0) //in this case there will be just one First Nearby basket in fact
                basketUser = clauseBuilderUserData.BasketCompositionList.Last().Basket;
            int lastTarnUpClauseId = -1;
            int lastDateCount = exoticMaskData.GenerateLastKO ? exoticMaskData.GeneratedDateList.Count : exoticMaskData.GeneratedDateList.Count - 1;
            for (int i = 0; i < lastDateCount; i++)
            {
                int paymentDate = exoticMaskData.GeneratedDateList[i].CouponPayDate;

                CSxClauseBuilderAutocallData.GeneratedDates date = exoticMaskData.GeneratedDateList[i];

                clauseBuilderUserData.OptionClauseList.Add(
                    new Sophis.Instrument.CSMClauseBuilderOptionClauseData
                    {
                        Id = idClause,
                        Name = "TarnUp",
                        Child = (idClause + 1).ToString(),
                        Continuation = (idClause + 2).ToString(),
                        PaymentDate = paymentDate,
                        Coupon = isSwappedOption ? 0 : 1,
                        Strike = date.AutocallLevel,
                        EndIds = isSwappedOption ? date.FixingClauseId.ToString() : "",
                        Comments = "Guaranteed notional of 100%*Notional *1I(ELN) paid only if early redemption"
                        //Properties updated later, see end of method
                    });

                lastTarnUpClauseId = idClause;
                idClause++;

                clauseBuilderUserData.OptionClauseList.Add(
                    new Sophis.Instrument.CSMClauseBuilderOptionClauseData
                    {
                        Id = idClause,
                        Name = "Cliquet",
                        Underlying = basketUser,
                        PaymentDate = paymentDate,
                        Coupon = 0,
                        Factor = 1,
                        Strike = 1,
                        Floor = exoticMaskData.KOPerfFloor,
                        Cap = exoticMaskData.KOPerfCap,
                        StartId = "1",
                        EndIds = date.FixingClauseId.ToString(),
                        Comments = "Early redemption if accumulated coupons > Autocall level with cash payment of Coupon (i) * Notional, else go to the next early redemption date"
                    });
                idClause++;
            }

            //2. Last Date
            CSxClauseBuilderAutocallData.GeneratedDates lastDate = exoticMaskData.GeneratedDateList.Last();
            int lastPaymentDate = lastDate.CouponPayDate;

            string basketStandard = clauseBuilderUserData.UnderlyingList.Last().LineId.ToString();
            if (clauseBuilderUserData.BasketCompositionList.Count > 0)
                basketStandard = clauseBuilderUserData.BasketCompositionList.Last().Basket;

            //if (!exoticMaskData.KiIsChecked)
            //{
            if (isSwappedOption || 0 == exoticMaskData.KOFactor)
                clauseBuilderUserData.OptionClauseList.Add(
                    new Sophis.Instrument.CSMClauseBuilderOptionClauseData
                    {
                        Id = idClause,
                        Name = "Nothing",
                        Comments = "At Maturity, if KO not crossed, then cash payment of 100%*Factor * Notional *1I(ELN)"
                    });
            else
                clauseBuilderUserData.OptionClauseList.Add(
                    new Sophis.Instrument.CSMClauseBuilderOptionClauseData
                    {
                        Id = idClause,
                        Name = "CashFlow",
                        PaymentDate = lastPaymentDate,
                        Coupon = exoticMaskData.KOFactor,
                        Comments = "At Maturity, if KO not crossed, then cash payment of 100%*Factor * Notional *1I(ELN)"
                    });

            idClause++;
           
            if (isSwappedOption)
            {
                clauseBuilderUserData.OptionClauseList.Add(new Sophis.Instrument.CSMClauseBuilderOptionClauseData
                {
                    Id = idClause,
                    Name = "RateLeg",
                    Value = "Accrued",
                    Comments = "Depends on the TarnUp clause"
                });
                int rateLegClauseId = idClause;
                idClause++;

                foreach (var clause in clauseBuilderUserData.OptionClauseList)
                {
                    if (clause.Name == "TarnUp")
                        clause.Properties = rateLegClauseId.ToString();
                }
            }
        }

        private void GenerateMemoryCouponClauses(CSxClauseBuilderAutocallData exoticMaskData, CSMOptionClauseBuilderUserData clauseBuilderUserData)
        {
            bool isSwappedOption = exoticMaskData.BasicData.IsSwappedOption;
            int idClause = 1;
            int lastDateCount = exoticMaskData.GenerateLastKO ? exoticMaskData.GeneratedDateList.Count : exoticMaskData.GeneratedDateList.Count - 1;

            //1. Create CashFlow
            if (0 < lastDateCount)
            {
                clauseBuilderUserData.OptionClauseList.Add(new Sophis.Instrument.CSMClauseBuilderOptionClauseData
                {
                    Id = idClause,
                    Name = "Nothing" //,
                });
                ++idClause;
            }

            //2.ExitUp On Components Clauses for all Dates including last one
            string lastUnderlying = clauseBuilderUserData.UnderlyingList.Last().LineId.ToString();
            if (clauseBuilderUserData.BasketCompositionList.Count > 0)
                lastUnderlying = clauseBuilderUserData.BasketCompositionList.Last().Basket;


            for (int i = 0; i < lastDateCount; i++)
            {
                int paymentDate = exoticMaskData.GeneratedDateList[i].CouponPayDate;

                CSxClauseBuilderAutocallData.GeneratedDates date = exoticMaskData.GeneratedDateList[i];

                if (clauseBuilderUserData.UnderlyingList.Count == 0)
                    continue;

                string endIds = "";
                int memStartDate = exoticMaskData.BasicData.IssueDate; //  i > 0 ? exoticMaskData.GeneratedDateList[i - 1].CouponDate : exoticMaskData.BasicData.IssueDate;
                int memEndDate = exoticMaskData.GeneratedDateList[i].CouponDate;
                foreach (KeyValuePair<int, int> memoryFixing in exoticMaskData.MemoryFixingsMap)
                {
                    int obsDate = memoryFixing.Key;
                    if (obsDate > memStartDate &&
                        obsDate <= memEndDate)
                    {
                        endIds += memoryFixing.Value.ToString() + ",";
                    }
                }

                if (endIds == "")
                    continue;

                endIds = endIds.TrimEnd(',');

                clauseBuilderUserData.OptionClauseList.Add(
                    new Sophis.Instrument.CSMClauseBuilderOptionClauseData
                    {
                        Id = idClause,
                        Name = "ExitUpOnComponents",
                        Value = exoticMaskData.UnderlyingList.Count.ToString(),
                        Underlying = lastUnderlying,
                        PaymentDate = paymentDate,
                        Coupon = (isSwappedOption ? 0 : 1) + date.AutocallCoupon,
                        Strike = date.AutocallLevel,
                        StartId = "1",
                        EndIds = endIds,
                        Comments = "Early redemption if perf > Autocall level on any date defined at the End Fixing Date with cash payment of 100%*Notional *1I(ELN) + Memory Coupon (i) * Notional, else go to the next early redemption date"
                        //Properties and Child updated later, see end of method
                    });

                idClause++;
            }



            CSxClauseBuilderAutocallData.GeneratedDates lastDate = exoticMaskData.GeneratedDateList.Last();

            //IF KI Option
            if (exoticMaskData.KiIsChecked == true)
            {
                //3. Add Switch Clause
                var KISwitchClause = new Sophis.Instrument.CSMClauseBuilderOptionClauseData
                {
                    Id = idClause,
                    Name = "Switch",
                    //Coupon = 0
                    Underlying = lastUnderlying,
                    PaymentDate = lastDate.CouponPayDate,
                    Coupon = 0,
                    Factor = 1,
                    Strike = exoticMaskData.KIBarrierLevel,
                    StartId = "1",
                    EndIds = (exoticMaskData.fixingKIId).ToString(), //"Min" Fixing of the KI Clause
                    Comments = "At Maturity, KI Switch is done"
                    //Child updated later, see end of method
                };
                clauseBuilderUserData.OptionClauseList.Add(KISwitchClause);
                idClause++;

                //4. Create "If KI" clause(s)
                AddSwitchChildClause(clauseBuilderUserData.OptionClauseList, ref idClause, exoticMaskData.PayoffTypeKI, exoticMaskData.FactorKI,
                    exoticMaskData.PerfCapKI, exoticMaskData.PerfFloorKI, exoticMaskData.LastPeriodEndfixingId, lastUnderlying, isSwappedOption, "If KI down crossed");
                //5. Create "If no KI" clause(s)
                int KISwitch2ndChildId = idClause;
                AddSwitchChildClause(clauseBuilderUserData.OptionClauseList, ref idClause, exoticMaskData.PayoffTypeNoKI, exoticMaskData.FactorNoKI,
                    exoticMaskData.PerfCapNoKI, exoticMaskData.PerfFloorNoKI, exoticMaskData.LastPeriodEndfixingId, lastUnderlying, isSwappedOption, "If KI not down crossed");

                KISwitchClause.Child = (KISwitchClause.Id + 1).ToString() + "," + KISwitch2ndChildId.ToString();
            }
            else //Add a CashFlow so that last ExitUpOnComponents has 2 children
            {
                if (isSwappedOption)
                    clauseBuilderUserData.OptionClauseList.Add(new Sophis.Instrument.CSMClauseBuilderOptionClauseData
                    {
                        Id = idClause,
                        Name = "Nothing"
                    });
                else
                    clauseBuilderUserData.OptionClauseList.Add(new Sophis.Instrument.CSMClauseBuilderOptionClauseData
                    {
                        Id = idClause,
                        Name = "CashFlow",
                        Coupon = 1,
                        PaymentDate = lastDate.CouponPayDate,
                        Comments = "Final redemption."
                    });
                idClause++;
            }

            //6. Rate leg clause if swapped option
            if (isSwappedOption)
            {
                clauseBuilderUserData.OptionClauseList.Add(new Sophis.Instrument.CSMClauseBuilderOptionClauseData
                {
                    Id = idClause,
                    Name = "RateLeg",
                    Value = "Accrued",
                    Underlying = lastUnderlying,
                    PaymentDate = lastDate.CouponPayDate,
                    Comments = "Depends on the ExitUp On Component clause"
                });
                idClause++;
            }
            int rateLegClauseId = clauseBuilderUserData.OptionClauseList.Last().Id;

            //7. Update Children and Properties
            for (int i = 1; i < clauseBuilderUserData.OptionClauseList.Count - 1; i++)
            {
                if (clauseBuilderUserData.OptionClauseList[i].Name == "ExitUpOnComponents")
                {
                    clauseBuilderUserData.OptionClauseList[i].Child = "1," + (clauseBuilderUserData.OptionClauseList[i].Id + 1).ToString();
                    if (isSwappedOption)
                        clauseBuilderUserData.OptionClauseList[i].Properties = rateLegClauseId.ToString();
                }
            }
        }

        protected void AddSwitchChildClause(CustomBindingList<CSMClauseBuilderOptionClauseData> list, ref int idClause, string payoffType, double factor,
            double perfCap, double perfFloor, int lastFixingClauseId, string underlying, bool isSwappedOption, string commentBegin)
        {
            switch (payoffType)
            {
                case ClauseBuilderExoticMaskDataAutocall.PayoffTypeFixedStr:
                    list.Add(new Sophis.Instrument.CSMClauseBuilderOptionClauseData
                    {
                        Id = idClause,
                        Name = "CashFlow",
                        Coupon = factor,
                        Comments = commentBegin + " and fixed coupon payoff, then cash payment of Factor * Notional"
                    });
                    idClause++;
                    break;
                case ClauseBuilderExoticMaskDataAutocall.PayoffTypePerfIndexedStr:
                    list.Add(new Sophis.Instrument.CSMClauseBuilderOptionClauseData
                    {
                        Id = idClause,
                        Name = "Cliquet",
                        Underlying = underlying,
                        Coupon = 0,
                        Factor = factor,
                        Strike = isSwappedOption ? 1 : 0,
                        Floor = perfFloor,
                        Cap = perfCap,
                        StartId = "1",
                        EndIds = lastFixingClauseId.ToString(),
                        Comments = commentBegin + " and perf-indexed payoff, then cash payment of 100% * Factor * Notional * (Performance(T) (floored, capped) - 100% * 1I(ELS))"
                    });
                    idClause++;
                    break;
                case ClauseBuilderExoticMaskDataAutocall.PayoffTypeAveragePerfStr:
                    list.Add(new Sophis.Instrument.CSMClauseBuilderOptionClauseData
                    {
                        Id = idClause,
                        Name = "Cliquet",
                        Underlying = underlying,
                        Coupon = isSwappedOption ? 0 : factor,
                        Factor = 1,
                        Strike = 1,
                        Floor = perfFloor,
                        Cap = perfCap,
                        StartId = "1",
                        EndIds = lastFixingClauseId.ToString(),
                        Comments = commentBegin + " and average perf payoff, then cash payment of 100%*Factor * Notional *1I(ELN) + (Average Performance (T) (floored, capped) -100%)* Notional"
                    });
                    idClause++;
                    break;
                case ClauseBuilderExoticMaskDataAutocall.PayoffTypeAbsPerfStr:
                    list.Add(new Sophis.Instrument.CSMClauseBuilderOptionClauseData
                    {
                        Id = idClause,
                        Name = "Product",
                        Child = (idClause + 1).ToString() + "," + (idClause + 2).ToString(),
                        Coupon = isSwappedOption ? 0 : 1,
                        Factor = 1,
                        Strike = 0,
                        Floor = MinConstant,
                        Cap = MaxConstant,
                        Comments = commentBegin + " and absolute perf payoff, then cash payment of  100%*Notional *1I(ELN) + Factor * Abs( Performance (T) (floored, capped) - 1) * Notional"
                    });
                    int clauseProductId = idClause;
                    idClause++;
                    list.Add(new Sophis.Instrument.CSMClauseBuilderOptionClauseData
                    {
                        Id = idClause,
                        Name = "Constant",
                        Value = GetFormatedString(factor, CSMConstants.UsefullDecimalCount)
                    });
                    idClause++;
                    list.Add(new Sophis.Instrument.CSMClauseBuilderOptionClauseData
                    {
                        Id = idClause,
                        Name = "Abs",
                        Child = (idClause + 1).ToString(),
                        Coupon = 0,
                        Factor = 1,
                        Strike = 0,
                        Floor = MinConstant,
                        Cap = MaxConstant
                    });
                    int clauseAbsId = idClause;
                    idClause++;
                    list.Add(new Sophis.Instrument.CSMClauseBuilderOptionClauseData
                    {
                        Id = idClause,
                        Name = "Cliquet",
                        Underlying = underlying,
                        Coupon = 0,
                        Factor = 1,
                        Strike = 1,
                        Floor = perfFloor,
                        Cap = perfCap,
                        StartId = "1",
                        EndIds = lastFixingClauseId.ToString()
                    });
                    idClause++;
                    break;
                default:
                    break;
            }
        }

        private void AddBasket(int id, string components, string basketType, CSMOptionClauseBuilderUserData clauseBuilderUserData)
        {
            CSMClauseBuilderBasketCompositionData basket = new CSMClauseBuilderBasketCompositionData
            {
                Basket = (Convert.ToChar(65 + id)).ToString(),
                Type = basketType,
                Components = components,
                BasketQuantoType = (int)eMQuantoType.M_qIsAQuanto
            };
            clauseBuilderUserData.BasketCompositionList.Add(basket);
            ClauseBuilderUtils.InitBasketClauseData(clauseBuilderUserData.UnderlyingList, clauseBuilderUserData.BasketCompositionList,
                clauseBuilderUserData.BasketClauseList, basket);
        }

    }
}
