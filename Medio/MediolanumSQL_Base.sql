--{{SOPHIS_SQL (do not delete this line)


--/* GUI FIELDS */
begin
-- Trade entry screen FO fields
TK_ADD_HISTOMVTS_COLUMN('MEDIO_OUTSIDE_HRS','number(1)', 0, 'Toolkit Asian future market trading hours check box');
TK_ADD_HISTOMVTS_COLUMN('MEDIO_OUTSIDE_HRS_LOG','VARCHAR2(256)', NULL, 'Toolkit field to log trade date changing information due to outside trading hrs');
TK_ADD_HISTOMVTS_COLUMN('MEDIO_PARENTORDERID','number', 0, 'Toolkit Hegding/funding parent order id');
/* RMA RBC TRADE ID */
tk_add_histomvts_column('TKT_RBC_TRADE_ID', 'VARCHAR2(256)', NULL, 'RBC Transaction ID');


-- Instrument fields - Index 
TK_ADD_TITRES_COLUMN('MEDIO_MARKET_TIMEZONE','VARCHAR2(40)', NULL, 'Index market time zone');
TK_ADD_TITRES_COLUMN('MEDIO_PIT_CLOSING','VARCHAR2(40)', NULL, 'Market pit session closing time');
TK_ADD_TITRES_COLUMN('MEDIO_PIT_OPENING','VARCHAR2(40)', NULL, 'Market pit session opening time');
-- Instrument fields - Swap 
TK_ADD_TITRES_COLUMN('MEDIO_VEGA_NOTIONAL','NUMBER(10)', 0, 'variance swap vega notional');
TK_ADD_TITRES_COLUMN('MEDIO_VOL_STRIKE','NUMBER(10)', 0, 'variance swap vol strike');

-- Instrument fields - cluase builder  
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

end;


-- /* Trade External Ref Definition */
INSERT INTO SWAP_POS_REFS_DEF (REF_NAME,REDUNDANT,USE_CUSTOM_REF) SELECT 'RBC_REPORTING_BE',1,1 FROM DUAL WHERE NOT EXISTS (SELECT * FROM SWAP_POS_REFS_DEF WHERE REF_NAME = 'RBC_REPORTING_BE');
INSERT INTO SWAP_POS_REFS_DEF (REF_NAME,REDUNDANT,USE_CUSTOM_REF) SELECT 'RBC_REPORTING_BE_EXCEPTION',1,1 FROM DUAL WHERE NOT EXISTS (SELECT * FROM SWAP_POS_REFS_DEF WHERE REF_NAME = 'RBC_REPORTING_BE_EXCEPTION');


-- /* ORDER PROPERTIES */
insert into ORDER_PROPERTY (ID,NAME,CATEGORY,DATATYPE,VISIBILITY) values (nvl((select max(id)+1 from ORDER_PROPERTY),1),'MEDIO_OUTSIDE_HRS','Toolkit','System.Boolean',1);
insert into ORDER_PROPERTY (ID,NAME,CATEGORY,DATATYPE,VISIBILITY) values (nvl((select max(id)+1 from ORDER_PROPERTY),1),'MEDIO_PARENTORDERID','Toolkit','System.Int32',1);


-- /* EXEC PROPERTIES */
insert into EXEC_PROPERTY(PROPERTYID,NAME,CATEGORY,DATATYPE,TYPE,VISIBILITY) values (nvl((select max(PROPERTYID)+1 from EXEC_PROPERTY),1),'MEDIO_OUTSIDE_HRS','Toolkit','System.Boolean',2,1);
insert into EXEC_PROPERTY(PROPERTYID,NAME,CATEGORY,DATATYPE,TYPE,VISIBILITY) values (nvl((select max(PROPERTYID)+1 from EXEC_PROPERTY),1),'MEDIO_IsHedgedFunded','Toolkit','System.Boolean',1,1);
insert into EXEC_PROPERTY(PROPERTYID,NAME,CATEGORY,DATATYPE,TYPE,VISIBILITY) values (nvl((select max(PROPERTYID)+1 from EXEC_PROPERTY),1),'MEDIO_PARENTORDERID','Toolkit','System.Int32',2,1);

-- /* TRANSACTION EXTERNAL REF */
INSERT INTO SWAP_POS_REFS_DEF (REF_NAME,REDUNDANT,USE_CUSTOM_REF) SELECT 'RBC_TRADE_ID',1,1 FROM DUAL WHERE NOT EXISTS (SELECT * FROM SWAP_POS_REFS_DEF WHERE REF_NAME = 'RBC_TRADE_ID');

-- /* INSTRUMENT EXTERNAL REF */
INSERT INTO EXTRNL_REFERENCES_DEFINITION (REF_IDENT,REF_NAME,REDUNDANCY) values (nvl((select max(REF_IDENT)+1 from EXTRNL_REFERENCES_DEFINITION),1),'MANAGER_CODE',1);

-- /*UCITS allotments*/ 
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

commit;

-- /* RMA Trigger */
create or replace TRIGGER "TR_RMA_MESSAGES_STATUS" BEFORE
INSERT OR
UPDATE ON RMA_MESSAGES FOR EACH ROW
BEGIN
IF (INSERTING) THEN
Begin
IF(:New.ERRORMESSAGE Is NOT NULL And :New.ERRORMESSAGE like '%TPCode_DuplicateExecution%') Then
BEGIN
:New.LASTSTATUS := 6;

INSERT INTO RMA_MESSAGES_AUDIT(APPLICATION,DATETIME,ENTEREDDATECODE,ERRORMESSAGE,EXTERNALREF,INTERNALREF,MODIFICATIONS,SOPHIS_USER,SOURCEID,STATUS) VALUES ('RBC_Uploader',Sysdate,:Old.ENTEREDDATECODE,'Duplicate deal - Status updated by trigger',:Old.EXTERNALREF,:Old.INTERNALREF,null,'RBC_Uploader',:Old.SOURCEID,:New.LASTSTATUS);
End;
END IF;
End;
ELSIF (UPDATING) Then
Begin
IF(:New.ERRORMESSAGE Is NOT NULL And :New.ERRORMESSAGE like '%TPCode_DuplicateExecution%') Then
BEGIN
:New.LASTSTATUS := 6;
INSERT INTO RMA_MESSAGES_AUDIT(APPLICATION,DATETIME,ENTEREDDATECODE,ERRORMESSAGE,EXTERNALREF,INTERNALREF,MODIFICATIONS,SOPHIS_USER,SOURCEID,STATUS) VALUES ('RBC_Uploader',Sysdate,:Old.ENTEREDDATECODE,'Duplicate deal - Status updated by trigger',:Old.EXTERNALREF,:Old.INTERNALREF,null,'RBC_Uploader',:Old.SOURCEID,:New.LASTSTATUS);

End;
END IF;
End;
End if;
End;


-- Tag rbc trades with an adhoc string in the ext ref table after upload
INSERT INTO EXTRNL_REFERENCES_TRADES (SOPHIS_IDENT,ORIGIN,VALUE)
select unique(refcon), 'RBC_IMPORT_SESSION', '6VanillaFunds import Q1-17 24-APR-2017@5pm' from HISTOMVTS where OPERATEUR in (select ident from RISKUSERS where name = 'RBCUploader')
and REFCON NOT IN (SELECT e.SOPHIS_IDENT FROM EXTRNL_REFERENCES_TRADES e WHERE e.ORIGIN = 'RBC_IMPORT_SESSION');


-- Allotment mapping used for the UCITS column
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

-- TEMPORARY TABLE FOR HANDLING OF FXALL TRADE ID EXTERNAL REFERENCE

-- Adding the external Reference itself:
delete swap_pos_refs_def where ref_name='FXALL_TRADE_ID';
insert into swap_pos_refs_def(REF_NAME,REDUNDANT,USE_CUSTOM_REF) VALUES('FXALL_TRADE_ID',1,1);

-- Temporary table.
create TABLE MEDIO_FXALL_TEMP_EXTRNREF (PLACEMENT_ID NUMBER,ENTITY NUMBER,EXTERNAL_REFERENCE VARCHAR2(250));

--}}SOPHIS_SQL