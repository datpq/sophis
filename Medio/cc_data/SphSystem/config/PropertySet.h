#if (defined(WIN32)||defined(_WIN64))
#	pragma once // speed up VC++ compilation
#endif

/**
* ----------------------------------------------------------------------------
* File : PropertySet.h
* Creation : Sep 25 2007 by RL
* Description :
* 
* ----------------------------------------------------------------------------
*/
#ifndef __SPHSYSTEM_CONFIG_PROPERTYSET_H__
#define __SPHSYSTEM_CONFIG_PROPERTYSET_H__

/*
** System includes
*/
#include "SphTools/base/CommonOS.h"
#include "SphTools/base/RefCountHandle.h"
#include __STL_INCLUDE_PATH(string)

/*
** Application includes
*/
#include "SphSystem/SphSystemExport.h"

// ------------------------------------------------------------------

#if (defined(WIN32)||defined(_WIN64))
#	pragma warning(push)
#	pragma warning(disable:4275) // Can not export a class derivated from a non exported one
#	pragma warning(disable:4251) // Can not export a class agregating a non exported one
#	pragma pack(push,8)
#else
#	pragma pack(8)
#endif

// ------------------------------------------------------------------

namespace sphSystem
{
	namespace config
	{
		class Property;
	}
}

// ------------------------------------------------------------------

namespace sphSystem
{
	namespace config
	{
		/**
		 * Property Set utility class.
		 */
		class SPHSYSTEM_API PropertySet
		{
		public:
			
			PropertySet();

			PropertySet(const PropertySet& in_other);

			PropertySet& operator=(const PropertySet& in_other);

			~PropertySet();

		public:

			size_t getSize() const;

			Property* operator[](size_t in_index);

			const Property* operator[](size_t in_index) const;

		public:

			bool isDefined(const _STL::string& in_propertyName) const;

			Property* getProperty(const _STL::string& in_propertyName);

			const Property* getProperty(const _STL::string& in_propertyName) const;

		public:

			bool addProperty(const Property& in_property, bool in_overwrite = false);

			void merge(const PropertySet& in_other);

			void merge(const PropertySet& in_other, bool in_recurse);

			void removeProperty(const _STL::string& in_propertyName);

		public:

			void dump(_STL::ostream& in_ostr) const;

		private:

			struct                                          Impl;
			typedef sophisTools::base::RefCountHandle<Impl> ImplH;
			ImplH m_impl;
		};

		inline static _STL::ostream& operator<<(_STL::ostream& in_ostr, const PropertySet& in_propertySet) {
			in_propertySet.dump(in_ostr);
			return in_ostr;
		}
	}
}

// ------------------------------------------------------------------

#if (defined(WIN32)||defined(_WIN64))
#	pragma warning(pop)
#	pragma pack(pop)
#else
#	pragma pack()
#endif

// ------------------------------------------------------------------

#endif // __PROPERTYSET_H__
