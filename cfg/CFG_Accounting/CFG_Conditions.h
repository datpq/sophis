#ifndef __CFG_Conditions__H__
	#define __CFG_Conditions__H__

#include "SphInc/accounting/SphRulesCondition.h"
#include "SphInc/portfolio/SphTransaction.h"
#include "SphInc/portfolio/SphPosition.h"
#include "SphInc/value/accounting/SphAmAccFundRulesCondition.h"
#include <stdio.h>

/**
Macros
*/
#ifdef CFG_ACCOUNTING_EXPORT
#	define CFG_ACCOUNTING __declspec(dllexport)
#else
#	define CFG_ACCOUNTING __declspec(dllimport)
#endif

class CFG_ACCOUNTING CFG_AccountingConditionsMgr
{
public:
	static void GetBondCriteria(const sophis::portfolio::CSRTransaction& trade, bool& isABond, bool& isAmortissable, bool& isInter);
private:
	static const char * __CLASS__;
};

class CFG_RulesConditionTradeIsPartialRedemption : public sophis::accounting::CSRRulesConditionTransaction
{
public:
	virtual bool get_condition( const sophis::portfolio::CSRTransaction& trade ) const;

private:
	DECLARATION_RULES_CONDITION_TRANSACTION(CFG_RulesConditionTradeIsPartialRedemption);
	
	static const char * __CLASS__;
};

class CFG_RulesConditionTradeIsFinalRedemption : public sophis::accounting::CSRRulesConditionTransaction
{
public:
	virtual bool get_condition( const sophis::portfolio::CSRTransaction& trade ) const;

private:
	DECLARATION_RULES_CONDITION_TRANSACTION(CFG_RulesConditionTradeIsFinalRedemption);
	
	static const char * __CLASS__;
};


class CFG_RulesConditionTradeWithAllot : public sophis::accounting::CSRRulesConditionTransaction
{
public:
	void set_allotment(_STL::string allotment);
	virtual bool get_condition( const sophis::portfolio::CSRTransaction& trade ) const;

private:
	DECLARATION_RULES_CONDITION_TRANSACTION(CFG_RulesConditionTradeWithAllot);
	_STL::string m_allotment;
	static const char * __CLASS__;
};

class CFG_RulesConditionPnLWithAllot : public sophis::accounting::CSRRulesConditionPosition
{
public:
	void set_allotment(_STL::string allotment);
	virtual bool get_condition( const portfolio::CSRPosition& position ) const;

private:
	DECLARATION_RULES_CONDITION_POSITION(CFG_RulesConditionPnLWithAllot);
	_STL::string m_allotment;
	static const char * __CLASS__;
};


class CFGIsSLRepoLeg : public sophis::accounting::CSRRulesConditionPosition
{
public:
	virtual bool get_condition( const sophis::portfolio::CSRPosition& position ) const;

private:
	DECLARATION_RULES_CONDITION_POSITION(CFGIsSLRepoLeg);
	static const char * __CLASS__;
};


class CFGIsNotSLRepoLeg : public sophis::accounting::CSRRulesConditionPosition
{
public:
	virtual bool get_condition( const sophis::portfolio::CSRPosition& position ) const;

private:
	DECLARATION_RULES_CONDITION_POSITION(CFGIsNotSLRepoLeg);
	static const char * __CLASS__;
};

class CFGIsDATOver2years : public sophis::accounting::CSRRulesConditionPosition
{
public:
	virtual bool get_condition( const sophis::portfolio::CSRPosition& position ) const;

private:
	DECLARATION_RULES_CONDITION_POSITION(CFGIsDATOver2years);
	static const char * __CLASS__;
};

class CFGIsDATLess2years : public sophis::accounting::CSRRulesConditionPosition
{
public:
	virtual bool get_condition( const sophis::portfolio::CSRPosition& position ) const;

private:
	DECLARATION_RULES_CONDITION_POSITION(CFGIsDATLess2years);
	static const char * __CLASS__;
};

class CFGIsDATOver2yearsTrade : public sophis::accounting::CSRRulesConditionTransaction
{
public:
	virtual bool get_condition( const portfolio::CSRTransaction& trade ) const;

private:
	DECLARATION_RULES_CONDITION_TRANSACTION(CFGIsDATOver2yearsTrade);
	static const char * __CLASS__;
};

class CFGIsDATLess2yearsTrade : public sophis::accounting::CSRRulesConditionTransaction
{
public:
	virtual bool get_condition( const portfolio::CSRTransaction& trade ) const;

private:
	DECLARATION_RULES_CONDITION_TRANSACTION(CFGIsDATLess2yearsTrade);
	static const char * __CLASS__;
};


class CFGBOEventAccRule : public sophis::accounting::CSRRulesConditionTransaction
{
public:
	virtual bool get_condition( const sophis::portfolio::CSRTransaction& trade ) const;

private:
	DECLARATION_RULES_CONDITION_TRANSACTION(CFGBOEventAccRule);
	static const char * __CLASS__;
};

class CFGBOEventAccRule2 : public sophis::accounting::CSRRulesConditionTransaction
{
public:
	virtual bool get_condition( const sophis::portfolio::CSRTransaction& trade ) const;

private:
	DECLARATION_RULES_CONDITION_TRANSACTION(CFGBOEventAccRule2);
	static const char * __CLASS__;
};

class CFGIsARedemption : public sophis::accounting::CSAmAccSrRulesCondition
{
public:
	virtual bool GetCondition( const sophis::value::CSAMFundSR& sr) const;

private:
	DECLARATION_RULES_CONDITION_SR(CFGIsARedemption);
	static const char * __CLASS__;
};


#endif //!__CFG_Conditions_H__