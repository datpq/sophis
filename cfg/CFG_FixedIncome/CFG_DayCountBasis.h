#ifndef __CFG_DayCountBasis_H__
#define __CFG_DayCountBasis_H__

#include "SphInc/instrument/SphInstrumentEnums.h"
#include "SphInc/static_data/SphDayCountBasis.h"

class CFG_ActActCouponDayCountBasis : public sophis::static_data::CSRDayCountBasis
{
public:
	DECLARATION_DAY_COUNT_BASIS(CFG_ActActCouponDayCountBasis);

	virtual double GetEquivalentYearCount(long startDate, long endDate, const sophis::static_data::SSDayCountCalculation& dccData) const;
	virtual int GetEquivalentDayCount(long startDate, long endDate, const sophis::static_data::SSDayCountCalculation& dccData) const;
};

#endif //!__BMCE_DayCountBasis_H__