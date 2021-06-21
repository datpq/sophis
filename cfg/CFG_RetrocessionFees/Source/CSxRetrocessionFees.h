#ifndef __CSxRetrocessionFees__H__
#define __CSxRetrocessionFees__H__

#include "SphInc/SphMacros.h"
#include __STL_INCLUDE_PATH(string)
#include __STL_INCLUDE_PATH(vector)

#include "CSxCustomMenu.h"


class CSxRetrocessionFees 
{
public:	
	
	CSxRetrocessionFees();
	~CSxRetrocessionFees();

	void GetData(_STL::vector<long> &fundList);	
	bool SaveData();

	void Compute(_STL::vector<long> &fundList, long startDate, long endDate);	

	struct SSxRetrocessionFeesData
	{
		long fThirdId;
		int fThirdType;
		long fFundCode;
		int fComputationMethod;
		long fStartDate;
		long fEndDate;
		double fCommissionRate;
		double fLevel1;
		double fRetrocessionRate1;
		double fLevel2;
		double fRetrocessionRate2;
		double fLevel3;
		double fRetrocessionRate3;
		double fLevel4;
		double fRetrocessionRate4;
		double fTVARate;		
	};

	struct SSxRetrocessionFeesDetailsData
	{
		long	fFundId;
		long	fBusinessPartnerId;
		int		fBusinessPartnerType;
		int		fComputationMethod;
		long	fStartDate;
		long	fEndDate;
		long	fNavDate;
		double	fNumberOfSharesForBusinessPartner;
		double	fTotalNumberOfShares;
		long	fNbDays;
		double	fNav;
		double	fFDG;
		double	fCDVM;
		double	fDDG;
		double	fMCL;
		double	fFundPromoterRetrocession;
		double	fPNB;

		SSxRetrocessionFeesDetailsData(long	fundId,
									long	businessPartnerId,
									int		businessPartnerType,
									int		computationMethod,
									long	startDate,
									long	endDate,
									long	navDate,
									double	numberOfSharesForBusinessPartner,
									double	totalNumberOfShares,
									long	nbDays,
									double	nav,
									double	FDG,
									double	CDVM,
									double	DDG,
									double	MCL,
									double	fundPromoterRetrocession,
									double	PNB
									)
		{
			fFundId = fundId;
			fBusinessPartnerId = businessPartnerId;
			fBusinessPartnerType = businessPartnerType;
			fComputationMethod = computationMethod;
			fStartDate = startDate;
			fEndDate = endDate;
			fNavDate = navDate;
			fNumberOfSharesForBusinessPartner = numberOfSharesForBusinessPartner;
			fTotalNumberOfShares = totalNumberOfShares;
			fNbDays = nbDays;
			fNav = nav;
			fFDG = FDG;
			fCDVM = CDVM;
			fDDG = DDG;
			fMCL = MCL;
			fFundPromoterRetrocession = fundPromoterRetrocession;
			fPNB = PNB;
		}
	};

	void ComputeFeesForOneData(SSxRetrocessionFeesData oneFeesData, long startDate, long endDate);
	void SaveResults(SSxRetrocessionFeesData &oneFeesData, long startDate, long endDate, double result, double averageAsset, long nbDays, 
						double retrocessionRate, double commissionRate, _STL::vector<SSxRetrocessionFeesDetailsData> &listOfRetroFeesDetails);

	double GetNbShares(long fundCode, long date);
	double GetNbSharesBusinessPartner(long fundCode, long businessPartnerId, long date);
	// double GetNAVPerShareForThisBusinessPartner(long fundCode, long businessPartnerId, long date);
	// double GetNAVPerShare(long fundCode, long date);
	double GetAssetValue(long fundCode, long date);

	double GetManagementFees(long fundCode, long startDate, long endDate, const char* name);
	
	double GetCouponRate(double amount, SSxRetrocessionFeesData oneFeesData, double &retrocessionRate);
	double GetCouponPerLevelRate(double amount, SSxRetrocessionFeesData oneFeesData);

	double GetFundPromoterRetroRate(SSxRetrocessionFeesData& oneFeesData, long startDate, long endDate);
	_STL::string GetBOStatusCondition(const _STL::string statusGroup);

	_STL::vector<SSxRetrocessionFeesData> fRetrocessionFeesList;

	struct RetrocessionFeesKey
	{
		RetrocessionFeesKey()
		{
			fThirdId = 0;
			fThirdType = 0;
			fFundId = 0;
		}

		RetrocessionFeesKey(long thirdId, int thirdType, long fundId)
		{
			fThirdId = thirdId;
			fThirdType = thirdType;
			fFundId = fundId;
		}

		long fThirdId;
		int  fThirdType;
		long fFundId;
	};

	class CompareRetrocessionFees
	{
	public:
		bool operator() (const RetrocessionFeesKey &a, const RetrocessionFeesKey &b) const
		{	
			if (a.fThirdId == b.fThirdId)
			{
				if (a.fThirdType == b.fThirdType)
					return a.fFundId < b.fFundId;
				else
					return a.fThirdType < b.fThirdType;
			}
			else
			{
				return a.fThirdId < b.fThirdId;
			}				
		}		
	};

protected:
	bool CheckData();	

	_STL::string fManagementFeesSteGestionBEName;
	_STL::string fManagementFeesCDVMBEName;
	_STL::string fManagementFeesDDGBEName;
	_STL::string fManagementFeesMCLBEName;
	_STL::string fRetrocessionPromoterFeesBEName;

private:
	static const char* __CLASS__;
};

#endif // __CSxRetrocessionFees__H__