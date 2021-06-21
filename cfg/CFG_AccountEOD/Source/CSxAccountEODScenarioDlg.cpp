/*
** Includes
*/

#include "CSxAccountEODScenarioDlg.h"
#include "SphInc/gui/SphButton.h"
#include "SphInc/gui/SphEditElement.h"
#include "SphSDBCInc/SphSQLQuery.h"
#include "SphInc/SphUserRights.h"
#include "SphTools/SphLoggerUtil.h"
#include <stdio.h>
#include __STL_INCLUDE_PATH(map)
#include __STL_INCLUDE_PATH(vector)
#include __STL_INCLUDE_PATH(string)
//DPH
//#include "stlport/stl/_auto_ptr.h"
#include "SphInc/market_data/SphMarketData.h"
#include "SphInc/scenario/SphScenario.h"
#include "SphInc/portfolio/SphPortfolio.h"
#include "SphTools/SphExceptions.h"
#include "SphTools/SphDay.h"
#include "SphInc/static_data/SphCurrency.h"
#include "SphInc/value/kernel/SphFund.h"
#include "SphInc/fund/SphFundBase.h"
//DPH
#if (TOOLKIT_VERSION < 720)
#include "SphLLInc\misc\ConfigurationFileWrapper.h";
#else
#include "SphInc\misc\ConfigurationFileWrapper.h";
#endif
#include "SphInc/value/kernel/SphFundPurchase.h"
#include "SphInc/value/kernel/SphFund.h"
#include "SphInc/backoffice_kernel/SphKernelStatusGroup.h"
#include "CSxAccountEODResultDlg.h"
/*
** Namespace
*/
using namespace sophis::gui;
using namespace sophis::sql;
using namespace sophis::market_data;
using namespace sophis::static_data;
using namespace sophis::value;
using namespace sophis::misc;
using namespace sophis::backoffice_kernel;

#define EDIT_ACCOUNTING_EOD_USER_RIGHT "Accounting EOD"



/*static*/ char* CSxAccountEODScenarioDlg::__CLASS__ = "CSxAccountEODScenarioDlg";
/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CSxAccountEODScenarioDlg::CSxAccountEODScenarioDlg() : CSRFitDialog()
{

	fResourceId	= IDD_CSxAccountEODScenario_DLG - ID_DIALOG_SHIFT;

	NewElementList(eNbFields);

	int nb = 0;

	if (fElementList)
	{
		fElementList[nb++]	= new CSxOKButton(this,eOK);
		fElementList[nb++]	= new CSRCancelButton(this);
		fElementList[nb++]	= new CSxAccountEntitiesEditList(this,eListOfAccountEntities);
		fElementList[nb++]	= new CSREditDate(this, eNAVDate);
	}
}

void CSxAccountEODScenarioDlg::FindClosedPositions(const CSRPortfolio * parentFolio)
{
	int positionCount = parentFolio->GetTreeViewPositionCount();
	if(!parentFolio->IsLoaded())
	{
		parentFolio->Load();
	}
	for (int i = 0; i < positionCount; ++i)
	{
		const CSRPosition * pos = parentFolio->GetNthTreeViewPosition(i);
		if (pos != NULL)
		{
				const CSRBond * bond = CSRInstrument::GetInstance<CSRBond>(pos->GetInstrumentCode());
				if(bond!=NULL)
				{
					if(fInsturmentAmounts.count(pos->GetInstrumentCode()) > 0) //key exists in map
					{
						//DPH
						//fInsturmentAmounts[pos->GetInstrumentCode()] += pos->GetInstrumentCount();
						fInsturmentAmounts[pos->GetInstrumentCode()] += (int)pos->GetInstrumentCount();
					}
					else
					{
						fInsturmentAmounts.insert(_STL::pair<int,int>(pos->GetInstrumentCode(),pos->GetInstrumentCount()));
					}
				}
		}
	}
	int childCount = parentFolio->GetChildCount();
	for(int i=0;i<childCount;++i)
	{
		const CSRPortfolio * tempChild = parentFolio->GetNthChild(i);
		if(tempChild!=NULL)
		{
			FindClosedPositions(tempChild);
		}
	}
}

