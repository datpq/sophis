#if (defined(WIN32)||defined(_WIN64))
#	pragma once
#endif

/*
 	File:		SphPrototype.h

 	Contains:	Class for the handling of a generic prototype.

 	Copyright:	© 2002 Sophis.
*/

#ifndef _SphPrototype_H_
#define _SphPrototype_H_

#include "../cc_data/SphCommon.h"
#include "../cc_data/SphPrototype_fwd.h"

/**
 * Removes the StlPort warning regarding #pragma alignment change
 * because the alignment is correctly put back in _epilog.h.
 * So, this warning may be ignored.
 */
#if (defined(WIN32)||defined(_WIN64))
#	pragma warning (push)
#	pragma warning (disable : 4786)
#	pragma warning (disable : 4900)
#	pragma warning (disable : 4290)
#endif
	
#include __STL_INCLUDE_PATH(map)
#include __STL_INCLUDE_PATH(iostream)
#include __STL_INCLUDE_PATH(strstream)

#if (defined(WIN32)||defined(_WIN64))
#	pragma warning(pop) // used #pragma pack to change alignment
#endif

#include "SphTools/SphExceptions.h"
#include "SphTools/RefCountHandle.h"


#if (defined(WIN32)||defined(_WIN64))
#	pragma warning(push)
#	pragma warning(disable:4786) // '...' : identifier was truncated to '255' characters in the debug information
#	pragma warning (disable : 4290)
#endif

class CSUMenu;
class CSRColleUtilisateur_v1;

namespace sophis {
	namespace tools	{

		/** Template for managing the dynamic creation of class.
		*	This is a map containing the key (generally a name) and an instance of the 
		*	derived class of X
		*	The class X is supposed to contain a Clone method.
		*	This feature is normally used to create a singleton (see <a href="classCSRVolatilityAction.html">CSRVolatilityAction</a> for example), for each class X called by a static method GetPrototype.
		*/
		template <class X, class Key , class lower, class _A>
		class CSRPrototype : public _STL::map<Key, X *, lower, _A>
		{
		public:
			typedef typename _STL::map<Key, X *, lower, _A>::iterator iterator;
			typedef typename _STL::map<Key, X *, lower, _A>::const_iterator const_iterator;


			/** No model found in the map. Exception thrown when asking for a model not included in the map. The string contains the name of X, as well as the key.
			*/
			struct	ExceptionNotFound : public sophisTools::base::ExceptionBase
			{
				ExceptionNotFound(const Key &k)
					: ExceptionBase("sophis::tools::CSRPrototype", NULL)
				{
					_k = k;
					_STL::ostrstream oss;
					oss << "key " << k << " not found on prototype " << typeid(X).name() << _STL::ends;

					getError() = (const char *) oss.str();
				    oss.rdbuf()->freeze(false);
				}
				Key  _k;
			};

			/** Model already included in the map. Exception thrown when inserting a model already included in the map. The string contains the name of X, as well as the key.
			*/
			struct	ExceptionFound : public sophisTools::base::ExceptionBase
			{
				ExceptionFound(const Key &k)
					: ExceptionBase("sophis::tools::CSRPrototype", NULL)
				{
					_k = k;
					_STL::ostrstream oss;
					oss << "key " << k << " key found on prototype " << typeid(X).name() << _STL::ends;

					getError() = (const char *) oss.str();
				    oss.rdbuf()->freeze(false);
				}
				Key  _k;
			};

			/** Trivial constructor.*/
			CSRPrototype()
			{
			}

			/** Copy constructor.
			*	This is rarely used - it is included here for thoroughness.
			*	The instance must be cloned because it is deleted in CSRPrototype::Clear().
			*/
			CSRPrototype(const CSRPrototype<X, Key , lower, _A> &prototype)
			{
				iterator p;

				for(p = begin(); p!= end(); ++p)
					p->second = p->second->Clone();
			}

			/** Destructor.
			*	Deletes all of the instances - see CSRPrototype::Clear.
			*/
			~CSRPrototype()
			{
				Clear();
			}

