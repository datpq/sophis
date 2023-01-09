#pragma once
#ifndef __CSxBrokerFeesConditionSEDOL_H__
#define __CSxBrokerFeesConditionSEDOL_H__

/*
** Includes
*/
#include "SphInc/backoffice_kernel/SphBrokerFees.h"


class CSxBrokerFeesConditionSEDOL : public sophis::backoffice_kernel::CSRBrokerFeesConditionTransaction
{
public:

	DECLARATION_BROKERFEES_RULES_CONDITION_TRANSACTION(CSxBrokerFeesConditionSEDOL);

	virtual bool get_condition(const sophis::portfolio::CSRTransaction & trade) const;

private:

	static const char* __CLASS__;
};


#endif //!__CSxBrokerFeesConditionSEDOL_H__
