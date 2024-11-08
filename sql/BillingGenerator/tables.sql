---------BL_LOGS
CREATE TABLE BL_LOGS
(
  ID        NUMBER,
  DT        TIMESTAMP(6),
  SEVERITY  VARCHAR2(10 BYTE),
  LOGGER    VARCHAR2(255 BYTE),
  MSG       VARCHAR2(4000 BYTE)
)
TABLESPACE CDC_DAT_10M
PCTUSED    0
PCTFREE    10
INITRANS   1
MAXTRANS   255
STORAGE    (
            INITIAL          10M
            NEXT             10M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
LOGGING 
NOCOMPRESS 
NOCACHE
NOPARALLEL
MONITORING;


CREATE INDEX IDX_BL_LOGS ON BL_LOGS
(LOGGER, DT)
LOGGING
TABLESPACE CDC_IDX_10M
PCTFREE    10
INITRANS   2
MAXTRANS   255
STORAGE    (
            INITIAL          64K
            NEXT             1M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
NOPARALLEL;


CREATE UNIQUE INDEX PK_BL_LOGS ON BL_LOGS
(ID)
LOGGING
TABLESPACE CDC_DAT_10M
PCTFREE    10
INITRANS   2
MAXTRANS   255
STORAGE    (
            INITIAL          10M
            NEXT             10M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
NOPARALLEL;

---------BL_FEES
CREATE GLOBAL TEMPORARY TABLE BL_FEES
(
  MVTIDENT          NUMBER(10),
  "from"            DATE,
  "to"              DATE,
  QTY               NUMBER,
  PRIX              NUMBER,
  TAUX_CHANGE       NUMBER,
  PRIX_DEVISE_FACT  NUMBER,
  VALUE             NUMBER,
  TAUX_COM          NUMBER,
  DAYS              INTEGER,
  MONTANT_FEE       NUMBER
)
ON COMMIT PRESERVE ROWS
NOCACHE;

---------BL_REBATES_POOL
CREATE GLOBAL TEMPORARY TABLE BL_REBATES_POOL
(
  MVTIDENT   NUMBER(10),
  "from"     DATE,
  "to"       DATE,
  VALUE      NUMBER,
  RATE_NAME  VARCHAR2(24 BYTE),
  RATE       NUMBER,
  SPREAD     NUMBER,
  DAYS       INTEGER,
  INTEREST   NUMBER
)
ON COMMIT PRESERVE ROWS
NOCACHE;

---------BL_REBATES_HORS_POOL
CREATE GLOBAL TEMPORARY TABLE BL_REBATES_HORS_POOL
(
  MVTIDENT          NUMBER(10),
  "from"            DATE,
  "to"              DATE,
  QTY               NUMBER,
  PRIX              NUMBER,
  TAUX_CHANGE       NUMBER,
  PRIX_DEVISE_FACT  NUMBER,
  HEDGING           NUMBER,
  VALUE             NUMBER,
  RATE_NAME         VARCHAR2(24 BYTE),
  RATE              NUMBER,
  SPREAD            NUMBER,
  DAYS              INTEGER,
  INTEREST          NUMBER
)
ON COMMIT PRESERVE ROWS
NOCACHE;

---------BL_CONTRACTS
CREATE GLOBAL TEMPORARY TABLE BL_CONTRACTS
(
  ENTITE        NUMBER(10),
  CONTREPARTIE  NUMBER(10),
  PERIMETERID   NUMBER(10),
  DEVISECTT     NUMBER(10),
  TAUX_VAR      NUMBER(10),
  MVTIDENT      NUMBER(10),
  REFCON        NUMBER(10)
)
ON COMMIT PRESERVE ROWS
NOCACHE;

---------CMA_RPT_OPERATIONS
CREATE TABLE CMA_RPT_OPERATIONS
(
  OP_ID             NUMBER(10),
  DTGEN             DATE,
  MVTTYPE           VARCHAR2(2 BYTE),
  MVTIDENT          NUMBER(10),
  MOIS_FACT         DATE,
  CTPY_ID           NUMBER(10),
  CTPY_LIBELLE      VARCHAR2(40 BYTE),
  OP_SICOVAM        NUMBER(10),
  OP_REF            VARCHAR2(40 BYTE),
  OP_LIBELLE        VARCHAR2(40 BYTE),
  OP_TYPE           VARCHAR2(2 BYTE),
  OP_TIMEBASIS      NUMBER(3),
  OP_START_DATE     DATE,
  OP_ECH_DATE       DATE,
  SSJ_ISIN          VARCHAR2(20 BYTE),
  SSJ_LIBELLE       VARCHAR2(40 BYTE),
  TYPEGAR           VARCHAR2(2 BYTE),
  FACT_METHOD       VARCHAR2(15 BYTE),
  FACT_FREQ         VARCHAR2(10 BYTE),
  FACT_CUR          VARCHAR2(3 BYTE),
  FACT_DAYS         NUMBER(4),
  CONST_CUR         VARCHAR2(3 BYTE),
  RATE_ID           NUMBER(10),
  RATE_LIBELLE      VARCHAR2(40 BYTE),
  OP_HEDGING        NUMBER(18,6),
  TA                NUMBER(18,6),
  OPCVM             VARCHAR2(40 BYTE),
  TA_REFCON         NUMBER(10),
  PERIMETER_ID      NUMBER,
  RISKUSER          NUMBER(10),
  ENTITY_ID         NUMBER(10),
  ENTITY_LIBELLE    VARCHAR2(50 BYTE),
  MIN_FEE           NUMBER,
  FEE_PAYED         NUMBER,
  EXPL_AMOUNT       NUMBER,
  USERID_INIT       VARCHAR2(40 BYTE),
  SEDOL             VARCHAR2(50 BYTE),
  TYPEVAL           VARCHAR2(1 BYTE),
  BAL_MIN_FEE       NUMBER,
  LEGAL_ENTITY_ID   VARCHAR2(24 BYTE),
  LEGAL_ENTITY_LIB  VARCHAR2(50 BYTE)
)
TABLESPACE CDC_DAT_10M
PCTUSED    0
PCTFREE    10
INITRANS   1
MAXTRANS   255
STORAGE    (
            INITIAL          10M
            NEXT             10M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
LOGGING 
NOCOMPRESS 
NOCACHE
NOPARALLEL
MONITORING;

CREATE INDEX IDX_CMA_RPT_OPER_EMCC ON CMA_RPT_OPERATIONS
(ENTITY_ID, MOIS_FACT, CTPY_ID, FACT_CUR)
LOGGING
TABLESPACE CDC_DAT_10M
PCTFREE    10
INITRANS   2
MAXTRANS   255
STORAGE    (
            INITIAL          10M
            NEXT             10M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
NOPARALLEL;

CREATE INDEX IDX_CMA_RPT_OPER_MVTIDENT ON CMA_RPT_OPERATIONS
(MVTIDENT)
LOGGING
TABLESPACE CDC_IDX_10M
PCTFREE    10
INITRANS   2
MAXTRANS   255
STORAGE    (
            INITIAL          64K
            NEXT             1M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
NOPARALLEL;

---------CMA_RPT_EXPLANATIONS
CREATE TABLE CMA_RPT_EXPLANATIONS
(
  EXPL_ID      NUMBER(10),
  OP_ID        NUMBER(10),
  DTGEN        DATE,
  DFROM        DATE,
  DTO          DATE,
  DAYS         NUMBER(5),
  QTY          NUMBER(12),
  LB           CHAR(1 BYTE),
  SPOT         NUMBER(18,6),
  SPOT_CUR     VARCHAR2(3 BYTE),
  FOREX        NUMBER(18,6),
  COMM_RATE    NUMBER(18,6),
  AMOUNT       NUMBER(22,6),
  INTEREST     NUMBER(22,6),
  SPREAD       NUMBER(18,6),
  RATE         NUMBER(18,6),
  PERIOD_RATE  NUMBER(18,6),
  REF          NUMBER(10),
  QTY_UNIT     NUMBER(12)
)
TABLESPACE CDC_DAT_10M
PCTUSED    0
PCTFREE    10
INITRANS   1
MAXTRANS   255
STORAGE    (
            INITIAL          10M
            NEXT             10M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
LOGGING 
NOCOMPRESS 
NOCACHE
NOPARALLEL
MONITORING;

CREATE INDEX CMA_RPT_EXPL_INDEX ON CMA_RPT_EXPLANATIONS
(OP_ID)
LOGGING
TABLESPACE CDC_IDX_10M
PCTFREE    10
INITRANS   2
MAXTRANS   255
STORAGE    (
            INITIAL          10M
            NEXT             10M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
NOPARALLEL;

---------TS_XML_TBL
CREATE TABLE TS_XML_TBL
(
  ID          VARCHAR2(24 BYTE) CONSTRAINT SYS_C004391 NOT NULL,
  SYSTEM      VARCHAR2(20 BYTE) CONSTRAINT SYS_C004392 NOT NULL,
  XML         CLOB,
  ISSTREAMED  CHAR(1 BYTE)                      DEFAULT 0,
  ISDONE      CHAR(1 BYTE)                      DEFAULT 0,
  DATEINSERT  DATE
)
TABLESPACE BLOB_DAT
PCTUSED    0
PCTFREE    10
INITRANS   1
MAXTRANS   255
STORAGE    (
            INITIAL          64K
            NEXT             1M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
LOGGING 
NOCOMPRESS 
LOB (XML) STORE AS 
      ( TABLESPACE  BLOB_DAT 
        ENABLE      STORAGE IN ROW
        CHUNK       8192
        PCTVERSION  10
        NOCACHE
        INDEX       (
          TABLESPACE BLOB_DAT
          STORAGE    (
                      INITIAL          64K
                      NEXT             1
                      MINEXTENTS       1
                      MAXEXTENTS       UNLIMITED
                      PCTINCREASE      0
                      BUFFER_POOL      DEFAULT
                     ))
        STORAGE    (
                    INITIAL          64K
                    NEXT             1M
                    MINEXTENTS       1
                    MAXEXTENTS       UNLIMITED
                    PCTINCREASE      0
                    BUFFER_POOL      DEFAULT
                   )
      )
NOCACHE
NOPARALLEL
MONITORING;