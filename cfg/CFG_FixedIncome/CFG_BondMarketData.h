#ifndef __CFG_BondMarketData_H__
#define __CFG_BondMarketData_H__

#include "SphInc/market_data/SphCreditRisk.h"
#include "SphInc/scenario/SphMarketDataOverloader.h"
#include "SphInc/instrument/SphDebtInstrument.h"
#include "SphInc/instrument/SphSwap.h"
#include "SphInc/static_data/SphDayCountBasis.h"
#include "SphInc/static_data/SphYieldCalculation.h"
#include "SphInc/instrument/SphOption.h"
#include "SphInc/market_data/SphYieldCurve.h"
#include "SphInc/scenario/SphOptimiser.h"
#include "SphInc/scenario/SphMaturityScenario.h"
#include "SphTools/SphExceptions.h"
#include "SphInc/static_data/SphCurrency.h"
#include "SphInc/static_data/SphYieldCurveFamily.h"

#if (defined(WIN32)||defined(_WIN64))
#	pragma warning (push)
#	pragma warning (disable : 4251)
#endif

class CSRAssetSwapSpreadManager;
class CSRCreditDefaultSwapCurveShockSpread;

//DPH
using namespace sophis::DAL;

enum typeSophisParameter	
{
	spCoutPretEmprunt = ipScenarioType + 1,
	spListeDividende,
	spVersionDividende,
	spPrepaCoxParametre,
	spAmountCoupon,
	spASWOptimizerID,
	spIsCreditSensMarketDataFromSophis,
	spForceDoNotUseSwapUnderlyingFixing,
	spMTMZCSpread,
	spCreditMarket,
	spNoCreditRisk
};

struct SScoupon_amount
{
	long	start_date;
	long	end_date;
	bool	discount;
	long	sicovam;
};

namespace sophis
{
	namespace market_data
	{
		class CSRMonetaryRateShock;


class SOPHIS_FIT CSRStaticYieldCalc
{
public:
	CSRStaticYieldCalc(	long currency, long currencyRef ) : fCurrency(currency), fCurrencyRef(currencyRef), fFrozenRate(NOTDEFINED)
	{
	};
	
	CSRStaticYieldCalc(const CSRStaticYieldCalc& yieldCalc);
	virtual CSRStaticYieldCalc* Clone() const;

	virtual ~CSRStaticYieldCalc(){};


	// f must be decreasing
	virtual double	f(const CSRMarketData &context){return 0.;};
	virtual double	fShocked(const CSRMarketData &context){return f(context);};


	virtual double	f(double x, const CSRMarketData &context);
	double	GetFrozenRate() const;

	virtual double GetReferenceStaticYield(	const CSRMarketData	&context, 
											const CSRMarketData	&optionContext,
											double				minimumYield,
											double				maximumYield,
											double				prec);

	virtual double	GetReferenceDateStaticYield(const CSRMarketData& context) {return 0.;};

	double	GetUniversalReferenceDateStaticYield(const CSRMarketData& context);

	long	GetCurrency() const {return fCurrency;};
	long	GetCurrencyRef() const {return fCurrencyRef;};

protected:
	long			fCurrency;
	long			fCurrencyRef;
	double			fFrozenRate;
};

class SOPHIS_FIT CSRStaticYieldCalcStaticSpread : public CSRStaticYieldCalc
{
public:
	CSRStaticYieldCalcStaticSpread(long currency, long maturity);

	CSRStaticYieldCalcStaticSpread(const CSRStaticYieldCalcStaticSpread& yieldCalc);
	virtual CSRStaticYieldCalc* Clone() const;

	virtual double GetReferenceStaticYield(	const CSRMarketData	&context, 
											const CSRMarketData	&optionContext,
											double				minimumYield,
											double				maximumYield,
											double				prec);

	virtual double	f(const CSRMarketData &context);