/*virtual*/ void	CSxAccountEODScenarioDlg::OpenAfterInit(void)
{
	char sqlQuery[1024];
	sprintf_s(sqlQuery,1024,"select id, name from account_entity where record_type=1 and name not like 'REFERENCE%%'");
	
	struct SSxResult
	{
		long fId;
		char fAccountEntityName[100];
	};

	SSxResult *resultBuffer = NULL;
	int		 nbResults = 0;

	CSRStructureDescriptor	desc(2, sizeof(SSxResult));

	ADD(&desc, SSxResult, fId, rdfInteger);
	ADD(&desc, SSxResult, fAccountEntityName, rdfString);	

	//DPH
	//CSRSqlQuery::QueryWithNResults(sqlQuery, &desc, (void **) &resultBuffer, &nbResults);
	CSRSqlQuery::QueryWithNResultsWithoutParam(sqlQuery, &desc, (void **)&resultBuffer, &nbResults);

	_STL::map<long,long> entityLastEODDateMap;
	_STL::map<long,long> fundNameToIdMap;

	GetLastEODDate(entityLastEODDateMap);	

	CSxAccountEntitiesEditList* accountEntitiesList = (CSxAccountEntitiesEditList*)GetElementByRelativeId(eListOfAccountEntities);
	accountEntitiesList->SetLineCount(0);
	CSRElement* elem = NULL;

	for (int i = 0; i < nbResults ; i++)
	{
		accountEntitiesList->Enlarge(1);		
		int line = accountEntitiesList->GetLineCount()-1;
		accountEntitiesList->LoadLine(line);		

		elem = accountEntitiesList->GetElementByIndex(CSxAccountEntitiesEditList::eFund);
		elem->SetValue(resultBuffer[i].fAccountEntityName);		

		//Last EOD date
		_STL::map<long,long>::iterator iter1 = entityLastEODDateMap.find(resultBuffer[i].fId);
		if (iter1 != entityLastEODDateMap.end())
		{
			elem = accountEntitiesList->GetElementByIndex(CSxAccountEntitiesEditList::eLastEODDate);
			elem->SetValue(&(iter1->second));
		}

		//NAV frequency
		char navFrequencyBuffer[100];
		strcpy_s(navFrequencyBuffer,100,"");
		elem = accountEntitiesList->GetElementByIndex(CSxAccountEntitiesEditList::eNavFrequency);

		long fundId = GetFundId(resultBuffer[i].fAccountEntityName);
		const CSAMFund* fund = dynamic_cast<const CSAMFund*>(CSRInstrument::GetInstance(fundId));
		
		if (fund)
		{				
			_STL::vector<SSUserIndicator> listOfIndicators = fund->GetUserIndicators();
			if (listOfIndicators.size() == 0)
			{
				strcpy_s(navFrequencyBuffer,100,"Not handled");				
			}
			

			for (int i = 0; i < listOfIndicators.size(); i++)
			{
				_STL::string buffer(listOfIndicators[i].name);
			buffer=RemoveEscape(buffer);
				if (!_stricmp(buffer.c_str(),"NAVFrequency"))
				{
					
					strcpy_s(navFrequencyBuffer,100,listOfIndicators[i].comment);
					break;
					
					
					
				}
			}


			///////////////////////////////

			/*eNAVDatesType navDatesType = fund->GetNAVDatesType();
			switch (navDatesType)
			{
			case ndDaily:
			strcpy_s(navFrequencyBuffer,100,"Daily");
			break;
			case ndWeekly:
			{
			const char * days[] = 
			{
			"Sunday",
			"Monday",
			"Tuesday",
			"Wednesday",
			"Thursday",
			"Friday",
			"Saturday"
			};

			sprintf_s(navFrequencyBuffer,100,"Every %s",days[fund->GetWeekNAVDay()-1]);
			}

			break;
			default:
			strcpy_s(navFrequencyBuffer,100,"Not handled");
			break;
			}*/
		}
		else
		{
			strcpy_s(navFrequencyBuffer,100,"Cannot match entity name with any fund");
		}

		elem->SetValue(navFrequencyBuffer);
		accountEntitiesList->SaveLine(line);
		fAccountEntityNameMap[resultBuffer[i].fAccountEntityName] = resultBuffer[i].fId;		

	}

	accountEntitiesList->Update();

	free((char*)resultBuffer);

	//Initialise EOD date
	long sophisDate = (CSRMarketData::GetCurrentMarketData())->GetDate();
	GetElementByRelativeId(eNAVDate)->SetValue(&sophisDate);

}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/	void CSxAccountEODScenarioDlg::OnOK()
{
	BEGIN_LOG("OnOK");

	//Get list of selected lines
	_STL::map<_STL::string,long> listOfSelectedEntities;

	CSxAccountEntitiesEditList* editList = (CSxAccountEntitiesEditList*)GetElementByAbsoluteId(eListOfAccountEntities);
	_STL::vector<long> selectedLines = editList->GetSelectedLines();	

	if (selectedLines.size() > 0)
	{
		for (unsigned int i = 0 ;i < selectedLines.size(); i++)
		{		
			editList->LoadLine(selectedLines[i]);
			char entityName[100];
			CSRElement* elem = editList->GetElementByIndex(CSxAccountEntitiesEditList::eFund);
			elem->GetValue(entityName);
			long lastEODDate = 0;
			elem = editList->GetElementByIndex(CSxAccountEntitiesEditList::eLastEODDate);
			elem->GetValue(&lastEODDate);

			_STL::string entityNameStr = entityName;
			listOfSelectedEntities[entityNameStr] = lastEODDate;		
		}

		long EODDate = 0;
		GetElementByRelativeId(eNAVDate)->GetValue(&EODDate);

		bool rollBack = false;
		bool ret = CheckData(listOfSelectedEntities, EODDate, rollBack);

		if (ret)
		{
			ProcessData(listOfSelectedEntities, EODDate, rollBack);
			EndDialog();
		}
	}
	else
	{
		CSRFitDialog::Message("Select an account entity");
	}	

	END_LOG();
}

