// ***********************************************************************************************
//
// File:		VarianceSwap.h
// 
// ***********************************************************************************************
#pragma once

#include "SphInc/instrument/SphSwap.h"
#include "SphInc/gui/SphInstrumentDialog.h"
#include "SphLLInc/interface/SphBasicDataGUIExports.h"
#include "SphInc/gui/SphInstrumentDialog.h"
#include "SphInc/gui/SphEditList.h"
#include "SphInc/gui/SphCustomMenu.h"
BOOST_INCLUDE_BEGIN
#include BOOST_INCLUDE_PATH(unordered_map.hpp)
BOOST_INCLUDE_END
//#include "SphInc/instrument/SphInstrumentCompatibility.h"

namespace sophis	{
	namespace instrument	{
		class CSRIndexedLeg;
		class CSRCashFlowDiagram;
	}
	namespace finance {

enum VarianceSwapDialogElements
{
	eVarSwapType = 1,
	eVarSwapLowerBarrLabel,
	eVarSwapLowerBarr,
	eVarSwapUpperBarrLabel,
	eVarSwapUpperBarr, // 5
	eVarSwapBarrFixingsLabel,
	eVarSwapBarrFixings,
	eVarSwapDenomLabel,
	eVarSwapDenom,
	eVarSwapAdjust, //10
	eVarCapLabel,
	eVarCap,
	eVarSwapUserDenom,
	eVarSwapIntraDayDelta,
	eVarSwapWithDividends, //15
	eVarSwapResult_Realized,
	eVarSwapResult_Future,
	eVarSwapResult_OneDay,
	eVarSwapResult_Total,
	eVarSwapResult_Factor, //20
	eVarSwapSplitEditList,
	eVarSwapFixingColumn,
	eVarSwapFixegLegTheo,
	eVarSwapSmileIntegration,
	eVarSwapMDE, // 25
	eVarSwapLast
};

enum VarSwapType
{
	eStandard = 0,
	eDownside,
	eUpside,
	eInteriorCorridor
};

enum VarFixingType
{
	eBoth = 0,
	eNumerator,
	eDenominator
};

enum VarDenomType
{
	eBusinessDays = 0,
	eTriggeredBusinessDays,
	eUserDefined
};

enum SmileIntegration
{
	esiStandard = 0,
	esiVeryAccurate,
	esiFlatVolatility
};

struct SSMDEFixing
{
	long	date;
	char	fixingType[20];
	double	fixing;
	long	underlying;
};


class SOPHIS_FINANCE CSRVarianceSwap	: public virtual sophis::instrument::CSRSwap
{
	DECLARATION_SWAP(CSRVarianceSwap)

public:

	// if reconstruct is true, variables are initialized from the dialog
	CSRVarianceSwap(SW_Donne** sw, bool initialiseRecomputeAll, bool reconstruct);
	virtual ~CSRVarianceSwap();

protected:
	void Initialize(SW_Donne** sw, bool initialiseRecomputeAll, bool reconstruct);
public:
	virtual const sophis::finance::CSRMetaModel * GetDefaultMetaModel() const;

	//virtual void	RecomputeAll(const market_data::CSRMarketData& context);

	//virtual double	GetLegTheoreticalValue(const market_data::CSRMarketData& context, int which) const;
	//virtual double	GetTheoreticalValue(const market_data::CSRMarketData & param) const;

	virtual bool	UseMetaModel() const;

	/** Retrieves a description from a variance swap.
	* @param dataSet is the description to be filled..
	* @throw GeneralException this instrument is invalid : the description cannot be built from it.
	*/
	virtual void GetDescription(tools::dataModel::DataSet& dataSet) const;

	/** Initialize this variance swap from a description
	* @param dataSet is an objet that contains data wich which this instrument can be built.
	*        A DataSet can be built from an XML file for instance.
	* @throw GeneralException the dataSet is invalid : the swap cannot be built from it.
	*/
	virtual void UpdateFromDescription(const tools::dataModel::DataSet& dataSet);

	virtual	instrument::CSRCashFlowDiagram* new_CashFlowDiagram(const market_data::CSRMarketData& context, int i) const;
	virtual void GetSwapInformation(int index, const sophis::market_data::CSRMarketData& context, sophis::instrument::CSRCashFlowInformationList& arrayToFill) const;

	//virtual double	GetVega(const market_data::CSRMarketData& context,int which) const;
	//virtual bool	IsAVegaUnderlying(int whichUnderlying) const;
	//virtual	double	GetEquityGlobalVega(const market_data::CSRMarketData& context) const;

