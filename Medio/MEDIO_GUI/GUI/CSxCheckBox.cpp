/*
** Includes
*/
#include "CSxCheckBox.h"
#include "SphTools/SphLoggerUtil.h"
#include <stdio.h>
/*
** Statics
*/
/*static*/ const char* CSxCheckBox::__CLASS__ = "CSxCheckBox";

/*
** Methods
*/

//-------------------------------------------------------------------------------------------------------------
CSxCheckBox::CSxCheckBox(CSRFitDialog*	dialog,	int ERId_Element, Boolean value/*=false*/,
						const char* columnName /*= kUndefinedField*/,bool noDlgModif/*=false*/,
						const char *	tagColonne /*= kSameAsOracleName*/)	
: CSRCheckBox(dialog, ERId_Element,value,columnName,noDlgModif,tagColonne)
{

}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ Boolean	CSxCheckBox::StringToValue(const char *sourc, int line)
{
	BEGIN_LOG("StringToValue");

	if (_stricmp(sourc, "TRUE") == 0 || _stricmp(sourc, "1") == 0)
	{
		fValue = true;
		return true;
	}
	else if (_stricmp(sourc, "FALSE") == 0 || _stricmp(sourc, "0") == 0)
	{
		fValue = false;
		return true;
	}
	END_LOG();
	return false;
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CSxCheckBox::ValueToString(char *dest, int line) const
{
	BEGIN_LOG("ValueToString");

	if (fValue == true)
	{
		sprintf_s(dest, sizeof(dest), "TRUE");
	}
	else
	{
		sprintf_s(dest, sizeof(dest), "FALSE");
	}
	END_LOG();
}