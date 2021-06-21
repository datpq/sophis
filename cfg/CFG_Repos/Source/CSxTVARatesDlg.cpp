#pragma warning(disable:4251)
/*
** Includes
*/

#include <afxwin.h>
#include "SphTools/base/CommonOS.h"

#include "SphInc/gui/SphButton.h"
#include "SphInc/gui/SphEditElement.h"
#include __STL_INCLUDE_PATH(map)
#include "Sphtools/compatibility/applemanager.h"
#include "SphLLInc/interface/dialogueinfos.h"

#include "CSxTVARates.h"
#include "CSxTVARatesDlg.h"

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
CSxTVARatesDlg::CSxTVARatesDlg() : CSRFitDialog()
{

	fResourceId	= IDD_TVA_RATES_DLG - ID_DIALOG_SHIFT;	

	NewElementList(eNbFields);

	int nb = 0;

	if (fElementList)
	{
		fElementList[nb++]	= new CSxOKButton(this);
		fElementList[nb++]	= new CSRCancelButton(this);

		fElementList[nb++] = new CSxTVARatesEditList(this,eTVARatesList);		
	}
	
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/void	CSxTVARatesDlg::Open(void)
{
	// Get data from the database
	CSRFitDialog::Open();	
	
	CSxTVARates::GetData();
	LoadFromTVARates();
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/	void CSxTVARatesDlg::OnOK()
{
	bool ret = true;

	if (GetModified())
	{								
		SaveToTVARates();		
	}

	CloseDlg();	
}

void CSxTVARatesDlg::LoadFromTVARates()
{	
	if (CSxTVARates::GetTVARatesListSize() > 0)
	{						
		CSxTVARatesEditList* pElem = (CSxTVARatesEditList*)GetElementByRelativeId(eTVARatesList);		

		size_t	nbResults	= CSxTVARates::GetTVARatesListSize();	
		pElem->SetLineCount((int)nbResults);

		_STL::map<long, CSxTVARates::SSxTVARatesData> listOfTVARates;
		CSxTVARates::GetTVARatesList(listOfTVARates);

		_STL::map<long, CSxTVARates::SSxTVARatesData>::iterator iter;
		int i = 0;
		for(iter = listOfTVARates.begin();iter != listOfTVARates.end(); iter++)
		{						
			CSRElement* pElement = NULL;

			CSxTVARates::SSxTVARatesData oneRow = iter->second;
			pElem->LoadLine(i);			

			pElement = pElem->GetElementByIndex(CSxTVARatesEditList::eID);
			pElement->SetValue(&(oneRow.fRateId));			
			pElement = pElem->GetElementByIndex(CSxTVARatesEditList::eRateType);
			pElement->SetValue(oneRow.fRateType);
			pElement = pElem->GetElementByIndex(CSxTVARatesEditList::eRateName);
			pElement->SetValue(oneRow.fRateName);
			double rateInPercent = oneRow.fRate*100;
			pElement = pElem->GetElementByIndex(CSxTVARatesEditList::eRate);
			pElement->SetValue(&rateInPercent);						

			pElem->SaveLine(i);
			i++;
		}
		pElem->Update();		
	}
}

void CSxTVARatesDlg::SaveToTVARates() const
{	
	CSxTVARates TVARates;
	CSxTVARatesEditList* pElem = (CSxTVARatesEditList*)GetElementByAbsoluteId(eTVARatesList);
	int nbLine = pElem->GetLineCount();		

	_STL::map<long, CSxTVARates::SSxTVARatesData> listOfTVARates;

	for (int i = 0 ;i < nbLine; i++)
	{
		CSxTVARates::SSxTVARatesData OneRow;
		pElem->LoadLine(i);
		CSRElement* element = NULL;
		element = pElem->GetElementByIndex(CSxTVARatesEditList::eID);
		element->GetValue(&(OneRow.fRateId));			
		element = pElem->GetElementByIndex(CSxTVARatesEditList::eRate);
		element->GetValue(&(OneRow.fRate));			
		OneRow.fRate /= 100.0;
		
		if (OneRow.fRate != CSxTVARates::GetTVARate(OneRow.fRateId))
		{
			listOfTVARates[OneRow.fRateId] = OneRow;
			TVARates.SetTVARate(OneRow.fRateId,OneRow.fRate);
		}
	}

	TVARates.SaveData(listOfTVARates);
}

//-------------------------------------------------------------------------------------------------------------
/*static*/ void CSxTVARatesDlg::Display()
{	
	//Check if the window is not already open
	CSRUserEditInfo *userInfo = new CSRUserEditInfo( 2002) ;
	if ( CSRFitDialog::IsActiveWindow( userInfo ) )
	{
		delete userInfo ;
		userInfo = NULL ;
	}
	else
	{
		// 3. Create the dialog instance
		CSxTVARatesDlg *dialog = new CSxTVARatesDlg();		
		dialog->DoDialog(false, "TVA Rates", true, userInfo);
	}
}

void CSxTVARatesDlg::CloseDlg()
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
/*-------------------------- CSxTVARatesEditList -------------------------*/

CSxTVARatesEditList::CSxTVARatesEditList(CSRFitDialog* dialog, int nre,
														 const char	*tableName) :
CSREditList(dialog, nre, tableName)
{
	fListeValeur = NULL;
	SetMaxSelection (-1);

	fColumnCount = eColumnCount;
	fColumns = new SSColumn[fColumnCount];
	int count = 0;

	fColumns[count].fColumnName		= "ID";
	fColumns[count].fColumnWidth	= 50;
	fColumns[count].fAlignmentType = aLeft;	
	fColumns[count].fElement = new CSRStaticLong(this, eID, 0, (_STL::numeric_limits<long>::max)());

	count++;

	fColumns[count].fColumnName		= "Type";
	fColumns[count].fColumnWidth	= 150;
	fColumns[count].fAlignmentType = aLeft;
	fColumns[count].fElement = new CSRStaticText(this,eRateType,NAME_SIZE);

	count++;

	fColumns[count].fColumnName		= "Name";
	fColumns[count].fColumnWidth	= 200;
	fColumns[count].fAlignmentType = aLeft;
	fColumns[count].fElement = new CSRStaticText(this,eRateName,NAME_SIZE);

	count++;

	fColumns[count].fColumnName		= "Rate (%)";
	fColumns[count].fColumnWidth	= 50;
	fColumns[count].fAlignmentType = aLeft;
	fColumns[count].fElement = new CSREditDouble(this,eRate,2,0,10000);		

	SetDynamicSize(true);
	SetLineCount(0);

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