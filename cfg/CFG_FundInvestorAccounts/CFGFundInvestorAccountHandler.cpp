/*
** Includes
*/
#pragma warning(disable:4251) // '...' : class '...' needs to have dll-interface to be used by clients of class '...'
#include "SphTools/SphLoggerUtil.h"
#include "SphTools/dataModel/DataSetUtil.h"
#include "SphTools/dataModel/DataDataSetAdapter.h"
#include "SphTools/dataModel/DataSetStorageUtil.h"
#include "SphTools/SphLoggerUtil.h"
#include "SphInc/instrument/SphInstrument.h"
#include "SphLLInc/SphTools/dataModel/DataSequence.h"
#include "SphTools/dataModel/DataSet.h" 
#include "SphTools/dataModel/Documentation.h"
#include "SphLLInc/SphEquityUpdate.h"
#include "SphLLInc/SphTools/base/StringUtil.h"
#include "SphTools/dataModel/DataDataSetAdapter.h"
#include "SphTools/dataModel/DataSequenceDataSetAdapter.h"
#include "SphLLInc/SphTools/dataModel/dataset.h"
#include "sphLLInc/sphtools/datamodel/valuetype.h"


#include "SphInc\backoffice_kernel\SphThirdParty.h"
#include <cstring>
// specific
#include "CFGFundInvestorAccountHandler.h"



#include __STL_INCLUDE_PATH(ostream)
#include __STL_INCLUDE_PATH(iostream)
#include __STL_INCLUDE_PATH(fstream)


/*
** Namespace
*/
using namespace sophis::value;
using namespace sophis::tools;
using namespace sophis::backoffice_kernel;
using namespace sophisTools::base;


//using namespace sophis::fpmlEngine::entity;
using namespace sophis::misc::dataModel;
using namespace sophis::tools::dataModel;
//using namespace sophis::xml::dataModel;
using namespace sophis::market_data;

// Logging definitions
const char* CFGFundInvestorAccountHandler::__CLASS__ = "CFGFundInvestorAccountHandler";

/**
* Exception thrown when the instrument is not found
*/
class NoSuchAccount : public misc::dataModel::NoSuchEntity
{
public:
	NoSuchAccount() 
		: misc::dataModel::NoSuchEntity("InvestorAccount")
	{}
	NoSuchAccount(const NoSuchAccount & rhs)
		: misc::dataModel::NoSuchEntity(rhs) 
	{}
};


/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
//CONSTRUCTORS
//-------------------------------------------------------------------------------------------------------------
CFGFundInvestorAccountHandler::CFGFundInvestorAccountHandler()
:fEntity(),fKey(),fAccountKey(NULL),fInvestorModified(false)
{}

/*-----------------------------------------------------------------------------
*
*/
CFGFundInvestorAccountHandler::CFGFundInvestorAccountHandler(const CFGFundInvestorAccountHandler & rhs)
:fEntity(),fKey(rhs.fKey),fAccountKey(NULL),fInvestorModified(false)
{
	/*if (rhs.fEntity)
		throw sophisTools::base::InvalidInvocationOrder(
		"can not clone a CFGFundInvestorAccountHandler holding a modified entity");*/
}

/*-----------------------------------------------------------------------------
* 
*/
CFGFundInvestorAccountHandler::~CFGFundInvestorAccountHandler()
{
	delete fAccountKey;
}

//-------------------------------------------------------------------------------------------------------------
//FIND
//-------------------------------------------------------------------------------------------------------------
/*-----------------------------------------------------------------------------
* CFGFundInvestorAccountHandler::find
*/
void 
CFGFundInvestorAccountHandler::find(long code)
throw (misc::dataModel::NoSuchEntity,tools::dataModel::DataModelException,sophisTools::base::ExceptionBase)
{
	BEGIN_LOG("find");

	if ( fKey )
		throw sophisTools::base::InvalidInvocationState(
		"CFGFundInvestorAccountHandler already within a session");
	try
	{
		fEntity = CSAmFundInvestorAccountSets::GetInstance()->GetAccount(code);
	}
	catch(InvalidArgument)
	{
		throw NoSuchAccount();
	}

	fKey =code;
	if (fAccountKey)
		delete fAccountKey;
	fAccountKey = new AccountKey();

	BasicLocalResource::name(FROM_STREAM("InvestorAccount["<<fKey<<"]"));

	END_LOG();
}

