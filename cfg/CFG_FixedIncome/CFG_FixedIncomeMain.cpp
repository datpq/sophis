///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)
#include "SphInc/SphMacros.h"
#include "SphTools/SphCommon.h"
#include "SphInc/SphPreference.h"
#include "version/CFG_FixedIncomeVersion.h"
#include "CFG_YieldCurveLinebyLine.h"
#include "CFG_YieldCurveFixedPoints.h"
#include "CFG_YieldCurveZCMAD.h"
#include "CFG_GlobalFunctions.h"
#include "CFG_BondDialog.h"
#include "CFG_StaticSpreadBond.h"
#include "CFG_ConstantAmortizedBond.h"
#include "CFG_RevisionClause.h"
#include "CFG_RevisableBondAction.h"
#include "CFG_DayCountBasis.h"
#include "CFG_YieldCurveDlg.h"
#include "CFG_YCScenario.h"

//}}SOPHIS_TOOLKIT_INCLUDE

UNIVERSAL_MAIN
{
	InitLinks(indirectionPtr, TOOLKIT_VERSION); // mandatory
	CSRPreference::SetToolkitVersion(CFG_FixedIncome_TOOLKIT_DESCRIPTION);

//{{SOPHIS_INITIALIZATION (do not delete this line)

	//INITIALISE_STANDARD_DIALOG(CSxYieldCurveDlg, kYieldCurveDialogId);
	//INITIALISE_SCENARIO(CFG_YCScenario, "ToolkitDialog");

	INITIALISE_YIELD_CURVE(CFG_YieldCurveLinebyLine, "Ligne a Ligne MAD");
	INITIALISE_YIELD_CURVE(CFG_YieldCurveFixedPoints, "Points Fixes MAD");
	INITIALISE_YIELD_CURVE(CFG_YieldCurveZCMAD, "ZC MAD");
	INITIALISE_GLOBAL_FUNCTIONS(CFG_GlobalFunctions);
	INITIALISE_STANDARD_DIALOG(CFG_BondDialog, kBondDialogId);
	INITIALISE_BOND(CFG_StaticSpreadBond, "Titre MAD remboursable in fine");
	INITIALISE_BOND(CFG_ConstantAmortizedBond, "Titre MAD amortissable");
	//DPH
	INITIALISE_BOND_META_MODEL(EmcPricerBond, "CFG_StaticSpreadBond_MetaModel", "CFG_StaticSpreadBond_MetaModelGuid")
	sophis::finance::CSRPricer* pricer = const_cast<sophis::finance::CSRPricer*>(sophis::finance::CSRPricer::GetPrototype().GetData("CFG_StaticSpreadBond_MetaModel"));
	//pricer->SetName(CSRPricer::gNoPricerName);
	pricer->SetName("No Meta Model");
	INITIALISE_BOND_META_MODEL(EmcPricerBond, "CFG_ConstantAmortizedBond_MetaModel", "CFG_ConstantAmortizedBond_MetaModelGuid")
	sophis::finance::CSRPricer* pricerConstantAmortizeBond = const_cast<sophis::finance::CSRPricer*>(sophis::finance::CSRPricer::GetPrototype().GetData("CFG_ConstantAmortizedBond_MetaModel"));
	pricerConstantAmortizeBond->SetName("No Meta Model");

	INITIALISE_CLAUSE(CFG_RevisionClause, CSRClause, "Revision");
	INITIALISE_INSTRUMENT_ACTION(CFG_RevisableBondAction, oUser1, "Revisable Bond Action");
	INITIALISE_DAY_COUNT_BASIS(CFG_ActActCouponDayCountBasis, "Act/Act CFG Coupon");
//}}SOPHIS_INITIALIZATION

};