void CSxAccountEODScenarioDlg::ProcessData(_STL::map<_STL::string,long> &listOfSelectedEntities, long EODDate, bool rollBack)
{
	BEGIN_LOG("ProcessData");

	try
	{
		//Step 1				
		MESS(Log::debug,"Start step 1");

		if (rollBack)
		{			
			RollBack(EODDate, listOfSelectedEntities);		
		}
		else
		{
			MESS(Log::debug,"No roll back needed");
		}

		MESS(Log::debug,"End step 1");

		//Step 2

		MESS(Log::debug,"Start step 2");

		//Change Sophis date to the EOD date

		long currentDate = CSRMarketData::GetCurrentMarketData()->GetDate();
		CSRMarketData::SSDates currentSSDates(true);

		//if (EODDate != currentDate) // || test extern bool SOPHIS_FIT gUtiliserHistoriqueTheorique;
		{			
			MESS(Log::debug,FROM_STREAM("Set prices date to EOD date (" << NumToDateDDMMYYY(EODDate) << ")"));

			CSRMarketData::SSDates dates;

			dates.fCalculation = EODDate;
			dates.fPosition = EODDate;
			dates.fSpot = EODDate;
			dates.fForex = EODDate;
			dates.fDividend = EODDate;
			dates.fVolatility = EODDate;
			dates.fCorrelation = EODDate;
			dates.fRepoCost = EODDate;
			dates.fRate = EODDate;
			dates.fCreditRiskDate = EODDate;
			dates.fSubscriptRedemptDate = EODDate;			
			dates.fUseHistoricalFairValue = true;

			//DPH 733
			//dates.UseIt(true);
			dates.UseIt();
		}	

		//Request 1

		char sqlQuery[1024];		
		_STL::string listOfEntitiesId = GetListOfAccountEntityId(listOfSelectedEntities);		

		sprintf_s(sqlQuery,1024,"update account_posting set status = 2 where status = 1 and date_to_num(posting_date) <= %ld and account_entity_id in (%s)", 
			EODDate, listOfEntitiesId.c_str());

		//DPH
		//CSRSqlQuery::QueryWithoutResultException(sqlQuery);
		CSRSqlQuery::QueryWithoutResultAndParamException(sqlQuery);

		//Request 2

		sprintf_s(sqlQuery,1024,"update account_posting set status = 10 where status = 9 and date_to_num(posting_date) <= %ld and account_entity_id in (%s)", 
			EODDate, listOfEntitiesId.c_str());

		//DPH
		//CSRSqlQuery::QueryWithoutResultException(sqlQuery);	
		CSRSqlQuery::QueryWithoutResultAndParamException(sqlQuery);

		//Request 3	

		_STL::map<_STL::string,long>::iterator iter;
		for (iter = listOfSelectedEntities.begin(); iter != listOfSelectedEntities.end(); iter++)
		{
			sprintf_s(sqlQuery,1024,"update account_aux_ledger_rules set account_entity_id= %ld where link_id=(select id from account_aux_ledger_rules_first where record_type=1)", 
				fAccountEntityNameMap[iter->first]);

			//DPH
			//CSRSqlQuery::QueryWithoutResultException(sqlQuery);			
			CSRSqlQuery::QueryWithoutResultAndParamException(sqlQuery);

			//Launch the auxiliary ledger scenario
			_STL::auto_ptr<CSRScenario> al (CSRScenario::GetPrototype().CreateInstance("Auxiliary Ledger Scenario Batch"));
			al->SetBatchMode("Auxiliary Ledger Scenario Batch","",EODDate);

			MESS(Log::debug,"Launch the auxiliary ledger scenario");

			al->Run();			
		}

		MESS(Log::debug,"End step 2");

		//Step 3

		MESS(Log::debug,"Start step 3");

		for (iter = listOfSelectedEntities.begin(); iter != listOfSelectedEntities.end(); iter++)
		{
			//Request 1		
			sprintf_s(sqlQuery,1024,"begin cfg_treat_repo_expiry_posting (23,24,num_to_date(%ld),%ld); end;", 
				EODDate, fAccountEntityNameMap[iter->first]);

			//DPH
			//CSRSqlQuery::QueryWithoutResultException(sqlQuery);			
			CSRSqlQuery::QueryWithoutResultAndParamException(sqlQuery);

			//Request 2
			sprintf_s(sqlQuery,1024,"begin cfg_treat_repo_init_posting (21,22,num_to_date(%ld),%ld); end;", 
				EODDate, fAccountEntityNameMap[iter->first]);

			//DPH
			//CSRSqlQuery::QueryWithoutResultException(sqlQuery);			
			CSRSqlQuery::QueryWithoutResultAndParamException(sqlQuery);

			//Request 3
			sprintf_s(sqlQuery,1024,"begin cfg_treat_repo_canc_posting (21,22,23,24,num_to_date(%ld),%ld); end;", 
				EODDate, fAccountEntityNameMap[iter->first]);

			//DPH
			//CSRSqlQuery::QueryWithoutResultException(sqlQuery);			
			CSRSqlQuery::QueryWithoutResultAndParamException(sqlQuery);

		}
		MESS(Log::debug,"End step 3");

		//Step 4

		MESS(Log::debug,"Start step 4");

		//Insert entity ids in ACCOUNT_ENTITY_EOD
		sprintf_s(sqlQuery,1024,"delete ACCOUNT_ENTITY_EOD");
		//DPH
		//CSRSqlQuery::QueryWithoutResultException(sqlQuery);			
		CSRSqlQuery::QueryWithoutResultAndParamException(sqlQuery);

		//Load and recompute the root portfolio
		const CSRPortfolio* rootFolio = CSRPortfolio::GetRootPortfolio();

		if (!rootFolio->IsLoaded())
			rootFolio->Load();

		rootFolio->Compute();
		
		//Clearing fInstrumentIds list and fInsturmentAmounts map
		fInstrumentIds.clear();
		fInsturmentAmounts.clear();

		//Build a list containing instrument ids (sicovam) for closed positions
		for(_STL::list<long>::iterator it=fFundIds.begin();it!=fFundIds.end();++it)
		{
			const CSAMFund* fund = dynamic_cast<const CSAMFund*>(CSRInstrument::GetInstance(*it));
			if(fund==NULL)
			{
				continue;
			}
			long folioCode=fund->GetTradingPortfolio();
			const CSRPortfolio * tempFolio = CSRPortfolio::GetCSRPortfolio(folioCode);
			FindClosedPositions(tempFolio);
			_STL::list<int>::iterator instrumentIdsIterator = fInstrumentIds.begin();
			for(_STL::map<int,int>::iterator mapIterator = fInsturmentAmounts.begin() ; mapIterator != fInsturmentAmounts.end() ; ++mapIterator)
			{
				if(mapIterator->second==0)
				{
					fInstrumentIds.insert(instrumentIdsIterator,mapIterator->first);
				}
			}
		}
		
		//Launch PnL Engine scenario
		_STL::auto_ptr<CSRScenario> PnLEngineScenario (CSRScenario::GetPrototype().CreateInstance("Acc-02 PnL Engine"));

		char buffer[1024];
		sprintf_s(buffer, 1024, "%s", listOfEntitiesId.c_str());
		PnLEngineScenario->SetBatchMode("Acc-02 PnL Engine",buffer,EODDate);

		MESS(Log::debug,"Launch PnL Engine scenario");
		PnLEngineScenario->Run();

		//Launch Balance Engine after PnL		
		_STL::auto_ptr<CSRScenario> balanceEngineScenario (CSRScenario::GetPrototype().CreateInstance("Acc-03 Balance Engine after PnL"));

		sprintf_s(buffer, 1024, "%s", listOfEntitiesId.c_str());

		balanceEngineScenario->SetBatchMode("Acc-02 PnL Engine",buffer,EODDate);

		MESS(Log::debug,"Launch Balance Engine after PnL");
		balanceEngineScenario->Run();			

		MESS(Log::debug,"End step 4");

		//Step 5
		MESS(Log::debug,"Start step 5");

		long BOKernelEventId = 0;
		ConfigurationFileWrapper::getEntryValue("CFG_AccountEOD", "SREODProcessEventID", BOKernelEventId, 0);
		_STL::string BOStatusGroup = "";	
		ConfigurationFileWrapper::getEntryValue("CFG_AccountEOD", "SREODProcessStatusGroup", BOStatusGroup, "");

		MESS(Log::debug,FROM_STREAM("BOKernelEventId = " << BOKernelEventId << "; BOStatusGroup = " << BOStatusGroup));

		UpdateSR(listOfSelectedEntities, EODDate, BOKernelEventId, BOStatusGroup);

		//Request 1
		sprintf_s(sqlQuery,1024,"update account_posting set status = 4 where status = 2 and date_to_num(posting_date) <= %ld and account_entity_id in (%s) and account_number not like 'XXX%%' and posting_type not in (1,3,21,22,23,24)", 
			EODDate, listOfEntitiesId.c_str());

		//DPH
		//CSRSqlQuery::QueryWithoutResultException(sqlQuery);			
		CSRSqlQuery::QueryWithoutResultAndParamException(sqlQuery);

		//Request 2
		sprintf_s(sqlQuery,1024,"update account_posting set status = 6 where status = 5 and date_to_num(posting_date) <= %ld and account_entity_id in (%s) and account_number not like 'XXX%%' and posting_type not in (1,3,21,22,23,24)", 
			EODDate, listOfEntitiesId.c_str());

		//DPH
		//CSRSqlQuery::QueryWithoutResultException(sqlQuery);			
		CSRSqlQuery::QueryWithoutResultAndParamException(sqlQuery);

		//Request 3
		sprintf_s(sqlQuery,1024,"update account_posting set status = 15 where status = 7 and date_to_num(posting_date) <= %ld and account_entity_id in (%s) and account_number not like 'XXX%%' and posting_type not in (1,3,21,22,23,24)", 
			EODDate, listOfEntitiesId.c_str());

		//DPH
		//CSRSqlQuery::QueryWithoutResultException(sqlQuery);			
		CSRSqlQuery::QueryWithoutResultAndParamException(sqlQuery);

		//Request 4
		sprintf_s(sqlQuery,1024,"update account_posting set status = 12 where status = 10 and date_to_num(posting_date) <= %ld and account_entity_id in (%s) and account_number not like 'XXX%%' and posting_type not in (1,3,21,22,23,24)", 
			EODDate, listOfEntitiesId.c_str());

		//DPH
		//CSRSqlQuery::QueryWithoutResultException(sqlQuery);			
		CSRSqlQuery::QueryWithoutResultAndParamException(sqlQuery);

		CSRSqlQuery::Commit();

		MESS(Log::debug,"End step 5");

		//Restore Sophis date		

		if (EODDate != currentDate)
		{			
			MESS(Log::debug,FROM_STREAM("Restore Sophis date to " << NumToDateDDMMYYY(currentDate)));						
			//DPH 733
			//currentSSDates.UseIt(true);
			currentSSDates.UseIt();
		}	

		//Display results for each accounting entity
		for (iter = listOfSelectedEntities.begin(); iter != listOfSelectedEntities.end(); iter++)
		{
			if (!DisplayEODResults(iter->first,EODDate))
			{
				_STL::map<_STL::string,long> entityElemMap;
				entityElemMap[iter->first] = iter->second;
				RollBack(EODDate, entityElemMap);
			}
		}									
	}
	catch(sophisTools::base::ExceptionBase &ex)
	{
		CSRFitDialog::Message(FROM_STREAM("ERROR ("<<ex<<") while running the account EOD scenario"));
		MESS(Log::error,FROM_STREAM("ERROR ("<<ex<<") while running the account EOD scenario"));
		CSRSqlQuery::RollBack();		
	}
	catch(...)
	{		
		CSRFitDialog::Message(FROM_STREAM("Unhandled exception occured while running the account EOD scenario"));
		MESS(Log::error,"Unhandled exception occured while running the account EOD scenario");
		CSRSqlQuery::RollBack();		
	}			

	END_LOG();
}

