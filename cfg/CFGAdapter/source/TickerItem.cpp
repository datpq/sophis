//#include "stdafx.h"
#include "stdio.h"
#include "TickerItem.h"

using namespace std;

CTickerItem::CTickerItem():
	m_Last(0.0),
	m_Ask(0.0),
	m_Bid(0.0),
	m_High(0.0),
	m_Low(0.0),
	m_Open(0.0),
	m_YesterdayLast(0.0),
	m_MonitorID(0),
	m_RequestID(0),
	m_TotalVolume(0),
	m_WhatChanged(0),
	m_Scaling(1.0),
	m_Valid(true),
	m_TickCount(0)
{
	m_TickerName = "Not Initialised";
}

CTickerItem::CTickerItem(string tickerName):
	m_Last(0.0),
	m_Ask(0.0),
	m_Bid(0.0),
	m_High(0.0),
	m_Low(0.0),
	m_Open(0.0),
	m_YesterdayLast(0.0),
	m_MonitorID(0),
	m_RequestID(0),
	m_TotalVolume(0),
	m_WhatChanged(0),
	m_Scaling(1.0),
	m_Valid(true),
	m_TickCount(0)
{
	m_TickerName = tickerName;
}

CTickerItem::~CTickerItem(void)
{
}

void CTickerItem::MakeInvalid()
{
	m_Last = -1.0;
	m_Ask = -1.0;
	m_Bid = -1.0;
	m_High = -1.0;
	m_Low = -1.0;
	m_Open = -1.0;
	m_YesterdayLast = -1.0;
	m_MonitorID = 0;
	m_RequestID = 0;
	m_TotalVolume = -1;
	m_WhatChanged = 0;
	m_Scaling = 1.0;
	m_Valid = false;
	m_TickCount = -1;
}

void CTickerItem::Copy(CTickerItem &ti)
{
	ti.SetLastPrice(m_Last);
	ti.SetAskPrice(m_Ask);
	ti.SetBidPrice(m_Bid);
	ti.SetHighPrice(m_High);
	ti.SetLowPrice(m_Low);
	ti.SetMonitorID(m_MonitorID);
	ti.SetOpenPrice(m_Open);
	ti.SetYesterdayLast(m_YesterdayLast);
	ti.SetRequestID(m_RequestID);
	ti.SetTickerName(m_TickerName);
	ti.SetTotalVolume(m_TotalVolume);
	ti.SetScaling(m_Scaling);
	ti.SetValid(m_Valid);
	ti.SetTickCount(m_TickCount);
}

void CTickerItem::PrintInformation()
{
	char	valid = 'F';

	if (m_Valid)
	{
		valid = 'T';
	}

	printf("%16s L:%f A:%f B:%f M:%f H:%f L:%f O:%f V:%u YL:%f V:%c TC:%d W:%d\n",
		m_TickerName.c_str(), m_Last, m_Ask, m_Bid, GetMidPrice(), m_High,
		m_Low, m_Open, m_TotalVolume, m_YesterdayLast, valid, m_TickCount, m_WhatChanged);
	fflush(stdout);
}

bool CTickerItem::IsEqual(CTickerItem *pTI)
{
	if (pTI->GetAskPrice() == GetAskPrice() &&
		pTI->GetBidPrice() == GetBidPrice() &&
		pTI->GetHighPrice() == GetHighPrice() &&
		pTI->GetLowPrice() == GetLowPrice() &&
		pTI->GetOpenPrice() == GetOpenPrice() &&
		pTI->GetLastPrice() == GetLastPrice() &&
		pTI->GetYesterdayLast() == GetYesterdayLast())
	{
		return true;
	}
	return false;
}

string CTickerItem::GetTickerName()
{
	return m_TickerName;
}

void CTickerItem::SetTickerName(string tickerName)
{
	m_TickerName = tickerName;
}

void CTickerItem::SetAskPrice(double price)
{
	m_Ask = price;
}

double CTickerItem::GetAskPrice()
{
	return m_Ask;
}

void CTickerItem::SetBidPrice(double price)
{
	m_Bid = price;
}

double CTickerItem::GetBidPrice()
{
	return m_Bid;
}

void CTickerItem::SetLastPrice(double price)
{
	m_Last = price;
	if (price < m_Low)
	{
		m_Low = price;
	}
	if (price > m_High)
	{
		m_High = price;
	}
}

double CTickerItem::GetLastPrice()
{
	return m_Last;
}

void CTickerItem::SetYesterdayLast(double price)
{
	m_YesterdayLast = price;
}

double CTickerItem::GetYesterdayLast()
{
	return m_YesterdayLast;
}

void CTickerItem::SetMonitorID(int monitorID)
{
	m_MonitorID = monitorID;
}

int CTickerItem::GetMonitorID()
{
	return m_MonitorID;
}

void CTickerItem::SetRequestID(int requestID)
{
	m_RequestID = requestID;
}

int CTickerItem::GetRequestID()
{
	return m_RequestID;
}

void CTickerItem::SetOpenPrice(double price)
{
	m_Open = price;
}

double CTickerItem::GetOpenPrice()
{
	return m_Open;
}

void CTickerItem::SetHighPrice(double price)
{
	m_High = price;
}

double CTickerItem::GetHighPrice()
{
	return m_High;
}

void CTickerItem::SetLowPrice(double price)
{
	m_Low = price;
}

double CTickerItem::GetLowPrice()
{
	return m_Low;
}

void CTickerItem::SetTotalVolume(int volume)
{
	m_TotalVolume = volume;
}

int CTickerItem::GetTotalVolume()
{
	return m_TotalVolume;
}

void CTickerItem::AddToTotalVolume(int volume)
{
	m_TotalVolume += volume;
}

double CTickerItem::GetMidPrice()
{
	return (m_Bid + m_Ask)/2;
}

void CTickerItem::SetWhatChanged(int change)
{
	m_WhatChanged = change;
}

int CTickerItem::GetWhatChanged()
{
	return m_WhatChanged;
}

void CTickerItem::SetScaling(double scaling)
{
	m_Scaling = scaling;
}

double CTickerItem::GetScaling()
{
	return m_Scaling;
}

void CTickerItem::SetValid(bool bValid)
{
	m_Valid = bValid;
}

bool CTickerItem::IsValid()
{
	return m_Valid;
}

bool CTickerItem::IsVolumeChange()
{
	return (m_WhatChanged & CHANGE_VOLUME) != 0;
}

bool CTickerItem::IsBidChange()
{
	return (m_WhatChanged & CHANGE_BID) != 0;
}

bool CTickerItem::IsAskChange()
{
	return (m_WhatChanged & CHANGE_ASK) != 0;
}

bool CTickerItem::IsHighChange()
{
	return (m_WhatChanged & CHANGE_HIGH) != 0;
}

bool CTickerItem::IsLowChange()
{
	return (m_WhatChanged & CHANGE_LOW) != 0;
}

bool CTickerItem::IsLastChange()
{
	return (m_WhatChanged & CHANGE_LAST) != 0;
}

bool CTickerItem::IsFirstValue()
{
	return (m_WhatChanged & CHANGE_FIRST_VALUE) != 0;
}

void CTickerItem::IncrementTickCount()
{
	m_TickCount++;
}

void CTickerItem::SetTickCount(int TickCount)
{
	m_TickCount = TickCount;
}

int CTickerItem::GetTickCount()
{
	return m_TickCount;
}
