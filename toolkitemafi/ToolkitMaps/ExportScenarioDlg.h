#ifndef __ExportScenarioDLG__H__
	#define __ExportScenarioDLG__H__
/*
** Includes
*/
#include "SphInc/gui/SphDialog.h"
#include "CtxEditList.h"

using namespace eff::gui;

namespace eff
{
	namespace maps
	{
		namespace gui
		{
			class ExportScenarioDlg : public sophis::gui::CSRFitDialog
			{
				//------------------------------------ PUBLIC ------------------------------------
			public:

				/**
				* Constructor
				By default, it is the dialog resource ID is 6030
				*/
				ExportScenarioDlg();

				/**
				* Destructor
				*/
				virtual ~ExportScenarioDlg();

				/**
				* Performs actions in response to pressing the OK button.
				This method is invoked if the dialog contains an element of type CSRElement-derived CSROKButton.
				Upon pressing the OK button, CSRFitDialog::OnOK() is subsequently invoked from CSROKBouton::Action().
				@version 4.5.2
				*/
				virtual	void	OnOK();

				//------------------------------------ PROTECTED ----------------------------------
			protected:

				//------------------------------------ PRIVATE ------------------------------------
			private:

			};

			class EdlExportRows : public CtxEditList
			{
			public:
				EdlExportRows(CSRFitDialog *dialog, int ERId_List, void (*f)(CtxEditList *, int lineNumber) = NULL)
					: CtxEditList(dialog, ERId_List, f)
				{
					Initialize();
				};
				virtual void Initialize();
				virtual void LoadData(const char * query);
			};
		}
	}
}

#endif // !__ExportScenario__H__