void CSxAccountEODScenarioDlg::UpdateSR(_STL::map<_STL::string,long> &listOfSelectedEntities, long EODDate, long BOKernelEventId,_STL::string BOStatusGroup)
{
	//Apply a BO kernel Event to the S/R that are in a specific BO status group	

	_STL::map<_STL::string,long>::iterator iter;

	for (iter = listOfSelectedEntities.begin(); iter != listOfSelectedEntities.end(); iter++)
	{
		long fundId = GetFundId((iter->first).c_str());
		ApplyBOEvent(fundId, EODDate, BOKernelEventId, BOStatusGroup);
	}

	//Request 1

	char sqlQuery[1024];
	_STL::string listOfEntitiesId = GetListOfAccountEntityId(listOfSelectedEntities);

	sprintf_s(sqlQuery,1024,"update account_posting set status = 2 where status = 1 and date_to_num(posting_date) <= %ld and account_entity_id in (%s)", 
		EODDate, listOfEntitiesId.c_str());

	//DPH
	//CSRSqlQuery::QueryWithoutResultException(sqlQuery);			
	CSRSqlQuery::QueryWithoutResultAndParamException(sqlQuery);

	//Request 2

	sprintf_s(sqlQuery,1024,"update account_posting set status = 10 where status = 9 and date_to_num(posting_date) <= %ld and account_entity_id in (%s)", 
		EODDate, listOfEntitiesId.c_str());

	//DPH
	//CSRSqlQuery::QueryWithoutResultException(sqlQuery);			
	CSRSqlQuery::QueryWithoutResultAndParamException(sqlQuery);
}

_STL::string CSxAccountEODScenarioDlg::GetBOStatusCondition(const _STL::string& statusGroup)
{
	_STL::string boGroupFilter = "";
	if (_stricmp(statusGroup.c_str(), "ALL"))
	{
		boGroupFilter = " and BACKOFFICE in (";
		_STL::vector<long> groupList; 
		CSRKernelGroupOfStatuses::GetList(groupList);
		_STL::vector<long>::const_iterator iterGrp = groupList.begin();
		for(; iterGrp != groupList.end(); iterGrp++)
		{
			CSRKernelGroupOfStatuses group(*iterGrp);
			// Find the status group with the input name
			if (!_stricmp(group.GetName(), statusGroup.c_str()))
			{
				_STL::vector<long> listStatus;
				group.GetStatuses(listStatus);
				_STL::vector<long>::const_iterator iterSt = listStatus.begin();
				size_t si = 1;
				// Add BO status clause
				for(; iterSt != listStatus.end(); si++, iterSt++)
				{		
					boGroupFilter += FROM_STREAM(*iterSt);
					if (si < listStatus.size())
						boGroupFilter += ",";
				}
				break;
			}
		}
		if (iterGrp == groupList.end())
			throw GeneralException("Status Group not found.");
		boGroupFilter += ") ";
	}
	else
	{
		// No need as already in the main request
		//boGroupFilter += CSRBusinessEvent::GetBOFilter() ;
	}

	return boGroupFilter;
}

void CSxAccountEODScenarioDlg::ApplyBOEvent(long fundId, long EODDate, long BOKernelEventId, _STL::string BOStatusGroup)
{
	const CSAMFund* fund = dynamic_cast<const CSAMFund*>(CSRInstrument::GetInstance(fundId));

	if (fund)
	{
		_STL::string BOStatusCondition = GetBOStatusCondition(BOStatusGroup);

		char sqlQuery[1024];
		sprintf_s(sqlQuery,1024,"select code from fund_purchase where fund = %ld and date_to_num(NAV_DATE) = %ld %s order by code", fundId, EODDate, BOStatusCondition.c_str());

		struct SSxResult
		{
			long fCode;			
		};

		SSxResult *resultBuffer = NULL;
		int		 nbResults = 0;

		CSRStructureDescriptor	desc(1, sizeof(SSxResult));

		ADD(&desc, SSxResult, fCode, rdfInteger);		

		//DPH
		//CSRSqlQuery::QueryWithNResults(sqlQuery, &desc, (void **) &resultBuffer, &nbResults);
		CSRSqlQuery::QueryWithNResultsWithoutParam(sqlQuery, &desc, (void **)&resultBuffer, &nbResults);

		for (int i = 0; i < nbResults; i++)
		{
			CSAMFundSR fundSR(resultBuffer[i].fCode);
			sophis::tools::CSREventVector messages;
			fundSR.Save(BOKernelEventId, messages);
		}		
	}
}

