#pragma once
#include <string>

using namespace _STL;

#define	CHANGE_ASK			(1)
#define	CHANGE_BID			(2)
#define	CHANGE_HIGH			(4)
#define	CHANGE_LOW			(8)
#define	CHANGE_VOLUME		(16)	
#define	CHANGE_LAST			(32)	
#define CHANGE_ALL			(1 + 2 + 4 + 8 + 16 + 32)
#define CHANGE_FIRST_VALUE	(128)

class CTickerItem
{
private:
	string	m_TickerName;
	double	m_Ask;
	double	m_Bid;
	double	m_Last;
	double	m_Open;
	double	m_High;
	double	m_Low;
	double	m_YesterdayLast;
	int		m_TotalVolume;
	int		m_MonitorID;
	int		m_RequestID;
	int		m_WhatChanged;
	double	m_Scaling;
	bool	m_Valid;
	int		m_TickCount;
public:
	CTickerItem();
	CTickerItem(string tickerName);
	~CTickerItem(void);

	void	PrintInformation();

	void	MakeInvalid();

	void	Copy(CTickerItem &ti);
	bool	IsEqual(CTickerItem *pTI);

	string	GetTickerName();
	void	SetTickerName(string tickerName);
	void	SetAskPrice(double price);
	double	GetAskPrice();
	void	SetBidPrice(double price);
	double	GetBidPrice();
	void	SetLastPrice(double price);
	double	GetLastPrice();
	void	SetMonitorID(int monitorID);
	int		GetMonitorID();
	void	SetRequestID(int requestID);
	int		GetRequestID();
	void	SetOpenPrice(double price);
	double	GetOpenPrice();
	void	SetHighPrice(double price);
	double	GetHighPrice();
	void	SetLowPrice(double price);
	double	GetLowPrice();
	void	SetYesterdayLast(double price);
	double	GetYesterdayLast();
	double	GetMidPrice();

	void	SetTotalVolume(int volume);
	int		GetTotalVolume();
	void	AddToTotalVolume(int volume);
	void	SetWhatChanged(int change);
	int		GetWhatChanged();
	void	SetScaling(double scaling);
	double	GetScaling();
	void	SetValid(bool bValid);
	bool	IsValid();
	bool	IsVolumeChange();
	bool	IsBidChange();
	bool	IsAskChange();
	bool	IsHighChange();
	bool	IsLowChange();
	bool	IsLastChange();
	bool	IsFirstValue();
	void	IncrementTickCount();
	void	SetTickCount(int TickCount);
	int		GetTickCount();
};
