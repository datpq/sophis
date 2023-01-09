#include "CSxVarianceSwapDlg.h"
#include <SphInc/gui/SphEditElement.h>
#include <SphInc/gui/SphInstrumentDialogWithTabs.h>
#include "SphInc/instrument/SphSwap.h"
#include "../../MediolanumConstants.h"
#include <SphInc/instrument/SphFixedLeg.h>
#include <SphInc/GUI/SphInstrumentUI.h>
#include <interface/transdlog.h>
#include <SphInc/gui/SphCode.h>
#include "SphInc/gui/SphDialogArea.h"
#include <SphTools/SphOStrStream.h>
#include "SphInc/finance/SphPricer.h"


const char* vegaNotionalName = MEDIO_GUI_FIELDNAME_SWAP_VEGA_NOTIONAL;
const char* volStrikeName = MEDIO_GUI_FIELDNAME_SWAP_VOL_STRIKE;
const char* varDaysName = MEDIO_GUI_FIELDNAME_SWAP_VAR_DAYS;
/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
WITHOUT_CONSTRUCTOR_INSTRUMENT_DIALOG(CSxSwapGeneralDlg)


//-------------------------------------------------------------------------------------------------------------
CSxSwapGeneralDlg::CSxSwapGeneralDlg() : CSRInstrumentDialog()
{
	//TODO check and confirm the dialog is working
	//INITIALISE_CASTAGE_PRECISION(CSRInstrumentDialog)
	fResourceId = IDD_SWAP_GENERAL - ID_DIALOG_SHIFT;

	int nb = InitElementList(fElementCount, eNbFields);
	if (fElementList)
	{
		fElementList[nb++] = new CSRStaticText(this, eVegaNotionalLab, 256, "Vega Notional");
		fElementList[nb++] = new CSRStaticText(this, eVolStrikeLab, 256, "Vol Strike %");
		fElementList[nb++] = new CSREditDouble(this, eVegaNotional, 2, -1e11, 1e11, 0., vegaNotionalName);
		fElementList[nb++] = new CSREditDouble(this, eVolStrike, 4, -1e11, 1e11, 0, volStrikeName);
		fElementList[nb++] = new CSRStaticText(this, eVarUnitsLab, 256, "Variance Units");
		fElementList[nb++] = new CSRStaticText(this, eVarStikeLab, 256, "Variance Strike");
		fElementList[nb++] = new CSREditDouble(this, eVarUnits, 4, -1e11, 1e11, 0);
		fElementList[nb++] = new CSREditDouble(this, eVarStike, 4, -1e11, 1e11, 0);
		fElementList[nb++] = new CSREditDouble(this, eVarDays, 0, -1e11, 1e11, 0, varDaysName);
		fElementList[nb++] = new CSRStaticText(this, eVarDaysLab, 256, "Variance Days");
	}
}


//-------------------------------------------------------------------------------------------------------------
void CSxSwapGeneralDlg::OpenAfterInit(void)
{
	CSRInstrumentDialog::OpenAfterInit();
	if (IsVarSwap()) //	Variance Swap
	{
		ShowAllUserElements();
		InitVarSwapDefaultValues();
		RefreshVarianceDays();
	}
	else if (IsVolSwap())
	{
		ShowAllUserElements(false);
	}
	else
		HideAllUserElements();
}


//-------------------------------------------------------------------------------------------------------------
void CSxSwapGeneralDlg::ElementValidation(int EAId_Modified)
{
	CSRInstrumentDialog::ElementValidation(EAId_Modified);
	CSRElement *element = GetElementByRelativeId(EAId_Modified);

	switch (EAId_Modified)
	{
		case eModel:
		{
			if (IsVarSwap())
			{
				ShowAllUserElements();
				InitVarSwapDefaultValues();
			}
			else if(IsVolSwap())
			{
				HideAllUserElements();
				ShowAllUserElements(false);
			}
			else
				HideAllUserElements();
		} break;

		case eVegaNotional:
		{
			if (element)
			{
				if (IsVolSwap())
				{
					CSRSwap* swap = dynamic_cast<CSRSwap*>(new_CSRInstrument());
					if (swap)
					{
						TItem*	notionalElem;
						TDlog * dlog = GetDlog();
						FindUserItem(dlog, eNotional, &notionalElem);
						if (notionalElem)
						{
							CSRElement *vegaNotionalElement = GetElementByRelativeId(eVegaNotional);
							double vegaNotional;
							vegaNotionalElement->GetValue((void*)&vegaNotional);
							*(notionalElem->infos.editdouble.value) = vegaNotional * 100;
							UpdateElement(eNotional);
							swap->SetNotional(*(notionalElem->infos.editdouble.value));
						}
					}
				}
				else if (IsVarSwap())
				{
					UpdateNotional();
					InitVarSwapDefaultValues();
				}
			}
		} break;

		case eVolStrike:
		{
			if (element)
			{
				double  volStrike;
				element->GetValue((void*)&volStrike);
				double fixedRate = volStrike *volStrike / 100;

				CSRFitDialog * parent = GetParent();
				if (!parent)
					return;

				CSRTabButton * tabPage = dynamic_cast<CSRTabButton *>(parent->GetElementByRelativeId(1));
				if (!tabPage)
					return;

				CSRFitDialog * generalDialog = tabPage->GetPageDlg(0);
				if (!generalDialog)
					return;

				CSRDialogArea * legArea = dynamic_cast<CSRDialogArea *>(generalDialog->GetElementByRelativeId(29)); //29
				if (!legArea)
					return;

				CSRFitDialog * legDialog = legArea->GetCurrentDialog();
				if (!legDialog)
					return;

				TItem*	fixedRateElement;
				FindUserItem(legDialog->GetDlog(), eFixRate, &fixedRateElement);
				if (fixedRateElement)
				{
					*(fixedRateElement->infos.editdouble.value) = fixedRate;
					legDialog->UpdateElement(eFixRate);
					legDialog->ElementValidation(eFixRate);
				}
				if (IsVarSwap())
				{
					UpdateNotional();
					InitVarSwapDefaultValues();
				}
			}
		} break;

		case eNotional:
		{
			if (IsVarSwap())
				InitVarSwapDefaultValues();
		}break;

		case eFixRate:
		{
			if (IsVarSwap())
				InitVarSwapDefaultValues();
		}break;
	}
}


