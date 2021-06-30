#pragma once
#include <msxml6.h>
#include <vector>
#include"SqlWrapper.h"
#include "StringUtils.h"
#include"msxmlutils.h"
#include <map>

using namespace eff::utils;
namespace eff
{
	namespace emafi
	{
		namespace reports
		{
			struct StBilan {
				long id_rubric;
				char rubric[50];
				char rubric_type[4];
				char label[255];
				double amount;
				char query[400];
				char info_type[3];
				char sum[2];
				//int rubric_order;
				//int label_order;
			};

			class Bilan_
			{
			public:
				Bilan_(IXMLDOMDocument* pXMLNode, IXMLDOMNode *pNodeWorkSheet,std::string worksheet_name, std::string report_name);
				virtual ~Bilan_();
				void appendRow(std::vector<Variant> data, bool isLastRow = false);
				void appendTitle(const char* title, bool &firstTitle, const char* subtitle, bool &firstSubTitle, int numOfTitles, int totalSubTitles);
				void appendTemplateNode(const char * templateName, const char * tableHeaderTitle);

				void appendRubric(string rubric, string rubric_type, bool insert = false, bool update = false);
				
				void loadInitialNodes();
				IXMLDOMNode * getTemplateNode(const char * nodeName);
				void updateExpandedRowCol();
				int getNumOfCols();
				void Bilan_::setColumnLenght(const char* title, const char* subTitle, const char* code, int numTitle); //bourama
				vector<SqlWrapper::eSqlDataType> arrColumnDataTypes;
			protected:
				void appendCell(CComVariant dataVal,const char* dataType,IXMLDOMNode* pRow, IXMLDOMNode* pCloneCell, IXMLDOMNode** pOutCell, int mergeDown = 0, int mergeAccross = 0);
				void updateFormula(IXMLDOMNode* pRow, std::string rubric_type, vector<IXMLDOMNode*> *formula_datas = NULL, const char * formula_operators = NULL);
				//XML Nodes
				IXMLDOMDocument* pXMLNode;
				IXMLDOMNode *pWorksheet, *pTable;
				IXMLDOMNode *pSubTitleRow, *pRubricRow, *pLabelRow, *pValRow, *pValTotalRow, *pCategRow, *pTotalRow;
				IXMLDOMNode *pSubTitleCell, *pRubricCell, *pLabelCell, *pValCell, *pValTotalCell, *pCategCell, *pTotalCell;
				IXMLDOMNode *pCurrentTitleRow, *pCurrentSubTitleRow;

				//auxiliary Nodes
				IXMLDOMNode *pCurrRow, *pCurrCell, *pPrevRubricRow, *pCurrTitleCell;
				std::map<string, IXMLDOMNode *> arrTemplateNodes;
				int numOfCols;

				//plain data
			
				std::string current_rubric, worksheet_name, current_rubric_type, current_title;
				int nb_labels; //per current rurbic

				std::map<string, IXMLDOMNode *> arrTaggedNodes;
				std::vector<IXMLDOMNode*> labels_to_sum, rubrics_to_sum, totals_to_sum;

				int nb_removed_rows, nb_removed_cols;
			};

			class Bilan
			{
			protected: //change private to protected for those data members to be used by the subclass (AR)

				virtual void loadTemplateFile();
				virtual void updateFormula();
				virtual void setRowFormula(IXMLDOMNode *pNodeRow, const char * sValue);

				std::vector<IXMLDOMNode *> m_rubricNodes;
				std::vector<int> m_rubricLabelCounts, m_totalNodeIdxs;
				long m_currentRubricId, m_firstRubricIdxToComputeTotal, m_currentRubricLabelCount, m_rowCount;
				double m_currentRubricVal,m_currentRubricValPrec;
				BSTR sXmlTemplateFile;
				IXMLDOMDocument *pXMLNode;
				IXMLDOMNode *pNodeTable, *pNodeRubric, *pNodeTotal, *pNodeTotalGeneral, *pNodeLabel, *pCategory, *m_currentRubricNode, *pNodeWorkSheet;
				std::string worksheet_name;
				bool isTotalExceeded;

