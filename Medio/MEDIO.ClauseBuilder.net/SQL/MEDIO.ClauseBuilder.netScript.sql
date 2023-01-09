--{{SOPHIS_SQL (do not delete this line)

--create table MASK_DATA_AUTOCALL_TKT
--(
--CODE number(12,0),
--COUPON_FREQ number,
--HAS_CON_COUPON NUMBER(1,0),
--CON_COUPON number,
--CON_COUPON_LEVEL NUMBER,
--constraint "PK_MASK_DATA_AUTOCALL_TKT" PRIMARY KEY(CODE),
--constraint FK_MASK_DATA_AUTOCALL_TKT foreign key (CODE) references TITRES (SICOVAM) on delete cascade
--)
--comment on table MASK_DATA_AUTOCALL_TKT is 'Specific data for toolkit autocall clause builder';
--comment on column MASK_DATA_AUTOCALL_TKT.CODE is 'Mandatory column, gives the code of the clause builder instrument';

--create table MASK_DATA_AUTOCALL_TKT as select * from GB_MASK_DATA_AUTOCALL where 1=0;
--alter table MASK_DATA_AUTOCALL_TKT add COUPON_FREQ number;
--alter table MASK_DATA_AUTOCALL_TKT add HAS_CON_COUPON NUMBER(1,0);
--alter table MASK_DATA_AUTOCALL_TKT add CON_COUPON number;
--alter table MASK_DATA_AUTOCALL_TKT add CON_COUPON_LEVEL number;

--create table AUDIT_MASK_DATA_AUTOCALL_TKT as select * from MASK_DATA_AUTOCALL_TKT where 1=0;
--alter table AUDIT_MASK_DATA_AUTOCALL_TKT add COUPON_FREQ number;
--alter table AUDIT_MASK_DATA_AUTOCALL_TKT add HAS_CON_COUPON NUMBER(1,0);
--alter table AUDIT_MASK_DATA_AUTOCALL_TKT add CON_COUPON number;
--alter table AUDIT_MASK_DATA_AUTOCALL_TKT add CON_COUPON_LEVEL number;

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

--}}SOPHIS_SQL