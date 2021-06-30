#include "CmdGenerate.h"
#include "Resource\resource.h"
#include "GenerateBilanDlg.h"
#include "Bilan.h"
#include "RapportGeneral.h"
#include "systemutils.h"
#include "Config.h"
#include "SphSDBCInc/SphSQLQuery.h"
#include "SqlUtils.h"
#include "EtatsReglementaireDlg.h"
#include "Log.h"
#include "CtxEditList.h"
#include "CtxElements.h"
#include "SphInc\portfolio\SphPosition.h"
#include "SphInc\portfolio\SphPortfolio.h"
#include "SphInc\SphRiskApi.h"
#include "RptUtils.h"
#include "msxmlutils.h"
#include "EdlFund.h"

#include"SqlWrapper.h"

#define MSG_LEN 255

using namespace eff::emafi::reports;
using namespace eff::emafi::gui;
using namespace eff::utils;
using namespace sophis::sql;
using namespace std;
using namespace eff::gui;
using namespace eff::utils::report;

void lauch_query_opch3( const char * reportType, StBilan*& v_actifs, int& count)
{
	char query[SQL_LEN] = {'\0'};

	_snprintf_s(query,sizeof(query),"SELECT RUBRIC, LABEL, LABEL_QUERY, RUBRIC_TYPE, INFO_TYPE, LABEL_SUM FROM V_EMAFI_PARAMETERS WHERE REPORT_TYPE = '%s' ORDER BY RUBRIC_ORDER, LABEL_ORDER", reportType);
	DEBUG("Preparing Bilan Query ...");
	DEBUG(query);
	CSRStructureDescriptor * gabarit = new CSRStructureDescriptor(6, sizeof(StBilan));
	ADD(gabarit, StBilan, rubric, rdfString);
	ADD(gabarit, StBilan, label, rdfString);
	ADD(gabarit, StBilan, query, rdfString);
	ADD(gabarit, StBilan, rubric_type, rdfString);
	ADD(gabarit, StBilan, info_type, rdfString);
	ADD(gabarit, StBilan, sum, rdfString);
	errorCode err  = QueryWithNResultsArray(query, gabarit, (void **)&v_actifs, &count);
	delete gabarit;
}

void appendRows(IA* report, string sStartDate, string sEndDate, string selectedFolioLabel, string sGestionnaire, int nCount, SqlWrapper* sql_wrapper, long folio_id, long lEndDate)
{
	//set parameter values
	IXMLDOMNode* pWorksheet = report->getWorksheetNode();


	const CSRPortfolio* folio = CSRPortfolio::GetCSRPortfolio(folio_id);
	if (folio)
	{
		folio->Load();
		set<long> folioIds;
		folioIds.insert(folioIds.begin(), folio_id);
		CSRPortfolio::DoReporting(folioIds, lEndDate, eReportingType::rDetailed, eAveragePriceComputationType::apcWap);
	}

	for(int i = 0; i<nCount;i++)
	{
		//code valable pour l'inventaire des actifs
		const CSRPosition* pos = CSRPosition::GetCSRPosition((*sql_wrapper)[i][11].value<long>(), (*sql_wrapper)[i][12].value<long>());
		double avgPrice = 0;
		if (pos)
			avgPrice = pos->GetAveragePrice();
		report->appendRow((*sql_wrapper)[i], avgPrice);

	}

	report->updateExpandedRow();
}


