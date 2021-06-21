

/* VALUE toolkit*/
#pragma warning(disable:4251) // '...' : struct '...' needs to have dll-interface to be used by clients of class '...')

/* RISQUE toolkit */
#include "SphTools/base/ExceptionBase.h"
#include "SphInc/value/kernel/SphFundFees.h"
#include "SphLLInc/portfolio/SphPortfolioColMenu.h"
#include "SphInc/gui/SphElement.h"
#include "SphInc/gui/SphCheckBox.h"

#include __STL_INCLUDE_PATH(algorithm)


#include "../Resource/resource.h"
#include "CSxCustomMenu.h"
#include "CSxManagementFeesGUI.h"


using namespace sophis::gui;
using namespace sophis::value;
using namespace sophis::portfolio;


DEFINE_FUND_FEES_PAGE(CSxManagementFees) 

//-----------------------------------------------------------------------------------------------------------------------
CSxManagementFees::Page::Page()
{
	INIT_FUND_FEES_PAGE(IDD_CFG_MANAGEMENT_TAB); // Create fElementList

	int nb=0;

	// Amount type NAV or portfolio column
	fElementList[nb++] = new CSRRadioButtonAmountType(this, cAmountChoice, cAmountChoice+1);		

	// NAV type
	fElementList[nb++]	= new CSxNAVTypeMenu(this, cNAVType, kUndefinedField);

	// Portfolio column
	fElementList[nb++] = new CSRPortfolioColMenu(this, cPtfCol);

	//Level list
	CSxEditListFees* feesList = new CSxEditListFees(this,cLevelList);
	fElementList[nb++] = feesList;

	//Add button 
	fElementList[nb++] = new CSxEditListFees::FeesAddButton(this, cFeesAddButton, feesList);

	//Delete button
	fElementList[nb++] = new CSxEditListFees::FeesRemoveButton(this, cFeesRemoveButton, feesList);

	// Mode type standard or per level
	fElementList[nb++] = new CSRRadioButtonModeType(this, cModeChoice, cModeChoice+1); 

	//Rate (in %)
	fElementList[nb++] = new CSRStaticText(this,cRateLabel,100,"Rate (in %)");
	fElementList[nb++] = new CSREditDouble(this,cRate, 6, -99, 99);

	fElementList[nb++] = new CSRStaticText(this,cRateRangeMinLabel, 100, "when value between");
	fElementList[nb++] = new CSREditDouble(this,cRateRangeMin, 0 ,0., 1.e300);

	fElementList[nb++] = new CSRStaticText(this, cRateRangeMaxLabel, 100, "and");
	fElementList[nb++] = new CSREditDouble(this, cRateRangeMax, 0, 0., 1e300, 0., kUndefinedField, true);

	// Day to day check box
	fElementList[nb++] = new CSRCheckBox(this, cDayToDayBox, false);

	assert(nb==fElementCount);
}

//----------------------------------------------------------------------------------------------------------------
void CSxManagementFees::Page::LoadInitialAvailability()
{
	fInitAvailPaymentDate = true;
	fInitAvailOffset = true;
	fInitAvailAccountingPeriodicity = true;
	fInitAvailAccountedInAdvance = true;
	fInitAvailPaymentPeriodicity = true;
	fInitAvailPaidInAdvance = true;
	fInitAvailCompounding = true;
	fInitAvailTimeBasis = true;
}

//----------------------------------------------------------------------------------------------------------------
/*virtual*/ void	CSxManagementFees::Page::OpenAfterInit(void)
{
	short initialVal = 1;
	GetElementByRelativeId(cModeChoice)->SetValue(&initialVal);
		
	ShowStandardMode();
}

//----------------------------------------------------------------------------------------------------------------
void CSxManagementFees::Page::ElementValidation(int EAId_Modified)
{
	CSAMFundFeesPage::ElementValidation(EAId_Modified);

	sophis::gui::CSRElement* element = GetElementByAbsoluteId(EAId_Modified);
	assert(element);
	switch (EAId_Modified)
	{
	case cAmountChoice: // NAV
	{
		ElementValidation(cNAVType);
		GetElementByAbsoluteId(cDayToDayBox)->Disable();
		int currentNavType = CSAMFundManagementFees::ntBeginning;
		element->GetValue(&currentNavType);
	}
	break;
	case 2: // cAmountChoice+1 (Portfolio Column)
	{
		GetElementByAbsoluteId(cDayToDayBox)->Enable();
		bool newValue = true;
		GetElementByAbsoluteId(cDayToDayBox)->SetValue(&newValue);
	}
	break;
	case cNAVType:
	{
		CSxManagementFees::eNAVType nt;
		element->GetValue(&nt);
		if (nt > CSxManagementFees::ntBeginning)
		{
			// NAV type > "period beginning" => !accounting in arrears
			//SetAccountedInAdvance(false); => Update a field that does not exist anymore
			//GetGlobalPage()->AccountingInAdvanceValidation(); // validate again (until convergence)
		}
		if (nt == CSAMFundManagementFees::ntDayToDay)
		{
			bool newValue = true;
			GetElementByAbsoluteId(cDayToDayBox)->SetValue(&newValue);
		}
		else
		{
			bool newValue = false;
			GetElementByAbsoluteId(cDayToDayBox)->SetValue(&newValue);
		}
	}
	break;
	}
}