/*-----------------------------------------------------------------------------
* CSxQuotationsGXMLHandler::find
*/
void 
CFGFundInvestorAccountHandler::find(const tools::dataModel::Data & data)
throw (misc::dataModel::NoSuchEntity,tools::dataModel::DataModelException,sophisTools::base::ExceptionBase)
{
	BEGIN_LOG("find");
	
	 fAccountKey =  this->GetKey(data, true);
	 
	 if (this->FindAccount(fEntity, *fAccountKey, false) == false)
	 {
		 throw NoSuchAccount();
	 }
	 
	 fKey = fEntity.GetId();
	
	END_LOG();
}




//-------------------------------------------------------------------------------------------------------------
//FIND - UPDATE - CHECK - CREATE
//-------------------------------------------------------------------------------------------------------------
void CFGFundInvestorAccountHandler::findOrCreate(const tools::dataModel::Data & description)
throw (tools::dataModel::DataModelException,sophisTools::base::ExceptionBase)
{
	BEGIN_LOG("findOrCreate");

	 fAccountKey =  this->GetKey(description, true);

	 //CheckIf the Depositary is laready linked to the investor
	 CheckInvestorAndDepositary(fAccountKey->InvestorId, fAccountKey->DepositaryId);

	 if (this->FindAccount(fEntity, *fAccountKey, false) == false)
	 {
		 //Create the Entity
		 this->CreateFromIdAndDescription(fEntity, description, fAccountKey->Name, fAccountKey->InvestorId, fAccountKey->DepositaryId, true);

		 //Add Account To the Official List
		 fKey = CSAmFundInvestorAccountSets::GetInstance()->AddAccount(fEntity);
		 if (fKey != 0)
			fEntity = CSAmFundInvestorAccountSets::GetInstance()->GetAccount(fKey);
	 }
	 else
	 {
		 fKey = fEntity.GetId();
	 }
	END_LOG();
}

/*-----------------------------------------------------------------------------
*
*/
void CFGFundInvestorAccountHandler::updateOrCreate(const tools::dataModel::Data & description)
throw (tools::dataModel::DataModelException,sophisTools::base::ExceptionBase)
{
	BEGIN_LOG("updateOrCreate");
	
	 fAccountKey =  this->GetKey(description, true);
	 //CheckIf the Depositary is laready linked to the investor
	 CheckInvestorAndDepositary(fAccountKey->InvestorId, fAccountKey->DepositaryId);

	 if(this->FindAccount(fEntity, *fAccountKey, false) == false)
	 {//If not Found, Create
		 //Create the Entity
		 this->CreateFromIdAndDescription(fEntity, description, fAccountKey->Name, fAccountKey->InvestorId, fAccountKey->DepositaryId, true);

		 //Add Account To the Official List
		 fKey = CSAmFundInvestorAccountSets::GetInstance()->AddAccount(fEntity);
		 if (fKey !=0)
			fEntity = CSAmFundInvestorAccountSets::GetInstance()->GetAccount(fKey);
	 }
	 //Else, Modify ( Update)
	 else
	 {
		 this->UpdateEntityFromDescription(fEntity, description, false);
		 fKey = fEntity.GetId();
		 //Modify the Account
		 CSAmFundInvestorAccountSets::GetInstance()->ModifyAccount(fKey, fEntity);
	 }
	END_LOG();
}

/*-----------------------------------------------------------------------------
*
*/
void CFGFundInvestorAccountHandler::checkOrCreate(const tools::dataModel::Data & description)

{
	BEGIN_LOG("checkOrCreate");	
		this->findOrCreate(description);
	END_LOG();
}

/*-----------------------------------------------------------------------------
*
*/
void CFGFundInvestorAccountHandler::create(const tools::dataModel::Data & description)
throw (misc::dataModel::DuplicateEntity,tools::dataModel::DataModelException,sophisTools::base::ExceptionBase)
{
	BEGIN_LOG("create");
		this->updateOrCreate(description);
	END_LOG();
}

//-------------------------------------------------------------------------------------------------------------
//ADD Reference - Description  To DATASET
//-------------------------------------------------------------------------------------------------------------
/*-----------------------------------------------------------------------------
*
*/
tools::dataModel::Data & CFGFundInvestorAccountHandler::addReference(tools::dataModel::DataSet & refs)
throw (tools::dataModel::DataModelException,sophisTools::base::ExceptionBase)
{
	BEGIN_LOG("addReference");
	using namespace tools::dataModel;

	if ( ! fKey )
		throw sophisTools::base::InvalidInvocationOrder(
		"No Quotation to retrieve the reference of");
	if (fEntity.GetId() == 0)
		throw NoSuchAccount();

	DataSequence & seq = refs.getDataSequence("FundInvestorAccount");
	seq.subValueKind(Set);
	Data &data = seq.incrementSize();
	DataSet &subDataSet = data.dataSetValue();
	if (&fInvestor)
		fInvestor.Describe(subDataSet);

	MESS(Log::debug,"DESCRIBE Investor "<<fInvestor.GetId());

	END_LOG();
	return data;
}

