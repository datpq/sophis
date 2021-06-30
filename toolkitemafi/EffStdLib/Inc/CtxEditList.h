#pragma once
#include "SphInc/gui/SphEditList.h"
#include <vector>

using namespace std;

namespace eff
{
	namespace gui
	{
		class CtxEditList : public CSREditList
		{
		private:
			void (*SelectedIndexChangedHandler)(CtxEditList *, int lineNumber);
			void (*DoubleClickHandler)(CtxEditList *, int lineNumber);
			std::string m_Query;
		public:
			CtxEditList(CSRFitDialog *dialog, int ERId_List,
				void (*selectedIndexChangedHandler)(CtxEditList *, int lineNumber) = NULL,
				void (*doubleClickHandler)(CtxEditList *, int lineNumber) = NULL);
			virtual void Initialize() {};
			virtual void LoadData(const char * query) { m_Query = query; };
			virtual void GetLineColor(int lineIndex, eTextColorType &color) const {};
			void ReloadData();

			/** Select all the lines with a given data in a given column
			@param colNumber is the number of column under which the cell is contained.
			*/
			void Selection(int colNumber, const char * cellValue);

			/**Transfers to 'address' the information handled by the CSRElement of the selected cell
			@param colNumber is the number of column under which the cell is contained.
			@param address is the address where the element's data value is to be copied.
			*/
			void GetSelectedValue(int colNumber, void * address);
			vector<void *> GetMultiSelectedValues(int colNumber);
			string GetCombineSelectedString(int colNumber);

			//Events
			virtual void SetSelectedIndexChangedHandler(void (*f)(CtxEditList *, int lineNumber)) { SelectedIndexChangedHandler = f; };
			virtual void SetDoubleClickHandler(void (*f)(CtxEditList *, int lineNumber)) { DoubleClickHandler = f; };

			//CSREditList
			virtual void Selection(int lineNumber);
			virtual void DoubleClick(int lineNumber);
			virtual void GetLineState(int lineIndex, eTextStyleType &style, eTextColorType &color, Boolean	&isSelected, Boolean &canEdit, Boolean &isStrikedThrough) const;
		};
	}
}