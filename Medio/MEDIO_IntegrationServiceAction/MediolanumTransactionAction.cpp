/*
** Includes
*/

#include "../MEDIO_GUI/GUI/CSxTransactionDlg.h"
#include "SphInc/misc/ConfigurationFileWrapper.h"
#include "SphSDBCInc/queries/SphQuery.h"
#include "SphSDBCInc/queries/SphQueryBuffered.h"
#include "SphSDBCInc/queries/SphInserter.h"
#include "SphSDBCInc/params/ins/SphIn.h"
#include "SphSDBCInc/params/ins/SphInOffset.h"
#include "SphSDBCInc/params/ins/SphInRef.h"
//#include "SphSDBCInc/params/ins/SphInArray.h"
#include "SphSDBCInc/params/outs/SphOutOffset.h"
#include "SphTools/SphLogger.h"
#include "SphTools/SphLoggerUtil.h"
#include "SphInc/SphUserRights.h"
#include "SphInc/backoffice_kernel/SphBusinessEvent.h"
#include "SphLLInc\cdc_donnee.h"

// specific
#include "MediolanumTransactionAction.h"
#include <SphInc/gui/SphEditUser.h>
#include <boost/algorithm/string/split.hpp>
#include <boost/algorithm/string/classification.hpp>

/*
** Namespace
*/
using namespace _STL;
using namespace sophis::portfolio;
using namespace sophis::tools;
using namespace sophis::sql;