	virtual double	GetReferenceDateStaticYield(const CSRMarketData& context);

protected:
	long fMaturity;
};

class SOPHIS_FIT CSRYieldCurveStaticSpread : public sophis::market_data::CSRYieldCurve
{
public:
	CSRYieldCurveStaticSpread(	const sophis::market_data::CSRYieldCurve	*curve,
								const CSRMarketData							*basicContext,
								const CSRMarketData							*optionContext,
								const CSRStaticYieldCalc					*yieldCalc,
								sophis::static_data::eYieldCalculationType	mode,
							    sophis::static_data::eDayCountBasisType		basis,
							    sophis::instrument::eSwapPeriodicityType	frequency,
								bool										throwException,
								long										start_date,
								long										currency);

	virtual double		CompoundFactor(double timeToMaturity, double overRate=0) const;

	double				GetEquivalentYearCount(double startDate, double endDate) const;
	double				GetZeroCouponDerivative(long startDate, long endDate) const;
	double				GetZeroCouponSecondDerivative(long startDate, long endDate) const;
	double				GetZeroCouponDerivative(double time) const;
	double				GetZeroCouponSecondDerivative(double time) const;

	double				GetReferenceInstrumenYield(double overRate) const;

	void				SetFrozenRate(double frozenRate){fFrozenRate = frozenRate;};
	double				GetFrozenRate() const			{return fFrozenRate;};

	void				SetDayCountCalculation(const sophis::static_data::SSDayCountCalculation &dccData);

	enum eMorphismType{
		mtAdditiveToMultiplicative,// means that f(x+y) = f(x)*f(y) (f is the compound factor) such that we may decompose the discount factors as a product from forward discount factors
		mtAbsolute,// means that f(x+y) = f(x) = f(y)
		mtAdditiveMinusOne, // means that f(x+y) = 1+ (f(x)-1 + f(y)-1) (f is the compound factor)
		mtComplex // no straight morphism
	};
	eMorphismType		GetCompoundFactorMorphismType() const;
protected:
	sophis::static_data::eYieldCalculationType		fMode;
	sophis::static_data::eDayCountBasisType			fBasis;
	sophis::instrument::eSwapPeriodicityType		fFrequency;
	double											fFrequencyDouble;
	static_data::SSDayCountCalculation				fDccData;
	mutable double									fFrozenRate;
	mutable _STL::map<double, double>				fReferenceValues;
	const CSRMarketData								*fBasicContext;
	const CSRMarketData								*fOptionContext;
	const sophis::market_data::CSRYieldCurve		*fBasicCurve; 
	const CSRStaticYieldCalc						*fYieldCalc;
	bool											fThrowException;
};

class SOPHIS_FIT CSRMarketDataYieldCurveStaticSpread : public scenario::CSRMarketDataOverloader
{
public:
	CSRMarketDataYieldCurveStaticSpread(const CSRMarketData	&context);
	CSRMarketDataYieldCurveStaticSpread(const CSRMarketData	&context, bool useCredit);

	~CSRMarketDataYieldCurveStaticSpread();

	void	InitialiseConstructor(	CSRStaticYieldCalc								*yieldCalc,
									sophis::static_data::eYieldCalculationType		mode,
									sophis::static_data::eDayCountBasisType			basis,
									sophis::instrument::eSwapPeriodicityType		frequency,
									bool											throwException,
									long											currency,
									long											start_date_curve);

	virtual	const sophis::market_data::CSRYieldCurve	*GetCSRYieldCurve(long currency) const;

	void	GetData(sophis::static_data::eYieldCalculationType	&mode,
					sophis::static_data::eDayCountBasisType		&basis,
					sophis::instrument::eSwapPeriodicityType	&frequency,
					long										&currency,
					long										&currencyRef) const;

	virtual	double GetInstrumentSpread(long instrumentCode, long maturity) const;
	//DPH
	/*
	virtual const sophis::market_data::CSRCreditRisk* GetCSRCreditRisk(long issuerCode, long currencyCode) const;
	virtual const CSRCreditRisk	*GetCSRCreditRiskWithDefault(	long								issuerCode, 
																long								currencyCode, 
																bool								withDefault) const;
	*/

protected:
	typedef _STL::map<_STL::pair<long,long> , const sophis::market_data::CSRYieldCurve*> MapCurveIdStartData;
	mutable MapCurveIdStartData	fCurveMap;
	sophis::static_data::eYieldCalculationType							fMode;
	sophis::static_data::eDayCountBasisType								fBasis;
	sophis::instrument::eSwapPeriodicityType							fFrequency;
	double																fFrequencyDouble;
	CSRStaticYieldCalc													*fYieldCalc;
	long																fCurrency;
	bool																fThrowException;
	long																fCurrencyRef;
	long																fStartDate;
	bool																fUseCredit;
};

class SOPHIS_FIT CSRCreditDefaultSwapCurveSpread : public CSRCreditDefaultSwapCurve //SOPHIS_FIT
{
public:
	CSRCreditDefaultSwapCurveSpread(long yieldCurveStartDate);

