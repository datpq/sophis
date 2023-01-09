#ifndef __CSxFXCheckDeal_H__
#define __CSxFXCheckDeal_H__

/*
** Includes
*/
// standard
#include "SphInc/portfolio/SphTransactionAction.h"
#include "SphInc/portfolio/SphTransaction.h"
#include "SphInc/instrument/SphForexSpot.h"
#include "SphInc\tools\SphValidation.h"

class CSxFXCheckDeal : public sophis::portfolio::CSRTransactionAction
{
public:

	static bool IsActivated();
	CSxFXCheckDeal();

	virtual  CSRTransactionAction* Clone() const { return new CSxFXCheckDeal(*this); }

	virtual void VoteForCreation(CSRTransaction &transaction, long event_id)
		throw (sophis::tools::VoteException);

	virtual void NotifyCreated(const CSRTransaction &transaction, tools::CSREventVector & message, long event_id)
		throw (sophisTools::base::ExceptionBase);
private:

	//tools::CSREventVector* m_message;

	static bool IsFXForward( const CSRForexSpot* pFX, long transactionDate, long settlementDate );
	static const char* __CLASS__;

};

#endif //!__CSxFXCheckDeal_H__