void appendRowsOPC4(IA* report, int nCount1, SqlWrapper* sql_wrapper1, SqlWrapper* sql_wrapper2, long folio_id, long lEndDate)
{
	//set parameter values
	IXMLDOMNode* pWorksheet = report->getWorksheetNode();

	const CSRPortfolio* folio = CSRPortfolio::GetCSRPortfolio(folio_id);

	if (folio)
	{
		folio->Load();
		set<long> folioIds;
		folioIds.insert(folioIds.begin(), folio_id);
		CSRPortfolio::DoReporting(folioIds, lEndDate, eReportingType::rDetailed, eAveragePriceComputationType::apcWap);
	}

	for (int i = 0; i<nCount1; i++)
	{
		const CSRPosition* pos = CSRPosition::GetCSRPosition((*sql_wrapper1)[i][11].value<long>(), (*sql_wrapper1)[i][12].value<long>());
		double avgPrice = 0;
		if (pos)
			avgPrice = pos->GetAveragePrice();
		report->appendRowOPC4((*sql_wrapper1)[i], avgPrice, (*sql_wrapper2)[0][6].value<double>());
	}

	if(nCount1!=0) report->updateExpandedRow();

	report->insertFooter();
	

	report->appendRowOPC4_tableau2("Dépôt à terme (Plus de 2 ans)", (*sql_wrapper2)[0][0].value<double>(), 0, (*sql_wrapper2)[0][6].value<double>());
	report->appendRowOPC4_tableau2("Créances représentatives des titres reçus en pen", (*sql_wrapper2)[0][1].value<double>(), 0, (*sql_wrapper2)[0][6].value<double>());
	report->appendRowOPC4_tableau2("Autres actifs", (*sql_wrapper2)[0][2].value<double>(), 0, (*sql_wrapper2)[0][6].value<double>());
	report->appendRowOPC4_tableau2("Liquidité", (*sql_wrapper2)[0][3].value<double>(), 0, (*sql_wrapper2)[0][6].value<double>());
	report->appendRowOPC4_tableau2("Total actifs", (*sql_wrapper2)[0][4].value<double>(), (*sql_wrapper2)[0][5].value<double>(), (*sql_wrapper2)[0][6].value<double>());

	report->updateExpandedRow();
}


void appendRowsOPC5(IA* report,SqlWrapper* sql_wrapper)
{
	
	IXMLDOMNode* pWorksheet = report->getWorksheetNode();


	for (int i = 3; i<10; i++)
	{		
		if (i==4)
			report->appendRowOPC5((*sql_wrapper)[0][i].value<double>(), false,true);
		if(i!=9 && i!=4) 
			report->appendRowOPC5((*sql_wrapper)[0][i].value<double>(), false,false);		
		if (i==9)
			report->appendRowOPC5((*sql_wrapper)[0][i].value<double>(), true,false);	
	}
	
	report->updateExpandedRow();
}



using namespace sophis::portfolio;
using namespace std;