	virtual ~CSRCreditDefaultSwapCurveSpread();

	virtual CSRCreditDefaultSwapCurve*  Clone() const;

	virtual double GetBaseRate(double				maturityDays,
							   double				recovery_rate,
							   long					seniority,
							   eCreditRiskCurve		creditRiskCurve,
							   long					defaultEvent,
							   const CSRMarketData& context) const;

	virtual double GetBaseZCFactor(	double					maturityDays,
									const CSRMarketData&	context) const;

	virtual SSRecoveryRate* GetRecoveryRate(long seniority, long defaultEvent) const;
	
	double	GetDefaultProbaInverse(	double											spread, 
									double											maturity, 
									double											zcBase, 
									double											frequency,
									const sophis::static_data::CSRDayCountBasis		*basis,
									const sophis::static_data::CSRYieldCalculation	*calc,
									const sophis::static_data::SSDayCountCalculation &dccData,
									const CSRMarketData								&context) const;

	double GetInstrumentProbaInverse(double						maturity,
									 double						zcBase, 
									 const CSRMarketData		&context) const;


	double	GetRisklessZeroCoupon(double maturity, const CSRMarketData& context) const;
	double	GetMaturityForShock(double maturityDays, const CSRMarketData& context) const;

	double BumpBaseRate(	double				 rate,
							double				 maturityDays,
							double				 recovery_rate,
							long				 seniority,
							eCreditRiskCurve	 creditRiskCurve,
							long				 defaultEvent,
							const CSRMarketData& context) const;

	virtual bool IsBumpToBeApplied(	long seniority,
									long defaultEvent,
									long seniorityReference,
									long defaultEventReference) const;

	double SpreadToZeroCoupon(	double											value, 
								double											maturity, 
								double											frequency,
								const sophis::static_data::CSRDayCountBasis		*basis,
								const sophis::static_data::CSRYieldCalculation	*calc,
								const sophis::static_data::SSDayCountCalculation &dccData) const;

	double ZeroCouponToSpread(	double											value, 
								double											maturity, 
								double											frequency,
								const sophis::static_data::CSRDayCountBasis		*basis,
								const sophis::static_data::CSRYieldCalculation	*calc,
								const sophis::static_data::SSDayCountCalculation &dccData) const;

	static double SpreadToZeroCoupon(	double											value, 
										double											startDate,
										double											endDate,
										double											frequency,
										const sophis::static_data::CSRDayCountBasis		*basis,
										const sophis::static_data::CSRYieldCalculation	*calc,
										const sophis::static_data::SSDayCountCalculation &dccData) ;

	static double ZeroCouponToSpread(	double											value, 
										double											startDate,
										double											endDate,
										double											frequency,
										const sophis::static_data::CSRDayCountBasis		*basis,
										const sophis::static_data::CSRYieldCalculation	*calc,
										const sophis::static_data::SSDayCountCalculation &dccData) ;

	virtual CSRCreditDefaultSwapCurveShock* new_CSRCreditDefaultSwapCurveShock(	double											recovery_rate,
																				long											seniority,
																				long											defaultEvent,
																				const sophis::static_data::CSRDayCountBasis		*basisPtr,
																				const sophis::static_data::CSRYieldCalculation	*modePtr,
																				bool												flag = false) const;
	mutable _STL::vector<const CSRCreditDefaultSwapCurveShockSpread*> fShockVect;

	double	GetSpread0() const {return fSpread0;};
	void	SetSpread0(double spread0) {fSpread0 = spread0;};
	
