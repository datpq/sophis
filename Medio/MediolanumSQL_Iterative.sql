--{{SOPHIS_SQL (do not delete this line)

------------------------------
----  MEDIOLANUM_713.16.13.599 ----
------------------------------
--/*FX Trades Market Way */
CREATE TABLE "MEDIO_TKT_CONFIG" 
(	"CONFIG_NAME" VARCHAR2(200 BYTE) NOT NULL ENABLE, 
    "CONFIG_VALUE" VARCHAR2(200 BYTE), 
    "COMMENTS" VARCHAR2(250 BYTE)
);
commit;


INSERT INTO "MEDIO_TKT_CONFIG" 
(CONFIG_NAME, CONFIG_VALUE, COMMENTS) VALUES 
('FX_BOOKING_ISO', '1', '1 = activate the toolkit to switch the booking of all FXtrades to ISO market way');
Commit;

------------------------------
----  MEDIOLANUM_713.18.9.615 ----
------------------------------
INSERT INTO "MEDIO_TKT_CONFIG" 
(CONFIG_NAME, CONFIG_VALUE, COMMENTS) VALUES 
('CHECK_REBALANCING_BREACH', '1', '1 = activate the toolkit check the breach. If config_name does not exist the check is activated.');
Commit;

------------------------------
----  MEDIOLANUM_713.16.13.560 ----
------------------------------
--/* Variance Swap */
begin
TK_ADD_TITRES_COLUMN('MEDIO_VARIANCE_DAYS','NUMBER(10)', 0, 'variance swap business days or user input');
end;

------------------------------
----  MEDIOLANUM_713.16.10.546 ----
------------------------------
create table MEDIO_TKT_ALLOTMENT_UCITS
(
		InstrumentType VARCHAR2(20) not null,
		Allotments VARCHAR2(100) not null,
    PRIMARY KEY (InstrumentType)
);

INSERT INTO MEDIO_TKT_ALLOTMENT_UCITS (InstrumentType, Allotments) VALUES ('TRS', 'TRS EQUITY SINGLE;TRS FIXED INCOME SINGLE'); 
INSERT INTO MEDIO_TKT_ALLOTMENT_UCITS (InstrumentType, Allotments) VALUES ('CDS', 'CDS'); 
INSERT INTO MEDIO_TKT_ALLOTMENT_UCITS (InstrumentType, Allotments) VALUES ('CDX', 'CDX'); 
INSERT INTO MEDIO_TKT_ALLOTMENT_UCITS (InstrumentType, Allotments) VALUES ('CONVERTIBLE BOND', 'CONVERTIBLE BOND' ); 


--/* Variance Swap */
begin
TK_ADD_TITRES_COLUMN('MEDIO_VEGA_NOTIONAL','NUMBER(10)', 0, 'variance swap vega notional');
TK_ADD_TITRES_COLUMN('MEDIO_VOL_STRIKE','NUMBER(10)', 0, 'variance swap vol strike');
end;

------------------------------
----  MEDIOLANUM_713.16.10.534 ----
------------------------------
create table MEDIO_TKT_ALLOTMENT_CONDS ( ID int, COND_NAME VARCHAR2(50), INSTR_TYPE VARCHAR2(50), INSTR_PROP VARCHAR2(50), COND_VALUES VARCHAR2(300));
INSERT INTO MEDIO_TKT_ALLOTMENT_CONDS (ID, COND_NAME, INSTR_TYPE, INSTR_PROP, COND_VALUES) VALUES (1, 'Is_GFP_Fund', 'Z', 'Legal Form', 'Open-End Fund'); INSERT INTO MEDIO_TKT_ALLOTMENT_CONDS (ID, COND_NAME, INSTR_TYPE, INSTR_PROP, COND_VALUES) VALUES (2, 'Is_ETF_Fund', 'Z', 'Legal Form', 'ETF,ETC,ETN');

------------------------------
----  MEDIOLANUM_713.16.9.520 ----
------------------------------

-- Medio NDF currencies 
CREATE TABLE MEDIO_NDF_CURRENCY( CURRENCY VARCHAR2(10));
INSERT INTO MEDIO_NDF_CURRENCY (CURRENCY) VALUES ('ARS');
INSERT INTO MEDIO_NDF_CURRENCY (CURRENCY) VALUES ('BRL');
INSERT INTO MEDIO_NDF_CURRENCY (CURRENCY) VALUES ('CLP');
INSERT INTO MEDIO_NDF_CURRENCY (CURRENCY) VALUES ('CNY');
INSERT INTO MEDIO_NDF_CURRENCY (CURRENCY) VALUES ('COP');
INSERT INTO MEDIO_NDF_CURRENCY (CURRENCY) VALUES ('EGP'); 
INSERT INTO MEDIO_NDF_CURRENCY (CURRENCY) VALUES ('GTQ');
INSERT INTO MEDIO_NDF_CURRENCY (CURRENCY) VALUES ('IDR');
INSERT INTO MEDIO_NDF_CURRENCY (CURRENCY) VALUES ('INR');
INSERT INTO MEDIO_NDF_CURRENCY (CURRENCY) VALUES ('KES');
INSERT INTO MEDIO_NDF_CURRENCY (CURRENCY) VALUES ('KRW');
INSERT INTO MEDIO_NDF_CURRENCY (CURRENCY) VALUES ('KZT');
INSERT INTO MEDIO_NDF_CURRENCY (CURRENCY) VALUES ('MYR');
INSERT INTO MEDIO_NDF_CURRENCY (CURRENCY) VALUES ('NGN');
INSERT INTO MEDIO_NDF_CURRENCY (CURRENCY) VALUES ('PEN');
INSERT INTO MEDIO_NDF_CURRENCY (CURRENCY) VALUES ('PHP');
INSERT INTO MEDIO_NDF_CURRENCY (CURRENCY) VALUES ('PKR');
INSERT INTO MEDIO_NDF_CURRENCY (CURRENCY) VALUES ('TWD');
INSERT INTO MEDIO_NDF_CURRENCY (CURRENCY) VALUES ('UAH');
INSERT INTO MEDIO_NDF_CURRENCY (CURRENCY) VALUES ('UYU');
INSERT INTO MEDIO_NDF_CURRENCY (CURRENCY) VALUES ('VEF');
INSERT INTO MEDIO_NDF_CURRENCY (CURRENCY) VALUES ('VND'); 

------------------------------
----  MEDIOLANUM_713.16.14	  ----
------------------------------
INSERT INTO EXTRNL_REFERENCES_DEFINITION (REF_IDENT,REF_NAME,REDUNDANCY) values (nvl((select max(REF_IDENT)+1 from EXTRNL_REFERENCES_DEFINITION),1),'MANAGER_CODE',1);


------------------------------
----  MEDIOLANUM_713.16.10	  ----
------------------------------
INSERT INTO SWAP_POS_REFS_DEF (REF_NAME,REDUNDANT,USE_CUSTOM_REF) SELECT 'RBC_REPORTING_BE',1,1 FROM DUAL WHERE NOT EXISTS (SELECT * FROM SWAP_POS_REFS_DEF WHERE REF_NAME = 'RBC_REPORTING_BE');
INSERT INTO SWAP_POS_REFS_DEF (REF_NAME,REDUNDANT,USE_CUSTOM_REF) SELECT 'RBC_REPORTING_BE_EXCEPTION',1,1 FROM DUAL WHERE NOT EXISTS (SELECT * FROM SWAP_POS_REFS_DEF WHERE REF_NAME = 'RBC_REPORTING_BE_EXCEPTION');


------------------------------
----  MEDIO_0.713.15.13	  ----
------------------------------
alter table GB_MASK_DATA_AUTOCALL add COUPON_FREQ number;
alter table GB_MASK_DATA_AUTOCALL add HAS_CON_COUPON NUMBER(1,0);
alter table GB_MASK_DATA_AUTOCALL add CON_COUPON number;
alter table GB_MASK_DATA_AUTOCALL add CON_COUPON_LEVEL number;
alter table GB_MASK_GENERATED_DATES add COUPON_LASTDATE DATE;
alter table GB_MASK_GENERATED_DATES add COUPON_LEVEL number;
alter table GB_MASK_GENERATED_DATES add CONDITIONAL_COUPON number;
alter table AUDIT_GB_MASK_DATA_AUTOCALL add COUPON_FREQ number;
alter table AUDIT_GB_MASK_DATA_AUTOCALL add HAS_CON_COUPON NUMBER(1,0);
alter table AUDIT_GB_MASK_DATA_AUTOCALL add CON_COUPON number;
alter table AUDIT_GB_MASK_DATA_AUTOCALL add CON_COUPON_LEVEL number;
alter table AUDIT_GB_MASK_GENERATED_DATES add COUPON_LASTDATE DATE;
alter table AUDIT_GB_MASK_GENERATED_DATES add COUPON_LEVEL number;
alter table AUDIT_GB_MASK_GENERATED_DATES add CONDITIONAL_COUPON number;

------------------------------
----  MEDIO_0.713.15.3	  ----
----	Toolkit 7&8		 -----
------------------------------

TK_ADD_HISTOMVTS_COLUMN('MEDIO_PARENTORDERID','number', 0, 'Hegding/funding parent order id');
insert into ORDER_PROPERTY (ID,NAME,CATEGORY,DATATYPE,VISIBILITY) values (nvl((select max(id)+1 from ORDER_PROPERTY),1),'MEDIO_PARENTORDERID','Toolkit','System.Int32',1);
insert into EXEC_PROPERTY(PROPERTYID,NAME,CATEGORY,DATATYPE,TYPE,VISIBILITY) values (nvl((select max(PROPERTYID)+1 from EXEC_PROPERTY),1),'MEDIO_PARENTORDERID','Toolkit','System.Int32',2,1);
insert into EXEC_PROPERTY(PROPERTYID,NAME,CATEGORY,DATATYPE,TYPE,VISIBILITY) values (nvl((select max(PROPERTYID)+1 from EXEC_PROPERTY),1),'MEDIO_IsHedgedFunded','Toolkit','System.Boolean',1,1);


------------------------------
----  MEDIO_0.713.12.0	  ----
------------------------------

--/* Hedging widget audit table */
create table MEDIO_AUDIT_HEDGING_POSITION
	(
		"POSITIONID" NUMBER(15,0),
		"OPERATORNAME" VARCHAR2(20),
		"ACTION" NUMBER(1),
		"MODIFICATIONTIME" TIMESTAMP,
		"HEDGINGCOMMENT" VARCHAR2(1000),
		"HEDGINGSTATUS" NUMBER(1),
		"NETCOMMITMENT" NUMBER,
		CONSTRAINT "PK_BSG_CLIENTDECISION" PRIMARY KEY ("POSITIONID", "MODIFICATIONTIME", "OPERATORNAME")	);

COMMENT ON TABLE  MEDIO_AUDIT_HEDGING_POSITION							IS '(MEDIO) Table which stores postion hedging actions history';
COMMENT ON COLUMN MEDIO_AUDIT_HEDGING_POSITION."POSITIONID"				IS 'Id of the position';
COMMENT ON COLUMN MEDIO_AUDIT_HEDGING_POSITION."OPERATORNAME"			IS 'Operator name';
COMMENT ON COLUMN MEDIO_AUDIT_HEDGING_POSITION."ACTION"					IS 'Action: ';
COMMENT ON COLUMN MEDIO_AUDIT_HEDGING_POSITION."MODIFICATIONTIME"		IS 'Modification timestamp';
COMMENT ON COLUMN MEDIO_AUDIT_HEDGING_POSITION."HEDGINGCOMMENT"			IS 'HEGDINGCOMMENT';
COMMENT ON COLUMN MEDIO_AUDIT_HEDGING_POSITION."HEDGINGSTATUS"			IS 'Hedging status:';
COMMENT ON COLUMN MEDIO_AUDIT_HEDGING_POSITION."NETCOMMITMENT"			IS 'Net commitment ';

commit;

--/* GUI FIELDS */
begin
-- Instrument->Barrier Option 
TK_ADD_TITRES_COLUMN('MAX_DELTA', 'number', NULL, 'Maximum delta is only used for barrier options'); 

-- Trade entry screen FO fields
TK_ADD_HISTOMVTS_COLUMN('MEDIO_HEDGE_CHECK','number(1)', 0, 'Toolkit hedging check box');
TK_ADD_HISTOMVTS_COLUMN('MEDIO_OUTSIDE_HRS','number(1)', 0, 'Toolkit Asian future market trading hours check box');
TK_ADD_HISTOMVTS_COLUMN('MEDIO_COMMENTS','VARCHAR2(40)', NULL, 'Toolkit comment field to enter hedging comments');
TK_ADD_HISTOMVTS_COLUMN('MEDIO_OUTSIDE_HRS_LOG','VARCHAR2(256)', NULL, 'Toolkit field to log trade date changing information due to outside trading hrs');

-- Instrument fields - Index 
TK_ADD_TITRES_COLUMN('MEDIO_MARKET_TIMEZONE','VARCHAR2(40)', NULL, 'Index market time zone');
TK_ADD_TITRES_COLUMN('MEDIO_PIT_CLOSING','VARCHAR2(40)', NULL, 'Market pit session closing time');
end;

-- /* USER RIGHTS */
insert into USER_RIGHT_TABLE (IDX,NAME,CATEGORY,COMMENTS,INTERNAL_RIGHT,RIGHT,RIGHT_TYPE) values ((select max(idx)+1 from USER_RIGHT_TABLE),'Hedge Check','Toolkit','Right to check trade screen fields',0, -1, 2);
insert into USER_RIGHT_TABLE (IDX,NAME,CATEGORY,COMMENTS,INTERNAL_RIGHT,RIGHT,RIGHT_TYPE) values ((select max(idx)+1 from USER_RIGHT_TABLE),'Netting and Hedging Validation ','Toolkit','Right to perform actions in Hedging Widget',0, -1, 2);

-- /* ORDER PROPERTIES */
-- 1) Hedging flag
insert into ORDER_PROPERTY (ID,NAME,CATEGORY,DATATYPE,VISIBILITY) values (nvl((select max(id)+1 from ORDER_PROPERTY),1),'MEDIO_HEDGE_CHECK','Toolkit','System.Boolean',1);
insert into ORDER_PROPERTY (ID,NAME,CATEGORY,DATATYPE,VISIBILITY) values (nvl((select max(id)+1 from ORDER_PROPERTY),1),'MEDIO_COMMENTS','Toolkit','System.String',1);
-- 2) Asian Market - outside trading hours 
insert into ORDER_PROPERTY (ID,NAME,CATEGORY,DATATYPE,VISIBILITY) values (nvl((select max(id)+1 from ORDER_PROPERTY),1),'MEDIO_OUTSIDE_HRS','Toolkit','System.Boolean',1);


