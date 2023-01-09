#ifndef __CSxShortSellDealCheck_H__
	#define __CSxShortSellDealCheck_H__

/*
** Includes
*/
// standard
#include "SphInc/backoffice_kernel/SphCheckDeal.h"
#include "SphInc/portfolio/SphTransaction.h"

/*
** Class
	to check if a deal can be transferred from one status to another one.
	With the back office kernel module, for each event in the workflow definition a checked deal condition
	may be added. This checked deal condition is a derived class from CSRCheckDeal and it is executed in
	an internal Transaction Action.
	@since 4.5.2
	@see CSRTransactionAction
*/

class CSxShortSellDealCheck : public sophis::backoffice_kernel::CSRCheckDeal
{
//------------------------------------ PUBLIC ---------------------------------
public:

	CSxShortSellDealCheck() : CSRCheckDeal()
	{
		if( _InclusiveBusEvents.size() <= 0 )
		{
			_InclusiveBusEvents = GetBusEventFromConfig();
		}
	}

	DECLARATION_CHECK_DEAL(CSxShortSellDealCheck);

	/** Ask for a creation of a transaction.
	When creating, once the the workflow selector and the event have been found,
	and if a checked deal condition has been added, this method is executed.
	@param transaction is the transaction to create. It is a non-const object so you can
	modify it.
	@throws VoteException if you reject that creation.
	*/
	virtual void VoteForCreation(sophis::portfolio::CSRTransaction& transaction) const
		throw (sophis::tools::VoteException);

	/** Ask for a modification of a transaction.
	When modifying, once the the workflow selector and the event have been found,
	and if a checked deal condition has been added, this method is executed.
	@param original is the original transaction before any modification.
	@param transaction is the modified transaction. It is a non-const object so you can
	modify it.
	@throws VoteException if you reject that modification.
	*/
	virtual void VoteForModification(const sophis::portfolio::CSRTransaction& original,
									 sophis::portfolio::CSRTransaction& transaction) const
		throw (sophis::tools::VoteException);

	/** Ask for a deletion of a transaction.
	When deleting, once the the workflow selector and the event have been found,
	and if a checked deal condition has been added, this method is executed.
	@param transaction is the original transaction before any modification.
	@throws VoteException if you reject that deletion.
	*/
	virtual void VoteForDeletion(const sophis::portfolio::CSRTransaction& transaction) const
		throw (sophis::tools::VoteException);

//------------------------------------ PRIVATE --------------------------------
private:
	void Check(sophis::portfolio::CSRTransaction& transaction) const  throw (sophis::tools::VoteException);

	CSRTransactionVector* GetProvisionTrades(CSRTransaction& transaction) const;
	static std::vector<long> GetBusEventFromConfig();
	static bool IsShortSelling(CSRTransactionVector* transactionVec);
	
	static void ModifyBusEvent(CSRTransaction& transaction, long busEvent)
		throw (sophis::tools::VoteException);
	
	static long GetShortSellBusEventFromConfig();

	static const char* __CLASS__;
	static std::vector<long> _InclusiveBusEvents;

};

#endif //!__CSxShortSellDealCheck_H__