#ifndef __CFGFundInvestorAccountHandler_H__
	#define __CFGFundInvestorAccountHandler_H__

/*
** Includes
*/
#include "SphLLInc/misc/dataModel/FpmlEntityHandler.h"
#include "SphTools/dataModel/BasicDataSet.h"

#include "SphInc/value/kernel/SphAmFundInvestor.h"
#include "SphInc/value/kernel/SphAmFundInvestorAccount.h"


#define CFG_FIA_NS "www.sophis.net/fundInvestorAccount"

struct AccountKey
{
	_STL::string Name;
	long InvestorId;
	long DepositaryId;
};
/*
** Class
*/
class CFGFundInvestorAccountHandler : public sophis::misc::dataModel::FpmlEntityHandler
{
	public:
	/**
	* Trivial constructor
	*/
	CFGFundInvestorAccountHandler();

	/**
	* constructor with an existing sr
	*/
	CFGFundInvestorAccountHandler(long key);

	/**
	* Copy constructor
	*/
	CFGFundInvestorAccountHandler(const CFGFundInvestorAccountHandler& rhs);

	/**
	*
	*/
	virtual ~CFGFundInvestorAccountHandler();

	/**
	* clone() method from Cloneable interface
	*/ 
	CCTOR_CLONEABLE(CFGFundInvestorAccountHandler, misc::dataModel::FpmlEntityHandler)

public:
	/**
	* Locates the entity with a code.
	*/
	virtual void find(long code)
		throw (misc::dataModel::NoSuchEntity,tools::dataModel::DataModelException,sophisTools::base::ExceptionBase);

	/**
	* Locates the entity with the data containing the identifier.
	* I.E. for instruments :
	*  <sph:instrumentIdentifier> <-- data
	*		<sph:reference sph:modifiable="UniquePrioritary" sph:name="Reference">MSFT.O</sph:reference>
	*	</sph:instrumentIdentifier>
	*/
	virtual void find(const tools::dataModel::Data & refs)
		throw (misc::dataModel::NoSuchEntity,tools::dataModel::DataModelException,sophisTools::base::ExceptionBase);

	/** Creates a new entity with its description, 
	* or returns existing one if it exists.
	* @param description Data pointing to the description. ie for instruments:
	*	<sph:share> <-- data
	*		<sph:name>test fpml engine 24</sph:name>
	*		<sph:currency>EUR</sph:currency>
	*		<sph:outstandingNumber>668197570</sph:outstandingNumber>
	*		<sph:pricing>
	*			<sph:nature>A</sph:nature>
	*		</sph:pricing>
	*		<sph:accountingRef>--01-01</sph:accountingRef>
	*		<sph:tradingUnit>1.0000000000000000</sph:tradingUnit>
	*		<sph:country>EUR</sph:country>
	*		<sph:recordDate>Excluded</sph:recordDate>
	*		<sph:beta>1.0000000000000000</sph:beta>
	*	</sph:share>
	*/
	virtual void findOrCreate(const tools::dataModel::Data & description)
		throw (tools::dataModel::DataModelException,sophisTools::base::ExceptionBase);

	/** Creates a new entity with a description, or updates existing one if it exists.
	* @param description Data pointing to the description. See findOrCreate for sample
	*/
	virtual void updateOrCreate(const tools::dataModel::Data & description)
		throw (tools::dataModel::DataModelException,sophisTools::base::ExceptionBase);

	/** Creates a new entity with a description, or updates existing one if it exists.
	* @param description Data pointing to the description. See findOrCreate for sample
	*/
	virtual void create(const tools::dataModel::Data & description)
		throw (misc::dataModel::DuplicateEntity,tools::dataModel::DataModelException,sophisTools::base::ExceptionBase);

	/** Creates a new entity with a description, or check existing one if it exists.
	* @param description Data pointing to the description. See checkOrCreate for sample
	*
	* throw WrongCheckEntity if the entity exists but is different than the one provided.
	*/
	virtual void checkOrCreate(const tools::dataModel::Data & description)
#ifdef IGNORE_WRONGCHECKENTITY
		throw (WrongCheckQuotation,tools::dataModel::DataModelException,sophisTools::base::ExceptionBase);
#else
		throw (misc::dataModel::WrongCheckEntity,tools::dataModel::DataModelException,sophisTools::base::ExceptionBase);
#endif

	/**
	* Adds the identifiers of the entity to a sequence inside a given dataset
	*/
	virtual tools::dataModel::Data & addReference(tools::dataModel::DataSet & refs)
		throw (tools::dataModel::DataModelException,sophisTools::base::ExceptionBase);

	/**
	* Adds a description of the entity to a sequence inside a given dataset
	*/
	virtual tools::dataModel::Data & addDescription(tools::dataModel::DataSet & desc)
		throw (tools::dataModel::DataModelException,sophisTools::base::ExceptionBase);

	//DPH
	virtual tools::dataModel::Data & addDescriptionInSingleDataSet(tools::dataModel::DataSet & desc)
		throw (tools::dataModel::DataModelException, sophisTools::base::ExceptionBase);

	/**
	* commits the entity to the db and prepares the events for sending
	*/
	virtual void commit(tools::CSREventVector & ev)
		throw (sophisTools::base::ExceptionBase);

	/**
	* rolls back any modification made to the entity
	*/
	virtual void rollback()
		throw ();

private:
	void CheckInvestorAndDepositary(long investorId, long depositaryId);
	
	bool FindAccount(sophis::value::CSAmFundInvestorAccount& accountToFind,const AccountKey& accountKey, bool exceptionIfNotFound);

	AccountKey* GetKey(const tools::dataModel::Data & data, bool exceptionIfNotFound);

	void CreateFromIdAndDescription(sophis::value::CSAmFundInvestorAccount& account, const tools::dataModel::Data & data, _STL::string accountName, long investorId, long depositaryId, bool exceptionIfNotFound);

	void UpdateEntityFromDescription(sophis::value::CSAmFundInvestorAccount& accountToModify, const tools::dataModel::Data & data, bool exceptionIfNotFound);

	const _STL::string GetFormattedDate(CSRDay& date);

private:
	static const char * __CLASS__;
	//Id of the Current FundInvestorAccout
	long fKey;
	sophis::value::CSAmFundInvestor fInvestor;
	bool fInvestorModified;

	AccountKey* fAccountKey;
	sophis::value::CSAmFundInvestorAccount fEntity;
	
};
#endif //!__CFGFundInvestorAccountHandler_H__