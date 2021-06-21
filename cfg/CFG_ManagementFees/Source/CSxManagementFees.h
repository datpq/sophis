
#ifndef _CSxManagementFees_H_
#define _CSxManagementFees_H_


/**
* System includes
*/

/**
* Application includes
*/
#include "SphInc/SphMacros.h"

/* RISQUE Toolkit */
#include __STL_INCLUDE_PATH(vector)

/* VALUE Toolkit */
#include "SphInc/value/kernel/SphFund.h"
#include "SphInc/value/kernel/SphFundFees.h"
#include "SphInc/value/kernel/SphAmManagementFees.h"

#include "CFG_ManagementFeesExports.h"


class CSxManagementFees : public virtual sophis::value::CSAMFundManagementFees
{
public:		

	CSxManagementFees();
	sophis::value::CSAMFundFees* Clone() const;
	void Initialise();
	void Initialise(const sophis::value::CSAMFundFees* fees); 
	static const char * GetStaticTemplateName() { return "CFGManagement"; }
	inline const char * GetTemplateName() const { return GetStaticTemplateName(); }	// overload		


	//double ComputeSincePeriodStart(const sophis::value::CSAMFund * fund, long date, double nav, double nbshares, double totalnav) const;	// overload	

	// Usually calls ComputeSincePeriodStart and GetAlreadyComputedFees
	virtual double Compute(const sophis::value::CSAMFund * fund, long date, double nav, double nbshares, double totalnav) const;			

	//DPH
	//virtual short ReadCustomInfos(long id);	// overload
	virtual sophis::sql::errorCode ReadCustomInfos(long id);	// overload

	//DPH
	//virtual short WriteCustomInfos(long* id, const sophis::value::CSAMFund * fund) const;	// overload
	virtual sophis::sql::errorCode WriteCustomInfos(long* id, const sophis::value::CSAMFund * fund) const;	// overload

	//DPH
	//virtual short UpdateCustomInfos(const sophis::value::CSAMFund * fund) const;	// overload	
	virtual sophis::sql::errorCode UpdateCustomInfos(const sophis::value::CSAMFund * fund) const;	// overload	
	
	enum eModeChoice
	{
		acStandard=1,
		acPerLevel
	};

	class Page;
	friend class Page;

protected:	

	// Day to day fees are not based on the ComputeSincePeriodStart
	// This method is required to overload ComputeFeesInPeriod and compute the Per Level
	/* Not Virtual */ double ComputeDayToDayFees(const sophis::value::CSAMFund * fund, long date, double nav, double nbshares, double totalnav) const;
	
	// Compute the management fee between startDate end endDate and the Per Level fee
	/* Not Virtual */ double ComputeFeesInPeriod(const sophis::value::CSAMFund * fund, long date, long startDate, long endDate, double baseAmount) const;

	double ComputePerLevelFeesInPeriod(const sophis::value::CSAMFund * fund, long date, long startDate, long endDate, double baseAmount) const;

	void Load(int i);
	
	struct SSxRatePerLevel
	{
		double fLevel;		
		double fRate;
	};
	
	static bool CompareSSxRatePerLevelElements(SSxRatePerLevel a, SSxRatePerLevel b);
	
	eModeChoice	fModeChoice;
	_STL::vector<SSxRatePerLevel> fRatesPerLevelList;

private:
	static const char* __CLASS__;	
};


#endif // _CSxManagementFees_H_