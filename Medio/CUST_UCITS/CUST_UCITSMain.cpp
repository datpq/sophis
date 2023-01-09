/*
** Includes
*/
// standard
#include "SphInc/SphMacros.h"
#include "SphTools/SphCommon.h"
#include "SphInc/SphPreference.h"
///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)
#include "version/CUST_UCITSVersion.h"
#include "CSxUCITSCommitment.h"
#include "../MediolanumConstants.h"
#include "CSxCheapestToDeliver.h"
#include "CSxGrossLeverageColumn.h"
#include "CSxGrossLeverageFundCcy.h"
#include "CSxGrossLeverageNavPercent.h"


//}}SOPHIS_TOOLKIT_INCLUDE


UNIVERSAL_MAIN
{
	InitLinks(indirectionPtr, TOOLKIT_VERSION); // mandatory
	CSRPreference::SetToolkitVersion(MEDIO_UCITS_CALCULATION_TOOLKIT_DESCRIPTION);

//{{SOPHIS_INITIALIZATION (do not delete this line)

	Initialise_UCITS_Data_Custom(iDerivative, ALLOTMENT_CERTIFICATE, new CSxUCIT_Filter(iDerivative, ALLOTMENT_CERTIFICATE), new CSxUCIT_Calculator<__ALLOTMENT_CERTIFICATE__>());
	Initialise_UCITS_Data_Custom(iSwap, ALLOTMENT_CDS, new CSxUCIT_Filter(iSwap, ALLOTMENT_CDS), new CSxUCIT_Calculator<__ALLOTMENT_CDS__>());
	Initialise_UCITS_Data_Custom(iSwap, ALLOTMENT_CDX, new CSxUCIT_Filter(iSwap, ALLOTMENT_CDX), new CSxUCIT_Calculator<__ALLOTMENT_CDX__>());
	Initialise_UCITS_Data_Custom(iBond, ALLOTMENT_COCO, new CSxUCIT_Filter(iBond, ALLOTMENT_COCO), new CSxUCIT_Calculator<__ALLOTMENT_COCO__>());
	Initialise_UCITS_Data_Custom(iBond, ALLOTMENT_CB, new CSxUCIT_Filter(iBond, ALLOTMENT_CB), new CSxUCIT_Calculator<__ALLOTMENT_CB__>());
	Initialise_UCITS_Data_Custom(iDerivative, ALLOTMENT_IR_DERIVATIVE, new CSxUCIT_Filter(iDerivative, ALLOTMENT_IR_DERIVATIVE), new CSxUCIT_Calculator<__ALLOTMENT_IR_DERIVATIVE__>());
	Initialise_UCITS_Data_Custom(iForexNonDeliverable, ALLOTMENT_NDF, new CSxUCIT_Filter(iForexNonDeliverable, ALLOTMENT_NDF), new CSxUCIT_Calculator<__ALLOTMENT_NDF__>());
	Initialise_UCITS_Data_Custom(iDerivative, ALLOTMENT_OTC_FX_OPTION_SINGLE, new CSxUCIT_Filter(iDerivative, ALLOTMENT_OTC_FX_OPTION_SINGLE), new CSxUCIT_Calculator<__ALLOTMENT_OTC_FX_OPTION_SINGLE__>());
	Initialise_UCITS_Data_Custom(iSwap, ALLOTMENT_TRS_EQUITY_SINGLE, new CSxUCIT_Filter(iSwap, ALLOTMENT_TRS_EQUITY_SINGLE), new CSxUCIT_Calculator<__ALLOTMENT_TRS_EQUITY_SINGLE__>());
	Initialise_UCITS_Data_Custom(iSwap, ALLOTMENT_TRS_EQUITY_BASKET, new CSxUCIT_Filter(iSwap, ALLOTMENT_TRS_EQUITY_BASKET), new CSxUCIT_Calculator<__ALLOTMENT_TRS_EQUITY_BASKET__>());
	Initialise_UCITS_Data_Custom(iSwap, ALLOTMENT_TRS_FIXED_INCOME_SINGLE, new CSxUCIT_Filter(iSwap, ALLOTMENT_TRS_FIXED_INCOME_SINGLE), new CSxUCIT_Calculator<__ALLOTMENT_TRS_FIXED_INCOME_SINGLE__>());
	Initialise_UCITS_Data_Custom(iForexFuture, ALLOTMENT_FX_FORWARD, new CSxUCIT_Filter(iForexFuture, ALLOTMENT_FX_FORWARD), new CSxUCIT_Calculator<__ALLOTMENT_FX_FORWARD__>());
	Initialise_UCITS_Data_Custom(iDerivative, ALLOTMENT_LISTED_OPTION, new CSxUCIT_Filter(iDerivative, ALLOTMENT_LISTED_OPTION, "FX-Vanilla Option"), new CSxUCIT_Calculator<__ALLOTMENT_LISTED_OPTION__>());

	INITIALISE_PORTFOLIO_COLUMN(CSxCheapestToDeliver, "Price CTD")
	INITIALISE_PORTFOLIO_COLUMN(CSxGrossLeverageColumn, "Gross Leverage")
	INITIALISE_PORTFOLIO_COLUMN(CSxGrossLeverageFundCcy, "Gross Leverage curr. fund")
	INITIALISE_PORTFOLIO_COLUMN(CSxGrossLeverageNavPercent, "Gross Leverage in % of NAV")

	//}}SOPHIS_INITIALIZATION
}