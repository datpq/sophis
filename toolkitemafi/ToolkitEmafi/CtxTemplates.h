#pragma once
#include "CtxComboBox.h"

namespace eff
{
	namespace emafi
	{
		namespace gui
		{
			class ComboBoxFolio : public eff::gui::CtxComboBox
			{
			public:
				ComboBoxFolio(CSRFitDialog *dialog, int ERId_Menu, void (*f)(CtxComboBox *) = NULL);
				ComboBoxFolio(CSREditList *list, int ERId_Menu, void (*f)(CtxComboBox *) = NULL);
			};

			class ComboBoxFund : public eff::gui::CtxComboBox
			{
			public:
				ComboBoxFund(CSRFitDialog *dialog, int ERId_Menu, void(*f)(CtxComboBox *) = NULL);
				ComboBoxFund(CSREditList *list, int ERId_Menu, void(*f)(CtxComboBox *) = NULL);
			};

			class ComboBoxFundFolio : public eff::gui::CtxComboBox
			{
			public:
				ComboBoxFundFolio(CSRFitDialog *dialog, int ERId_Menu, void(*f)(CtxComboBox *) = NULL);
				ComboBoxFundFolio(CSREditList *list, int ERId_Menu, void(*f)(CtxComboBox *) = NULL);
			};

			class ComboBoxAccountNumber : public eff::gui::CtxComboBox
			{
			public:
				ComboBoxAccountNumber(CSRFitDialog *dialog, int ERId_Menu, void (*f)(CtxComboBox *) = NULL);
				ComboBoxAccountNumber(CSREditList *list, int ERId_Menu, void (*f)(CtxComboBox *) = NULL);
			};
		}
	}
}