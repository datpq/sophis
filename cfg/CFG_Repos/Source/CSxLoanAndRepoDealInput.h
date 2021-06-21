#ifndef __CSxLoanAndRepoDealInput_H__
#define __CSxLoanAndRepoDealInput_H__

/*
** Includes
*/
#include "SphInc/collateral/SphLoanAndRepoDialog.h"
#include "SphInc/portfolio/SphPaymentMthdGUIOverloader.h"
#include "../Resource/resource.h"


class CSxLoanAndRepoDealInput : public sophis::collateral::CSRLoanAndRepoDialog
{
	//------------------------------------ PUBLIC ------------------------------------
public:

	virtual void	OpenAfterInit(void);


	/**Validates an element in the context.
	When selection moves from one element of the dialog to another one, the method CSRElement::Validation()
	of the current element is called and (if the call ends successfully) so is CSRFitDialog::ElementValidation().
	The method is overridden in order to :
	- check the coherence of the current setting,
	- take an appropriate action with respect to the current context.
	@param EAId_Modified is the absolute ID of the modified element
	@version 4.5.2
	*/
	virtual	void	ElementValidation(int EAId_Modified);

	/** Called before the transaction is created.
	@.
	*/
	virtual void OnCreateTransaction(portfolio::CSRTransaction& transaction, sophis::tools::CSREventVector &messages);

	/** Called before the transaction is modified.	
	*/
	virtual void OnModifyTransaction(portfolio::CSRTransaction& transaction, sophis::tools::CSREventVector &messages);

	void UpdateSpread();
	void UpdateRepoAmount();
	void UpdateInterestAmount();

	void ShowRepoAmountFields();
	void ShowRepoSpreadFields();

	void DisableTradeDateFields();

	double GetOutstanding(const CSRBond* bond);

	//------------------------------------ PROTECTED ------------------------------------
protected:


	//------------------------------------ PRIVATE ------------------------------------
private:

	DECLARATION_DIALOG(CSxLoanAndRepoDealInput)
		static const char * __CLASS__;
};


#endif //!__CSxLoanAndRepoDealInput_H__