--{{SOPHIS_SQL (do not delete this line)

drop table MEDIO_AUDIT_HEDGING_POSITION;

begin
TK_DROP_TITRES_COLUMN('MEDIO_VOL_STRIKE');
TK_DROP_TITRES_COLUMN('MEDIO_VEGA_NOTIONAL');
TK_DROP_TITRES_COLUMN('MEDIO_OUTSIDE_HRS');
TK_DROP_TITRES_COLUMN('MEDIO_OUTSIDE_HRS_LOG');
TK_DROP_HISTOMVTS_COLUMN('MEDIO_MARKET_TIMEZONE')
TK_DROP_HISTOMVTS_COLUMN('MEDIO_PIT_CLOSING')
TK_DROP_HISTOMVTS_COLUMN('MEDIO_PIT_OPENING')
TK_DROP_HISTOMVTS_COLUMN('TKT_RBC_TRADE_ID')
TK_DROP_HISTOMVTS_COLUMN('MEDIO_PARENTORDERID')
end;	

DELETE FROM ORDER_PROPERTY WHERE NAME = 'MEDIO_OUTSIDE_HRS' or NAME = 'MEDIO_PARENTORDERID';
DELETE FROM EXEC_PROPERTY WHERE NAME =  'MEDIO_OUTSIDE_HRS' or NAME = 'MEDIO_PARENTORDERID' or NAME = 'MEDIO_IsHedgedFunded';
DELETE FROM SWAP_POS_REFS_DEF WHERE NAME = 'RBC_REPORTING_BE' or NAME = 'RBC_REPORTING_BE_EXCEPTION';

-- Clause builder 
alter table GB_MASK_DATA_AUTOCALL drop column COUPON_FREQ;
alter table GB_MASK_DATA_AUTOCALL drop column HAS_CON_COUPON;
alter table GB_MASK_DATA_AUTOCALL drop column CON_COUPON;
alter table GB_MASK_DATA_AUTOCALL drop column CON_COUPON_LEVEL;
alter table GB_MASK_GENERATED_DATES drop column COUPON_LASTDATE;
alter table GB_MASK_GENERATED_DATES drop column COUPON_LEVEL;
alter table GB_MASK_GENERATED_DATES drop column CONDITIONAL_COUPON;
alter table AUDIT_GB_MASK_DATA_AUTOCALL drop column COUPON_FREQ;
alter table AUDIT_GB_MASK_DATA_AUTOCALL drop column HAS_CON_COUPON;
alter table AUDIT_GB_MASK_DATA_AUTOCALL drop column CON_COUPON;
alter table AUDIT_GB_MASK_DATA_AUTOCALL drop column CON_COUPON_LEVEL;
alter table AUDIT_GB_MASK_GENERATED_DATES drop column COUPON_LASTDATE;
alter table AUDIT_GB_MASK_GENERATED_DATES drop column COUPON_LEVEL;
alter table AUDIT_GB_MASK_GENERATED_DATES drop column CONDITIONAL_COUPON;


commit;

--}}SOPHIS_SQL