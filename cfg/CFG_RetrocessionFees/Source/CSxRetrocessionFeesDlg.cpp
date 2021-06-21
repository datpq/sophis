/*
** Includes
*/

#include <afxwin.h>
#include "SphTools/base/CommonOS.h"
#include "CSxRetrocessionFeesDlg.h"
#include "SphInc/gui/SphButton.h"
#include "SphInc/gui/SphEditElement.h"
#include __STL_INCLUDE_PATH(vector)
#include "SphInc/SphUserRightsEnums.h"
#include "Sphtools/compatibility/applemanager.h"
#include "SphLLInc/interface/dialogueinfos.h"

#include "CSxCustomMenu.h"
#include "CSxRetrocessionFees.h"
#include "Constants.h"

//DPH
#include "SphInc\gui\SphDialogUI.h"

/*
** Namespace
*/
using namespace sophis::gui;


/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CSxRetrocessionFeesDlg::CSxRetrocessionFeesDlg(eRightStatusType userRight) : CSRFitDialog()
{

	fResourceId	= IDD_RETROCESSION_FEES_DLG - ID_DIALOG_SHIFT;
	fUserRights = userRight;

	NewElementList(eNbFields);

	int nb = 0;

	if (fElementList)
	{
		fElementList[nb++]	= new CSxOKButton(this);
		fElementList[nb++]	= new CSRCancelButton(this);
		
		CSxRetrocessionFeesEditList* feesList = new CSxRetrocessionFeesEditList(this,eRetrocessionFeesList);
		fElementList[nb++]	= feesList;

		//Add button 
		fElementList[nb++] = new CSxRetrocessionFeesEditList::FeesAddButton(this, eAddButton, feesList);

		//Delete button
		fElementList[nb++] = new CSxRetrocessionFeesEditList::FeesRemoveButton(this, eRemoveButton, feesList);
	}

	if (userRight == rsReadOnly)
	{
		for (int i = 2; i < fElementCount; i++)
		{
			fElementList[i]->Disable();
		}
	}
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/void	CSxRetrocessionFeesDlg::Open(void)
{
	// Get data from the database
	CSRFitDialog::Open();	
	
	CSxRetrocessionFees fees;
	_STL::vector<long> fundList;
	fees.GetData(fundList);
	LoadFromFees(&fees);
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/	void CSxRetrocessionFeesDlg::OnOK()
{
	bool ret = true;

	if (GetModified())
	{						
		CSxRetrocessionFees fees;
		SaveToFees(&fees);
		ret = fees.SaveData();
	}

	if (ret)
		CloseDlg();	
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ CSxRetrocessionFeesDlg::~CSxRetrocessionFeesDlg()
{
}

void CSxRetrocessionFeesDlg::LoadFromFees(const CSxRetrocessionFees* fees)
{	
	if (fees)
	{						
		CSxRetrocessionFeesEditList* pElem = (CSxRetrocessionFeesEditList*)GetElementByRelativeId(eRetrocessionFeesList);		

		size_t	nbResults	= (fees->fRetrocessionFeesList).size();	
		pElem->SetLineCount((int)nbResults);

		for(unsigned int i = 0; i < nbResults; i++)
		{						
			CSRElement* pElement = NULL;
			
			CSxRetrocessionFees::SSxRetrocessionFeesData oneRow = (fees->fRetrocessionFeesList)[i];
			pElem->LoadLine(i);
			
			CSxCustomMenuStringQuery* customMenuStringQueryElem = NULL;
			customMenuStringQueryElem = (CSxCustomMenuStringQuery*)pElem->GetElementByIndex(CSxRetrocessionFeesEditList::eName);
			_STL::string val = customMenuStringQueryElem->GetVal(oneRow.fThirdId);
			char buffer[256];
			sprintf_s(buffer,256,val.c_str());
			customMenuStringQueryElem->SetValue(buffer);

			pElement = pElem->GetElementByIndex(CSxRetrocessionFeesEditList::eThirdType);
			pElement->SetValue(&(oneRow.fThirdType));

			customMenuStringQueryElem = NULL;
			customMenuStringQueryElem = (CSxCustomMenuStringQuery*)pElem->GetElementByIndex(CSxRetrocessionFeesEditList::eFundName);
			val = customMenuStringQueryElem->GetVal(oneRow.fFundCode);			
			sprintf_s(buffer,256,val.c_str());
			customMenuStringQueryElem->SetValue(buffer);
			
			pElement = pElem->GetElementByIndex(CSxRetrocessionFeesEditList::eComputationMethod);
			pElement->SetValue(&(oneRow.fComputationMethod));
			pElement = pElem->GetElementByIndex(CSxRetrocessionFeesEditList::eCommissionRate);
			pElement->SetValue(&(oneRow.fCommissionRate));
			pElement = pElem->GetElementByIndex(CSxRetrocessionFeesEditList::eLevel1);
			pElement->SetValue(&(oneRow.fLevel1));
			pElement = pElem->GetElementByIndex(CSxRetrocessionFeesEditList::eRetrocessionRate1);
			pElement->SetValue(&(oneRow.fRetrocessionRate1));
			pElement = pElem->GetElementByIndex(CSxRetrocessionFeesEditList::eLevel2);
			pElement->SetValue(&(oneRow.fLevel2));
			pElement = pElem->GetElementByIndex(CSxRetrocessionFeesEditList::eRetrocessionRate2);
			pElement->SetValue(&(oneRow.fRetrocessionRate2));
			pElement = pElem->GetElementByIndex(CSxRetrocessionFeesEditList::eLevel3);
			pElement->SetValue(&(oneRow.fLevel3));
			pElement = pElem->GetElementByIndex(CSxRetrocessionFeesEditList::eRetrocessionRate3);
			pElement->SetValue(&(oneRow.fRetrocessionRate3));
			pElement = pElem->GetElementByIndex(CSxRetrocessionFeesEditList::eLevel4);
			pElement->SetValue(&(oneRow.fLevel4));
			pElement = pElem->GetElementByIndex(CSxRetrocessionFeesEditList::eRetrocessionRate4);
			pElement->SetValue(&(oneRow.fRetrocessionRate4));
			pElement = pElem->GetElementByIndex(CSxRetrocessionFeesEditList::eTVA_Rate);
			pElement->SetValue(&(oneRow.fTVARate));
			pElement = pElem->GetElementByIndex(CSxRetrocessionFeesEditList::eStartDate);
			pElement->SetValue(&(oneRow.fStartDate));
			pElement = pElem->GetElementByIndex(CSxRetrocessionFeesEditList::eEndDate);
			pElement->SetValue(&(oneRow.fEndDate));			

			pElem->SaveLine(i);
		}
		pElem->Update();		
	}
}

void CSxRetrocessionFeesDlg::SaveToFees(CSxRetrocessionFees* fees) const
{	
	if (fees)
	{		
		CSxRetrocessionFeesEditList* pElem = (CSxRetrocessionFeesEditList*)GetElementByAbsoluteId(eRetrocessionFeesList);
		int nbLine = pElem->GetLineCount();		

		(fees->fRetrocessionFeesList).clear();

		for (int i = 0 ;i < nbLine; i++)
		{
			CSxRetrocessionFees::SSxRetrocessionFeesData OneRow;
			pElem->LoadLine(i);
			CSRElement* element = NULL;
			element = pElem->GetElementByIndex(CSxRetrocessionFeesEditList::eName);
			CSxCustomMenuStringQuery* customMenuStringQueryElem = dynamic_cast<CSxCustomMenuStringQuery*>(element);
			if (customMenuStringQueryElem)
			{
				char buffer[SIZE_NAME_ELEMENT];
				customMenuStringQueryElem->GetValue(buffer);
				OneRow.fThirdId = customMenuStringQueryElem->GetCode(buffer);				
			}
			

			element = pElem->GetElementByIndex(CSxRetrocessionFeesEditList::eThirdType);
			element->GetValue(&(OneRow.fThirdType));

			element = pElem->GetElementByIndex(CSxRetrocessionFeesEditList::eFundName);
			customMenuStringQueryElem = NULL;
			customMenuStringQueryElem = dynamic_cast<CSxCustomMenuStringQuery*>(element);
			if (customMenuStringQueryElem)
			{
				char buffer[SIZE_NAME_ELEMENT];
				customMenuStringQueryElem->GetValue(buffer);
				OneRow.fFundCode = customMenuStringQueryElem->GetCode(buffer);				
			}			

			element = pElem->GetElementByIndex(CSxRetrocessionFeesEditList::eComputationMethod);
			element->GetValue(&(OneRow.fComputationMethod));

			element = pElem->GetElementByIndex(CSxRetrocessionFeesEditList::eCommissionRate);
			element->GetValue(&(OneRow.fCommissionRate));

			element = pElem->GetElementByIndex(CSxRetrocessionFeesEditList::eLevel1);
			element->GetValue(&(OneRow.fLevel1));

			element = pElem->GetElementByIndex(CSxRetrocessionFeesEditList::eRetrocessionRate1);
			element->GetValue(&(OneRow.fRetrocessionRate1));

			element = pElem->GetElementByIndex(CSxRetrocessionFeesEditList::eLevel2);
			element->GetValue(&(OneRow.fLevel2));

			element = pElem->GetElementByIndex(CSxRetrocessionFeesEditList::eRetrocessionRate2);
			element->GetValue(&(OneRow.fRetrocessionRate2));

			element = pElem->GetElementByIndex(CSxRetrocessionFeesEditList::eLevel3);
			element->GetValue(&(OneRow.fLevel3));

			element = pElem->GetElementByIndex(CSxRetrocessionFeesEditList::eRetrocessionRate3);
			element->GetValue(&(OneRow.fRetrocessionRate3));

			element = pElem->GetElementByIndex(CSxRetrocessionFeesEditList::eLevel4);
			element->GetValue(&(OneRow.fLevel4));

			element = pElem->GetElementByIndex(CSxRetrocessionFeesEditList::eRetrocessionRate4);
			element->GetValue(&(OneRow.fRetrocessionRate4));

			element = pElem->GetElementByIndex(CSxRetrocessionFeesEditList::eTVA_Rate);
			element->GetValue(&(OneRow.fTVARate));
			
			element = pElem->GetElementByIndex(CSxRetrocessionFeesEditList::eStartDate);
			element->GetValue(&(OneRow.fStartDate));
			
			element = pElem->GetElementByIndex(CSxRetrocessionFeesEditList::eEndDate);
			element->GetValue(&(OneRow.fEndDate));

			(fees->fRetrocessionFeesList).push_back(OneRow);
		}		
	}
}

//-------------------------------------------------------------------------------------------------------------
/*static*/ void CSxRetrocessionFeesDlg::Display(eRightStatusType userRight)
{	
	//Check if the window is not already open
	CSRUserEditInfo *userInfo = new CSRUserEditInfo( 2001) ;
	if ( CSRFitDialog::IsActiveWindow( userInfo ) )
	{
		delete userInfo ;
		userInfo = NULL ;
	}
	else
	{
		// 3. Create the dialog instance
		CSxRetrocessionFeesDlg *dialog = new CSxRetrocessionFeesDlg(userRight);		
		dialog->DoDialog(false, "Retrocession Fees", true, userInfo);
	}
}

void CSxRetrocessionFeesDlg::CloseDlg()
{
	//DPH
	//HWND hWnd = GetHWND();
	HWND hWnd = CSRFitDialogUI::GetHWND(*this);
	if (::IsWindow(hWnd))
	{
		CWnd * wnd = CWnd::FromHandlePermanent(hWnd);
		if (0 != wnd)
			wnd = wnd->GetParent();
		if (0 != wnd)
			wnd->PostMessage(WM_CLOSE);

	}
}

///////////////////////////////////////////////////////////////////////////
/*-------------------------- CSxRetrocessionFeesEditList -------------------------*/

CSxRetrocessionFeesEditList::CSxRetrocessionFeesEditList(CSRFitDialog* dialog, int nre,
													   const char	*tableName) :
CSREditList(dialog, nre, tableName)
{
	fListeValeur = NULL;
	SetMaxSelection (-1);
	
	fColumnCount = eColumnCount;
	fColumns = new SSColumn[fColumnCount];
	int count = 0;

	fColumns[count].fColumnName		= "Business partner";
	fColumns[count].fColumnWidth	= 130;
	fColumns[count].fAlignmentType = aLeft;
	fColumns[count].fElement = new CSxThirdPartyMenu(this,eName);

	count++;

	fColumns[count].fColumnName		= "Type";
	fColumns[count].fColumnWidth	= 100;
	fColumns[count].fAlignmentType = aLeft;
	fColumns[count].fElement = new CSxThirdTypeMenu(this,eThirdType,kUndefinedField);

	count++;

	fColumns[count].fColumnName		= "Fund";
	fColumns[count].fColumnWidth	= 120;
	fColumns[count].fAlignmentType = aLeft;
	fColumns[count].fElement = new CSxInternalFundMenu(this,eFundName);

	count++;

	fColumns[count].fColumnName		= "Computation method";
	fColumns[count].fColumnWidth	= 120;
	fColumns[count].fAlignmentType = aLeft;
	fColumns[count].fElement = new CSxComputationMethodMenu(this,eComputationMethod,kUndefinedField);

	count++;

	fColumns[count].fColumnName		= "Start date";
	fColumns[count].fColumnWidth	= 80;
	fColumns[count].fAlignmentType = aLeft;
	fColumns[count].fElement = new CSREditDate(this,eStartDate);

	count++;

	fColumns[count].fColumnName		= "End date";
	fColumns[count].fColumnWidth	= 80;
	fColumns[count].fAlignmentType = aLeft;
	fColumns[count].fElement = new CSREditDate(this,eEndDate);

	count++;

	fColumns[count].fColumnName		= "Commission rate";
	fColumns[count].fColumnWidth	= 90;
	fColumns[count].fAlignmentType = aLeft;
	fColumns[count].fElement = new CSREditDouble(this,eCommissionRate,2,-10,2e10,0,kUndefinedField,false);

	count++;

	fColumns[count].fColumnName		= "Level 1";
	fColumns[count].fColumnWidth	= 70;
	fColumns[count].fAlignmentType = aLeft;
	fColumns[count].fElement = new CSREditDouble(this,eLevel1,2,-10,2e10,0,kUndefinedField,false);
	
	count++;

	fColumns[count].fColumnName		= "Rate 1";
	fColumns[count].fColumnWidth	= 60;
	fColumns[count].fAlignmentType = aLeft;
	fColumns[count].fElement = new CSREditDouble(this,eRetrocessionRate1,2,-10,2e10,0,kUndefinedField,false);

	count++;

	fColumns[count].fColumnName		= "Level 2";
	fColumns[count].fColumnWidth	= 70;
	fColumns[count].fAlignmentType = aLeft;
	fColumns[count].fElement = new CSREditDouble(this,eLevel2,2,-10,2e10,0,kUndefinedField,false);

	count++;

	fColumns[count].fColumnName		= "Rate 2";
	fColumns[count].fColumnWidth	= 60;
	fColumns[count].fAlignmentType = aLeft;
	fColumns[count].fElement = new CSREditDouble(this,eRetrocessionRate2,2,-10,2e10,0,kUndefinedField,false);

	count++;

	fColumns[count].fColumnName		= "Level 3";
	fColumns[count].fColumnWidth	= 70;
	fColumns[count].fAlignmentType = aLeft;
	fColumns[count].fElement = new CSREditDouble(this,eLevel3,2,-10,2e10,0,kUndefinedField,false);

	count++;

	fColumns[count].fColumnName		= "Rate 3";
	fColumns[count].fColumnWidth	= 60;
	fColumns[count].fAlignmentType = aLeft;
	fColumns[count].fElement = new CSREditDouble(this,eRetrocessionRate3,2,-10,2e10,0,kUndefinedField,false);

	count++;

	fColumns[count].fColumnName		= "Level 4";
	fColumns[count].fColumnWidth	= 70;
	fColumns[count].fAlignmentType = aLeft;
	fColumns[count].fElement = new CSREditDouble(this,eLevel4,2,-10,2e10,0,kUndefinedField,false);

	count++;

	fColumns[count].fColumnName		= "Rate 4";
	fColumns[count].fColumnWidth	= 60;
	fColumns[count].fAlignmentType = aLeft;
	fColumns[count].fElement = new CSREditDouble(this,eRetrocessionRate4,2,-10,2e10,0,kUndefinedField,false);

	count++;

	fColumns[count].fColumnName		= "TVA Rate";
	fColumns[count].fColumnWidth	= 75;
	fColumns[count].fAlignmentType = aLeft;
	fColumns[count].fElement = new CSREditDouble(this,eTVA_Rate,2,-10,2e10,0,kUndefinedField,false);	

	SetDynamicSize(true);
	SetLineCount(0);

}

CSxRetrocessionFeesEditList::~CSxRetrocessionFeesEditList()
{
}

void CSxRetrocessionFeesEditList::OnAddFees()
{	
	Enlarge(1);	
	SaveLine(GetLineCount()-1);

}

void CSxRetrocessionFeesEditList::OnRemoveFees()
{	
	_STL::vector<long>& selection = GetSelectedLines();
	for (int i = (int)selection.size()-1; i>=0; i--)
	{
		RemoveLine(selection[i]);
	}
}

//Add button for fees list-----------------------------------------------------------
CSxRetrocessionFeesEditList::FeesAddButton::FeesAddButton(CSRFitDialog* dialog, int nre, CSxRetrocessionFeesEditList* feeslist) : 
CSRButton(dialog, nre),
fFeesList (feeslist)
{
}

void CSxRetrocessionFeesEditList::FeesAddButton::Action()
{
	fFeesList->OnAddFees();
}

//Remove button for fees list---------------------------------------------------------
CSxRetrocessionFeesEditList::FeesRemoveButton::FeesRemoveButton(CSRFitDialog* dialog, int nre, CSxRetrocessionFeesEditList * feeslist) : 
CSRButton(dialog, nre),
fFeesList (feeslist)
{
}

void CSxRetrocessionFeesEditList::FeesRemoveButton::Action()
{
	fFeesList->OnRemoveFees();
}

////// CSxOKButton /////////////////////

CSxOKButton::CSxOKButton(CSRFitDialog *dialog, int ERId_Element):CSROKButton(dialog,ERId_Element)
{	
}

/*virtual*/	void	CSxOKButton::Action()
{
	CSRFitDialog* dlg = GetDialog();
	dlg->OnOK();
}