	virtual bool	HasAnAccruedCoupon() const;
	virtual bool	HasAnAccruedCoupon(int whichLeg) const;

	virtual	double	GetAccruedCoupon(long pariPassuDate, long accruedCouponDate) const;
	virtual	double	GetAccruedCoupon(int whichLeg, long pariPassuDate, long accruedCouponDate) const;
	virtual	int		GetAccrualPeriod(int whichLeg, long pariPassuDate,long accruedCouponDate) const;
	
	virtual double	GetCouponAmount(long				startDate, 
									long				endDate, 
									const market_data::CSRMarketData& context, 
									bool				discounted) const; 

	//virtual void	GetPriceDeltaGamma(	const market_data::CSRMarketData& context, 
	//									double				*price, 
	//									double				*delta, 
	//									double				*gamma, 
	//									int 				whichUnderlying) const;

	//virtual double	GetFirstDerivative(const market_data::CSRMarketData& context, int whichUnderlying) const;
	//virtual double	GetSecondDerivative(const market_data::CSRMarketData& context, int which1, int which2) const;

	virtual sophis::instrument::eVolatilityDependencyType GetVolatilityDependencyType() const;

	virtual bool	SpecialCoupon(int which_leg ) const;
	virtual double	GetTicketCoupon(int which_leg, int which_cash_flow, bool *end_of_day, portfolio::eTransactionType &businessEvent) const;

	virtual double	GetTicketCoupon(const flux_jambe &ptrFlux, int which) const;

	virtual	double	GetImpliedSpread(const sophis::market_data::CSRMarketData& context, double price, int which) const;

	virtual sophis::gui::CSRFitDialog*	new_FixingDialog(long fixingDate) const;
	virtual sophis::instrument::SSAlert*NewAlertList(long forecastDate, int *nb) const;

	virtual bool	GreeksForSupport() const;

	VarDenomType GetDenominatorType() const;
	double		 GetUserDenominator() const;
	double		 GetVarianceCap() const;
	bool		 GetUseIntraday() const;
	bool		 GetAdjustVarianceToDividends() const;

	double	GetVarianceDaysTriggered(const sophis::CSRComputationResults& results) const;
	//bool	GetFillComputedValues() const;

	bool	GetReinitNotional() const;
	void	SetReinitNotional(bool b) const;

	bool	GetUseIntraDayDelta() const;
	bool	GetDividendsInThePast() const;

	bool	GetAdjust() const;
	long	GetStrikeCount() const;

	const _STL::map<long,_STL::pair<double, double> >& GetSplitList() const;

	virtual	bool	SplitDone(long splitDate, long underlyingCode, double factor) const;

	//void	SetFillComputedValues(bool b) const;//fFillComputedValues is mutable !!! 
												//the method is const in order to be accessed in CSRHestonMetaModel::RecomputeAll()

	//virtual void			GetCalculationData(sophis::tools::CSRArchive & archive) const;
	//virtual void			SetCalculationData (const sophis::tools::CSRArchive & archive);

	//computation method returns the variance and the expectation on a dayly basis for a 252 year basis
	//virtual double GetRealizedVariance(const market_data::CSRMarketData &context, long startDate, long endDate, double& expectation, double & lastFixing, bool &lastFixingFixed, long &histoCount, long& triggerCount, long underlying) const;

	// returns log(spotNum/spotDen)^2 according to the swap type and fixing type
	// used in the computation of realized and one-day variance
	// @param triggered is always true for a standard variance swap
	//	for a corridor variance swap, triggered is true iff the condition
	//  to take the variance into account is true
	//virtual double GetInstantaneousVariance(double spotNum,
	//										double spotDen,
	//										double splitNum,
	//										double splitDen,
	//										bool   &triggered) const;

	//double GetTheoVarSwapPayoffCorrection(	long	equityCode,
	//										double	startDate,
	//										double	endDate) const;

	// @param dividendArray is filled with the ex-dividend dates and dividend values
	// between startDate and endDate
	void GetDividendArray(	const instrument::CSRInstrument											*instrument,
							const market_data::CSRMarketData										&context,
							long																	startDate,
							long																	endDate,
							bool																	fromCorporateActions, // past dividends should be retrieved from CAs
							_STL::map<long, _STL::pair<market_data::eDividendValueType, double> >	&dividendArray) const;


	//virtual double	GetCashDividendJumpSize(double	cashDividend,
	//										double	forward,
	//										double  locVol,
	//										double	maturity) const;


	/*virtual*/ double GetParkinsonNumber() const;

	virtual void	AddMDEDate(long mdeDate);