void CmdGenerate::generateReport(const char * bilanTemplateFile, long folio_id,
	long lStartDate, long lEndDate, string fileType, string typeDate, bool is_simulation, string reportTypeList)
{
	string templateBilanFile = Config::getInstance()->getDllDirectory() + "\\" + bilanTemplateFile;
	string folio, sGestionnaire, sEntity;
	try {
		//try to get fund's name
		folio = SqlUtils::QueryReturning1StringException("SELECT T.LIBELLE FROM TITRES T WHERE T.TYPE = 'Z' AND T.MNEMO = %d", folio_id);
	} catch(const CSRNoRowException){
		//if error then get the folio's name
		folio = SqlUtils::QueryReturning1StringException("SELECT NAME FROM FOLIO WHERE IDENT = %d", folio_id);
	}
	try {
		sGestionnaire = SqlUtils::QueryReturning1StringException("SELECT TI.EXTERNREF FROM TITRES T JOIN TIERS TI ON TI.IDENT = T.CODE_EMET WHERE T.MNEMO = %d", folio_id);
	} catch (const CSRNoRowException){}
	try {
		sEntity = SqlUtils::QueryReturning1StringException("SELECT TI.NAME FROM TITRES T JOIN TIERS TI ON TI.IDENT = T.CODE_EMET WHERE T.MNEMO = %d", folio_id);
	} catch (const CSRNoRowException){}
	string sStartDate = SqlUtils::QueryReturning1StringException("SELECT TO_CHAR(NUM_TO_DATE(%d), 'DD/MM/YYYY') FROM DUAL", lStartDate);
	string sStartDateYear = SqlUtils::QueryReturning1StringException("SELECT TO_CHAR(NUM_TO_DATE(%d), 'YYYY') FROM DUAL", lStartDate);
	string sEndDate = SqlUtils::QueryReturning1StringException("SELECT TO_CHAR(NUM_TO_DATE(%d), 'DD/MM/YYYY') FROM DUAL", lEndDate);
	string sEndDateYear = SqlUtils::QueryReturning1StringException("SELECT TO_CHAR(NUM_TO_DATE(%d), 'YYYY') FROM DUAL", lEndDate);

	map<string, string> arrParamValues;
	arrParamValues["{DATE_TYPE}"] = string(typeDate) == "Generation Date" ? "GENERATION_DATE" : "POSTING_DATE";
	arrParamValues["{START_DATE}"] = sStartDate;
	
	arrParamValues["{START_DATE_YEAR}"] = sStartDateYear;
	arrParamValues["{END_DATE}"] = sEndDate;
	arrParamValues["{END_DATE_YEAR}"] = sEndDateYear;
	arrParamValues["{FOLIO_ID}"] = to_string((long long)folio_id);
	arrParamValues["{STATUS_LIST}"] = is_simulation ? "2,5,6,7,9,10,12,15" : "4,6,12,15";
	arrParamValues["{FOLIO_NAME}"] = folio;
	arrParamValues["{GESTIONNAIRE}"] = sGestionnaire;
	arrParamValues["{ENTITY}"] = sEntity;
	arrParamValues["{BANKS}"] = "'514','51400','514%'";
	arrParamValues["{START_DATE_GL}"] = sStartDate; // Bourama..Just for report type Grand Livre

	string reportQuery = StrFormat("SELECT REPORT_TYPE, NAME, PARAMETERVALUES, WORKSHEET, QUERY, QUERYDESC FROM EMAFI_REPORT WHERE ENABLED = 1 AND REPORT_TYPE IN (%s) ORDER BY REPORT_ORDER", reportTypeList.c_str());
	DEBUG("REPORT SELECTION QUERY = %s", reportQuery.c_str());
	SqlWrapper* reportWrapper = new SqlWrapper("c4,c50,c2000,c50,c4000,c255", reportQuery.c_str());
	int reportCount = reportWrapper->GetRowCount();

	RapportGeneral rapportG(templateBilanFile.c_str());
	rapportG.LoadParamValues(arrParamValues); 

	/*pictures data for each worksheet*/
	picture *pictures = new picture[reportCount];
	for(int i = 0; i < reportCount; i++)
	{
		vector<Variant> reportCols = (*reportWrapper)[i];
		string reportType = reportCols[0].value<string>();
		arrParamValues["{REPORT_TYPE}"] = reportType;
	
		string reportName = reportCols[1].value<string>();
		string reportParams = reportCols[2].value<string>();
		string reportWorksheet = reportCols[3].value<string>();
		string reportQuery = reportCols[4].value<string>();
		string reportQueryDesc = reportCols[5].value<string>();

		rapportG.creatWorksheet(reportWorksheet.c_str(), reportName.c_str());
		IXMLDOMNode* pWorkSheet = rapportG.getWorkSheetNodes(i);

		StrReplace(reportQuery, arrParamValues);
		map<string, string> arrReportParamValues;
		if (!reportParams.empty()) {
			StrReplace(reportParams, arrParamValues);

			vector<string> arrPvs = StrSplit(reportParams.c_str(), ";");
			for (vector<string>::iterator it = arrPvs.begin() ; it != arrPvs.end(); ++it)
			{
				vector<string> onePv = StrSplit(it->c_str(), "=");
				arrReportParamValues[onePv[0]] = onePv[1];
			}
			XmlReplace(pWorkSheet, arrReportParamValues);
		}
				/* Start Bourama
		add {START_DATE_GL} to Bilan just for report type Grand Livre */
	
		if (reportType != "GL"){ // delete entry in worksheet
			map<string, string> arrReportParamValues;
			arrReportParamValues[string("Date de debut : ")+sStartDate] = "";
			//arrReportParamValues[sStartDate] = "";
			XmlReplace(pWorkSheet, arrReportParamValues);
		}

		/*End Bourama*/

		DEBUG("REPORT_GENERATION: REPORT_TYPE = %s, REPORT_NAME = %s", reportType.c_str(), reportName.c_str());
		
		if(reportWorksheet == "InventaireActifs")
		{
			IA* inventaire = new IA(rapportG.pXMLDom, pWorkSheet, reportWorksheet);
			BSTR bstr = _com_util::ConvertStringToBSTR(reportName.c_str());
			setNodeAttribute(rapportG.pXMLDom,pWorkSheet,L"ss:Name",bstr);

			inventaire->init();
			StrReplace(reportQuery, arrParamValues);
			DEBUG("REPORT_TYPE = %s, QUERY = %s", reportType.c_str(), reportQuery.c_str());
			SqlWrapper * sqlWrapper = new SqlWrapper(reportQueryDesc.c_str(), reportQuery.c_str());
			appendRows(inventaire, sStartDate, sEndDate, folio, sGestionnaire, sqlWrapper->GetRowCount(), sqlWrapper, folio_id, lEndDate);
			delete sqlWrapper;
		}
		else
		{
			Bilan_* bilan = new Bilan_(rapportG.pXMLDom, pWorkSheet, reportWorksheet, reportName);

			vector<string> reportQueries = StrSplit(reportQuery.c_str(), "@");
			vector<string> reportQueriesDesc = StrSplit(reportQueryDesc.c_str(), "@");
			for(int j = 0; j<reportQueries.size(); j++) {
				for(int k = 0; k<j; k++) {
					StrReplace(reportQueries[j], StrFormat("{T%d}", k+1), reportQueries[k]);
				}
				string templateNodeName = StrFormat("{TableHeader%d}", j+1);
				if (arrReportParamValues.find(templateNodeName.c_str()) != arrReportParamValues.end()) {
					bilan->appendTemplateNode("{TableHeader}", arrReportParamValues[templateNodeName].c_str());
				}

				DEBUG("REPORT_TYPE = %s, QUERY = %s", reportType.c_str(), reportQueries[j].c_str());
				if (bilan->getTemplateNode("{TITLE}")) { 
					string titleQuery = StrFormat("\n\
SELECT T.DESCRIPTION TITLE, S.DESCRIPTION SUBTITLE, \n\
	COUNT(S.CODE) OVER (PARTITION BY T.CODE) NUM_OF_SUBTITLES, COUNT(S.CODE) OVER (PARTITION BY R.REPORT_TYPE) TOTAL_SUBTITLES \n\
FROM EMAFI_REPORT R \n\
	LEFT JOIN EMAFI_PARAMETRAGE T ON T.CATEGORIE = 'ReportTitre%d_' || R.REPORT_TYPE \n\
	LEFT JOIN EMAFI_PARAMETRAGE S ON S.CATEGORIE = T.CODE \n\
WHERE R.ENABLED = 1 AND R.REPORT_TYPE = '%s' \n\
ORDER BY R.REPORT_ORDER, T.ORDRE, S.ORDRE", j+1, reportType.c_str());

					DEBUG("TITLE QUERY = %s", titleQuery.c_str());
					SqlWrapper * titleQueryWrapper = new SqlWrapper("c100,c100,l,l", titleQuery.c_str());
					int titleCount = titleQueryWrapper->GetRowCount();
					bool firstTitle = true;
					bool firstSubTitle = true;
					for(int k=0; k<titleCount; k++)
					{
						string title = (*titleQueryWrapper)[k][0].value<string>();
						if (title.empty()) title = " "; // avoid the NULL value
						string subTitle = (*titleQueryWrapper)[k][1].value<string>();
						long numOfSubTitles = (*titleQueryWrapper)[k][2].value<long>();
						long totalSubTitles = (*titleQueryWrapper)[k][3].value<long>();
						StrReplace(title, arrParamValues);
						StrReplace(subTitle, arrParamValues);
						bilan->appendTitle(title.c_str(), firstTitle, subTitle.c_str(), firstSubTitle, numOfSubTitles, totalSubTitles);
					}
				}
				if (!reportQueries[j].empty()) {
					//Start Bourama
					//SqlWrapper * sqlWrapper = new SqlWrapper(reportQueriesDesc[j].c_str(), reportQueries[j].c_str());
					map<string, string> arrLongDouble;
					arrLongDouble["l"] = "d";
					StrReplace(reportQueriesDesc[j], arrLongDouble);
					SqlWrapper * sqlWrapper = new SqlWrapper(reportQueriesDesc[j].c_str(), reportQueries[j].c_str());
					//End Bourama
					bilan->arrColumnDataTypes = sqlWrapper->GetDataTypes();
					for(int iter = 0; iter < sqlWrapper->GetRowCount();iter++)
					{
						vector<Variant> arrResults = (*sqlWrapper)[iter];
						bilan->appendRow(arrResults, iter==sqlWrapper->GetRowCount()-1);
					}
					delete sqlWrapper;
				}

				templateNodeName = StrFormat("{TableFooter%d}", j+1);
				if (arrReportParamValues.find(templateNodeName.c_str()) != arrReportParamValues.end()) {
					bilan->appendTemplateNode("{TableFooter}", arrReportParamValues[templateNodeName].c_str());
				}
			}
			bilan->updateExpandedRowCol();
			//Bourama dynamic column
			for (int j = 0; j < reportQueries.size(); j++) {
				string titleQuery = StrFormat("\n\
				SELECT T.DESCRIPTION TITLE, S.DESCRIPTION SUBTITLE, T.CODE CODE, \n\
				COUNT(S.CODE) OVER (PARTITION BY T.CODE) NUM_OF_SUBTITLES, COUNT(S.CODE) OVER (PARTITION BY R.REPORT_TYPE) TOTAL_SUBTITLES \n\
				FROM EMAFI_REPORT R \n\
				LEFT JOIN EMAFI_PARAMETRAGE T ON T.CATEGORIE = 'ReportTitre%d_' || R.REPORT_TYPE \n\
				LEFT JOIN EMAFI_PARAMETRAGE S ON S.CATEGORIE = T.CODE \n\
				WHERE R.ENABLED = 1 AND R.REPORT_TYPE = '%s' \n\
				ORDER BY R.REPORT_ORDER, T.ORDRE, S.ORDRE", j + 1, reportType.c_str());

				DEBUG("TITLE QUERY = %s", titleQuery.c_str());
				SqlWrapper * titleQueryWrapper = new SqlWrapper("c100,c100,c100,l,l", titleQuery.c_str());
				int titleCount = titleQueryWrapper->GetRowCount();
				map<string, string> arrQuot;
				arrQuot["'"] = "''";
				for (int k = 0; k < titleCount; k++)
				{
					string title = (*titleQueryWrapper)[k][0].value<string>();
					if (title.empty()) title = " "; // avoid the NULL value
					string subTitle = (*titleQueryWrapper)[k][1].value<string>();
					string code = (*titleQueryWrapper)[k][2].value<string>();
					StrReplace(title, arrQuot);
					StrReplace(subTitle, arrQuot);
					bilan->setColumnLenght(title.c_str(), subTitle.c_str(), code.c_str(), k);//
				}
			}// End Bourama

			
		}
		pictures[i].fileName = eff::utils::Config::getInstance()->getDllDirectory() + "\\eff.png";
		rapportG.getLogoPosition(i, pictures[i].rowIndex, pictures[i].cellIndex);
	}

	std::string generatedBilanFile = Config::getInstance()->getDllDirectory() + "\\EtatsComptables_" +
		+ folio.c_str() + "_" + GetWindowsUserName() + "_" + GetCurDateTimeYYMMDDHHMMSS() + ".xml";
	StrReplace(generatedBilanFile, "/", "_");

	rapportG.save(generatedBilanFile.c_str());
	std::string fileStr;
	std::string filePdfStr;
	const char* pdfFileName = NULL;
	if(fileType == "PDF")
	{
		filePdfStr = Config::getInstance()->getDllDirectory() + "\\EtatsComptables_" +
			+ folio.c_str() + "_" + GetWindowsUserName() + "_" + GetCurDateTimeYYMMDDHHMMSS() + ".pdf";
		StrReplace(filePdfStr, "/", "_");
		pdfFileName = filePdfStr.c_str();
	}

	fileStr = Config::getInstance()->getDllDirectory() + "\\EtatsComptables_" +
			+ folio.c_str() + "_" + GetWindowsUserName() + "_" + GetCurDateTimeYYMMDDHHMMSS() + ".xlsx";
	StrReplace(fileStr, "/", "_");
	const char* excelFileName = fileStr.c_str();
	

	if (OpenExcel(generatedBilanFile.c_str(), pdfFileName, excelFileName, pictures) != 1) {
		GetDialog()->Message(LoadResourceString(MSG_FILE_GENERATED, generatedBilanFile.c_str()).c_str());
	}

	delete[] pictures;
	DEBUG("END");
}

