#include "SphInc/gui/SphButton.h"

namespace eff
{
	namespace emafi
	{
		namespace gui
		{
			enum etatType
			{
				ETAT_COMPTABLE,
				ETAT_REGLEMENTAIRE
			};
			class CmdGenerate : public CSRButton
			{
			private:
				etatType eType;
				void generateBilan(const char * bilanTemplateFile);
				void generateEtatReglementaire(const char* etatRegTemplateFile);
			public:
				void generateReport(const char * bilanTemplateFile, long folio_id, long startDate, long endDate, std::string fileType, std::string typeDate, bool is_simulation, std::string reportTypeList);
				CmdGenerate(CSRFitDialog *dialog, int ERId_Element,etatType eType = etatType::ETAT_COMPTABLE) : CSRButton(dialog, ERId_Element), eType(eType) {}
				virtual	void Action();
			};
		}
	}
}