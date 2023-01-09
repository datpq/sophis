#ifndef __CSxIsBBHIOnboardedDIMAutoTransmitCondition_H__
#define __CSxIsBBHIOnboardedDIMAutoTransmitCondition_H__


/*
** Includes
*/
#include "SphInc/portfolio/SphTransferTrade.h"
#include "SphInc\portfolio\SphTransaction.h"


namespace sophis {

	namespace portfolio {

		/**
		Class CSxIsBBHIOnboardedDIMAutoTransmitCondition:
		*/
		class CSxIsBBHIOnboardedDIMAutoTransmitCondition : public sophis::portfolio::CSRAutoTransmitCondition
		{
			//------------------------------------ PUBLIC ---------------------------------
		public:

			DECLARATION_AUTO_TRANSMIT_CONDITION(CSxIsBBHIOnboardedDIMAutoTransmitCondition)

				// Name for the condition popup
				virtual const char* GetName() const /*= 0*/;

			/** Test if the transaction ticket satisfies the customized rule
			* @param transaction is a transaction ticket,
			* @since 6.2
			* @returns true by default
			*/
			virtual bool AppliedTo(const CSRTransaction &transaction) const;

		private:
			static const char* __CLASS__;
		};
	} // namespace portfolio
} // namespace sophis
#endif