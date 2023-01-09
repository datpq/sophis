#pragma once
#ifndef __CSxBrokerFeesCtryInc_H__
#define __CSxBrokerFeesCtryInc_H__

/*
** Includes
*/
#include "SphInc/backoffice_kernel/SphBrokerFees.h"

#define	My_INITIALISE_BROKERFEES_RULES_CONDITION_TRANSACTION_CTRYOFINC(classkey, name) \
	INITIALISE_BROKERFEES_RULES_CONDITION_TRANSACTION(CSxBrokerFeesCtryInc, name) \
	CSxBFCtryIncMapping::MyDict[classkey] = name;

class CSxBFCtryIncMapping
{
public:
	CSxBFCtryIncMapping() {};
	static _STL::map<int, _STL::string> MyDict;
};

struct SSxCtryInc
{
	char fName[100];
	SSxCtryInc()
	{
		fName[0] = '\0';
	};
};


long GetCntryIncorpSectorID();


class CSxBrokerFeesCtryInc : public sophis::backoffice_kernel::CSRBrokerFeesConditionTransaction
{
	//------------------------------------------- PUBLIC -------------------------------------------
public:
	DECLARATION_BROKERFEES_RULES_CONDITION_TRANSACTION(CSxBrokerFeesCtryInc);

	CSxBrokerFeesCtryInc::CSxBrokerFeesCtryInc()
	{
		classKey = CSxBFCtryIncMapping::MyDict.size();
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

#endif //__CSxBrokerFeesCtryInc_H__