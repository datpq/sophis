#include "SphInc/gui/SphButton.h"
#include "SphInc/gui/SphEditElement.h"
#include "CSxAccountEODResultDlg.h"

/*static*/ char* CSxAccountEODResultDlg::__CLASS__ = "CSxAccountEODResultDlg";

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CSxAccountEODResultDlg::CSxAccountEODResultDlg(SSxdata &data) : CSRFitDialog()
{

	fResourceId	= IDD_CSxAccountEODResults_DLG - ID_DIALOG_SHIFT;

	NewElementList(eNbFields);

	int nb = 0;

	if (fElementList)
	{
		fElementList[nb++]	= new CSROKButton(this,eOK);
		fElementList[nb++]	= new CSRCancelButton(this);
		fElementList[nb++]	= new CSRStaticDouble(this,eNetAsset,2,0.0,1000000000000,0.0);
		fElementList[nb++]	= new CSRStaticDouble(this,eFees,2,0.0,1000000000000,0.0);
		fElementList[nb++]	= new CSRStaticDouble(this,eAgios,2,0.0,1000000000000,0.0);
		fElementList[nb++]	= new CSRStaticDouble(this,eNumberOfShares,2,0.0,1000000000000,0.0);
		fElementList[nb++]	= new CSRStaticDouble(this,eNavPerShare,2,0.0,1000000000000,0.0);
	}

	fData = data;
}

/*virtual*/ void	CSxAccountEODResultDlg::OpenAfterInit(void)
{
	GetElementByRelativeId(eNetAsset)->SetValue(&fData.fNetAsset);
	GetElementByRelativeId(eFees)->SetValue(&fData.fFees);
	GetElementByRelativeId(eAgios)->SetValue(&fData.fAgios);
	GetElementByRelativeId(eNumberOfShares)->SetValue(&fData.fNumberOfShares);
	GetElementByRelativeId(eNavPerShare)->SetValue(&fData.fNavPerShare);
}

/*virtual*/	void	CSxAccountEODResultDlg::OnOK()
{
	EndDialog();
}

