
/*
** Includes
*/
// specific
#include "CSxTransactionDlg.h"
#include "CSxCheckBox.h"
#include "../MediolanumConstants.h"
#include "SphInc/static_data/SphCurrency.h"
#include  "..\Tools\CSxSQLHelper.h"
#include "SphTools/SphLoggerUtil.h"

/*
** Namespace
*/
using namespace sophis::gui;
using namespace sophis::value;
using namespace std;

/*
** Methods
*/
const char* CSxTransactionDlg::__CLASS__ = "CSxTransactionDlg";
const char* tradingHrsCheckName = MEDIO_GUI_FIELDNAME_OUTSIDE_HRS;
const char* tradingHrsLogName = MEDIO_GUI_FIELDNAME_OUTSIDE_HRS_LOG;
const char* parentOrderIDName = MEDIO_GUI_FIELDNAME_PARENT_ORDER_ID;
const char* rbcTradeIDName = MEDIO_GUI_FIELDNAME_RBC_TRADE_ID;
const char* rbcCommentName = MEDIO_GUI_FIELDNAME_RBC_COMMENT;
const char* rbcCapsRefName = MEDIO_GUI_FIELDNAME_RBC_CAPSREF;
const char* rbcUCITSVName = MEDIO_GUI_FIELDNAME_RBC_UCITSVCODE;
const char* rbcTransTypeName = MEDIO_GUI_FIELDNAME_RBC_TRANSTYPE;

std::vector<long> CSxTransactionDlg::AllotmentsCDSlist;
long CSxTransactionDlg::_tradeDateInit = 0;

//-------------------------------------------------------------------------------------------------------------
WITHOUT_CONSTRUCTOR_DIALOG(CSxTransactionDlg)

