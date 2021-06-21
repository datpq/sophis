#pragma warning(disable:4251)
#include "CFG_Conditions.h"
#include "SphTools/SphLoggerUtil.h"
#include "SphInc/backoffice_kernel/SphAllotment.h"
#include "SphInc/instrument/SphInstrument.h"
#include "SphInc/instrument/SphDebtInstrument.h"
#include "SphInc/instrument/SphBond.h"
#include "SphInc/backoffice_kernel/SphBusinessEvent.h"
#include "SphInc/backoffice_kernel/SphKernelEvent.h"
#include "SphInc/backoffice_kernel/SphCorporateAction.h"
//DPH
#if (TOOLKIT_VERSION < 720)
#include "SphLLInc\misc\ConfigurationFileWrapper.h";
#else
#include "SphInc\misc\ConfigurationFileWrapper.h";
#endif
#include "SphInc/portfolio/SphTransactionVector.h"
#include "SphInc/backoffice_kernel/SphKernelStatus.h"
#include "SphSDBCInc/SphSQLQuery.h"
#include "SphInc\value\kernel\SphFundPurchase.h"
#include "SphInc\value\kernel\SphFund.h"
#include "UpgradeExtension.h"

using namespace sophis::portfolio;
using namespace sophis::accounting;
using namespace sophis::backoffice_kernel;
using namespace sophis::value;

const char* CFG_RulesConditionTradeWithAllot::__CLASS__ = "CFG_Conditions";
const char* CFG_RulesConditionPnLWithAllot::__CLASS__ = "CFG_Conditions";
const char* CFGIsSLRepoLeg::__CLASS__ = "CFGIsSLRepoLeg";
const char* CFGIsNotSLRepoLeg::__CLASS__ = "CFGIsNotSLRepoLeg";
const char* CFGIsDATOver2years::__CLASS__ = "CFGIsDATOver2years";
const char* CFGIsDATLess2years::__CLASS__ = "CFGIsDATLess2years";
const char* CFGIsDATOver2yearsTrade::__CLASS__ = "CFGIsDATOver2yearsTrade";
const char* CFGIsDATLess2yearsTrade::__CLASS__ = "CFGIsDATLess2yearsTrade";
const char * CFGBOEventAccRule::__CLASS__		= "CFGBOEventAccRulePaid";
const char * CFGBOEventAccRule2::__CLASS__		= "CFGBOEventAccRuleFeesPaid";
const char * CFGIsARedemption::__CLASS__		= "CFGIsARedemption";
const char * CFG_RulesConditionTradeIsPartialRedemption::__CLASS__ = "CFG_RulesConditionTradeIsPartialRedemption";
const char * CFG_RulesConditionTradeIsFinalRedemption::__CLASS__ = "CFG_RulesConditionTradeIsFinalRedemption";
const char * CFG_AccountingConditionsMgr::__CLASS__ = "CFG_AccountingConditionsMgr";

void CFG_AccountingConditionsMgr::GetBondCriteria(const sophis::portfolio::CSRTransaction& trade, bool& isABond, bool& isAmortissable, bool& isFinal)
{
	BEGIN_LOG("GetBondCriteria");

	MESS(Log::debug, "GetBondCriteria for trade (" << trade.GetTransactionCode() << ")");
	isABond = false;
	isAmortissable = false;
	isFinal = false;

	const CSRBond* pBond = dynamic_cast<const CSRBond*>(CSRInstrument::GetInstance(trade.GetInstrumentCode()));
	if (pBond)
	{
		isABond = true;
		
		long pariPassuDate   = trade.GetPariPassuDate();
		double qty = trade.GetQuantity();
		double tradeNotional = trade.GetNotional();
		if (tradeNotional == 0)
			tradeNotional = 1;
		long accruedCouponDate = pBond->GetAccruedCouponDate(trade.GetTransactionDate(), trade.GetSettlementDate());				
		double instNotional = pBond->GetNotional();
		if (instNotional == 0)
			instNotional = 1;
		double instSecondNotional = pBond->GetSecondNotional(pariPassuDate, accruedCouponDate);
		double grossAmount = trade.GetGrossAmount();
		

		MESS(Log::debug, "Instrument Floating Notional " << instSecondNotional);
		MESS(Log::debug, "Instrument Notional " << instNotional);
		MESS(Log::debug, "Trade Quantity " << qty);
		MESS(Log::debug, "Trade Gross Amount " << grossAmount);
		MESS(Log::debug, "Trade Notional " << tradeNotional);
		
		isAmortissable = (grossAmount/tradeNotional == 1) ? false : true;
		isFinal = (trade.GetTransactionDate() == pBond->GetExpiry());
	}	
	
	MESS(Log::debug, "Is A Bond (" << isABond << ") - Is Depreciable/Amortizable (" << isAmortissable << ") - Is Final (" << isFinal << ")");
	
	END_LOG();
}