			protected:
				long nRemovedRows;
			public:
				
				/*getters and setters*/
				IXMLDOMNode* getWorksheetNode();
				std::string getWorksheetName();

				Bilan(IXMLDOMDocument* pXMLNode, IXMLDOMNode *pNodeWorkSheet,std::string worksheet_name);
				void init();
				void appendRow(long id_rubric, const char * rubric, const char * rubric_type, const char * label, double amount, double amountPrec, double amount_second = 0, double amountPrec_second = 0);
				virtual void updateExpandedRow();

				virtual ~Bilan(void);
			};

			class IA : public Bilan // class for 'inventaire des actifs' Report
			{
			public:

				IA::IA(IXMLDOMDocument *pXMLDom, IXMLDOMNode *pNodeWorkSheet, std::string worksheet_name) : Bilan(pXMLDom, pNodeWorkSheet, worksheet_name), currentEmetteur("NaN")
				{
					pNodeEmetteur = pNodeQuantity = pNodeEmetteurRow = m_currentEmetteurNode = pCodeIsin = pNodeIsin = NULL;
					pNodeDepot = pNodePourcentage = NULL;
				}
				
				void appendRow(const char* emetteur_name,const char * code_isin, const char * designation, double quantite, 
								double  prix,double  val_boursiere, double actif_pourcentage, long number_of_assets);

				void appendRow(std::vector<Variant> data, double prix);
				void appendRowOPC4(vector<Variant> data, double prix, double total);
				void appendRowOPC4_tableau2(const char* colonne, double val1, double val2, double val3);
				void insertFooter();
				void appendRowOPC5(double valeur, bool endofFile, bool sautDeLigne);
				virtual ~IA(void);
						
			private:
				virtual void loadTemplateFile();

				IXMLDOMNode *pNodeEmetteur, *pNodeQuantity, *pNodeEmetteurRow, *m_currentEmetteurNode;
				IXMLDOMNode  *pNodeDepot, *pNodePourcentage, *pNodeFooterRow, *pNodeFooter;
				IXMLDOMNode* pCodeIsin, *pNodeIsin, *pNodeEmetteurSibling;
				std::string currentEmetteur;
			};



			/********* RAPPORTS REGLEMENTAIRE ***********/
			class OPCH3
			{
			public:
				OPCH3(IXMLDOMDocument* pXMLNode,IXMLDOMNode* pNodeWorkSheet, std::string worksheet_name,std::string report_type, int fundCount, int colCount);
				
				void appendColumn(const char* rubric, const char* label, const char* query,const char* rubric_type, const char* info_type, const char* sum, long fund_id, std::string consultDate,bool isFirstRow = false);
				void updateExpandedColumn();
				void updateExpandedRow();
				void appendRow(StBilan* oneRow, long fund_id,std::string sConsultDate, bool isFirstRow = false);
				void makeTitles(StBilan* oneRow);
				virtual ~OPCH3();
			private:

				IXMLDOMDocument* pXMLNode;
				IXMLDOMNode *pNodeWorkSheet, *pParam, *pParamVal, *pNodeRubric, *pNodeTotal, *pRubricVal, *pLabelRow, *pValRow, *pCurrLabelCell, *pCurrValCell, *pCurrRubricVal, *pTotalVal, *pCurrValRow, *pNodeTable;
				std::string worksheet_name, currentRubric, report_type;
				
				int n_RemovedCol, m_ColCount, m_LabelCount, n_RemovedRow;
				std::vector<int> m_rubricLabelCounts, mLabelToSum;
				int n_Rubrics, nFirstIndex;
				int fundCount, colCount;
				bool toUpdate;
				void loadInitialNodes();
				void updateFormula();	
				
			};
		}
	}
}


