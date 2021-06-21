#pragma warning(disable:4251)
/*
** Includes
*/
// standard
#include "SphInc/SphMacros.h"
#include "SphTools/SphCommon.h"
#include "SphInc/SphPreference.h"
///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)
#include "version/CFGCheckVersion.h"

#include "CheckPosition.h"
#include "CheckCoupon.h"
#include "CheckPrice.h"

#include "SphInc/instrument/SphEquity.h"
#include "ShareDialog.h"
#include "CheckDevise.h"
#include "BOModifiableFields.h"
#include "FOModifiableFields.h"
#include "SRCompliance.h"
#include "CheckListedBond.h"
#include "MandatoryCounterpart.h"
//}}SOPHIS_TOOLKIT_INCLUDE


UNIVERSAL_MAIN
{
	InitLinks(indirectionPtr, TOOLKIT_VERSION); // mandatory
	CSRPreference::SetToolkitVersion(CFGCheck_TOOLKIT_DESCRIPTION);

//{{SOPHIS_INITIALIZATION (do not delete this line)


	// By default, this action is initialized with oAfterSophisValidation. It is called after all sophis triggers.
	// You can change this default value if needed.
	INITIALISE_TRANSACTION_ACTION(CheckPosition,oAfterSophisValidation, "CheckPosition")
	// By default, this action is initialized with oAfterSophisValidation. It is called after all sophis triggers.
	// You can change this default value if needed.
	INITIALISE_TRANSACTION_ACTION(CheckCoupon,oAfterSophisValidation, "CheckCoupon")
	// By default, this action is initialized with oAfterSophisValidation. It is called after all sophis triggers.
	// You can change this default value if needed.
	INITIALISE_TRANSACTION_ACTION(CheckPrice,oAfterSophisValidation, "CheckPrice")
	INITIALISE_STANDARD_DIALOG(ShareDialog, kShareDialogId)
	// By default, this action is initialized with oAfterSophisValidation. It is called after all sophis triggers.
	// You can change this default value if needed.
	INITIALISE_TRANSACTION_ACTION(CheckDevise,oAfterSophisValidation, "CheckDevise")
	INITIALISE_KERNEL_EDIT_MODEL(BOModifiableFields, "CFG BO")
	INITIALISE_KERNEL_EDIT_MODEL(FOModifiableFields, "CFG FO")
	INITIALISE_SUBSCRIPTION_ACTION(SRCompliance,CSAMFundSubscriptionAction::oAfterSophisValidation, "SRCompliance")
	// By default, this action is initialized with oAfterSophisValidation. It is called after all sophis triggers.
	// You can change this default value if needed.
	//INITIALISE_TRANSACTION_ACTION(CheckListedBond,oAfterSophisValidation, "CheckListedBond")
	INITIALISE_KERNEL_ACTION_CONDITION(MandatoryCounterpart, "isCounterpartDefined")
//}}SOPHIS_INITIALIZATION
}
