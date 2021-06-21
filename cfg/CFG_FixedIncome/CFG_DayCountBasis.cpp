#include "CFG_DayCountBasis.h"
#include "SphTools/SphDay.h"

using namespace sophis::static_data;
using namespace sophis::instrument;

//------------------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_DAY_COUNT_BASIS(CFG_ActActCouponDayCountBasis);

double CFG_ActActCouponDayCountBasis::GetEquivalentYearCount(long startDate, long endDate, const SSDayCountCalculation& dccData) const
{
	CSRDay tempDay = endDate;
	tempDay.fYear--;
	long yearPrec = tempDay;
	if(yearPrec>startDate)
		return 1 + GetEquivalentYearCount(startDate,yearPrec,dccData);
	double yearlyDays = (double) (endDate - yearPrec);
	double val = (double)(endDate - startDate) / yearlyDays;
	return val;
};

int CFG_ActActCouponDayCountBasis::GetEquivalentDayCount(long startDate, long endDate, const SSDayCountCalculation& dccData) const
{
	int nbDays = (int) (endDate - startDate);
	return nbDays;
};