			/** Clear in the std sense.
			*	Overloaded because we also need to delete the created instance
			*/
			void	Clear()
			{
				iterator	p;

				for(p = begin(); p != end(); ++p)
					delete p->second;
				clear();
			}

			/** Create an instance of X.
			*	To create the instance, it assumes a Clone method exists in the class X
			*
			*	@params k is the key to find the instance.
			*	@params withException decide if the method throws or return NULL if not found
			*	
			*	@throws Exception if not found.
			*/
			X * CreateInstance(const Key & k, bool withException=true) const
				throw (ExceptionNotFound)
			{
				const_iterator p = find(k);

				if(p == end()) {
					if (!withException) return NULL;
					throw ExceptionNotFound (k);
				}
				return dynamic_cast_ex<X*>(p->second->Clone());
			}

			/** Create an instance of X.
			*	To create the instance, it assumes a Clone method exists in the class X
			*	This version is useful when the key is a camelised string
			*	@params k is the key to find the instance.
			*	@params equal is the operator of compare.
			*	@params withException decide if the method throws or return NULL if not found
			*	
			*	@throws Exception if not found.
			*	@since 5.3
			*/
			template <class Y>
			X * CreateInstance(const Key & k, Y &equal, bool withException=true) const
				throw (ExceptionNotFound)
			{
				const_iterator p ;
				
				for(p = begin(); p!= end(); ++p)
				{
					if(equal(p->first, k))
						return dynamic_cast_ex<X*>(p->second->Clone());

				}
				if (!withException) return NULL;
				throw ExceptionNotFound (k);
			}


			/** Give the original instance.
			*	The main difference with 'find' is that an exception is thrown if data is not found
			*	
			*	@params k is the key to find the instance.
			*	@params withException decide if the method throws or return NULL if not found
			*
			*	@throws ExceptionNotFound exception if not found.
			*/
			const X * GetData(const Key & k, bool withException=true) const
				throw (ExceptionNotFound)
			{
				const_iterator p = find(k);

				if(p == end()) {
					if (!withException) return NULL;
					throw ExceptionNotFound (k);
				}
				return p->second;
			}

			/** Give the original key for the instance.
			*	This is the reciprocal method of CSRPrototype::GetData.
			*	
			*	@param Original is the original instance to find the key.
			*
			*	@throws sophisTools::base::IndexOutOfBoundsException exception if not found.
			*/
			Key GetKey(const X * original) const
			{
				const_iterator	p;

				for(p = begin(); p!= end(); ++p)
					if(p->second == original)
						return p->first;
				throw sophisTools::base::IndexOutOfBoundsException (-1, 0, (long)size(), "Prototype::GetKey ");
			}

			/** Insert a new member in the prototype.
			*	The insert is overloaded in order to check the map. In this way, we can avoid having
			*	two models with the same name.
			*
			*	@param Key is the key of the clone to be able to find it back.
			*	@param Clone is the original instance used to dynamically create an instance.
			*
			*	@throws ExceptionFound exception if the key is already present.
			*/
			void insert(const Key & k, X * clone)
			{
				iterator p = find(k);

				if(p != end() && !gDoNotThrowExceptionIfAlreadyExist)
					throw ExceptionFound (k);
				(*this)[k] = clone;
			}

			/** Find the position in the map with a key.
			*	Even if it is not optimized \f$n log n\f$, the prototype
			*	contains only a few instances, so it is not an issue.
			*	Moreover, the prototype is populated when the software is run in the UNIVERSAL_MAIN f the dll's, so the position is stable in	an execution.
			*
			*	@param Key is the key of the clone
			*	@params withException decide if the method throws if key not found
			*
			*	@throws ExceptionNotFound if key not found & withException is true
			*
			*	@return The position in the map from 0 to size()-1
			*			-1 when the key is not found & withException is false
			*/
			int	GetIndex(const Key & k, bool withException=true) const
			{
				const_iterator  p = find(k), iter;
				if(p == end()) {
					if(!withException) return -1;
					throw ExceptionNotFound (k);
				}
				int	index = 0;
				for (iter = begin(); iter != end(); ++iter)
				{
					if(iter == p)
						break;
					++index;
				}
				return index;
			}