/*-----------------------------------------------------------------------------
*
*/
tools::dataModel::Data & CFGFundInvestorAccountHandler::addDescription(tools::dataModel::DataSet & desc)
throw (misc::dataModel::NoSuchEntity,tools::dataModel::DataModelException,sophisTools::base::ExceptionBase)
{
	BEGIN_LOG("addDescription");
	using namespace tools::dataModel;

	if ( ! fKey )
		throw sophisTools::base::InvalidInvocationOrder(
		"No Quotation to retrieve the reference of");
	if (fEntity.GetId() == 0)
		throw NoSuchAccount();

	DataSequence & seq = desc.getDataSequence("FundInvestorAccount");
	seq.subValueKind(Set);
	Data &data = seq.incrementSize();
	DataSet &subDataSet = data.dataSetValue();
	if (&fInvestor)
		fInvestor.Describe(subDataSet);

	MESS(Log::debug,"DESCRIBE Investor "<<fInvestor.GetId());

	END_LOG();
	return data;
}

//DPH
tools::dataModel::Data & CFGFundInvestorAccountHandler::addDescriptionInSingleDataSet(tools::dataModel::DataSet & desc)
throw (misc::dataModel::NoSuchEntity, tools::dataModel::DataModelException, sophisTools::base::ExceptionBase)
{
	BEGIN_LOG("addDescriptionInSingleDataSet");
	END_LOG();
	return addDescription(desc);
}

//-------------------------------------------------------------------------------------------------------------
//COMMIT & ROLLBACK
//-------------------------------------------------------------------------------------------------------------
/*-----------------------------------------------------------------------------
*
*/
void CFGFundInvestorAccountHandler::commit(sophis::tools::CSREventVector &ev)
throw (sophisTools::base::ExceptionBase)
{
BEGIN_LOG("commit");
	

	if ( ! fKey )
		throw sophisTools::base::InvalidInvocationOrder("No Quotation to save to database");

	if (fEntity.GetId() == 0)
	{
		MESS(Log::warning,"No Entity to Save");
		// nothing to do
		return;
	}

	//Here We have to SAve the investor, even if it has not been modified : 
	//OtherWise not coherency Event is sent
	if (&fInvestor /*&& (fInvestorModified==true)*/)
	{
		fInvestor.Save(ev);
		fInvestorModified = false;
		MESS(Log::debug,"SAVE Investor "<<fInvestor.GetId()<<" OK.");
	}
	else
	{
			MESS(Log::debug,"Investor "<<fEntity.GetInvestor()<<" not saved.");
	}
	// Save to DB
	if (&ev)
	{
		CSAmFundInvestorAccountSets::GetInstance()->SaveToDatabase(ev);
		MESS(Log::debug,"CSAmFundInvestorAccountSets::GetInstance()->SaveToDatabase(ev)");
	}
	else
	{
		CSAmFundInvestorAccountSets::GetInstance()->Save();
		MESS(Log::debug,"CSAmFundInvestorAccountSets::GetInstance()->Save();");
	}
	END_LOG();
}

/*-----------------------------------------------------------------------------
*
*/
void CFGFundInvestorAccountHandler::rollback()
throw ()
{
	BEGIN_LOG("rollback");
	delete fAccountKey;
	fAccountKey = NULL;
	fKey = NULL;
	fInvestorModified = false;
	END_LOG();
}

/*-----------------------------------------------------------------------------
*PRIVTATE
* Search the account wit the key : Investor, Depositary, Name
*If found = Return the founded key, and set fKey
*If not found return a new CSAmFundInvestorAccount, built on the 
*/

