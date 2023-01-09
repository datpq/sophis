#ifndef _SOPHIS_EVENT_FIELD_H
#define _SOPHIS_EVENT_FIELD_H

#include "SphTools/base/CommonOS.h"
#include "SphEventCoreExport.h"
#include <errno.h>
#include <stddef.h>

#pragma managed(push, off)
#include __STL_INCLUDE_PATH(string)
#include __STL_INCLUDE_PATH(vector)
#pragma managed(pop)


EVENT_PROLOG
namespace sophis
{
	namespace event
	{
		class CSFields;

		class CSField
		{
		public:
			// Constructors / destructor
			SOPHIS_EVENT_CORE CSField(); // Default constructor
			SOPHIS_EVENT_CORE CSField(const CSField & copy); // Copy constructor
			SOPHIS_EVENT_CORE ~CSField(); // Destructor
			SOPHIS_EVENT_CORE CSField(const _STL::string & pName, const _STL::string & pType, const _STL::string & pValue, const CSFields & subFields); // User constructor

			// Name accessors
			SOPHIS_EVENT_CORE _STL::string & GetName(_STL::string & value) const;
			SOPHIS_EVENT_CORE _STL::string GetName() const;
			SOPHIS_EVENT_CORE errno_t GetName(char * value, size_t size) const;
			SOPHIS_EVENT_CORE void SetName(const _STL::string & value);

			// Type accessors
			SOPHIS_EVENT_CORE _STL::string & GetType(_STL::string & value) const;
			SOPHIS_EVENT_CORE _STL::string GetType() const;
			SOPHIS_EVENT_CORE errno_t GetType(char * value, size_t size) const;
			SOPHIS_EVENT_CORE void SetType(const _STL::string & value);

			// Value accessors
			SOPHIS_EVENT_CORE _STL::string & GetValue(_STL::string & value) const;
			SOPHIS_EVENT_CORE _STL::string GetValue() const;
			SOPHIS_EVENT_CORE errno_t GetValue(char * value, size_t size) const;
			SOPHIS_EVENT_CORE void SetValue(const _STL::string & value);

			// SubFields accessors
			SOPHIS_EVENT_CORE bool IsSubFieldsEmpty() const;
			SOPHIS_EVENT_CORE void ClearSubFields();
			SOPHIS_EVENT_CORE void AddSubFields(const CSField & value);
			SOPHIS_EVENT_CORE CSField & GetSubFieldsAt(size_t index, CSField & value) const;
			SOPHIS_EVENT_CORE void SetSubFieldsAt(size_t index, const CSField & value);
			SOPHIS_EVENT_CORE size_t GetSubFieldsSize() const;
			SOPHIS_EVENT_CORE void ResizeSubFields(size_t size);
			SOPHIS_EVENT_CORE errno_t GetSubFields(CSField * value, size_t size) const;
			SOPHIS_EVENT_CORE errno_t SetSubFields(const CSField * value, size_t size);

		private:
			_STL::string fName;
			_STL::string fType;
			_STL::string fValue;
			_STL::vector<CSField> fSubFields;
		}; // class CSField

		class CSFields
		{
		public:
			// Constructors / destructor
			SOPHIS_EVENT_CORE CSFields(); // Default constructor
			SOPHIS_EVENT_CORE CSFields(const CSFields & copy); // Copy constructor
			SOPHIS_EVENT_CORE ~CSFields(); // Destructor

			// Fields accessors
			SOPHIS_EVENT_CORE bool IsFieldsEmpty() const;
			SOPHIS_EVENT_CORE void ClearFields();
			SOPHIS_EVENT_CORE void AddFields(const CSField & value);
			SOPHIS_EVENT_CORE CSField & GetFieldsAt(size_t index, CSField & value) const;
			SOPHIS_EVENT_CORE void SetFieldsAt(size_t index, const CSField & value);
			SOPHIS_EVENT_CORE size_t GetFieldsSize() const;
			SOPHIS_EVENT_CORE void ResizeFields(size_t size);
			SOPHIS_EVENT_CORE errno_t GetFields(CSField * value, size_t size) const;
			SOPHIS_EVENT_CORE errno_t SetFields(const CSField * value, size_t size);

		private:
			_STL::vector<CSField> fFields;
		}; // class CSFields
	} // namespace event
} // namespace sophis

EVENT_EPILOG
#endif // _SOPHIS_EVENT_FIELD_H
