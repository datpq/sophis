#include "CtxTemplates.h"

using namespace eff::emafi::gui;

#define SQL_FOLIO "SELECT IDENT, NAME FROM FOLIO ORDER BY NAME"
#define SQL_FUND "SELECT T.SICOVAM, T.LIBELLE FROM TITRES T WHERE T.TYPE = 'Z' AND T.MNEMO IS NOT NULL AND T.MNEMO != 0 ORDER BY T.LIBELLE"
#define SQL_FUND_FOLIO "SELECT T.MNEMO, T.LIBELLE FROM TITRES T WHERE T.TYPE = 'Z' AND T.MNEMO IS NOT NULL AND T.MNEMO != 0 ORDER BY T.LIBELLE"
#define SQL_ACCOUNT_NUMBER "SELECT ROWNUM, ACCOUNT_NUMBER FROM (SELECT DISTINCT ACCOUNT_NUMBER FROM ACCOUNT_MAP WHERE ACCOUNT_NUMBER IS NOT NULL AND ACCOUNT_NUMBER NOT LIKE '%#%') ORDER BY 2"

ComboBoxFolio::ComboBoxFolio(CSRFitDialog *dialog, int ERId_Menu, void (*f)(CtxComboBox *))
	: CtxComboBox(dialog, ERId_Menu, SQL_FOLIO,	true, -1, f, false) {}
ComboBoxFolio::ComboBoxFolio(CSREditList *list, int ERId_Menu, void (*f)(CtxComboBox *))
	: CtxComboBox(list, ERId_Menu, SQL_FOLIO, true, -1, f, false) {}

ComboBoxFund::ComboBoxFund(CSRFitDialog *dialog, int ERId_Menu, void(*f)(CtxComboBox *))
	: CtxComboBox(dialog, ERId_Menu, SQL_FUND, true, -1, f, false) {}
ComboBoxFund::ComboBoxFund(CSREditList *list, int ERId_Menu, void(*f)(CtxComboBox *))
	: CtxComboBox(list, ERId_Menu, SQL_FUND, true, -1, f, false) {}

ComboBoxFundFolio::ComboBoxFundFolio(CSRFitDialog *dialog, int ERId_Menu, void(*f)(CtxComboBox *))
	: CtxComboBox(dialog, ERId_Menu, SQL_FUND_FOLIO, true, -1, f, false) {}
ComboBoxFundFolio::ComboBoxFundFolio(CSREditList *list, int ERId_Menu, void(*f)(CtxComboBox *))
	: CtxComboBox(list, ERId_Menu, SQL_FUND_FOLIO, true, -1, f, false) {}

ComboBoxAccountNumber::ComboBoxAccountNumber(CSRFitDialog *dialog, int ERId_Menu, void (*f)(CtxComboBox *))
	: CtxComboBox(dialog, ERId_Menu, SQL_ACCOUNT_NUMBER, true, -1, f, false) {}
ComboBoxAccountNumber::ComboBoxAccountNumber(CSREditList *list, int ERId_Menu, void (*f)(CtxComboBox *))
	: CtxComboBox(list, ERId_Menu, SQL_ACCOUNT_NUMBER, true, -1, f, false) {}