bool CSxAccountEODScenarioDlg::DisplayEODResults(_STL::string accountEntityName, long EODDate)
{
	bool ret = true;

	CSxAccountEODResultDlg::SSxdata resultData;

	fInstrumentIds.sort();
	fInstrumentIds.unique();
	_STL::stringstream ss;
	bool firstValue = true;

	for(_STL::list<int>::iterator instrumentIdsIterator=fInstrumentIds.begin();instrumentIdsIterator!=fInstrumentIds.end();++instrumentIdsIterator)
	{
		if(firstValue)
		{
			ss << *instrumentIdsIterator;
			firstValue = false;
		}
		else
		{
			ss << "," << *instrumentIdsIterator;
		}
	}

	//Get net asset	
	char sqlQuery[4096];
	const _STL::string &tmp = ss.str();   
	const char* cstr = tmp.c_str();

	if(strlen(cstr) > 0)
	{
		sprintf_s(sqlQuery,4096,"select sum(decode(credit_debit,'D',amount,'C',-1*amount)) from account_posting where status in (4,6,12,15) "
			"and account_entity_id = %ld "
			"and account_number in (select account_number from account_map where account_name_id in (select id from account_name where category in ('Classe 3','Classe 4','Classe 5'))) "
			"and date_to_num(posting_date) <= %ld and not (posting_rule_id in (SELECT DISTINCT ID FROM ACCOUNT_BAL_POSTING_RULES) "
			"and status=4 and CREDIT_DEBIT='C' AND INSTRUMENT_ID IS NOT NULL AND INSTRUMENT_ID IN(%s))", fAccountEntityNameMap[accountEntityName], EODDate,cstr);
	}
	else
	{
		sprintf_s(sqlQuery,4096,"select sum(decode(credit_debit,'D',amount,'C',-1*amount)) from account_posting where status in (4,6,12,15) "
			"and account_entity_id = %ld "
			"and account_number in (select account_number from account_map where account_name_id in (select id from account_name where category in ('Classe 3','Classe 4','Classe 5'))) "
			"and date_to_num(posting_date) <= %ld", fAccountEntityNameMap[accountEntityName], EODDate);
	}

	CSRSqlQuery::QueryReturning1Double(sqlQuery,&resultData.fNetAsset);

	//Get fees	
	sprintf_s(sqlQuery,4096,"select sum(decode(credit_debit,'D',amount,'C',-1*amount)) from account_posting where status in (4,6,12,15) " 
		"and account_entity_id = %ld and account_number in ('44921') and date_to_num(posting_date) = %ld", fAccountEntityNameMap[accountEntityName], EODDate);

	CSRSqlQuery::QueryReturning1Double(sqlQuery,&resultData.fFees);

	//Agios	
	sprintf_s(sqlQuery,4096,"select sum(decode(credit_debit,'D',amount,'C',-1*amount)) from account_posting where status in (4,6,12,15) "
		"and account_entity_id = %ld and account_number in ('44931') and date_to_num(posting_date) = %ld",fAccountEntityNameMap[accountEntityName], EODDate);

	CSRSqlQuery::QueryReturning1Double(sqlQuery,&resultData.fAgios);

	//Number of shares

	sprintf_s(sqlQuery,4096,"select sum(decode(SIGN,'+',quantity,'-',-1*quantity)) from account_posting where account_number in ('1123','1124') "
		"and account_entity_id = %ld and date_to_num(posting_date) <= %ld", fAccountEntityNameMap[accountEntityName], EODDate);

	CSRSqlQuery::QueryReturning1Double(sqlQuery,&resultData.fNumberOfShares);

	//NAV/Share	

	if (resultData.fNumberOfShares)
		resultData.fNavPerShare = resultData.fNetAsset/resultData.fNumberOfShares;			

	CSxAccountEODResultDlg *resultDialog = new CSxAccountEODResultDlg(resultData);
	
	fFundIds.clear();
	fInstrumentIds.clear();
	fInsturmentAmounts.clear();
	// Display a modal dialog
	char title[256];
	_STL::string EODDateStr = NumToDateDDMMYYY(EODDate);
	sprintf_s(title,256,"Accounting End of day - %s (%s)",accountEntityName.c_str(),EODDateStr.c_str());

	if (resultDialog->DoDialog(true,title) == drCancel)
		ret = false;

	delete resultDialog;

	return ret;
}

