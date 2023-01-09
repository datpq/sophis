#pragma once

/*
** Includes
*/
#include "SphInc/backoffice_kernel/SphBrokerFees.h"

class CSxBrokerFeesIsMAML : public sophis::backoffice_kernel::CSRBrokerFeesConditionTransaction
{
public:
	
	DECLARATION_BROKERFEES_RULES_CONDITION_TRANSACTION(CSxBrokerFeesIsMAML);
	virtual bool get_condition(const sophis::portfolio::CSRTransaction & trade) const;

private:
	static const char* __CLASS__;
};