			/** Find the position in the map with a class.
			*	This is a templated method.
			*	Even if it is not optimized \f$n log n\f$ where dynamic
			*	is the time for a dynamic cast, the prototype
			*	contains only a few instances so it is not an issue.
			*	Moreover, the prototype is populated when the software is run
			*	in the UNIVERSAL_MAIN f the dll's, so the position is stable in
			*	an execution.
			*
			*	@param Dummy is included here simply to build the correct method
			*	@param withException decide if the method throws if the instance not found
			*	
			*	@throws ExceptionBase with the name of GetInstance
			*	
			*	@return The position in the map from 0 to size()-1
			*			-1 when no object of the given type can't be found
			*/
			template <class Y>
			int	GetIndexByClass(Y * dummy = 0, bool withException=true)
			{
				const_iterator  iter;
				int	index = 0;
				for (iter = begin(); iter != end(); ++iter)
				{
					if(typeid(*iter->second)==typeid(Y))
						return index;
					++index;
				}
				if(!withException) return -1;
				_STL::ostrstream	oss;
				_STL::string	errorStr;
				oss << "sophis::tools::CSRPrototype::GetIndexByClass " << typeid(Y).name() << "not found on prototype " << typeid(X).name() << _STL::ends;
				errorStr = (const char *) oss.str();
				oss.rdbuf()->freeze(false);
				throw sophisTools::base::GeneralException( errorStr.c_str());
			}

			/** Find the iterator in the map with a class.
			*	
			*	This is a templated method
			*	Even if it is not optimized \f$n log n\dynamic\f$ where dynamic
			*	is the time for a dynamic cast, the prototype
			*	contains only a few instances so it is not an issue.
			*	Moreover, the prototype is populated when the software is run
			*	in the UNIVERSAL_MAIN f the dll's, so the position is stable in
			*	an execution.
			*
			*	@param Dummy is just here to build the right method
			*	@param withException decide if the method throws if the instance not found
			*
			*	@throws ExceptionBase with the name of GetInstance
			*
			*	@return the iterator referencing the object, end() 
			*			in case no object of the given type is found
			*/
			template <class Y>
			const_iterator	GetIteratorByClass(Y * dummy = 0,  bool withException=true)
			{
				const_iterator  iter;
				for (iter = begin(); iter != end(); ++iter)
				{
					if(typeid(*iter->second)==typeid(Y))
						return iter;
				}
				if(!withException) return end();
				_STL::ostrstream	oss;
				_STL::string	errorStr;
				oss << "sophis::tools::CSRPrototype::GetIteratorByClass " << typeid(Y).name() << "not found on prototype " << typeid(X).name() << _STL::ends;
				errorStr = (const char *) oss.str();
				oss.rdbuf()->freeze(false);
				throw sophisTools::base::GeneralException( errorStr.c_str());
			}

			/** Find the iterator in the map with an instance.
			*	
			*	This is a templated method
			*	Even if it is not optimized \f$n log n\dynamic\f$ where dynamic
			*	is the time for a dynamic cast, the prototype
			*	contains only a few instances so it is not an issue.
			*	Moreover, the prototype is populated when the software is run
			*	in the UNIVERSAL_MAIN f the dll's, so the position is stable in
			*	an execution.
			*
			*	@return the iterator on the good generator; end() if not found.
			*	@since 5.3
			*/
			template <class Y>
			const_iterator	GetIteratorByInstance(const Y & instance)
			{
				const_iterator  iter;
				for (iter = begin(); iter != end(); ++iter)
				{
					if(typeid(*iter->second)==typeid(instance))
						return iter;
				}
				return end();
			}