void CSxAccountEODScenarioDlg::RollBack(long EODDate, _STL::map<_STL::string,long> &listOfSelectedEntities)
{
	BEGIN_LOG("RollBack");

	try
	{

		MESS(Log::debug, FROM_STREAM("Start rollback process. EOD date (" << NumToDateDDMMYYY(EODDate) << ")"));

		_STL::string listOfEntitiesId = GetListOfAccountEntityId(listOfSelectedEntities);

		//Get rollback date	
		long rollbackDate = 0;

		char sqlQuery[1024];
		sprintf_s(sqlQuery,1024,"Select max(date_to_num(posting_date)) from account_posting where rule_type in (2,3) and date_to_num(posting_date) < %ld and account_entity_id in (%s)", EODDate, listOfEntitiesId.c_str());
		CSRSqlQuery::QueryReturning1LongException(sqlQuery,&rollbackDate);

		MESS(Log::debug, FROM_STREAM("Rollback date is " << NumToDateDDMMYYY(rollbackDate)));	

		//Update S/R
		long BOKernelEventId = 0;
		ConfigurationFileWrapper::getEntryValue("CFG_AccountEOD", "SREODCancelEventID", BOKernelEventId, 0);
		_STL::string BOStatusGroup = "";	
		ConfigurationFileWrapper::getEntryValue("CFG_AccountEOD", "SREODCancelStatusGroup", BOStatusGroup, "");

		MESS(Log::debug,FROM_STREAM("BOKernelEventId = " << BOKernelEventId << "; BOStatusGroup = " << BOStatusGroup));
		UpdateSR(listOfSelectedEntities, EODDate, BOKernelEventId,BOStatusGroup);

		//Request 1

		sprintf_s(sqlQuery,1024,"delete from account_posting where account_entity_id in (%s) and rule_type = 2 and date_to_num(posting_date) > %ld", 
			listOfEntitiesId.c_str(), rollbackDate);

		//DPH
		//CSRSqlQuery::QueryWithoutResultException(sqlQuery);
		CSRSqlQuery::QueryWithoutResultAndParamException(sqlQuery);

		//Request 2

		sprintf_s(sqlQuery,1024,"update account_posting set status=4, link_id=null where account_entity_id in (%s) and status=6 and rule_type=2 and date_to_num(posting_date) = %ld", 
			listOfEntitiesId.c_str(), rollbackDate);

		//DPH
		//CSRSqlQuery::QueryWithoutResultException(sqlQuery);
		CSRSqlQuery::QueryWithoutResultAndParamException(sqlQuery);

		//request 3

		sprintf_s(sqlQuery,1024,"update account_posting set status=2, link_id=null where account_entity_id in (%s) and status=5 and rule_type=2 and date_to_num(posting_date) = %ld", 
			listOfEntitiesId.c_str(), rollbackDate);

		//DPH
		//CSRSqlQuery::QueryWithoutResultException(sqlQuery);
		CSRSqlQuery::QueryWithoutResultAndParamException(sqlQuery);

		//Request 4

		sprintf_s(sqlQuery,1024,"delete from account_posting where account_entity_id in (%s) and rule_type = 3 and date_to_num(posting_date) > %ld", 
			listOfEntitiesId.c_str(), rollbackDate);

		//DPH
		//CSRSqlQuery::QueryWithoutResultException(sqlQuery);
		CSRSqlQuery::QueryWithoutResultAndParamException(sqlQuery);

		//Request 5

		sprintf_s(sqlQuery,1024,"update account_posting set status=4, link_id=null where account_entity_id in (%s) and status=6 and rule_type=3 and date_to_num(posting_date) = %ld", 
			listOfEntitiesId.c_str(), rollbackDate);

		//DPH
		//CSRSqlQuery::QueryWithoutResultException(sqlQuery);
		CSRSqlQuery::QueryWithoutResultAndParamException(sqlQuery);

		//request 6

		sprintf_s(sqlQuery,1024,"update account_posting set status=2, link_id=null where account_entity_id in (%s) and status=5 and rule_type=3 and date_to_num(posting_date) = %ld", 
			listOfEntitiesId.c_str(), rollbackDate);

		//DPH
		//CSRSqlQuery::QueryWithoutResultException(sqlQuery);
		CSRSqlQuery::QueryWithoutResultAndParamException(sqlQuery);

		//Request 7

		sprintf_s(sqlQuery,1024,"delete from account_posting where account_entity_id in (%s) and rule_type = 4 and date_to_num(posting_date) >= %ld", 
			listOfEntitiesId.c_str(), rollbackDate);

		//DPH
		//CSRSqlQuery::QueryWithoutResultException(sqlQuery);
		CSRSqlQuery::QueryWithoutResultAndParamException(sqlQuery);

		//request 8

		sprintf_s(sqlQuery,1024,"update account_posting set status=2, link_id=null where account_entity_id in (%s) and status=8 and rule_type=1 and date_to_num(posting_date) >= %ld", 
			listOfEntitiesId.c_str(), rollbackDate);

		//DPH
		//CSRSqlQuery::QueryWithoutResultException(sqlQuery);
		CSRSqlQuery::QueryWithoutResultAndParamException(sqlQuery);

		//Request 9 (bug case 00706393)

		sprintf_s(sqlQuery,1024,"update account_posting set status = 12 where status = 10 and date_to_num(posting_date) >= %ld and account_entity_id in (%s)"
									" and account_number not like 'XXX%%' and posting_type in (21,22,23,24)", rollbackDate, listOfEntitiesId.c_str());

		//DPH
		//CSRSqlQuery::QueryWithoutResultException(sqlQuery);
		CSRSqlQuery::QueryWithoutResultAndParamException(sqlQuery);

		//Commit

		CSRSqlQuery::Commit();
	}
	catch(sophisTools::base::ExceptionBase &ex)
	{
		CSRFitDialog::Message(FROM_STREAM("ERROR ("<<ex<<") while running the rollback process"));
		MESS(Log::error, FROM_STREAM("ERROR ("<<ex<<") while running the rollback process"));
		END_LOG();
		throw ex;
	}
	catch(...)
	{		
		CSRFitDialog::Message(FROM_STREAM("Unhandled exception occured while running the rollback process"));
		MESS(Log::error, FROM_STREAM("Unhandled exception occured while running the rollback process"));
		END_LOG();
		throw;
	}	

	MESS(Log::debug, "End of rollback process");

	END_LOG();
}