-- /* EXEC PROPERTIES */
-- 1) Hedging flag
insert into EXEC_PROPERTY(PROPERTYID,NAME,CATEGORY,DATATYPE,TYPE,VISIBILITY) values (nvl((select max(PROPERTYID)+1 from EXEC_PROPERTY),1),'MEDIO_HEDGE_CHECK','Toolkit','System.Boolean',2,1);
insert into EXEC_PROPERTY(PROPERTYID,NAME,CATEGORY,DATATYPE,TYPE,VISIBILITY) values (nvl((select max(PROPERTYID)+1 from EXEC_PROPERTY),1),'MEDIO_COMMENTS','Toolkit','System.String',2,1);
-- 2) Asian Market - outside trading hours
insert into EXEC_PROPERTY(PROPERTYID,NAME,CATEGORY,DATATYPE,TYPE,VISIBILITY) values (nvl((select max(PROPERTYID)+1 from EXEC_PROPERTY),1),'MEDIO_OUTSIDE_HRS','Toolkit','System.Boolean',2,1);


-- /* EXTERNAL REF */
-- 1) Market external ref 
insert into EXTRNL_REF_MARKET_DEFINITION values((select max(ref_ident)+1 from EXTRNL_REF_MARKET_DEFINITION), 'Pit closing time (local)');
insert into EXTRNL_REF_MARKET_DEFINITION values((select max(ref_ident)+1 from EXTRNL_REF_MARKET_DEFINITION), 'Timezone');

