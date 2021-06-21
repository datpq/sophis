
#pragma warning(disable:4251) // '...' : struct '...' needs to have dll-interface to be used by clients of class '...'
#include "SphInc/gui/SphMenu.h"
#include "SphInc/value/kernel/SphAmManagementFees.h"
#include <stdio.h>

#include "CSxCustomMenu.h"
#include "Constants.h"

using namespace _STL;
using namespace sophis::value;

using namespace sophis::gui;

///////////////////////////////////////////////////////////////////////////
/*-------------------------- CSxCustomMenuLong -------------------------*/

CSxCustomMenuLong::CSxCustomMenuLong(CSRFitDialog * dialog, 
									 int ERId_Menu, 
									 bool editable,
									 NSREnums::eDataType valueType,
									 unsigned int valueSize, 
									 short listValue, 
									 const char * columnName)
	: CSRCustomMenu(dialog, ERId_Menu, editable, valueType, valueSize, listValue, columnName)
{					
}

CSxCustomMenuLong::~CSxCustomMenuLong()
{}

void CSxCustomMenuLong::BuildMenu()
{}

void CSxCustomMenuLong::SetValueFromList()
{
	if((short)fData.size() && (short)fData.size() >= fListValue)
		*((long*)fValue) = fData[fListValue - 1];
	else
		*((long*)fValue) = 0;
}

void CSxCustomMenuLong::SetListFromValue()
{
	for (size_t i = 0; i < fData.size(); i++)
	{
		if (fData[i] == *((long*)fValue))
		{
			fListValue = (short)i + 1;
			return;
		}
	}
	fListValue = 0;
}

////////////////////////////////////////////////////////////////////////////////
/*-------------------------- CSxNAVTypeMenu -------------------------*/

CSxNAVTypeMenu::CSxNAVTypeMenu(CSRFitDialog	*dialog, int ERId_Menu, const char *columnName)
	: CSxCustomMenuLong(dialog, ERId_Menu, true, NSREnums::dLong, 0, 2, columnName)
{						
	fData.push_back(CSAMFundManagementFees::ntBeginning);
	fData.push_back(CSAMFundManagementFees::ntEnd);
	fData.push_back(CSAMFundManagementFees::ntAverage);
	fData.push_back(CSAMFundManagementFees::ntDayToDay);

	fDecalageInitial = 0;
	BuildMenu();
	SetValueFromList();
}

void CSxNAVTypeMenu::BuildMenu()
{
	for (size_t i = 0; i < fData.size(); i++ )
	{		
		char buffer[NAME_SIZE];
		switch(fData[i])
		{
		case CSAMFundManagementFees::ntBeginning:
			sprintf_s(buffer,NAME_SIZE,"Period beginning");
			break;
		case CSAMFundManagementFees::ntEnd:
			sprintf_s(buffer,NAME_SIZE,"Period end");
			break;
		case CSAMFundManagementFees::ntAverage:
			sprintf_s(buffer,NAME_SIZE,"Average");
			break;
		case CSAMFundManagementFees::ntDayToDay:
			sprintf_s(buffer,NAME_SIZE,"Day to day");
			break;		
		}

		this->AddElement(buffer);				
	}
}

////////////////////////////////////////////////////////////////////////////////
/*-------------------------- CSxEditListFees -------------------------*/

CSxEditListFees::CSxEditListFees(CSRFitDialog* dialog, int nre,
								 const char	*tableName) :
CSREditList(dialog, nre, tableName, 50)
{
	fListeValeur = NULL;
	SetMaxSelection (-1);		
	
	fColumnCount = 2;
	fColumns = new SSColumn[fColumnCount];

	fColumns[0].fColumnName		= "Level" ;
	fColumns[0].fColumnWidth	= 101;
	fColumns[0].fAlignmentType = aLeft;
	fColumns[0].fElement = new CSREditDouble(this,0,2,0,1000000000000,0,kUndefinedField,true);

	fColumns[1].fColumnName		= "Rate (%)" ;
	fColumns[1].fColumnWidth	= 101;
	fColumns[1].fAlignmentType = aLeft;
	fColumns[1].fElement = new CSREditDouble(this,1,5,0,10000,0,kUndefinedField,false);

	SetDynamicSize(true);
	SetLineCount(0);	
}

void CSxEditListFees::OnAddElement()
{	
	Enlarge(1);	
	SaveLine(GetLineCount()-1);

}

void CSxEditListFees::OnRemoveElement()
{	
	vector<long>& selection = GetSelectedLines();	
	for (int i = (int)selection.size()-1; i>=0; i--)
	{
		RemoveLine(selection[i]);
	}
}

//Add button for fees list-----------------------------------------------------------
CSxEditListFees::FeesAddButton::FeesAddButton(CSRFitDialog* dialog, int nre, CSxEditListFees* feeslist) : 
CSRButton(dialog, nre),
fFeesList (feeslist)
{
}

void CSxEditListFees::FeesAddButton::Action()
{
	fFeesList->OnAddElement();
}

//Remove button for fees list---------------------------------------------------------
CSxEditListFees::FeesRemoveButton::FeesRemoveButton(CSRFitDialog* dialog, int nre, CSxEditListFees * feeslist) : 
CSRButton(dialog, nre),
fFeesList (feeslist)
{
}

void CSxEditListFees::FeesRemoveButton::Action()
{
	fFeesList->OnRemoveElement();
}
