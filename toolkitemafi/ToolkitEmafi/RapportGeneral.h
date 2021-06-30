#pragma once
#include <msxml6.h>
#include<map>
#include<vector>

namespace eff
{
	namespace emafi
	{
		namespace reports
		{
			class RapportGeneral
			{
			private:
				BSTR sXmlTemplateFile;
							
				IXMLDOMNode* pWorkbook;
				void loadTemplateFile();
				std::vector<IXMLDOMNode*> pWSNodes;
				std::map<std::string, IXMLDOMNode*> initialWS; // initial worksheets used to create new ws by duplicating them, they must be tracked to be deleted after
			
				char etat_type;

			public:
				RapportGeneral(const char * xmlTemplateFile, char etat_type = 'C');
				IXMLDOMNode* getWorkSheetNodes(int i);
				void save(const char * xmlFileDest);
		
				void creatWorksheet(const char * worksheetName, const char * newWorksheetName);
				void LoadParamValues(std::map<std::string, std::string> arrParamValues);

				void getLogoPosition(int sheetIndex, int&,int&);
				~RapportGeneral(void);
				IXMLDOMDocument *pXMLDom;
			};
		}
	}
}