	virtual void	AddMDEFixing(long mdeDate, double newFixing, long underlyingCode = 0);


	
	//computation method returns the variance and the expectation for the given maturity
	//virtual double	GetVolatilityCoupon(long								equityCode,
	//									long								startDate,
	//									long								endDate,
	//									double&								expectation,
	//									const market_data::CSRMarketData&	context) const;
	
	//virtual	double GetVarianceLegValue(const sophis::instrument::CSRIndexedLeg &leg, const market_data::CSRMarketData &context) const;

	//bool GetVarianceLegValueFromMetaModel(const market_data::CSRMarketData& context, double& value) const;

public:
	typedef _STL::vector<_STL::pair<long, double> >	fixingVect;
	virtual void GetUnderlyingFixing(	long								underlying,
										long								startDate,
										long								endDate,
										fixingVect							&fixingVector,
										bool								&lastFixingFixed,
										double								spot_missing,
										const market_data::CSRMarketData&	context) const;
// evaluates the number of business days spent between the barriers
	// between startDate and endDate
	//virtual double ComputeNumberOfDaysInsideCorridor(	long								equityCode,
	//													long								startDate,
	//													long								endDate,
	//													const market_data::CSRMarketData	&context) const;

	// computes the adjustment necessary when fixing type is "numerator only" or "denominator only"
	// and preference fAdjust is equal to true
	//virtual double GetAdjustment(	long							equityCode,
	//								double							barrierMin,
	//								double							barrierMax,
	//								double							forward0,
	//								double							forward,
	//								double							startDate,
	//								double							maturity,
	//								const market_data::CSRMarketData&context) const;

	// when the swap is not protected against dividends,
	// this is a local adjustment of the jump risk due to dividends
	//virtual double	GetDividendJumps(	const market_data::CSRMarketData	&context, 
	//									long								equityCode,
	//									long								startDate,
	//									long								endDate) const;

	virtual bool IsAReplaceFixingDate (long underlying, long date) const;

	virtual double GetReplacedFixing (long underlying, long date) const;

//protected:

	// loads the calculation data from the CSRVarianceSwapDialog popup
	void   Construct();

	//double GetIntegralCall(	long equityCode,
	//						double strikeMin,
	//						double forward,
	//						double  startDate,
	//						double maturity,
	//						const market_data::CSRMarketData& context,
	//						double strikeMax = NOTDEFINED) const;
	//
	//double GetIntegralPut(	long equityCode,
	//						double strikeMax,
	//						double forward,
	//						double  startDate,
	//						double maturity,
	//						const market_data::CSRMarketData& context,
	//						double strikeMin = NOTDEFINED) const;

	//virtual double GetWeight(	int i,
	//							double strike,
	//							double strikeMoins,
	//							double strikePlus,
	//							double forward,
	//							bool	callIntegral) const;

	//double TimeIntegral(	long								equityCode,
	//						long								startDate,
	//						long								endDate,
	//						const market_data::CSRMarketData&	context,
	//						double								lowerBarrier,
	//						double								upperBarrier) const;

public:
	inline double	GetCumulativeSplit() const {return fCumulativeSplit;};
	inline void	SetCumulativeSplit(double d) const { fCumulativeSplit = d;};
	inline double	GetLowerBarrier() const {return fLowerBarrier;};
	inline double	GetUpperBarrier() const {return fUpperBarrier;};

	inline VarSwapType		GetVarSwapType() const {return fVarSwapType;};
	inline VarFixingType	GetVarFixingType() const {return fFixingType;};

	inline void SetVarFixingColumn(long tag) {fFixingColumnTag = tag;};
	inline void SetVarianceCap(double cap) {fVarianceCap = cap;};
	inline void SetSmileIntegration(SmileIntegration prec) {fSmileIntegration = prec;};

	inline SmileIntegration GetSmileIntegration() const { return fSmileIntegration; };
	
	inline bool				GetForceTheoVarSwapPayoff() const { return fForceTheoVarSwapPayoff; };
	inline void				SetForceTheoVarSwapPayoff( bool onoff ) { fForceTheoVarSwapPayoff = onoff; };

	sophis::static_data::CSRCalendar* GetLocalCalendar(const instrument::CSRInstrument*) const;