			/** Find the const_iterator in the map with its position.
			*
			*	This is the same as vector::operator[].
			*	Even if it is not optimized \f$n log n\dynamic\f$ where dynamic
			*	is the time for a dynamic cast, the prototype
			*	contains only a few instances so it is not an issue.
			*
			*	@param Index is the position from 0 to n-1
			*	@param withException decide if the methods throws if the index is not good.
			*
			*	@throws IndexOutOfBoundsException if the index is not good (withException = true)
			*
			*	@return the const_iterator
			*			end() if not found (withException = false)
			*/
			const_iterator	GetNth(int index, bool withException=true) const
			{
				const_iterator  p;
				int	i = index;
				for(p=begin(); p!= end(); ++p)
				{
					if(i-- == 0)
						return p;
				}
				if(!withException) return end();
				throw sophisTools::base::IndexOutOfBoundsException (index, 0, (long)size(), "Prototype::GetNth ");
			}

			/** Find the original instance in the map with its position.
			*	
			*	This is the same as GetNth except that it does not send an exception.
			*	Even if it is not optimized \f$n log n\dynamic\f$ where dynamic
			*	is the time for a dynamic cast, the prototype
			*	contains only a few instances so it is not an issue.
			*	
			*	@param Index is the position from 0 to n-1
			*	
			*	@return The original instance or NULL if not found
			*/
			X *	GetNthCached(int index) const
			{
				const_iterator  p;
				int	i = index;
				for(p=begin(); p!= end(); ++p)
				{
					if(i-- == 0)
						return p->second;
				}
				return NULL;
			}

			/** Create a popup menu containing all the names of the prototype.
			*	
			*	This is for internal use.
			*	The key must be a string and you need to link with SphRiskTools.
			*/
			CSUMenu	*new_Menu()	const
			{
				CSUMenu	* ret;
				const_iterator  p;

				ret = ::NewMenu("");

				for(p=begin(); p!= end(); ++p)
					ret->Append(p->first);
				return ret;
			}
		};

		/** Template to manage the dynamic creation of a class (with constructor) with one parameter.
		*	
		*	An example of its use is the class <a href="classCSRExtraction.html">CSRExtraction</a>.
		*	It is a map containing the key (generally a name) and an instance of the derived class of X
		*	The class X is supposed to contain a Create method. Calling new X(Param &param)
		*	Param is supposed to be the parameter of the constructor.
		*/
		template  <class X, class Key , class Param, class lower = _STL::less <Key>, class _A = _STL::allocator<X*> >
		class CSRPrototypeParameter : public  CSRPrototype<X, Key, lower, _A>
		{
		public:
			/** Dynamically create an instance of the class, named by key with parameter params.
			*
			*	@params k is the key to find the instance
			*	@params withException decide if the methods throws if not found
			*	@throws Exception if not found (withException=true)
			*	Call virtual X::Create if found
			*   @return Null if not found (withException=false)
			*/
			X * CreateInstance(const Key & k, Param param, bool withException=true)
				throw (typename sophis::tools::CSRPrototype< X, Key>::ExceptionNotFound)
			{
				iterator p = find(k);

				if(p == end()) {
					if(!withException) return NULL;
					throw CSRPrototype< X, Key>::ExceptionNotFound (k);
				}
				return p->second->Create(param);
			}

			/** Create an instance of X.
			*	This version is useful when the key is a camelised string
			*	@params k is the key to find the instance.
			*	@params equal is the operator of compare.
			*	@params withException decide if the methods throws if not found
			*	
			*	@throws Exception if not found. (withException=true)
			*   @return Null if not found (withException=false)
			*	@since 5.3
			*/
			template <class Y>
			X * CreateInstance(const Key & k, Param param, Y &equal, bool withException=true) const
				throw (ExceptionNotFound)
			{
				const_iterator p ;
				
				for(p = begin(); p!= end(); ++p)
				{
					if(equal(p->first, k))
						return p->second->Create(param);
				}
				if(!withException) return NULL;
				throw CSRPrototype< X, Key>::ExceptionNotFound (k);
			}
		};


