#pragma once
#include "SphInc/gui/SphEditElement.h"
#include "SphInc/gui/SphButton.h"
#include "SphInc/gui/SphRadioButton.h"
#include "SphInc/gui/SphCheckBox.h"
//#include "SphInc/gui/SphElement.h"

#define MIN_VALUE -1000000000
#define MAX_VALUE +1000000000

namespace eff
{
	namespace gui
	{
		class CtxEditText : public CSREditText
		{
		private:
			//bool canBeModifiedInAList;
			void (*OnValiding)(CtxEditText *);
		public:
			CtxEditText(CSRFitDialog *dialog, int ERId_Text, int maxCharacterCount, void (*f)(CtxEditText *) = NULL)
				: CSREditText(dialog, ERId_Text, maxCharacterCount), OnValiding(f) {}
			//CtxEditText(CSREditList *list, int CNb_Text, int maxCharacterCount, bool canBeModifiedInAList)
			//	: CSREditText(list, CNb_Text, maxCharacterCount), canBeModifiedInAList(canBeModifiedInAList) {}
			//virtual Boolean	CanBeModifiedInAList(void) const {
			//	return canBeModifiedInAList;
			//}

			//Events
			virtual void SetOnValiding(void(*f)(CtxEditText *)) { OnValiding = f; };

			//CSRElement
			virtual Boolean Validation(void);
		};

		class CtxStaticDouble : public CSRStaticDouble
		{
		public:
			CtxStaticDouble(CSRFitDialog* dialog, int ERId_Double, int numberOfDecimalPlaces = 2)
				: CSRStaticDouble(dialog, ERId_Double, numberOfDecimalPlaces, MIN_VALUE, MAX_VALUE) {}
			CtxStaticDouble(CSREditList* list, int ERId_Double, int numberOfDecimalPlaces = 2)
				: CSRStaticDouble(list, ERId_Double, numberOfDecimalPlaces, MIN_VALUE, MAX_VALUE) {}
		};

		class CtxEditDouble : public CSREditDouble
		{
		public:
			CtxEditDouble(CSRFitDialog* dialog, int ERId_Double, int numberOfDecimalPlaces = 2)
				: CSREditDouble(dialog, ERId_Double, numberOfDecimalPlaces, MIN_VALUE, MAX_VALUE) {}
			CtxEditDouble(CSREditList* list, int ERId_Double, int numberOfDecimalPlaces = 2)
				: CSREditDouble(list, ERId_Double, numberOfDecimalPlaces, MIN_VALUE, MAX_VALUE) {}
		};

		class CtxStaticLong : public CSRStaticLong
		{
		public:
			CtxStaticLong(CSRFitDialog *dialog, int ERId_Long) : CSRStaticLong(dialog, ERId_Long, MIN_VALUE, MAX_VALUE) {}
			CtxStaticLong(CSREditList *list, int CNb_Long) : CSRStaticLong(list, CNb_Long, MIN_VALUE, MAX_VALUE) {}
		};

		class CtxEditShort : public CSREditShort
		{
		public:
			CtxEditShort(CSRFitDialog *dialog, int ERId_Short) : CSREditShort(dialog, ERId_Short, (short)MIN_VALUE, (short)MAX_VALUE) {}
			CtxEditShort(CSREditList *list, int CNb_Short) : CSREditShort(list, CNb_Short, (short)MIN_VALUE, (short)MAX_VALUE) {}
		};

		class CtxButton : public CSRButton
		{
		private:
			void (*OnClick)(CtxButton *);
		public:
			CtxButton(CSRFitDialog *dialog, int ERId_Element, void (*f)(CtxButton *) = NULL)
				: CSRButton(dialog, ERId_Element), OnClick(f) {}

			//Events
			virtual void SetOnClickHandler(void(*f)(CtxButton *)) { OnClick = f; };

			//CSRButton
			virtual void Action();
		};

		class CtxCheckBox : public CSRCheckBox
		{
		private:
			void(*OnCheckedChanged)(CtxCheckBox *, bool);
		public:
			CtxCheckBox(CSRFitDialog* dialog, int ERId_Element, Boolean value = false, void(*f)(CtxCheckBox *, bool) = NULL)
				: CSRCheckBox(dialog, ERId_Element, value), OnCheckedChanged(f) {}

			//Events
			virtual void SetOnCheckedChanged(void(*f)(CtxCheckBox *, bool)) { OnCheckedChanged = f; };

			//CSRElement
			virtual Boolean Validation(void);
		};

		class CtxRadioButton : public CSRRadioButton
		{
		private:
			void(*OnCheckedChanged)(CtxRadioButton *, short);
		public:
			CtxRadioButton(CSRFitDialog* dialog, int ERId_FirstButton, int ERId_LastButton, short value = 1, void(*f)(CtxRadioButton *, short) = NULL)
				: CSRRadioButton(dialog, ERId_FirstButton, ERId_LastButton, value), OnCheckedChanged(f) {}

			//Events
			virtual void SetOnCheckedChanged(void(*f)(CtxRadioButton *, short)) { OnCheckedChanged = f; };

			//CSRElement
			virtual Boolean Validation(void);
		};

		class CtxGuiUtils
		{
		public:
			static void Enabled(CSRElement *, bool enabled);
			static std::string GetOpenFile(const char * title, const char * filter, CSRElement *);
			static std::vector<std::string> GetOpenFileMulti(const char * title, const char * filter, CSRElement *);
		};
	}
}

