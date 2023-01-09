#pragma once

/*
** Includes
*/
// standard
#include "SphInc/portfolio/SphPortfolioColumn.h"
#include "SphInc/portfolio/SphExtraction.h"
#include "SphInc\value\kernel\SphAM_UCITS_Tools.h"

extern char const __ALLOTMENT_CERTIFICATE__[];
extern char const __ALLOTMENT_CDS__[];
extern char const __ALLOTMENT_CDX__[];
extern char const __ALLOTMENT_COCO__[];
extern char const __ALLOTMENT_CB__[];
extern char const __ALLOTMENT_IR_DERIVATIVE__[];
extern char const __ALLOTMENT_NDF__[];
extern char const __ALLOTMENT_OTC_FX_OPTION_SINGLE__[];
extern char const __ALLOTMENT_TRS_EQUITY_SINGLE__[];
extern char const __ALLOTMENT_TRS_EQUITY_BASKET__[];
extern char const __ALLOTMENT_TRS_FIXED_INCOME_SINGLE__[];
extern char const __ALLOTMENT_FX_FORWARD__[];
extern char const __ALLOTMENT_LISTED_OPTION__[];

/** I_UCITS_Calculator **/
template<char const * ALLOTMENT>
class CSxUCIT_Calculator : public sophis::portfolio::I_UCITS_Calculator
{
	//------------------------------------ PUBLIC ---------------------------------
public:
	virtual double calculate(const sophis::instrument::CSRInstrument *instr, const CSRPosition* amPosition);

	//------------------------------------ private ---------------------------------
private:
	static const char * __CLASS__;
};

/** UCITS_Filter **/
class CSxUCIT_Filter : public sophis::portfolio::UCITS_Filter
{

	//------------------------------------ PUBLIC ---------------------------------
public:
	CSxUCIT_Filter(char intrType, const char* allotment, const char* productType="");

	virtual bool match(const sophis::instrument::CSRInstrument *instr);
	//------------------------------------ private ---------------------------------

private:
	long allotment_;
	char instrumentType_;		// for debug
	std::string productType_;

	// static std::vector<long> _Allotments;
};

static long LoadAllotmentIdFromDB(std::string allotmentName);


// static void LoadAllotmentList(std::string sql, std::vector<long> &AllotmentVec);