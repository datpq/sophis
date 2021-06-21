
#ifndef _CSxManagementFeesGUI_H_
#define _CSxManagementFeesGUI_H_

#include "SphInc/SphMacros.h"
#include "SphInc/gui/SphRadioButton.h"
#include "SphInc/gui/SphEditList.h"
#include "SphInc/value/kernel/SphFundFeesGUI.h"


#include "CSxManagementFees.h"


class CSxManagementFees::Page : public sophis::value::CSAMFundFeesPage
{
	DECLARE_FUND_FEES_PAGE

public:
	/**
	*	This function proceed in the validation of all the GUI elements when the user
	*	is editing one of them.	
	*/
	void ElementValidation(int EAId_Modified); // overload (from CSRFitDialog)

	void LoadInitialAvailability();

	virtual void	OpenAfterInit(void);	

	enum Controls 
	{ 
		cAmountChoice=1, //1,2	// 1
		cNAVType=3,
		cPtfCol,
		cLevelList,		
		cFeesAddButton,			// 6
		cFeesRemoveButton,
		cModeChoice, //8,9 
		cRateLabel=10,
		cRate,				 
		cRateRangeMinLabel,		// 12
		cRateRangeMin,
		cRateRangeMaxLabel,
		cRateRangeMax,		
		cDayToDayBox,			// 16
		COUNT=14
	};

protected:
	class CSRRadioButtonAmountType : public sophis::gui::CSRRadioButton
	{
		public:
			CSRRadioButtonAmountType(CSxManagementFees::Page* dialog, int ERId_FirstButton, int ERId_LastButton);
			Boolean	Validation(); // overload
	};
	friend class CSRRadioButtonAmountType;

	class CSRRadioButtonModeType : public sophis::gui::CSRRadioButton
	{
		public:
			CSRRadioButtonModeType(CSxManagementFees::Page* dialog, int ERId_FirstButton, int ERId_LastButton);
			Boolean	Validation(); // overload
	};
	friend class CSRRadioButtonModeType;

	void ShowStandardMode(bool isStandardMode = true);
};

#endif // _CSxManagementFeesGUI_H_