CONSTRUCTOR_RULES_CONDITION_TRANSACTION(CFG_RulesConditionTradeIsPartialRedemption)


bool CFG_RulesConditionTradeIsPartialRedemption::get_condition( const sophis::portfolio::CSRTransaction& trade ) const
{
	bool isABond = false, isAmortissable = false, isFinal = false;
	CFG_AccountingConditionsMgr::GetBondCriteria(trade, isABond, isAmortissable, isFinal);
	return isABond && isAmortissable && !isFinal;
}

CONSTRUCTOR_RULES_CONDITION_TRANSACTION(CFG_RulesConditionTradeIsFinalRedemption)

bool CFG_RulesConditionTradeIsFinalRedemption::get_condition( const sophis::portfolio::CSRTransaction& trade ) const
{
	bool isABond = false, isAmortissable = false, isFinal = false;
	CFG_AccountingConditionsMgr::GetBondCriteria(trade, isABond, isAmortissable, isFinal);
	return isABond && isAmortissable && isFinal;
}

CONSTRUCTOR_RULES_CONDITION_TRANSACTION(CFG_RulesConditionTradeWithAllot)
/*virtual*/ bool CFG_RulesConditionTradeWithAllot::get_condition( const CSRTransaction& trade ) const
{
	BEGIN_LOG("get_condition");

	//DPH
	//long refcon = trade.GetTransactionCode();
	TransactionIdent refcon = trade.GetTransactionCode();

	long instrumentCode = trade.GetInstrumentCode();
	const CSRInstrument * instr = CSRInstrument::GetInstance(instrumentCode);
	if (instr)
	{
		if (instr->GetType() == 'P')
		{
			//DPH
			//long underlyingCode = instr->GetUnderlying(0);
			//long underlyingCode = UpgradeExtension::GetUnderlying(instr, 0);
			//const CSRInstrument * under = CSRInstrument::GetInstance(underlyingCode);
			const CSRInstrument * under = instr->GetUnderlyingInstrument();
			if (under)
			{
				long AllotmentId = under->GetAllotment();
				const char * AllotmentName = SSAllotment::GetName(AllotmentId);
				if (strcmp(AllotmentName, m_allotment.c_str()) == 0)
				{
					return true;
				}
			}
		}
		else
		{
			char query [256] = {'\0'};
			sprintf_s(query, "select code_emet from titres where sicovam=(select sicovam from histomvts where refcon=(select reference from histomvts where refcon=%ld))",refcon);
			long code_emet = 0; 
			errorCode err = CSRSqlQuery::QueryReturning1Long (query, &code_emet);
			if (err)
				return false;

			const CSRInstrument * inst = CSRInstrument::GetInstance(code_emet);
			if (inst)
			{
				long AllotmentId = inst->GetAllotment();
				const char * AllotmentName = SSAllotment::GetName(AllotmentId);
				if (strcmp(AllotmentName, m_allotment.c_str()) == 0)
				{
					return true;
				}
			}
		}
	}

	END_LOG();
	return false;
}

void CFG_RulesConditionTradeWithAllot::set_allotment(_STL::string allotment)
{
	m_allotment = allotment;
}

CONSTRUCTOR_RULES_CONDITION_POSITION(CFG_RulesConditionPnLWithAllot)
/*virtual*/ bool CFG_RulesConditionPnLWithAllot::get_condition( const portfolio::CSRPosition& position ) const
{
	BEGIN_LOG("get_condition");

	long InstrumentCode = position.GetInstrumentCode();
	const CSRInstrument * instr = CSRInstrument::GetInstance(InstrumentCode);
	if (instr)
	{
		if (instr->GetType() == 'P')
		{
			//DPH
			//long underlyingCode = instr->GetUnderlying(0);
			//long underlyingCode = UpgradeExtension::GetUnderlying(instr, 0);
			//const CSRInstrument * under = CSRInstrument::GetInstance(underlyingCode);
			const CSRInstrument * under = instr->GetUnderlyingInstrument();
			if (under)
			{
				long AllotmentId = under->GetAllotment();
				const char * AllotmentName = SSAllotment::GetName(AllotmentId);
				if (strcmp(AllotmentName, m_allotment.c_str()) == 0)
				{
					return true;
				}
			}
		}
	}

	END_LOG();
	return false;
}