void CmdGenerate::generateBilan(const char * bilanTemplateFile)
{
	DEBUG("BEGIN");

	GenerateBilanDlg *bilanDlg = (GenerateBilanDlg *)GetDialog();
	long selectedFolioId = 0;
	CSRRadioButton *radioSelection = (CSRRadioButton *)bilanDlg->GetElementByAbsoluteId(IDC_RADIO_FOLIO - ID_ITEM_SHIFT);
	int selectedValue = *(radioSelection->GetValue());
	if (selectedValue == 1){
		selectedFolioId = ((CtxComboBox *)bilanDlg->GetElementByAbsoluteId(IDC_CBO_FOLIO - ID_ITEM_SHIFT))->SelectedValue();
	}
	else if (selectedValue == 2) {
		selectedFolioId = ((CtxComboBox *)bilanDlg->GetElementByAbsoluteId(IDC_CBO_FUND - ID_ITEM_SHIFT))->SelectedValue();
	}
	CSREditDate *startDate = (CSREditDate *)bilanDlg->GetElementByAbsoluteId(IDC_TXT_START_DATE - ID_ITEM_SHIFT);
	long lStartDate = 0;
	startDate->GetValue(&lStartDate);
	
	CSREditDate *endDate = (CSREditDate *)bilanDlg->GetElementByAbsoluteId(IDC_TXT_END_DATE - ID_ITEM_SHIFT);
	long lEndDate = 0;
	endDate->GetValue(&lEndDate);

	CtxComboBoxItem selectedFileType = bilanDlg->GetSelectedFileType();
	std::string fileType = selectedFileType.label;

	CSRCheckBox* simulationCheckBox = (CSRCheckBox*)bilanDlg->GetElementByAbsoluteId(IDC_CHECK_SIMULATION - ID_ITEM_SHIFT);
	bool is_simulation = *(simulationCheckBox->GetValue());

	CtxComboBox* typeDates = (CtxComboBox*)bilanDlg->GetElementByAbsoluteId(IDC_COMBO_TYPEDATE - ID_ITEM_SHIFT);
	string typeDate(typeDates->SelectedText());

	CtxEditList* reportsList = (CtxEditList*)bilanDlg->GetElementByAbsoluteId(IDC_LIST_REPORT - ID_ITEM_SHIFT);
	if(reportsList->GetSelectedLines().size() == 0)
	{
		bilanDlg->Message(LoadResourceString(MSG_SELECT_REPORT).c_str());
		return;
	}
	generateReport(bilanTemplateFile, selectedFolioId, lStartDate, lEndDate, fileType, typeDate, is_simulation, reportsList->GetCombineSelectedString(1));
}



