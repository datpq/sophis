#ifndef __SRCompliance_H__
	#define __SRCompliance_H__

/*
** Includes
*/
#include "SphInc/value/kernel/SphFundSouscriptionAction.h"
#include "SphInc/value/kernel/SphFundPurchase.h"

/*
** Class
*/
class SRCompliance : public sophis::value::CSAMFundSubscriptionAction
{
	DECLARATION_SUBSCRIPTION_ACTION(SRCompliance);

//------------------------- PUBLIC -------------------------
public:

	/** Ask for a creation of a subscription.
	When creating, all the triggers will be called via VoteForCreation to check if they accept the
	creation in the order eOrder + lexicographical order.
	At this stage, the position ID and the ID of the subscription are not
	necessarily created.
	@param subscription is the subscription to create. It is a non-const object so you can
	modify it.
	@throws VoteException if you reject that creation.
	*/
	virtual void VoteForCreation(sophis::value::CSAMFundSR &subscription)
		throw (sophis::tools::VoteException);

	/** Ask for a modification of a subscription.
	When modifying, all triggers will be called via VoteForModification to check whether they accept the
	modification in the order eOrder + lexicographical order.
	@param original is the original subscription before any modification.
	@param subscription is the modified subscription. It is a non-const object so you can
	modify it.
	@throws VoteException if you reject that creation.
	*/
	virtual void VoteForModification(const sophis::value::CSAMFundSR & original, sophis::value::CSAMFundSR &subscription)
		throw (sophis::tools::VoteException);

	/** Ask for a deletion of a subscription.
	When deleting, all the triggers will be called via VoteForDeletion to check if they accept the
	modification in the order eOrder + lexicographical order.
	@param subscription is the original subscription before any modification.
	@throws VoteException if you reject that creation.
	*/
	virtual void VoteForDeletion(const sophis::value::CSAMFundSR &subscription)
		throw (sophis::tools::VoteException);

	
	double GetThirdPartiesAmount(const int fundId, const int investorId, const int businessPartnerId, const long paymentDate);	

	double GetThirdPartiesShares(const int fundId, const int investorId, const int businessPartnerId, const long paymentDate);

	bool IsEqual(const sophis::value::CSAMFundSR &original, const sophis::value::CSAMFundSR &subscription);

	void CheckAMFundSRModification(const sophis::value::CSAMFundSR & original, sophis::value::CSAMFundSR &subscription);

	void CheckAMFundSRDeletion(const sophis::value::CSAMFundSR &subscription);

	void CheckLastNav(const sophis::value::CSAMFundSR &subscription) throw (VoteException);

	void HandleExceptionModification(const VoteException & ex);

	void HandleExceptionDeletion(const VoteException & ex);

	void HandleExceptionCreation(const VoteException & ex);

	void HandleException(const VoteException & ex, _STL::string confirmMsg);

	
//------------------------- PROTECTED -------------------------
protected:

	struct SSAmount
	{
		double fAmount;
	};

	struct SSShares
	{
		long fShares;
	};

//------------------------- PRIVATE -------------------------
private:

	/*
	** Logger data
	*/
	static const char * __CLASS__;

};

#endif //!__SRCompliance_H__