//----------------------------------------------------------------------------------------------------------------
void CSxManagementFees::Page::LoadFromFees(const CSAMFundFees* fees)
{
	const CSxManagementFees* manFees = dynamic_cast<const CSxManagementFees*>(fees);
	assert(manFees);
	if (manFees)
	{
		CSRElement* element = 0;
		element = GetElementByAbsoluteId(cAmountChoice);
		element->SetValue(&manFees->fAmountChoice);

		element = GetElementByAbsoluteId(cNAVType);
		bool dayToDayBoxValue = false;
		CSRElement * dayToDayBox = GetElementByAbsoluteId(cDayToDayBox);
		CSRPopupMenu * popup = dynamic_cast<CSRPopupMenu*>(GetElementByAbsoluteId(cPtfCol));
		if (manFees->fAmountChoice == acNAV)
		{
			element->SetValue(&manFees->fNAVType);
			element->Enable(true); // Enable NAVType menu
			popup->Enable(false); // Disable portfolio column menu
			dayToDayBox->Enable(false);
			if (manFees->fNAVType == CSxManagementFees::ntDayToDay)
			{
				dayToDayBoxValue = true;
			}
			else
			{
				dayToDayBoxValue = false;
			}
		}
		else
		{
			popup->StringToValue(manFees->fPtfColName, 0);
			popup->Enable(true); // Enable portfolio column menu
			popup->Update();
			element->Enable(false); // Disable NAVType menu 
			dayToDayBox->Enable(true);
			dayToDayBoxValue = true;

		}
		dayToDayBox->SetValue(&dayToDayBoxValue);

		element = GetElementByAbsoluteId(cModeChoice);
		element->SetValue(&manFees->fModeChoice);

		if (manFees->fModeChoice==acStandard)
		{			
			ShowStandardMode();
		}
		else
		{
			ShowStandardMode(false);
		}
		
		element = GetElementByAbsoluteId(cRate);
		element->SetValue(&manFees->fRate);
		element = GetElementByAbsoluteId(cRateRangeMin);
		element->SetValue(&manFees->fRateRangeMin);
		element = GetElementByAbsoluteId(cRateRangeMax);
		element->SetValue(&manFees->fRateRangeMax);

		//Rate per level
		CSxEditListFees* pElem = (CSxEditListFees*)GetElementByRelativeId(cLevelList);		

		size_t	nbResults	= (manFees->fRatesPerLevelList).size();	
		pElem->SetLineCount((int)nbResults);
				
		for(size_t i = 0; i < nbResults; i++)
		{						
			CSRElement* pElement = NULL;	
			SSxRatePerLevel ratePerLevel = (manFees->fRatesPerLevelList)[i];
			pElem->LoadLine((int)i);			
			pElement = pElem->GetElementByIndex(0);
			pElement->SetValue(&(ratePerLevel.fLevel));
			pElement = pElem->GetElementByIndex(1);
			pElement->SetValue(&(ratePerLevel.fRate));
			pElem->SaveLine((int)i);
		}
		pElem->Update();
	}
}

//----------------------------------------------------------------------------------------------------------------
void CSxManagementFees::Page::SaveToFees(CSAMFundFees* fees) const
{
	CSxManagementFees* manFees = dynamic_cast<CSxManagementFees*>(fees);
	assert(manFees);
	if (manFees)
	{
		CSRElement* element = 0;
		element = GetElementByAbsoluteId(cAmountChoice);
		element->GetValue(&manFees->fAmountChoice);
		if (manFees->fAmountChoice == acNAV) // NAV Type
		{
			element = GetElementByAbsoluteId(cNAVType);
			element->GetValue(&manFees->fNAVType);
		}
		else // Portfolio column
		{
			manFees->fNAVType = CSAMFundManagementFees::ntDayToDay;
		}
		element = GetElementByAbsoluteId(cPtfCol);
		//DPH
		//element->ValueToString(manFees->fPtfColName, 0);
		element->ValueToString(manFees->fPtfColName, 0, 40);
		element = GetElementByAbsoluteId(cModeChoice);
		element->GetValue(&manFees->fModeChoice);
		element = GetElementByAbsoluteId(cRate);
		element->GetValue(&manFees->fRate);
		element = GetElementByAbsoluteId(cRateRangeMin);
		element->GetValue(&manFees->fRateRangeMin);
		element = GetElementByAbsoluteId(cRateRangeMax);
		element->GetValue(&manFees->fRateRangeMax);

		// Rate per level
		CSxEditListFees* pElem = (CSxEditListFees*)GetElementByAbsoluteId(cLevelList);
		int nbLine = pElem->GetLineCount();		

		(manFees->fRatesPerLevelList).clear();

		for (int i = 0 ;i < nbLine; i++)
		{
			SSxRatePerLevel managementFees;
			pElem->LoadLine(i);
			element = NULL;
			element = pElem->GetElementByIndex(0);
			element->GetValue(&(managementFees.fLevel));

			element = NULL;
			element = pElem->GetElementByIndex(1);
			element->GetValue(&(managementFees.fRate));			

			(manFees->fRatesPerLevelList).push_back(managementFees);
		}

		_STL::sort((manFees->fRatesPerLevelList).begin(),(manFees->fRatesPerLevelList).end(),CSxManagementFees::CompareSSxRatePerLevelElements);
	}
}