	long											fStrikeCount;
	VarSwapType										fVarSwapType;
	double											fLowerBarrier;
	double											fUpperBarrier;
	VarFixingType									fFixingType;
	VarDenomType									fDenomType;
	bool											fAdjust;
	double											fVarianceCap;
	bool											fUseIntraDayDelta;
	bool											fDividendsInThePast;
	double											fUserDenom;
	_STL::map<long, _STL::pair<double, double> >	fSplitList;
	_STL::vector<long>								fMDEIgnoreList;
	_STL::map<_STL::pair<long, long>, double>       fMDEReplaceList; // the key is the pair (date, UnderlyingCode) and the data is the Fixing value
	mutable double									fCumulativeSplit;
	long											fFixingColumnTag;

	SmileIntegration								fSmileIntegration;

	// when denominator preference is "triggered business days"
	mutable bool					fReinitNotional;

	// For volatility indices, bool to force the pref THEO_VAR_SWAP_PAYOFF
	bool							fForceTheoVarSwapPayoff;

private:
	//DEPRECATED_RESULTS double	GetRealizedVariance() const;
	//DEPRECATED_RESULTS double	GetFutureVariance() const;
	//DEPRECATED_RESULTS double	GetOneDayVariance() const;
	//DEPRECATED_RESULTS double	GetfFactorVariance() const;
	//DEPRECATED_RESULTS double	GetFixedLegTheo() const;
	//DEPRECATED_RESULTS double	GetVarianceDays() const;
	//DEPRECATED_RESULTS double	GetVarianceRemainingDays() const;
	//DEPRECATED_RESULTS double	GetTradingDaysCount() const;
	//DEPRECATED_RESULTS void	SetTradingDaysCount(double d) const;
	//DEPRECATED_RESULTS double	GetTriggeredDaysCount() const;
	//DEPRECATED_RESULTS void	SetTriggeredDaysCount(double d) const;
	//DEPRECATED_RESULTS void	SetRealizedVariance(double val) const;
	//DEPRECATED_RESULTS void	SetFutureVariance(double val) const;
	//DEPRECATED_RESULTS void	SetOneDayVariance(double val) const;
	//DEPRECATED_RESULTS void	SetFactorVariance(double val) const;
	//DEPRECATED_RESULTS void	SetFixedLegTheo(double val) const;
	//DEPRECATED_RESULTS void	SetVarianceDays(double val) const;
	//DEPRECATED_RESULTS void	SetVarianceRemainingDays(double val) const;

	//DEPRECATED_RESULTS mutable double					fRealizedVariance;
	//DEPRECATED_RESULTS mutable double					fFutureVariance;
	//DEPRECATED_RESULTS mutable double					fOneDayVariance;
	//DEPRECATED_RESULTS mutable double					fFactorVariance;
	//DEPRECATED_RESULTS mutable double					fFixedLegTheo;
	//DEPRECATED_RESULTS mutable double					fVarianceDays;
	//DEPRECATED_RESULTS mutable double					fVarianceRemainingDays;
	//DEPRECATED_RESULTS  mutable double					fTradingDaysCount;
	//DEPRECATED_RESULTS mutable double					fTriggeredDaysCount;
};

class SOPHIS_FINANCE CSRVarianceSwapDialog: public sophis::gui::CSRInstrumentDialog
{
public:
	DECLARATION_INSTRUMENT_DIALOG(CSRVarianceSwapDialog)

	virtual void	OpenAfterInit(void);
	virtual	void	ElementValidation(int EAId_Modified);

	virtual void BeforeRecompute(const instrument::CSRInstrument &security);
	virtual void AfterRecompute(const instrument::CSRInstrument &security, const sophis::CSRComputationResults& results);
};

class CSRSplitList : public sophis::gui::CSREditList
{
public:
	CSRSplitList(sophis::gui::CSRFitDialog *dialog, int	ERId_List);
	virtual ~CSRSplitList() {};

	void SaveLine(int lineIndex);
	void SetCalendar(const sophis::static_data::CSRCalendar* calendar);
	virtual void Describe(sophis::tools::dataModel::DataSet& data_set, const sophis::gui::SSDataDescribed &dataPtr) const;

protected:

	INHERITED(CSREditList)
	const sophis::static_data::CSRCalendar* fCalendar;
};

class CSRMDEList : public sophis::gui::CSREditList
{
public:
	CSRMDEList(sophis::gui::CSRFitDialog *dialog, int ERId_List, bool displayUnderlying = false);
	virtual ~CSRMDEList() {};

	void SaveLine(int lineIndex);
	void SetCalendar(const sophis::static_data::CSRCalendar* calendar);
	virtual void Describe(sophis::tools::dataModel::DataSet& data_set, const sophis::gui::SSDataDescribed &dataPtr) const;

protected:

	INHERITED(CSREditList)
		const sophis::static_data::CSRCalendar* fCalendar;
};

	}
}