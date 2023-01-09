#pragma once

/*
** Includes
*/
#include "SphInc/backoffice_kernel/SphBrokerFees.h"

class CSxBrokerFeesIsException : public sophis::backoffice_kernel::CSRBrokerFeesConditionTransaction
{
public:

	DECLARATION_BROKERFEES_RULES_CONDITION_TRANSACTION(CSxBrokerFeesIsException);
	virtual bool get_condition(const sophis::portfolio::CSRTransaction & trade) const;

private:
	static const char* __CLASS__;
};

long GetExceptionSectorID();