const char * MediolanumTransactionAction::__CLASS__ = "MediolanumTransactionAction";
_STL::string MediolanumTransactionAction::CashUserNames = "";

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_TRANSACTION_ACTION(MediolanumTransactionAction)

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void MediolanumTransactionAction::VoteForCreation(CSRTransaction &transaction)
throw (VoteException)
{
	BEGIN_LOG("VoteForCreation");
	const CSRInstrument* inst = CSRInstrument::GetInstance(transaction.GetInstrumentCode());
	if(inst)
	{
		if(inst->GetType() == 'C')
		{
			if (CashUserNames.empty())
			{
				ConfigurationFileWrapper::getEntryValue("MediolanumRMA", "CashUserNames", CashUserNames, "RBCUploader,SSBCustody");
				MESS(Log::debug, "Got CashUserNames from config : " << CashUserNames);
			}
			std::string delimiter = ",";
			vector<string> SplitVec;
			boost::split(SplitVec, CashUserNames, boost::is_any_of(delimiter));
			bool userIsOk = false;
			for (vector<string>::iterator it = SplitVec.begin(); it != SplitVec.end(); ++it) {
				long userID = CSRUserRights::ConvNameToIdent(it->c_str());
				if (userID == transaction.GetOperator()) {
					userIsOk = true;
					break;
				}
			}
			if (!userIsOk)
			{
				MESS(Log::debug, "Trade is not from designated operator " << CashUserNames << "! Do nothing");
				return;
			}
			else
			{
				const backoffice_kernel::CSRBusinessEvent* bus = backoffice_kernel::CSRBusinessEvent::GetBusinessEventByName("Fee");
				if(bus->GetIdent() == transaction.GetTransactionType())
				{
					transaction.SetNetAmountOnly(transaction.GetQuantity());	
				}
				else
				{
					transaction.SetNetAmountOnly(-transaction.GetQuantity());	
				}
				MESS(Log::debug, "Trade is from designated operator " << CashUserNames << " and of type 'C'. Set net amount to " << transaction.GetNetAmount());
			}
		}

		//transaction.SetPoolFactor(0.81234)
	}
	END_LOG();
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void MediolanumTransactionAction::VoteForModification(const CSRTransaction & original, CSRTransaction &transaction)
throw (VoteException)
{
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void MediolanumTransactionAction::VoteForDeletion(const CSRTransaction &transaction)
throw (VoteException)
{

}


//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void MediolanumTransactionAction::NotifyCreated(const CSRTransaction &transaction, tools::CSREventVector & message)
throw (ExceptionBase)
{
	BEGIN_LOG("NotifyCreated");
	try
	{
		transaction.LoadUserElement();
		char buf[256];
		transaction.LoadGeneralElement(CSxTransactionDlg::eRBCTradeId,buf);
		_STL::string key("RBC_TRADE_ID");
		_STL::string value(buf);
		InsertOrUpdateExtRefToDB((TransactionIdent)transaction.getReservedCode(),key,value);

		char bufComm[256];
		transaction.LoadGeneralElement(CSxTransactionDlg::eRBCComment, bufComm);
		_STL::string keyComm("TKT_RBC_COMMENT");
		_STL::string valueComm(bufComm);
		InsertOrUpdateExtRefToDB((TransactionIdent)transaction.getReservedCode(), keyComm, valueComm);

		char bufCaps[256];
		transaction.LoadGeneralElement(CSxTransactionDlg::eRBCCapsRef, bufCaps);
		_STL::string keyCaps("TKT_RBC_CAPS_REF");
		_STL::string valueCaps(bufCaps);
		InsertOrUpdateExtRefToDB((TransactionIdent)transaction.getReservedCode(), keyCaps, valueCaps);

		char bufUcits[256];
		transaction.LoadGeneralElement(CSxTransactionDlg::eRBCUcitsv, bufUcits);
		_STL::string keyUcits("TKT_RBC_UCITSVCODE");
		_STL::string valueUcits(bufUcits);
		InsertOrUpdateExtRefToDB((TransactionIdent)transaction.getReservedCode(), keyUcits, valueUcits);


		char bufTranType[256];
		transaction.LoadGeneralElement(CSxTransactionDlg::eRBCTransType, bufTranType);
		_STL::string keyTranType("TKT_RBC_TRANSTYPE");
		_STL::string valueTranType(bufTranType);
		InsertOrUpdateExtRefToDB((TransactionIdent)transaction.getReservedCode(), keyTranType, valueTranType);
	}
	catch(ExceptionBase e)
	{
		MESS(Log::error, e);
		END_LOG();
	}
	END_LOG();
}


//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void MediolanumTransactionAction::NotifyModified(const CSRTransaction &original, const CSRTransaction &transaction, tools::CSREventVector & message)
throw (ExceptionBase)
{
}


//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void MediolanumTransactionAction::NotifyDeleted(const CSRTransaction &transaction, tools::CSREventVector & message)
throw (ExceptionBase)
{

}

bool MediolanumTransactionAction::InsertOrUpdateExtRefToDB(const TransactionIdent ident, const _STL::string& refName, const _STL::string& refValue)
{
	BEGIN_LOG("InsertOrUpdateExtRefToDB");
	CSRQuery query;
	try
	{
		query.SetName("[CSRSwapTransactionReferencesCache] insert/update/delete transaction external ref");
		query << "merge into EXTRNL_REFERENCES_TRADES spr "
			<< "using (select "
			<< CSRIn(ident) << " as REFCON,"
			<< CSRIn(refName) << " as REF_NAME,"
			<< CSRIn(refValue) << " as REF_VALUE from dual) ref "
			<< "on (ref.REFCON=spr.SOPHIS_IDENT and ref.REF_NAME=spr.ORIGIN) "
			<< "when matched then "
			<<		"update set spr.VALUE=ref.REF_VALUE "
			<<		"delete where (ref.REF_VALUE is null) "
			<< "when not matched then "
			<< "insert (spr.SOPHIS_IDENT, spr.ORIGIN, spr.VALUE) "
			<< "values (ref.REFCON, ref.REF_NAME, ref.REF_VALUE) "
			<< "where (ref.REF_VALUE is not null)";
		query.Execute();
	}
	catch (sophisTools::base::ExceptionBase ex)
	{
		MESS(Log::error, ex);
		END_LOG();
		return false;
	}
	LOG(Log::debug, "Successfully updated the table EXTRNL_REFERENCES_TRADES");
	END_LOG();
	return true;
}