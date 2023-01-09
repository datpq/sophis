#ifndef __CSxIsModifiedAutoTransmitCondition_H__
	#define __CSxIsModifiedAutoTransmitCondition_H__


/*
** Includes
*/
#include "SphInc/portfolio/SphTransferTrade.h"
#include "SphInc\portfolio\SphTransaction.h"


namespace sophis	{

	namespace portfolio	{

	/**
	Class CSxIsModifiedAutoTransmitCondition:
	*/
	class CSxIsModifiedAutoTransmitCondition : public sophis::portfolio::CSRAutoTransmitCondition
	{
	//------------------------------------ PUBLIC ---------------------------------
	public:

		DECLARATION_AUTO_TRANSMIT_CONDITION(CSxIsModifiedAutoTransmitCondition)

		// Name for the condition popup
		virtual const char* GetName() const /*= 0*/;

		/** Test if the transaction ticket satisfies the customized rule			
		* @param transaction is a transaction ticket,        
		* @since 6.2
		* @returns true by default
		*/
		virtual bool AppliedTo(const CSRTransaction &transaction) const;
	};
	} // namespace portfolio
} // namespace sophis
#endif