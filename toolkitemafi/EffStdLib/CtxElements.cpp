#include "Inc/CtxElements.h"
#include "Inc/systemutils.h"
#include "SphInc\gui\SphDialogUI.h"

using namespace eff::gui;

Boolean CtxEditText::Validation(void)
{
	if (OnValiding != NULL) {
		OnValiding(this);
	}
	return CSREditText::Validation();
}

void CtxButton::Action()
{
	if (OnClick != NULL) {
		OnClick(this);
	}
}

Boolean CtxRadioButton::Validation(void)
{
	if (OnCheckedChanged != NULL) {
		OnCheckedChanged(this, *GetValue());
	}
	return CSRRadioButton::Validation();
}

Boolean CtxCheckBox::Validation(void)
{
	CtxCheckBox::Update();
	if (OnCheckedChanged != NULL) {
		OnCheckedChanged(this, *GetValue());
	}
	return CSRCheckBox::Validation();
}

void CtxGuiUtils::Enabled(CSRElement * element, bool enabled)
{
	if (enabled) {
		element->Enable();
	} else {
		element->Disable();
	}
}

std::string CtxGuiUtils::GetOpenFile(const char * title, const char * filter, CSRElement * element)
{
	return GetOpenFileDlg(title, filter, sophis::gui::CSRFitDialogUI::GetHWND(*element->GetDialog()));
}

std::vector<std::string> CtxGuiUtils::GetOpenFileMulti(const char * title, const char * filter, CSRElement * element)
{
	return GetOpenFileDlgMulti(title, filter, sophis::gui::CSRFitDialogUI::GetHWND(*element->GetDialog()));
}