		/** Template for managing dynamic creation of class (with constructor) with two parameters.
		*	An example of use can be found in AMInStream.h
		*	It is a map containing the key (generally a name) and an instance of the derived class of X
		*	The class X is supposed to contain a Create method. Calling new X(Param::first_type, Param::second_type)
		*	Param is supposed to be a _STL::pair
		*/
		template  <class X, class Key , class Param, class lower = _STL::less <Key>, class _A = _STL::allocator<X*> >
		class CSRPrototypePairParameter : public  CSRPrototype<X, Key, lower, _A>
		{
		public:
			/** Dynamically creates an instance of the class, naming by key, with parameters t1 and t2.
			*	@params t1 is the first type of the pair.
			*	@params t2 is the second type of the pair.
			*	@throws Exception if not found
			*	Call virtual X::Create if found
			*/
			X * CreateInstance(const Key & k, typename Param::first_type t1, typename Param::second_type t2, bool withException=true)
				throw (typename sophis::tools::CSRPrototype< X, Key>::ExceptionNotFound)
			{
				iterator p = find(k);

				if(p == end()) {
					if(!withException) return NULL;
					throw CSRPrototype< X, Key>::ExceptionNotFound (k);
				}
				return p->second->Create(t1, t2);
			}

			/** Dynamically creates an instance of the class, naming by key, with parameters t1 and t2.
			*
			*	@params t1 is the first type of the pair.
			*	@params t1 is designed to be a va_list.
			*	@throws Exception if not found
			*	Call virtual X::Create if found
			*/
			X * CreateInstance(const Key & k, typename Param::first_type t1, ...)
				throw (typename sophis::tools::CSRPrototype<X,Key>::ExceptionNotFound)
			{
				va_list	ag;
				va_start(ag,t1);
				X * x = CreateInstance(k, t1, ag);
				va_end(ag);
				return x;
			}
		};

#	ifdef USE_OLD_PROTOTYPE_INSTALL

		/**
		 * @obsolete from 4.5.1
		 */
		typedef const char* const_char_ptr_t; // unix port, template parameters can not be with spaces
		template  <class _Parent, class _Derivate, class _Key = const_char_ptr_t >
		struct install
		{
			install(const _Key & k)
			{
				_Derivate *clone = new _Derivate;
				_Parent::GetPrototype().insert(k,clone);
			}
		};
#	else
		template  <class _Derivate>
		struct install
		{
			install(const typename _Derivate::prototype::key_type & k)
			{
				_Derivate *clone = new _Derivate;
				_Derivate::GetPrototype().insert(k,clone);
			}
		};
#	endif

		/** Prototype with two key whose second one is a lonf.
		It is to optimise the search for id.
		X is supposed to have a method called GetId() to get the second key.
		@since 4.5.1.0.15
		*/
		template <class X, class Key , class lower = _STL::less <Key>, class _A = _STL::allocator<X*> >
		class CSRPrototypeWithId : public CSRPrototype<X, Key, lower, _A>
		{
		public:
			
			/** Get an instance by Id.
			Loop on the prototype. The algorith is in O(n log n) when n is the size of the column.
			@param id is the id created by the sopfware once for all.
			@return a pointer which may be null and must not be deleted.
			*/
			const X * GetInstance(long id) const
			{
				if(fMapById.size() != size())
				{
					fMapById.clear();
					const_iterator scan;
					for(scan = begin(); scan != end(); ++scan)
						fMapById[scan->second->GetId()] = scan->second;

				}
				map_by_id::const_iterator ite = fMapById.find(id);
				if(ite != fMapById.end())
					return ite->second;
				else
					return NULL;
			}

		protected:
			typedef _STL::map<long, const X *> map_by_id;
			mutable map_by_id	fMapById;
		};

		/** Internal Use.
		*	This is a global, for avoiding an exception being sent when the API is unloaded and then loaded.
		*/
		extern bool	SPHTOOLS_API gDoNotThrowExceptionIfAlreadyExist;
	}
}

#if (defined(WIN32)||defined(_WIN64))
#	pragma warning(pop)
#endif

#endif