void CFG_RulesConditionPnLWithAllot::set_allotment(_STL::string allotment)
{
	m_allotment = allotment;
}

CONSTRUCTOR_RULES_CONDITION_POSITION(CFGIsSLRepoLeg)
/*virtual*/ bool CFGIsSLRepoLeg::get_condition( const CSRPosition& position ) const
{
	sophis::portfolio::CSRTransactionVector transaction_list;
	position.GetTransactions(transaction_list);

	char query [256] = {'\0'};
	sprintf_s(query, "select ID_CA from CORPORATE_ACTION_PREF where ID_BE=21");
	long InitiationEventCodeDeposit = 0; 
	errorCode err = CSRSqlQuery::QueryReturning1Long (query, &InitiationEventCodeDeposit);
	if (err)
		return false;

	for (unsigned int i = 0 ; i < transaction_list.size() ; i++)
	{
		CSRTransaction myTrans = transaction_list[i];
		//DPH
		//long refcon = myTrans.GetTransactionCode();
		eTransactionType transtype =myTrans.GetTransactionType(); 
		if (InitiationEventCodeDeposit == transtype)
		{
			return true;
		}
	}
	return false;
}

CONSTRUCTOR_RULES_CONDITION_POSITION(CFGIsNotSLRepoLeg)
/*virtual*/ bool CFGIsNotSLRepoLeg::get_condition( const CSRPosition& position ) const
{
	sophis::portfolio::CSRTransactionVector transaction_list;
	position.GetTransactions(transaction_list);

	CSRCADefaultBusinessEvent caDefaultBusinessEvent;
	eTransactionType InitiationEventCode = caDefaultBusinessEvent.GetValueElseDefault(CSRCADefaultBusinessEvent::item_stock_loan_lnb_initiation);

	for (unsigned int i = 0 ; i < transaction_list.size() ; i++)
	{
		CSRTransaction myTrans = transaction_list[i];
		//DPH
		//long refcon = myTrans.GetTransactionCode();
		eTransactionType transtype =myTrans.GetTransactionType(); 
		if (InitiationEventCode == transtype)
		{
			return true;
		}
	}
	return false;
}

CONSTRUCTOR_RULES_CONDITION_POSITION(CFGIsDATLess2years)
/*virtual*/ bool CFGIsDATLess2years::get_condition( const CSRPosition& position ) const
{
	long instrumentCode = position.GetInstrumentCode();
	const CSRInstrument * inst = CSRInstrument::GetInstance(instrumentCode);
	if (inst)
	{
		if (inst->GetType() == 'T')
		{
			const CSRDebtInstrument * myDebt = dynamic_cast<const CSRDebtInstrument *>(inst);
			if (NULL != myDebt)
			{
				long startDate = myDebt->GetStartDate();
				long endDate = myDebt->GetExpiry();
				if (endDate - startDate <= 2*365 )
					return true;
			}
		}
	}
	return false;
}


CONSTRUCTOR_RULES_CONDITION_POSITION(CFGIsDATOver2years)
/*virtual*/ bool CFGIsDATOver2years::get_condition( const CSRPosition& position ) const
{
	long instrumentCode = position.GetInstrumentCode();
	const CSRInstrument * inst = CSRInstrument::GetInstance(instrumentCode);
	if (inst)
	{
		if (inst->GetType() == 'T')
		{
			const CSRDebtInstrument *myDebt = dynamic_cast<const CSRDebtInstrument *>(inst);
			if (NULL != myDebt)
			{
				long startDate = myDebt->GetStartDate();
				long endDate = myDebt->GetExpiry();
				if (endDate - startDate > 2*365 )
					return true;
			}
		}
	}
	return false;
}


CONSTRUCTOR_RULES_CONDITION_TRANSACTION(CFGIsDATLess2yearsTrade)
/*virtual*/ bool CFGIsDATLess2yearsTrade::get_condition( const CSRTransaction& trade ) const
{
	long instrumentCode = trade.GetInstrumentCode();
	const CSRInstrument * inst = CSRInstrument::GetInstance(instrumentCode);
	if (inst)
	{
		if (inst->GetType() == 'T')
		{
			const CSRDebtInstrument * myDebt = dynamic_cast<const CSRDebtInstrument *>(inst);
			if (NULL != myDebt)
			{
				long startDate = myDebt->GetStartDate();
				long endDate = myDebt->GetExpiry();
				if (endDate - startDate <= 2*365 )
					return true;
			}
		}
	}
	return false;
}


