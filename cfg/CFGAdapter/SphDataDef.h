/********************************************************************
*	@date:		08.05.2009 
*	
*	@file:	 	DataDef.h
*
*	@author		:	
*					Copyright (C) 2004 SOPHIS
*	
*	@purpose:	
*
*/

#pragma once

#ifndef _DataDef_H_
#define _DataDef_H_

#pragma message("WARNING : Use local copy of SphDataDef.h if before DRT 6.1.0.3")


/**
*	Includes
*/
#include "SphTools/SphCommon.h"
#include "SphTools/UnsafeRefCount.h"
#include "SphTools/RefCountHandle.h"

#include __STL_INCLUDE_PATH(iosfwd)
#include __STL_INCLUDE_PATH(string)
#include __STL_INCLUDE_PATH(map)
#include __STL_INCLUDE_PATH(vector)
#include __STL_INCLUDE_PATH(set)


//typename redefine
namespace{
	typedef __int32			Tag_t;
	typedef double			DateTime_t;
	typedef __int64			Date_t;
	typedef __int64			Time_t;
	typedef __int64			Long_t;
	typedef double			Real_t;
	typedef _STL::string	String_t;
}

namespace sophis{
	namespace quotesource{
		namespace data{
			
			/** TopicKey
			@member type 
			type of key. Can be RIC name or CUSIP code
			@member value
			string value of key

			@version 6.1.0.0
			*/
			struct TopicKey_t{
				struct KeyType{
					enum Value{
						RIC = 0,
						CUSIP,
						LAST
					};
				};
				TopicKey_t (KeyType::Value in_type, const String_t & in_value) : type(in_type), value(in_value) {}
				TopicKey_t (const String_t & in_value) : type(KeyType::RIC), value(in_value) {}
				TopicKey_t () : type(KeyType::RIC), value("") {}

				inline const String_t& toString() const {return value;}
				inline operator const String_t&() const {return value;}

				//for collection
				inline bool operator<(const sophis::quotesource::data::TopicKey_t& topicKey) const{
					if(this->type < topicKey.type) return true;
					else if(this->value < topicKey.value) return true;
					else return false;
				}
				inline bool operator==(const sophis::quotesource::data::TopicKey_t& topicKey) const{
					return this->type == topicKey.type && this->value == topicKey.value;
				}
				inline bool operator!=(const sophis::quotesource::data::TopicKey_t& topicKey) const{
					return this->type != topicKey.type || this->value != topicKey.value;
				}

				KeyType::Value	type;	// reference kind : RIC, ticker, CUSIP, SEDOL, etc..
				String_t	value;	// Value of the reference : ACCP.PA, AC FP Equity, ...
			};

			/** FieldDataType
			@enum member LONG	  field value is a long integer. storage type is __int64
			@enum member DOUBLE   field value is a double float. srorage type is double
			@enum member STRING   field value is a string. storage type is string
			@enum member DATETIME field value is a datetime. storage type is double
			@enum member DATE     field value is a date. storage type is __int64,
								  calculated from number of days from 01/01/1904
		    @enum member TIME     field value is a time. storage type is __int64, 
								  calculated from number of seconds from midnight

			@version 6.1.0.0
			*/
			struct FieldDataType{
				enum Value{
					LONG = 0,
					DOUBLE,
					STRING,
					DATETIME,
					DATE,
					TIME
				};
			};

			/** DictionaryType
			@enum member STREAMING	streaming field dictionary
			@enum member SNAPSHOT   snapshot field dictionary

			@version 6.1.0.0
			*/
			struct DictionaryType{
				enum Value{
					STREAMING = 0,
					SNAPSHOT
				};
			};

			const String_t DictionaryTypeStr[2] = {
				"STREAMING",
				"SNAPSHOT"
			};

			/** Property
			@member name 
			property name
			@member value
			property value

			@version 6.1.0.0
			*/
			struct Property{
				Property(const String_t& n, const String_t& v):name(n), value(v){}
				Property(const Property& src){
					this->name = src.name;
					this->value = src.value;
				}

				String_t name;
				String_t value;
			};

			/** List of properties	
			@version 6.1.0.0
			*/
			typedef _STL::vector<Property> PropertyList;

			/** SnapshotRequestType
			@enum member EOD_REQUEST	eod request
			@enum member HISTORIC_REQUEST   historic request

			@version 6.1.0.0
			*/
			struct SnapshotRequestType{
				enum Value{
					EOD_REQUEST = 0,
					HISTORIC_REQUEST
				};
			};

			/** SnapshotRequest
			@member m_StartDate 
			Snapshot interval start date
			@member m_EndDate
			Snapshot interval end date
			@member m_Topics
			Topics to snapshot

			@version 6.1.0.0
			*/
			struct SnapshotRequest{
				SnapshotRequestType::Value m_Type;
				Date_t	m_StartDate;
				Date_t  m_EndDate;
				_STL::set<TopicKey_t> m_Topics;
			};

			/** SnapshotError
			@member closure 
			Correlation id of snapshot
			@member date
			Snapshot date on which error is reported
			@member topicKey
			Key of topic
			@member reason
			error message

			@version 6.1.0.0
			*/
			struct SnapshotError{
				enum SnapErrorType
				{
					SOURCE_ERROR,
					SUB_ERROR
				};

				Long_t closure;
				Date_t date;
				TopicKey_t topicKey;
				String_t reason;
				SnapErrorType errortype;
				String_t errorcode;
			};
			typedef _STL::vector<SnapshotError> SnapshotErrorQ;

			/** List of client id
			@version 6.1.0.0
			*/
			typedef _STL::vector<String_t> ClientIdList;

			/** Adapter admin event
			@member consoleid
			the console id associated to the event. 0 means all connected consoles(a broadcast event), other values mean a specific console.
			@member eventid
			user defined id of event. 
			@member data
			buffer which contains the serialized event data
			@version 6.1.0.0
			*/
			struct AdapterAdminEventEx{
				Long_t consoleid;
				Long_t eventid;
				String_t data;
			};
		}
	}
}

#endif //_DataDef_H_