/*
** Includes
*/

// standard
#include "SphInc/portfolio/SphTransaction.h"
#include "SphTools/SphLoggerUtil.h"
#include "SphInc/instrument/SphInstrument.h"
// specific
#include "CSxUnderlyingRICDealCheck.h"
//#include "SphInc/SphIncludes.h"
//#include "../../cc_data/CSRic.h"

#include "SphInc/instrument/SphLeg.h"


/*
** Namespace
*/
using namespace sophis::backoffice_kernel;
using namespace sophis::portfolio;
using namespace sophisTools::logger;

/*static*/ const char* CSxUnderlyingRICDealCheck::__CLASS__ = "CSxUnderlyingRICDealCheck";

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_CHECK_DEAL(CSxUnderlyingRICDealCheck);

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CSxUnderlyingRICDealCheck::VoteForCreation(CSRTransaction& transaction) const
throw (sophis::tools::VoteException)
{
	BEGIN_LOG("VoteForCreation");
	Check(transaction);
	END_LOG();
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CSxUnderlyingRICDealCheck::VoteForModification(const CSRTransaction& original,
									 CSRTransaction& transaction) const
throw (sophis::tools::VoteException)
{
	BEGIN_LOG("VoteForModification");
	Check(transaction);
	END_LOG();
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CSxUnderlyingRICDealCheck::VoteForDeletion(const CSRTransaction& transaction) const
throw (sophis::tools::VoteException)
{
	BEGIN_LOG("VoteForDeletion");
	Check(const_cast<CSRTransaction&>(transaction));
	END_LOG();
}


//-------------------------------------------------------------------------------------------------------------
void CSxUnderlyingRICDealCheck::Check(sophis::portfolio::CSRTransaction& transaction) const
throw(sophis::tools::VoteException)
{
	BEGIN_LOG("Check");

	const CSRInstrument* inst = transaction.GetInstrument();
	std::string msg = "";

	if (inst)
	{
		// EQU-Options | EQU-Stock Derivatives
		if (inst->GetType() == eInstrumentType::iDerivative)
		{
			const CSRInstrument* underlying = inst->GetUnderlyingInstrument();
			if (underlying)
			{
				if (underlying->GetType() == eInstrumentType::iEquity || underlying->GetType() == eInstrumentType::iIndex)
				{
					if (GetRIC(underlying).empty())
					{
						throw VoteException(FROM_STREAM("The underlying of instrument" << inst->GetCode() << " does not have a valid RIC. Please check"));
					}
				}
			}
		}

		// EQU-Swaps-All | EQU-Swaps-Equity Swap Variable Index | EQU-Volatility Derivative
		else if(inst->GetType() == eInstrumentType::iSwap)
		{
			const CSRSwap* pSwap = dynamic_cast<const CSRSwap*>(inst);
			if (!pSwap)
				throw VoteException(FROM_STREAM("Unexpected error : " << inst->GetCode() << " cannot be cast as a swap."));

			//Receiving leg
			const CSRLeg *receivLeg = pSwap->GetLeg(0);
			long recvSicovam = receivLeg->GetUnderlyingCode();
			
			// Paying Leg
			const CSRLeg *payLeg = pSwap->GetLeg(1);
			long paySicovam = payLeg->GetUnderlyingCode();

			const CSRInstrument* recvUnderlying = CSRInstrument::GetInstance(recvSicovam);
			const CSRInstrument* payUnderlying = CSRInstrument::GetInstance(paySicovam);

			bool noRIC = false;
			if (recvUnderlying)
			{
				if (recvUnderlying->GetType() == eInstrumentType::iEquity || recvUnderlying->GetType() == eInstrumentType::iIndex)
				{
					noRIC = GetRIC(recvUnderlying).empty();
				}
			}
			if (payUnderlying)
			{
				if (payUnderlying->GetType() == eInstrumentType::iEquity || payUnderlying->GetType() == eInstrumentType::iIndex)
				{
					noRIC = GetRIC(payUnderlying).empty();
				}
			}
			if (noRIC)
			{
				throw VoteException(FROM_STREAM("The underlying of swap " << inst->GetCode() << " does not have a valid RIC. Please check"));
			}
		}
	}
	else
	{
		msg = "Cannot get the instrument of transaction #" + transaction.GetTransactionCode();
		LOG(Log::debug, msg);
		throw VoteException(msg.c_str());
	}

	END_LOG();
}

/*static*/ std::string CSxUnderlyingRICDealCheck::GetRIC(const CSRInstrument* underlying)
{
	//CSRic* RIC = underlying->GetRic();
	//const TRicList * ricList = RIC->getRicList();
	// return std::string(ricList->reuter);
	SSComplexReference complexRef;
	complexRef.type = "REUTER";
	complexRef.value = "";
	underlying->GetRic(complexRef);
	return complexRef.value; 

}