CONSTRUCTOR_RULES_CONDITION_TRANSACTION(CFGIsDATOver2yearsTrade)
/*virtual*/ bool CFGIsDATOver2yearsTrade::get_condition( const CSRTransaction& trade ) const
{
	long instrumentCode = trade.GetInstrumentCode();
	const CSRInstrument * inst = CSRInstrument::GetInstance(instrumentCode);
	if (inst)
	{
		if (inst->GetType() == 'T')
		{
			const CSRDebtInstrument *myDebt = dynamic_cast<const CSRDebtInstrument *>(inst);
			if (NULL != myDebt)
			{
				long startDate = myDebt->GetStartDate();
				long endDate = myDebt->GetExpiry();
				if (endDate - startDate > 2*365 )
					return true;
			}
		}
	}
	return false;
}

//////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////
CONSTRUCTOR_RULES_CONDITION_TRANSACTION(CFGBOEventAccRule)

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CFGBOEventAccRule::get_condition( const CSRTransaction& trade ) const
{
	BEGIN_LOG("get_condition");

	static long defaultStatus = 0;
	static bool isInitialized = false;


	// Get default status
	if (!isInitialized)
	{
		try
		{
			_STL::string defaultBOStatus = "";
			ConfigurationFileWrapper::getEntryValue("ACCOUNTING", "CFG_Status_Paid", defaultBOStatus, "Paid");
			_STL::vector<long> statusList;
			CSRKernelStatus::GetList(statusList);
			for (unsigned int i = 0; i < statusList.size(); i++)
			{
				CSRKernelStatus tempStatus(statusList[i]);
				if (strcmp(tempStatus.GetName(), defaultBOStatus.c_str()) == 0)
					defaultStatus = statusList[i];
			}

			if (defaultStatus  == 0)
				throw GeneralException(FROM_STREAM("Failed to find BO Status '" << defaultBOStatus.c_str() << "'"));
			isInitialized = true;
		}
		catch(ExceptionBase & ex)
		{
			MESS(Log::warning, "Failed to find property 'CFG_Status_Paid' in section 'ACCOUNTING' (" << (const char *)ex << ")");
			return false;
		}
	}

	// Get transaction status
	eBackOfficeType transactionStatus = trade.GetBackOfficeType();

	if (transactionStatus == defaultStatus)
		return true;
	else
	{
		MESS(Log::debug, "BO Status 'Paid' - Transaction status " << transactionStatus << ", Expected Status " << defaultStatus);
		END_LOG();
		return false;
	}

	END_LOG();
	return false;
}

//////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////
CONSTRUCTOR_RULES_CONDITION_TRANSACTION(CFGBOEventAccRule2)

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CFGBOEventAccRule2::get_condition( const CSRTransaction& trade ) const
{
	BEGIN_LOG("get_condition");

	static long defaultStatus = 0;
	static bool isInitialized = false;


	// Get default status
	if (!isInitialized)
	{
		try
		{
			_STL::string defaultBOStatus = "";
			ConfigurationFileWrapper::getEntryValue("ACCOUNTING", "CFG_Status_Fees_Paid", defaultBOStatus, "Fees Paid");
			_STL::vector<long> statusList;
			CSRKernelStatus::GetList(statusList);
			for (unsigned int i = 0; i < statusList.size(); i++)
			{
				CSRKernelStatus tempStatus(statusList[i]);
				if (strcmp(tempStatus.GetName(), defaultBOStatus.c_str()) == 0)
					defaultStatus = statusList[i];
			}

			if (defaultStatus  == 0)
				throw GeneralException(FROM_STREAM("Failed to find BO Status '" << defaultBOStatus.c_str() << "'"));
			isInitialized = true;
		}
		catch(ExceptionBase & ex)
		{
			MESS(Log::warning, "Failed to find property 'CFG_Status_Fees_Paid' in section 'ACCOUNTING' (" << (const char *)ex << ")");
			return false;
		}
	}

	// Get transaction status
	eBackOfficeType transactionStatus = trade.GetBackOfficeType();

	if (transactionStatus == defaultStatus)
		return true;
	else
	{
		MESS(Log::debug, "BO Status 'Fees Paid' - Transaction status " << transactionStatus << ", Expected Status " << defaultStatus);
		END_LOG();
		return false;
	}

	END_LOG();
	return false;
}

CONSTRUCTOR_RULES_CONDITION_SR(CFGIsARedemption)
/*virtual */bool CFGIsARedemption::GetCondition( const sophis::value::CSAMFundSR& sr) const
{
	BEGIN_LOG("get_condition");

	try
	{
		double amount = sr.GetNbShares();
		if (amount < 0)
			return true;
		else
			return false;
	}
	catch(...)
	{
		return false;
	}

	return false;

	END_LOG();
}