-- adding Trading target portfolio parameter
INSERT INTO MEDIO_TKT_CONFIG VALUES('Trading_Target_Folio_Name','MIFL','Name of main trading portfolio');
-- Changing portfolio Names
update FOLIO set name = 'MIFL' where name = 'MAML';
update FOLIO set name = 'MIFL Cash' where name = 'MAML Cash';

-- Adding Target Column name for new 	 column
insert into MEDIO_TKT_CONFIG(config_name,Config_value,comments)  VALUES ('MEDIO_AggregFirst_Target_Col','Compliance - NR - ValidParent ','');


------------------------------
----  MEDIO_0. 713.19.9:	  ---- 
----	Toolkit 707		 -----
------------------------------

-- backing up current folio table
create table folio_backup as select * from folio;


-- Clean up any Existing Fees Accruals Entries...
delete folio where name ='Fee Accruals';

-- Creating new Fee Accruals folio under each fund with underlying ref='1EUR' and Entity = fund entity
insert into folio (name, ident, mgr, sicovam, currency, limite_perte,infos,type,entite, business_line_id)
select 'Fee Accruals',SEQFOLIO.NEXTVAL,f.ident,t.sicovam,0,0.000000000000,'',4,fu.entity, null from titres t, folio f, funds fu where
t.reference='1EUR' and fu.tradingfolio = f.ident and fu.tradingfolio in (select tradingfolio from funds where sicovam in (select sicovam from titres where type='Z'));


------------------------------
----  MEDIO_0.713.19.9.721	  ----
------------------------------

insert into MEDIO_TKT_CONFIG (CONFIG_NAME,CONFIG_VALUE) values('Use_Trading_Target_Folio_Cash',0);

commit;

------------------------------
----  MEDIO_0.713.19.9.728	  ----
------------------------------
begin
    tk_add_histomvts_column('MEDIO_GROSS_CONS_AMOUNT', 'NUMBER', NULL, 'MEDIO Gross Consideration Amount');
end;

--}}SOPHIS_SQL