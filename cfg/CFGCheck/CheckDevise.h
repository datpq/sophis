#ifndef __CheckDevise_H__
	#define __CheckDevise_H__

/*
** Includes
*/
#include "SphInc/portfolio/SphTransactionAction.h"
#include "SphInc/portfolio/SphTransaction.h"
#pragma warning(disable:4251)
#include "SphInc/value/kernel/SphCashInterface.h"
#include "SphInc/value/kernel/SphFundPortfolio.h"

/*
** Class
*/
class CheckDevise : public sophis::portfolio::CSRTransactionAction
{
	DECLARATION_TRANSACTION_ACTION(CheckDevise);

//------------------------- PUBLIC -------------------------
public:

	/** Ask for a creation of a transaction.
	When creating, all the triggers will be called via VoteForCreation to check if they accept the
	creation in the order eOrder + lexicographical order.
	At this stage, the position ID and the ID of the transaction are not
	necessarily created.
	@param transaction is the transaction to create. It is a non-const object so you can
	modify it.
	@throws VoteException if you reject that creation.
	*/
	virtual void VoteForCreation(CSRTransaction &transaction)
		throw (sophis::tools::VoteException);

	/** Ask for a modification of a transaction.
	When modifying, all triggers will be called via VoteForModification to check whether they accept the
	modification in the order eOrder + lexicographical order.
	@param original is the original transaction before any modification.
	@param transaction is the modified transaction. It is a non-const object so you can
	modify it.
	@throws VoteException if you reject that modification.
	*/
	virtual void VoteForModification(const CSRTransaction & original, CSRTransaction &transaction)
		throw (sophis::tools::VoteException);

	/** Ask for a deletion of a transaction.
	When deleting, all the triggers will be called via VoteForDeletion to check if they accept the
	modification in the order eOrder + lexicographical order.
	@param transaction is the original transaction before any modification.
	@throws VoteException if you reject that deletion.
	*/
	virtual void VoteForDeletion(const CSRTransaction &transaction)
		throw (sophis::tools::VoteException);



	void FundCheckCurrency(const sophis::portfolio::CSRTransaction * original, const sophis::portfolio::CSRTransaction * transaction, bool isDeleted = false);
	void CheckBalance(const sophis::value::CSAMPortfolio* pRootFolio, const sophis::portfolio::CSRTransaction * transaction) throw (sophis::tools::VoteException);
	void ChangeSophisCurrentDate(const long newDate);
//------------------------------------------------ PRIVATE ------------------------------------------------
private:
	/*
	** Logger data
	*/
	static const char * __CLASS__;

	static bool isForceMode;
	
	static bool isEquity;

	static const long MAD;
	
};

#endif //!__CheckDevise_H__