//-------------------------------------------------------------------------------------------------------------
CSxTransactionDlg::CSxTransactionDlg() : CSAMTransactionDialog()
{
	fResourceId	= IDD_TRANSACTION_DIALOG - ID_DIALOG_SHIFT;
		
	// MANDATORY: begin
	// specify the number of toolkit fields in the ticked dialog of value
	// it is VERY IMPORTANT for the memory allocation 
	// to add CSAMTransactionDialog::cCOUNT to your own toolkit field number
	int nbCount2  					= CSAMTransactionDialog::cCOUNT + eNbFields;
	CSRElement ** fElementList2		= new CSRElement*[nbCount2];

	if (fElementList2)
	{
		for (int i = 0; i < CSAMTransactionDialog::cCOUNT; i++) 
		{
			fElementList2[i] = fElementList[i];
		}
		// MANDATORY: end

		// ADD HERE YOUR TOOLKIT FIELDS
		// number of elements
		int nb = CSAMTransactionDialog::cCOUNT;

		fElementList2[nb++] = new CSxCheckBox(this, eTradingHoursCheck, false, tradingHrsCheckName);
		fElementList2[nb++] = new CSREditText(this, eTradingHoursCheckLog, 256, 0, tradingHrsLogName);
		fElementList2[nb++] = new CSREditLong(this, eParentOrderId, 0, 100000000, 0, parentOrderIDName);
		fElementList2[nb++] = new CSREditText(this, eRBCTradeId, 240,NULL,rbcTradeIDName);
		fElementList2[nb++] = new CSRStaticText(this,eGrossConsText,100);
		fElementList2[nb++] = new CSREditDouble(this,eGrossConsAmount,2,0,999999,0,"MEDIO_GROSS_CONS_AMOUNT",false,false,"MEDIO_GROSS_CONS_AMOUNT");
		fElementList2[nb++] = new CSRStaticText(this,eGrossConsCCY,3);
		fElementList2[nb++] = new CSRStaticText(this, eMedioPriceLabel, 40, "Trade Price");
		fElementList2[nb++] = new CSREditDouble(this, eMedioPriceValue, 8, 0, 1000, 0.0, "MEDIO_CDS_IMPORTPRICE", false, false, "MEDIO_CDS_IMPORTPRICE");
		fElementList2[nb++] = new CSREditText(this, eRBCComment, 240, NULL, rbcCommentName);
		fElementList2[nb++] = new CSREditText(this, eRBCCapsRef, 240, NULL, rbcCapsRefName);
		fElementList2[nb++] = new CSREditText(this, eRBCUcitsv, 240, NULL, rbcUCITSVName);
		fElementList2[nb++] = new CSREditText(this, eRBCTransType, 240, NULL, rbcTransTypeName);
		// fElementList2[nb++] = new ...

		// END ADD HERE YOUR TOOLKIT FIELDS
		// MANDATORY: begin
		// memory reallocation
		CSRAssignement<CSRElement **> * assignement = new CSRAssignement<CSRElement**>(fElementList, fElementList2);
		CSRAssignement<int> * fCount = new CSRAssignement<int>(fElementCount, nbCount2);
		// MANDATORY: end
	}
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/	void CSxTransactionDlg::Open(void)
{
	BEGIN_LOG("Open");
	CSAMTransactionDialog::Open();
	if (CSxTransactionDlg::AllotmentsCDSlist.size() == 0)
	{
		CSxTransactionDlg::AllotmentsCDSlist = CSxSQLHelper::GetAllotmentsFromConfig();
	}
	END_LOG();
	//TO DO
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CSxTransactionDlg::OpenAfterInit(void)
{
	BEGIN_LOG("OpenAfterInit");
	CSAMTransactionDialog::OpenAfterInit();
	unique_ptr<CSRTransaction> trade(this->new_CSRTransaction());
	_tradeDateInit = trade->GetTransactionDate();

	CSRElement* elementHedgeCheck = GetElementByAbsoluteId(eTradingHoursCheck);
	bool isChecked = false;
	elementHedgeCheck->GetValue(&isChecked);
	elementHedgeCheck->Disable();
	if(!isChecked)
	{	
		GetElementByAbsoluteId(eTradingHoursCheckLog)->Hide();
	}

	CheckInstrumentType();

	CSRElement* medioPriceLabel  = GetElementByAbsoluteId(eMedioPriceLabel);
	CSRElement* medioPriceValue  = GetElementByAbsoluteId(eMedioPriceValue);

	if(medioPriceLabel!=nullptr&& medioPriceValue!=nullptr)
	{
		double medioPriceSet=0;
		medioPriceValue->GetValue(&medioPriceSet);
		if(medioPriceSet==0)
		{
			medioPriceLabel->Hide();
			medioPriceValue->Hide();
			RefreshMedioPriceAmountsOrQty(true);
		}
		else
		{
			medioPriceLabel->SetValue("Trade Price");
			medioPriceLabel->Show(true);
			medioPriceValue->Show(true);
			RefreshMedioPriceAmountsOrQty(true);
		}
	}

	END_LOG();
	//TO DO
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/void CSxTransactionDlg::ElementValidation(int EAId_Modified)
{
	BEGIN_LOG("ElementValidation");
	CSAMTransactionDialog::ElementValidation(EAId_Modified);

	unique_ptr<CSRTransaction> t(this->new_CSRTransaction());
	double grossCons = 0.0;
	CSRElement* grossConsAmount = GetElementByAbsoluteId(eGrossConsAmount);
	double grossAmount = t->GetGrossAmount();
	double accrued = t->GetAccruedAmount();
	CSRTransaction::ePaymentCurrencyType ePCT = t->GetPaymentCurrencyType();
	if(ePCT == CSRTransaction::ePaymentCurrencyType::pcUnderlying)
	{

		CSRTransaction::eForexCertaintyType	CertaintyType =	t->GetForexCertaintyType();
		double TradeForex = t->GetForexSpot();
		if(CertaintyType == CSRTransaction::eForexCertaintyType::fcCertain)
			accrued = accrued*TradeForex;
		else
			accrued = accrued/TradeForex;
	}

	double checkGrossCons = grossAmount-accrued;
	grossConsAmount->GetValue(&grossCons);

	if(abs(grossCons - checkGrossCons)>10e-4)
	{

		grossConsAmount->SetValue(&checkGrossCons);
		
	}

	if(EAId_Modified == eDIReference)
		CheckInstrumentType();

	// Handling of the CCY for gross consideration
	if(EAId_Modified == eDISettlementCurrency)
	{
		//if(t->GetPaymentCurrencyType()== CSRTransaction::ePaymentCurrencyType::pcSettlement)
		{
			long ccy = t->GetSettlementCurrency();
			char cur[4] = "";
			CSRElement* grossConsCCY  = GetElementByAbsoluteId(eGrossConsCCY);
			CSRCurrency::CurrencyToString(ccy,cur);
			grossConsCCY->SetValue(&cur);
			grossConsCCY->Show(true);
		}
	}

	if (CheckAllotment(t.get()))//this applies only for CDS/CDX configured allotments
	{

		if (EAId_Modified == eDIPriceType)
		{

			bool canBe = 1;
			eAskQuotationType instrPryceType = t->GetInstrument()->GetAskQuotationType(&canBe, &canBe, &canBe);
			eAskQuotationType priceType = t->GetAskQuotationType();

			if (instrPryceType == eAskQuotationType::aqInRate && priceType != eAskQuotationType::aqInRate)
			{
				CSRFitDialog::Message("Instrument price is expected to be spread.Please check instrument quotation unit with Product support team.");
			}
			else if (instrPryceType == eAskQuotationType::aqInPercentage && priceType == eAskQuotationType::aqInRate)
			{
				CSRFitDialog::Message("Instrument price is not expected to be spread.Please check instrument quotation unit with Product support team.");
			}


			ResetMedioPrice();
		}

		if (EAId_Modified == eDIReference)
		{
			ResetMedioPrice();
		}

		eAskQuotationType priceType = t->GetAskQuotationType();

		if (EAId_Modified == eDISpot)
		{
			if (priceType == eAskQuotationType::aqInPercentage)
			{
				CSRElement* medioPriceLabel = GetElementByAbsoluteId(eMedioPriceLabel);
				CSRElement* medioPriceValue = GetElementByAbsoluteId(eMedioPriceValue);

				if (medioPriceLabel != nullptr&& medioPriceValue != nullptr)
				{
					medioPriceLabel->SetValue("Trade Price");
					medioPriceLabel->Show(true);
					medioPriceValue->Show(true);

					double insertedPrice = t->GetSpot();

					if (insertedPrice > 100.0)
					{
						medioPriceValue->SetValue(&insertedPrice);
						double adjustedPrice = -(insertedPrice - 100.0);
						t->SetSpot(adjustedPrice);
					}

					if (insertedPrice > 0)
					{
						double adjustedtkt = 100.0 - insertedPrice;
						medioPriceValue->SetValue(&insertedPrice);
						t->SetSpot(adjustedtkt);
					}
					else
					{
						ResetMedioPrice();
						END_LOG();
						return;
					}
					double orgNetAmount = t->GetNetAmount();
					double grossAmt = t->GetGrossAmount();


					double revertedGrossAmt = -grossAmt;
					double revertedNetAmount = -orgNetAmount;

					t->SetNetAmountOnly(revertedNetAmount);
					UpdateElement(eDINetAmount);
					SetDoubleValue(eDIGrossAmount, revertedGrossAmt);

				}
			}
		}
		else
		{
			RefreshMedioPriceAmountsOrQty(false);
		}

		if (priceType == eAskQuotationType::aqInRate)
		{
			if (EAId_Modified == eDINegotiationDate || EAId_Modified == eDIQuantity || EAId_Modified == eDISpot)
			{
				double tradeAccured = t->GetAccruedAmount2();

				if (EAId_Modified != eDISpot)//sign already reverted when called for spot, avoiding double revert
				{
					if (tradeAccured != 0)
					{
						double reversedAccrued = -1 * tradeAccured;
						SetDoubleValue(eDIAccruedAmount2, reversedAccrued);
						tradeAccured = reversedAccrued;
					}
				}

				double netAmt = t->GetNetAmount();
				double diffAccrued = 0;
				long currentTradeDate = t->GetTransactionDate();
				if (_tradeDateInit != currentTradeDate)
				{
					t->SetTransactionDate(_tradeDateInit);
					double prevAccrued = t->GetAccruedAmount2();; //get the core accrued from the previous date
					t->SetTransactionDate(currentTradeDate);

					diffAccrued = abs(abs(prevAccrued) - abs(tradeAccured));
					LOG(Log::debug, FROM_STREAM("CSX/CDS in rate.Trade date modified!Accrued diff= " << diffAccrued));
				}
				LOG(Log::debug, FROM_STREAM("CSX/CDS in rate.Net amount is = " << netAmt));
				double updatedAmount = abs(netAmt) - diffAccrued;
				if (netAmt < 0)
					updatedAmount = -1 * updatedAmount;

				LOG(Log::debug, FROM_STREAM("CSX/CDS in rate.Recomputed net amount is = " << updatedAmount));
				LOG(Log::debug, FROM_STREAM("CSX/CDS in rate.Adjusted accrued is = " << tradeAccured));

				t->SetAccruedAmount2(tradeAccured);
				t->SetNetAmountOnly(updatedAmount);
				UpdateElement(eDINetAmount);
				SetDoubleValue(eDIGrossAmount, updatedAmount);

			}
		}
	}
	END_LOG();
	//TO DO
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/void CSxTransactionDlg::GetSpecificElements(_STL::vector<long> &elemsV)
{
	CSAMTransactionDialog::GetSpecificElements(elemsV);

//	elemsV.push_back(/*to complete*/);
}

void CSxTransactionDlg::CheckInstrumentType()
{

	CSRElement* grossConsText = GetElementByAbsoluteId(eGrossConsText);
	CSRElement* grossConsAmount = GetElementByAbsoluteId(eGrossConsAmount);
	CSRElement* grossConsCCY  = GetElementByAbsoluteId(eGrossConsCCY);
	CSRTransaction* trans = this->new_CSRTransaction();

	grossConsText->Hide();
	grossConsAmount->Hide();
	grossConsCCY->Hide();

	const CSRInstrument * ins = trans->GetInstrument();

	if(ins && ins->GetType()=='O')
	{
		grossConsText->SetValue("Gross Consideration");
		grossConsText->Show(true);
		grossConsAmount->Show(true);
		CSRTransaction* t = this->new_CSRTransaction();
		long ccy = t->GetSettlementCurrency();
		char cur[4] = "";
		CSRCurrency::CurrencyToString(ccy,cur);
		grossConsCCY->SetValue(&cur);
		grossConsCCY->Show(true);
	}

}

void CSxTransactionDlg::ResetMedioPrice()
{

	CSRElement* medioPriceValue = GetElementByAbsoluteId(eMedioPriceValue);

	if (medioPriceValue != nullptr)
	{
		double resetPriceValue = 0.0;
		medioPriceValue->SetValue(&resetPriceValue);
	}

}

void CSxTransactionDlg::RefreshMedioPriceAmountsOrQty(bool onOpenDialog)
{

	unique_ptr<CSRTransaction> t(this->new_CSRTransaction());

	eAskQuotationType priceType = t->GetAskQuotationType();
	if (priceType == eAskQuotationType::aqInPercentage)
	{
		const CSRInstrument * ins = t->GetInstrument();

		if (ins != nullptr)
		{
			long allotmentID = ins->GetAllotment();
			if (std::find(CSxTransactionDlg::AllotmentsCDSlist.begin(), CSxTransactionDlg::AllotmentsCDSlist.end(), allotmentID) != CSxTransactionDlg::AllotmentsCDSlist.end())
			{

				// the qty sign is flipped when saving the trade in the db( see CSxCDSPriceAction) so when opening the dialog we display the original value
				if(onOpenDialog==true)
				{
				    double tradeQuantity = t->GetQuantity();
					double revertedQty=-tradeQuantity;


					double nominal=t->GetNotional();
					double revertedNominal=-nominal;

					SetDoubleValue(eDIQuantity,revertedQty);
					SetDoubleValue(eDINominal,revertedNominal);
					double grossAmt= t->GetGrossAmount();
					
					SetDoubleValue(eDIGrossAmount,grossAmt);
				}
				else
				{

				double orgNetAmount=t->GetNetAmount();
				double grossAmt=t->GetGrossAmount();


				double revertedGrossAmt=-grossAmt;
				double revertedNetAmount=-orgNetAmount;

				t->SetNetAmountOnly(revertedNetAmount);
				UpdateElement(eDINetAmount);
				SetDoubleValue(eDIGrossAmount,revertedGrossAmt);
				}
			}
		}
	}
}


bool CSxTransactionDlg::CheckAllotment(CSRTransaction * trade)
{
	bool result = false;
	const CSRInstrument * ins = trade->GetInstrument();
	if (ins != nullptr)
	{
		long allotmentID = ins->GetAllotment();
		if (std::find(CSxTransactionDlg::AllotmentsCDSlist.begin(), CSxTransactionDlg::AllotmentsCDSlist.end(), allotmentID) != CSxTransactionDlg::AllotmentsCDSlist.end())
		{
			result = true;
		}
	}

	return result;
}