_STL::string CSxAccountEODScenarioDlg::GetListOfAccountEntityId(_STL::map<_STL::string,long> &listOfSelectedEntities)
{
	_STL::string listOfSelectedEntitiesStr = "";

	_STL::map<_STL::string,long>::iterator iter = listOfSelectedEntities.begin();
	listOfSelectedEntitiesStr = FROM_STREAM(fAccountEntityNameMap[iter->first]);
	iter++;

	while (iter != listOfSelectedEntities.end())
	{
		listOfSelectedEntitiesStr += FROM_STREAM("," << fAccountEntityNameMap[iter->first]);
		iter++;
	}

	return listOfSelectedEntitiesStr;
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ CSxAccountEODScenarioDlg::~CSxAccountEODScenarioDlg()
{
}

void CSxAccountEODScenarioDlg::GetLastEODDate(_STL::map<long,long> &entityLastEODDateMap)
{
	char sqlQuery[1024];
	sprintf_s(sqlQuery,1024,"select E.id, max(date_to_num(P.posting_date)) from account_entity E, account_posting P where  E.ID = P.account_entity_id and E.record_type=1 and E.name not like 'REFERENCE%%' "
		"and P.rule_type in (2,3) and P.status in (2,4) group by E.id");

	struct SSxResult
	{
		long fId;
		long fLastEODDate;
	};

	SSxResult *resultBuffer = NULL;
	int		 nbResults = 0;

	CSRStructureDescriptor	desc(2, sizeof(SSxResult));

	ADD(&desc, SSxResult, fId, rdfInteger);
	ADD(&desc, SSxResult, fLastEODDate, rdfInteger);	

	//DPH
	//CSRSqlQuery::QueryWithNResults(sqlQuery, &desc, (void **) &resultBuffer, &nbResults);	
	CSRSqlQuery::QueryWithNResultsWithoutParam(sqlQuery, &desc, (void **)&resultBuffer, &nbResults);

	for (int i = 0; i < nbResults ; i++)
	{
		entityLastEODDateMap[resultBuffer[i].fId] = resultBuffer[i].fLastEODDate;
	}	

	free((char*)resultBuffer);
}

long CSxAccountEODScenarioDlg::GetFundId(const char* fundName)
{
	long res = 0;

	if (fFundNameToIdMap.empty())
	{
		char sqlQuery[1024];
		sprintf_s(sqlQuery,1024,"select sicovam, libelle from titres where type = 'Z' and typepro = 1");

		struct SSxResult
		{
			long fFundId;
			char fFundName[100];
		};

		SSxResult *resultBuffer = NULL;
		int		 nbResults = 0;

		CSRStructureDescriptor	desc(2, sizeof(SSxResult));

		ADD(&desc, SSxResult, fFundId, rdfInteger);
		ADD(&desc, SSxResult, fFundName, rdfString);	

		//DPH
		//CSRSqlQuery::QueryWithNResults(sqlQuery, &desc, (void **) &resultBuffer, &nbResults);	
		CSRSqlQuery::QueryWithNResultsWithoutParam(sqlQuery, &desc, (void **)&resultBuffer, &nbResults);

		for (int i = 0; i < nbResults ; i++)
		{
			fFundNameToIdMap[resultBuffer[i].fFundName] = resultBuffer[i].fFundId;
		}	

		free((char*)resultBuffer);
	}

	_STL::map<_STL::string,long>::iterator iter = fFundNameToIdMap.find(fundName);
	if (iter != fFundNameToIdMap.end())
		res = iter->second;

	return res;
}
_STL::string CSxAccountEODScenarioDlg::RemoveEscape(_STL::string initialName)
{
	_STL::string::iterator it;	
	_STL::string str="";
    _STL::string strFinal="";
     for ( it=initialName.begin() ; it < initialName.end(); it++ )
     {
		 str=*it;
      if(str!=" ")
        {
        strFinal.append(str);
        }
     }
	 return strFinal;
}

bool CSxAccountEODScenarioDlg::CheckData(_STL::map<_STL::string,long> &listOfSelectedEntities, long EODDate, bool &rollBack)
{
	BEGIN_LOG("CheckData");

	bool ret = true;

	try
	{
		// Check if the EOD date is an open day
		/*long ccyCode = CSRCurrency::StringToCurrency("MAD");
		const CSRCurrency* ccy = CSRCurrency::GetCSRCurrency(ccyCode);

		if (ccy)
		{
			if (ccy->IsABankHolidayDay(EODDate))
			{
				char mess[250];
				sprintf_s(mess, 250, "The entered EOD date (%s) is a bank holiday. Please enter an open day", NumToDateDDMMYYY(EODDate).c_str());
				CSRFitDialog::Message(mess);
				return false;
			}
		}*/

	/*	eNAVDatesType navDatesTypeOfFirstElement = ndCustom;*/
		char navDatesTypeOfFirstElement[81] = "";
		long lastEODDateOfFirstElement = 0;
		_STL::map<_STL::string,long>::iterator iter;				
		iter = listOfSelectedEntities.begin();

		if (iter != listOfSelectedEntities.end())
		{
			lastEODDateOfFirstElement = iter->second;
			long fundId = GetFundId((iter->first).c_str());
			const CSAMFund* fund = dynamic_cast<const CSAMFund*>(CSRInstrument::GetInstance(fundId));
			if (fund)
			{
				/*navDatesTypeOfFirstElement = fund->GetNAVDatesType();*/

				const _STL::vector<SSUserIndicator> listOfIndicators= fund->GetUserIndicators();
				for (int i = 0; i < listOfIndicators.size(); i++)
			     {
					 _STL::string buffer(listOfIndicators[i].name);
			         buffer=RemoveEscape(buffer);
				if (!_stricmp(buffer.c_str(),"NAVFrequency"))
				{
					
						strcpy_s(navDatesTypeOfFirstElement,81,listOfIndicators[i].comment);						
						break;
					
				}
			}
		}

		fFundIds.clear();
		_STL::list<long>::iterator fundIdsIterator = fFundIds.begin();

		for (iter = listOfSelectedEntities.begin(); iter != listOfSelectedEntities.end(); iter++)
		{						
			long fundId = GetFundId((iter->first).c_str());
			const CSAMFund* fund = dynamic_cast<const CSAMFund*>(CSRInstrument::GetInstance(fundId));

			if (fund)
			{
				fFundIds.insert(fundIdsIterator,fund->GetCode());
				if (EODDate <= iter->second)
					rollBack = true;

				if (EODDate < CSRDay::GetSystemDate())
				{
					//Check user right				
					eRightStatusType rightStatusType = GetEditAccountingEODUserRight();

					if (rightStatusType == rsEnable)
					{
						char mess[250];
						sprintf_s(mess,250, "The entered EOD date is less than today.\n Do you want to proceed anyway ?");
						int res = CSRFitDialog::ConfirmDialog(mess);
						if (res == CONFIRM_DIALOG_CANCEL || res == CONFIRM_DIALOG_NO)
						{
							ret = false;
							break;
						}						
					}
					else
					{
						char mess[250];
						sprintf_s(mess,250, "This user is not allowed to launch an accounting EOD at a date that is less than today");
						CSRFitDialog::Message(mess);

						ret = false;
						break;
					}						
				}


				//Check 4 descativated

				/*if (EODDate > iter->second && fund->GetNAVDatesType()== ndDaily && EODDate != ccy->MatchingBusinessDay(iter->second+1, haFollowing))
				{
					char mess[250];
					sprintf_s(mess,250, "The entered EOD date is not equal to the \"last EOD date + 1 open  day\" of fund : %s. Some EOD have not been launched."
						"\n Do you want to proceed anyway?", (iter->first).c_str());
					int res = CSRFitDialog::ConfirmDialog(mess);
					if (res == CONFIRM_DIALOG_CANCEL || res == CONFIRM_DIALOG_NO)
					{
						ret = false;
						break;
					}
				}*/
				//Check 10 desactivated

				/*if (EODDate > iter->second && fund->GetNAVDatesType()== ndWeekly && EODDate != ccy->MatchingBusinessDay(iter->second+7, haFollowing))
				{
					char mess[250];
					sprintf_s(mess,250, "The entered EOD date is not equal to the \"last EOD date + 7 open days\" of weekly fund : %s. Some EOD have not been launched."
						"\n Do you want to proceed anyway?", (iter->first).c_str());
					int res = CSRFitDialog::ConfirmDialog(mess);
					if (res == CONFIRM_DIALOG_CANCEL || res == CONFIRM_DIALOG_NO)
					{
						ret = false;
						break;
					}
				}*/

				//Check deal status
				if(CheckDealBOStatus(iter->first,EODDate) == false)
				{
					char mess[250];
					sprintf_s(mess,250, "There are some trades that have not been validated BO."
						"\n Do you want to proceed anyway?", (iter->first).c_str());
					int res = CSRFitDialog::ConfirmDialog(mess);
					if (res == CONFIRM_DIALOG_CANCEL || res == CONFIRM_DIALOG_NO)
					{
						ret = false;
						break;
					}
				}

				//Check SR BO status
				if(CheckSRBOStatus(iter->first,EODDate) == false)
				{
					char mess[250];
					sprintf_s(mess,250, "There are some S/R that have not been validated BO."
						"\n Do you want to proceed anyway?", (iter->first).c_str());
					int res = CSRFitDialog::ConfirmDialog(mess);
					if (res == CONFIRM_DIALOG_CANCEL || res == CONFIRM_DIALOG_NO)
					{
						ret = false;
						break;
					}
				}
				//Check 8 desactived

				//Check if funds have the same NAV frequency
				
				/*if (fund->GetNAVDatesType() != navDatesTypeOfFirstElement)
				{
					char mess[250];
					sprintf_s(mess,250, "You cannot launch both daily and weekly funds at the same time."
						"\n Select funds of the same frequency type and try again");
					CSRFitDialog::Message(mess);
					ret = false;
					break;
				}*/

				//Check 8 modified
				char dateType[81]="";
				const _STL::vector<SSUserIndicator> listOfIndicators= fund->GetUserIndicators();
				for (int i = 0; i < listOfIndicators.size(); i++)
			    {
					_STL::string buffer(listOfIndicators[i].name);
			        buffer=RemoveEscape(buffer);
				if (!_stricmp(buffer.c_str(),"NAVFrequency"))
				    {
					strcpy_s(dateType,81,listOfIndicators[i].comment);
					break;				
				    }
				}

				if (_stricmp(dateType, navDatesTypeOfFirstElement))
				{
					char mess[250];
					sprintf_s(mess,250, "You cannot launch funds with different 'NAV Frequency' at the same time."
						"\n Select funds of the same 'NAV Frequency' reference and try again");
					CSRFitDialog::Message(mess);
					ret = false;
					break;
				}

				//Check if funds have the same last EOD date
				if (iter->second != lastEODDateOfFirstElement)
				{
					char mess[250];
					sprintf_s(mess,250, "You cannot launch funds with different last EOD date."
						"\n Select funds with the same last EOD date and try again");
					CSRFitDialog::Message(mess);
					ret = false;
					break;
				}
				}
			}			
		}
	}
	catch(sophisTools::base::ExceptionBase &ex)
	{
		CSRFitDialog::Message(FROM_STREAM("ERROR ("<<ex<<") while running the check process"));	
		MESS(Log::error, FROM_STREAM("ERROR ("<<ex<<") while running the check process"));
		ret = false; 
	}
	catch(...)
	{		
		CSRFitDialog::Message(FROM_STREAM("Unhandled exception occured while running the check process"));	
		MESS(Log::error, FROM_STREAM("Unhandled exception occured while running the check process"));
		ret = false;
	}

	END_LOG();

	return ret;
}

eRightStatusType CSxAccountEODScenarioDlg::GetEditAccountingEODUserRight()
{
	long userID		= 0;
	long groupID	= 0;

	CSRPreference::GetUserID(&userID,&groupID);
	if(userID==1)
	{
		return rsEnable;
	}
	else
	{
		CSRUserRights userRights(userID);
		userRights.LoadDetails();
		eRightStatusType	userRightStatus = userRights.GetUserDefRight(EDIT_ACCOUNTING_EOD_USER_RIGHT);
		if(userRightStatus==rsSameAsParent)
		{
			CSRUserRights groupRights(groupID);
			groupRights.LoadDetails();
			return groupRights.GetUserDefRight(EDIT_ACCOUNTING_EOD_USER_RIGHT);
		}
		else
			return userRightStatus;
	}
}

bool CSxAccountEODScenarioDlg::CheckDealBOStatus(_STL::string accountEntityName, long eodDate)
{
	bool ret = false;

	char sqlQuery[1024];
	sprintf_s(sqlQuery,1024, "select count(*) from histomvts "
		"where backoffice not in "
		"(select kernel_status_id from bo_kernel_status_component "
		"where kernel_status_group_id in "
		"(select id from bo_kernel_status_group where record_type=1 and upper(name) = upper('Accounting Scope'))) "
		"and date_to_num(dateneg) <= %ld "
		"and entite = (select ident from tiers where name = '%s')",eodDate,accountEntityName.c_str());

	long res = 0;
	CSRSqlQuery::QueryReturning1LongException(sqlQuery,&res);
	if (!res)
		ret = true;
	else
		ret = false;

	return ret;
}

bool CSxAccountEODScenarioDlg::CheckSRBOStatus(_STL::string accountEntityName, long eodDate)
{
	bool ret = false;

	char sqlQuery[1024];
	sprintf_s(sqlQuery,1024, "select count(*) from fund_purchase " 
		"where backoffice not in "
		"(select kernel_status_id from bo_kernel_status_component "
		"where kernel_status_group_id in (select id from bo_kernel_status_group where record_type=1 and upper(name) = upper('Accounting Scope'))) "
		"and date_to_num(nav_date) <= %ld "
		"and fund = (select sicovam from titres where libelle = '%s')",eodDate,accountEntityName.c_str());

	long res = 0;
	CSRSqlQuery::QueryReturning1LongException(sqlQuery,&res);
	if (!res)
		ret = true;
	else
		ret = false;

	return ret;
}

_STL::string CSxAccountEODScenarioDlg::NumToDateDDMMYYY(long sphDate)
{
	CSRDay Date(sphDate);	

	char year[5]; 
	sprintf_s(year, 5, "%04d", long(Date.fYear));

	char month[3];  
	sprintf_s(month, 3,  "%02d", long(Date.fMonth));

	char day[3];  
	sprintf_s(day, 3, "%02d", long(Date.fDay));

	return FROM_STREAM(day << "/" << month << "/" << year);

}

///////////////////////////////////////////////////////////////////////////
/*-------------------------- CSxAccountEntitiesEditList -------------------------*/

CSxAccountEntitiesEditList::CSxAccountEntitiesEditList(CSRFitDialog* dialog, int nre) :
CSREditList(dialog, nre)
{	
	fListeValeur = NULL;
	SetMaxSelection (-1);

	fColumnCount = eColumnCount;
	fColumns = new SSColumn[fColumnCount];
	int count = 0;

	fColumns[count].fColumnName		= "Fund";
	fColumns[count].fColumnWidth	= 140;
	fColumns[count].fAlignmentType = aLeft;
	fColumns[count].fElement = new CSRStaticText(this,eFund,100);

	count++;

	fColumns[count].fColumnName		= "Last EOD Date";
	fColumns[count].fColumnWidth	= 100;
	fColumns[count].fAlignmentType = aLeft;
	fColumns[count].fElement = new CSRStaticDate(this,eLastEODDate);

	count++;

	fColumns[count].fColumnName		= "NAV Frequency";
	fColumns[count].fColumnWidth	= 100;
	fColumns[count].fAlignmentType = aLeft;
	fColumns[count].fElement = new CSRStaticText(this,eNavFrequency,70);

	SetDynamicSize(true);
	SetLineCount(0);
}

CSxAccountEntitiesEditList::~CSxAccountEntitiesEditList()
{
}

////// CSxOKButton /////////////////////

CSxOKButton::CSxOKButton(CSRFitDialog *dialog, int ERId_Element, _STL::string tooltip):CSRButton(dialog,ERId_Element,tooltip)
{	
}

/*virtual*/	void	CSxOKButton::Action()
{
	CSRFitDialog* dlg = GetDialog();
	dlg->OnOK();
}