AccountKey* CFGFundInvestorAccountHandler::GetKey(const tools::dataModel::Data & data, bool exceptionIfNotFound)
{
	BEGIN_LOG("GetKey");
	const DataSet & descriptionContent = data.dataSetValue();

	AccountKey* accountKey = new AccountKey();

	//Investor
	MESS(Log::info,"Try Get Investor.");
	const CSRThirdParty* investor;
	if (descriptionContent.has("investor"))
	{
		const Data & data = descriptionContent.getData("investor",Set);
		//DPH
		//investor = CSRThirdParty::Find(descriptionContent,exceptionIfNotFound);
		const char * investorPartyId = data.dataSetValue().getData("partyId", Sequence)
			.dataSequenceValue().getNth(0).plainValue().getString();
		char investorRef[255];
		strcpy_s(investorRef, investorPartyId);
		MESS(Log::info, "investorRef = " << investorRef);
		investor = CSRThirdParty::GetCSRThirdPartyByReference(investorRef);
		MESS(Log::info, "investor ident = " << investor->GetIdent());
		if (investor)
		{
			accountKey->InvestorId = investor->GetIdent();
			MESS(Log::debug,"Investor is" << investor->GetIdent());
		}
		else
		{
			MESS(Log::warning,"Unable To Get Investor");
		}
	}

	//Depositary
	MESS(Log::info,"Try Get Depositary.");
	const CSRThirdParty* depositary;
	if (descriptionContent.has("depositary", exceptionIfNotFound))
	{
		const Data & data = descriptionContent.getData("depositary",Set);
		//DPH
		//depositary = CSRThirdParty::Find(data,exceptionIfNotFound);
		const char * depositaryPartyId = data.dataSetValue().getData("partyId", Sequence)
			.dataSequenceValue().getNth(0).plainValue().getString();
		char depositaryRef[255];
		strcpy_s(depositaryRef, depositaryPartyId);
		MESS(Log::info, "depositaryRef = " << depositaryRef);
		depositary = CSRThirdParty::GetCSRThirdPartyByReference(depositaryRef);
		if (depositary)
		{
			accountKey->DepositaryId = depositary->GetIdent();
			MESS(Log::debug,"Depositary is" << depositary->GetIdent());
		}
		else
		{
			MESS(Log::warning,"Unable To Get Depositary");
		}
	}

	//AccountName
	_STL::string accountName;
	MESS(Log::debug,"Try Get accountName.");
	if (descriptionContent.has("account", true))
	{
		const AttributeSet & attrs = descriptionContent.getAttributes("account");
		if (attrs.has("name"))
		{
			accountName = attrs.get("name").getString();
			accountKey->Name = accountName;
			MESS(Log::debug,"Account Name is "<< accountName);
		}
		else
		{
			MESS(Log::warning,"Unable To Get Account Name");
		}
	}
	END_LOG();
	return accountKey;
}


bool CFGFundInvestorAccountHandler::FindAccount(sophis::value::CSAmFundInvestorAccount& accountToFind,const AccountKey& accountKey, bool exceptionIfNotFound)
{
	BEGIN_LOG("FindAccount");
	bool found = false;

	//Find the Account
	_STL::set<long> accountIds;
	CSAmFundInvestorAccountSets::GetInstance()->GetAccountsFiltered(accountIds, accountKey.InvestorId, accountKey.DepositaryId, CSAmFundInvestorAccount::eAccountType::fiaInstrument);
	for (_STL::set<long>::const_iterator it=accountIds.begin(); it!=accountIds.end(); ++it)
	{
		CSAmFundInvestorAccount a = CSAmFundInvestorAccountSets::GetInstance()->GetAccount(*it);
		if (a.GetName() == accountKey.Name)
		{
			fKey = *it;
			accountToFind = a;
			found = true;
			MESS(Log::debug,"Account ALREADY EXIST :" <<accountKey.Name <<"; key= "<< fKey<<"; name= "<<accountToFind.GetName()<<"; investor= "<<accountToFind.GetInvestor()<<"; depositary= "<<accountToFind.GetDepositary()<<" is Found");
			break;
		}
	}
	
	//if We have not found the Account, We can Create a new one
	if (found == false)
	{
		MESS(Log::debug,"Account " <<accountKey.Name <<" not found");
		if (exceptionIfNotFound)
			throw NoSuchAccount();
	}
	END_LOG();
	return found;
}

