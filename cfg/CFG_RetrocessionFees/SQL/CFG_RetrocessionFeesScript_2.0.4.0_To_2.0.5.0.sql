--{{SOPHIS_SQL (do not delete this line)

alter table CFG_RETRO_FEES_PARAMETERS add (LEVEL_4 NUMBER, RETROCESSION_RATE_4 NUMBER);

alter table CFG_RETRO_FEES_RESULTS add (AVERAGE_ASSET NUMBER, NB_DAYS NUMBER(10,0), RETRO_RATE NUMBER, COMMISSION_RATE NUMBER);

create table CFG_RETRO_FEES_DETAILS
(
  FUND_ID                     NUMBER(10,0),
  BUSINESS_PARTNER_ID         NUMBER(10,0),
  BUSINESS_PARTNER_TYPE       NUMBER(2,0),
  COMPUTATION_METHOD          NUMBER(2,0),  
  START_DATE                  DATE,
  END_DATE                    DATE,
  NAV_DATE                    DATE,
  NB_SHARES_BUSINESS_PARTNER  NUMBER,
  NB_SHARES_TOTAL             NUMBER,
  NB_DAYS                     NUMBER(10,0),
  NAV                         NUMBER,
  FDG                         NUMBER,
  CDVM                        NUMBER,
  DDG                        NUMBER,
  MCL                        NUMBER,
  FUND_PROMOTER_RETROCESSION NUMBER,
  PNB                        NUMBER,  
  CONSTRAINT PK_CFG_RETRO_FEES_DETAILS PRIMARY KEY (FUND_ID,BUSINESS_PARTNER_ID,BUSINESS_PARTNER_TYPE,COMPUTATION_METHOD,START_DATE,END_DATE,NAV_DATE)
);

--}}SOPHIS_SQL