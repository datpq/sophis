#ifndef __CSxAccountEODResultDlg__H__
#define __CSxAccountEODResultDlg__H__
/*
** Includes
*/
#include "SphInc/gui/SphDialog.h"
#include "SphInc/gui/SphEditList.h"
#include "SphInc/gui/SphButton.h"
#include "..\Resource\resource.h"

/*
** Class
*/

class CSxAccountEODResultDlg : public sophis::gui::CSRFitDialog
{
	//------------------------------------ PUBLIC ------------------------------------
public:

	struct SSxdata;

	/**
	* Constructor
	By default, it is the dialog resource ID is 6030
	*/
	CSxAccountEODResultDlg(SSxdata &data);
	

	virtual void	OpenAfterInit(void);
	virtual	void	OnOK();

	struct SSxdata
	{
		SSxdata()
		{
			fNetAsset = 0.0;
			fFees = 0.0;
			fAgios = 0.0;
			fNumberOfShares = 0.0;
			fNavPerShare = 0.0;
		}

		double fNetAsset;
		double fFees;
		double fAgios;
		double fNumberOfShares;
		double fNavPerShare;
	};

	// Fields enumeration
	// for every new item in dialog, add its enumeration here...
	enum // already without ID_ITEM_SHIFT
	{
		eOK = 1,
		eCancel,
		eNetAsset,
		eFees,
		eAgios,
		eNumberOfShares,
		eNavPerShare,
		eNbFields = 7
	};

	//------------------------------------ PROTECTED ----------------------------------
protected:

	//------------------------------------ PRIVATE ------------------------------------
private:

	static char* __CLASS__;

	SSxdata fData;

};

#endif // !__CSxAccountEODResultDlg__H__