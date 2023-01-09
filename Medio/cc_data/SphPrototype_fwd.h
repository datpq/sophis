#if (defined(WIN32)||defined(_WIN64))
#	pragma once // speed up VC++ compilation
#endif

/**
 * 
 * @author : OP
 * @date : 2005/04/05
 */

#ifndef __SOPHIS_SPHPROTOTYPE_FWD_H_INCLUDED__
#define __SOPHIS_SPHPROTOTYPE_FWD_H_INCLUDED__

#include "../cc_data/SphCommon.h"

#include __STL_INCLUDE_PATH(map)

namespace sophis 
{
	namespace tools	
	{

		/** 
		 * Forward declaration of generic (template) prototype class
		 */
		template <class X, class Key , class lower = _STL::less <Key>, class _A = _STL::allocator<X*> >
			class CSRPrototype;
	}
}

#endif // __SOPHIS_SPHPROTOTYPE_FWD_H_INCLUDED__
