#ifndef __CFG_RevisableBondAction_H__
#define __CFG_RevisableBondAction_H__

#include "SphInc/instrument/SphInstrumentAction.h"
#include "SphInc/instrument/SphBond.h"

class CFG_RevisableBondAction : public sophis::instrument::CSRInstrumentAction
{
public:
	DECLARATION_INSTRUMENT_ACTION(CFG_RevisableBondAction);
	virtual void VoteForCreation(CSRInstrument &instrument)
		throw (tools::VoteException , sophisTools::base::ExceptionBase);
	virtual void VoteForModification(CSRInstrument &instrument, NSREnums::eParameterModificationType type)
		throw (tools::VoteException, sophisTools::base::ExceptionBase);

	void ModifyRedemptions(CSRBond* pBond);
	double GetUnitCouponValue(const CSRBond* pBond, const SSRedemption& r) const;
};

#endif //!CFG_RevisableBondAction_H__