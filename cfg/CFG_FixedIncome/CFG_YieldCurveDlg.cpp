/*
** Includes
*/
#include "SphInc/gui/SphEditElement.h"
// specific
#include "Resource/resource.h"
#include "CFG_YieldCurveDlg.h"
#include "SphInc/scenario/SphScenario.h"
#include "SphInc/gui/SphEditList.h"
#include "SphInc/gui/SphCustomMenu.h"
#include "SphInc/market_data/SphYieldCurve.h"
#include "SphInc/DataAccessLayer/Structures/SphYieldCurveData.h"
#include "SphInc/DataAccessLayer/Structures/SphYieldCurvePoints.h"



/*
** Namespace
*/
using namespace sophis::gui;
using namespace sophis::scenario;
using namespace sophis::market_data;
using namespace std;

using namespace sophis::static_data;
/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
WITHOUT_CONSTRUCTOR_INSTRUMENT_DIALOG(CSxYieldCurveDlg)

//-------------------------------------------------------------------------------------------------------------

long CSxYieldCurveDlg::yieldCurveId = 0;

CSxYieldCurveDlg::CSxYieldCurveDlg() : CSRInstrumentDialog()
{
	fResourceId = IDD_YIELD_CURVE - ID_DIALOG_SHIFT;

	NewElementList(eNbFields);

	int nb = 0;

	if (fElementList)
	{
		fElementList[nb++] = new EnhancedDataButton(this, eDisplayButton);
	
	}
}


/*virtual*/ void	CSxYieldCurveDlg::OpenAfterInit(void)
{
	CSRInstrumentDialog::OpenAfterInit();

	unique_ptr<SSYieldCurve> crv(new_SSYieldCurve());
	if (crv != nullptr)
	{
		yieldCurveId = crv->fYieldCurveCode;
		
		std::string modelName=	crv->GetModelName();
	
		if ( modelName== "Ligne a Ligne MAD")
		{
			EnhancedDataButton* button = dynamic_cast<EnhancedDataButton*>(GetElementByRelativeId(eDisplayButton));
			if (button != nullptr)
			{
				button->SetCurveId(yieldCurveId);
				button->Show();
			}

		}
	}

}


void CSxYieldCurveDlg::ElementValidation(int EAId_Modified)
{
		CSRInstrumentDialog::ElementValidation(EAId_Modified);
		/*
	switch (EAId_Modified)
	{	
	case 6://YC model change
	{
		
		CSRInstrument * lastTest = this->new_CSRInstrument();
		if (lastTest != nullptr)
		{
			CSRYieldCurve * curve = dynamic_cast<CSRYieldCurve*>(lastTest);
			if (curve != nullptr)
			{
				CSRButton* button = dynamic_cast<CSRButton*>(GetElementByRelativeId(eDisplayButton));
				if (button != nullptr)
				{
					button->Show();
				}
		}
		}
		delete lastTest;


		int eNetPrice = 6;

		char zz[200] = "";
		GetElementByAbsoluteId(eNetPrice)->GetValue(zz);

		SSCellValue cellValue;
		SSCellStyle cellStyle;
		GetElementByAbsoluteId(eNetPrice)->GetDisplayValue(&cellValue, &cellStyle, 0, true);

		short result=GetElementByAbsoluteId(eNetPrice)->GetListValue();


		CSRElement * modelDropdown = dynamic_cast<CSRElement*>(GetElementByAbsoluteId(6));
		if (modelDropdown != nullptr)
		{
			SSCellValue cellValue;
			SSCellStyle cellStyle;
			modelDropdown->GetDisplayValue(&cellValue, &cellStyle, 0, true);
			
			if (strcmp(cellValue.nullTerminatedString ,"Ligne a Ligne MAD")==0)
			{

				CSRButton* button = dynamic_cast<CSRButton*>(GetElementByRelativeId(eDisplayButton));
				if (button != nullptr)
				{
					button->Show();
				}

			}
		}
		break;
	}
	default:
	{
		break;
	}
	}
	*/

}


EnhancedDataButton::EnhancedDataButton(CSRFitDialog *dialog, int ERId_Element /* = 1 */)
	:CSRButton(dialog, ERId_Element)
{

}



void EnhancedDataButton::Action()
{
	unique_ptr<CSRScenario> tktScenario (CSRScenario::GetPrototype().CreateInstance("ToolkitDialog"));
	if (tktScenario != nullptr)
	{
		tktScenario->SetCode(fcurveId);
		tktScenario->Simulation();
	}
	
}