	void	ResetBaseCreditRisk();
	void	SetBaseCreditRisk(const CSRCreditRisk* cr);

protected:
	const	sophis::static_data::CSRDayCountBasis		*fBasisActAct;
	const   sophis::static_data::CSRYieldCalculation	*fCalcLinear;
	mutable CSRCreditRisk								*fBaseCreditRisk; 
	double												fSpread0;
	mutable sophis::market_data::CSRYieldCurve			*fRiskyYieldCurve;
	mutable sophis::market_data::CSRMonetaryRateShock	*fMonetaryShock;
	long fYieldCurveStartDate;

	void	SetShock(const CSRCreditDefaultSwapCurveShockSpread* shock);
};

class SOPHIS_FIT CSRCreditRiskSpread : public CSRCreditRisk
{
public:
	CSRCreditRiskSpread(const SSCreditRiskParameters &parameters);
	CSRCreditRiskSpread();
	~CSRCreditRiskSpread();

	void	InitialiseConstructor(	CSRStaticYieldCalc									*creditRiskCalc,
									double												spread,
									sophis::instrument::eSpreadType						spreadType,
									sophis::static_data::eYieldCalculationType			mode,
									sophis::static_data::eDayCountBasisType				basis,
									const sophis::static_data::SSDayCountCalculation	&dccData,
									long												currencyOrFamily,
									long												yieldCurveStartDate,
									long												instrumentCode = 0);

	virtual void			SetCreditDefaultSwapCurve();
	virtual void			SetData(const SSCreditRiskData& data, const CSRMarketData& context);
	virtual void			GetData(SSCreditRiskData& data, SSCreditRiskParameters& parameters) const;

	virtual double			GetCreditLegValue(	double					today,
												double					startDate, 
												double					endDate,
												long					discountCurrency,
												const CSRMarketData&	context,
												double					recoveryRate,
											  long					seniorityForCurve,
											  eCreditRiskCurve		creditRiskCurve,
											  long					defaultEvent,
											  const market_data::CSRCreditRisk*	creditRisk) const;

	virtual CSRCreditRisk	*Clone() const;

	const CSRStaticYieldCalc*							GetCreditRiskCalc() const {return fCreditRiskCalc;};
	sophis::instrument::eSpreadType						GetSpreadType() const	{return fSpreadType;};
	const	sophis::static_data::CSRDayCountBasis*		GetBasisOption() const	{return fBasisOption;};
	const   sophis::static_data::CSRYieldCalculation*	GetModeOption() const	{return fModeOption;};
	double												GetFreqOption() const	{return fFreqOption;};
	long												GetCurrencyOrFamily() const {return fCurrencyOrFamily;};
	const static_data::SSDayCountCalculation	&		GetDayCountCalculation() const { return fDccData; };
	long												GetInstrumentCode() const {return fInstrumentCode;};

protected:
	CSRStaticYieldCalc*									fCreditRiskCalc;
	sophis::instrument::eSpreadType						fSpreadType;
	const	sophis::static_data::CSRDayCountBasis		*fBasisOption;
	const   sophis::static_data::CSRYieldCalculation	*fModeOption;
	double												fFreqOption;
	sophis::static_data::eYieldCalculationType			fMode;
	sophis::static_data::eDayCountBasisType				fBasis;
	double												fInstSpread;
	long												fCurrencyOrFamily;
	long												fYieldCurveStartDate;
	sophis::static_data::SSDayCountCalculation			fDccData;
	long												fInstrumentCode;
};

	}
}

class CSRYieldCurveMarketData : public CSRMarketDataOverloader
{
public:
	CSRYieldCurveMarketData(const CSRMarketData& context, const CSRYieldCurve* yieldCurve, long currency)
		: CSRMarketDataOverloader(context), fBasicCurve(yieldCurve), fCurrency(currency) {};

	virtual	const CSRYieldCurve* GetCSRYieldCurve(long currency) const
		{ return fBasicCurve; };

private:
	long					fCurrency;
	const CSRYieldCurve*	fBasicCurve;
};


#if (defined(WIN32)||defined(_WIN64))
#	pragma warning (pop)
#endif

#endif //__CFG_BondMarketData_H__