void CmdGenerate::generateEtatReglementaire(const char* etatRegTemplateFile)
{
	DEBUG("BEGIN");

	long lConsultDate = 0;

	EtatReglementaireDlg* regDlg = (EtatReglementaireDlg*)GetDialog();
	CtxEditList* fundsList = (CtxEditList*)regDlg->GetElementByAbsoluteId(IDC_LST_FUND - ID_ITEM_SHIFT);

	vector<long> selectedFunds = fundsList->GetSelectedLines();

	EdlFundItem selectedFund = EdlFundItem();
	fundsList->GetSelectedValue(0, &selectedFund.sicovam);
	fundsList->GetSelectedValue(1, &selectedFund.libelle);

	regDlg->GetElementByAbsoluteId(IDC_DATE_CONSULTATION - ID_ITEM_SHIFT)->GetValue(&lConsultDate);
	string sConsultDate = SqlUtils::QueryReturning1StringException("SELECT TO_CHAR(NUM_TO_DATE(%d), 'DD/MM/YYYY') FROM DUAL", lConsultDate);

	std::string templateBilanFile = Config::getInstance()->getDllDirectory() + "\\" + etatRegTemplateFile;
	int reportCount = 0;

	CtxEditList* reportsList = (CtxEditList*)regDlg->GetElementByAbsoluteId(IDC_LST_REPORT_REG - ID_ITEM_SHIFT);
	vector<long> selectedLines = reportsList->GetSelectedLines();
	if (selectedLines.empty())
	{
		regDlg->Message(LoadResourceString(MSG_SELECT_REPORT).c_str());
		return;
	}

	string sGestionnaire, sPeriodicity;
	long folio_id_;
	try {
		sGestionnaire = SqlUtils::QueryReturning1StringException("SELECT TI.EXTERNREF FROM TITRES T JOIN TIERS TI ON TI.IDENT = T.CODE_EMET WHERE T.SICOVAM = %d", long(selectedFund.sicovam));
	}
	catch (const CSRNoRowException){}


	try {
		sPeriodicity = SqlUtils::QueryReturning1StringException("select DECODE(NAVDATESTYPE,1,'Daily',2,'Weekly','Custom') FROM FUNDS WHERE SICOVAM = %d", long(selectedFund.sicovam));
	}
	catch (const CSRNoRowException){}

	try {
		folio_id_ = SqlUtils::QueryReturning1LongException("SELECT T.MNEMO  FROM TITRES T WHERE T.LIBELLE = '%s'", selectedFund.libelle);
	}
	catch (const CSRNoRowException){}

	map<string, string> arrParamValues;

	arrParamValues["{END_DATE}"] = sConsultDate;
	if (selectedFunds.size() == 1)
	{
		arrParamValues["{GESTIONNAIRE}"] = sGestionnaire; //only if 1 fund is selected
		arrParamValues["{PERIODICITE}"] = sPeriodicity; //only if 1 fund is selected
		arrParamValues["{DEPOSITARY}"] = "";
		arrParamValues["{FOLIO_NAME}"] = selectedFund.libelle;
	}
	else
	{
		arrParamValues["{GESTIONNAIRE}"] = "";
		arrParamValues["{PERIODICITE}"] = "";
	}

	string query = StrFormat("SELECT REPORT_TYPE, NAME, PARAMETERVALUES, WORKSHEET, QUERY, QUERYDESC FROM EMAFI_REPORT WHERE ETAT_TYPE = 'R' AND ENABLED = 1 AND REPORT_TYPE IN (%s) ORDER BY REPORT_ORDER ",
		reportsList->GetCombineSelectedString(1).c_str());
	SqlWrapper* reportWrapper = new SqlWrapper("c4,c50,c2000,c50,c4000,c255", query.c_str());

	reportCount = reportWrapper->GetRowCount();
	RapportGeneral rapportG(templateBilanFile.c_str(), 'R');
	
	picture *pictures = new picture[reportCount];

	for (int i = 0; i<reportCount; i++) {
		int count = 0;
		vector<Variant> reportCols = (*reportWrapper)[i];

		string reportType = reportCols[0].value<string>();
		string reportName = reportCols[1].value<string>();
		string reportParams = reportCols[2].value<string>();
		string reportWorksheet = reportCols[3].value<string>();
		string reportQuery = reportCols[4].value<string>();
		string reportQueryDesc = reportCols[5].value<string>();
		//Getting the worksheet node
		rapportG.creatWorksheet(reportWorksheet.c_str(), reportName.c_str());
		IXMLDOMNode* pWorkSheet = rapportG.getWorkSheetNodes(i);
		BSTR bst = _com_util::ConvertStringToBSTR(reportName.c_str());
		setNodeAttribute(rapportG.pXMLDom, pWorkSheet, L"ss:Name", bst);
		
		
		map<string, string> arrReportParamValues;

		if (!reportParams.empty()) {
			

			vector<string> arrPvs = StrSplit(reportParams.c_str(), ";");
			for (vector<string>::iterator it = arrPvs.begin(); it != arrPvs.end(); ++it)
			{
				vector<string> onePv = StrSplit(it->c_str(), "=");
				arrReportParamValues[onePv[0]] = onePv[1];
			}
			XmlReplace(pWorkSheet, arrReportParamValues);
		}
		DEBUG("REPORT_GENERATION: REPORT_TYPE = %s, REPORT_NAME = %s", reportType.c_str(), reportName.c_str());
		
		if (reportWorksheet == "OPC6" || reportWorksheet == "OPC4" || reportWorksheet=="OPC5")
		{
			rapportG.LoadParamValues(arrParamValues);
		}
	
		if (reportWorksheet == "OPC6" || reportWorksheet == "OPCH3") 
		{
			StBilan* opc = NULL;
			lauch_query_opch3(reportType.c_str(), opc, count);
			OPCH3 op(rapportG.pXMLDom, pWorkSheet, "OPCH3", reportType, selectedFunds.size(), count);
			op.makeTitles(opc);
			for (auto iter = selectedFunds.begin(); iter != selectedFunds.end(); ++iter)
			{
				double fund_id;
				fundsList->LoadElement(*iter, 0, &fund_id);
				op.appendRow(opc, (long)fund_id, sConsultDate, iter == selectedFunds.begin());
			}
			op.updateExpandedColumn();
			op.updateExpandedRow();
		}
		if (reportWorksheet == "OPC4")
		{
			IA* inventaire = new IA(rapportG.pXMLDom, pWorkSheet, reportWorksheet);
			BSTR bstr = _com_util::ConvertStringToBSTR(reportName.c_str());
			setNodeAttribute(rapportG.pXMLDom, pWorkSheet, L"ss:Name", bstr);

			inventaire->init();
			
			map<string, string> arrParamQuery;
			arrParamQuery["{NAV_DATE}"] = sConsultDate;
			arrParamQuery["{FUND_ID}"] = to_string(long long(selectedFund.sicovam));
			
			vector<string> reportQueries = StrSplit(reportQuery.c_str(), "@");
			vector<string> reportQueriesDesc = StrSplit(reportQueryDesc.c_str(), "@");


			StrReplace(reportQueries[0], arrParamQuery);
			StrReplace(reportQueries[1], arrParamQuery);
			SqlWrapper * sqlWrapper = new SqlWrapper(reportQueriesDesc[0].c_str(), reportQueries[0].c_str());
			SqlWrapper * sqlWrapper2 = new SqlWrapper(reportQueriesDesc[1].c_str(), reportQueries[1].c_str());


			appendRowsOPC4(inventaire, sqlWrapper->GetRowCount(), sqlWrapper, sqlWrapper2, folio_id_, lConsultDate);
			delete sqlWrapper;
			delete sqlWrapper2;
		}

		if (reportWorksheet == "OPC5")
		{
			IA* opc5Rapport = new IA(rapportG.pXMLDom, pWorkSheet, reportWorksheet);
			BSTR bstr = _com_util::ConvertStringToBSTR(reportName.c_str());
			setNodeAttribute(rapportG.pXMLDom, pWorkSheet, L"ss:Name", bstr);

			opc5Rapport->init();

			map<string, string> arrParamQuery;
			arrParamQuery["{NAV_DATE}"] = sConsultDate;
			arrParamQuery["{FUND_ID}"] = to_string(long long(selectedFund.sicovam));
			
			StrReplace(reportQuery, arrParamQuery);
			SqlWrapper * sqlWrapper = new SqlWrapper(reportQueryDesc.c_str(), reportQuery.c_str());
			
			appendRowsOPC5(opc5Rapport, sqlWrapper);
			delete sqlWrapper;			
		}
	}

	CtxComboBoxItem selectedFileType = *((CtxComboBox*)regDlg->GetElementByAbsoluteId(IDC_COMBO_FILETYPE_REG - ID_ITEM_SHIFT))->SelectedItem();
	std::string fileType = selectedFileType.label;

	std::string generatedBilanFile = Config::getInstance()->getDllDirectory() + "\\EtatsReglementaire_" +
		+"_" + GetWindowsUserName() + "_" + GetCurDateTimeYYMMDDHHMMSS() + ".xml";

	rapportG.save(generatedBilanFile.c_str());
	std::string fileStr;
	std::string filePdfStr;
	const char* pdfFileName = NULL;
	if (fileType == "PDF")
	{
		filePdfStr = Config::getInstance()->getDllDirectory() + "\\EtatsReglementaires_" +
			+"_" + GetWindowsUserName() + "_" + GetCurDateTimeYYMMDDHHMMSS() + ".pdf";
		pdfFileName = filePdfStr.c_str();
	}

	fileStr = Config::getInstance()->getDllDirectory() + "\\EtatsReglementaires_" +
		+"_" + GetWindowsUserName() + "_" + GetCurDateTimeYYMMDDHHMMSS() + ".xlsx";
	const char* excelFileName = fileStr.c_str();

	if (OpenExcel(generatedBilanFile.c_str(), pdfFileName, excelFileName) != 1) {
		GetDialog()->Message(LoadResourceString(MSG_FILE_GENERATED, generatedBilanFile.c_str()).c_str());
	}
	delete[] pictures;
}




void CmdGenerate::Action()
{
	SqlUtils::QueryWithoutResultException("BEGIN EMAFI.UPDATE_CRE; END;");
	CSRSqlQuery::Commit();
	if(eType == etatType::ETAT_COMPTABLE)
		generateBilan("Bilan.xml");
	else
		generateEtatReglementaire("Reglementaire.xml");
}
