#ifndef ___CSxCheckBox_H___
#define ___CSxCheckBox_H___

/*
** Includes
*/
#include "SphInc/gui/SphCheckBox.h"
#include "SphInc/gui/SphDialog.h"

/*
** Class
*/
class CSxCheckBox : public sophis::gui::CSRCheckBox
{
//------------------------------------ PUBLIC ------------------------------------
public:

	/**Constructor.
	The constructor CSRCheckBox::CSRCheckBox() calls the constructor CSRElement::CSRElement() to which it passes
	on the parameters dialog, ERId_Element and columnName, then initialises the fields fValue and fNoDlgModif.
	The parameter fieldInTable is effective only when deriving a generic security dialog or a model.
	It is always assumed that the name of all user-created fields obey the
	following pattern : ZZZ_name_of_field where ZZZ stands for the initials of the bank.
	@param dialog points to the dialog to which the checkbox belongs.
	@param ERId_Element is the relative number of the checkbox. If not strictly positive, CSRFitDialog will
	assume that there is no such element in the associated resource, hence the CSRCheckBox will not be visually handled.
	However, the column columnName corresponding to this checkbox will be taken into account whenever necessary and its content will always be accessible.
	@param value is the default value (fValue) set to false. This parameter is used to initialise fValue if columnName is nvZero or whenever creating a security.
	@param columnName is the name of a Sophis Xxx table column handled by the CSRXxx object.
	For instance, ZZZ_MyColumn of the table TITRES is handled by CSRInstrument. If this parameter is not nvZero and
	if this security exists, the content of ZZZ_MyColumn with respect to this security will be used (instead of value) to initialise fValue.
	@param noDlgModif is to state whether the value of the checkbox can be modified.
	@param tagColonne is the tag for the column for the DataService; the possible values are a string, kSameAsOracleName, or no tag.
	@version 4.5.2.1 new parameter tagColonne.
	*/
	CSxCheckBox(sophis::gui::CSRFitDialog*	dialog,
					int 			ERId_Element,
					Boolean			value=false,
					const char*			columnName = kUndefinedField,
					bool			noDlgModif=false,
					const char *	tagColonne = kSameAsOracleName);
	virtual Boolean	StringToValue(const char *sourc, int line);

	virtual void	ValueToString(char *dest, int line) const;

//------------------------------------ PROTECTED ------------------------------
protected:


//------------------------------------ PRIVATE ------------------------------------
private:

	/** For log purpose
	@version 1.0.0.0
	*/
	static const char * __CLASS__;
};

#endif //!___CSxCheckBox_H___