//-------------------------------------------------------------------------------------------------------------
/// Used to calculate the notional for Variance Swap
void CSxSwapGeneralDlg::UpdateNotional() const
{
	CSRSwap* swap = dynamic_cast<CSRSwap*>(new_CSRInstrument());
	if (swap)
	{
		CSRElement *volStrikeElement = GetElementByRelativeId(eVolStrike);
		CSRElement *vegaNotionalElement = GetElementByRelativeId(eVegaNotional);
		if (volStrikeElement && vegaNotionalElement)
		{
			double vegaNotional, volStrike;
			vegaNotionalElement->GetValue((void*)&vegaNotional);
			volStrikeElement->GetValue((void*)&volStrike);
			double notional = 10000 * vegaNotional / (2 * volStrike);
			TItem*	notionalElem;
			TDlog * dlog = GetDlog();
			FindUserItem(dlog, eNotional, &notionalElem);
			if (notionalElem)
			{
				*(notionalElem->infos.editdouble.value) = notional;
				UpdateElement(eNotional);
				swap->SetNotional(notional);
			}
		}
	}
}


//-------------------------------------------------------------------------------------------------------------
void CSxSwapGeneralDlg::RefreshVarianceDays()
{
	CSRSwap* swap = dynamic_cast<CSRSwap*>(new_CSRInstrument());
	if (swap)
	{
		const sophis::CSRComputationResults * results = swap->GetComputationResults(swap->GetCode());
		CSRPricer * pricer = swap->GetPricer();
		if (pricer)
		{
			CSRComputationResults results;
			pricer->ComputeAll(*swap, *gApplicationContext, results);
			swap->RecomputeAllDependend(*gApplicationContext, results);
			double days = results.VarianceDays();
			SetElementValue(eVarDays, &days);
		}
	}
}


//-------------------------------------------------------------------------------------------------------------
int CSxSwapGeneralDlg::InitElementList(int currentNbFields, int nbNewFields)
{
	CSRElement** oldElementList = fElementList;
	fElementList = NULL;
	NewElementList(currentNbFields + nbNewFields);

	int nb = 0;
	for (; nb < currentNbFields; nb++)
	{
		fElementList[nb] = oldElementList[nb];
	}
	delete[]oldElementList;
	oldElementList = NULL;
	return nb;
}


//-------------------------------------------------------------------------------------------------------------
bool CSxSwapGeneralDlg::IsVarSwap() const
{
	return CheckModelName("Variance Swap");
}


//-------------------------------------------------------------------------------------------------------------
bool CSxSwapGeneralDlg::IsVolSwap() const
{
	return CheckModelName("Volatility Swap");
}

//-------------------------------------------------------------------------------------------------------------
bool CSxSwapGeneralDlg::CheckModelName(char* modelNameCompare) const
{
	CSRInstrument *instrument = new_CSRInstrument();
	if (instrument == nullptr)
		return false;
	char modelName[40];
	instrument->GetModelName(modelName);
	return (strcmp(modelName, modelNameCompare) == 0);
}

//-------------------------------------------------------------------------------------------------------------
void CSxSwapGeneralDlg::InitVarSwapDefaultValues() const
{
	CSRElement *element = GetElementByRelativeId(eVarUnits);
	CSRSwap* swap = dynamic_cast<CSRSwap*>(new_CSRInstrument());
	if (swap)
	{
		if (element) 
		{
			double  varUnits = swap->GetNotional() / 10000;
			element->SetValue(&varUnits);
			UpdateElement(eVarUnits);
		}

		element = GetElementByRelativeId(eVarStike);
		if (element)
		{
			CSRFixedLeg* leg = dynamic_cast<CSRFixedLeg*>(swap->GetLeg(1));
			if (leg)
			{
				double  varStike = leg->GetFixedRate() * 10000;
				element->SetValue(&varStike);
				UpdateElement(eVarStike);
			}
		}
	}
}


/*
*	Adding Refresh Button
*
*/
CSxRefreshButton::CSxRefreshButton(CSRFitDialog *dialog, int ERId_Element) : CSRButton(dialog, ERId_Element)
{
}


//-------------------------------------------------------------------------------------------------------------
void CSxRefreshButton::Action()
{
	CSxSwapGeneralDlg *dialog = dynamic_cast<CSxSwapGeneralDlg*>(this->GetDialog());
	if (!dialog)
		return;

	dialog->RefreshVarianceDays();
}
