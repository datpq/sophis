#pragma once
#include "SphInc/gui/SphCustomMenu.h"
#include <string>

struct CtxComboBoxItem {
	long value;
	char label[255];
};

namespace eff
{
	namespace gui
	{
		class CtxComboBox : public CSRCustomMenu
		{
		private:
			std::vector<CtxComboBoxItem> m_Items;
			std::string textValue;
			bool m_Enabled;
			void (*SelectedIndexChangedHandler)(CtxComboBox *);
		public:
			CtxComboBox(CSRFitDialog *dialog, int ERId_Menu, const char * query = NULL, bool enabled = true, long selectedValue = -1,
				void (*f)(CtxComboBox *) = NULL, bool fireEvent = true);
			CtxComboBox(CSRFitDialog *dialog, int ERId_Menu, void (*f)(CtxComboBox *) = NULL);
			CtxComboBox(CSREditList *list, int ERId_Menu, const char * query = NULL, bool enabled = true, long selectedValue = -1,
				void (*f)(CtxComboBox *) = NULL, bool fireEvent = true);
			CtxComboBox(CSREditList *list, int ERId_Menu, void (*f)(CtxComboBox *) = NULL);

			int SelectedIndex() const;
			void SelectedIndex(const int selectedIndex, bool fireEvent = true);
			long SelectedValue() const;
			void SelectedValue(const long selectedValue, bool fireEvent = true);
			CtxComboBoxItem * SelectedItem() const;
			std::string SelectedText() const;
			void SelectedText(const std::string& selectedText, bool fireEvent = true);
			bool ContainsItemText(const char * text);
			bool ContainsItemValue(long value);

			void LoadItems(const char * arrItems[], unsigned size, long selectedIndex = -1);
			void LoadData(const char * query, long selectedValue = -1, bool fireEvent = true);

			//Events
			virtual void SetSelectedIndexChangedHandler(void (*f)(CtxComboBox *)) { SelectedIndexChangedHandler = f; };

			//CSRCustomMenu
			virtual void Action();
			virtual void Open();
			//virtual void AddElement(const char* element);
		};
	}
}

