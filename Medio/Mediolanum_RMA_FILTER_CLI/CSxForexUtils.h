#pragma once
class CSxForexUtils
{

public:
	static bool IsFXPairReversed(long leftCcy, long rightCcy);
	static long GetFWDInstrCode(long leftCcy, long rightCcy, long date);
	static long GetNDFInstrCode(long leftCcy, long rightCcy, long date);

private:
	CSxForexUtils(void);
	~CSxForexUtils(void);

	static const char * __CLASS__;
};

