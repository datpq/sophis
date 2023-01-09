#ifndef __CSxSwapDlg__H__
#define __CSxSwapDlg__H__


/*
** Includes
*/

#include "SphInc/gui/SphInstrumentDialog.h"
#include "..\Resource\resource.h"
#include <afxres.h>
#include <SphInc/gui/SphButton.h>

/*
** Class
*/
class CSxSwapGeneralDlg : public CSRInstrumentDialog
{
	DECLARATION_INSTRUMENT_DIALOG(CSxSwapGeneralDlg)

	enum
	{		
		eVegaNotionalLab = IDC_STATIC_VEGA_NOTIONAL - ID_ITEM_SHIFT,
		eVolStrikeLab = IDC_STATIC_VOL_STRIKE - ID_ITEM_SHIFT,
		eVegaNotional = IDC_SWAP_VEGA_NOTIONAL - ID_ITEM_SHIFT,
		eVolStrike = IDC_SWAP_VOL_STRIKE - ID_ITEM_SHIFT,
		eVarUnits = IDC_SWAP_VAR_UNITS - ID_ITEM_SHIFT,
		eVarUnitsLab = IDC_STATIC_VAR_UNITS - ID_ITEM_SHIFT,
		eVarStike = IDC_SWAP_VAR_STRIKE - ID_ITEM_SHIFT,
		eVarStikeLab = IDC_STATIC_VAR_STRIKE - ID_ITEM_SHIFT,
		eVarDays = IDC_SWAP_VAR_DAYS - ID_ITEM_SHIFT,
		eVarDaysLab = IDC_STATIC_VAR_DAYS - ID_ITEM_SHIFT,
		eNbFields = 10,
		eModel = 50,
		eNotional = 4,
		eFixRate = 6,
	};

	virtual void	OpenAfterInit(void);
	virtual void	ElementValidation(int EAId_Modified);
	virtual	void	RefreshVarianceDays(void);

private:
	int InitElementList(int currentNbFields, int nbNewFields);
	void HideAllUserElements() const
	{
		CSRElement * valuationField = GetElementByRelativeId(eVegaNotionalLab);
		if (valuationField) valuationField->SetValue("");
		valuationField = GetElementByRelativeId(eVolStrikeLab);
		if (valuationField) valuationField->SetValue("");
		valuationField = GetElementByRelativeId(eVegaNotional);
		if (valuationField) valuationField->Hide();
		valuationField = GetElementByRelativeId(eVolStrike);
		if (valuationField) valuationField->Hide();
		valuationField = GetElementByRelativeId(eVarUnitsLab);
		if (valuationField) valuationField->SetValue("");
		valuationField = GetElementByRelativeId(eVarStikeLab);
		if (valuationField) valuationField->SetValue("");
		valuationField = GetElementByRelativeId(eVarStike);
		if (valuationField) valuationField->Hide();
		valuationField = GetElementByRelativeId(eVarUnits);
		if (valuationField) valuationField->Hide();
		valuationField = GetElementByRelativeId(eVarDaysLab);
		if (valuationField) valuationField->SetValue("");
		valuationField = GetElementByRelativeId(eVarDays);
		if (valuationField) valuationField->Hide();
	}

	void ShowAllUserElements(bool isVarSwap = true) const
	{
		CSRElement * valuationField = GetElementByRelativeId(eVegaNotionalLab);
		if (valuationField) valuationField->SetValue("Vega Notional");
		valuationField = GetElementByRelativeId(eVegaNotional);
		if (valuationField) valuationField->Show();
		if (isVarSwap)
		{
			valuationField = GetElementByRelativeId(eVolStrikeLab);
			if (valuationField) valuationField->SetValue("Vol Strike %");
			valuationField = GetElementByRelativeId(eVolStrike);
			if (valuationField) valuationField->Show();
			valuationField = GetElementByRelativeId(eVarUnitsLab);
			if (valuationField) valuationField->SetValue("Variance Units");
			valuationField = GetElementByRelativeId(eVarStikeLab);
			if (valuationField) valuationField->SetValue("Variance Strike");
			valuationField = GetElementByRelativeId(eVarStike);
			if (valuationField)
			{
				valuationField->Show();
				valuationField->Enable(false);
			}
			valuationField = GetElementByRelativeId(eVarUnits);
			if (valuationField)
			{
				valuationField->Show();
				valuationField->Enable(false);
			}
			valuationField = GetElementByRelativeId(eVarDaysLab);
			if (valuationField) valuationField->SetValue("Variance Days");
			valuationField = GetElementByRelativeId(eVarDays);
			if (valuationField) valuationField->Show();
			if (valuationField) valuationField->Enable(false);
		}
	}
	void UpdateNotional() const;
	bool IsVolSwap() const;
	bool IsVarSwap() const;
	bool CheckModelName(char* modelNameCompare) const;
	void InitVarSwapDefaultValues() const;
};


class CSxRefreshButton : public sophis::gui::CSRButton
{
	//------------------------------------ PUBLIC ------------------------------------
public:
	CSxRefreshButton(CSRFitDialog *dialog, int ERId_Element);
	void CSxRefreshButton::Action();
};

#endif //!__CSxSwapDlg__H__
