#pragma once
#ifndef __CSxBrokerFeesCtryCode_H__
#define __CSxBrokerFeesCtryCode_H__

/*
** Includes
*/
#include "SphInc/backoffice_kernel/SphBrokerFees.h"


#define	My_INITIALISE_BROKERFEES_RULES_CONDITION_TRANSACTION_CTRYCODE(classkey, name) \
	INITIALISE_BROKERFEES_RULES_CONDITION_TRANSACTION(CSxBrokerFeesCtryCode, name) \
	CSxBFCtryCodeMapping::MyDict[classkey] = name;

class CSxBFCtryCodeMapping
{
public:
	CSxBFCtryCodeMapping() {};
	static _STL::map<int, _STL::string> MyDict;
};



long GetCntryCodeSectorID();

class CSxBrokerFeesCtryCode : public sophis::backoffice_kernel::CSRBrokerFeesConditionTransaction
{
	//------------------------------------------- PUBLIC -------------------------------------------
public:
	DECLARATION_BROKERFEES_RULES_CONDITION_TRANSACTION(CSxBrokerFeesCtryCode);

	CSxBrokerFeesCtryCode::CSxBrokerFeesCtryCode()
	{
		classKey = CSxBFCtryCodeMapping::MyDict.size();
	}

	/*
	** Test if Broker Fees has to be applied.
	*/
	virtual bool get_condition(const sophis::portfolio::CSRTransaction & trade) const;

	//------------------------------------------- PROTECTED -------------------------------------------
protected:

	//------------------------------------------- PRIVATE -------------------------------------------
private:
	/*
	** Logger data
	*/
	int classKey;
	static const char * __CLASS__;

};

#endif //__CSxBrokerFeesCtryCode_H__