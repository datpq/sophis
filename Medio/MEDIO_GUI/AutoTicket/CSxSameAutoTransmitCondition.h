#ifndef __CSxSameAutoTransmitCondition_H__
	#define __CSxSameAutoTransmitCondition_H__


/*
** Includes
*/
#include "SphInc/portfolio/SphTransferTrade.h"
#include "SphInc\portfolio\SphTransaction.h"


namespace sophis	{

	namespace portfolio	{

	/**
	Class CSxSameAutoTransmitCondition:
	*/
	class CSxSameAutoTransmitCondition : public sophis::portfolio::CSRAutoTransmitCondition
	{
	//------------------------------------ PUBLIC ---------------------------------
	public:


		DECLARATION_AUTO_TRANSMIT_CONDITION(CSxSameAutoTransmitCondition)

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