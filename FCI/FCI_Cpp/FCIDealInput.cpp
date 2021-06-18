#include "SphTools/SphLoggerUtil.h"
#include <afxwin.h> //for HWND and CWnd 
#include "SphLLInc\interface\transdlog.h" // for TItem 

/*
** Includes
*/
// specific
#include "FCIDealInput.h"

/*
** Namespace
*/
using namespace sophis::gui;
using namespace sophis::value;

/*
** Static
*/
const char * FCIDealInput::__CLASS__ = "FCIDealInput";

//-------------------------------------------------------------------------------------------------------------
WITHOUT_CONSTRUCTOR_DIALOG(FCIDealInput)

//-------------------------------------------------------------------------------------------------------------
FCIDealInput::FCIDealInput() : CSAMTransactionDialog()
{
	BEGIN_LOG("FCIDealInput");

	try
	{
		fResourceId = IDD_TRANSACTION_DIALOG - ID_DIALOG_SHIFT;

		int nb = InitElementList(CSAMTransactionDialog::cCOUNT, eNbFields);

		if (fElementList)
		{
			fElementList[nb++] = new CSREditDouble(this, eNewQuantity, 6, 0, 9999999999, 0, "NEW_QUANTITY");
			fElementList[nb++] = new CSRStaticText(this, eNewQuantityLabel, 100);
		}
	}
	catch (ExceptionBase & ex)
	{
		MESS(Log::error, "FCIDealInput failed: (" << (const char *)ex << ")");
	}
	catch (...)
	{
		MESS(Log::error, "FCIDealInput unknown error");
	}

	END_LOG();
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/	void FCIDealInput::Open(void)
{
	CSAMTransactionDialog::Open();

	//TO DO
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void FCIDealInput::OpenAfterInit(void)
{
	CSAMTransactionDialog::OpenAfterInit();

	BEGIN_LOG("OpenAfterInit");

	try
	{
		//GetElementByRelativeId does not work with standard fields, so here we have to get the handle of the field by window function
		HWND hwnd = FindWindowEx(NULL, NULL, NULL, _T("Deal Input"));
		// or you can take the current dialog (which is the deal input ) like this: HWND lngHWnd = GetHWND(); 

		TItem * item = 0;
		CWnd* pWndTDlg = CWnd::FromHandle(hwnd);
		if (pWndTDlg)
		{
			CWnd* pWnd = pWndTDlg->GetDlgItem(eDealInputID::eDIQuantity + ID_ITEM_SHIFT);
			FindUserItem(GetDlog(), eDealInputID::eDIQuantity, &item);
			if (item)
				quantityValue = item->infos.editdouble.value; // to check the value of the field 
		}
	}
	catch (ExceptionBase & ex)
	{
		MESS(Log::error, "OpenAfterInit failed: (" << (const char *)ex << ")");
	}
	catch (...)
	{
		MESS(Log::error, "OpenAfterInit unknown error");
	}

	END_LOG();
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/void FCIDealInput::ElementValidation(int EAId_Modified)
{
	CSAMTransactionDialog::ElementValidation(EAId_Modified);

	BEGIN_LOG("ElementValidation");

	try
	{
		double quantity = 0.;
		double newQuantity = 0.;
		CSRTransaction* transaction = new_CSRTransaction();
		if (transaction)
		{
			switch (EAId_Modified)
			{
			case eDIQuantity:
				quantity = transaction->GetQuantity();
				if (quantity != 0) {
					newQuantity = 1 / quantity;
					GetElementByRelativeId(eNewQuantity)->SetValue(&newQuantity);
				}
				break;
			case eNewQuantity:
				GetElementByRelativeId(eNewQuantity)->GetValue(&newQuantity);
				if (newQuantity != 0) {
					quantity = 1 / newQuantity;
					if (quantityValue) {
						*quantityValue = quantity;
						UpdateItem(GetDlog(), eDealInputID::eDIQuantity);
					}
				}
				break;
			default:
				break;
			}

			delete transaction;
		}
	}
	catch (ExceptionBase & ex)
	{
		MESS(Log::error, "ElementValidation failed: (" << (const char *)ex << ")");
	}
	catch (...)
	{
		MESS(Log::error, "ElementValidation unknown error");
	}

	END_LOG();
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/void FCIDealInput::GetSpecificElements(std::vector<long> &elemsV)
{
	CSAMTransactionDialog::GetSpecificElements(elemsV);

	//	elemsV.push_back(/*to complete*/);
}
