/*
** Includes
*/
#include "GestionOdDlg.h"
#include "Resource\resource.h"
#include "SphInc/gui/SphButton.h"
#include "SphInc/gui/SphEditElement.h"
#include "TabPageConsultationDlg.h"
#include "TabPageValidationDlg.h"
#include "TabPageInsertionDlg.h"
#include "SphInc/SphUserRights.h"
#include "SphSDBCInc/SphSQLQuery.h"
#include "SqlUtils.h"

/*
** Namespace
*/
using namespace sophis::gui;
using namespace eff::emafi::gui;

using namespace sophis::sql;
using namespace std;
/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------


	struct StUserRights {
		long id_status;
		char caption[50];
		char profile[50];
	};

	TabGestionOD::TabGestionOD(CSRFitDialog *dialog, int ERId_Element) : CSRTabButton(dialog, ERId_Element)
	{
	}

	void TabGestionOD::Open()
	{
		CSRFitDialog * dialog_insert = new TabPageInsertionDlg();
		dialog_insert->SetTitle("Insertion");
		AppendPage(dialog_insert);

		CSRStructureDescriptor * gabarit = new CSRStructureDescriptor(3, sizeof(StUserRights));
		ADD(gabarit, StUserRights, id_status, rdfInteger);
		ADD(gabarit, StUserRights, caption, rdfString);
		ADD(gabarit, StUserRights, profile, rdfString);
		//7.1.3
		//try {
			char query[SQL_LEN] = {'\0'};
			_snprintf_s(query, sizeof(query), "SELECT ID_STATUS, VALIDATION_CAPTION, PROFILE FROM EMAFI_ODSTATUS WHERE ID_STATUS > 0 ORDER BY 1");	

			StUserRights * arrRights;
			int count = 0;
			CSRUserRights user;

			errorCode err  = QueryWithNResultsArray(query, gabarit, (void **)&arrRights, &count);
			
			for(int i=count-1;i>=0; i--)
			{
				if(user.HasAccess(arrRights[i].profile))
				{
					TabPageConsultationDlg* dialog_consult = new TabPageConsultationDlg(arrRights[i].id_status);							
					dialog_consult->SetTitle("Consultation");
					AppendPage(dialog_consult);
					break;
				}
			}

			for(int i=1; i<count; i++)
			{
				if(user.HasAccess(arrRights[i].profile))
				{

					TabPageValidationDlg* dialog_valid = new TabPageValidationDlg(arrRights[i].id_status);							
					dialog_valid->SetTitle(string(arrRights[i].caption).c_str());
					AppendPage(dialog_valid);
				}
			}
		//} catch (const sophis::sql::OracleException &ex) {
		//	throw;
		//}
		delete gabarit;
	}


GestionOdDlg::GestionOdDlg() : CSRFitDialog()
{

	fResourceId	= IDD_DLG_GestionOd - ID_DIALOG_SHIFT;

	NewElementList(2);

	int nb = 0;

	if (fElementList)
	{
		fElementList[nb++]	= new TabGestionOD(this, IDC_TAB_GestionOD - ID_ITEM_SHIFT);
		fElementList[nb++]	= new CSRCancelButton(this);
	}
	
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ GestionOdDlg::~GestionOdDlg()
{
}