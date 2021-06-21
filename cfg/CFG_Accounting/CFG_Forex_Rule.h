#ifndef __CFG_Forex_Rule_H__
	#define __CFG_Forex_Rule_H__
/*
** Includes
*/
#include "SphBoCommon/SphBoSTL.h"
#include "SphInc/SphMacros.h"
#include "SphInc/accounting/SphChartOfAccount.h"
#include "SphLLInc\Accounting\Processing\OverLoadForexRule.h"
#include "SphInc/Value/accounting/SphAmAccForexRule.h"

//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////
class CFG_Forex_Rule : /*Risque Part*/ public sophis::accounting::CSRDefaultForexRule , /*Value Part*/ public sophis::accounting::CSAmAccForexRule
{
public:
	DECLARATION_FOREX_RULE(CFG_Forex_Rule);
	//////////////////////////////////////////////////////////////////////////
	// RISQUE PART
	//////////////////////////////////////////////////////////////////////////
	virtual double get_amount(	double	amount,
		long	currency_amount,
		long	new_currency,
		const	portfolio::CSRTransaction& trade,
		bool	&notYetFixed,
		long	date) const;

	virtual double get_amount(	double	amount,
		long	currency_amount,
		long	new_currency,
		const portfolio::CSRPosition& position	) const ;

	//////////////////////////////////////////////////////////////////////////
	// VALUE PART
	//////////////////////////////////////////////////////////////////////////
	/** Method called by the S/R engine to convert an amount.
	@param amount is the original amount 
	@param currency_amount is the currency of the original amount 
	@param new_currency is the currency to convert the original amount.
	@param theSR is the original S/R
	*/
	virtual double get_amount(	double	amount,
		long	currency_amount,
		long	new_currency,
		const	sophis::value::CSAMFundSR& theSR,
		bool	&notYetFixed,
		long	date) const;

	/** Method called by the account interest engine to convert an amount.
	@param amount is the original amount 
	@param currency_amount is the currency of the original amount 
	@param new_currency is the currency to convert the original amount.
	@param interest is the original account interest
	*/
	virtual double get_amount(	double	amount,
		long	currency_amount,
		long	new_currency,
		const	sophis::value::SSAMAccountInterest& interest,
		bool	&notYetFixed,
		long	date) const;

	/** Method called by the cash transfer engine to convert an amount.
	@param amount is the original amount 
	@param currency_amount is the currency of the original amount 
	@param new_currency is the currency to convert the original amount.
	@param cashTransfer is the original cash transfer
	*/
	virtual double get_amount(	double	amount,
		long	currency_amount,
		long	new_currency,
		const	sophis::value::SSAMCashTransferInfos& cashTransfer,
		bool	&notYetFixed,
		long	date) const;

	/** Return the 'Default' forex rule for Risque
	@return the default forex rule and throw an exception is the rule does not exist
	@remark You need to delete the pointer after use (created by a clone)
	*/
	const sophis::accounting::CSRForexRule * GetRisqueDefaultForexRule() const;

	/** Return the 'Default' forex rule for Value
	@return the default forex rule and throw an exception is the rule does not exist
	@remark You need to delete the pointer after use (created by a clone)
	*/
	const sophis::accounting::CSAmAccForexRule * GetValueDefaultForexRule() const;

private:
	/**
	Logger data
	*/
	static const char * __CLASS__;

	static const sophis::accounting::CSRForexRule* pRisqueForexRule;
	static const sophis::accounting::CSAmAccForexRule* pValueForexRule;
};

#endif