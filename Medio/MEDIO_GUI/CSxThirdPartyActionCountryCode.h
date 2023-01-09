#ifndef __CSxThirdPartyActionCountryCode_H__
#define __CSxThirdPartyActionCountryCode_H__

/*
** Includes
*/
// standard
#include "SphInc/backoffice_kernel/SphThirdPartyDlg.h"
#include "SphInc/backoffice_kernel/SphThirdPartyAction.h"

#include <string>
//#include <vector>
#include <unordered_set>

class CSxThirdPartyActionCountryCode : public sophis::backoffice_kernel::CSRThirdPartyAction
{
	DECLARATION_THIRDPARTY_ACTION(CSxThirdPartyActionCountryCode)

//------------------------------------ PUBLIC ---------------------------------
public:
	CSxThirdPartyActionCountryCode(void);
	~CSxThirdPartyActionCountryCode(void);

	virtual void VoteForCreation(sophis::backoffice_kernel::CSRThirdPartyDlg &tparty)
		throw (sophis::tools::VoteException);

	virtual void VoteForModification(sophis::backoffice_kernel::CSRThirdPartyDlg &tparty)
		throw (sophis::tools::VoteException);

	//std::vector<std::string> ALL_COUNTRY_CODES;
	std::unordered_set<std::string> ALL_COUNTRY_CODES;
//------------------------------------ PRIVATE --------------------------------
private:
	void checkCountryCode(sophis::backoffice_kernel::CSRThirdPartyDlg &tparty)
		throw (sophis::tools::VoteException);
	static const char* __CLASS__;
};

#endif //!__CSxThirdPartyActionCountryCode_H__
