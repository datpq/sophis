--{{SOPHIS_SQL (do not delete this line)
CREATE TABLE FUND_FEES_CFGMANAGEMENT
  ( ID                     NUMBER(10,0),
    RATE                   NUMBER,
    TIME_BASIS             NUMBER(2,0),
    CALC_TYPE              NUMBER(2,0),
    ACCOUNTING_PERIODICITY NUMBER(10,0),
    ACCOUNTING_ARREARS     NUMBER(1,0),
    PAYMENT_PERIODICITY    NUMBER(10,0),
    PAYMENT_ARREARS        NUMBER(1,0),
    NAVTYPE                NUMBER(2,0),
    RATE_RANGE_MIN         NUMBER(14,0),
    RATE_RANGE_MAX         NUMBER(14,0),
    AMOUNT_TYPE            NUMBER(2,0) DEFAULT 1,
    PTFCOL_NAME            VARCHAR2(40 BYTE),
    CONSTRAINT PK_FUND_FEES_CFGMANAGEMENT PRIMARY KEY (ID)
  );
  
CREATE SEQUENCE SEQ_FUND_FEES_CFGMANAGEMENT START WITH 1 INCREMENT BY 1;

CREATE TABLE CFGMANAGEMENT_FEES_BY_LEVEL
  (	ID NUMBER(10,0), 
	  CFG_LEVEL NUMBER,
	  RATE NUMBER(16,6),
	  CONSTRAINT CFGMANAGEMENT_FEES_BY_LEVEL_PK PRIMARY KEY (ID,CFG_LEVEL),
	  CONSTRAINT CFGMANAGEMENT_FEES_BY_LEVEL_FK FOREIGN KEY(ID) REFERENCES FUND_FEES_CFGMANAGEMENT(ID) ON DELETE CASCADE
  );
  
insert into CFGMANAGEMENT_FEES_BY_LEVEL (select ID,0,RATE from FUND_FEES_CFGMANAGEMENT);

commit;  
       
--}}SOPHIS_SQL