//----------------------------------------------------------------------------------------------------------------
void CSxManagementFees::Page::ShowStandardMode(bool isStandardMode)
{
	if (isStandardMode)
	{
		GetElementByRelativeId(cRateLabel)->Show();	
		GetElementByRelativeId(cRate)->Show();	
		GetElementByRelativeId(cRateRangeMinLabel)->Show();	
		GetElementByRelativeId(cRateRangeMin)->Show();	
		GetElementByRelativeId(cRateRangeMaxLabel)->Show();	
		GetElementByRelativeId(cRateRangeMax)->Show();	
		GetElementByRelativeId(cLevelList)->Hide();	
		GetElementByRelativeId(cFeesAddButton)->Hide();	
		GetElementByRelativeId(cFeesRemoveButton)->Hide();
	}
	else
	{
		GetElementByRelativeId(cRateLabel)->Hide();	
		GetElementByRelativeId(cRate)->Hide();	
		GetElementByRelativeId(cRateRangeMinLabel)->Hide();	
		GetElementByRelativeId(cRateRangeMin)->Hide();	
		GetElementByRelativeId(cRateRangeMaxLabel)->Hide();	
		GetElementByRelativeId(cRateRangeMax)->Hide();	
		GetElementByRelativeId(cLevelList)->Show();	
		GetElementByRelativeId(cFeesAddButton)->Show();	
		GetElementByRelativeId(cFeesRemoveButton)->Show();
	}
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
CSxManagementFees::Page::CSRRadioButtonModeType::CSRRadioButtonModeType(CSxManagementFees::Page* dialog, int ERId_FirstButton, int ERId_LastButton) 
	: CSRRadioButton(dialog, ERId_FirstButton, ERId_LastButton)
{
}

//-----------------------------------------------------------------------------------------------------------------------------------------
Boolean	CSxManagementFees::Page::CSRRadioButtonAmountType::Validation()
{
	CSRElement* elementNAVType = fDialog->GetElementByAbsoluteId(CSxManagementFees::Page::cNAVType);
	CSRElement* elementPtfCol = fDialog->GetElementByAbsoluteId(CSxManagementFees::Page::cPtfCol);
	assert(elementNAVType && elementPtfCol);
	elementNAVType->Enable((eAmountChoice)fValue==acNAV);
	elementPtfCol->Enable((eAmountChoice)fValue==acPtfCol);
	return true;
}

//-----------------------------------------------------------------------------------------------------------------------------------------
Boolean	CSxManagementFees::Page::CSRRadioButtonModeType::Validation()
{
	CSRElement* elem = fDialog->GetElementByAbsoluteId(CSxManagementFees::Page::cRateLabel);
	elem->Show((eModeChoice)fValue==acStandard);

	elem = fDialog->GetElementByAbsoluteId(CSxManagementFees::Page::cRate);
	elem->Show((eModeChoice)fValue==acStandard);

	elem = fDialog->GetElementByAbsoluteId(CSxManagementFees::Page::cRateRangeMinLabel);
	elem->Show((eModeChoice)fValue==acStandard);

	elem = fDialog->GetElementByAbsoluteId(CSxManagementFees::Page::cRateRangeMin);
	elem->Show((eModeChoice)fValue==acStandard);

	elem = fDialog->GetElementByAbsoluteId(CSxManagementFees::Page::cRateRangeMaxLabel);
	elem->Show((eModeChoice)fValue==acStandard);

	elem = fDialog->GetElementByAbsoluteId(CSxManagementFees::Page::cRateRangeMax);
	elem->Show((eModeChoice)fValue==acStandard);

	elem = fDialog->GetElementByAbsoluteId(CSxManagementFees::Page::cLevelList);
	elem->Show((eModeChoice)fValue==acPerLevel);

	elem = fDialog->GetElementByAbsoluteId(CSxManagementFees::Page::cFeesAddButton);
	elem->Show((eModeChoice)fValue==acPerLevel);

	elem = fDialog->GetElementByAbsoluteId(CSxManagementFees::Page::cFeesRemoveButton);
	elem->Show((eModeChoice)fValue==acPerLevel);
	
	return true;
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
CSxManagementFees::Page::CSRRadioButtonAmountType::CSRRadioButtonAmountType(CSxManagementFees::Page* dialog, int ERId_FirstButton, int ERId_LastButton) 
	: CSRRadioButton(dialog, ERId_FirstButton, ERId_LastButton)
{
}