#pragma once

#ifndef __ToDoIfTradeInstrumentQuotation__H__
	#define __ToDoIfTradeInstrumentQuotation__H__

/*
** Includes
*/

#include "SphInc/backoffice_kernel/SphPostingToDoIf.h"
#include "SphInc/portfolio/SphPosition.h"

namespace eff {
	namespace kamco {
		namespace accounting {

/** Interface to create a to do if condition in a p&l rule.
In the p&l engine, define a condition to apply simple rules.
The difference with the {@link CSRRulesConditionPosition} is that the condition applies to
a rule as a group of posting to select the right rule to apply. That one applies to a single posting
once the rule is selected to know if that amount has to be posted.
You can implement this interface to add a to do if on the list.
The p&l engine, when finding a rule with an amount, will call
the method to_do to know if the amount has to be posted once the amount and currency is calculated.
The balance engine will call the same method for each account to transfer when the balance is calculated.
@version 4.5.0 Used also for balance engine for forex balance engine.
*/
class ToDoIfTradeInstrumentIsQuoted : public sophis::backoffice_kernel::CSRPostingToDoIf
{
//------------------------------------ PUBLIC ------------------------------------
public:

	DECLARATION_POSTING_TO_DO_IF(ToDoIfTradeInstrumentIsQuoted);

	/** filter to decide if ToDoIf posting should be applied to position
	@param position is position to analyze
	@param amount is amount of posting
	@param posting_currency is currency of amount
	@param accounting_currency is currency of accounting entity
	@param amount_currency is currency 'Currency Type' column in parameterization screen
	@returns true if posting has to be applied, otherwise - false
	*/
	virtual bool to_do( const portfolio::CSRTransaction& trade,
						double amount, long posting_currency,
						long accounting_currency, long amount_currency) const /*= 0 */override;
private:
	static const char* __CLASS__;
};

class ToDoIfTradeInstrumentIsUnquoted : public sophis::backoffice_kernel::CSRPostingToDoIf
{
public:
	DECLARATION_POSTING_TO_DO_IF(ToDoIfTradeInstrumentIsUnquoted);
	virtual bool to_do(const portfolio::CSRTransaction& trade,
		double amount, long posting_currency,
		long accounting_currency, long amount_currency) const /*= 0 */override;
private:
	static const char* __CLASS__;

};

class ToDoIfPositionInstrumentIsQuoted : public sophis::backoffice_kernel::CSRPostingToDoIfPosition
{
public:
	DECLARATION_POSTING_TO_DO_IF_POSITION(ToDoIfPositionInstrumentIsQuoted);
	virtual bool to_do(const portfolio::CSRPosition& position,
		double amount, long posting_currency,
		long accounting_currency, long amount_currency) const /*= 0 */override;
private:
	static const char* __CLASS__;
};

class ToDoIfPositionInstrumentIsUnquoted : public sophis::backoffice_kernel::CSRPostingToDoIfPosition
{
public:
	DECLARATION_POSTING_TO_DO_IF_POSITION(ToDoIfPositionInstrumentIsUnquoted);
	virtual bool to_do(const portfolio::CSRPosition& position,
		double amount, long posting_currency,
		long accounting_currency, long amount_currency) const /*= 0 */override;
private:
	static const char* __CLASS__;
};

		}
	}
}
#endif //!__ToDoIfTradeInstrumentQuotation__H__
