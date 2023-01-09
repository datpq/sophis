#ifndef __CSxOnSettleDateAutoCondition_H__
	#define __CSxOnSettleDateAutoCondition_H__


/*
** Includes
*/
#include "SphInc/portfolio/SphTransferTrade.h"
#include "SphInc\portfolio\SphTransaction.h"


namespace sophis	{

	namespace portfolio	{

	/**
	Class CSxOnSettleDateAutoCondition:
	*/
	class CSxOnSettleDateAutoCondition : public sophis::portfolio::CSRAutoTransmitCondition
	{
	//------------------------------------ PUBLIC ---------------------------------
	public:

		DECLARATION_AUTO_TRANSMIT_CONDITION(CSxOnSettleDateAutoCondition)

		// Name for the condition popup
		virtual const char* GetName() const /*= 0*/;

		/** Test if the transaction ticket satisfies the customized rule			
		* @param transaction is a transaction ticket,        
		* @since 6.2
		* @returns true by default
		*/
		virtual bool AppliedTo(const CSRTransaction &transaction) const;

		/** Test if the corporate action ticket satisfies the customized rule			
		* @param corpAction is a corporate action ticket,        
		* @since 6.2
		* @returns true by default
		*/
		virtual bool AppliedTo(const CSRTransferTrade::CorporateAction &corpAction) const;

		/** Test if the fixing ticket satisfies the customized rule			
		* @param fixing is a fixing ticket,        
		* @param fixingType is the type of fixing (maFixingAsiatique, maFixingDigital, etc...)
		* @since 6.2
		* @returns true by default
		*/
		virtual bool AppliedTo(const CSRTransferFixings::Fixing &fixing, long fixingType) const;

		/** Test if the split ticket satisfies the customized rule			
		* @param ticket is a split ticket,        
		* @param splitType is the type of split (maDivisionInstrument, maDivisionVolatilite, etc...)
		* @since 6.2
		* @returns true by default
		*/
		virtual bool AppliedTo(const CSRTransferSplits::Split &ticket, long splitType) const;

		/** Test if the option barrier ticket satisfies the customized rule			
		* @param ticket is a option barrier ticket,        			
		* @since 6.2
		* @returns true by default
		*/
		virtual bool AppliedTo(const CSRTransferOptionBarrier::OptionBarrier &ticket) const;

		/** Test if the custom ticket satisfies the customized rule			
		* @param ticket is a custom ticket,        			
		* @since 6.2
		* @returns true by default
		*/
		virtual bool AppliedTo(const CSRTransferCustomTicket::CustomTicket &ticket) const;

	private:

		static const char* __CLASS__;

	};
	} // namespace portfolio
} // namespace sophis
#endif