void CFGFundInvestorAccountHandler::UpdateEntityFromDescription(sophis::value::CSAmFundInvestorAccount& accountToModify, const tools::dataModel::Data & data, bool exceptionIfNotFound)
{
	BEGIN_LOG("UpdateEntityFromDescription");
	const DataSet & descriptionContent = data.dataSetValue();

	_STL::string accountLibelle="";
	_STL::string comment = "";
	if (descriptionContent.has("account"))
	{
		MESS(Log::debug,"Xml description has an Account elem");
		const AttributeSet & attrs = descriptionContent.getAttributes("account");
		if (attrs.has("libelle"))
		{
			accountLibelle = attrs.get("libelle").getString();
			MESS(Log::debug,"Account libelle is : "<<accountLibelle);
		}
		const DataSet & accountDs = descriptionContent.lookup("account", true).dataSetValue();

		CSRDay dayFrom;
		CSRDay dayTo;
		if (accountDs.has("validFrom"))
		{
			dayFrom = accountDs.getPlainValue("validFrom", sophis::tools::dataModel::ValueKind::Date).getMacDate();
			MESS(Log::debug,"valid From is : "<<dayFrom.toLong());
		}
		if (accountDs.has("validTo"))
		{
			dayTo = accountDs.getPlainValue("validTo", sophis::tools::dataModel::ValueKind::Date).getMacDate();
			MESS(Log::debug,"valid To is : "<<dayTo.toLong());
		}

		comment =  FROM_STREAM(GetFormattedDate(dayFrom)<<" -> "<<GetFormattedDate(dayTo)<<" ; "<<accountLibelle);
		MESS(Log::debug,"Comment is : "<<comment);
		accountToModify.SetComments(comment);
	}
	END_LOG();
}


void CFGFundInvestorAccountHandler::CheckInvestorAndDepositary(long investorId, long depositaryId)
{
	BEGIN_LOG("CheckInvestorAndDepositary");
	if(CSAmFundInvestorsMgr::GetInstance(true)->GetInvestor(investorId, fInvestor) == false)
	{
		MESS(Log::warning,"No Such Investor existing : "<<investorId);
		MESS(Log::warning,"Creating it : "<<investorId);
		//fInvestor// =  CSAmFundInvestor();
		fInvestor.SetId(investorId);
		fInvestorModified = true;
	}
	
	CSAmFundInvestor::Depositaries depositaries = fInvestor.GetDepositaries();
	MESS(Log::debug,"INVESTOR : "<<investorId<<" has been FOUND");
	
	//If we do not find the depo in the the list of the investor
	_STL::pair<CSAmFundInvestor::Depositaries::iterator, bool> p =	depositaries.insert(depositaryId);
	if (p.second == true)
	{
		fInvestor.SetDepositaries(depositaries);
		fInvestorModified = true;
		MESS(Log::debug,"Add DEPOSITARY : "<<depositaryId<<" To Investor"<<investorId);
	}
	else
	{
		fInvestorModified = false;
		MESS(Log::debug,"DEPOSITARY : "<<depositaryId<<" is Already a depository of Investor"<<investorId);
	}
	
	END_LOG();
}

void CFGFundInvestorAccountHandler::CreateFromIdAndDescription(sophis::value::CSAmFundInvestorAccount& account, const tools::dataModel::Data & data, _STL::string accountName, long investorId, long depositaryId, bool exceptionIfNotFound)
{
	BEGIN_LOG("CreateFromIdAndDescription");
	const DataSet & descriptionContent = data.dataSetValue();

	//sophis::value::CSAmFundInvestorAccount* account = new CSAmFundInvestorAccount();
	//AccountType == here, always instrument
	account.SetAccountType(CSAmFundInvestorAccount::eAccountType::fiaInstrument);
	//SetName
	account.SetName(accountName);
	//Set Investor
	account.SetInvestor(investorId);
	//Set Depositary
	account.SetDepositary(depositaryId);

	_STL::string accountLibelle="";
	_STL::string comment = "";
	if (descriptionContent.has("account"))
	{
		MESS(Log::debug,"description has Account");
		const AttributeSet & attrs = descriptionContent.getAttributes("account");
		if (attrs.has("libelle"))
		{
			accountLibelle = attrs.get("libelle").getString();
		}
		const DataSet & accountDs =  descriptionContent.lookup("account", true).dataSetValue();

		CSRDay dayFrom;
		CSRDay dayTo;
		if (accountDs.has("validFrom", true))
			dayFrom = accountDs.getPlainValue("validFrom", sophis::tools::dataModel::ValueKind::Date).getMacDate();
		if (accountDs.has("validTo", true))
			dayTo = accountDs.getPlainValue("validTo", sophis::tools::dataModel::ValueKind::Date).getMacDate();

		comment =  FROM_STREAM(GetFormattedDate(dayFrom)<<" -> "<<GetFormattedDate(dayTo)<<" ; "<<accountLibelle);
		account.SetComments(comment);
		MESS(Log::debug,"Comment is : "<<comment);
	}
	END_LOG();
}

const _STL::string CFGFundInvestorAccountHandler::GetFormattedDate(CSRDay& date)
{
	char buff[256];
	sprintf_s(buff,"%#04d/%#02d/%#02d",date.fYear,date.fMonth,date.fDay);
	_STL::string s = buff;
	return s;
}