CREATE OR REPLACE PACKAGE BL
AS
    /******************************************************************************
    PACKAGE BL (BILLING GENERATOR)
      
    27/02/2012 - @DPH: First creation of package
    07/05/2012 - @DPH: V1.01 test arrondi TotalRebates, verify dateval when find max(mvtident) for pool
    07/05/2012 - @DPH: V1.02 Fee_mark bug
    11/05/2012 - @DPH: V1.03 deal inter-entity and bug fixed_price coté centième  
    22/05/2012 - @DPH: V1.04 two many closing bloc all process
    23/05/2012 - @DPH: V1.05 optimization and some evols
    31/05/2012 - @DPH: V1.06 correction fact_cur and devisepay
    11/06/2012 - @DPH: V1.07 do not delete CMA_RPT 2 times for the same POOL
    18/06/2012 - @DPH: V1.08 Alimentation de la quantite pour les deals ou ENTITE = CONTREPARTIE
    03/07/2012 - @DPH: V1.09 correction of Patch 536.21. Use DmHistorique instead of BO_LB_FEEMARK
    20/06/2012 - @DPH: V2.01 lot 2. Infine
    01/08/2012 - @DPH: V2.02 BillingExplanation, export excel, error suite Generate
    07/08/2012 - @DPH: V2.03 Deal Inter-Entite
    31/08/2012 - @DPH: V2.04 Zero Amount
    04/09/2012 - @DPH: V2.05 Remove old CMA when Deal was already deleted
    11/09/2012 - @DPH: V2.06 obligations cotées en pourcentage. QC236
    11/09/2012 - @DPH: V2.07 GET_JOURS_OUVRES used in Cash Pool Monitor
    10/10/2012 - @DPH: V2.08 TTF adaptation
    17/10/2012 - @DPH: V2.09 FeePayed, RebatePayed for In Fine. BG FM monthly --> daily 
    18/10/2012 - @DPH: V2.10 problem Workflow_id = -1. RemoveDeal. DealAction
    25/10/2012 - @DPH: V2.11 problem DateVal when update, keep all current data when modify montant 
    31/10/2012 - @DPH: V2.12 Use real date as default date. 
    08/11/2012 - @YO: V2.13 problem Delivery_type=0. 
    14/11/2012 - @DPH: V2.14 OP_START_DATE and OP_ECH_DATE. QC280 
    15/11/2012 - @DPH: V2.15 suppression des tickets. QC281 
    19/11/2012 - @DPH: V2.16 QC282. SM/DT, Optimization. Correction is_cancelled
    21/11/2012 - @DPH: V2.17 OP_ECH_DATE. QC280.
    27/11/2012 - @DPH: V2.18 QC282. rewrite BS_LIB.EXCHANGE_RATE in BG 
    04/12/2012 - @YO: V2.19 mode simulation QC283
    17/01/2013 - @DPH: V2.20 exclude null value from DmHistorique QC292
    29/01/2013 - @DPH: V2.21 problem infinite loop if all cours = null
    13/03/2013 - @DPH: V2.22 problem sauvegarde des ticket pool non mensuel
    14/03/2013 - @YO: V2.23 Source PNL for BOOSTER
    21/03/2013 - @DPH: V2.24 BG sur 3 mois glissants
    18/06/2013 - @DPH: V2.25 p_riskuser is used in IHM
    ******************************************************************************/
    
    PROCEDURE SET_LOG_LEVEL(level int);
    PROCEDURE SET_LOGGER(logger varchar2);
    
    FUNCTION GET_OUTPUT_VALUE(varName varchar2) RETURN varchar2;
    PROCEDURE SET_OUTPUT_VALUE(varName varchar2, val varchar2);

    -- called from Sophis, batch 
    PROCEDURE BILLING_GENERATOR(
        p_source        in  varchar2,
        p_date          in  date,
        p_datefin       in  date default null,
        p_periodicite   in  int default 2, -- 2 = mensuel, 1 = Infine
        p_isFilled      in  boolean default false,
        p_entite        in  HISTOMVTS.ENTITE%TYPE default null,
        p_contrepartie  in  varchar2 default null,
        p_devise        in  varchar2 default null,
        p_perimeterid   in  TITRES.PERIMETERID%TYPE default null,
        p_mvtident      in  HISTOMVTS.MVTIDENT%TYPE default null,
        p_riskuser      in  RISKUSERS.IDENT%TYPE default null,
        p_refcon        in  HISTOMVTS.REFCON%TYPE default null,
        p_mode          in  varchar2 default null);
    
    -- called from IHM de Test, PNL
    PROCEDURE BILLING_GENERATOR(
        p_source        in  varchar2,
        p_date          in  date,
        p_datefin       in  date default null,
        p_periodicite   in  int default null, -- 2 = mensuel, 1 = Infine
        p_mvtident      in  HISTOMVTS.MVTIDENT%TYPE,
        p_riskuser      in  RISKUSERS.IDENT%TYPE default null,
        p_refcon        in  HISTOMVTS.REFCON%TYPE default null,
        p_mode          in  varchar2 default null,
        FeePayed        out number,
        Balance_MinFee  out number,
        TotalFees       out number,
        RebatePayed     out number,
        TotalRebates    out number);
    
        
    /******************************************************************************
    Utilities function
    ******************************************************************************/
    -- log functions
    LOG_ERROR           constant int := 1;
    LOG_WARN            constant int := 2;
    LOG_INFO            constant int := 3;
    LOG_DEBUG           constant int := 4;
    
    -- possible values of p_source
    SRC_FM              constant varchar2(2) := 'FM'; -- fin de mois
    SRC_PI              constant varchar2(2) := 'PI'; -- Pirum
    SRC_T               constant varchar2(2) := 'T';  -- Transactionnel
    SRC_Q               constant varchar2(2) := 'Q';  -- Quotidien
    SRC_I               constant varchar2(2) := 'I';  -- Infine, used in Blotter Kernel contextual menu
    SRC_PNL             constant varchar2(2) := 'B';  -- PNL, used by BOOSTER

    -- possible values of p_periodicite
    PER_M               constant int := 2; -- Mensuel
    PER_I               constant int := 1; -- Infine
    
    --possible values of p_simulation
    MODE_E              constant varchar2(1) := ' ';  -- empty
    MODE_S              constant varchar2(1) := 'S';  -- Simulation
    
    PROCEDURE ERROR(s varchar2,
        p1 varchar2 default null, p2 varchar2 default null, p3 varchar2 default null,
        p4 varchar2 default null, p5 varchar2 default null, p6 varchar2 default null,
        p7 varchar2 default null, p8 varchar2 default null, p9 varchar2 default null,
        p10 varchar2 default null, p11 varchar2 default null, p12 varchar2 default null, p13 varchar2 default null);

    PROCEDURE WARN(s varchar2,
        p1 varchar2 default null, p2 varchar2 default null, p3 varchar2 default null,
        p4 varchar2 default null, p5 varchar2 default null, p6 varchar2 default null,
        p7 varchar2 default null, p8 varchar2 default null, p9 varchar2 default null,
        p10 varchar2 default null, p11 varchar2 default null, p12 varchar2 default null, p13 varchar2 default null);

    PROCEDURE INFO(s varchar2,
        p1 varchar2 default null, p2 varchar2 default null, p3 varchar2 default null,
        p4 varchar2 default null, p5 varchar2 default null, p6 varchar2 default null,
        p7 varchar2 default null, p8 varchar2 default null, p9 varchar2 default null,
        p10 varchar2 default null, p11 varchar2 default null, p12 varchar2 default null, p13 varchar2 default null);

    PROCEDURE DEBUG(s varchar2,
        p1 varchar2 default null, p2 varchar2 default null, p3 varchar2 default null,
        p4 varchar2 default null, p5 varchar2 default null, p6 varchar2 default null,
        p7 varchar2 default null, p8 varchar2 default null, p9 varchar2 default null,
        p10 varchar2 default null, p11 varchar2 default null, p12 varchar2 default null, p13 varchar2 default null);

    FUNCTION FORMAT(s varchar2,
        p1 varchar2 default null, p2 varchar2 default null, p3 varchar2 default null,
        p4 varchar2 default null, p5 varchar2 default null, p6 varchar2 default null,
        p7 varchar2 default null, p8 varchar2 default null, p9 varchar2 default null,
        p10 varchar2 default null, p11 varchar2 default null, p12 varchar2 default null, p13 varchar2 default null)
        RETURN varchar2;

    -- calendar functions
    FUNCTION GET_WEEKDAY_AMERICA(p_date date) RETURN int;
    FUNCTION GET_JOURS_OUVRES(p_date1 date, p_date2 date) RETURN int;
    FUNCTION GET_FIRST_WORKING_DAY(p_date date, p_devise FERIES.CODEDEV%TYPE, p_direction int default 1) RETURN date;
    FUNCTION GET_NEXT_GXML_ID(p_system TS_XML_TBL.SYSTEM%TYPE) RETURN TS_XML_TBL.ID%TYPE;
    PROCEDURE WAIT_FOR_GXML(p_system TS_XML_TBL.SYSTEM%TYPE);

    -- report functions
    FUNCTION RPT_GET_ID_START(id_middle in int default null) RETURN int;
    FUNCTION RPT_GET_ID_END(id_middle in int default null) RETURN int;
    FUNCTION RPT_GET_DATE_START(id_middle in int default null) RETURN date;
    FUNCTION RPT_GET_DATE_END(id_middle in int default null) RETURN date;
    PROCEDURE RPT_PRINT_RUN(id_middle in int default null);
END BL;
/

CREATE OR REPLACE PACKAGE BODY BL
AS
    /******************************************************************************
    PACKAGE BL (BILLING GENERATOR)
      
    27/02/2012 - @DPH: First creation of package
    ******************************************************************************/
    LOG_LEVEL           int := LOG_INFO;
    LOG_LOGGER          BL_LOGS.LOGGER%TYPE := 'BL';
    sid                 varchar2(8);
    gxml_id             int;
    
    -- use a hashtable to store all output parameters
    TYPE MAP_VARCHAR IS TABLE OF VARCHAR2(255) INDEX BY VARCHAR2(20);
    output_params       MAP_VARCHAR;
    
    MIN_DATE            constant date           := to_date('20000101', 'YYYYMMDD');
    MAX_DATE            constant date           := to_date('99991231', 'YYYYMMDD');
    NULL_DATE           constant date           := num_to_date(0);
    TP_BILLING          constant varchar2(10)   := 'Billing';
    TP_FT               constant varchar2(2)    := 'T'; -- dates theoriques
    TP_FR               constant varchar2(2)    := 'R'; -- date reels
    TP_FD               constant varchar2(2)    := TP_FR; -- default value. when release PROD, change this default value
    CMA_FEES            constant int            := 0;
    CMA_REBATES         constant int            := 1;
    OPERATEUR_FM        HISTOMVTS.OPERATEUR%TYPE;
    OPERATEUR_PI        HISTOMVTS.OPERATEUR%TYPE;
    --BO_CONTROLLED       constant HISTOMVTS.BACKOFFICE%TYPE := 1071;
    
    MATH_ROUND          constant int            := 6; -- precision of decimal number
    DATA_ROUND          constant int            := 2; -- precision when stock in database
    
    ERR_TIERSPROPERTIES constant int            := -20500;
    ERR_MVTIDENT        constant int            := -20501;
    ERR_LOCK            constant int            := -20502;
    ERR_USER_NOT_FOUND  constant int            := -20503;
    ERR_SOURCE          constant int            := -20504;
    ERR_RISKUSER        constant int            := -20505;
    ERR_MANY_ROWS       constant int            := -20506;
    ERR_PERIODICITE     constant int            := -20507;
    ERR_MODE            constant int            := -20508;
    ERR_ENTITE          constant int            := -20509;
    ERR_CONTREPARTIE    constant int            := -20510;
    ERR_DEVISE          constant int            := -20511;
    ERR_CONVENTION      constant int            := -20512;

    GXML_SYSTEM         constant TS_XML_TBL.SYSTEM%TYPE := 'BillingGenerator';
    GXML_EVENT_CA       constant BO_KERNEL_EVENTS.NAME%TYPE := 'New deal accept';
    GXML_EVENT_CP       constant BO_KERNEL_EVENTS.NAME%TYPE := 'New deal pending';
    GXML_EVENT_UPDATE   constant BO_KERNEL_EVENTS.NAME%TYPE := 'Transmit Modification';
    GXML_EVENT_CANCEL   constant BO_KERNEL_EVENTS.NAME%TYPE := 'Transmit Deletion';
    GXML_ACTION_CREATE  constant varchar2(6)    := 'CREATE';
    GXML_ACTION_UPDATE  constant varchar2(6)    := 'UPDATE';
    GXML_ACTION_CANCEL  constant varchar2(6)    := 'CANCEL';
    GXML_FORMAT_DATE    constant varchar2(10) := 'YYYY-MM-DD';
    GXML_FORMAT_NUM     constant varchar2(20) := '9999999999.99';
    GXML_ECN            constant HISTOMVTS.ECN%TYPE := 'BG';
    DATE_DEBUT_PNL      constant date           := MIN_DATE;
    DATE_FIN_PNL        constant date           := trunc(sysdate);

    -- global parameters    
    GP_SOURCE           varchar2(2);
    GP_PERIODICITE      int := PER_M;
    GP_RISKUSER         RISKUSERS.IDENT%TYPE := NULL;               
    GP_MODE             varchar2(1);
    
    TYPE RATE_INFO IS RECORD (
        dateval         date,
        rate            HISTOMVTS.COURS%TYPE
    );
    
    TYPE DATE_NUM IS RECORD(
        dt              date,
        num             number
    );
    
    /*    
    TYPE FEE_INFO IS RECORD (
        "from"          date,
        "to"            date,
        qty             HISTOMVTS.QUANTITE%TYPE,
        prix            HISTOMVTS.COURS%TYPE,
        taux_change     DMHISTORIQUE.LAST%TYPE,
        taux_com        HISTOMVTS.COMMISSION%TYPE,
        days            int,
        montant_fee     number
    );
    TYPE FEE_LIST IS VARRAY(31) OF FEE_INFO;
    */
    TYPE FEE_LIST IS VARRAY(3000) OF BL_FEES%ROWTYPE;
    TYPE REBATE_POOL_LIST IS VARRAY(3000) OF BL_REBATES_POOL%ROWTYPE;
    TYPE REBATE_HORS_POOL_LIST IS VARRAY(3000) OF BL_REBATES_HORS_POOL%ROWTYPE;
    
    FUNCTION GET_OUTPUT_VALUE(varName varchar2)
    RETURN varchar2 AS
    BEGIN
        return output_params(varName);
    exception
        when no_data_found then
            return null;
    END;
    
    PROCEDURE SET_OUTPUT_VALUE(varName varchar2, val varchar2)
    AS
    BEGIN
        output_params(varName) := val;
    END;
    
    FUNCTION ROUND_DATA(n number)
    RETURN number AS
    BEGIN
        -- do Round 2 times, to avoid the case
        -- select round(round(1.5249, 3), 2), round(1.5249, 2) from dual
        return round(round(n, DATA_ROUND+1), DATA_ROUND);
    END; 
    
    FUNCTION GET_NEXT_GXML_ID(p_system TS_XML_TBL.SYSTEM%TYPE)
    RETURN TS_XML_TBL.ID%TYPE
    AS
        GXML_ID_FORMAT      constant varchar2(15) := '099999999999';
    BEGIN
        -- get the next gxml_id
        if gxml_id is null then
            begin
                SELECT TO_NUMBER(MAX(ID))
                    into gxml_id
                FROM TS_XML_TBL WHERE SYSTEM = p_system AND ID LIKE '0%';
            exception
                when no_data_found then
                    gxml_id := 0;
            end;
            output_params('BeginGXMLId') := trim(to_char(nvl(gxml_id, 0) + 1, GXML_ID_FORMAT));
        end if;
        gxml_id := nvl(gxml_id, 0) + 1;
        output_params('EndGXMLId') := trim(to_char(gxml_id, GXML_ID_FORMAT));
        return output_params('EndGXMLId');
    EXCEPTION
        WHEN OTHERS THEN
            ERROR('GET_NEXT_GXML_ID. code = {1}, message = {2}, backtrace={3}', SQLCODE, SQLERRM, DBMS_UTILITY.FORMAT_ERROR_BACKTRACE);
            RAISE;
    END;
    
    PROCEDURE LOCK_VERIFY
    AS
        rc      number;
    BEGIN
        SELECT COUNT(*)
            into rc
        FROM CMA_RPT_OPERATIONS WHERE OP_ID IN (-1, -2);
        if rc > 0 then
            raise_application_error(ERR_LOCK, 'An other batch is running. Try again later!');
        end if;
    EXCEPTION
        WHEN OTHERS THEN
            ERROR('LOCK_VERIFY. code = {1}, message = {2}, backtrace={3}', SQLCODE, SQLERRM, DBMS_UTILITY.FORMAT_ERROR_BACKTRACE);
            RAISE;
    END;
    
    PROCEDURE LOCK_BATCH
    AS
    BEGIN
        INSERT INTO CMA_RPT_OPERATIONS (OP_ID, DTGEN) VALUES (-2, SYSDATE);
        COMMIT;
    EXCEPTION
        WHEN OTHERS THEN
            ERROR('LOCK_BATCH. code = {1}, message = {2}, backtrace={3}', SQLCODE, SQLERRM, DBMS_UTILITY.FORMAT_ERROR_BACKTRACE);
            RAISE;
    END;
    
    PROCEDURE UNLOCK_BATCH
    AS
    BEGIN
        DELETE CMA_RPT_OPERATIONS WHERE OP_ID = -2;
        COMMIT;
    EXCEPTION
        WHEN OTHERS THEN
            ERROR('UNLOCK_BATCH. code = {1}, message = {2}, backtrace={3}', SQLCODE, SQLERRM, DBMS_UTILITY.FORMAT_ERROR_BACKTRACE);
            RAISE;
    END;
    
    PROCEDURE INSERT_GXML_FLUX(
        p_id            in  varchar2,
        flux            in  XMLType)
    AS
        gxml_id_str     TS_XML_TBL.ID%TYPE;
    BEGIN
        gxml_id_str := GET_NEXT_GXML_ID(GXML_SYSTEM);
        INSERT INTO TS_XML_TBL(ID, SYSTEM, XML, ISDONE, DATEINSERT)
        VALUES (gxml_id_str, GXML_SYSTEM, flux.getClobVal(), 0, sysdate);
                
        INFO('{1} was inserted into GXML with id = {2}. rowcount = {3}', p_id, gxml_id_str, SQL%ROWCOUNT);
    EXCEPTION
        WHEN OTHERS THEN
            ERROR('INSERT_GXML_FLUX(p_id={4}). code = {1}, message = {2}, backtrace={3}', SQLCODE, SQLERRM, DBMS_UTILITY.FORMAT_ERROR_BACKTRACE, p_id);
            RAISE;
    END;
    
    PROCEDURE GXML_CANCEL(
        p_id            in  varchar2,
        p_refcon        in  HISTOMVTS.REFCON%TYPE)
    AS
        uid_            varchar2(50);
        result          XMLType;
        gxml_id_str     TS_XML_TBL.ID%TYPE;
    BEGIN
        uid_ := p_id || '_' || to_char(sysdate, 'YYYYMMDDHH24MISS');
        INFO('GXML_CANCEL({1})', uid_);
        
        -- pour le traitement quotidien, on verifie chaque fois si le flux est déjà envoyé
        -- afin d'éviter le fait que GXML n'est pas UP et on envoie en doublon
        -- pour le traitement mensuel, on ne fait pas cette vérification car c'est très couteux
        -- (extraction xml à partir de BLOB coute chère)
        if GP_SOURCE = SRC_Q and GP_PERIODICITE = PER_I then
            begin
                SELECT ID
                    into gxml_id_str
                FROM TS_XML_TBL WHERE SYSTEM = GXML_SYSTEM AND ISDONE = 0
                    AND EXTRACTVALUE(XMLTYPE(XML), '/Transaction/ACTION') = GXML_ACTION_CANCEL 
                    AND INSTR(EXTRACTVALUE(XMLTYPE(XML), '/Transaction/ID'), p_id || '_') = 1;
                    
                ERROR('REPORT. Flux {1}({2},{3}) was already inserted, and is waiting. Do not insert one more time.', gxml_id_str, p_id, GXML_ACTION_CANCEL);
                return;
            exception
                WHEN NO_DATA_FOUND THEN
                    null; -- no error, do nothing
            end;
        end if;
        
        SELECT XMLRoot(XMLElement("Transaction",
                XMLAttributes(  'http://www.w3.org/2001/XMLSchema-instance' AS "xmlns:xsi",
                                'http://www.w3.org/2001/XMLSchema' AS "xmlns:xsd"),
            XMLForest(
                uid_                ID,
                GXML_EVENT_CANCEL   "WorkflowEvent",
                GXML_ACTION_CANCEL  "ACTION",
                p_refcon            "REFCON")),
            VERSION '1.0')
        into result FROM DUAL;
        
        INSERT_GXML_FLUX(uid_, result);
    EXCEPTION
        WHEN OTHERS THEN
            ERROR('GXML_CANCEL(p_id={4}, p_refcon={5}). code = {1}, message = {2}, backtrace={3}', SQLCODE, SQLERRM, DBMS_UTILITY.FORMAT_ERROR_BACKTRACE, p_id, p_refcon);
            RAISE;
    END;
    
    PROCEDURE GXML_CREATE_UPDATE(
        p_id            in  varchar2,
        HH              in  HISTOMVTS%ROWTYPE,
        p_action        in  varchar2,
        p_workflowEvent in  BO_KERNEL_EVENTS.NAME%TYPE)
    AS
        uid_            varchar2(100);
        result          XMLType;
        xml_id          TS_XML_TBL.ID%TYPE;
        xml_cours       varchar2(20);
        xml_montant     varchar2(20);
    BEGIN
        uid_ := p_id || '_' || to_char(sysdate, 'YYYYMMDDHH24MISS');
        INFO('GXML_CREATE_UPDATE({1}, {2}, {3})', uid_, p_action, p_workflowEvent);
        
        -- pour le traitement quotidien, on verifie chaque fois si le flux est déjà envoyé
        -- afin d'éviter le fait que GXML n'est pas UP et on envoie en doublon
        -- pour le traitement mensuel, on ne fait pas cette vérification car c'est très couteux
        -- (extraction xml à partir de BLOB coute chère)
        if GP_SOURCE = SRC_Q and GP_PERIODICITE = PER_I then
            begin
                SELECT ID
                    into xml_id
                FROM TS_XML_TBL WHERE SYSTEM = GXML_SYSTEM AND ISDONE = 0
                    AND EXTRACTVALUE(XMLTYPE(XML), '/Transaction/ACTION') = p_action 
                    AND INSTR(EXTRACTVALUE(XMLTYPE(XML), '/Transaction/ID'), p_id || '_') = 1;
                    
                ERROR('REPORT. Flux {1}({2},{3}) was already inserted, and is waiting. Do not insert one more time.', xml_id, p_id, p_action);
                return;
            exception
                WHEN NO_DATA_FOUND THEN
                    null; -- no error, do nothing
            end;
        end if;
        
        xml_cours := TRIM(TO_CHAR(ROUND_DATA(HH.COURS), GXML_FORMAT_NUM));
        xml_montant := TRIM(TO_CHAR(ROUND_DATA(HH.MONTANT), GXML_FORMAT_NUM));
        
        SELECT XMLRoot(XMLElement("Transaction",
                XMLAttributes(  'http://www.w3.org/2001/XMLSchema-instance' AS "xmlns:xsi",
                                'http://www.w3.org/2001/XMLSchema' AS "xmlns:xsd"),
            XMLForest(
                uid_                ID,
                p_workflowEvent     "WorkflowEvent",
                p_action            "ACTION",
                HH.REFCON          "REFCON",
                HH.OPCVM            OPCVM,
                HH.SICOVAM          SICOVAM,
                TO_CHAR(HH.DATENEG, GXML_FORMAT_DATE) DATENEG,
                TO_CHAR(HH.DATEVAL, GXML_FORMAT_DATE) DATEVAL,
                HH.TYPE             TYPE,
                case when HH.ENTITE = HH.CONTREPARTIE
                    or GP_PERIODICITE = PER_I then HH.QUANTITE else NULL end QUANTITE, -- QUANTITE and COURS are determined by Sophis 
                case when HH.ENTITE = HH.CONTREPARTIE
                    or GP_PERIODICITE = PER_I then xml_cours else NULL end COURS, -- QUANTITE and COURS are determined by Sophis
                xml_montant MONTANT,
                NULL                COUPON,
                HH.INFOS            INFOS,
                HH.MVTIDENT         MVTIDENT,
                NULL                HEURENEG,
                NULL                COURTIER,
                HH.DEPOSITAIRE      DEPOSITAIRE,
                NULL                AJUSTEMENT,
                NULL                TYPESICO,
                HH.CONTREPARTIE     CONTREPARTIE,
                HH.OPERATEUR        OPERATEUR,
                --BO_CONTROLLED  BACKOFFICE, -- determined by Sophis
                NULL                INFOSBACKOFFICE,
                NULL                FRAISMARCHE,
                NULL                FRAISCOURTAGE,
                NULL                TRANSBACK,
                NULL                REFERENCE,
                NULL                BRUT_NET,
                NULL                CMPT_ORDRE,
                NULL                MONTANTCOURU, -- sera renseigné pour LOT2
                NULL                TYPECOURS,
                NULL                DATECOUPON,
                NULL                DATEJOUISSANCE,
                NULL                MONTANTCOURU2, -- sera renseigné pour LOT2
                HH.DEVISEPAY        DEVISEPAY,
                1                   TAUXCHANGE,
                NULL                DATECOMPTABLE,
                NULL                REFMVTBACK,
                NULL                TAXTRANSACTION,
                NULL                TAXIMPOT,
                NULL                CONTREPARTIE2,
                HH.ENTITE           ENTITE,
                NULL                CERTAIN,
                2                   CREATION,
                NULL                COURSDEVPAY,
                NULL                CDC_ENVOI,
                NULL                REFGRAPPAGE,
                NULL                ARCHIVE,
                NULL                WORKFLOW_ID,
                NULL                DELIVERY_TYPE,
                case when GP_PERIODICITE = PER_I then
                        NVL((SELECT NAME FROM BO_CASH_WORKFLOW WHERE ID = HH.WORKFLOW_ID), 'N/A') || ' / ' ||
                        decode(HH.DELIVERY_TYPE, -1, 'All', 1, 'DVP', 2, 'FOP', 'N/A')
                    when GP_PERIODICITE = PER_M then 'N/A / N/A' end SMDT, -- Mensuel: QC282. for all monthly billing ticket
                NULL                FRAISCOUNTERPARTY,
                NULL                FIXING_TYPE,
                NULL                COMMISSION,
                NULL                COMMISSION_DATE,
                NULL                BACK2BACK,
                NULL                LOOKLIKE,
                GXML_ECN            ECN, --hard code in GXML. BG by default.
                NULL                TRADEID_ECN, -- id venant du GXML
                NULL                TRADEVERSIONID_ECN,
                case when GP_PERIODICITE = PER_I then HH.PAYMENT_METHOD
                    when GP_PERIODICITE = PER_M then NULL end PAYMENT_METHOD, -- Mensuel: determined by Sophis
                NULL                DELIVERY_DATE,
                NULL                NXS_CUSTOMER,
                NULL                FORCELOAD,
                NULL                SALES,
                NULL                PRODUCT_AXIS,
                NULL                CLIENT_AXIS,
                NULL                RC,
                NULL                CX,
                NULL                SC,
                NULL                RFQ_ID,
                NULL                RFQ_TRADEID,
                --1                   CHECKBROKERFEES, --hard code in GXML
                --1                   CHECKMARKETFEES, --hard code in GXML
                --1                   CHECKCOUNTERFEES, --hard code in GXML
                NULL                ENTRY_DATE,
                NULL                CASH_DEPOSITARY,
                NULL                TEMPLATE_ID,
                --HH.DEPOSITARY_OF_THE_COUNTERPART DEPOSITARY_OF_THE_COUNTERPART, -- determined by Sophis
                NULL                MIRROR_REFERENCE,
                --1                   VERSION, -- determined by Sophis
                NULL                NXS_COMMENT,
                NULL                EVG_DURATION,
                NULL                NXS_PREPAID,
                NULL                REAL_DATEVAL,
                NULL                REAL_DELIVERY_DATE)),
            VERSION '1.0')
        into result FROM DUAL;
            
        INSERT_GXML_FLUX(uid_, result);
    EXCEPTION
        WHEN OTHERS THEN
            ERROR('GXML_CREATE_UPDATE(p_id={4}, p_action={5}, p_workflowEvent={6}). code = {1}, message = {2}, backtrace={3}', SQLCODE, SQLERRM, DBMS_UTILITY.FORMAT_ERROR_BACKTRACE, p_id, p_action, p_workflowEvent);
            RAISE;
    END;
    
    FUNCTION GET_FIRST_WORKING_DAY(
        p_date          in  date,
        p_devise        in  FERIES.CODEDEV%TYPE,
        p_direction     in  int default 1)
    RETURN date
    AS
        result          date := p_date;
        rc              int;
    BEGIN
        --TODO: create a cache here to make more performance 
        loop
            if GET_WEEKDAY_AMERICA(result) not in (7, 1) then -- not samedi, dimanche
                SELECT COUNT(*)
                    into rc
                FROM FERIES F WHERE F.CODEDEV = p_devise AND F.DATEFER = result;
                exit when rc = 0;
            end if;
            result := result + p_direction;
        end loop;
        return result;
    EXCEPTION
        WHEN OTHERS THEN
            ERROR('GET_FIRST_WORKING_DAY(p_date={4}, p_devise={5}, p_direction={6}). code = {1}, message = {2}, backtrace={3}',
                SQLCODE, SQLERRM, DBMS_UTILITY.FORMAT_ERROR_BACKTRACE, p_date, p_devise, p_direction);
            RAISE;
    END;
    
    PROCEDURE SEND_TO_GXML(
        p_refcon        in  HISTOMVTS.REFCON%TYPE, -- refcon de la MEP, ADM
        p_id            in  varchar2,
        p_type          in  HISTOMVTS.TYPE%TYPE,
        p_dateneg       in  date, -- Menusel dateneg = FinMen, Infine dateneg = dateneg du closing
        p_dateval       in  date, -- Menusel dateval = FinMen, Infine dateval = DateSitu (dateval du closing)
        p_montant       in  number)
    AS
        TYPE AGREEMENT_INFO IS RECORD (
            refcon                  HISTOMVTS.REFCON%TYPE, -- refcon du ticket 7, 101 s'il est déjà créé auparavant
            mvtident                HISTOMVTS.MVTIDENT%TYPE,
            montant                 HISTOMVTS.MONTANT%TYPE,
            is_cpty_internal        int,
            is_cancelled            int,
            backoffice              HISTOMVTS.BACKOFFICE%TYPE,
            is_pool                 int,
            is_pending              int,
            perimeterid             TITRES.PERIMETERID%TYPE,
            devisectt               TITRES.DEVISECTT%TYPE,
            taux_var                TITRES.TAUX_VAR%TYPE
        );
        a                       AGREEMENT_INFO;
        result                  XMLType;
        HH                      HISTOMVTS%ROWTYPE;
        mvtident_mirror         HISTOMVTS.MVTIDENT%TYPE;
        refcon_mirror           HISTOMVTS.REFCON%TYPE;
        id_mirror               varchar2(50);
        strMsg                  varchar2(1000);
        xml_montant             HISTOMVTS.MONTANT%TYPE;
    BEGIN
        INFO('SEND_TO_GXML(refcon={1}, id={2}, type={3}, dateneg={4}, dateval={5}, montant={6})',
            p_refcon, p_id, p_type, p_dateneg, p_dateval, p_montant);
            
        -- get devisepay and dateval            
        SELECT CASE WHEN p_type = 101 THEN T.DEVISECTT
                WHEN T.DEVISEAC IS NULL OR T.DEVISEAC = 0 THEN H.DEVISEPAY ELSE T.DEVISEAC END
            into HH.DEVISEPAY
        FROM HISTOMVTS H
            JOIN TITRES T ON T.SICOVAM = H.SICOVAM
        WHERE H.REFCON = p_refcon; -- refcon de la MEP
        
        if GP_PERIODICITE = PER_M then
            HH.DATEVAL := GET_FIRST_WORKING_DAY(p_dateval, HH.DEVISEPAY);
        elsif GP_PERIODICITE = PER_I then
            HH.DATEVAL := p_dateval;
        end if;
        
        SELECT H.OPCVM, H.SICOVAM, HC.WORKFLOW_ID, HC.DELIVERY_TYPE, HC.PAYMENT_METHOD,
                CASE WHEN T.TYPE = 'C' AND T.MODELE = 'Collateral' THEN 0 ELSE  
                    (SELECT NVL(SUM(H1.QUANTITE), 0) FROM HISTOMVTS H1
                        JOIN BUSINESS_EVENTS BE ON BE.ID = H1.TYPE AND BE.COMPTA = 1
                    WHERE H1.MVTIDENT = H.MVTIDENT AND H1.DATEVAL <= HH.DATEVAL) end,
                p_type, p_dateneg, p_montant,
                CASE GP_SOURCE WHEN SRC_FM THEN OPERATEUR_FM
                    WHEN SRC_Q  THEN OPERATEUR_FM
                    ELSE NULL END,
                H.MVTIDENT, H.DEPOSITAIRE, H.CONTREPARTIE, H.ENTITE, H.MIRROR_REFERENCE,
                CASE WHEN T.TYPE = 'C' AND T.MODELE = 'Collateral' THEN 1 ELSE 0 END,
                CASE WHEN GP_PERIODICITE = PER_I AND HC.BACKOFFICE IN (
                    SELECT KSC.KERNEL_STATUS_ID
                    FROM BO_KERNEL_STATUS_COMPONENT KSC
                        JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                            AND KSG.NAME IN ('_STEP_Pending') AND KSG.RECORD_TYPE = 1)
                THEN 1 ELSE 0 END,
                T.PERIMETERID, T.DEVISECTT, T.TAUX_VAR
            into HH.OPCVM, HH.SICOVAM, HH.WORKFLOW_ID, HH.DELIVERY_TYPE, HH.PAYMENT_METHOD,
                HH.QUANTITE, HH.TYPE, HH.DATENEG, HH.MONTANT, HH.OPERATEUR,
                HH.MVTIDENT, HH.DEPOSITAIRE, HH.CONTREPARTIE, HH.ENTITE, HH.MIRROR_REFERENCE,
                a.is_pool, a.is_pending, a.perimeterid, a.devisectt, a.taux_var
        FROM HISTOMVTS H
            JOIN TITRES T ON T.SICOVAM = H.SICOVAM
            LEFT JOIN HISTOMVTS HC ON HC.MVTIDENT = H.MVTIDENT AND HC.TYPE IN (102, 501)
                AND HC.BACKOFFICE NOT IN (
                    SELECT KSC.KERNEL_STATUS_ID
                    FROM BO_KERNEL_STATUS_COMPONENT KSC
                        JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                            AND KSG.NAME IN ('_STEP_Cancelled', '_MOD_Cancelled') AND KSG.RECORD_TYPE = 1)
                AND GP_PERIODICITE = PER_I AND HC.DATEVAL = p_dateval
        WHERE H.REFCON = p_refcon; -- refcon de la MEP
        
        xml_montant := ROUND_DATA(HH.MONTANT);
        HH.COURS := case HH.QUANTITE when 0 then 0 else HH.MONTANT/HH.QUANTITE end;
        
        -- is a BORROW inter-entite. Make negative the QUANTITE and COURS
        if HH.ENTITE = HH.CONTREPARTIE and HH.QUANTITE > 0 then
            HH.QUANTITE := -HH.QUANTITE;
            HH.COURS := -HH.COURS;
        end if;
        
        -- cherche si le deal existe
        a.refcon := null;
        loop
        begin
            SELECT H.REFCON, H.MVTIDENT, H.MONTANT, IS_CPTY_INTERNAL(H.CONTREPARTIE, H.ENTITE),
                CASE WHEN H.BACKOFFICE NOT IN (
                    SELECT KSC.KERNEL_STATUS_ID
                    FROM BO_KERNEL_STATUS_COMPONENT KSC
                        JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                            AND KSG.NAME IN ('_STEP_Settled') AND KSG.RECORD_TYPE = 1)
                THEN 1 ELSE 0 END, H.BACKOFFICE
                into a.refcon, a.mvtident, a.montant, a.is_cpty_internal, a.is_cancelled, a.backoffice
            FROM HISTOMVTS H
            WHERE ((GP_PERIODICITE = PER_M AND H.TYPE = p_type AND H.DATENEG = p_dateneg
                        AND H.BACKOFFICE IN (
                            SELECT KSC.KERNEL_STATUS_ID FROM BO_KERNEL_STATUS_COMPONENT KSC
                                JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                                AND KSG.RECORD_TYPE = 1 AND KSG.NAME = 'All But Pending FO'))
                    OR (GP_PERIODICITE = PER_I AND ((p_type in (7, 700) AND H.TYPE IN (7, 700))
                                                OR (p_type in (101, 701) AND H.TYPE IN (101, 701)))
                        --AND H.DATEVAL = p_dateval
                        AND H.DATENEG = p_dateneg
                        AND H.BACKOFFICE NOT IN (
                            SELECT KSC.KERNEL_STATUS_ID
                            FROM BO_KERNEL_STATUS_COMPONENT KSC
                                JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                                    AND KSG.NAME IN ('_STEP_Cancelled', '_MOD_Cancelled') AND KSG.RECORD_TYPE = 1)))
                AND ((a.is_pool = 0 AND H.MVTIDENT = HH.MVTIDENT) OR
                     (a.is_pool = 1 AND H.MVTIDENT IN (
                        SELECT MVTIDENT FROM BL_CONTRACTS
                        WHERE PERIMETERID = a.perimeterid AND DEVISECTT = a.devisectt AND TAUX_VAR = a.taux_var
                            AND ENTITE = HH.ENTITE AND CONTREPARTIE = HH.CONTREPARTIE)))
                        --SELECT DISTINCT HM.MVTIDENT FROM HISTOMVTS HM
                        --    JOIN TITRES T ON T.SICOVAM = HM.SICOVAM
                        --        AND T.TYPE = 'C' AND T.MODELE = 'Collateral' AND T.AFFECTATION IN (11, 60)
                        --        AND T.PERIMETERID = a.perimeterid AND T.DEVISECTT = a.devisectt AND T.TAUX_VAR = a.taux_var
                        --WHERE HM.TYPE = 16
                        --    AND HM.ENTITE = HH.ENTITE AND HM.CONTREPARTIE = HH.CONTREPARTIE))) 
                AND (a.refcon IS NULL OR H.REFCON = a.refcon);
            exit;
        exception
            when NO_DATA_FOUND then
                a.refcon := NULL;
                exit;
            when TOO_MANY_ROWS then
                if a.is_pool = 1 then
                    strMsg := FORMAT('Found multiple ticket in pool of refcon {1}, dateneg = {2}, last refcon = {3} --> reject',
                        p_refcon, p_dateneg, a.refcon);
                elsif GP_PERIODICITE = PER_M then
                    strMsg := FORMAT('Found multiple ticket in position {1}, type = {2}, dateneg = {3}, last refcon = {4} --> reject',
                        HH.MVTIDENT, p_type, p_dateneg, a.refcon);
                elsif GP_PERIODICITE = PER_I then
                    strMsg := FORMAT('Found multiple ticket in position {1}, type = {2}, dateval = {3}, last refcon = {4} --> reject',
                        HH.MVTIDENT, p_type, p_dateval, a.refcon);
                end if;
                WARN(strMsg);
                raise_application_error(ERR_MANY_ROWS, strMsg);
                -- the following query does not execute
                -- il will be moved to the begin of block when no error raised
                SELECT MAX(H.REFCON)
                    into a.refcon
                FROM HISTOMVTS H
                WHERE ((GP_PERIODICITE = PER_M AND H.TYPE = p_type AND H.DATENEG = p_dateneg
                            AND H.BACKOFFICE IN (
                                SELECT KSC.KERNEL_STATUS_ID FROM BO_KERNEL_STATUS_COMPONENT KSC
                                    JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                                    AND KSG.RECORD_TYPE = 1 AND KSG.NAME = 'All But Pending FO'))
                        OR (GP_PERIODICITE = PER_I AND ((p_type in (7, 700) AND H.TYPE IN (7, 700))
                                                    OR (p_type in (101, 701) AND H.TYPE IN (101, 701)))
                            AND H.BACKOFFICE NOT IN (
                                SELECT KSC.KERNEL_STATUS_ID
                                FROM BO_KERNEL_STATUS_COMPONENT KSC
                                    JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                                        AND KSG.NAME IN ('_STEP_Cancelled', '_MOD_Cancelled') AND KSG.RECORD_TYPE = 1)))
                    AND ((a.is_pool = 0 AND H.MVTIDENT = HH.MVTIDENT) OR
                         (a.is_pool = 1 AND H.MVTIDENT IN (
                            SELECT MVTIDENT FROM BL_CONTRACTS
                            WHERE PERIMETERID = a.perimeterid AND DEVISECTT = a.devisectt AND TAUX_VAR = a.taux_var
                            AND ENTITE = HH.ENTITE AND CONTREPARTIE = HH.CONTREPARTIE)));
                            --SELECT DISTINCT HM.MVTIDENT FROM HISTOMVTS HM
                            --    JOIN TITRES T ON T.SICOVAM = HM.SICOVAM
                            --        AND T.TYPE = 'C' AND T.MODELE = 'Collateral' AND T.AFFECTATION IN (11, 60)
                            --        AND T.PERIMETERID = a.perimeterid AND T.DEVISECTT = a.devisectt AND T.TAUX_VAR = a.taux_var
                            --WHERE HM.TYPE = 16
                            --    AND HM.ENTITE = HH.ENTITE AND HM.CONTREPARTIE = HH.CONTREPARTIE)));
        end;
        end loop;
        
        DEBUG('ticket refcon = {1}', a.refcon);
        
        if a.refcon is null then
            if GP_MODE = MODE_S then  --MODE SIMULATION
                INFO('SIMULATION. provenance=C; p_refcon={1}; devisepay={2}; type={3}; montant={4};',
                    p_refcon, HH.DEVISEPAY, p_type, TRIM(TO_CHAR(ROUND_DATA(p_montant), '999999999D99', 'NLS_NUMERIC_CHARACTERS = '',.''')));
            elsif trunc(p_dateneg, 'MM') = trunc(sysdate, 'MM') then -- do not verify audit_mvt in FACTU end of month
                DEBUG('End of month --> Create deal always. mvtident = {1}', HH.MVTIDENT);
                GXML_CREATE_UPDATE(p_id, HH, GXML_ACTION_CREATE,
                    case when a.is_pending = 1 then GXML_EVENT_CP else GXML_EVENT_CA end); -- create deal
            else   
                begin
                    SELECT AU.REFCON
                        into a.refcon
                    FROM AUDIT_MVT AU
                        JOIN AUDIT_MVT AM1 ON AM1.REFCON = AU.REFCON
                            AND AM1.VERSION = AU.VERSION - 1
                            AND AM1.USERID != 1
                    WHERE AU.MVTIDENT = HH.MVTIDENT AND AU.TYPE = HH.TYPE
                        AND AU.DATENEG = HH.DATENEG AND AU.MONTANT = xml_montant
                        AND AU.BACKOFFICE = 1181;
                    INFO('Deal already created/deleted. --> do not recreate it. refcon = {1}', a.refcon);
                exception
                    when no_data_found then
                        DEBUG('Deal does not exist. Create it. mvtident = {1}', HH.MVTIDENT);
                        GXML_CREATE_UPDATE(p_id, HH, GXML_ACTION_CREATE,
                            case when a.is_pending = 1 then GXML_EVENT_CP else GXML_EVENT_CA end); -- create deal
                    when too_many_rows then
                        INFO('More than one deal already created/deleted. --> do not recreate it.');
                end;
            end if;
            
            /*
            -- Gestion des miroirs
            if HH.MIRROR_REFERENCE = -1 then
            begin
                SELECT H2.MVTIDENT
                    into mvtident_mirror
                FROM HISTOMVTS H2
                WHERE H2.MIRROR_REFERENCE = p_refcon;
            exception
                when NO_DATA_FOUND then
                    WARN('Mirroring impossible pour refcon {1}', p_refcon);
            end;
            DEBUG('mvtident_mirror = {1}', mvtident_mirror);
            
            id_mirror := p_id || '_M';
            HH.MVTIDENT := mvtident_mirror;
            HH.QUANTITE := -HH.QUANTITE;
            HH.MONTANT := -HH.MONTANT;
            
            GXML_CREATE(id_mirror, HH);
            
            end if;*/
        elsif GP_PERIODICITE != PER_I then -- not Infine             
            -- in the case of POOL, mvtident may be changed because it take MAX(mvtident) of POOL
            if a.mvtident = HH.MVTIDENT then
                if ROUND_DATA(a.montant) != ROUND_DATA(HH.MONTANT) then
                    if GP_MODE = MODE_S then  --MODE SIMULATION
                        INFO('SIMULATION. provenance=M; refcon={1}; devisepay={2}; type={3}; montant={4};',
                            a.refcon, HH.DEVISEPAY, p_type, TRIM(TO_CHAR(ROUND_DATA(p_montant), '999999999D99', 'NLS_NUMERIC_CHARACTERS = '',.''')));
                    else
                        DEBUG('old montant = {1}, montant = {2}', a.montant, HH.MONTANT);
                        if (a.is_cpty_internal = 1 and a.is_cancelled = 0) or a.backoffice = 1071 then -- CONTROLLED
                            HH.REFCON := a.refcon;
                            HH.INFOS := 'Deal modified through GXML';
                            
                            -- keep all current data
                            SELECT H.TYPE, H.DATENEG, H.DATEVAL, H.OPCVM, H.SICOVAM, H.ENTITE, H.CONTREPARTIE,
                                H.MVTIDENT, H.DEPOSITAIRE, H.WORKFLOW_ID, H.DELIVERY_TYPE, H.PAYMENT_METHOD  
                                into HH.TYPE, HH.DATENEG, HH.DATEVAL, HH.OPCVM, HH.SICOVAM, HH.ENTITE, HH.CONTREPARTIE,
                                HH.MVTIDENT, HH.DEPOSITAIRE, HH.WORKFLOW_ID, HH.DELIVERY_TYPE, HH.PAYMENT_METHOD
                            FROM HISTOMVTS H WHERE H.REFCON = HH.REFCON;
                            
                            GXML_CREATE_UPDATE(p_id, HH, GXML_ACTION_UPDATE, GXML_EVENT_UPDATE); -- update deal
                        else
                            DEBUG('Deal {1} existe but with is_cpty_internal = {2}, backoffice = {3}). --> No action',
                                a.refcon, a.is_cpty_internal, a.backoffice);
                        end if;
                    end if;
                else
                    DEBUG('Deal {1} existe but same montant. --> No action', a.refcon);
                end if;
            else
                if GP_MODE = MODE_S then   --MODE SIMULATION
                    INFO('SIMULATION. provenance=C; p_refcon={1}; devisepay={2}; type={3}; montant={4};',
                        p_refcon, HH.DEVISEPAY, p_type, TRIM(TO_CHAR(ROUND_DATA(p_montant), '999999999D99', 'NLS_NUMERIC_CHARACTERS = '',.''')));
                else
                    if a.backoffice = 1071 then -- CONTROLLED
                        DEBUG('Deal exist mvtident={1}, new mvtident = {2}. Cancel the old one and create a new.', a.mvtident, HH.MVTIDENT);
                        GXML_CANCEL(FORMAT('BG_{1}', a.refcon), a.refcon);
                        GXML_CREATE_UPDATE(p_id, HH, GXML_ACTION_CREATE,
                            case when a.is_pending = 1 then GXML_EVENT_CP else GXML_EVENT_CA end); -- create deal
                    else
                        DEBUG('Deal exist mvtident={1}, new mvtident = {2}, but backoffice = {3}. --> No action',
                            a.mvtident, HH.MVTIDENT, a.backoffice);
                    end if;
                end if;                 
            end if;
            DELETE BL_OPERATIONS WHERE TA_REFCON = a.refcon;
            DEBUG('Deal exist OK --> to be retained', a.refcon);
        else
            DEBUG('Deal exist but Infine --> do nothing');
        end if;
    EXCEPTION
        WHEN OTHERS THEN
            ERROR('REPORT. Preparing GXML error(p_refcon={4}, p_id={5}). code = {1}, message = {2}, backtrace={3}',
                SQLCODE, SQLERRM, DBMS_UTILITY.FORMAT_ERROR_BACKTRACE, p_refcon, p_id);
            -- do not raise error in these case
            if SQLCODE not in (ERR_MANY_ROWS) then
                RAISE;
            end if;
    END;
    
    PROCEDURE WAIT_FOR_GXML(p_system TS_XML_TBL.SYSTEM%TYPE)
    AS
        gxml_try_number     int;
        gxml_count          int;
        gxml_last_count     int;
        sleep               int;
        GXML_WAIT_TRY           constant int := 30;
        GXML_WAIT_SMALL         constant int := 4;
        GXML_WAIT_BIG           constant int := 10;
    BEGIN
        INFO('Waiting for GXML result...');
        gxml_try_number := 0;
        gxml_count := 0;
        loop
            gxml_last_count := gxml_count;
            SELECT COUNT(*)
                into gxml_count
            FROM TS_XML_TBL WHERE SYSTEM = p_system AND ISDONE IN (0, 8, 9) AND ID BETWEEN output_params('BeginGXMLId') AND output_params('EndGXMLId');
            if gxml_count = 0 then -- if all is done --> exit loop
                INFO('GXML finished');
                exit;
            end if;
            if gxml_last_count = gxml_count then -- if nothing happens, try for GXML_WAIT_TRY times
                gxml_try_number := gxml_try_number + 1;
            else
                gxml_try_number := 0; -- if somthing happens, reset the try counter
            end if;
            if gxml_try_number > GXML_WAIT_TRY then
                ERROR('REPORT. Waiting for GXML error. Nothing happens after {1} tries, count = {2}', GXML_WAIT_TRY, gxml_count);
                exit;
            end if;
            sleep := case when gxml_count > 100 then GXML_WAIT_BIG else GXML_WAIT_SMALL end;
            INFO('Waiting for {1} seconds. count = {2}, try number = {3}', sleep, gxml_count, gxml_try_number);
            DBMS_LOCK.SLEEP(sleep);
        end loop;
    EXCEPTION
        WHEN OTHERS THEN
            ERROR('WAIT_FOR_GXML. code = {1}, message = {2}, backtrace={3}', SQLCODE, SQLERRM, DBMS_UTILITY.FORMAT_ERROR_BACKTRACE);
            RAISE;
    END;

    FUNCTION GET_ROW_COUNT(strSQL in varchar2) RETURN number
    AS
        cnt             number;
    BEGIN
        execute immediate FORMAT('SELECT COUNT(*) FROM ({1})', strSQL) INTO cnt;
        return cnt;
    EXCEPTION
        WHEN OTHERS THEN
            ERROR('GET_ROW_COUNT. code = {1}, message = {2}, backtrace={3}', SQLCODE, SQLERRM, DBMS_UTILITY.FORMAT_ERROR_BACKTRACE);
            RAISE;
    END;
    
    FUNCTION BUILD_WHERE_COND(
        strSQL          in  varchar2,
        p_entite        in  HISTOMVTS.ENTITE%TYPE default null,
        p_contrepartie  in  varchar2 default null,
        p_mvtident      in  HISTOMVTS.MVTIDENT%TYPE default null,
        p_devise        in  varchar2 default null,
        p_perimeterid   in  TITRES.PERIMETERID%TYPE default null,
        p_refcon        in  HISTOMVTS.REFCON%TYPE default null,
        p_just_op       in  boolean default false)
    RETURN varchar2
    AS
        result          varchar2(10000) := strSQL;
    BEGIN
        if p_entite is not null then
            if not p_just_op then
                result := FORMAT('{1}
    AND H.ENTITE = {2}', result, p_entite);
            else
                result := FORMAT('{1}
    AND OP.ENTITY_ID = {2}', result, p_entite);
            end if;
        end if;
        if p_contrepartie is not null then
            if not p_just_op then
                result := FORMAT('{1}
    AND H.CONTREPARTIE IN ({2})', result, p_contrepartie);
            else
                result := FORMAT('{1}
    AND OP.CTPY_ID IN ({2})', result, p_contrepartie);
            end if;
        end if;
        if p_mvtident is not null then
            if not p_just_op then
                result := FORMAT('{1}
    AND H.MVTIDENT = {2}', result, p_mvtident);
            else
                result := FORMAT('{1}
    AND OP.MVTIDENT = {2}', result, p_mvtident);
            end if;
        end if;
        if p_devise is not null then
            if not p_just_op then
                -- use DEVISEAC for HORS POOL, use DEVISECTT for POOL
                result := FORMAT('{1}
    AND ((T.TYPE IN (''P'', ''L'') AND (CASE WHEN T.DEVISEAC IS NULL OR T.DEVISEAC = 0 THEN H.DEVISEPAY ELSE T.DEVISEAC END) = STR_TO_DEVISE(''{2}''))
        OR (T.TYPE NOT IN (''P'', ''L'') AND T.DEVISECTT = STR_TO_DEVISE(''{2}'')))', result, p_devise);
            else
                result := FORMAT('{1}
    AND OP.FACT_CUR = ''{2}''', result, p_devise);
            end if;
        end if;
        if p_perimeterid is not null then
            if not p_just_op then
                result := FORMAT('{1}
    AND T.PERIMETERID = {2}', result, p_perimeterid);
            else
                result := FORMAT('{1}
    AND OP.PERIMETER_ID = {2}', result, p_perimeterid);
            end if;
        end if;
        if p_refcon is not null then
            result := FORMAT('{1}
    AND H.REFCON = {2}', result, p_refcon);
        end if;
        return result;
    EXCEPTION
        WHEN OTHERS THEN
            ERROR('BUILD_WHERE_COND(code = {1}, message = {2}, backtrace={3}', SQLCODE, SQLERRM, DBMS_UTILITY.FORMAT_ERROR_BACKTRACE);
            RAISE;
    END;

    PROCEDURE INSERT_EXPL_MIRROR(
        EX              in  CMA_RPT_EXPLANATIONS%ROWTYPE,
        op_id_mirror    in  CMA_RPT_OPERATIONS.OP_ID%TYPE,
        mvtident_mirror in  HISTOMVTS.MVTIDENT%TYPE)
    AS
        EX_MIRROR       CMA_RPT_EXPLANATIONS%ROWTYPE := EX; -- duplicate all values
    BEGIN
        if mvtident_mirror is not null then
            SELECT CMA_RPT_EXPL_SEQ.NEXTVAL INTO EX_MIRROR.EXPL_ID FROM DUAL;
            EX_MIRROR.OP_ID := op_id_mirror;
            EX_MIRROR.AMOUNT := -1 * EX_MIRROR.AMOUNT;
            EX_MIRROR.INTEREST := -1 * EX_MIRROR.INTEREST;
            EX_MIRROR.QTY := -1 * EX_MIRROR.QTY;
            EX_MIRROR.QTY_UNIT := -1 * EX_MIRROR.QTY_UNIT;
            EX_MIRROR.LB := case EX_MIRROR.LB when 'L' then 'B' when 'B' then 'L' end; 
            INSERT INTO CMA_RPT_EXPLANATIONS VALUES EX_MIRROR;
        end if;
    EXCEPTION
        WHEN OTHERS THEN
            ERROR('INSERT_EXPL_MIRROR(op_id_mirror={4}, mvtident_mirror={5}). code = {1}, message = {2}, backtrace={3}',
                SQLCODE, SQLERRM, DBMS_UTILITY.FORMAT_ERROR_BACKTRACE, op_id_mirror, mvtident_mirror);
            RAISE;
    END;    

    PROCEDURE GENERATE_CMA(
        p_date          in  date,
        p_refcon        in  HISTOMVTS.REFCON%TYPE, --refcon de la MEP, ADM
        p_billing_type  in  int,
        ComList         in  FEE_LIST default null,
        FeePayed        in  number default null,
        Balance_MinFee  in  number default null,
        TotalFees       in  number default null,
        RebPoolList     in  REBATE_POOL_LIST default null,
        RebHorsPoolList in  REBATE_HORS_POOL_LIST default null,
        RebatePayed     in  number default null,
        TotalRebates    in  number default null)
    AS
        OP              CMA_RPT_OPERATIONS%ROWTYPE;
        OP_MIRROR       CMA_RPT_OPERATIONS%ROWTYPE;
        EX              CMA_RPT_EXPLANATIONS%ROWTYPE;
        
        TYPE AGREEMENT_INFO IS RECORD (
            --MEP
            mvtident                HISTOMVTS.MVTIDENT%TYPE,
            contrepartie            HISTOMVTS.CONTREPARTIE%TYPE,
            dateval                 date,
            real_dateval            date,
            delivery_date           date,
            real_delivery_date      date, 
            entite                  HISTOMVTS.ENTITE%TYPE,
            mirror_reference        HISTOMVTS.MIRROR_REFERENCE%TYPE,
            
            --TITRES
            sicovam                 TITRES.SICOVAM%TYPE,
            affectation             TITRES.AFFECTATION%TYPE,
            reference               TITRES.REFERENCE%TYPE,
            typederive              TITRES.TYPEDERIVE%TYPE,
            capitalise2             TITRES.CAPITALISE2%TYPE,
            amort                   TITRES.AMORT%TYPE,
            j1refcon2               TITRES.J1REFCON2%TYPE,
            devisectt               TITRES.DEVISECTT%TYPE,
            deviseac                TITRES.DEVISEAC%TYPE,
            taux_var                TITRES.TAUX_VAR%TYPE,
            fixing_tag1             TITRES.FIXING_TAG1%TYPE,
            beta                    TITRES.BETA%TYPE,
            perimeterid             TITRES.PERIMETERID%TYPE,
            coupon1                 TITRES.COUPON1%TYPE,
            mnemo_v2                TITRES.MNEMO_V2%TYPE,
            libelle                 TITRES.LIBELLE%TYPE,
            underlying_type         TITRES.TYPE%TYPE,
            quotation_type          TITRES.QUOTATION_TYPE%TYPE,
            underlying_nominal      TITRES.NOMINAL%TYPE,
            underlying_nbtitres     TITRES.NBTITRES%TYPE,
            UNDERLYING_COTATIONCUR  varchar2(3), --devise_to_str(TT1.DEVISECTT)
            reference_5             TITRES.REFERENCE%TYPE,
            reference_6             TITRES.REFERENCE%TYPE,
            libelle_5               TITRES.LIBELLE%TYPE,
            is_pool                 int,
            
            --OTHERS
            section                 NATIXIS_FOLIO_SECTION_ENTITE.SECTION%TYPE,
            ctpy_libelle            TIERS.NAME%TYPE,
            entity_libelle          TIERS.NAME%TYPE,
            legal_entity_id         TIERSPROPERTIES.VALUE%TYPE,
            legal_entity_lib        TIERS.NAME%TYPE,
            userid_init             RISKUSERS.NAME%TYPE,
            sedol                   EXTRNL_REFERENCES_INSTRUMENTS.VALUE%TYPE,
            op_libelle              EXTERNAL_OP_REF.INTOPREF%TYPE,
            tp_value                TIERSPROPERTIES.VALUE%TYPE
        );
        a                           AGREEMENT_INFO;
        
        TYPE MIRROR_INFO IS RECORD (
            mvtident                HISTOMVTS.MVTIDENT%TYPE,
            contrepartie            HISTOMVTS.CONTREPARTIE%TYPE,
            entite                  HISTOMVTS.ENTITE%TYPE,
            ctpy_libelle            TIERS.NAME%TYPE,
            entity_libelle          TIERS.NAME%TYPE,
            legal_entity_id         TIERSPROPERTIES.VALUE%TYPE,
            legal_entity_lib        TIERS.NAME%TYPE,
            op_libelle              EXTERNAL_OP_REF.INTOPREF%TYPE
        );
        a_mirror                    MIRROR_INFO;
        
        FinMen                      date;
        underlyingNomUnit           number;
        
        MVTTYPE_FEES                constant int := 0;
        MVTTYPE_REBATES             constant int := 1;
        MVTTYPE_REPO                constant int := 2;
        MVTTYPE_POOL                constant int := 3;
        
    BEGIN
        INFO('GENERATE_CMA(date = {1}, refcon = {2}, billing_type = {3})', p_date, p_refcon, p_billing_type);
        
        FinMen := add_months(trunc(p_date, 'MM'), 1) - 1;
        
        SELECT H.MVTIDENT, H.CONTREPARTIE, H.DATEVAL, DECODE(H.REAL_DATEVAL, NULL_DATE, NULL, H.REAL_DATEVAL),
            NVL(H.DELIVERY_DATE, H.DATEVAL), DECODE(H.REAL_DELIVERY_DATE, NULL_DATE, NULL, H.REAL_DELIVERY_DATE), H.ENTITE, H.MIRROR_REFERENCE,
            T.SICOVAM, T.AFFECTATION, T.REFERENCE, T.TYPEDERIVE, T.CAPITALISE2, T.AMORT, T.J1REFCON2, T.DEVISECTT,
            CASE WHEN T.DEVISEAC IS NULL OR T.DEVISEAC = 0 THEN H.DEVISEPAY ELSE T.DEVISEAC END,
            T.TAUX_VAR, T.FIXING_TAG1, T.BETA, T.PERIMETERID, T.COUPON1,
            TT1.MNEMO_V2, TT1.LIBELLE, TT1.TYPE, TT1.QUOTATION_TYPE, TT1.NOMINAL, TT1.NBTITRES, DEVISE_TO_STR(TT1.DEVISECTT),
            TT5.REFERENCE, TT6.REFERENCE, TT5.LIBELLE, 
            CASE WHEN T.TYPE = 'C' AND T.MODELE = 'Collateral' THEN 1 ELSE 0 END,
            FSE.SECTION, TC.NAME, TE.NAME, TP2.VALUE, T5.NAME, U.NAME, R.VALUE, OPR.INTOPREF, NVL(TP.VALUE, TP_FD)
            into a
        FROM HISTOMVTS H
            JOIN NATIXIS_FOLIO_SECTION_ENTITE FSE ON FSE.IDENT = H.OPCVM
            JOIN TITRES T ON T.SICOVAM = H.SICOVAM
            LEFT JOIN TITRES TT1 ON TT1.SICOVAM = T.CODE_EMET -- same alias name as SP
            LEFT JOIN EXTRNL_REFERENCES_INSTRUMENTS R ON R.SOPHIS_IDENT = TT1.SICOVAM AND R.REF_IDENT = 8002
            LEFT JOIN TITRES TT5 ON TT5.SICOVAM = T.TAUX_VAR -- same alias name as SP 
            LEFT JOIN TITRES TT6 ON TT6.SICOVAM = T.FIXING_TAG1 -- same alias name as SP
            JOIN TIERS TC ON TC.IDENT = H.CONTREPARTIE
            JOIN TIERS TE ON TE.IDENT = H.ENTITE
            JOIN RISKUSERS U ON U.IDENT = H.OPERATEUR
            LEFT JOIN EXTERNAL_OP_REF OPR ON OPR.MVTIDENT = H.MVTIDENT
            LEFT JOIN TIERSPROPERTIES TP ON TP.CODE = H.CONTREPARTIE AND TP.NAME = TP_BILLING
            LEFT JOIN TIERSPROPERTIES TP2 ON TP2.CODE = H.ENTITE AND TP2.NAME = 'Legal Entity'
            LEFT JOIN TIERS T5 ON T5.REFERENCE = TRIM(TP2.VALUE)
        WHERE H.REFCON = p_refcon; -- refcon de la MEP ou ADM 
        
        if a.mirror_reference = -1 then
        begin
            SELECT H2.MVTIDENT, H2.CONTREPARTIE, H2.ENTITE, TC.NAME, TE.NAME, TP2.VALUE, T5.NAME, OPR.INTOPREF
                into a_mirror
            FROM HISTOMVTS H2
                JOIN TIERS TC ON TC.IDENT = H2.CONTREPARTIE
                JOIN TIERS TE ON TE.IDENT = H2.ENTITE
                LEFT JOIN EXTERNAL_OP_REF OPR ON OPR.MVTIDENT = H2.MVTIDENT
                LEFT JOIN TIERSPROPERTIES TP2 ON TP2.CODE = H2.ENTITE AND TP2.NAME = 'Legal Entity'
                LEFT JOIN TIERS T5 ON T5.REFERENCE = TRIM(TP2.VALUE)
            WHERE H2.MIRROR_REFERENCE = p_refcon;
        exception
            when NO_DATA_FOUND then
                WARN('Mirroring impossible pour position {1}', a.mvtident);
        end;
        end if;
        DEBUG('mvtident mirror = {1}', a_mirror.mvtident);
            
        if not (a.is_pool = 1 and ROUND_DATA(TotalRebates) = 0) then -- CMA of POOL is generated when total rebates != 0
            
            -- OP_ID
            SELECT CMA_RPT_OPER_SEQ.NEXTVAL INTO OP.OP_ID FROM DUAL;
            
            -- DTGEN
            OP.DTGEN := sysdate;
            
            -- MVTTYPE
            OP.MVTTYPE := case  when p_billing_type = CMA_FEES then MVTTYPE_FEES
                                when p_billing_type = CMA_REBATES and a.is_pool = 1 then MVTTYPE_POOL
                                when p_billing_type = CMA_REBATES and a.affectation in (60) then MVTTYPE_REBATES
                                when p_billing_type = CMA_REBATES and a.affectation in (62, 63, 65) then MVTTYPE_REPO end;
            
            -- MVTIDENT
            OP.MVTIDENT := a.mvtident;
            
            -- MOIS_FACT
            OP.MOIS_FACT := p_date; -- infine --> DateSitu, mensuel --> DebMen 
            
            OP.CTPY_ID := a.contrepartie;
            OP.CTPY_LIBELLE := a.ctpy_libelle;
            
            -- OP_SICOVAM
            OP.OP_SICOVAM :=    case when a.is_pool = 1 then a.taux_var
                                     else a.sicovam end;
            
            -- OP_REF
            OP.OP_REF :=        case when a.is_pool = 1 then a.reference_5
                                     else a.reference end;

            -- OP_LIBELLE                                     
            OP.OP_LIBELLE :=    case when a.is_pool = 1 then a.libelle_5
                                     else a.op_libelle end;

            -- OP_TYPE            
            OP.OP_TYPE := case  when a.is_pool = 1 then null
                                when a.affectation in (59, 60, 61, 64) then 'PE'
                                when a.affectation in (62, 63, 65) then 'RP' end;
            
            -- OP_TIMEBASIS
            OP.OP_TIMEBASIS := case when a.typederive in (1) then 360
                                    when a.typederive in (2, 6, 7) then 365
                                    else 0 end;
            
            -- OP_START_DATE
            OP.OP_START_DATE := case when a.is_pool = 1 then null 
                                     when a.tp_value = TP_FT then least(a.dateval, a.delivery_date)
                                     when a.tp_value = TP_FR then
                                        case when a.real_dateval is null then a.real_delivery_date
                                             when a.real_delivery_date is null then a.real_dateval
                                             else least(a.real_dateval, a.real_delivery_date) end end; 
            
            -- OP_ECH_DATE
            if a.is_pool = 1 then
                OP.OP_ECH_DATE := null;
            else
                --BS_LIB.DATE_MATURITY(a.mvtident, FinMen);
                /*SELECT NVL(
                    (SELECT H.COMMISSION_DATE
                    FROM HISTOMVTS H WHERE H.REFCON = 
                        (SELECT MAX(REFCON) FROM HISTOMVTS H
                        WHERE H.MVTIDENT = a.mvtident AND H.TYPE IN (304)
                            AND H.DATENEG <= FinMen)),
                    (SELECT H.COMMISSION_DATE
                    FROM HISTOMVTS H WHERE H.REFCON = 
                        (SELECT MIN(H.REFCON) FROM HISTOMVTS H
                            JOIN BUSINESS_EVENTS BE ON BE.ID = H.TYPE AND BE.COMPTA = 1
                        WHERE H.MVTIDENT = a.mvtident AND H.COMMISSION_DATE IS NOT NULL)))
                into OP.OP_ECH_DATE FROM DUAL;*/
                OP.OP_ECH_DATE := null;
                begin
                SELECT CASE WHEN a.tp_value = TP_FT THEN LEAST(HC.DATEVAL, NVL(HC.DELIVERY_DATE, HC.DATEVAL))
                            WHEN a.tp_value = TP_FR THEN
                                CAST(CASE WHEN DECODE(HC.REAL_DATEVAL, NULL_DATE, NULL, HC.REAL_DATEVAL) IS NULL THEN DECODE(HC.REAL_DELIVERY_DATE, NULL_DATE, NULL, HC.REAL_DELIVERY_DATE)
                                          WHEN DECODE(HC.REAL_DELIVERY_DATE, NULL_DATE, NULL, HC.REAL_DELIVERY_DATE) IS NULL THEN DECODE(HC.REAL_DATEVAL, NULL_DATE, NULL, HC.REAL_DATEVAL)
                                          ELSE LEAST(DECODE(HC.REAL_DATEVAL, NULL_DATE, NULL, HC.REAL_DATEVAL), DECODE(HC.REAL_DELIVERY_DATE, NULL_DATE, NULL, HC.REAL_DELIVERY_DATE)) END AS DATE) END
                    into OP.OP_ECH_DATE
                FROM HISTOMVTS HC
                WHERE HC.TYPE IN (102, 501) AND HC.MVTIDENT = a.mvtident
                    AND ((GP_PERIODICITE = PER_M
                            AND HC.BACKOFFICE IN (
                                SELECT KSC.KERNEL_STATUS_ID FROM BO_KERNEL_STATUS_COMPONENT KSC
                                    JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                                    AND KSG.RECORD_TYPE = 1 AND KSG.NAME = 'All But Pending FO'))
                        OR (GP_PERIODICITE = PER_I
                            AND HC.BACKOFFICE NOT IN (
                                SELECT KSC.KERNEL_STATUS_ID
                                FROM BO_KERNEL_STATUS_COMPONENT KSC
                                    JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                                        AND KSG.NAME IN ('_STEP_Cancelled', '_MOD_Cancelled') AND KSG.RECORD_TYPE = 1)));
                exception
                    when no_data_found then
                        null;
                end;
            end if;
            
            -- SSJ_ISIN
            OP.SSJ_ISIN := case when a.affectation in (62, 63, 65) or a.is_pool = 1 then null else a.mnemo_v2 end; 
            -- SSJ_LIBELLE
            OP.SSJ_LIBELLE := case when a.affectation in (62, 63, 65) or a.is_pool = 1 then null else a.libelle end; 
            
            -- TYPEGAR
            OP.TYPEGAR := case  when a.affectation in (59, 64, 65) or a.is_pool = 1 then 'NA'
                                when a.affectation in (60) and a.capitalise2 = 3 then 'CC' 
                                when a.affectation in (60) and a.capitalise2 = 2 then 'CP'
                                when a.affectation in (61, 62, 63) then 'SP' end;
            
            -- FACT_METHOD
            OP.FACT_METHOD := case  when a.is_pool = 1 then null
                                    else
                                        case a.amort    when 1 then 'WC'
                                                        when 2 then 'FP'
                                                        when 3 then 'OB'
                                                        when 4 then 'SP'
                                                        when 5 then 'FM'
                                                        when 6 then 'FPR'
                                                        when 7 then 'RB' end end;
                                                        
            -- FACT_FREQ
            OP.FACT_FREQ := case when a.is_pool = 1 then 'Mensuel'
                                 when a.j1refcon2 = PER_I then 'In Fine'
                                 when a.j1refcon2 = PER_M then 'Mensuel' end;
            
            -- FACT_CUR
            OP.FACT_CUR := case when (p_billing_type = CMA_REBATES and a.affectation in (60)) or a.is_pool = 1 then devise_to_str(a.devisectt)
                                else devise_to_str(a.deviseac) end;

            -- FACT_DAYS
            OP.FACT_DAYS := 0;
            if p_billing_type = CMA_FEES then
                for i in 1..ComList.count
                loop
                    OP.FACT_DAYS := OP.FACT_DAYS + ComList(i).days;                
                end loop;
            else
                for i in 1..RebPoolList.count
                loop
                    OP.FACT_DAYS := OP.FACT_DAYS + RebPoolList(i).days;                
                end loop;
                for i in 1..RebHorsPoolList.count
                loop
                    OP.FACT_DAYS := OP.FACT_DAYS + RebHorsPoolList(i).days;                
                end loop;
            end if;
                                            
            -- CONST_CUR
            OP.CONST_CUR := devise_to_str(a.devisectt);
            
            -- RATE_ID
            OP.RATE_ID := case  when a.affectation in (59, 61, 64) then null
                                when a.affectation in (60) or a.is_pool = 1 then a.taux_var
                                when a.affectation in (62, 63, 65) then a.fixing_tag1 end;
            
            -- RATE_LIBELLE
            OP.RATE_LIBELLE := case  when a.affectation in (59, 61, 64) then null
                                when a.affectation in (60) or a.is_pool = 1 then a.reference_5
                                when a.affectation in (62, 63, 65) then a.reference_6 end;
                                
            -- OP_HEDGING
            OP.OP_HEDGING := case when a.is_pool = 1  then 0 else a.beta end;
            
            -- TA
            OP.TA := null; -- GXML
            
            -- OPCVM
            OP.OPCVM := a.section;
            
            -- TA_REFCON
            OP.TA_REFCON := null; -- GXML
            
            -- PERIMETER_ID
            OP.PERIMETER_ID := a.perimeterid;
            
            -- RISKUSER
            OP.RISKUSER := GP_RISKUSER;
            
            -- ENTITY_ID
            OP.ENTITY_ID := a.entite;
            
            -- ENTITY_LIBELLE
            OP.ENTITY_LIBELLE := a.entity_libelle;
            
            -- LEGAL_ENTITY_ID
            OP.LEGAL_ENTITY_ID := a.legal_entity_id;
            
            -- LEGAL_ENTITY_LIB
            OP.LEGAL_ENTITY_LIB := a.legal_entity_lib;
            
            -- MIN_FEE
            OP.MIN_FEE := case when (p_billing_type = CMA_REBATES or a.j1refcon2 = PER_I) then 0 else a.coupon1 end;
            
            -- FEE_PAYED
            OP.FEE_PAYED := case when p_billing_type = CMA_FEES then ROUND_DATA(FeePayed)
                                 when a.j1refcon2 = PER_M then 0
                                 when a.j1refcon2 = PER_I then ROUND_DATA(RebatePayed) end;
            
            -- BAL_MIN_FEE
            OP.BAL_MIN_FEE := case when (p_billing_type = CMA_REBATES or a.j1refcon2 = PER_I) then 0
                                   else ROUND_DATA(Balance_MinFee) end;
            
            -- EXPL_AMOUNT.
            OP.EXPL_AMOUNT := case when a.j1refcon2 = PER_M then
                                        case when p_billing_type = CMA_FEES then ROUND_DATA(TotalFees + Balance_MinFee)
                                        else ROUND_DATA(TotalRebates) end
                                   when a.j1refcon2 = PER_I then
                                        case when p_billing_type = CMA_FEES then ROUND_DATA(TotalFees - FeePayed)
                                        else ROUND_DATA(TotalRebates - RebatePayed) end end;
            
            -- USERID_INIT
            OP.USERID_INIT := case when a.is_pool = 1 then null else a.userid_init end;
            
            -- SEDOL
            OP.SEDOL := case when a.affectation in (62, 63, 65) or a.is_pool = 1 then null else a.sedol end;
            
            -- TYPEVAL
            OP.TYPEVAL := case when a.affectation in (62, 63, 65) or a.is_pool = 1 then null else a.underlying_type end;
            
            INSERT INTO CMA_RPT_OPERATIONS VALUES OP;
            
            DEBUG('op_id = {1}, mois_fact = {2}, mvtident = {3}, rowcount = {4}', OP.OP_ID, OP.MOIS_FACT, OP.MVTIDENT, SQL%ROWCOUNT);
            
            -- Gestion des deals miroirs
            if a_mirror.mvtident is not null then
                OP_MIRROR := OP; -- duplicate all values
                SELECT CMA_RPT_OPER_SEQ.NEXTVAL INTO OP_MIRROR.OP_ID FROM DUAL;
                OP_MIRROR.MVTIDENT := a_mirror.mvtident;
                OP_MIRROR.CTPY_ID := a_mirror.contrepartie;
                OP_MIRROR.CTPY_LIBELLE := a_mirror.ctpy_libelle;
                OP_MIRROR.OP_LIBELLE := a_mirror.op_libelle;
                OP_MIRROR.ENTITY_ID := a_mirror.entite;
                OP_MIRROR.ENTITY_LIBELLE := a_mirror.entity_libelle;
                OP_MIRROR.LEGAL_ENTITY_ID := a_mirror.legal_entity_id;
                OP_MIRROR.LEGAL_ENTITY_LIB := a_mirror.legal_entity_lib;
                OP_MIRROR.TA := -1 * OP_MIRROR.TA;
                OP_MIRROR.EXPL_AMOUNT := -1 * OP_MIRROR.EXPL_AMOUNT; 
                OP_MIRROR.FEE_PAYED := -1 * OP_MIRROR.FEE_PAYED;
                OP_MIRROR.TA_REFCON := null; -- GXML
                INSERT INTO CMA_RPT_OPERATIONS VALUES OP_MIRROR;
                
                DEBUG('Mirror op_id = {1}, mois_fact = {2}, mvtident = {3}, rowcount = {4}', OP_MIRROR.OP_ID, OP_MIRROR.MOIS_FACT, OP_MIRROR.MVTIDENT, SQL%ROWCOUNT);
            end if;
            
            -- obligation cotée en pourcentage
            underlyingNomUnit := 1;
            if a.quotation_type = 2 and a.underlying_type != 'A' then -- equivalent avec UNDERLYING_PRICE_TYPE = 2
                if a.underlying_type in ('O', 'D') then
                    underlyingNomUnit := case a.underlying_nominal when 0 then 1 else a.underlying_nominal end;                             
                elsif a.underlying_type in ('N') then
                    underlyingNomUnit := case a.underlying_nbtitres when 0 then 1 else a.underlying_nbtitres end;
                else
                    underlyingNomUnit := 1;
                end if;
            end if;
                    
            if p_billing_type = CMA_FEES then
                for i in 1..ComList.count
                loop
                    SELECT CMA_RPT_EXPL_SEQ.NEXTVAL INTO EX.EXPL_ID FROM DUAL;
                    EX.OP_ID := OP.OP_ID;
                    EX.DTGEN := sysdate;
                    EX.DFROM := ComList(i)."from";
                    EX.DTO := ComList(i)."to";
                    EX.DAYS := ComList(i).days;
                    EX.QTY := ComList(i).qty; -- null for Repo and Pool
                    EX.LB := case when EX.QTY <= 0 then 'L' when EX.QTY > 0 then 'B' end;
                    EX.SPOT := ComList(i).prix; -- null for Repo and Pool
                    EX.SPOT_CUR := a.UNDERLYING_COTATIONCUR;
                    EX.FOREX := ComList(i).taux_change; -- null for Repo and Pool
                    EX.COMM_RATE := ComList(i).taux_com; -- null for other type
                    EX.AMOUNT := ComList(i).value;
                    EX.INTEREST := ComList(i).montant_fee;
                    EX.SPREAD := null; -- null for commissions
                    EX.RATE := null; -- null for commissions
                    EX.PERIOD_RATE := null;
                    EX.REF := null;
                    EX.QTY_UNIT := EX.QTY / underlyingNomUnit;
                    
                    INSERT INTO CMA_RPT_EXPLANATIONS VALUES EX;
                    
                    -- Gestion des deals miroirs
                    INSERT_EXPL_MIRROR(EX, OP_MIRROR.OP_ID, a_mirror.mvtident);
                end loop;
                DEBUG('Explanations for fees inserted. {1} rows.', ComList.count);
            elsif p_billing_type = CMA_REBATES then
                for i in 1..RebHorsPoolList.count
                loop
                    SELECT CMA_RPT_EXPL_SEQ.NEXTVAL INTO EX.EXPL_ID FROM DUAL;
                    EX.OP_ID := OP.OP_ID;
                    EX.DTGEN := sysdate;
                    EX.DFROM := RebHorsPoolList(i)."from";
                    EX.DTO := RebHorsPoolList(i)."to";
                    EX.DAYS := RebHorsPoolList(i).days;
                    EX.QTY := case when OP.MVTTYPE in (MVTTYPE_REPO, MVTTYPE_POOL) then 0 else RebHorsPoolList(i).qty end; -- null for Repo and Pool, 0 is not null
                    EX.LB := case when EX.QTY < 0 then 'L' when EX.QTY > 0 then 'B'  else null end;
                    EX.SPOT := case when OP.MVTTYPE in (MVTTYPE_REPO, MVTTYPE_POOL) then null else RebHorsPoolList(i).prix end; -- null for Repo and Pool
                    EX.SPOT_CUR := a.UNDERLYING_COTATIONCUR;
                    EX.FOREX := case when OP.MVTTYPE in (MVTTYPE_REPO, MVTTYPE_POOL) then null else RebHorsPoolList(i).taux_change end; -- null for Repo and Pool
                    EX.COMM_RATE := null; -- null for other type
                    EX.AMOUNT := RebHorsPoolList(i).value;
                    EX.INTEREST := RebHorsPoolList(i).interest;
                    EX.SPREAD := RebHorsPoolList(i).spread; -- null for commissions
                    EX.RATE := RebHorsPoolList(i).rate; -- null for commissions
                    EX.PERIOD_RATE := null;
                    EX.REF := null;
                    EX.QTY_UNIT := case when OP.MVTTYPE in (MVTTYPE_REPO, MVTTYPE_POOL) then null else EX.QTY / underlyingNomUnit end;
                    
                    INSERT INTO CMA_RPT_EXPLANATIONS VALUES EX;
                    
                    -- Gestion des deals miroirs
                    INSERT_EXPL_MIRROR(EX, OP_MIRROR.OP_ID, a_mirror.mvtident);
                end loop;
                for i in 1..RebPoolList.count
                loop
                    SELECT CMA_RPT_EXPL_SEQ.NEXTVAL INTO EX.EXPL_ID FROM DUAL;
                    EX.OP_ID := OP.OP_ID;
                    EX.DTGEN := sysdate;
                    EX.DFROM := RebPoolList(i)."from";
                    EX.DTO := RebPoolList(i)."to";
                    EX.DAYS := RebPoolList(i).days;
                    EX.QTY := 0; -- null for Repo and Pool
                    EX.LB := case when EX.QTY < 0 then 'L' when EX.QTY > 0 then 'B' else null end; -- null
                    EX.SPOT := null; -- null for Repo and Pool
                    EX.SPOT_CUR := a.UNDERLYING_COTATIONCUR;
                    EX.FOREX := null; -- null for Repo and Pool
                    EX.COMM_RATE := null; -- null for other type
                    EX.AMOUNT := RebPoolList(i).value;
                    EX.INTEREST := RebPoolList(i).interest;
                    EX.SPREAD := RebPoolList(i).spread; -- null for commissions
                    EX.RATE := RebPoolList(i).rate; -- null for commissions
                    EX.PERIOD_RATE := null;
                    EX.REF := null;
                    EX.QTY_UNIT := case when OP.MVTTYPE in (MVTTYPE_REPO, MVTTYPE_POOL) then null else EX.QTY / underlyingNomUnit end;
                    
                    INSERT INTO CMA_RPT_EXPLANATIONS VALUES EX;
                    
                    -- Gestion des deals miroirs
                    INSERT_EXPL_MIRROR(EX, OP_MIRROR.OP_ID, a_mirror.mvtident);
                end loop;
                DEBUG('Explanations for rebates inserted. {1} rows.', RebHorsPoolList.count + RebPoolList.count);
            end if;
        end if;
        
        INFO('GENERATE_CMA.END');
    EXCEPTION
        WHEN OTHERS THEN 
            ERROR('GENERATE_CMA. code = {1}, message = {2}, backtrace={3}', SQLCODE, SQLERRM, DBMS_UTILITY.FORMAT_ERROR_BACKTRACE);
            RAISE;
    END;
        
    
    PROCEDURE CALCUL_COMMISSION(
        p_refcon        in  HISTOMVTS.REFCON%TYPE,
        p_dateneg_c     in  date, -- dateneg du closing
        DebCom          in  date,
        FinCom          in  date,
        ComList         out FEE_LIST,
        FeePayed        out number,
        Balance_MinFee  out number,
        TotalFees       out number,
        DebPos          in  date,
        p_isFilled      in  boolean default false)
    AS
        TYPE AGREEMENT_INFO IS RECORD (
            --MEP
            quantite                HISTOMVTS.QUANTITE%TYPE,
            cours                   HISTOMVTS.COURS%TYPE,
            contrepartie            HISTOMVTS.CONTREPARTIE%TYPE,
            entite                  HISTOMVTS.ENTITE%TYPE,
            dateval                 date,
            commission              HISTOMVTS.COMMISSION%TYPE,
            mvtident                HISTOMVTS.MVTIDENT%TYPE,
            
            --TITRES
            amort                   TITRES.AMORT%TYPE,
            coupon1                 TITRES.COUPON1%TYPE,
            code_emet               TITRES.CODE_EMET%TYPE,
            perimeterid             TITRES.PERIMETERID%TYPE,
            typederive              TITRES.TYPEDERIVE%TYPE,
            BILLING_CUR             varchar2(3), --devise_To_Str(TT.DEVISEAC)
            UNDERLYING_COTATIONCUR  varchar2(3), --devise_to_str(TT1.DEVISECTT)
            underlying_type         TITRES.TYPE%TYPE,
            quotation_type          TITRES.QUOTATION_TYPE%TYPE,
            underlying_nominal      TITRES.NOMINAL%TYPE,
            underlying_nbtitres     TITRES.NBTITRES%TYPE,
            
            --OTHERS
            unitedecotation         MARCHE.UNITEDECOTATION%TYPE,
            tp_value                TIERSPROPERTIES.VALUE%TYPE
        );
        a                           AGREEMENT_INFO;
        
        c FEE_LIST := FEE_LIST();

        curRates            SYS_REFCURSOR;
        currentRate         RATE_INFO; -- associated with curRates
        nextRate            RATE_INFO; -- associated with curRates
        
        d1                  date;
        idx                 integer := 0;
        rc                  integer;
        underlyingNomUnit   number;
        
        -- cours of DmHistorique, BO_LB_FEEMARK,
        -- has the same structure as RATE_INFO. Used in when calculate prix for OPEN_BASIS, FEE_MARK
        curCours            SYS_REFCURSOR; 
        currentCours        RATE_INFO;
        nextCours           RATE_INFO;
        
        XSicovam            TITRES.SICOVAM%TYPE; -- sicovam of exchange Titres
        XSicovam1           TITRES.SICOVAM%TYPE; -- sicovam of exchange Titres/EUR
        XSicovam2           TITRES.SICOVAM%TYPE; -- sicovam of exchange EUR/Titres
        curXRates           SYS_REFCURSOR;       -- cursor for the cours of exchange
        curXRates1          SYS_REFCURSOR;       -- cursor for the cours of exchange Titres/EUR
        curXRates2          SYS_REFCURSOR;       -- cursor for the cours of exchange EUR/Titres
        currentXRate        RATE_INFO;
        currentXRate1       RATE_INFO;
        currentXRate2       RATE_INFO;
        nextXRate           RATE_INFO;
        nextXRate1          RATE_INFO;
        nextXRate2          RATE_INFO;
        EUR                 constant varchar2(3) := 'EUR';
        newPrix             boolean := false;
        newPrixDate         date;
        
        -- Cursor to retrieve Qty by date
        curQty              SYS_REFCURSOR;
        currentQty          DATE_NUM;
        nextQty             DATE_NUM;
        
        -- Facture Condition
        lastFC              BL_FEES%ROWTYPE;
        currentFC           BL_FEES%ROWTYPE;
        
        -- TIME BASIS, CALC BASISS
        calc_basis          number;
        
        TC_FIXED_PRICE      TITRES.AMORT%TYPE := 2;
        TC_OPEN_BASIS       TITRES.AMORT%TYPE := 3;
        TC_SET_PRICE        TITRES.AMORT%TYPE := 4;
        TC_FEE_MARK         TITRES.AMORT%TYPE := 5;
        TC_FP_RP            TITRES.AMORT%TYPE := 6; -- Fixed Price with Revisable Price 
        
    BEGIN
        INFO('CALCUL_COMMISSION(refcon = {1}, dateneg_c = {2}, DebCom = {3}, FinCom = {4}, DebPos = {5})',
            p_refcon, p_dateneg_c, DebCom, FinCom, DebPos);

        ComList := FEE_LIST();
        FeePayed := 0;
        Balance_MinFee := 0;
        TotalFees := 0;
            
        SELECT
            H.QUANTITE, H.COURS, H.CONTREPARTIE, H.ENTITE, H.DATEVAL, H.COMMISSION, H.MVTIDENT,
            T.AMORT, T.COUPON1, T.CODE_EMET, T.PERIMETERID, T.TYPEDERIVE,
            DEVISE_TO_STR(T.DEVISEAC), DEVISE_TO_STR(TT1.DEVISECTT), TT1.TYPE, TT1.QUOTATION_TYPE, TT1.NOMINAL, TT1.NBTITRES,
            M.UNITEDECOTATION, decode (GP_SOURCE, SRC_PNL, TP_FT, NVL(TP.VALUE, TP_FD)) --PNL : Facturation en dates Théoriques
            into a
        FROM HISTOMVTS H
            JOIN TITRES T ON T.SICOVAM = H.SICOVAM
            LEFT JOIN TITRES TT1 ON TT1.SICOVAM = T.CODE_EMET
            LEFT JOIN MARCHE M ON M.MNEMOMARCHE = TT1.MARCHE AND M.CODEDEVISE = TT1.DEVISECTT
            LEFT JOIN TIERSPROPERTIES TP ON TP.CODE = H.CONTREPARTIE AND TP.NAME = TP_BILLING
        WHERE H.REFCON = p_refcon;
        
        INFO('quantite = {1}, cours = {2}, contrepartie = {3}, entite = {4}, dateval = {5}, commission = {6}, mvtident = {7}',
            a.quantite, a.cours, a.contrepartie, a.entite, a.dateval, a.commission, a.mvtident);
        INFO('amort = {1}, coupon1 = {2}, code_emet = {3}, perimeterid = {4}, typederive = {5}, unitedecotation = {6}, tiersproperties = {7}',
            a.amort, a.coupon1, a.code_emet, a.perimeterid, a.typederive, a.unitedecotation, a.tp_value);
        INFO('UNDERLYING_COTATIONCUR = {1}, BILLING_CUR = {2}, underlying_type = {3}, quotation_type = {4}, underlying_nominal = {5}, a.underlying_nbtites = {6}',
            a.UNDERLYING_COTATIONCUR, a.BILLING_CUR, a.underlying_type, a.quotation_type, a.underlying_nominal, a.underlying_nbtitres);
        
        calc_basis := case a.typederive when 1 then 360 when 2 then 365 when 6 then 365.25 when 7 then 365 else 360 end;
        --SELECT DECODE(a.typederive, 1, 360, 2, 365, 6, 365.25, 7, 365, 360) INTO calc_basis FROM DUAL;
        if a.typederive is null then
            WARN('TIME BASIS non gere. Par defaut : Actual/360');
        elsif a.typederive not in (1, 2, 6, 7) then
            WARN('TIME BASIS not supported');
        end if;
        
        if a.amort != TC_SET_PRICE then
            -- cursor of rates.
            -- when there's more than one LOAN REPO COMMISSION (27) at one dateval, choose the last refcon
            -- BL.CALCUL_COMMISSION(2679087,
            --    to_date('01/10/2011', 'DD/MM/YYYY'), to_date('30/10/2011', 'DD/MM/YYYY'));        
            open curRates for
                SELECT DATEVAL, RATE FROM (
                    SELECT MIN_DATE DATEVAL, H.COMMISSION RATE, H.REFCON FROM HISTOMVTS H
                    WHERE H.REFCON = p_refcon
                    UNION
                    SELECT HL.DATEVAL, HL.COURS RATE, HL.REFCON
                    FROM HISTOMVTS H
                        JOIN HISTOMVTS HL ON HL.MVTIDENT = H.MVTIDENT AND HL.TYPE IN (27)
                            AND HL.BACKOFFICE IN (
                                SELECT KSC.KERNEL_STATUS_ID FROM BO_KERNEL_STATUS_COMPONENT KSC
                                    JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                                    AND KSG.RECORD_TYPE = 1 AND KSG.NAME = 'All But Pending FO')
                    WHERE H.REFCON = p_refcon
                ) ORDER BY DATEVAL, REFCON;
        end if;
        
        if a.amort = TC_FIXED_PRICE then
            currentFC.prix := a.cours;
        elsif a.amort = TC_OPEN_BASIS then
            begin
                -- Get the last cours (cours du veille)
                -- Attention: Never use Rownum with Order By
                if DebCom = DebPos then
                    nextCours.dateval := DebCom - 1; -- just only a date < DebCom, to be used in while condition
                    nextCours.rate := a.cours;
                else
                    -- just only a date < DebCom, to be used in while condition
                    nextCours.dateval := DebCom;
                    nextCours.rate := null;
                    while (nextCours.rate is null)
                    loop
                        -- for performance optimization, do not use COURS IS NOT NULL in the following query
                        SELECT nextCours.dateval - 1, D.CLOSE + CASE WHEN a.underlying_type != 'A' THEN NVL(D.COUPON, 0) ELSE 0 END
                            into nextCours
                        FROM DMHISTORIQUE D
                        WHERE D.SICOVAM = a.code_emet
                            AND D.JOUR = (
                                SELECT MAX(D.JOUR)
                                FROM DMHISTORIQUE D
                                WHERE D.SICOVAM = a.code_emet
                                    AND D.JOUR < nextCours.dateval);
                    end loop;
                end if;
            exception
                when NO_DATA_FOUND then
                    nextCours.dateval := MIN_DATE;
                    nextCours.rate := 0;
                    ERROR('No cours found for sicovam = {1} before {2}', a.code_emet, DebCom);
            end;
            -- for performance optimization, do not use COURS IS NOT NULL in the following query
            open curCours for
                SELECT D.JOUR, D.CLOSE + CASE WHEN a.underlying_type != 'A' THEN NVL(D.COUPON, 0) ELSE 0 END
                FROM DMHISTORIQUE D
                WHERE D.SICOVAM = a.code_emet AND D.JOUR >= DebCom AND D.JOUR < FinCom
                ORDER BY 1;
        elsif a.amort = TC_SET_PRICE then
            if GP_PERIODICITE = PER_M then
                WARN('Type de commission à SET PRICE incompatible avec fréquence mensuelle pour MvtIdent {1}', a.mvtident);
            elsif GP_PERIODICITE = PER_I then
                currentFC.prix := a.cours;
            end if;
        elsif a.amort = TC_FEE_MARK then
            begin
                nextCours.dateval := DebCom + 1;
                nextCours.rate := null;
                while (nextCours.rate is null)
                loop
                    -- for performance optimization, do not use COURS IS NOT NULL in the following query
                    -- 536.21
                    SELECT  DISTINCT H.DATEVAL,
                            NVL((SELECT D.CLOSE + CASE WHEN a.underlying_type != 'A' THEN NVL(D.COUPON, 0) ELSE 0 END
                            FROM DMHISTORIQUE D WHERE D.SICOVAM = a.code_emet
                                AND D.JOUR =
                                    (SELECT MAX(DM.JOUR)
                                    FROM DMHISTORIQUE DM WHERE DM.SICOVAM = a.code_emet AND DM.JOUR < H.DATEVAL)), 0) RATE
                        into nextCours
                    FROM HISTOMVTS H
                    WHERE H.TYPE = 16 AND H.MVTIDENT = a.mvtident
                        AND ((H.BACKOFFICE IN (
                            SELECT KSC.KERNEL_STATUS_ID FROM BO_KERNEL_STATUS_COMPONENT KSC
                                JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                                AND KSG.RECORD_TYPE = 1 AND KSG.NAME = 'All But Pending FO') AND GP_SOURCE != SRC_PNL )
                                OR (H.BACKOFFICE NOT IN (
                                        SELECT KSC.KERNEL_STATUS_ID
                                        FROM BO_KERNEL_STATUS_COMPONENT KSC
                                            JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                                                AND KSG.NAME IN ('_STEP_Cancelled', '_MOD_Cancelled') AND KSG.RECORD_TYPE = 1) AND GP_SOURCE = SRC_PNL  ))
                        AND H.DATEVAL = (SELECT MAX(HM.DATEVAL) FROM HISTOMVTS HM
                            WHERE HM.TYPE = 16 AND HM.MVTIDENT = a.mvtident AND HM.DATEVAL <= nextCours.dateval - 1
                                AND ((HM.BACKOFFICE IN (
                                    SELECT KSC.KERNEL_STATUS_ID FROM BO_KERNEL_STATUS_COMPONENT KSC
                                        JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                                        AND KSG.RECORD_TYPE = 1 AND KSG.NAME = 'All But Pending FO') AND GP_SOURCE != SRC_PNL )
                                        OR (HM.BACKOFFICE NOT IN (
                                                SELECT KSC.KERNEL_STATUS_ID
                                                FROM BO_KERNEL_STATUS_COMPONENT KSC
                                                    JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                                                        AND KSG.NAME IN ('_STEP_Cancelled', '_MOD_Cancelled') AND KSG.RECORD_TYPE = 1) AND GP_SOURCE = SRC_PNL  )));
                end loop;
                
                -- Get the last cours (cours du jour)
                -- Attention: Never use Rownum with Order By
                
                /* -- 536.21 to be uncommented when return to BO_LB_FEEMARK
                
                SELECT F.FEE_DATE, F.SPOT
                    into nextCours
                FROM BO_LB_FEEMARK F
                WHERE F.CTPY_ID = a.contrepartie AND F.ENTITY_ID = a.entite
                    AND F.PERIMETER_ID = a.perimeterid AND F.SICOVAM = a.code_emet
                    AND F.FEE_DATE = (
                        SELECT MAX(F.FEE_DATE)
                        FROM BO_LB_FEEMARK F
                        WHERE F.CTPY_ID = a.contrepartie AND F.ENTITY_ID = a.entite
                            AND F.PERIMETER_ID = a.perimeterid AND F.SICOVAM = a.code_emet
                            AND F.FEE_DATE <= DebCom
                            AND EXISTS (SELECT * FROM HISTOMVTS H
                                WHERE H.TYPE = 16 AND H.MVTIDENT = a.mvtident
                                    AND H.DATENEG = F.FEE_DATE
                                    AND H.BACKOFFICE IN (
                                        SELECT KSC.KERNEL_STATUS_ID FROM BO_KERNEL_STATUS_COMPONENT KSC
                                            JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                                            AND KSG.RECORD_TYPE = 1 AND KSG.NAME = 'All But Pending FO')));
                */
            exception
                when NO_DATA_FOUND then
                    nextCours.dateval := MIN_DATE;
                    nextCours.rate := a.cours; -- cours of MEP
                    WARN('No cours feemark found for sicovam {1} before {2}. Take cours of MEP', a.code_emet, DebCom);
            end;
            -- for performance optimization, do not use COURS IS NOT NULL in the following query
            -- 536.21
            open curCours for
                SELECT  H.DATEVAL,
                        NVL((SELECT D.CLOSE + CASE WHEN a.underlying_type != 'A' THEN NVL(D.COUPON, 0) ELSE 0 END
                        FROM DMHISTORIQUE D WHERE D.SICOVAM = a.code_emet
                            AND D.JOUR =
                                (SELECT MAX(DM.JOUR)
                                FROM DMHISTORIQUE DM WHERE DM.SICOVAM = a.code_emet AND DM.JOUR < H.DATEVAL)), 0) RATE
                FROM HISTOMVTS H
                WHERE H.TYPE = 16 AND H.MVTIDENT = a.mvtident AND H.DATEVAL > DebCom AND H.DATEVAL <= FinCom
                    AND ((H.BACKOFFICE IN (
                        SELECT KSC.KERNEL_STATUS_ID FROM BO_KERNEL_STATUS_COMPONENT KSC
                            JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                            AND KSG.RECORD_TYPE = 1 AND KSG.NAME = 'All But Pending FO') AND GP_SOURCE != SRC_PNL )
                            OR (H.BACKOFFICE NOT IN (
                                    SELECT KSC.KERNEL_STATUS_ID
                                    FROM BO_KERNEL_STATUS_COMPONENT KSC
                                        JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                                            AND KSG.NAME IN ('_STEP_Cancelled', '_MOD_Cancelled') AND KSG.RECORD_TYPE = 1) AND GP_SOURCE = SRC_PNL  ))
                ORDER BY 1;
            
            /* -- 536.21 to be uncommented when return to BO_LB_FEEMARK
            
            open curCours for
                SELECT F.FEE_DATE, F.SPOT FROM BO_LB_FEEMARK F
                WHERE F.CTPY_ID = a.contrepartie AND F.ENTITY_ID = a.entite
                    AND F.PERIMETER_ID = a.perimeterid AND F.SICOVAM = a.code_emet
                    AND F.FEE_DATE > DebCom AND F.FEE_DATE <= FinCom
                    AND EXISTS (SELECT * FROM HISTOMVTS H
                        WHERE H.TYPE = 16 AND H.MVTIDENT = a.mvtident
                            AND H.DATENEG = F.FEE_DATE
                            AND H.BACKOFFICE IN (
                                SELECT KSC.KERNEL_STATUS_ID FROM BO_KERNEL_STATUS_COMPONENT KSC
                                    JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                                    AND KSG.RECORD_TYPE = 1 AND KSG.NAME = 'All But Pending FO'))
                ORDER BY 1;
            */
        elsif a.amort = TC_FP_RP then
            -- Get the last cours (cours du jour)
            open curCours for
                SELECT DATEVAL, COURS FROM (
                    SELECT MIN_DATE DATEVAL, H.COURS, H.REFCON FROM HISTOMVTS H
                    WHERE H.REFCON = p_refcon
                    UNION
                    SELECT H.DATEVAL, H.COURS, H.REFCON FROM HISTOMVTS H
                    WHERE H.MVTIDENT = a.mvtident AND H.TYPE IN (303) -- SPOT MODIF
                        AND H.DATEVAL <= FinCom
                        --AND H.BACKOFFICE NOT IN (13, 29, 38, 268)
                        AND H.BACKOFFICE IN (
                            SELECT KSC.KERNEL_STATUS_ID FROM BO_KERNEL_STATUS_COMPONENT KSC
                                JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                                AND KSG.RECORD_TYPE = 1 AND KSG.NAME = 'All But Pending FO')
                ) ORDER BY DATEVAL, REFCON;
            FETCH curCours INTO nextCours;   
        end if;

        -- if different devise, get the XSicovam        
        if a.UNDERLYING_COTATIONCUR != a.BILLING_CUR then
            -- BS_LIB.EXCHANGE_RATE. instead of searching sicovam by reference = USD/EUR, EUR/USD, we get sicovam by code = USD. 
            begin
                if a.UNDERLYING_COTATIONCUR = EUR or a.BILLING_CUR = EUR then
                    -- BS_LIB.EXCHANGE_RATE
                    /*SELECT T.SICOVAM
                        into XSicovam
                    FROM TITRES T WHERE T.TYPE = 'E' AND T.REFERENCE = a.UNDERLYING_COTATIONCUR || '/' || a.BILLING_CUR;*/
                    XSicovam := STR_TO_DEVISE(CASE a.UNDERLYING_COTATIONCUR WHEN EUR THEN a.BILLING_CUR ELSE a.UNDERLYING_COTATIONCUR END);
                else
                    -- BS_LIB.EXCHANGE_RATE
                    /*SELECT T.SICOVAM
                        into XSicovam1
                    FROM TITRES T WHERE T.TYPE = 'E' AND T.REFERENCE = a.UNDERLYING_COTATIONCUR || '/' || EUR;
                    SELECT T.SICOVAM
                        into XSicovam2
                    FROM TITRES T WHERE T.TYPE = 'E' AND T.REFERENCE = EUR || '/' || a.BILLING_CUR;*/
                    XSicovam1 := STR_TO_DEVISE(a.UNDERLYING_COTATIONCUR);
                    XSicovam2 := STR_TO_DEVISE(a.BILLING_CUR);
                end if;
            exception
                when no_data_found then
                    ERROR('exchange of {1}/{2} not found', a.UNDERLYING_COTATIONCUR, a.BILLING_CUR);
            end;
            if a.UNDERLYING_COTATIONCUR = EUR or a.BILLING_CUR = EUR then
                if a.amort in (TC_FIXED_PRICE) then
                    -- get the last taux of MEP.dataval
                    begin
                        -- Get the last cours (cours du veille)
                        -- Attention: Never use Rownum with Order By
                        -- BS_LIB.EXCHANGE_RATE
                        /*SELECT D.JOUR, D.D*/
                        nextXRate.dateval := a.dateval;
                        nextXRate.rate := null;
                        while (nextXRate.rate is null)
                        loop
                            -- for performance optimization, do not use COURS IS NOT NULL in the following query
                            SELECT D.JOUR, CASE WHEN (DEV.INVERSERRIC = 1 AND a.BILLING_CUR = EUR)
                                                    OR (DEV.INVERSERRIC = 0 AND a.UNDERLYING_COTATIONCUR = EUR)
                                                THEN 1/D.D ELSE D.D END D
                                into nextXRate
                            FROM DMHISTORIQUE D
                                JOIN DEVISEV2 DEV ON DEV.CODE = D.SICOVAM
                            WHERE D.SICOVAM = XSicovam
                                    AND D.JOUR = (
                                        SELECT MAX(D.JOUR) FROM DMHISTORIQUE D
                                        WHERE D.SICOVAM = XSicovam AND D.JOUR < nextXRate.dateval);
                        end loop;
                    exception
                        when no_data_found then
                            nextXRate.dateval := MIN_DATE;
                            nextXRate.rate := 1;
                            ERROR('exchange for {1}/{2} not found before MEP.dateval {3}. sicovam = {4}',
                                a.UNDERLYING_COTATIONCUR, a.BILLING_CUR, a.dateval, XSicovam);
                    end;
                    currentFC.taux_change := nextXRate.rate;
                elsif a.amort in (TC_OPEN_BASIS, TC_FEE_MARK, TC_FP_RP) then
                    begin
                        -- Get the last cours (cours du veille)
                        -- Attention: Never use Rownum with Order By
                        -- BS_LIB.EXCHANGE_RATE
                        /*SELECT D.JOUR, D.D*/
                        nextXRate.dateval := case when nextCours.dateval = MIN_DATE then a.dateval else nextCours.dateval end;
                        nextXRate.rate := null;
                        while (nextXRate.rate is null)
                        loop
                            -- for performance optimization, do not use COURS IS NOT NULL in the following query
                            SELECT D.JOUR, CASE WHEN (DEV.INVERSERRIC = 1 AND a.BILLING_CUR = EUR)
                                                    OR (DEV.INVERSERRIC = 0 AND a.UNDERLYING_COTATIONCUR = EUR)
                                                THEN 1/D.D ELSE D.D END D
                                into nextXRate
                            FROM DMHISTORIQUE D
                                JOIN DEVISEV2 DEV ON DEV.CODE = D.SICOVAM
                            WHERE D.SICOVAM = XSicovam
                                AND D.JOUR = (
                                    SELECT MAX(D.JOUR) FROM DMHISTORIQUE D
                                    WHERE D.SICOVAM = XSicovam
                                        --AND D.JOUR < DebCom
                                        AND D.JOUR < nextXRate.dateval);
                        end loop;
                    exception
                        when no_data_found then
                            nextXRate.dateval := MIN_DATE;
                            nextXRate.rate := 1;
                            ERROR('exchange for {1}/{2} not found before {3}. sicovam = {4}',
                                a.UNDERLYING_COTATIONCUR, a.BILLING_CUR, DebCom, XSicovam);
                    end;
                    -- for performance optimization, do not use COURS IS NOT NULL in the following query
                    open curXRates for
                        -- BS_LIB.EXCHANGE_RATE
                        /*SELECT D.JOUR, D.D FROM DMHISTORIQUE D*/
                        SELECT D.JOUR, CASE WHEN (DEV.INVERSERRIC = 1 AND a.BILLING_CUR = EUR)
                                                OR (DEV.INVERSERRIC = 0 AND a.UNDERLYING_COTATIONCUR = EUR)
                                            THEN 1/D.D ELSE D.D END D
                        FROM DMHISTORIQUE D
                            JOIN DEVISEV2 DEV ON DEV.CODE = D.SICOVAM
                        WHERE D.SICOVAM = XSicovam AND D.JOUR < FinCom
                            --AND D.JOUR >= DebCom
                            AND D.JOUR >= case when nextCours.dateval = MIN_DATE then a.dateval else nextCours.dateval end
                        ORDER BY 1;
                end if;
            else
                if a.amort in (TC_FIXED_PRICE) then
                    -- get the last taux of MEP.dataval
                    begin
                        -- Get the last cours (cours du veille)
                        -- Attention: Never use Rownum with Order By
                        -- BS_LIB.EXCHANGE_RATE
                        /*SELECT D.JOUR, D.D*/
                        nextXRate1.dateval := a.dateval;
                        nextXRate1.rate := null;
                        while (nextXRate1.rate is null)
                        loop
                            -- for performance optimization, do not use COURS IS NOT NULL in the following query
                            SELECT D.JOUR, CASE WHEN DEV.INVERSERRIC = 1 THEN 1/D.D ELSE D.D END D
                                into nextXRate1
                            FROM DMHISTORIQUE D
                                JOIN DEVISEV2 DEV ON DEV.CODE = D.SICOVAM
                            WHERE D.SICOVAM = XSicovam1
                                    AND D.JOUR = (
                                        SELECT MAX(D.JOUR) FROM DMHISTORIQUE D
                                        WHERE D.SICOVAM = XSicovam1 AND D.JOUR < nextXRate1.dateval);
                        end loop;
                    exception
                        when no_data_found then
                            nextXRate1.dateval := MIN_DATE;
                            nextXRate1.rate := 1;
                            ERROR('exchange for {1}/{2} not found before MEP.dateval {3}. sicovam = {4}',
                                a.UNDERLYING_COTATIONCUR, EUR, a.dateval, XSicovam1);
                    end;
                    begin
                        -- Get the last cours (cours du veille)
                        -- Attention: Never use Rownum with Order By
                        -- BS_LIB.EXCHANGE_RATE
                        /*SELECT D.JOUR, D.D*/
                        nextXRate2.dateval := a.dateval;
                        nextXRate2.rate := null;
                        while (nextXRate2.rate is null)
                        loop
                            -- for performance optimization, do not use COURS IS NOT NULL in the following query
                            SELECT D.JOUR, CASE WHEN DEV.INVERSERRIC = 0 THEN 1/D.D ELSE D.D END D -- attention INVERSERRIC is in inverse direction
                                into nextXRate2
                            FROM DMHISTORIQUE D
                                JOIN DEVISEV2 DEV ON DEV.CODE = D.SICOVAM
                            WHERE D.SICOVAM = XSicovam2
                                AND D.JOUR = (
                                    SELECT MAX(D.JOUR) FROM DMHISTORIQUE D
                                    WHERE D.SICOVAM = XSicovam2 AND D.JOUR < nextXRate2.dateval);
                        end loop;
                    exception
                        when no_data_found then
                            nextXRate2.dateval := MIN_DATE;
                            nextXRate2.rate := 1;
                            ERROR('exchange for {1}/{2} not found before MEP.dateval {3}. sicovam = {4}',
                                EUR, a.BILLING_CUR, a.dateval, XSicovam2);
                    end;
                    currentFC.taux_change := nextXRate1.rate * nextXRate2.rate;
                elsif a.amort in (TC_OPEN_BASIS, TC_FEE_MARK, TC_FP_RP) then
                    begin
                        -- Get the last cours (cours du veille)
                        -- Attention: Never use Rownum with Order By
                        -- BS_LIB.EXCHANGE_RATE
                        /*SELECT D.JOUR, D.D*/
                        nextXRate1.dateval := case when nextCours.dateval = MIN_DATE then a.dateval else nextCours.dateval end;
                        nextXRate1.rate := null;
                        while (nextXRate1.rate is null)
                        loop
                            -- for performance optimization, do not use COURS IS NOT NULL in the following query
                            SELECT D.JOUR, CASE WHEN DEV.INVERSERRIC = 1 THEN 1/D.D ELSE D.D END D
                                into nextXRate1
                            FROM DMHISTORIQUE D
                                JOIN DEVISEV2 DEV ON DEV.CODE = D.SICOVAM
                            WHERE D.SICOVAM = XSicovam1
                                AND D.JOUR = (
                                    SELECT MAX(D.JOUR) FROM DMHISTORIQUE D
                                    WHERE D.SICOVAM = XSicovam1
                                        --AND D.JOUR < DebCom
                                        AND D.JOUR < nextXRate1.dateval);
                        end loop;
                    exception
                        when no_data_found then
                            nextXRate1.dateval := MIN_DATE;
                            nextXRate1.rate := 1;
                            ERROR('exchange for {1}/{2} not found before {3}. sicovam = {4}',
                                a.UNDERLYING_COTATIONCUR, EUR, DebCom, XSicovam1);
                    end;
                    begin
                        -- Get the last cours (cours du veille)
                        -- Attention: Never use Rownum with Order By
                        -- BS_LIB.EXCHANGE_RATE
                        /*SELECT D.JOUR, D.D*/
                        nextXRate2.dateval := case when nextCours.dateval = MIN_DATE then a.dateval else nextCours.dateval end;
                        nextXRate2.rate := null;
                        while (nextXRate2.rate is null)
                        loop
                            -- for performance optimization, do not use COURS IS NOT NULL in the following query
                            SELECT D.JOUR, CASE WHEN DEV.INVERSERRIC = 0 THEN 1/D.D ELSE D.D END D -- attention INVERSERRIC is in inverse direction
                                into nextXRate2
                            FROM DMHISTORIQUE D
                                JOIN DEVISEV2 DEV ON DEV.CODE = D.SICOVAM
                            WHERE D.SICOVAM = XSicovam2
                                AND D.JOUR = (
                                    SELECT MAX(D.JOUR) FROM DMHISTORIQUE D
                                    WHERE D.SICOVAM = XSicovam2
                                        --AND D.JOUR < DebCom
                                        AND D.JOUR < nextXRate2.dateval);
                        end loop;
                    exception
                        when no_data_found then
                            nextXRate2.dateval := MIN_DATE;
                            nextXRate2.rate := 1;
                            ERROR('exchange for {1}/{2} not found before {3}. sicovam = {4}',
                                EUR, a.BILLING_CUR, DebCom, XSicovam2);
                    end;
                    -- for performance optimization, do not use COURS IS NOT NULL in the following query
                    open curXRates1 for
                        -- BS_LIB.EXCHANGE_RATE
                        /*SELECT D.JOUR, D.D FROM DMHISTORIQUE D*/
                        SELECT D.JOUR, CASE WHEN DEV.INVERSERRIC = 1 THEN 1/D.D ELSE D.D END D
                        FROM DMHISTORIQUE D
                            JOIN DEVISEV2 DEV ON DEV.CODE = D.SICOVAM
                        WHERE D.SICOVAM = XSicovam1 AND D.JOUR < FinCom
                            --AND D.JOUR >= DebCom
                            AND D.JOUR >= case when nextCours.dateval = MIN_DATE then a.dateval else nextCours.dateval end
                        ORDER BY 1;
                    -- for performance optimization, do not use COURS IS NOT NULL in the following query
                    open curXRates2 for
                        -- BS_LIB.EXCHANGE_RATE
                        /*SELECT D.JOUR, D.D FROM DMHISTORIQUE D*/
                        SELECT D.JOUR, CASE WHEN DEV.INVERSERRIC = 0 THEN 1/D.D ELSE D.D END D -- attention INVERSERRIC is in inverse direction
                        FROM DMHISTORIQUE D
                            JOIN DEVISEV2 DEV ON DEV.CODE = D.SICOVAM
                        WHERE D.SICOVAM = XSicovam2 AND D.JOUR < FinCom
                            --AND D.JOUR >= DebCom
                            AND D.JOUR >= case when nextCours.dateval = MIN_DATE then a.dateval else nextCours.dateval end
                        ORDER BY 1;
                end if;
            end if;
        end if;
        
        --For debug
        --a.tp_value := TP_FT;
        DEBUG('tp_value = {1}, XSicovam = {2}', a.tp_value, XSicovam);
        
        --cursor for calculate QTY
        open curQty for
            SELECT 
                CASE WHEN a.tp_value = TP_FT THEN
                        CASE WHEN NVL(H.DELIVERY_DATE, H.DATEVAL) <= DebCom THEN DebCom
                             ELSE NVL(H.DELIVERY_DATE, H.DATEVAL) END  
                     WHEN a.tp_value = TP_FR THEN
                        CASE WHEN H.REAL_DELIVERY_DATE IS NULL OR H.REAL_DELIVERY_DATE = NULL_DATE
                                OR H.REAL_DELIVERY_DATE <= DebCom THEN DebCom
                             ELSE H.REAL_DELIVERY_DATE END END,
                NVL(SUM(H.QUANTITE), 0)
            FROM HISTOMVTS H
                JOIN BUSINESS_EVENTS BE ON BE.ID = H.TYPE AND BE.COMPTA = 1
            WHERE H.MVTIDENT = a.mvtident
                AND ((H.BACKOFFICE IN (
                SELECT KSC.KERNEL_STATUS_ID FROM BO_KERNEL_STATUS_COMPONENT KSC
                    JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                    AND KSG.RECORD_TYPE = 1 AND KSG.NAME = 'All But Pending FO') AND GP_SOURCE != SRC_PNL )
                    OR (H.BACKOFFICE NOT IN (
                            SELECT KSC.KERNEL_STATUS_ID
                            FROM BO_KERNEL_STATUS_COMPONENT KSC
                                JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                                    AND KSG.NAME IN ('_STEP_Cancelled', '_MOD_Cancelled') AND KSG.RECORD_TYPE = 1) AND GP_SOURCE = SRC_PNL  ))            
                AND ((a.tp_value = TP_FT AND NVL(H.DELIVERY_DATE, H.DATEVAL) <= FinCom) OR 
                     (a.tp_value = TP_FR AND H.REAL_DELIVERY_DATE IS NOT NULL AND H.REAL_DELIVERY_DATE != NULL_DATE
                        AND H.REAL_DELIVERY_DATE <= FinCom))
            GROUP BY
                CASE WHEN a.tp_value = TP_FT THEN
                        CASE WHEN NVL(H.DELIVERY_DATE, H.DATEVAL) <= DebCom THEN DebCom
                             ELSE NVL(H.DELIVERY_DATE, H.DATEVAL) END  
                     WHEN a.tp_value = TP_FR THEN
                        CASE WHEN H.REAL_DELIVERY_DATE IS NULL OR H.REAL_DELIVERY_DATE = NULL_DATE
                                OR H.REAL_DELIVERY_DATE <= DebCom THEN DebCom
                             ELSE H.REAL_DELIVERY_DATE END END
            ORDER BY 1;
        
        -- qty
        --currentFC.qty := a.quantite;
        currentFC.qty := 0;
        
        -- mvtident
        currentFC.mvtident := a.mvtident;
        
        -- obligation cotée en pourcentage
        underlyingNomUnit := 1;
        if a.quotation_type = 2 and a.underlying_type != 'A' then -- equivalent avec UNDERLYING_PRICE_TYPE = 2
            if a.underlying_type in ('O', 'D') then
                underlyingNomUnit := case a.underlying_nominal when 0 then 1 else a.underlying_nominal end;                             
            elsif a.underlying_type in ('N') then
                underlyingNomUnit := case a.underlying_nbtitres when 0 then 1 else a.underlying_nbtitres end;
            else
                underlyingNomUnit := 1;
            end if;
        end if;
            
        for i in 0..(FinCom - DebCom)
        loop
            d1 := DebCom + i;
            
            if a.amort != TC_SET_PRICE then
                while (curRates%FOUND or curRates%FOUND is null) and (nextRate.dateval <= d1 or nextRate.dateval is null)
                loop
                    currentRate := nextRate;
                    loop
                        FETCH curRates INTO nextRate;
                        exit when curRates%NOTFOUND or nextRate.rate is not null;
                    end loop;
                    DEBUG('date = {1}, dateval = {2}, rate = {3}', d1, currentRate.dateval, currentRate.rate);
                end loop;
                currentFC.taux_com := currentRate.rate;
            else
                currentFC.taux_com := 0;
            end if;

            -- get the quantite
            while (curQty%FOUND or curQty%FOUND is null) and (nextQty.dt <= d1 or nextQty.dt is null)
            loop
                currentQty.num := nextQty.num;
                currentQty.dt := nextQty.dt;
                FETCH curQty INTO nextQty;
                DEBUG('date = {1}, dt = {2}, qty = {3}', d1, currentQty.dt, currentQty.num);
                currentFC.qty := currentFC.qty + nvl(currentQty.num, 0);
            end loop;
            
            -- prix du sous-jacent
            if a.amort in (TC_FIXED_PRICE) then
                currentFC.prix := a.cours; -- currentFC.prix does not change
            elsif a.amort in (TC_OPEN_BASIS, TC_FEE_MARK, TC_FP_RP) then
                -- For optimization, do not use one SELECT to retrieve cours 
                while (curCours%FOUND or curCours%FOUND is null)
                    and ((a.amort in (TC_OPEN_BASIS) and nextCours.dateval < d1)
                      or (a.amort in (TC_FEE_MARK, TC_FP_RP) and nextCours.dateval <= d1))
                loop
                    newPrix := true; -- nouveau prix récupéré, donc un nouveau taux de change est à récupérer
                    if a.amort in (TC_OPEN_BASIS) then
                        newPrixDate := d1;
                    else
                        newPrixDate := case nextCours.dateval when MIN_DATE then a.dateval else nextCours.dateval end;
                    end if;
                    currentCours := nextCours;
                    currentCours.dateval := newPrixDate;
                    loop
                        FETCH curCours INTO nextCours;
                        exit when curCours%NOTFOUND or nextCours.rate is not null;
                    end loop;
                    if a.amort = TC_OPEN_BASIS then
                        DEBUG('date = {1}, jour = {2}, open_basis = {3}', d1, currentCours.dateval, currentCours.rate);
                    elsif a.amort = TC_FEE_MARK then
                        DEBUG('date = {1}, fee_date = {2}, fee_mark = {3}', d1, currentCours.dateval, currentCours.rate);
                    elsif a.amort = TC_FP_RP then
                        DEBUG('date = {1}, dateval = {2}, fixed_price_revisable_price = {3}', d1, currentCours.dateval, currentCours.rate);
                    end if;
                end loop;
                currentFC.prix := currentCours.rate;
            elsif a.amort = TC_SET_PRICE then
                if GP_PERIODICITE = PER_M then
                    currentFC.prix := 0;
                elsif GP_PERIODICITE = PER_I then
                    currentFC.prix := a.cours; -- currentFC.prix does not change
                end if;
            else
                ERROR('Commission type is incorrect ({1})', a.amort);
            end if;
            
            --marché coté en centième
            if a.unitedecotation = 2 then
                currentFC.prix := currentFC.prix / 100;
            end if;
            
            --marché coté en pourcentage
            if a.quotation_type = 2 and a.underlying_type != 'A' then -- equivalent avec UNDERLYING_PRICE_TYPE = 2
                currentFC.prix := currentFC.prix / 100;
            end if;
            
            -- Cherher le taux de change 
            if a.UNDERLYING_COTATIONCUR != a.BILLING_CUR then
                if newPrix then -- si ce prix est nouveau, vient d'être récupéré, on récupère le change
                    newPrix := false;
                    if a.amort in (TC_FIXED_PRICE) then
                        null; -- currentFC.taux_change does not change
                    elsif a.amort in (TC_OPEN_BASIS, TC_FEE_MARK, TC_FP_RP) then
                        if a.UNDERLYING_COTATIONCUR = EUR or a.BILLING_CUR = EUR then
                            --while (curXRates%FOUND or curXRates%FOUND is null) and nextXRate.dateval < d1
                            while (curXRates%FOUND or curXRates%FOUND is null) and nextXRate.dateval < newPrixDate
                            loop
                                currentXRate := nextXRate;
                                loop
                                    FETCH curXRates INTO nextXRate;
                                    exit when curXRates%NOTFOUND or nextXRate.rate is not null;
                                end loop;
                                --DEBUG('date = {1}, jour = {2}, taux_change = {3}', d1, currentXRate.dateval, currentXRate.rate);
                                DEBUG('date = {1}, jour = {2}, taux_change = {3}', newPrixDate, currentXRate.dateval, currentXRate.rate);
                            end loop;
                            currentFC.taux_change := currentXRate.rate;
                        else
                            --while (curXRates1%FOUND or curXRates1%FOUND is null) and nextXRate1.dateval < d1
                            while (curXRates1%FOUND or curXRates1%FOUND is null) and nextXRate1.dateval < newPrixDate
                            loop
                                currentXRate1 := nextXRate1;
                                loop
                                    FETCH curXRates1 INTO nextXRate1;
                                    exit when curXRates1%NOTFOUND or nextXRate1.rate is not null;
                                end loop;
                                --DEBUG('date = {1}, jour = {2}, taux_change1 = {3}', d1, currentXRate1.dateval, currentXRate1.rate);
                                DEBUG('date = {1}, jour = {2}, taux_change1 = {3}', newPrixDate, currentXRate1.dateval, currentXRate1.rate);
                            end loop;
                            --while (curXRates2%FOUND or curXRates2%FOUND is null) and nextXRate2.dateval < d1
                            while (curXRates2%FOUND or curXRates2%FOUND is null) and nextXRate2.dateval < newPrixDate
                            loop
                                currentXRate2 := nextXRate2;
                                loop
                                    FETCH curXRates2 INTO nextXRate2;
                                    exit when curXRates2%NOTFOUND or nextXRate2.rate is not null;
                                end loop;
                                --DEBUG('date = {1}, jour = {2}, taux_change2 = {3}', d1, currentXRate2.dateval, currentXRate2.rate);
                                DEBUG('date = {1}, jour = {2}, taux_change2 = {3}', newPrixDate, currentXRate2.dateval, currentXRate2.rate);
                            end loop;
                            currentFC.taux_change := currentXRate1.rate * currentXRate2.rate;
                        end if;
                    end if;
                end if;
            else
                currentFC.taux_change := 1;
            end if;
            
            -- Prix en devise de facturation
            currentFC.prix_devise_fact := currentFC.prix * currentFC.taux_change;
            
            -- value
            currentFC.value := currentFC.prix_devise_fact * currentFC.qty * underlyingNomUnit;
            --DEBUG('prix_devise_fact={1}, qty={2}, underlyingNomUnit={3}, value={4}',
            --    currentFC.prix_devise_fact, currentFC.qty, underlyingNomUnit, currentFC.value);
            
            -- if facturation condition changed. increse one element
            if lastFC.qty is null or (lastFC.qty != currentFC.qty or lastFC.taux_com != currentFC.taux_com or
                lastFC.prix != currentFC.prix or lastFC.taux_change != currentFC.taux_change) or GP_SOURCE = SRC_PNL then
                c.extend;
                idx := c.count;
                c(idx) := currentFC;
                c(idx).qty := c(idx).qty * underlyingNomUnit;
                c(idx)."from" := d1;
                c(idx)."to" := d1;
                c(idx).days := 1;
                lastFC := currentFC;
            else
                c(idx)."to" := d1;
                c(idx).days := c(idx).days + 1; -- increase one days
            end if;
            
            DEBUG('from = {1}, to = {2}, qty = {3}, prix = {4}, taux_com = {5}, taux_change = {6}, prix_devise_fact = {7}, value = {8}, days = {9}',
                c(idx)."from", c(idx)."to", c(idx).qty, c(idx).prix, c(idx).taux_com, c(idx).taux_change, c(idx).prix_devise_fact, c(idx).value, c(idx).days);
        end loop;
        
        if curXRates%ISOPEN then
            close curXRates;
        end if;
        if curXRates1%ISOPEN then
            close curXRates1;
        end if;
        if curXRates2%ISOPEN then
            close curXRates2;
        end if;
        if a.amort in (TC_OPEN_BASIS, TC_FEE_MARK, TC_FP_RP) then
            close curCours;
        end if;
        if a.amort != TC_SET_PRICE then
            close curRates;
        end if;
        close curQty;
        
        -- calcul montant_fee and TotalFees
        for i in 1..c.count
        loop
            c(i).montant_fee :=
                c(i).qty * c(i).prix * c(i).taux_change *
                (c(i).taux_com / 100) * c(i).days / calc_basis;
            -- obligation cotée en pourcentage
            --if a.quotation_type = 2 and a.underlying_type != 'A' then -- equivalent avec UNDERLYING_PRICE_TYPE = 2
            --    c(i).montant_fee := c(i).montant_fee / 100;
            --end if;
            TotalFees := TotalFees + c(i).montant_fee;
            
            --round
            c(i).prix := round(c(i).prix, MATH_ROUND);
            c(i).taux_change := round(c(i).taux_change, MATH_ROUND);
            c(i).prix_devise_fact := round(c(i).prix_devise_fact, MATH_ROUND);
            c(i).value := round(c(i).value, MATH_ROUND);
            c(i).montant_fee := round(c(i).montant_fee, MATH_ROUND);
        end loop;
        if a.amort = TC_SET_PRICE and GP_PERIODICITE = PER_I then
            c(c.count).montant_fee := a.commission;
            TotalFees := a.commission;
        end if;
        
        -- insert into global temporary table
        if p_isFilled then
            DEBUG('insert into BL_FEES. {1} rows', c.count);
            DELETE BL_FEES WHERE MVTIDENT = a.mvtident;
            for i in 1..c.count
            loop
                INSERT INTO BL_FEES VALUES c(i);
            end loop;
        end if;
        
        -- FeePayed
        if GP_PERIODICITE = PER_M then
            SELECT NVL(SUM(H.MONTANT), 0)
                into FeePayed
            FROM HISTOMVTS H
            WHERE H.TYPE IN (7)
                AND H.BACKOFFICE IN (
                    SELECT KSC.KERNEL_STATUS_ID FROM BO_KERNEL_STATUS_COMPONENT KSC
                        JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                        AND KSG.RECORD_TYPE = 1 AND KSG.NAME = 'All But Pending FO')
                AND H.MVTIDENT = a.mvtident AND H.DATENEG < DebCom;
        elsif GP_PERIODICITE = PER_I then
            SELECT NVL(SUM(H.MONTANT), 0)
                into FeePayed
            FROM HISTOMVTS H
            WHERE H.TYPE IN (7, 700)
                AND H.BACKOFFICE NOT IN (
                    SELECT KSC.KERNEL_STATUS_ID
                    FROM BO_KERNEL_STATUS_COMPONENT KSC
                        JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                            AND KSG.NAME IN ('_STEP_Cancelled', '_MOD_Cancelled') AND KSG.RECORD_TYPE = 1)
                AND H.MVTIDENT = a.mvtident
                AND H.DATENEG < p_dateneg_c;
                --AND H.DATEVAL < p_datesitu;
        end if;
            
        -- Balance of Minimum Fee
        if (a.coupon1 is not null and a.coupon1 != 0) or (GP_PERIODICITE = PER_I) then -- minimum fee exist or Infine
            SELECT COUNT(*)
                into rc
            FROM HISTOMVTS HC WHERE HC.TYPE IN (102, 501) AND HC.MVTIDENT = a.mvtident AND 
                ((a.tp_value = TP_FT AND NVL(HC.DELIVERY_DATE, HC.DATEVAL) - 1 BETWEEN DebCom AND FinCom) OR
                 (a.tp_value = TP_FR AND HC.REAL_DELIVERY_DATE IS NOT NULL AND HC.REAL_DELIVERY_DATE != NULL_DATE
                    AND HC.REAL_DELIVERY_DATE - 1 BETWEEN DebCom AND FinCom));
            DEBUG('MinFee exist. Closing Count = {1}', rc);   
            if rc > 0 then -- exist closing of the month
                if abs(FeePayed) + abs(TotalFees) < abs(a.coupon1) then
                    Balance_MinFee := abs(a.coupon1) - abs(FeePayed) - abs(TotalFees);
                else
                    Balance_MinFee := 0;
                end if;
                if a.quantite < 0 then
                    Balance_MinFee := Balance_MinFee * -1;
                end if;
            end if;            
        end if;

    <<FIN>>        
        ComList := c;
        INFO('CALCUL_COMMISSION.END(ComList.Count = {1}, FeePayed = {2}, Balance_MinFee = {3}, TotalFees = {4})',
            ComList.count, FeePayed, Balance_MinFee, TotalFees);
    EXCEPTION
        WHEN OTHERS THEN 
            ERROR('CALCUL_COMMISSION. code = {1}, message = {2}, backtrace={3}', SQLCODE, SQLERRM, DBMS_UTILITY.FORMAT_ERROR_BACKTRACE);
            RAISE;
    END;
    
    PROCEDURE CALCUL_REMUN_COLLAT_POOL(
        p_refcon        in  HISTOMVTS.REFCON%TYPE,
        DebReb          in  date,
        FinReb          in  date,
        RebPoolList     out REBATE_POOL_LIST,
        TotalRebates    out number,
        p_isFilled      in  boolean default false)
    AS
        TYPE AGREEMENT_INFO IS RECORD (
            --ADM
            mvtident                HISTOMVTS.MVTIDENT%TYPE,
            entite                  HISTOMVTS.ENTITE%TYPE,
            contrepartie            HISTOMVTS.CONTREPARTIE%TYPE,
            
            --TITRES
            perimeterid             TITRES.PERIMETERID%TYPE,
            devisectt               TITRES.DEVISECTT%TYPE,
            typederive              TITRES.TYPEDERIVE%TYPE,
            taux                    TITRES.TAUX%TYPE,
            taux_var                TITRES.TAUX_VAR%TYPE,
            reference_2             TITRES.REFERENCE%TYPE,
            sicovam_2               TITRES.SICOVAM%TYPE,
            
            --OTHERS
            tp_value                TIERSPROPERTIES.VALUE%TYPE
        );
        a                           AGREEMENT_INFO;
        
        c                   REBATE_POOL_LIST := REBATE_POOL_LIST();
        d1                  date;
        idx                 integer := 0;
        cash1               number;
        cash2               number;
        calc_basis          number;
        
        --cursor contains Rates
        curRates            SYS_REFCURSOR;
        currentRate         RATE_INFO;
        nextRate            RATE_INFO;
        
        --cursor contains Spreads
        curSpreads          SYS_REFCURSOR;
        currentSpread       RATE_INFO;
        nextSpread          RATE_INFO;
        
        -- Cursor to retrieve Cash1, Cash2
        curCash1            SYS_REFCURSOR;
        currentCash1        DATE_NUM;
        nextCash1           DATE_NUM;
        curCash2            SYS_REFCURSOR;
        currentCash2        DATE_NUM;
        nextCash2           DATE_NUM;
        
        -- Facture Condition
        lastFC              BL_REBATES_POOL%ROWTYPE;
        currentFC           BL_REBATES_POOL%ROWTYPE;
    BEGIN
        INFO('CALCUL_REMUN_COLLAT_POOL(refcon = {1}, DebReb = {2}, FinReb = {3})',
                p_refcon, DebReb, FinReb);
                
        TotalRebates := 0;
                
        SELECT H.MVTIDENT, H.ENTITE, H.CONTREPARTIE,
            T.PERIMETERID, T.DEVISECTT, T.TYPEDERIVE, T.TAUX, T.TAUX_VAR, 
            T2.REFERENCE, T2.SICOVAM, decode (GP_SOURCE, SRC_PNL, TP_FT, NVL(TP.VALUE, TP_FD)) --PNL : Facturation en dates Théoriques
            into a
        FROM HISTOMVTS H
            JOIN TITRES T ON T.SICOVAM = H.SICOVAM
            LEFT JOIN TITRES T2 ON T2.SICOVAM = T.TAUX_VAR
            LEFT JOIN TIERSPROPERTIES TP ON TP.CODE = H.CONTREPARTIE AND TP.NAME = TP_BILLING
        WHERE H.REFCON = p_refcon;
        
        INFO('mvtident = {1}, entite = {2}, contrepartie = {3}', a.mvtident, a.entite, a.contrepartie);
        INFO('perimeterid = {1}, devisectt = {2}, typederive = {3}, taux = {4}, taux_var = {5}',
            a.perimeterid, a.devisectt, a.typederive, a.taux, a.taux_var);  
        INFO('reference_2 = {1}, sicovam_2 = {2}, tiersproperties = {3}', a.reference_2, a.sicovam_2, a.tp_value);
            
        calc_basis := case a.typederive when 1 then 360 when 2 then 365 when 6 then 365.25 when 7 then 365 else 360 end;
        if a.typederive is null then
            WARN('TIME BASIS non gere. Par defaut : Actual/360');
        elsif a.typederive not in (1, 2, 6, 7) then
            WARN('TIME BASIS not supported');
        end if;
        
        --for debug
        --a.tp_value := TP_FT;
            
        if a.sicovam_2 is not null then
            DEBUG('taux variable = {1}', a.reference_2);
            begin
                -- Get the last cours (cours du jour, non pas du veille)
                -- Attention: Never use Rownum with Order By
                nextRate.dateval := DebReb + 1;
                nextRate.rate := null;
                while (nextRate.rate is null)
                loop
                    -- for performance optimization, do not use COURS IS NOT NULL in the following query
                    SELECT D.JOUR, D.D
                        into nextRate
                    FROM DMHISTORIQUE D
                    WHERE D.SICOVAM = a.sicovam_2
                        AND D.JOUR = (
                            SELECT MAX(D.JOUR)
                            FROM DMHISTORIQUE D
                            WHERE D.SICOVAM = a.sicovam_2
                                AND D.JOUR <= nextRate.dateval - 1);
                end loop;
            EXCEPTION
                when NO_DATA_FOUND then
                    nextRate.dateval := MIN_DATE;
                    nextRate.rate := 0;
                    ERROR('No cours found for sicovam = {1} before {2}', a.sicovam_2, DebReb);
            end;
            -- for performance optimization, do not use COURS IS NOT NULL in the following query
            open curRates for
                SELECT D.JOUR, D.D FROM DMHISTORIQUE D
                WHERE D.SICOVAM = a.sicovam_2 AND D.JOUR > DebReb AND D.JOUR <= FinReb
                ORDER BY 1;
        end if;
        
        --Spreads
        begin
            SELECT BEGIN_DATE, MAX(SHORT_MARGIN)
                into nextSpread
            FROM (
                SELECT MC.BEGIN_DATE, MC.SHORT_MARGIN
                FROM BO_PE_MARGIN_CALL MC
                WHERE MC.CTPY_ID = a.contrepartie AND MC.ENTITY_ID = a.entite AND MC.PERIMETER_ID = a.perimeterid
                    AND MC.CURRENCY = a.devisectt AND MC.RATE = a.taux_var
                    AND MC.BEGIN_DATE = (
                        SELECT MAX(MC.BEGIN_DATE)
                        FROM BO_PE_MARGIN_CALL MC
                        WHERE MC.CTPY_ID = a.contrepartie AND MC.ENTITY_ID = a.entite AND MC.PERIMETER_ID = a.perimeterid
                            AND MC.CURRENCY = a.devisectt AND MC.RATE = a.taux_var
                            AND MC.BEGIN_DATE <= DebReb)
            ) GROUP BY BEGIN_DATE;
        exception
            when NO_DATA_FOUND then
                nextSpread.dateval := MIN_DATE;
                nextSpread.rate := 0;
                ERROR('Spread non défini avant {1} (contrepartie = {2}, entite = {3}, convention = {4}, devise = {5}, taux_variable = {6}',
                    DebReb, a.contrepartie, a.entite, a.perimeterid, a.devisectt, a.taux_var);
        end;
        open curSpreads for
            SELECT BEGIN_DATE, MAX(SHORT_MARGIN)
            FROM (
                SELECT MC.BEGIN_DATE, MC.SHORT_MARGIN
                FROM BO_PE_MARGIN_CALL MC
                WHERE MC.CTPY_ID = a.contrepartie AND MC.ENTITY_ID = a.entite AND MC.PERIMETER_ID = a.perimeterid
                    AND MC.CURRENCY = a.devisectt AND MC.RATE = a.taux_var AND MC.BEGIN_DATE > DebReb AND MC.BEGIN_DATE <= FinReb
            ) GROUP BY BEGIN_DATE
            ORDER BY 1;
                
        --cursor for calculate Cash1
        --TODO: perf
        open curCash1 for
            SELECT
                CASE WHEN a.tp_value = TP_FT THEN
                        CASE WHEN H.DATEVAL IS NOT NULL AND H.DATEVAL <= DebReb THEN DebReb
                             ELSE H.DATEVAL END
                     WHEN a.tp_value = TP_FR THEN
                        CASE WHEN H.REAL_DATEVAL IS NULL OR H.REAL_DATEVAL = NULL_DATE
                                OR H.REAL_DATEVAL <= DebReb THEN DebReb
                             ELSE H.REAL_DATEVAL END END,
                NVL(SUM(H.MONTANT), 0)
            FROM HISTOMVTS H
                JOIN TITRES T ON T.SICOVAM = H.SICOVAM
                    AND T.TYPE IN 'C' AND T.MODELE = 'Collateral' AND T.AFFECTATION IN (11, 60) -- POOL
                    AND T.PERIMETERID = a.perimeterid
                    AND T.DEVISECTT = a.devisectt
                    AND NVL(T.TAUX_VAR, 0) = NVL(a.taux_var, 0)
                JOIN NATIXIS_FOLIO_SECTION_ENTITE FSE ON FSE.IDENT = H.OPCVM AND FSE.SECTION != '799' -- not in section simulation
            WHERE H.TYPE IN (16)
                AND ((H.BACKOFFICE IN (
                        SELECT KSC.KERNEL_STATUS_ID FROM BO_KERNEL_STATUS_COMPONENT KSC
                            JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                            AND KSG.RECORD_TYPE = 1 AND KSG.NAME = 'All But Pending FO') AND GP_SOURCE != SRC_PNL )
                            OR (H.BACKOFFICE NOT IN (
                                    SELECT KSC.KERNEL_STATUS_ID
                                    FROM BO_KERNEL_STATUS_COMPONENT KSC
                                        JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                                            AND KSG.NAME IN ('_STEP_Cancelled', '_MOD_Cancelled') AND KSG.RECORD_TYPE = 1) AND GP_SOURCE = SRC_PNL  ))
                AND (H.MIRROR_REFERENCE IS NULL OR H.MIRROR_REFERENCE IN (0, -1)) -- si deal mirroirs, prendre que les deals peres
                AND H.ENTITE = a.entite AND H.CONTREPARTIE = a.contrepartie
                AND ((a.tp_value = TP_FT AND H.DATEVAL IS NOT NULL AND H.DATEVAL <= FinReb) OR 
                     (a.tp_value = TP_FR AND H.REAL_DATEVAL IS NOT NULL AND H.REAL_DATEVAL != NULL_DATE
                        AND H.REAL_DATEVAL <= FinReb))
            GROUP BY
                CASE WHEN a.tp_value = TP_FT THEN
                        CASE WHEN H.DATEVAL IS NOT NULL AND H.DATEVAL <= DebReb THEN DebReb
                             ELSE H.DATEVAL END
                     WHEN a.tp_value = TP_FR THEN
                        CASE WHEN H.REAL_DATEVAL IS NULL OR H.REAL_DATEVAL = NULL_DATE
                                OR H.REAL_DATEVAL <= DebReb THEN DebReb
                             ELSE H.REAL_DATEVAL END END
            ORDER BY 1;
        
        --cursor for calculate Cash2
        --TODO: perf
        open curCash2 for
            SELECT
                CASE WHEN a.tp_value = TP_FT THEN
                        CASE WHEN H.DATEVAL IS NOT NULL AND H.DATEVAL <= DebReb THEN DebReb
                             ELSE H.DATEVAL END
                     WHEN a.tp_value = TP_FR THEN
                        CASE WHEN H.REAL_DATEVAL IS NULL OR H.REAL_DATEVAL = NULL_DATE
                                OR H.REAL_DATEVAL <= DebReb THEN DebReb
                             ELSE H.REAL_DATEVAL END END,
                NVL(SUM(H.MONTANT), 0)
            FROM HISTOMVTS H
                JOIN TITRES T ON T.SICOVAM = H.SICOVAM
                    AND T.TYPE IN ('P')                   -- Instrument de P/E
                    AND T.AFFECTATION IN (60)             -- L/B Cash Collat
                    AND T.CAPITALISE2 = 2                 -- Collatéral est géré en POOL
                    AND T.PERIMETERID = a.perimeterid
                    AND T.DEVISECTT = a.devisectt
                    AND NVL(T.TAUX_VAR, 0) = NVL(a.taux_var, 0)
                JOIN NATIXIS_FOLIO_SECTION_ENTITE FSE ON FSE.IDENT = H.OPCVM AND FSE.SECTION != '799' -- not in section simulation
                LEFT JOIN HISTOMVTS HO ON HO.MVTIDENT = H.MVTIDENT AND HO.TYPE IN (1, 500)
                    --AND HO.DATENEG <= FinReb 
                JOIN BUSINESS_EVENTS BE ON BE.ID = H.TYPE AND BE.COMPTA = 1
            WHERE H.ENTITE = a.entite AND H.CONTREPARTIE = a.contrepartie
                AND ((H.BACKOFFICE IN (
                    SELECT KSC.KERNEL_STATUS_ID FROM BO_KERNEL_STATUS_COMPONENT KSC
                        JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                        AND KSG.RECORD_TYPE = 1 AND KSG.NAME = 'All But Pending FO') AND GP_SOURCE != SRC_PNL )
                        OR (H.BACKOFFICE NOT IN (
                                SELECT KSC.KERNEL_STATUS_ID
                                FROM BO_KERNEL_STATUS_COMPONENT KSC
                                    JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                                        AND KSG.NAME IN ('_STEP_Cancelled', '_MOD_Cancelled') AND KSG.RECORD_TYPE = 1) AND GP_SOURCE = SRC_PNL  ))
                AND (HO.MIRROR_REFERENCE IS NULL OR HO.MIRROR_REFERENCE IN (0, -1)) -- si deal mirroirs, prendre que les deals peres
                --AND NOT EXISTS
                --    (SELECT * FROM HISTOMVTS HC
                --    WHERE HC.MVTIDENT = H.MVTIDENT
                --        AND HC.BACKOFFICE IN (
                --            SELECT KSC.KERNEL_STATUS_ID FROM BO_KERNEL_STATUS_COMPONENT KSC
                --                JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                --                AND KSG.RECORD_TYPE = 1 AND KSG.NAME = 'All But Pending FO')
                --        AND HC.TYPE IN (102, 501)
                --        AND ((a.tp_value = TP_FT AND HC.DATEVAL < DebReb) OR
                --             (a.tp_value = TP_FR AND HC.REAL_DATEVAL IS NOT NULL AND HC.REAL_DATEVAL != NULL_DATE
                --                  AND HC.REAL_DATEVAL < DebReb)))
                AND ((a.tp_value = TP_FT AND H.DATEVAL IS NOT NULL AND H.DATEVAL <= FinReb) OR 
                     (a.tp_value = TP_FR AND H.REAL_DATEVAL IS NOT NULL AND H.REAL_DATEVAL != NULL_DATE
                        AND H.REAL_DATEVAL <= FinReb))
            GROUP BY
                CASE WHEN a.tp_value = TP_FT THEN
                        CASE WHEN H.DATEVAL IS NOT NULL AND H.DATEVAL <= DebReb THEN DebReb
                             ELSE H.DATEVAL END
                     WHEN a.tp_value = TP_FR THEN
                        CASE WHEN H.REAL_DATEVAL IS NULL OR H.REAL_DATEVAL = NULL_DATE
                                OR H.REAL_DATEVAL <= DebReb THEN DebReb
                             ELSE H.REAL_DATEVAL END END
            ORDER BY 1; 
                     
        currentFC.value := 0;
        
        for i in 0..FinReb-DebReb
        loop
            d1 := DebReb + i;
            
            -- taux variable
            if a.taux_var is null or a.taux_var = 0 then
                currentFC.rate := 0;
            else
                while (curRates%FOUND or curRates%FOUND is null) and nextRate.dateval <= d1
                loop
                    currentRate := nextRate;
                    loop
                        FETCH curRates INTO nextRate;
                        exit when curRates%NOTFOUND or nextRate.rate is not null;
                    end loop;
                    DEBUG('date = {1}, jour = {2}, rate = {3}', d1, currentRate.dateval, currentRate.rate);
                end loop;
                currentFC.rate := currentRate.rate;
            end if;
            
            -- spread
            while (curSpreads%FOUND or curSpreads%FOUND is null) and nextSpread.dateval <= d1
            loop
                currentSpread := nextSpread;
                loop
                    FETCH curSpreads INTO nextSpread;
                    exit when curSpreads%NOTFOUND or nextSpread.rate is not null;
                end loop;
                DEBUG('date = {1}, dateval = {2}, spread = {3}', d1, currentSpread.dateval, currentSpread.rate);
            end loop;
            currentFC.spread := currentSpread.rate;

            -- assiette CASH
            while (curCash1%FOUND or curCash1%FOUND is null) and (nextCash1.dt <= d1 or nextCash1.dt is null)
            loop
                currentCash1.num := nextCash1.num;
                currentCash1.dt := nextCash1.dt;
                DEBUG('fetch curCash1');
                FETCH curCash1 INTO nextCash1;
                DEBUG('date = {1}, dt = {2}, cash1 = {3}', d1, currentCash1.dt, currentCash1.num);
                currentFC.value := currentFC.value + nvl(currentCash1.num, 0);
            end loop;
            
            while (curCash2%FOUND or curCash2%FOUND is null) and (nextCash2.dt <= d1 or nextCash2.dt is null)
            loop
                currentCash2.num := nextCash2.num;
                currentCash2.dt := nextCash2.dt;
                DEBUG('fetch curCash2');
                FETCH curCash2 INTO nextCash2;
                DEBUG('date = {1}, dt = {2}, cash2 = {3}', d1, currentCash2.dt, currentCash2.num);
                currentFC.value := currentFC.value + nvl(currentCash2.num, 0);
            end loop;
            
            -- if facturation condition changed. increse one element
            -- (or true) Pas de regroupement à faire pour les pools: always enter in the IF THEN
            if lastFC.rate is null or (lastFC.rate != currentFC.rate or lastFC.spread != currentFC.spread or
                lastFC.value != currentFC.value) or true then
                c.extend;
                idx := c.count;
                c(idx) := currentFC;
                c(idx)."from" := d1;
                c(idx)."to" := d1;
                c(idx).days := 1;
                lastFC := currentFC;
            else
                c(idx)."to" := d1;
                c(idx).days := c(idx).days + 1; -- increase one days
            end if;
            
            DEBUG('from = {1}, to = {2}, value = {3}, rate_name = {4}, rate = {5}, spread = {6}, days = {7}, interest = {8}',
                c(idx)."from", c(idx)."to", c(idx).value, c(idx).rate_name, c(idx).rate, c(idx).spread, c(idx).days, c(idx).interest);
        end loop;
        
        if a.taux_var is not null and a.taux_var != 0 then
            close curRates;
        end if;
        close curSpreads;
        
        -- calcul interest and TotalRebates
        for i in 1..c.count
        loop
            c(i).interest := c(i).value * -1 * (c(i).rate + c(i).spread) * c(i).days / 100 / calc_basis; 
            TotalRebates := TotalRebates + c(i).interest;
             
            --round
            c(i).value := round(c(i).value, MATH_ROUND);
            c(i).interest := round(c(i).interest, MATH_ROUND);
        end loop;
        
        -- insert into global temporary table
        if p_isFilled then
            DEBUG('insert into BL_REBATES_POOL. {1} rows', c.count);
            DELETE BL_REBATES_POOL WHERE MVTIDENT = a.mvtident;
            for i in 1..c.count
            loop
                INSERT INTO BL_REBATES_POOL VALUES (
                    a.mvtident, c(i)."from", c(i)."to", c(i).value, a.reference_2, 
                    c(i).rate, c(i).spread, c(i).days, c(i).interest);
            end loop;
        end if;
        
    <<FIN>>        
        RebPoolList := c;
        INFO('CALCUL_REMUN_COLLAT_POOL.END(RebPoolList.Count = {1}, TotalRebates = {2})', RebPoolList.count, TotalRebates);
    EXCEPTION
        WHEN OTHERS THEN 
            ERROR('CALCUL_REMUN_COLLAT_POOL. code = {1}, message = {2}, backtrace={3}', SQLCODE, SQLERRM, DBMS_UTILITY.FORMAT_ERROR_BACKTRACE);
            RAISE;
    END;
        
    PROCEDURE CALCUL_REMUN_COLLAT_HORS_POOL(
        p_refcon        in  HISTOMVTS.REFCON%TYPE,
        p_dateneg_c     in  date, -- dateneg du closing
        DebReb          in  date,
        FinReb          in  date,
        RebHorsPoolList out REBATE_HORS_POOL_LIST,
        RebatePayed     out number,
        TotalRebates    out number,
        p_isFilled      in  boolean default false)
    AS
        TYPE AGREEMENT_INFO IS RECORD (
            --MEP
            quantite                HISTOMVTS.QUANTITE%TYPE,
            cours                   HISTOMVTS.COURS%TYPE,
            dateval                 date,
            commission              HISTOMVTS.COMMISSION%TYPE,
            montant                 HISTOMVTS.MONTANT%TYPE,
            mvtident                HISTOMVTS.MVTIDENT%TYPE,
            
            --TITRES
            affectation             TITRES.AFFECTATION%TYPE,
            fixing_tag1             TITRES.FIXING_TAG1%TYPE,
            reference_5             TITRES.REFERENCE%TYPE,
            reference_6             TITRES.REFERENCE%TYPE,
            devisectt_5             TITRES.DEVISECTT%TYPE,
            devisectt_6             TITRES.DEVISECTT%TYPE,
            taux_var                TITRES.TAUX_VAR%TYPE,
            taux                    TITRES.TAUX%TYPE,
            beta                    TITRES.BETA%TYPE,
            typederive              TITRES.TYPEDERIVE%TYPE,
            code_emet               TITRES.CODE_EMET%TYPE,
            underlying_type         TITRES.TYPE%TYPE,
            quotation_type          TITRES.QUOTATION_TYPE%TYPE,
            underlying_nominal      TITRES.NOMINAL%TYPE,
            underlying_nbtitres     TITRES.NBTITRES%TYPE,
            deviseac                TITRES.DEVISEAC%TYPE,
            devisectt               TITRES.DEVISECTT%TYPE,
            
            --OTHERS
            tp_value                TIERSPROPERTIES.VALUE%TYPE,
            convention              BO_PE_PERIMETER_DEF.NAME%TYPE
        );
        a                           AGREEMENT_INFO;
        c REBATE_HORS_POOL_LIST := REBATE_HORS_POOL_LIST();
        -- taux variable à utiliser dans le loop
        -- cursor of SPREAD.
        CURSOR curRates IS
            SELECT DATEVAL, RATE FROM (
                SELECT MIN_DATE DATEVAL,
                    CASE    WHEN T.AFFECTATION IN (62,63,65) OR (T.AFFECTATION IN (60) AND T.AMORT IN (1, 7)) THEN H.COMMISSION
                            WHEN T.AFFECTATION IN (60) AND T.AMORT NOT IN (1, 7) THEN T.TAUX END RATE, H.REFCON
                FROM HISTOMVTS H JOIN TITRES T ON T.SICOVAM=H.SICOVAM AND T.AFFECTATION in (62,63,65,60)
                WHERE H.REFCON = p_refcon
                UNION
                SELECT HL.DATEVAL, HL.COURS RATE, HL.REFCON
                FROM HISTOMVTS H
                JOIN HISTOMVTS HL
                    ON HL.MVTIDENT = H.MVTIDENT AND HL.TYPE IN (28)
                    --AND HL.BACKOFFICE NOT IN (13, 29, 38, 268)
                    AND ((HL.BACKOFFICE IN (
                            SELECT KSC.KERNEL_STATUS_ID FROM BO_KERNEL_STATUS_COMPONENT KSC
                                JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                                AND KSG.RECORD_TYPE = 1 AND KSG.NAME = 'All But Pending FO') AND GP_SOURCE != SRC_PNL )
                                OR (HL.BACKOFFICE NOT IN (
                                        SELECT KSC.KERNEL_STATUS_ID
                                        FROM BO_KERNEL_STATUS_COMPONENT KSC
                                            JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                                                AND KSG.NAME IN ('_STEP_Cancelled', '_MOD_Cancelled') AND KSG.RECORD_TYPE = 1) AND GP_SOURCE = SRC_PNL  ))
                WHERE H.REFCON = p_refcon
            ) ORDER BY DATEVAL, REFCON;
            
        currentRate         RATE_INFO; -- associated with curRates
        nextRate            RATE_INFO; -- associated with curRates
        d1                  date;
        idx                 integer := 0;
        rc                  integer;
        underlyingNomUnit   number;
        
        -- cours of DmHistorique, BO_LB_FEEMARK,
        -- has the same structure as RATE_INFO. Used in when calculate prix for OPEN_BASIS, FEE_MARK
        curCours            SYS_REFCURSOR; 
        currentCours        RATE_INFO;
        nextCours           RATE_INFO;
        XSicovam            TITRES.SICOVAM%TYPE; -- sicovam of exchange Titres
        curXRates           SYS_REFCURSOR;       -- cursor for the cours of exchange
        currentXRate        RATE_INFO;
        nextXRate           RATE_INFO;
        
        -- Cursor to retrieve Qty by date
        curQty              SYS_REFCURSOR;
        currentQty          DATE_NUM;
        nextQty             DATE_NUM;
        
        -- Cursor to retrieve Montant by date
        curMontant          SYS_REFCURSOR;
        currentMontant      DATE_NUM;
        nextMontant         DATE_NUM;
        
        -- Facture Condition
        lastFC              BL_REBATES_HORS_POOL%ROWTYPE;
        currentFC           BL_REBATES_HORS_POOL%ROWTYPE;
        -- TIME BASIS, CALC BASISS
        calc_basis          number;
        titres2_sicovam     TITRES.SICOVAM%TYPE;
        titres2_devisectt   TITRES.DEVISECTT%TYPE;
        taux_euribor_3mois  DMHISTORIQUE.D%TYPE;
        date_fixing         date;
        
    BEGIN
        INFO('CALCUL_REMUN_COLLAT_HORS_POOL(refcon = {1}, DebReb = {2}, FinReb = {3})',
            p_refcon, DebReb, FinReb);

        RebatePayed := 0;            
        TotalRebates := 0;
            
        SELECT H.QUANTITE, H.COURS, H.DATEVAL, H.COMMISSION, H.MONTANT, H.MVTIDENT,
            T.AFFECTATION, T.FIXING_TAG1, TT5.REFERENCE, TT6.REFERENCE, TT5.DEVISECTT, TT6.DEVISECTT, T.TAUX_VAR, T.TAUX, 
            T.BETA, T.TYPEDERIVE, T.CODE_EMET, TT1.TYPE, TT1.QUOTATION_TYPE, TT1.NOMINAL, TT1.NBTITRES,
            CASE WHEN T.DEVISEAC IS NULL OR T.DEVISEAC = 0 THEN H.DEVISEPAY ELSE T.DEVISEAC END, T.DEVISECTT, decode (GP_SOURCE, SRC_PNL, TP_FT, NVL(TP.VALUE, TP_FD)), PE.NAME --PNL : Facturation en dates Théoriques
            into a
        FROM HISTOMVTS H
            JOIN TITRES T ON T.SICOVAM = H.SICOVAM
            JOIN BO_PE_PERIMETER_DEF PE ON PE.ID = T.PERIMETERID
            LEFT JOIN TITRES TT1 ON TT1.SICOVAM = T.CODE_EMET
            LEFT JOIN TITRES TT5 ON TT5.SICOVAM = T.TAUX_VAR -- same alias name as SP 
            LEFT JOIN TITRES TT6 ON TT6.SICOVAM = T.FIXING_TAG1 -- same alias name as SP
            LEFT JOIN TIERSPROPERTIES TP ON TP.CODE = H.CONTREPARTIE AND TP.NAME = TP_BILLING
        WHERE H.REFCON = p_refcon;
            
        INFO('quantite = {1}, cours = {2}, dateval = {3}, commission = {4}, montant = {5}, mvtident = {6}, devisectt_5={7}, devisectt_6={8}',
            a.quantite, a.cours, a.dateval, a.commission, a.montant, a.mvtident, a.devisectt_5, a.devisectt_6);
        INFO('affectation = {1}, fixing_tag1 = {2}, reference_5 = {3}, reference_6 = {4}, taux_var = {5}, taux = {6}, beta = {7}, typederive = {8}, code_emet = {9}',
            a.affectation, a.fixing_tag1, a.reference_5, a.reference_6, a.taux_var, a.taux, a.beta, a.typederive, a.code_emet);
        INFO('underlying_type = {1}, quotation_type = {2}, underlying_nominal = {3}, underlying_nbtitres = {4}, deviseac = {5}, devisectt = {6}, tiersproperties = {7}, convetion = {8}',
            a.underlying_type, a.quotation_type, a.underlying_nominal, a.underlying_nbtitres, a.deviseac, a.devisectt, a.tp_value, a.convention);
            
        --For debug
        --a.tp_value := TP_FT;
        DEBUG('tp_value = {1}', a.tp_value);
        
        calc_basis := case a.typederive when 1 then 360 when 2 then 365 when 6 then 365.25 when 7 then 365 else 360 end;
        if a.typederive is null then
            WARN('TIME BASIS non gere. Par defaut : Actual/360');
        elsif a.typederive not in (1, 2, 6, 7) then
            WARN('TIME BASIS not supported');
        end if;
        open curRates;
        
        -- get taux_variable and reference
        if a.affectation in (62, 63, 65) then
            if a.fixing_tag1 is not null and a.fixing_tag1 != 0 then
                titres2_sicovam := a.fixing_tag1;
                titres2_devisectt := a.devisectt_6;
                currentFC.rate_name := a.reference_6;
            end if;                                      
        elsif a.affectation in (60) then
            if a.taux_var is not null and a.taux_var != 0 then
                titres2_sicovam := a.taux_var;
                titres2_devisectt := a.devisectt_5;
                currentFC.rate_name := a.reference_5;
            end if;               
        end if;
        
        if titres2_sicovam is not null then
            SELECT CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END
                into rc
            FROM NATIXIS_GROUP_PARAM NGP
                JOIN TITRES T ON T.SICOVAM = titres2_sicovam
                    AND INSTR(',' || NGP.VALUE || ',', ',' || T.REFERENCE || ',') > 0
            WHERE NGP.TYPE = 'BGENERATOR' AND NGP.KEY = 'TauxVariable' AND NGP.ENABLED = 1;
            DEBUG('titres2_sicovam = {1}, rc = {2}', titres2_sicovam, rc); 
            -- type Euribor 3 Mois
            if rc > 0 then
                if titres2_devisectt != str_to_devise('GBP') then
                    date_fixing := GET_FIRST_WORKING_DAY(a.dateval - 2, titres2_devisectt, -1); -- J - 2
                else
                    date_fixing := GET_FIRST_WORKING_DAY(a.dateval, titres2_devisectt, -1); -- J
                end if;
                DEBUG('date_fixing = {1}, titres2_devisectt = {2}', date_fixing, titres2_devisectt);
                begin
                    SELECT D.D
                        into taux_euribor_3mois
                    FROM DMHISTORIQUE D
                    WHERE D.SICOVAM = titres2_sicovam
                        AND D.JOUR = (
                            SELECT MAX(D.JOUR) FROM DMHISTORIQUE D
                            WHERE D.SICOVAM = titres2_sicovam AND D.JOUR <= date_fixing);
                exception
                    when no_data_found then
                        taux_euribor_3mois := 0;
                        WARN('No taux euribor 3mois found for sicovam = {1} before {2}', titres2_sicovam, date_fixing);
                end;
            else
                -- Get the last cours
                begin    
                   -- Get the last cours (cours du jour, non pas du veille)
                   -- Attention: Never use Rownum with Order By
                    nextCours.dateval := DebReb + 1;
                    nextCours.rate := null;
                    while (nextCours.rate is null)
                    loop
                        -- for performance optimization, do not use COURS IS NOT NULL in the following query
                        SELECT D.JOUR, D.D
                            into nextCours
                        FROM DMHISTORIQUE D
                        WHERE D.SICOVAM = titres2_sicovam
                            AND D.JOUR = (
                                SELECT MAX(D.JOUR) FROM DMHISTORIQUE D
                                WHERE D.SICOVAM = titres2_sicovam AND D.JOUR <= nextCours.dateval - 1);
                    end loop;
                exception
                   when no_data_found then
                       nextCours.dateval := MIN_DATE;
                       nextCours.rate := 0;
                       WARN('No cours found for sicovam = {1} before {2}', titres2_sicovam, DebReb);
                end;
                -- for performance optimization, do not use COURS IS NOT NULL in the following query
                open curCours for
                   SELECT D.JOUR, D.D FROM DMHISTORIQUE D
                   WHERE D.SICOVAM = titres2_sicovam AND D.JOUR > DebReb AND D.JOUR <= FinReb
                   ORDER BY 1;
            end if;
        end if;
           
        --cursor for calculate QTY
        open curQty for
            SELECT 
                CASE WHEN a.tp_value = TP_FT THEN
                        CASE WHEN NVL(H.DELIVERY_DATE, H.DATEVAL) <= DebReb THEN DebReb
                             ELSE NVL(H.DELIVERY_DATE, H.DATEVAL) END  
                     WHEN a.tp_value = TP_FR THEN
                        CASE WHEN H.REAL_DELIVERY_DATE IS NULL OR H.REAL_DELIVERY_DATE = NULL_DATE
                                OR H.REAL_DELIVERY_DATE <= DebReb THEN DebReb
                             ELSE H.REAL_DELIVERY_DATE END END,
                NVL(SUM(H.QUANTITE), 0)
            FROM HISTOMVTS H
                JOIN BUSINESS_EVENTS BE ON BE.ID = H.TYPE AND BE.COMPTA = 1
            WHERE H.MVTIDENT = a.mvtident
                AND ((H.BACKOFFICE IN (
                        SELECT KSC.KERNEL_STATUS_ID FROM BO_KERNEL_STATUS_COMPONENT KSC
                            JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                            AND KSG.RECORD_TYPE = 1 AND KSG.NAME = 'All But Pending FO') AND GP_SOURCE != SRC_PNL )
                            OR (H.BACKOFFICE NOT IN (
                                    SELECT KSC.KERNEL_STATUS_ID
                                    FROM BO_KERNEL_STATUS_COMPONENT KSC
                                        JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                                            AND KSG.NAME IN ('_STEP_Cancelled', '_MOD_Cancelled') AND KSG.RECORD_TYPE = 1) AND GP_SOURCE = SRC_PNL  ))
                AND ((a.tp_value = TP_FT AND NVL(H.DELIVERY_DATE, H.DATEVAL) <= FinReb) OR 
                     (a.tp_value = TP_FR AND H.REAL_DELIVERY_DATE IS NOT NULL AND H.REAL_DELIVERY_DATE != NULL_DATE
                        AND H.REAL_DELIVERY_DATE <= FinReb))
            GROUP BY
                CASE WHEN a.tp_value = TP_FT THEN
                        CASE WHEN NVL(H.DELIVERY_DATE, H.DATEVAL) <= DebReb THEN DebReb
                             ELSE NVL(H.DELIVERY_DATE, H.DATEVAL) END  
                     WHEN a.tp_value = TP_FR THEN
                        CASE WHEN H.REAL_DELIVERY_DATE IS NULL OR H.REAL_DELIVERY_DATE = NULL_DATE
                                OR H.REAL_DELIVERY_DATE <= DebReb THEN DebReb
                             ELSE H.REAL_DELIVERY_DATE END END
            ORDER BY 1;

        --cursor for calculate MONTANT
        open curMontant for           
            SELECT
                CASE WHEN a.tp_value = TP_FT THEN
                        CASE WHEN H.DATEVAL IS NOT NULL AND H.DATEVAL <= DebReb THEN DebReb
                             ELSE H.DATEVAL END
                     WHEN a.tp_value = TP_FR THEN
                        CASE WHEN H.REAL_DATEVAL IS NULL OR H.REAL_DATEVAL = NULL_DATE 
                                OR H.REAL_DATEVAL <= DebReb THEN DebReb
                             ELSE H.REAL_DATEVAL END END,
                NVL(SUM(H.MONTANT), 0)
            FROM HISTOMVTS H
                LEFT JOIN BUSINESS_EVENTS BE ON BE.ID = H.TYPE AND BE.COMPTA = 1
            WHERE H.MVTIDENT = a.mvtident
                AND (H.TYPE IN (16) OR BE.ID IS NOT NULL) 
                AND ((H.BACKOFFICE IN (
                    SELECT KSC.KERNEL_STATUS_ID FROM BO_KERNEL_STATUS_COMPONENT KSC
                        JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                        AND KSG.RECORD_TYPE = 1 AND KSG.NAME = 'All But Pending FO') AND GP_SOURCE != SRC_PNL )
                        OR (H.BACKOFFICE NOT IN (
                                SELECT KSC.KERNEL_STATUS_ID
                                FROM BO_KERNEL_STATUS_COMPONENT KSC
                                    JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                                        AND KSG.NAME IN ('_STEP_Cancelled', '_MOD_Cancelled') AND KSG.RECORD_TYPE = 1) AND GP_SOURCE = SRC_PNL  ))
                AND ((a.tp_value = TP_FT AND H.DATEVAL IS NOT NULL AND H.DATEVAL <= FinReb) OR 
                     (a.tp_value = TP_FR AND H.REAL_DATEVAL IS NOT NULL AND H.REAL_DATEVAL != NULL_DATE
                        AND H.REAL_DATEVAL <= FinReb))
            GROUP BY
                CASE WHEN a.tp_value = TP_FT THEN
                        CASE WHEN H.DATEVAL IS NOT NULL AND H.DATEVAL <= DebReb THEN DebReb
                             ELSE H.DATEVAL END
                     WHEN a.tp_value = TP_FR THEN
                        CASE WHEN H.REAL_DATEVAL IS NULL OR H.REAL_DATEVAL = NULL_DATE 
                                OR H.REAL_DATEVAL <= DebReb THEN DebReb
                             ELSE H.REAL_DATEVAL END END
            ORDER BY 1; 
                    
        -- Taux de change
        if a.affectation = 60 then
            currentFC.taux_change := 1;
            if NVL(a.deviseac, 0) != NVL(a.devisectt, 0) then
                INFO('Contrat {1}: devise de facturation {2} différente de la devise du collatéral {3}',
                    a.mvtident, a.deviseac, a.devisectt);
            end if;
        end if;
        
        -- qty, montant
        --currentFC.qty := a.quantite;
        currentFC.qty := 0;
        --currentFC.value := a.montant;
        currentFC.value := 0;
        
        -- mvtident
        currentFC.mvtident := a.mvtident;
        
        -- obligation cotée en pourcentage
        underlyingNomUnit := 1;
        if a.quotation_type = 2 and a.underlying_type != 'A' then -- equivalent avec UNDERLYING_PRICE_TYPE = 2
            if a.underlying_type in ('O', 'D') then
                underlyingNomUnit := case a.underlying_nominal when 0 then 1 else a.underlying_nominal end;                             
            elsif a.underlying_type in ('N') then
                underlyingNomUnit := case a.underlying_nbtitres when 0 then 1 else a.underlying_nbtitres end;
            else
                underlyingNomUnit := 1;
            end if;
        end if;
            
        for i in 0..(FinReb - DebReb)
        loop
            d1 := DebReb + i;
        
            -- get rate
            if titres2_sicovam is null then 
                currentFC.rate := 0;
            else
                if taux_euribor_3mois is not null then
                    currentFC.rate := taux_euribor_3mois;
                else
                    while (curCours%FOUND or curCours%FOUND is null) and nextCours.dateval <= d1
                    loop
                        currentCours := nextCours;
                        loop
                            FETCH curCours INTO nextCours;
                            exit when curCours%NOTFOUND or nextCours.rate is not null;
                        end loop;
                        DEBUG('date = {1}, jour = {2}, rate = {3}', d1, currentCours.dateval, currentCours.rate);
                    end loop;
                    currentFC.rate := currentCours.rate;
                end if;                              
            end if;
              
            -- get spread
            while (curRates%FOUND or curRates%FOUND is null) and (nextRate.dateval <= d1 or nextRate.dateval is null)
            loop
                currentRate := nextRate;
                loop
                    FETCH curRates INTO nextRate;
                    exit when curRates%NOTFOUND or nextRate.rate is not null;
                end loop;
               DEBUG('date = {1}, dateval = {2}, spread = {3}', d1, currentRate.dateval, currentRate.rate);
            end loop;
            currentFC.spread := currentRate.rate;
           
            -- get the quantite
            while (curQty%FOUND or curQty%FOUND is null) and (nextQty.dt <= d1 or nextQty.dt is null)
            loop
                currentQty.num := nextQty.num;
                currentQty.dt := nextQty.dt;
                FETCH curQty INTO nextQty;
                DEBUG('date = {1}, dt = {2}, qty = {3}', d1, currentQty.dt, currentQty.num);
                currentFC.qty := currentFC.qty + nvl(currentQty.num, 0);
            end loop;
            
            -- get Hedging
            currentFC.hedging := a.beta;
           
           -- get assiette cash
            while (curMontant%FOUND or curMontant%FOUND is null) and (nextMontant.dt <= d1 or nextMontant.dt is null)
            loop
                currentMontant.num := nextMontant.num;
                currentMontant.dt := nextMontant.dt;
                FETCH curMontant INTO nextMontant;
                DEBUG('date = {1}, dt = {2}, montant = {3}', d1, currentMontant.dt, currentMontant.num);
                currentFC.value := currentFC.value + nvl(currentMontant.num, 0);
            end loop;
        
           -- get collat price
            if (currentFC.qty<>0) then
                currentFC.prix := currentFC.value/ (currentFC.qty * underlyingNomUnit);
            else
                currentFC.prix := 0;
            end if;
            
            -- prix_devise_fact
            if a.affectation = 60 then
                currentFC.prix_devise_fact := currentFC.prix * currentFC.taux_change;
            end if; 
           
            -- if facturation condition changed. increse one element
            if lastFC.qty is null or (lastFC.qty != currentFC.qty or lastFC.value != currentFC.value or
                lastFC.rate != currentFC.rate or lastFC.spread != currentFC.spread)
                or a.convention like '%_POOL' or GP_SOURCE = SRC_PNL then -- Ne pas effectuer ce regroupement sur les positions ayant une convention _POOL ou src=PNL
                c.extend;
                idx := c.count;
                c(idx) := currentFC;
                c(idx).qty := c(idx).qty * underlyingNomUnit;
                c(idx)."from" := d1;
                c(idx)."to" := d1;
                c(idx).days := 1;
                lastFC := currentFC;
            else
                c(idx)."to" := d1;
                c(idx).days := c(idx).days + 1; -- increase one days
            end if;
            
            DEBUG('from = {1}, to = {2}, qty = {3}, prix = {4}, taux_change = {5}, prix_devise_fact = {6}',
                c(idx)."from", c(idx)."to", c(idx).qty, c(idx).prix, c(idx).taux_change, c(idx).prix_devise_fact);
            DEBUG('hedging = {1}, value = {2}, rate_name = {3}, rate = {4}, spread = {5}, days = {6}, interest= {7}',
                c(idx).hedging, c(idx).value, c(idx).rate_name, c(idx).rate, c(idx).spread, c(idx).days, c(idx).interest);
        end loop;
        
        close curRates;
        if titres2_sicovam is not null and taux_euribor_3mois is null then
            close curCours;
        end if;
        close curQty;
        close curMontant;
        
        -- calcul interest and TotalRebates
        for i in 1..c.count
        loop
            c(i).interest := c(i).value * -1 * (c(i).rate + c(i).spread) * c(i).days / 100 / calc_basis; 
            TotalRebates := NVL(TotalRebates, 0) + c(i).interest; 
            
            --round
            c(i).prix := round(c(i).prix, MATH_ROUND);
            c(i).taux_change := round(c(i).taux_change, MATH_ROUND);
            c(i).prix_devise_fact := round(c(i).prix_devise_fact, MATH_ROUND);
            c(i).value := round(c(i).value, MATH_ROUND);
            c(i).interest := round(c(i).interest, MATH_ROUND);
        end loop;
        
        -- RebatePayed
        if GP_PERIODICITE = PER_M then
            RebatePayed := 0;
        elsif GP_PERIODICITE = PER_I then
            SELECT NVL(SUM(H.MONTANT), 0)
                into RebatePayed
            FROM HISTOMVTS H
                JOIN TITRES T ON T.SICOVAM = H.SICOVAM
            WHERE ((T.TYPE = 'P' AND H.TYPE IN (101, 701)) OR (T.TYPE = 'L' AND H.TYPE IN (7, 700)))
                AND H.BACKOFFICE NOT IN (
                    SELECT KSC.KERNEL_STATUS_ID
                    FROM BO_KERNEL_STATUS_COMPONENT KSC
                        JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                            AND KSG.NAME IN ('_STEP_Cancelled', '_MOD_Cancelled') AND KSG.RECORD_TYPE = 1)
                AND H.MVTIDENT = a.mvtident
                AND H.DATENEG < p_dateneg_c;
                --AND H.DATEVAL < p_datesitu;
        end if;
        
        -- insert into global temporary table
        if p_isFilled then
            DELETE BL_REBATES_HORS_POOL WHERE MVTIDENT = a.mvtident;
            for i in 1..c.count
            loop
                INSERT INTO BL_REBATES_HORS_POOL VALUES c(i);
            end loop;
        end if;
        
    <<FIN>>        
        RebHorsPoolList := c;
        INFO('CALCUL_REMUN_COLLAT_HORS_POOL.END(RebHorsPoolList.Count = {1}, RebatePayed = {2}, TotalRebates = {3})',
            RebHorsPoolList.count, RebatePayed, TotalRebates);
    EXCEPTION
        WHEN OTHERS THEN 
            ERROR('CALCUL_REMUN_COLLAT_HORS_POOL. code = {1}, message = {2}, backtrace={3}', SQLCODE, SQLERRM, DBMS_UTILITY.FORMAT_ERROR_BACKTRACE);
            RAISE;
    END;
    
    PROCEDURE CALCUL_REMUN_COLLAT(
        p_refcon        in  HISTOMVTS.MVTIDENT%TYPE,
        p_dateneg_c     in  date, -- dateneg du closing
        DebReb          in  date,
        FinReb          in  date,
        RebPoolList     out REBATE_POOL_LIST,
        RebHorsPoolList out REBATE_HORS_POOL_LIST,
        RebatePayed     out number,
        TotalRebates    out number,
        p_isFilled      in  boolean default false)
    AS
        -- ERROR exact fetch returns more than requested number of rows
        -- when trying to name variable as the same as column. So i add "_" to the variable's name
        is_pool         int;
    BEGIN
        INFO('CALCUL_REMUN_COLLAT(refcon = {1}, DebReb = {2}, FinReb = {3})',
                p_refcon, DebReb, FinReb);

        RebatePayed := 0;                
        TotalRebates := 0;
        RebPoolList := REBATE_POOL_LIST();
        RebHorsPoolList := REBATE_HORS_POOL_LIST();
                
        SELECT CASE WHEN T.TYPE = 'C' AND T.MODELE = 'Collateral' THEN 1 ELSE 0 END
            into is_pool
        FROM HISTOMVTS H
            JOIN TITRES T ON T.SICOVAM = H.SICOVAM
        WHERE H.REFCON = p_refcon;
        
        DEBUG('is_pool = {1}', is_pool);
        
        if is_pool = 1 then
            CALCUL_REMUN_COLLAT_POOL(p_refcon, DebReb, FinReb, RebPoolList, TotalRebates, p_isFilled);
        else
            CALCUL_REMUN_COLLAT_HORS_POOL(p_refcon, p_dateneg_c, DebReb, FinReb,
                RebHorsPoolList, RebatePayed, TotalRebates, p_isFilled);
        end if;
    EXCEPTION
        WHEN OTHERS THEN 
            ERROR('CALCUL_REMUN_COLLAT. code = {1}, message = {2}, backtrace={3}', SQLCODE, SQLERRM, DBMS_UTILITY.FORMAT_ERROR_BACKTRACE);
            RAISE;
    END;

    PROCEDURE BILLING_GENERATOR(
        p_source        in  varchar2,
        p_date          in  date,
        p_datefin       in  date default null,
        p_periodicite   in  int default 2, -- 2 = mensuel, 1 = Infine
        p_isFilled      in  boolean default false,
        p_entite        in  HISTOMVTS.ENTITE%TYPE default null,
        p_contrepartie  in  varchar2 default null,
        p_devise        in  varchar2 default null,
        p_perimeterid   in  TITRES.PERIMETERID%TYPE default null,
        p_mvtident      in  HISTOMVTS.MVTIDENT%TYPE default null,
        p_riskuser      in  RISKUSERS.IDENT%TYPE default null,
        p_refcon        in  HISTOMVTS.REFCON%TYPE default null,
        p_mode          in  varchar2 default null,
        FeePayed        out number,
        Balance_MinFee  out number,
        TotalFees       out number,
        RebatePayed     out number,
        TotalRebates    out number)
    AS
        
        TYPE AGREEMENT_INFO IS RECORD (
            --MEP
            refcon                  HISTOMVTS.REFCON%TYPE,
            mvtident                HISTOMVTS.MVTIDENT%TYPE,
            sicovam                 HISTOMVTS.SICOVAM%TYPE,
            contrepartie            HISTOMVTS.CONTREPARTIE%TYPE,
            mirror_reference        HISTOMVTS.MIRROR_REFERENCE%TYPE,
            dateval                 date,
            real_dateval            date,
            delivery_date           date,
            real_delivery_date      date, 
            delivery_type           HISTOMVTS.DELIVERY_TYPE%TYPE,
            entite                  HISTOMVTS.ENTITE%TYPE,
            commission_date         date,
            
            --CLOSING
            refcon_c                HISTOMVTS.REFCON%TYPE,
            dateneg_c               date,
            dateval_c               date,
            real_dateval_c          date,
            delivery_date_c         date,
            real_delivery_date_c    date,
            delivery_type_c         HISTOMVTS.DELIVERY_TYPE%TYPE,
            
            --TITRES
            affectation             TITRES.AFFECTATION%TYPE,
            perimeterid             TITRES.PERIMETERID%TYPE,
            devisectt               TITRES.DEVISECTT%TYPE,
            deviseac                TITRES.DEVISEAC%TYPE,
            taux_var                TITRES.TAUX_VAR%TYPE,
            is_pool                 int,
            
            --OTHER
            tp_value                TIERSPROPERTIES.VALUE%TYPE,
            date_situ               date
        );
        a                           AGREEMENT_INFO;
        
        TYPE POOL_INFO IS RECORD (
            entite                  HISTOMVTS.ENTITE%TYPE,
            contrepartie            HISTOMVTS.CONTREPARTIE%TYPE,
            perimeterid             TITRES.PERIMETERID%TYPE,
            devisectt               TITRES.DEVISECTT%TYPE,
            taux_var                TITRES.TAUX_VAR%TYPE
        );
        TYPE POOL_LIST IS TABLE OF POOL_INFO;
        pl                  POOL_LIST := POOL_LIST();
        
        rc                  number;
        montant             number;
        mvtident_mirror     HISTOMVTS.MVTIDENT%TYPE;
        mvtident_pool       HISTOMVTS.MVTIDENT%TYPE;
        refcon_pool         HISTOMVTS.REFCON%TYPE;
        DebPos              date; -- Debut Position
        FinPos              date; -- Fin Position
        DebMen              date; -- Debut mois
        FinMen              date; -- Fin mois
        DebCom              date; -- Debut Commission
        FinCom              date; -- Fin Commission
        ComList             FEE_LIST;
        DebReb              date; -- Debut Remun Collat
        FinReb              date; -- Fin Remun Collat
        RebPoolList         REBATE_POOL_LIST;
        RebHorsPoolList     REBATE_HORS_POOL_LIST;
        strSQL              varchar2(10000);
        whereSQL            varchar2(10000);
        curAgreements1      SYS_REFCURSOR; -- P/E hors POOL
        curAgreements2      SYS_REFCURSOR; -- POOL sur P/E
        curDeals            SYS_REFCURSOR; -- Deals to be removed when passage FM --> FM
        agrCount1           number;
        agrCount2           number;
        pnl_periodicite     number;

        -- Constants    
        DT_DVP              constant HISTOMVTS.DELIVERY_TYPE%TYPE    := 1; -- DVP
        DT_FOP              constant HISTOMVTS.DELIVERY_TYPE%TYPE    := 2; -- FOP
        --TODAY               constant date := trunc(sysdate);
        
    BEGIN
        begin
            SELECT IDENT
                into OPERATEUR_FM
            FROM RISKUSERS WHERE NAME = 'APP_PROD_BLG_FM';
        exception
            when no_data_found then
                raise_application_error(ERR_USER_NOT_FOUND, 'user OPERATEUR_FM not found');
        end;
        OPERATEUR_PI := OPERATEUR_FM; -- utiliser le FM pour le traitement Pirum
        
        --assign global parameters
        GP_SOURCE := p_source;
        GP_PERIODICITE := p_periodicite;
        GP_RISKUSER := case GP_SOURCE
                        when SRC_FM then OPERATEUR_FM
                        when SRC_PI then OPERATEUR_PI
                        when SRC_Q  then OPERATEUR_FM
                        when SRC_T  then p_riskuser
                        when SRC_I  then p_riskuser
                        when SRC_PNL  then OPERATEUR_FM end;
        GP_MODE := case when p_mode is null then MODE_E else p_mode end;

        -- Important: Do not change the format of this info, it's used in extraction of report
        INFO('BILLING_GENERATOR.BEGIN(source={1}, date={2}, periodicite={3}, entite={4}, contrepartie={5}, devise={6}, perimeterid={7}, mvtident={8}, refcon={9}, riskuser={10}, p_mode={11}, dateFin={12})',
            GP_SOURCE, p_date, GP_PERIODICITE, p_entite, p_contrepartie, p_devise, p_perimeterid, p_mvtident, p_refcon, GP_RISKUSER, GP_MODE, p_dateFin);
            
        DEBUG('OPERATEUR_FM = {1}, OPERATEUR_PI = {2}', OPERATEUR_FM, OPERATEUR_PI);

        -- initializing output parameters
        if GET_OUTPUT_VALUE('BeginLogId') is null then
            SELECT BL_LOGS_SEQ.CURRVAL INTO output_params('BeginLogId') FROM DUAL;
            output_params('EndLogId') := output_params('BeginLogId');
        end if;
        if GET_OUTPUT_VALUE('BeginGXMLId') is null then
            output_params('BeginGXMLId') := 1;
            output_params('EndGXMLId') := 0; -- between begin and end return no rows
        end if;
        output_params('FeePayed') := 0;
        output_params('Balance_MinFee') := 0;
        output_params('TotalFees') := 0;
        output_params('RebatePayed') := 0;
        output_params('TotalRebates') := 0;
        output_params('Montant_F') := 0; -- Montant Fee
        output_params('Montant_R') := 0; -- Montant Rebate
        
        if GP_SOURCE not in (SRC_FM, SRC_PI, SRC_T, SRC_Q, SRC_I, SRC_PNL) then
            raise_application_error(ERR_SOURCE, FORMAT('source {1} is incorrect', GP_SOURCE));
        end if;
        if GP_SOURCE in (SRC_FM, SRC_PI, SRC_T, SRC_Q, SRC_I) then
            if GP_PERIODICITE not in (PER_M, PER_I) then
                raise_application_error(ERR_PERIODICITE, FORMAT('periodicity {1} is incorrect', GP_PERIODICITE));
            end if;
            if GP_SOURCE in (SRC_T, SRC_I) and GP_RISKUSER is null then
                raise_application_error(ERR_RISKUSER, FORMAT('riskuser must be specified for source {1}', GP_SOURCE));
            end if;
            if GP_MODE not in (MODE_E, MODE_S) then
                raise_application_error(ERR_MODE, FORMAT('mode {1} is incorrect', GP_MODE));
            end if;
        else -- source PNL
            if (GP_PERIODICITE is not NULL) AND (GP_PERIODICITE not in (PER_M, PER_I)) THEN
                raise_application_error(ERR_PERIODICITE, FORMAT('Seules les periodicites Mensuelle et In Fine sont gerees'));
            end if;
            if GP_PERIODICITE is null then
                SELECT T.J1REFCON2 
                into pnl_periodicite
                FROM HISTOMVTS H, TITRES T WHERE H.SICOVAM = T.SICOVAM AND H.MVTIDENT = p_mvtident and rownum=1;
                INFO('pnl_periodicite  = {1}', pnl_periodicite );
                if pnl_periodicite not in (PER_M, PER_I) then
                    raise_application_error(ERR_PERIODICITE, FORMAT('Seules les periodicites Mensuelle et In Fine sont gerees'));
                else
                    GP_PERIODICITE:=pnl_periodicite;
                end if;
            end if;    
            if p_mvtident is NULL then
                raise_application_error(ERR_MVTIDENT, FORMAT('Mvtident obligatoire'));
            end if;
            if p_entite is not NULL then
                raise_application_error(ERR_ENTITE, FORMAT('Entite interdite'));
            end if;
            if p_contrepartie is not NULL then
                raise_application_error(ERR_CONTREPARTIE, FORMAT('Contrepartie interdite'));
            end if;
            if p_devise is not NULL then
                raise_application_error(ERR_DEVISE, FORMAT('Devise interdite'));
            end if;
            if p_perimeterid is not NULL then
                raise_application_error(ERR_CONVENTION, FORMAT('Convention interdite'));
            end if;
        end if;
        

        if not GP_SOURCE in (SRC_PNL) then -- laisse passer les appels PNL
            LOCK_VERIFY;
        end if;
        
        -- lock when batch mode
        if GP_SOURCE in (SRC_FM, SRC_PI, SRC_Q) and GP_MODE != MODE_S then
            LOCK_BATCH;
        end if;
            
        -- correct the input parameters
        if GP_SOURCE in (SRC_PNL) then
            DebMen := nvl(p_date, DATE_DEBUT_PNL); -- DebMen  pour PNL
            FinMen := nvl(p_datefin, DATE_FIN_PNL);--     FinMen     pour PNL
            FinMen := to_date(to_char (FinMen, 'YYYYMMDD'),  'YYYYMMDD');
        else
            DebMen := nvl(p_date, sysdate);
            DebMen := trunc(DebMen, 'MM'); -- the first day of month
            FinMen := add_months(DebMen, 1) - 1; -- the last day of month
        end if;
        
        INFO('DebMen = {1}, FinMen = {2}', DebMen, FinMen);

        if p_mvtident is not null then
            SELECT COUNT(*)
                into rc
            FROM HISTOMVTS H WHERE H.MVTIDENT = p_mvtident AND H.TYPE IN (1, 500, 16);
            if rc = 0 then
                raise_application_error(ERR_MVTIDENT, FORMAT('mvtident {1} is incorrect', p_mvtident));
            end if; 
        end if;
        
        -- mise en cache des statut valid
        /*
        SELECT KSC.KERNEL_STATUS_ID
            bulk collect into valid_status_list
        FROM BO_KERNEL_STATUS_COMPONENT KSC
            JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                AND KSG.RECORD_TYPE = 1 AND KSG.NAME = 'All But Pending FO';*/
        
        --goto RemoveDeal;
        --goto UpdateCMA;
        
        -- delete old data of CMA
        if GP_SOURCE in (SRC_T, SRC_PI) then
            -- hors pool
            strSQL := FORMAT('
    SELECT OP_ID FROM CMA_RPT_OPERATIONS OP
        JOIN HISTOMVTS H ON H.MVTIDENT = OP.MVTIDENT
            AND H.TYPE IN (1, 500) 
            AND (H.MIRROR_REFERENCE IS NULL OR H.MIRROR_REFERENCE IN (0, -1)) -- si deal mirroirs, prendre que les deals peres
        JOIN TITRES T ON T.SICOVAM = H.SICOVAM
            AND T.TYPE IN (''P'', ''L'')            -- Instrument de type P/E or REPO
            AND T.J1REFCON2 = {2}                   -- 2 = mensuel, 1 = Infine
            AND T.AFFECTATION NOT IN (2)            -- SEC COLLAT
    WHERE ((T.J1REFCON2 = {3} AND OP.MOIS_FACT = NUM_TO_DATE({1}))
            OR (T.J1REFCON2 = {4} AND TRUNC(OP.MOIS_FACT, ''MM'') = NUM_TO_DATE({1})))', date_to_num(DebMen), GP_PERIODICITE, PER_M, PER_I);
            strSQL := BUILD_WHERE_COND(strSQL, p_entite, p_contrepartie, p_mvtident, p_devise, p_perimeterid, p_refcon);
            -- mirror
            if p_mvtident is not null then
                strSQL := replace(strSQL, FORMAT('H.MVTIDENT = {1}', p_mvtident),
                    FORMAT('(H.MVTIDENT = {1} OR H.MVTIDENT IN (
            SELECT HF.MVTIDENT FROM HISTOMVTS HF
            JOIN HISTOMVTS HP ON HP.MVTIDENT = {1} AND HP.TYPE IN (1, 500)
                AND HP.MIRROR_REFERENCE = -1 AND HF.MIRROR_REFERENCE = HP.REFCON))', p_mvtident));
            end if;
            INFO(strSQL);
            EXECUTE IMMEDIATE FORMAT('
DELETE CMA_RPT_EXPLANATIONS WHERE OP_ID IN ({1})', strSQL);
            DEBUG('delete CMA_RPT_EXPLANATIONS rowcount = {1}', SQL%ROWCOUNT);
            EXECUTE IMMEDIATE FORMAT('
DELETE CMA_RPT_OPERATIONS WHERE OP_ID IN ({1})', strSQL);
            DEBUG('delete CMA_RPT_OPERATIONS rowcount = {1}', SQL%ROWCOUNT);
            
            -- here we remove CMA of deleted deals. Where clause is based on CMA_RPT_OPERATION instead of Histomvts
            strSQL := FORMAT('
    SELECT OP_ID FROM CMA_RPT_OPERATIONS OP
        JOIN TITRES T ON T.SICOVAM = OP.OP_SICOVAM
    WHERE ((OP.FACT_FREQ = ''Mensuel'' AND {2} = {3} AND OP.MOIS_FACT = NUM_TO_DATE({1}))
        OR (OP.FACT_FREQ = ''In Fine'' AND {2} = {4} AND TRUNC(OP.MOIS_FACT, ''MM'') = NUM_TO_DATE({1})))
    AND (OP.TA_REFCON IS NULL OR NOT EXISTS (SELECT * FROM HISTOMVTS WHERE REFCON = OP.TA_REFCON))', date_to_num(DebMen), GP_PERIODICITE, PER_M, PER_I);
            strSQL := BUILD_WHERE_COND(strSQL, p_entite, p_contrepartie, p_mvtident, null, p_perimeterid, p_refcon, true);
            if p_devise is not null then -- problem of generate in EUR, remun collat is generated in USD --> Generate in USD will remove operations generated in EUR
                strSQL := strSQL || FORMAT('
    AND ((T.TYPE IN (''P'', ''L'') AND T.DEVISEAC = STR_TO_DEVISE(''{1}''))
        OR (T.TYPE NOT IN (''P'', ''L'') AND T.DEVISECTT = STR_TO_DEVISE(''{1}'')))', p_devise);
            end if;            
            INFO(strSQL);
            EXECUTE IMMEDIATE FORMAT('
DELETE CMA_RPT_EXPLANATIONS WHERE OP_ID IN ({1})', strSQL);
            DEBUG('delete CMA_RPT_EXPLANATIONS rowcount = {1}', SQL%ROWCOUNT);
            EXECUTE IMMEDIATE FORMAT('
DELETE CMA_RPT_OPERATIONS WHERE OP_ID IN ({1})', strSQL);
            DEBUG('delete CMA_RPT_OPERATIONS rowcount = {1}', SQL%ROWCOUNT);
        
            -- pool
            if GP_PERIODICITE = PER_M then
                whereSQL := BUILD_WHERE_COND('
                        SELECT H.ENTITE, H.CONTREPARTIE, T.PERIMETERID, T.DEVISECTT, T.TAUX_VAR
                        FROM HISTOMVTS H
                            JOIN TITRES T ON T.SICOVAM = H.SICOVAM
                                AND T.TYPE = ''C'' AND T.MODELE = ''Collateral'' AND T.AFFECTATION IN (11, 60)
                        WHERE H.TYPE IN (16)', p_entite, p_contrepartie, p_mvtident, p_devise, p_perimeterid, p_refcon);
                whereSQL := FORMAT('
                SELECT DISTINCT H.MVTIDENT FROM HISTOMVTS H
                    JOIN TITRES T ON T.SICOVAM = H.SICOVAM
                        AND T.TYPE = ''C'' AND T.MODELE = ''Collateral'' AND T.AFFECTATION IN (11, 60)
                    JOIN ({1}) POOL ON POOL.ENTITE = H.ENTITE AND POOL.CONTREPARTIE = H.CONTREPARTIE
                        AND POOL.PERIMETERID = T.PERIMETERID AND POOL.DEVISECTT = T.DEVISECTT AND POOL.TAUX_VAR = T.TAUX_VAR', whereSQL);

                strSQL := FORMAT('
        SELECT OP_ID FROM CMA_RPT_OPERATIONS OP
        WHERE OP.MOIS_FACT = NUM_TO_DATE({1})', date_to_num(DebMen));
                --AND OP.MVTIDENT IN ({2})', date_to_num(DebMen), whereSQL); -- old pool has mvtident = 0 in cma_rpt_operations
                if p_entite is not null then
                    strSQL := FORMAT('{1}
        AND OP.ENTITY_ID = {2}', strSQL, p_entite);
                end if;
                if p_contrepartie is not null then
                    strSQL := FORMAT('{1}
        AND OP.CTPY_ID IN ({2})', strSQL, p_contrepartie);
                end if;
                if p_mvtident is not null then
                    strSQL := FORMAT('{1}
        AND OP.MVTIDENT = {2}', strSQL, p_mvtident);
                end if;
                if p_devise is not null then
                    strSQL := FORMAT('{1}
        AND OP.FACT_CUR = ''{2}''', strSQL, p_devise);
                end if;
                if p_perimeterid is not null then
                    strSQL := FORMAT('{1}
        AND OP.PERIMETER_ID = {2}', strSQL, p_perimeterid);
                end if;
                
                /*
                INFO(strSQL);
                EXECUTE IMMEDIATE FORMAT('
    DELETE CMA_RPT_EXPLANATIONS WHERE OP_ID IN ({1})', strSQL);
                DEBUG('delete CMA_RPT_EXPLANATIONS rowcount = {1}', SQL%ROWCOUNT);
                EXECUTE IMMEDIATE FORMAT('
    DELETE CMA_RPT_OPERATIONS WHERE OP_ID IN ({1})', strSQL);
                DEBUG('delete CMA_RPT_OPERATIONS rowcount = {1}', SQL%ROWCOUNT);
                */
            end if;
            
            COMMIT;
        
        end if; -- end of delete old CMA
        
        DELETE BL_CONTRACTS;
        
        --SAUVEGARDE DES TICKETS MENSUELS
        if GP_SOURCE in (SRC_FM) then
            strSQL := FORMAT('
INSERT INTO BL_OPERATIONS(OP_ID, TA_REFCON)
SELECT 0, H.REFCON FROM HISTOMVTS H
    JOIN TITRES T ON T.SICOVAM = H.SICOVAM
        AND ((T.TYPE IN (''P'', ''L'')          -- Instrument de type P/E or REPO
                AND T.AFFECTATION NOT IN (2))   -- SEC COLLAT
            OR  (T.TYPE = ''C'' AND T.MODELE = ''Collateral'' AND T.AFFECTATION IN (11, 60)))     -- POOL
        AND (T.J1REFCON2 = {2} OR T.J1REFCON2 IS NULL) -- Mensuel or null
    JOIN NATIXIS_FOLIO_SECTION_ENTITE FSE ON FSE.IDENT = H.OPCVM AND FSE.SECTION != ''799'' -- not in section simulation
WHERE H.TYPE IN (7, 101) -- Commission, RemunCollat
    AND (H.MIRROR_REFERENCE IS NULL OR H.MIRROR_REFERENCE IN (0, -1)) -- si deal mirroirs, prendre que les deals peres
    AND H.DATENEG = NUM_TO_DATE({1})
    AND H.BACKOFFICE NOT IN (
        SELECT KSC.KERNEL_STATUS_ID
        FROM BO_KERNEL_STATUS_COMPONENT KSC
            JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                AND KSG.NAME IN (''_STEP_Cancelled'', ''_MOD_Cancelled'') AND KSG.RECORD_TYPE = 1)
    AND (''{4}'' = ''{5}'' -- simulation mode
        OR (''{4}'' != ''{5}'' -- not simulation mode
            AND H.ECN = ''{3}'' 
            AND ((H.MIRROR_REFERENCE = -1 AND IS_CPTY_INTERNAL(H.CONTREPARTIE, H.ENTITE) = 1)
                OR (NVL(H.MIRROR_REFERENCE, 0) = 0 AND H.BACKOFFICE = 1071))))',
                date_to_num(FinMen), PER_M, GXML_ECN, GP_MODE, MODE_S);
        
            strSQL := BUILD_WHERE_COND(strSQL, p_entite, p_contrepartie, p_mvtident, p_devise, p_perimeterid); 

            DELETE BL_OPERATIONS;
            INFO(strSQL);
            EXECUTE IMMEDIATE strSQL;
            INFO('Tickets mensuels sauvegardes = {1}', SQL%ROWCOUNT);
        end if;

        -- Selection des Contrats
        -- Union of 2 queries: Contrat P/E hors POOL and POOL sur P/E
        strSQL := FORMAT('
SELECT H.REFCON, H.MVTIDENT FROM HISTOMVTS H
    JOIN TITRES T ON T.SICOVAM = H.SICOVAM
        AND T.TYPE IN (''P'', ''L'')            -- Instrument de type P/E or REPO
        AND T.J1REFCON2 = {7}                   -- 2 = mensuel, 1 = Infine
        AND T.AFFECTATION NOT IN (2)            -- SEC COLLAT
    JOIN NATIXIS_FOLIO_SECTION_ENTITE FSE ON FSE.IDENT = H.OPCVM AND FSE.SECTION != ''799'' -- not in section simulation
    LEFT JOIN TIERSPROPERTIES TP ON TP.CODE = H.CONTREPARTIE AND TP.NAME = ''{5}''
WHERE H.TYPE IN (1, 500) -- MEP
    AND ((H.BACKOFFICE IN (
        SELECT KSC.KERNEL_STATUS_ID FROM BO_KERNEL_STATUS_COMPONENT KSC
            JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
            AND KSG.RECORD_TYPE = 1 AND KSG.NAME = ''All But Pending FO'') AND ''{9}'' != ''{13}'' )
            OR (H.BACKOFFICE NOT IN (
                    SELECT KSC.KERNEL_STATUS_ID
                    FROM BO_KERNEL_STATUS_COMPONENT KSC
                        JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                            AND KSG.NAME IN (''_STEP_Cancelled'', ''_MOD_Cancelled'') AND KSG.RECORD_TYPE = 1) AND ''{9}'' = ''{13}'' ))
    AND (H.MIRROR_REFERENCE IS NULL OR H.MIRROR_REFERENCE IN (0, -1)) -- si deal mirroirs, prendre que les deals peres
    AND ((T.J1REFCON2 = 2 -- Contrat Mensuel 
        AND H.DATENEG <= NUM_TO_DATE({2})                -- date negociation future
        AND NOT EXISTS 
            (SELECT * FROM HISTOMVTS HC
            WHERE HC.MVTIDENT = H.MVTIDENT
                AND HC.BACKOFFICE IN (
                    SELECT KSC.KERNEL_STATUS_ID FROM BO_KERNEL_STATUS_COMPONENT KSC
                        JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                        AND KSG.RECORD_TYPE = 1 AND KSG.NAME = ''All But Pending FO'')
                AND HC.TYPE IN (102, 501) AND
                (((NVL(TP.VALUE, ''{6}'') = ''{3}'' OR ''{9}'' = ''{13}'') AND HC.DATEVAL < NUM_TO_DATE({1}) AND NVL(HC.DELIVERY_DATE, HC.DATEVAL) < NUM_TO_DATE({1})) OR
                (''{9}'' != ''{13}'' AND NVL(TP.VALUE, ''{6}'') = ''{4}''
                    AND ((T.TYPE IN (''L'') AND DECODE(HC.REAL_DATEVAL, NUM_TO_DATE(0), NULL, HC.REAL_DATEVAL) < NUM_TO_DATE({1}))
                        OR (T.AFFECTATION IN (59, 61, 64) AND DECODE(HC.REAL_DELIVERY_DATE, NUM_TO_DATE(0), NULL, HC.REAL_DELIVERY_DATE) < NUM_TO_DATE({1}))
                        OR (T.AFFECTATION IN (60) AND DECODE(HC.REAL_DATEVAL, NUM_TO_DATE(0), NULL, HC.REAL_DATEVAL) < NUM_TO_DATE({1})
                            AND DECODE(HC.REAL_DELIVERY_DATE, NUM_TO_DATE(0), NULL, HC.REAL_DELIVERY_DATE) < NUM_TO_DATE({1}))))))
    ) OR (T.J1REFCON2 = 1 -- Contrat Infine
        AND ( ''{9}'' = ''{13}'' OR EXISTS
            (SELECT * FROM HISTOMVTS HC
            WHERE HC.MVTIDENT = H.MVTIDENT AND HC.TYPE IN (102, 501)
                AND HC.BACKOFFICE NOT IN (
                    SELECT KSC.KERNEL_STATUS_ID
                    FROM BO_KERNEL_STATUS_COMPONENT KSC
                        JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                            AND KSG.NAME IN (''_STEP_Cancelled'', ''_MOD_Cancelled'') AND KSG.RECORD_TYPE = 1)
                AND ((''{9}'' = ''{10}'' -- Quotidien
                        AND HC.DATEVAL >= NUM_TO_DATE({8}))
                    OR (''{9}'' = ''{12}'' -- Infine
                        AND HC.DATEVAL = NUM_TO_DATE({8}))
                    OR (''{9}'' = ''{11}'' -- Transactionnel
                        AND HC.DATEVAL BETWEEN NUM_TO_DATE({1}) AND NUM_TO_DATE({2})))))))',
            date_to_num(DebMen), date_to_num(FinMen), TP_FT, TP_FR, TP_BILLING, TP_FD, GP_PERIODICITE, date_to_num(p_date), GP_SOURCE, SRC_Q, SRC_T, SRC_I, SRC_PNL);

        strSQL := BUILD_WHERE_COND(strSQL, p_entite, p_contrepartie, p_mvtident, p_devise, p_perimeterid, p_refcon);
        strSQL := 'INSERT INTO BL_CONTRACTS(REFCON, MVTIDENT) ' || strSQL; 
        INFO(strSQL);
        EXECUTE IMMEDIATE strSQL;
        strSQL := 'SELECT REFCON, MVTIDENT FROM BL_CONTRACTS WHERE ENTITE IS NULL'; 
        agrCount1 := GET_ROW_COUNT(strSQL);
        OPEN curAgreements1 FOR strSQL; -- Hors Pool
        INFO('agreements hors pool count = {1}', agrCount1);
        
        --remark: récupère dateneg plus recente
        strSQL := FORMAT('
    SELECT H.ENTITE, H.CONTREPARTIE, T.PERIMETERID, T.DEVISECTT, T.TAUX_VAR, H.MVTIDENT, H.REFCON,
        RANK() OVER (PARTITION BY H.ENTITE, H.CONTREPARTIE, T.PERIMETERID, T.DEVISECTT, T.TAUX_VAR, H.MVTIDENT
            ORDER BY H.DATENEG DESC, H.REFCON DESC) R
    FROM HISTOMVTS H
        JOIN TITRES T ON T.SICOVAM = H.SICOVAM
            AND T.TYPE = ''C'' AND T.MODELE = ''Collateral'' AND T.AFFECTATION IN (11, 60)     -- POOL
        JOIN NATIXIS_FOLIO_SECTION_ENTITE FSE ON FSE.IDENT = H.OPCVM AND FSE.SECTION != ''799'' -- not in section simulation
    WHERE H.TYPE IN (16) -- ADM (Appel de Marge)
        AND H.DATEVAL <= NUM_TO_DATE({1})
        AND ((H.BACKOFFICE IN (
            SELECT KSC.KERNEL_STATUS_ID FROM BO_KERNEL_STATUS_COMPONENT KSC
                JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                AND KSG.RECORD_TYPE = 1 AND KSG.NAME = ''All But Pending FO'') AND ''{4}'' != ''{5}'') OR
                ( H.BACKOFFICE NOT IN (
                    SELECT KSC.KERNEL_STATUS_ID
                    FROM BO_KERNEL_STATUS_COMPONENT KSC
                        JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                            AND KSG.NAME IN (''_STEP_Cancelled'', ''_MOD_Cancelled'') AND KSG.RECORD_TYPE = 1)AND ''{4}'' = ''{5}''))
        AND (H.MIRROR_REFERENCE IS NULL OR H.MIRROR_REFERENCE IN (0, -1)) -- si deal mirroirs, prendre que les deals peres
        AND {2} = {3} -- prend que le Contrat Mensuel
        ', date_to_num(FinMen), GP_PERIODICITE, PER_M, GP_SOURCE, SRC_PNL);
        strSQL := BUILD_WHERE_COND(strSQL, p_entite, p_contrepartie, p_mvtident, p_devise, p_perimeterid, p_refcon);
        strSQL := FORMAT('
INSERT INTO BL_CONTRACTS(ENTITE, CONTREPARTIE, PERIMETERID, DEVISECTT, TAUX_VAR, MVTIDENT, REFCON)
    SELECT ENTITE, CONTREPARTIE, PERIMETERID, DEVISECTT, TAUX_VAR, MVTIDENT, REFCON
    FROM ({1})
    WHERE R = 1', strSQL);
        INFO(strSQL);
        EXECUTE IMMEDIATE strSQL;
        strSQL := 'SELECT REFCON, MVTIDENT FROM BL_CONTRACTS WHERE ENTITE IS NOT NULL'; 
        agrCount2 := GET_ROW_COUNT(strSQL);
        INFO('agreements pool count = {1}', agrCount2);
        
        --goto FIN_TOUS;
        
        OPEN curAgreements2 FOR strSQL; -- Pool
        
        LOOP
        begin -- catch exception and pass to the next loop
            if curAgreements1%FOUND is null or curAgreements1%FOUND then
                FETCH curAgreements1 INTO a.refcon, a.mvtident;
            end if;
            if curAgreements1%NOTFOUND then -- Hors Pool fini, passe à POOL
                DEBUG('curAgreements1 end, fetch curAgreements2');
                FETCH curAgreements2 INTO a.refcon, a.mvtident;
                EXIT WHEN curAgreements2%NOTFOUND;
            end if;
            
            --For debug, will be commented
            --EXIT WHEN curAgreements1%ROWCOUNT + curAgreements2%ROWCOUNT > 100;
            
            -- Important: Do not change the format of this info, it's used in extraction of report
            INFO('Processing contract {4}{1}/{2} with mvtident = {3}',
                curAgreements1%ROWCOUNT + curAgreements2%ROWCOUNT, agrCount1 + agrCount2, a.mvtident,
                case when curAgreements1%NOTFOUND then 'pool ' else '' end);
            
            a.refcon_c := null;        
            loop
            begin
                SELECT
                    H.REFCON, H.MVTIDENT, H.SICOVAM, H.CONTREPARTIE, H.MIRROR_REFERENCE, H.DATEVAL,
                    DECODE(H.REAL_DATEVAL, NULL_DATE, NULL, H.REAL_DATEVAL), NVL(H.DELIVERY_DATE, H.DATEVAL),
                    DECODE(H.REAL_DELIVERY_DATE, NULL_DATE, NULL, H.REAL_DELIVERY_DATE), H.DELIVERY_TYPE, H.ENTITE, H.COMMISSION_DATE,
                    HC.REFCON, HC.DATENEG, HC.DATEVAL, DECODE(HC.REAL_DATEVAL, NULL_DATE, NULL, HC.REAL_DATEVAL),
                    NVL(HC.DELIVERY_DATE, HC.DATEVAL), DECODE(HC.REAL_DELIVERY_DATE, NULL_DATE, NULL, HC.REAL_DELIVERY_DATE), HC.DELIVERY_TYPE,
                    T.AFFECTATION, T.PERIMETERID, T.DEVISECTT,
                    CASE WHEN T.DEVISEAC IS NULL OR T.DEVISEAC = 0 THEN H.DEVISEPAY ELSE T.DEVISEAC END, T.TAUX_VAR,
                    CASE WHEN T.TYPE = 'C' AND T.MODELE = 'Collateral' THEN 1 ELSE 0 END, decode (GP_SOURCE, SRC_PNL, TP_FT, NVL(TP.VALUE, TP_FD)), --PNL : Facturation en dates Théoriques
                    CASE WHEN GP_PERIODICITE = PER_M THEN DebMen WHEN GP_PERIODICITE = PER_I AND GP_SOURCE != SRC_PNL THEN HC.DATEVAL END
                    into a
                FROM HISTOMVTS H
                    JOIN TITRES T ON T.SICOVAM = H.SICOVAM
                    LEFT JOIN TIERSPROPERTIES TP ON TP.CODE = H.CONTREPARTIE AND TP.NAME = TP_BILLING
                    LEFT JOIN HISTOMVTS HC ON HC.MVTIDENT = H.MVTIDENT AND HC.TYPE IN (102, 501)
                        AND ((T.J1REFCON2 = 2 -- Mensuel
                                AND GP_SOURCE != SRC_PNL  --source differente de PNL
                                AND HC.BACKOFFICE IN (
                                    SELECT KSC.KERNEL_STATUS_ID FROM BO_KERNEL_STATUS_COMPONENT KSC
                                        JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                                        AND KSG.RECORD_TYPE = 1 AND KSG.NAME = 'All But Pending FO'))
                            OR ( (GP_SOURCE = SRC_PNL  --source egale a PNL
                                OR T.J1REFCON2 = 1) -- Infine
                                AND HC.BACKOFFICE NOT IN (
                                    SELECT KSC.KERNEL_STATUS_ID
                                    FROM BO_KERNEL_STATUS_COMPONENT KSC
                                        JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                                            AND KSG.NAME IN ('_STEP_Cancelled', '_MOD_Cancelled') AND KSG.RECORD_TYPE = 1)))
                        AND (a.refcon_c IS NULL OR HC.REFCON = a.refcon_c)
                WHERE H.REFCON = a.refcon;
                exit;
            exception
                when TOO_MANY_ROWS then
                    ERROR('Position has many closing. MEP = {1}. Last closing = {2} --> reject', a.refcon, a.refcon_c);
                    raise_application_error(ERR_MANY_ROWS,
                        FORMAT('Position has many closing. MEP = {1}. Last closing = {2} --> reject', a.refcon, a.refcon_c));
                    -- the following query does not execute
                    -- il will be moved to the begin of block when no error raised
                    SELECT MAX(HC.REFCON)
                        into a.refcon_c
                    FROM HISTOMVTS H
                        LEFT JOIN HISTOMVTS HC ON HC.MVTIDENT = H.MVTIDENT AND HC.TYPE IN (102, 501)
                            AND HC.BACKOFFICE IN (
                                SELECT KSC.KERNEL_STATUS_ID FROM BO_KERNEL_STATUS_COMPONENT KSC
                                    JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                                    AND KSG.RECORD_TYPE = 1 AND KSG.NAME = 'All But Pending FO')
                    WHERE H.REFCON = a.refcon;
            end;
            end loop;
            
            INFO('refcon = {1}, mvtident = {2}, sicovam = {3}, contrepartie = {4}, mirror_reference = {5}, dateval = {6}, real_dateval = {7}',
                a.refcon, a.mvtident, a.sicovam, a.contrepartie, a.mirror_reference, a.dateval, a.real_dateval);
            INFO('delivery_date = {1}, real_delivery_date = {2}, delivery_type = {3}, entite = {4}, commission_date = {5}, refcon_c = {6}, dateneg_c={7}, dateval_c = {8}',
                a.delivery_date, a.real_delivery_date, a.delivery_type, a.entite, a.commission_date, a.refcon_c, a.dateneg_c, a.dateval_c);
            INFO('real_dateval_c = {1}, delivery_date_c = {2}, real_delivery_date_c = {3}, delivery_type_c = {4}',
                a.real_dateval_c, a.delivery_date_c, a.real_delivery_date_c, a.delivery_type_c);
            INFO('affectation = {1}, perimeterid = {2}, devisectt = {3}, deviseac = {4}, taux_var = {5}, tiersproperties = {6}',
                a.affectation, a.perimeterid, a.devisectt, a.deviseac, a.taux_var, a.tp_value);

            --For debug
            --a.tp_value := TP_FT;
            if a.tp_value is null or a.tp_value not in (TP_FT, TP_FR) then
                raise_application_error(ERR_TIERSPROPERTIES, FORMAT('TiersProperties is incorrect for tiers {1}', a.contrepartie));
            end if;
            
            if a.deviseac is null or a.deviseac = 0 then
                WARN('mvtident {1} sans devise de facturation --> contrat non traité', a.mvtident);
                goto FIN;
            end if;
            
            -- delete old data
            mvtident_mirror := null;
            if a.mirror_reference = -1 then
            begin
                SELECT H.MVTIDENT
                    into mvtident_mirror
                FROM HISTOMVTS H WHERE H.MIRROR_REFERENCE = a.refcon;
            exception
                when no_data_found then
                    WARN('Delete old data CMA. Mirroring impossible pour refcon {1}', a.refcon);
            end;
            end if;
            
            -- old data CMA was removed before selection of contract. Here we try to delete one more times
            if GP_SOURCE not in (SRC_I, SRC_PNL) and GP_MODE != MODE_S then
                if a.is_pool = 1 then
                    rc := 0;
                    for i in 1..pl.count
                    loop
                        if pl(i).entite = a.entite and pl(i).contrepartie = a.contrepartie and
                            pl(i).perimeterid = a.perimeterid and pl(i).devisectt = a.devisectt and
                            pl(i).taux_var = a.taux_var then
                            WARN('CMA already deleted for the POOL.');
                            rc := 1;--> do not delete CMA
                            exit;
                        end if;
                    end loop;
                    if rc = 0 then
                        DEBUG('Delete old data CMA for Pool. (entite={1}, contrepartie={2}, perimeterid={3}, devisectt={4}, taux_var={5})',
                            a.entite, a.contrepartie, a.perimeterid, a.devisectt, a.taux_var);
                        DELETE CMA_RPT_EXPLANATIONS WHERE OP_ID IN (
                            SELECT OP_ID FROM CMA_RPT_OPERATIONS
                            WHERE MOIS_FACT = DebMen
                                AND MVTIDENT IN (
                                    SELECT MVTIDENT FROM BL_CONTRACTS
                                    WHERE PERIMETERID = a.perimeterid AND DEVISECTT = a.devisectt AND TAUX_VAR = a.taux_var
                                        AND ENTITE = a.entite AND CONTREPARTIE = a.contrepartie));
                                    --SELECT DISTINCT H.MVTIDENT FROM HISTOMVTS H
                                    --    JOIN TITRES T ON T.SICOVAM = H.SICOVAM
                                    --        AND T.TYPE = 'C' AND T.MODELE = 'Collateral' AND T.AFFECTATION IN (11, 60)
                                    --        AND T.PERIMETERID = a.perimeterid AND T.DEVISECTT = a.devisectt AND T.TAUX_VAR = a.taux_var
                                    --WHERE H.ENTITE = a.entite AND H.CONTREPARTIE = a.contrepartie));
                        DEBUG('delete CMA_RPT_EXPLANATIONS for Pool. rowcount = {1}', SQL%ROWCOUNT);
                            DELETE CMA_RPT_OPERATIONS
                            WHERE MOIS_FACT = DebMen
                                AND MVTIDENT IN (
                                    SELECT MVTIDENT FROM BL_CONTRACTS
                                    WHERE PERIMETERID = a.perimeterid AND DEVISECTT = a.devisectt AND TAUX_VAR = a.taux_var
                                        AND ENTITE = a.entite AND CONTREPARTIE = a.contrepartie);
                                    --SELECT DISTINCT H.MVTIDENT FROM HISTOMVTS H
                                    --    JOIN TITRES T ON T.SICOVAM = H.SICOVAM
                                    --        AND T.TYPE = 'C' AND T.MODELE = 'Collateral' AND T.AFFECTATION IN (11, 60)
                                    --        AND T.PERIMETERID = a.perimeterid AND T.DEVISECTT = a.devisectt AND T.TAUX_VAR = a.taux_var
                                    --WHERE H.ENTITE = a.entite AND H.CONTREPARTIE = a.contrepartie);
                        DEBUG('delete CMA_RPT_OPERATIONS for Pool. rowcount = {1}', SQL%ROWCOUNT);
                    end if;
                else
                    DEBUG('Delete old data CMA. mvtident = {1}, mvtident_mirror = {2}', a.mvtident, mvtident_mirror);
                    DELETE CMA_RPT_EXPLANATIONS WHERE OP_ID IN (
                        SELECT OP_ID FROM CMA_RPT_OPERATIONS WHERE MVTIDENT IN (a.mvtident, mvtident_mirror) AND (GP_PERIODICITE = PER_I OR MOIS_FACT = DebMen));
                    DEBUG('delete CMA_RPT_EXPLANATIONS rowcount = {1}', SQL%ROWCOUNT);
                        
                    DELETE CMA_RPT_OPERATIONS WHERE MVTIDENT IN (a.mvtident, mvtident_mirror) AND (GP_PERIODICITE = PER_I OR MOIS_FACT = DebMen);
                    DEBUG('delete CMA_RPT_OPERATIONS rowcount = {1}', SQL%ROWCOUNT);
                end if;
                
                COMMIT;
            end if;
        
        <<CalculCommissions>>    
            DEBUG('starting CalculCommissions');
            
            SELECT COUNT(*) into rc
            FROM HISTOMVTS H
                JOIN TITRES T ON T.SICOVAM = H.SICOVAM
                    AND ((T.AFFECTATION IN (59, 61, 64) AND T.AMORT NOT IN (1)) -- L/B, L/B Stock Collat, Triparty L/B -- Without Commission
                        OR (T.AFFECTATION IN (60) AND T.AMORT NOT IN (1, 7))) -- L/B Cash Collat -- Without Commission, Rebates
            WHERE H.REFCON = a.refcon;
            
            DEBUG('rc = {1}', rc);
            
            if rc = 0 then
                DEBUG('No Commissions');
                goto CalculRemunCollat;
            end if;
            
            DEBUG('Commissions potentielles a calculer');
            
            if a.tp_value = TP_FT then -- dates theoriques
                DEBUG('dates theoriques');
                
                DebPos := a.delivery_date;
                -- use case instead of select nvl2 from dual --> more performance
                -- may i have to check a.refcon_c is not null -- CLOSING exists
                FinPos := case  when a.delivery_date_c is not null then a.delivery_date_c - 1
                                --removed in spec 1.5
                                --when a.commission_date is not null and a.commission_date > TODAY then a.commission_date - 1
                                else MAX_DATE end;
                
                if DebPos is null or FinPos is null then
                    ERROR('DebPos or FinPos is null for refcon={1}', a.refcon);
                    goto CalculRemunCollat;
                end if; 
            elsif a.tp_value = TP_FR then -- FR dates reels
                DEBUG('dates reels');
                
                if a.real_delivery_date is null then -- MEP n'est pas encore denouee
                    DEBUG('MEP pas encore denouee');
                    
                    goto CalculRemunCollat;
                else
                    DebPos := a.real_delivery_date;
                    -- may i have to check a.refcon_c is not null -- CLOSING exists
                    if GP_PERIODICITE = PER_M then -- Mensuel
                        FinPos := case  when a.real_delivery_date_c is not null then a.real_delivery_date_c - 1
                                        --removed in spec 1.5
                                        --when a.commission_date is not null and a.commission_date > TODAY then a.commission_date - 1
                                        else MAX_DATE end;
                    elsif GP_PERIODICITE = PER_I then -- InFine
                        FinPos := a.delivery_date_c - 1;
                    end if; 
                end if;
            end if;
            
                     
            DEBUG('DebPos = {1}, FinPos = {2}', DebPos, FinPos);
            
            if DebPos > FinPos then
                ERROR('Date debut position > Date fin position pour le calcul des commissions');
                goto CalculRemunCollat;
            end if;
            
            if GP_PERIODICITE = PER_I then -- Infine
                if (GP_SOURCE = SRC_PNL) then
                    DebCom := greatest(DebMen, DebPos);
                    FinCom := least(FinMen, FinPos);
                else
                    DebCom := DebPos;
                    FinCom := FinPos;
                end if;
            elsif GP_PERIODICITE = PER_M then -- Mensuel
                DebCom := greatest(DebMen, DebPos);
                FinCom := least(FinMen, FinPos);
            end if;
            DEBUG('DebCom = {1}, FinCom = {2}', DebCom, FinCom);
            
            if DebCom > FinCom then
                DEBUG('No commission period');
                goto CalculRemunCollat;
            end if;
            
            DEBUG('Commission period found');
            BEGIN
                SAVEPOINT SP_FEES;
                CALCUL_COMMISSION(a.refcon, a.dateneg_c, DebCom, FinCom, ComList, FeePayed, Balance_MinFee, TotalFees, DebPos, p_isFilled);
                
                -- FM, Q generate ticket
                montant := case when GP_PERIODICITE = PER_M then TotalFees + Balance_MinFee
                                when GP_PERIODICITE = PER_I then TotalFees + Balance_MinFee - FeePayed end;
                output_params('Montant_F') := ROUND_DATA(montant);
                if GP_SOURCE in (SRC_FM, SRC_Q) and ROUND_DATA(montant) !=0 then
                    -- insert ticket histomvts by GXML.
                    SEND_TO_GXML(a.refcon, FORMAT('BG_{1}_{2}_{3}_0_{4}_{5}', a.mvtident, a.refcon,
                        to_char(case when GP_PERIODICITE = PER_M then FinMen
                                when GP_PERIODICITE = PER_I then a.date_situ end, 'YYYYMMDD'), GP_SOURCE, GP_PERIODICITE), 7,
                        case when GP_PERIODICITE = PER_M then FinMen
                            when GP_PERIODICITE = PER_I then a.dateneg_c end,
                        case when GP_PERIODICITE = PER_M then FinMen
                            when GP_PERIODICITE = PER_I then a.date_situ end,
                        montant);
                end if;
                
                if GP_SOURCE not in (SRC_I, SRC_PNL) and GP_MODE != MODE_S then
                    GENERATE_CMA(case when GP_PERIODICITE = PER_M then DebMen else a.date_situ end,
                        a.refcon, CMA_FEES, ComList, FeePayed, Balance_MinFee, TotalFees);
                end if;
                COMMIT;
            EXCEPTION
                WHEN OTHERS THEN
                    ERROR('REPORT. COMMISSION error(mvtident={4},refcon={5},datesitu={6},source={7}). code = {1}, message = {2}, backtrace={3}',
                        SQLCODE, SQLERRM, DBMS_UTILITY.FORMAT_ERROR_BACKTRACE, a.mvtident, a.refcon, a.date_situ, GP_SOURCE);
                    ROLLBACK TO SAVEPOINT SP_FEES;
                    -- only one contract is processed
                    if agrCount1 + agrCount2 = 1 then
                        raise;
                    end if;                    
            END; 

        <<CalculRemunCollat>>
            DEBUG('starting CalculRemunCollat');
            
            SELECT COUNT(*) into rc
            FROM HISTOMVTS H
                JOIN TITRES T ON T.SICOVAM = H.SICOVAM
                    AND ((T.TYPE = 'C' AND T.MODELE = 'Collateral' AND T.AFFECTATION IN (11, 60)) -- POOL
                        OR T.AFFECTATION IN (62, 63, 65) -- REPO, REPO Pool, Triparty REPO
                        OR (T.AFFECTATION IN (60) AND T.CAPITALISE2 IN (3))) -- L/B Cash Collat -- Per Contract
            WHERE H.REFCON = a.refcon;
            
            DEBUG('rc = {1}', rc);
            
            if rc = 0 then
                DEBUG ('No RemunCollat');
                goto FIN;
            end if;
            
            DEBUG('RemunCollat potentielles a calculer');
            
            if a.is_pool = 1 then -- POOL
                DEBUG('POOL');
                for i in 1..pl.count
                loop
                    if pl(i).entite = a.entite and pl(i).contrepartie = a.contrepartie and
                        pl(i).perimeterid = a.perimeterid and pl(i).devisectt = a.devisectt and
                        pl(i).taux_var = a.taux_var then
                        DEBUG('Pool already processed');
                        goto FIN;
                    end if;
                end loop;
                pl.extend;
                pl(pl.count).entite := a.entite;
                pl(pl.count).contrepartie := a.contrepartie;
                pl(pl.count).perimeterid := a.perimeterid;
                pl(pl.count).devisectt := a.devisectt;
                pl(pl.count).taux_var := a.taux_var;
                DebReb := DebMen;
                FinReb := FinMen;
            else
                if a.tp_value = TP_FT then -- dates theoriques
                    DEBUG('dates theoriques');
                    
                    DebPos := a.dateval;
                    -- use case instead of select nvl2 from dual --> more performance
                    FinPos := case  when a.dateval_c is not null then a.dateval_c - 1
                                    --removed in spec 1.5
                                    --when a.commission_date is not null and a.commission_date > TODAY then a.commission_date - 1
                                    else MAX_DATE end;
                elsif a.tp_value = TP_FR then -- dates reels
                    DEBUG('dates reels');
                    
                    --20121112 : modification de la règle pour la détermination des bornes de la position pour le calcul de la remun collat, sans delivery_type
                    if a.real_dateval is null then -- MEP n'est pas encore denouee en Cash
                        DEBUG('MEP pas encore denouee en Cash');
                    
                        goto FIN;
                    end if;
                   
                    DebPos := a.real_dateval;
                    if GP_PERIODICITE = PER_M then -- Mensuel
                        if a.refcon_c is not null then
                            if a.real_dateval_c is null then -- CLOSING n'est pas encore denouee en cash
                                DEBUG('Closing pas encore denoue en Cash');
                                
                                FinPos := MAX_DATE;
                            else
                                FinPos := a.real_dateval_c - 1;
                            end if;
                        --removed in spec 1.5
                        --elsif a.commission_date is not null and a.commission_date > TODAY then
                        --    FinPos := a.commission_date - 1;
                        else
                            FinPos := MAX_DATE;
                        end if;
                    elsif GP_PERIODICITE = PER_I then -- Infine
                        FinPos := a.dateval_c - 1;
                    end if;
                end if;

                if DebPos is null then
                    DebPos := NULL_DATE;
                end if;
                
                DEBUG('DebPos = {1}, FinPos = {2}', DebPos, FinPos);
            
                if DebPos > FinPos then
                    ERROR('Date debut position > Date fin position pour le calcul de la remun collat');
                    goto FIN;
                end if;
            
                if GP_PERIODICITE = PER_I then -- Infine
                    if (GP_SOURCE = SRC_PNL) then
                        DebReb := greatest(DebMen, DebPos);
                        FinReb := least(FinMen, FinPos);
                    else
                        DebReb := DebPos;
                        FinReb := FinPos;
                    end if;
                elsif GP_PERIODICITE = PER_M then -- Mensuel
                    DebReb := greatest(DebMen, DebPos);
                    FinReb := least(FinMen, FinPos);
                end if;
            
            end if;
                        
            DEBUG('DebReb = {1}, FinReb = {2}', DebReb, FinReb);
            
            if DebReb > FinReb then
                DEBUG('No RemunCollat period');
                goto FIN;
            end if;
            
            DEBUG('RemunCollat period found');
            BEGIN
                SAVEPOINT SP_REBATES;
                CALCUL_REMUN_COLLAT(a.refcon, a.dateneg_c, DebReb, FinReb, RebPoolList, RebHorsPoolList, RebatePayed, TotalRebates, p_isFilled);
                
                -- when POOL, take the MAX(mvtident) of the pool
                if a.is_pool = 1 then
                begin
                    /*
                    SELECT MAX(H.REFCON), H.MVTIDENT
                        into refcon_pool, mvtident_pool
                    FROM HISTOMVTS H
                        JOIN TITRES T ON T.SICOVAM = H.SICOVAM
                            AND T.TYPE = 'C' AND T.MODELE = 'Collateral' AND T.AFFECTATION IN (11, 60)
                            AND T.PERIMETERID = a.perimeterid AND T.DEVISECTT = a.devisectt AND T.TAUX_VAR = a.taux_var
                        JOIN NATIXIS_FOLIO_SECTION_ENTITE FSE ON FSE.IDENT = H.OPCVM AND FSE.SECTION != '799' -- not in section simulation
                    WHERE H.TYPE = 16 AND H.DATEVAL <= FinMen
                        AND H.BACKOFFICE IN (
                            SELECT KSC.KERNEL_STATUS_ID FROM BO_KERNEL_STATUS_COMPONENT KSC
                                JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                                AND KSG.RECORD_TYPE = 1 AND KSG.NAME = 'All But Pending FO')
                        AND H.ENTITE = a.entite AND H.CONTREPARTIE = a.contrepartie
                        AND H.MVTIDENT = 
                            (SELECT MAX(MVTIDENT) FROM HISTOMVTS HM
                                JOIN TITRES T ON T.SICOVAM = HM.SICOVAM
                                    AND T.TYPE = 'C' AND T.MODELE = 'Collateral' AND T.AFFECTATION IN (11, 60)
                                    AND T.PERIMETERID = a.perimeterid AND T.DEVISECTT = a.devisectt AND T.TAUX_VAR = a.taux_var
                                JOIN NATIXIS_FOLIO_SECTION_ENTITE FSE ON FSE.IDENT = HM.OPCVM AND FSE.SECTION != '799' -- not in section simulation
                            WHERE HM.TYPE = 16 AND HM.DATEVAL <= FinMen
                                AND HM.BACKOFFICE IN (
                                    SELECT KSC.KERNEL_STATUS_ID FROM BO_KERNEL_STATUS_COMPONENT KSC
                                        JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                                        AND KSG.RECORD_TYPE = 1 AND KSG.NAME = 'All But Pending FO')
                                AND HM.ENTITE = a.entite AND HM.CONTREPARTIE = a.contrepartie)
                    GROUP BY H.MVTIDENT;*/
                    SELECT REFCON, MVTIDENT
                        into refcon_pool, mvtident_pool
                    FROM BL_CONTRACTS
                    WHERE ENTITE = a.entite AND CONTREPARTIE = a.contrepartie
                        AND PERIMETERID = a.perimeterid AND DEVISECTT = a.devisectt AND TAUX_VAR = a.taux_var
                        AND MVTIDENT = 
                            (SELECT MAX(MVTIDENT)
                            FROM BL_CONTRACTS
                            WHERE ENTITE = a.entite AND CONTREPARTIE = a.contrepartie
                                AND PERIMETERID = a.perimeterid AND DEVISECTT = a.devisectt AND TAUX_VAR = a.taux_var);
                    DEBUG('Pool. refcon, mvtident = {1}, {2}', refcon_pool, mvtident_pool);
                exception
                    when no_data_found then
                        ERROR('No pool found');
                        raise;
                end;
                end if;
                
                -- FM, Q generate ticket
                montant := case when GP_PERIODICITE = PER_M then TotalRebates
                                when GP_PERIODICITE = PER_I then TotalRebates - RebatePayed end;
                if (a.affectation in (60) or a.is_pool = 1) then
                    output_params('Montant_R') := ROUND_DATA(montant);
                else
                    output_params('Montant_F') := ROUND_DATA(montant);
                end if;
                if GP_SOURCE in (SRC_FM, SRC_Q) and ROUND_DATA(montant) !=0 then
                    -- insert ticket histomvts by GXML.
                    if a.is_pool = 1 then
                        SEND_TO_GXML(refcon_pool, FORMAT('BG_{1}_{2}_{3}_1_{4}_{5}', mvtident_pool, refcon_pool,
                            to_char(FinMen, 'YYYYMMDD'), GP_SOURCE, GP_PERIODICITE),
                            case when (a.affectation in (60) or a.is_pool = 1) then 101 else 7 end, FinMen, FinMen, TotalRebates);
                    else
                        SEND_TO_GXML(a.refcon, FORMAT('BG_{1}_{2}_{3}_1_{4}_{5}', a.mvtident, a.refcon,
                            to_char(case when GP_PERIODICITE = PER_M then FinMen
                                when GP_PERIODICITE = PER_I then a.date_situ end, 'YYYYMMDD'), GP_SOURCE, GP_PERIODICITE),
                            case when (a.affectation in (60) or a.is_pool = 1) then 101 else 7 end,
                            case when GP_PERIODICITE = PER_M then FinMen
                                when GP_PERIODICITE = PER_I then a.dateneg_c end,
                            case when GP_PERIODICITE = PER_M then FinMen
                                 when GP_PERIODICITE = PER_I then a.date_situ end,
                            montant);
                    end if;
                end if;
                
                if GP_SOURCE not in (SRC_I, SRC_PNL) and GP_MODE != MODE_S then
                    if a.is_pool = 1 then
                        GENERATE_CMA(DebMen,
                            refcon_pool, CMA_REBATES, null, null, null, null, RebPoolList, RebHorsPoolList, RebatePayed, TotalRebates);
                    else
                        GENERATE_CMA(case when GP_PERIODICITE = PER_M then DebMen else a.date_situ end,
                            a.refcon, CMA_REBATES, null, null, null, null, RebPoolList, RebHorsPoolList, RebatePayed, TotalRebates);
                    end if;
                end if;
                COMMIT;
            EXCEPTION
                WHEN OTHERS THEN
                    ERROR('REPORT. REMUN COLLAT error(mvtident={4},refcon={5},datesitu={6},source={7}). code = {1}, message = {2}, backtrace={3}',
                        SQLCODE, SQLERRM, DBMS_UTILITY.FORMAT_ERROR_BACKTRACE, a.mvtident, a.refcon, a.date_situ, GP_SOURCE);
                    ROLLBACK TO SAVEPOINT SP_REBATES;                    
                    -- only one contract is processed
                    if agrCount1 + agrCount2 = 1 then
                        raise;
                    end if;                    
            END; 

        <<FIN>>
            INFO('TotalFees={1}, FeePayed={2}, Balance_MinFee={3}', TotalFees, FeePayed, Balance_MinFee);
            INFO('TotalRebates={1}, RebatePayed={2}', TotalRebates, RebatePayed);
        exception
            when others then
                error('REPORT. Contract error(mvtident={4},refcon={5},datesitu={6},source={7}). code = {1}, message = {2}, backtrace={3}',
                    SQLCODE, SQLERRM, DBMS_UTILITY.FORMAT_ERROR_BACKTRACE, a.mvtident, a.refcon, a.date_situ, GP_SOURCE);
                -- only one contract is processed
                if agrCount1 + agrCount2 = 1 then
                    raise;
                end if;                    
        end; -- end of exception catch in loop    
        END LOOP;
        close curAgreements1;
        close curAgreements2;
        
        <<RemoveDeal>>
        -- suppression des tickets qui ont été générés à tort par SRC_FM
        -- pas la peine d'attendre la fin de GXML car c'est indépendant.
        if GP_SOURCE in (SRC_FM) then
            INFO('Remove deal generated by previous FM generation');
            /*
            whereSQL := BUILD_WHERE_COND('', p_entite, p_contrepartie, p_mvtident, p_devise, p_perimeterid);
            if p_mvtident is not null and mvtident_mirror is not null then
                whereSQL := replace(whereSQL, FORMAT('MVTIDENT = {1}', p_mvtident), FORMAT('MVTIDENT IN ({1}, {2})', a.mvtident, mvtident_mirror));
            end if;
            if p_mvtident is not null and a.is_pool = 1 then
                --TODO: Perf
                DEBUG('POOL. remove the deal of the pool', a.mvtident, mvtident_pool);
                whereSQL := replace(whereSQL, FORMAT('MVTIDENT = {1}', p_mvtident), FORMAT(
'MVTIDENT IN 
    (SELECT DISTINCT HM.MVTIDENT FROM HISTOMVTS HM
        JOIN TITRES T ON T.SICOVAM = HM.SICOVAM
            AND T.TYPE = ''C'' AND T.MODELE = ''Collateral'' AND T.AFFECTATION IN (11, 60)
            AND T.PERIMETERID = {1} AND T.DEVISECTT = {2} AND T.TAUX_VAR = {3}
    WHERE HM.TYPE = 16
        AND HM.ENTITE = {4} AND HM.CONTREPARTIE = {5})', a.perimeterid, a.devisectt, a.taux_var, a.entite, a.contrepartie));
            end if;
            
            strSQL := FORMAT('
SELECT H.REFCON, OP.OP_ID FROM HISTOMVTS H
    JOIN CMA_RPT_OPERATIONS OP ON OP.TA_REFCON = H.REFCON AND OP.MOIS_FACT = NUM_TO_DATE({1})
    JOIN TITRES T ON T.SICOVAM = H.SICOVAM AND T.J1REFCON2 = {3} -- Mensuel
WHERE (H.MIRROR_REFERENCE IS NULL OR H.MIRROR_REFERENCE IN (0, -1)) -- si deal mirroirs, prendre que les deals peres
    AND H.ECN = ''{4}''
    AND ((H.MIRROR_REFERENCE = -1 AND IS_CPTY_INTERNAL(H.CONTREPARTIE, H.ENTITE) = 1
        AND H.BACKOFFICE NOT IN (
            SELECT KSC.KERNEL_STATUS_ID
            FROM BO_KERNEL_STATUS_COMPONENT KSC
                JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                    AND KSG.NAME IN (''_STEP_Cancelled'', ''_MOD_Cancelled'') AND KSG.RECORD_TYPE = 1))
        OR (NVL(H.MIRROR_REFERENCE, 0) = 0 AND H.BACKOFFICE = 1071))
    {2}', date_to_num(DebMen), whereSQL, PER_M, GXML_ECN);*/
    
            -- no need to test where condition here, it's already done in the step Sauvegarde des tickets
            -- mvtident also selected here for performance optimization when delete CMA
            strSQL := '
SELECT TA_REFCON, H.MVTIDENT FROM BL_OPERATIONS
    JOIN HISTOMVTS H ON H.REFCON = TA_REFCON';
                INFO(strSQL);
                open curDeals for strSQL;
                loop
                    fetch curDeals into a.refcon, rc;
                    exit when curDeals%NOTFOUND;
                    if GP_MODE = MODE_S then   --MODE SIMULATION
                        INFO('SIMULATION. provenance=S; refcon={1};', a.refcon);
                    else
                        begin
                            SAVEPOINT SP_REMOVE_DEAL;
                            
                            GXML_CANCEL(FORMAT('BG_{1}', a.refcon), a.refcon);
                            DELETE CMA_RPT_EXPLANATIONS WHERE OP_ID IN (SELECT OP_ID FROM CMA_RPT_OPERATIONS WHERE MVTIDENT = rc AND TA_REFCON = a.refcon);
                            DELETE CMA_RPT_OPERATIONS WHERE MVTIDENT = rc AND TA_REFCON = a.refcon;
                            --DELETE CMA_RPT_EXPLANATIONS WHERE OP_ID = rc;
                            --DELETE CMA_RPT_OPERATIONS WHERE OP_ID = rc;
                             
                            COMMIT;
                        exception
                            when others then
                                ERROR('Remove Deal error. code = {1}, message = {2}, backtrace={3}', SQLCODE, SQLERRM, DBMS_UTILITY.FORMAT_ERROR_BACKTRACE);
                                ROLLBACK TO SAVEPOINT SP_REMOVE_DEAL;
                        end;
                    end if;
                end loop;
                INFO('Deals removed (rowcount = {1})', curDeals%ROWCOUNT);
                rc := curDeals%ROWCOUNT;
                close curDeals;
                
                -- Comme GXML est en mode asynchrone, nous devons attendre qu'il soit fini ou RIEN bougé
                --WAIT_FOR_GXML(GXML_SYSTEM);
            
        end if;            
        
        <<UpdateCMA>>
        -- Find out the result when FM and Q (GXML was called)
        if GP_SOURCE in (SRC_FM, SRC_Q) and GP_MODE != MODE_S then
            -- Comme GXML est en mode asynchrone, nous devons attendre qu'il soit fini ou RIEN bougé
            WAIT_FOR_GXML(GXML_SYSTEM);
            
            --if GXML does not create link of mirrors. Do it here.
            
            INFO('Make link CMA-GXML');
            whereSQL := BUILD_WHERE_COND('', p_entite, p_contrepartie, p_mvtident, p_devise, p_perimeterid);
            if p_mvtident is not null and mvtident_mirror is not null then
                whereSQL := replace(whereSQL, FORMAT('MVTIDENT = {1}', p_mvtident), FORMAT('MVTIDENT IN ({1}, {2})', a.mvtident, mvtident_mirror));
            end if;
            if p_mvtident is not null and a.is_pool = 1 then
                DEBUG('POOL. change mvtident from {1} to {2}', a.mvtident, mvtident_pool);
                whereSQL := replace(whereSQL, FORMAT('MVTIDENT = {1}', p_mvtident), FORMAT('MVTIDENT = {2}', a.mvtident, mvtident_pool));
            end if;
            
            strSQL := FORMAT('
INSERT INTO BL_OPERATIONS
    SELECT OP_ID, MONTANT, REFCON, R FROM 
        (SELECT OP.OP_ID, H.MONTANT, H.REFCON, RANK() OVER (PARTITION BY OP.OP_ID ORDER BY H.REFCON) R
        FROM CMA_RPT_OPERATIONS OP
        JOIN HISTOMVTS H
            JOIN TITRES T ON T.SICOVAM = H.SICOVAM
                AND (T.J1REFCON2 = {3} OR (T.TYPE = ''C'' AND T.MODELE = ''Collateral''))
            LEFT JOIN HISTOMVTS HC ON HC.MVTIDENT = H.MVTIDENT AND HC.TYPE IN (102, 501)
                AND {3} = 1 AND HC.DATEVAL >= NUM_TO_DATE({4})
                AND HC.BACKOFFICE NOT IN (
                    SELECT KSC.KERNEL_STATUS_ID
                    FROM BO_KERNEL_STATUS_COMPONENT KSC
                        JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                            AND KSG.NAME IN (''_STEP_Cancelled'', ''_MOD_Cancelled'') AND KSG.RECORD_TYPE = 1)
            ON H.MVTIDENT = OP.MVTIDENT
                AND ((OP.MVTTYPE IN (0, 2) AND H.TYPE IN (7, 700)) OR (OP.MVTTYPE IN (1, 3) AND H.TYPE IN (101, 701)))
                AND (({3} = 2 AND H.DATENEG = NUM_TO_DATE({1}) -- Mensuel
                        AND H.BACKOFFICE IN (
                            SELECT KSC.KERNEL_STATUS_ID FROM BO_KERNEL_STATUS_COMPONENT KSC
                                JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                            AND KSG.RECORD_TYPE = 1 AND KSG.NAME = ''All But Pending FO''))
                    OR ({3} = 1 AND H.DATENEG = HC.DATENEG AND H.DATEVAL >= NUM_TO_DATE({4}) -- Infine
                        AND H.BACKOFFICE NOT IN (
                            SELECT KSC.KERNEL_STATUS_ID
                            FROM BO_KERNEL_STATUS_COMPONENT KSC
                                JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                                    AND KSG.NAME IN (''_STEP_Cancelled'', ''_MOD_Cancelled'') AND KSG.RECORD_TYPE = 1)))
        WHERE (({3} = 2 AND OP.MOIS_FACT = NUM_TO_DATE({2})) OR ({3} = 1 AND OP.MOIS_FACT = HC.DATEVAL AND OP.MOIS_FACT >= NUM_TO_DATE({4})))
            -- OP.FACT_FREQ =
            -- OP.MOIS_FACT <= FinMen dans le cas Transactionnel
            {5})
    WHERE R <= 2', date_to_num(FinMen), date_to_num(DebMen), GP_PERIODICITE, date_to_num(p_date), whereSQL);
            INFO(strSQL);
            
            DELETE BL_OPERATIONS;
                
            EXECUTE IMMEDIATE strSQL;
            
            UPDATE CMA_RPT_OPERATIONS
                SET TA = NULL, TA_REFCON = NULL
            WHERE OP_ID IN (SELECT OP_ID FROM BL_OPERATIONS WHERE R > 1);
            
            UPDATE CMA_RPT_OPERATIONS OP_MAIN
                SET (TA, TA_REFCON) =
                (SELECT TA, TA_REFCON FROM BL_OPERATIONS WHERE OP_ID = OP_MAIN.OP_ID
                    AND OP_ID NOT IN (SELECT OP_ID FROM BL_OPERATIONS WHERE R > 1))
            WHERE EXISTS
                (SELECT * FROM BL_OPERATIONS OP WHERE OP.OP_ID = OP_MAIN.OP_ID
                    AND OP_ID NOT IN (SELECT OP_ID FROM BL_OPERATIONS WHERE R > 1));
            
            INFO('rows updated = {1}', SQL%ROWCOUNT);
            
            COMMIT;
        end if;
        
        FeePayed := round(FeePayed, MATH_ROUND);
        Balance_MinFee := round(Balance_MinFee, MATH_ROUND);
        TotalFees := round(TotalFees, MATH_ROUND);
        RebatePayed := round(RebatePayed, MATH_ROUND);
        TotalRebates := round(TotalRebates, MATH_ROUND);
        -- store output parameters value, to be retrieved in Sophis
        output_params('FeePayed') := FeePayed;
        output_params('Balance_MinFee') := Balance_MinFee;
        output_params('TotalFees') := TotalFees;
        output_params('RebatePayed') := RebatePayed;
        output_params('TotalRebates') := TotalRebates;
        
        <<FIN_TOUS>>
        -- unlock when batch mode
        if GP_SOURCE in (SRC_FM, SRC_PI, SRC_Q) and GP_MODE != MODE_S then
            UNLOCK_BATCH;
        end if;
        
        -- Important: Do not change the format of this info, it's used in extraction of report
        INFO('BILLING_GENERATOR.END(source={1}, date={2}, periodicite={3}, entite={4}, contrepartie={5}, devise={6}, perimeterid={7}, mvtident={8}, refcon={9}, riskuser={10}, p_mode={11}, dateFin={12})',
            GP_SOURCE, p_date, GP_PERIODICITE, p_entite, p_contrepartie, p_devise, p_perimeterid, p_mvtident, p_refcon, GP_RISKUSER, GP_MODE, p_dateFin);
        SELECT BL_LOGS_SEQ.CURRVAL INTO output_params('EndLogId') FROM DUAL;
    EXCEPTION
        WHEN OTHERS THEN 
            -- unlock in case of exception. Not the exception raised by LOCK
            if SQLCODE != ERR_LOCK then
                UNLOCK_BATCH;
            end if;
            ERROR('BILLING_GENERATOR. code = {1}, message = {2}, backtrace={3}', SQLCODE, SQLERRM, DBMS_UTILITY.FORMAT_ERROR_BACKTRACE);
            INFO('BILLING_GENERATOR.END(source={1}, date={2}, periodicite={3}, entite={4}, contrepartie={5}, devise={6}, perimeterid={7}, mvtident={8}, refcon={9}, riskuser={10}, p_mode={11})',
                GP_SOURCE, p_date, GP_PERIODICITE, p_entite, p_contrepartie, p_devise, p_perimeterid, p_mvtident, p_refcon, GP_RISKUSER, GP_MODE);
            SELECT BL_LOGS_SEQ.CURRVAL INTO output_params('EndLogId') FROM DUAL;
            RAISE;
    END;
    
    -- called from Sophis and Batch
    PROCEDURE BILLING_GENERATOR(
        p_source        in  varchar2,
        p_date          in  date,
        p_datefin       in  date default null,
        p_periodicite   in  int default 2, -- 2 = mensuel, 1 = Infine
        p_isFilled      in  boolean default false,
        p_entite        in  HISTOMVTS.ENTITE%TYPE default null,
        p_contrepartie  in  varchar2 default null,
        p_devise        in  varchar2 default null,
        p_perimeterid   in  TITRES.PERIMETERID%TYPE default null,
        p_mvtident      in  HISTOMVTS.MVTIDENT%TYPE default null,
        p_riskuser      in  RISKUSERS.IDENT%TYPE default null,
        p_refcon        in  HISTOMVTS.REFCON%TYPE default null,
        p_mode          in  varchar2 default null)
    AS
        FeePayed        number;
        Balance_MinFee  number;
        TotalFees       number;
        RebatePayed     number;
        TotalRebates    number;
    BEGIN
        BILLING_GENERATOR(p_source, p_date, p_datefin, p_periodicite, p_isFilled, p_entite, p_contrepartie, p_devise, p_perimeterid,
            p_mvtident, p_riskuser, p_refcon, p_mode, FeePayed, Balance_MinFee, TotalFees, RebatePayed, TotalRebates);
    END;
    
    -- called from IHM de Test, PNL
    PROCEDURE BILLING_GENERATOR(
        p_source        in  varchar2,
        p_date          in  date,
        p_datefin       in  date default null,
        p_periodicite   in  int default null, -- 2 = mensuel, 1 = Infine
        p_mvtident      in  HISTOMVTS.MVTIDENT%TYPE,
        p_riskuser      in  RISKUSERS.IDENT%TYPE default null,
        p_refcon        in  HISTOMVTS.REFCON%TYPE default null,
        p_mode          in  varchar2 default null,
        FeePayed        out number,
        Balance_MinFee  out number,
        TotalFees       out number,
        RebatePayed     out number,
        TotalRebates    out number)
    AS
    BEGIN
        BILLING_GENERATOR(p_source, p_date,p_datefin, p_periodicite, true, null, null, null, null,
            p_mvtident, p_riskuser, p_refcon, null, FeePayed, Balance_MinFee, TotalFees, RebatePayed, TotalRebates);  
    END;
    
   
    /******************************************************************************
    Utilities function
    ******************************************************************************/
    PROCEDURE SET_LOG_LEVEL(level int) AS
    BEGIN
        LOG_LEVEL := level;
    END;
    
    PROCEDURE SET_LOGGER(logger varchar2) AS
    BEGIN
        LOG_LOGGER := logger;
    END;
    
    PROCEDURE LOG(s varchar2, level int) AS
        severity        BL_LOGS.SEVERITY%TYPE;
        logger          BL_LOGS.LOGGER%TYPE;
        PRAGMA AUTONOMOUS_TRANSACTION;
        MAX_LENGTH      constant int := 4000; -- max length of varchar field in oracle table
        sTemp           varchar2(4000);
        pos             int;
    BEGIN
        --DBMS_OUTPUT.put(to_char(sysdate,'YYYY-MM-DD HH24:MI:SS:    '));
        DBMS_OUTPUT.put(to_char(localtimestamp,'YYYY-MM-DD HH24:MI:SS.FF2    '));
        severity := case level  when LOG_ERROR  then 'ERROR'
                                when LOG_WARN   then 'WARN'
                                when LOG_INFO   then 'INFO'
                                when LOG_DEBUG  then 'DEBUG' end;
        if sid is null then
            SELECT SYS_CONTEXT('USERENV','SID') into sid FROM DUAL;
            output_params('Logger') := LOG_LOGGER || '_' || sid;
        end if;
        logger := LOG_LOGGER || '_' || sid;
        
        pos := 1;
        while pos <= length(s)
        loop
            sTemp := substr(s, pos, MAX_LENGTH);
            INSERT INTO BL_LOGS VALUES (BL_LOGS_SEQ.NEXTVAL, localtimestamp, severity, logger, sTemp);
            pos := pos + MAX_LENGTH;
        end loop;
        COMMIT;
        DBMS_OUTPUT.put_line ('[' || severity || ']    ' || s);
    EXCEPTION
        WHEN OTHERS THEN
            ERROR('LOG. code = {1}, message = {2}, backtrace={3}', SQLCODE, SQLERRM, DBMS_UTILITY.FORMAT_ERROR_BACKTRACE);
            RAISE;
    END;
    
    PROCEDURE ERROR(s varchar2,
        p1 varchar2 default null, p2 varchar2 default null, p3 varchar2 default null,
        p4 varchar2 default null, p5 varchar2 default null, p6 varchar2 default null,
        p7 varchar2 default null, p8 varchar2 default null, p9 varchar2 default null,
        p10 varchar2 default null, p11 varchar2 default null, p12 varchar2 default null, p13 varchar2 default null) AS
    BEGIN
        if LOG_LEVEL >= LOG_ERROR then 
            if p1 is not null then
                ERROR(FORMAT(s, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13));
            else
                LOG(s, LOG_ERROR);
            end if;
        end if;
    END;
    
    PROCEDURE WARN(s varchar2,
        p1 varchar2 default null, p2 varchar2 default null, p3 varchar2 default null,
        p4 varchar2 default null, p5 varchar2 default null, p6 varchar2 default null,
        p7 varchar2 default null, p8 varchar2 default null, p9 varchar2 default null,
        p10 varchar2 default null, p11 varchar2 default null, p12 varchar2 default null, p13 varchar2 default null) AS
    BEGIN
        if LOG_LEVEL >= LOG_WARN then
            if p1 is not null then
                WARN(FORMAT(s, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13));
            else
                LOG(s, LOG_WARN);
            end if;
        end if;
    END;
    
    PROCEDURE INFO(s varchar2,
        p1 varchar2 default null, p2 varchar2 default null, p3 varchar2 default null,
        p4 varchar2 default null, p5 varchar2 default null, p6 varchar2 default null,
        p7 varchar2 default null, p8 varchar2 default null, p9 varchar2 default null,
        p10 varchar2 default null, p11 varchar2 default null, p12 varchar2 default null, p13 varchar2 default null) AS
    BEGIN
        if LOG_LEVEL >= LOG_INFO then
            if p1 is not null then
                INFO(FORMAT(s, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13));
            else
                LOG(s, LOG_INFO);
            end if;
        end if;
    END;
    
    PROCEDURE DEBUG(s varchar2,
        p1 varchar2 default null, p2 varchar2 default null, p3 varchar2 default null,
        p4 varchar2 default null, p5 varchar2 default null, p6 varchar2 default null,
        p7 varchar2 default null, p8 varchar2 default null, p9 varchar2 default null,
        p10 varchar2 default null, p11 varchar2 default null, p12 varchar2 default null, p13 varchar2 default null) AS
    BEGIN
        if LOG_LEVEL >= LOG_DEBUG then
            if p1 is not null then
                DEBUG(FORMAT(s, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13));
            else
                LOG(s, LOG_DEBUG);
            end if;
        end if;
    END;
    
    FUNCTION FORMAT(s varchar2,
        p1 varchar2 default null, p2 varchar2 default null, p3 varchar2 default null,
        p4 varchar2 default null, p5 varchar2 default null, p6 varchar2 default null,
        p7 varchar2 default null, p8 varchar2 default null, p9 varchar2 default null,
        p10 varchar2 default null, p11 varchar2 default null, p12 varchar2 default null, p13 varchar2 default null)
    RETURN varchar2
    AS
        type ParamList is varray(13) of varchar2(10000);
        params ParamList := ParamList(
            p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13);
        result varchar2(10000) := s;
    BEGIN
        for i in 1..params.count loop
            --exit when params(i) is null;
            result := replace(result, '{' || i || '}', params(i));
        end loop;
        return result;
    END;
    
    FUNCTION GET_WEEKDAY_AMERICA(p_date date)
    RETURN int
    AS
        monday      constant date := to_date('03/01/2000', 'DD/MM/YYYY');
        result      int;
    BEGIN
        --alter session set nls_territory='AMERICA';
        --AMERICA: lundi = 2, mardi = 3, ..., samedi = 7, dimanche = 1
        --alter session set nls_territory='UNITED KINGDOM';
        --UNITED KINGDOM': lundi = 1, mardi = 2, ..., samedi = 6, dimanche = 7
        
        --this function return the weekday of a date in american style without alter session
        
        result := case to_char(p_date, 'D')
            when to_char(monday + 0, 'D') THEN 2 -- lundi
            when to_char(monday + 1, 'D') THEN 3 -- mardi
            when to_char(monday + 2, 'D') THEN 4 -- mercredi
            when to_char(monday + 3, 'D') THEN 5 -- jeudi
            when to_char(monday + 4, 'D') THEN 6 -- vendredi
            when to_char(monday + 5, 'D') THEN 7 -- samedi
            when to_char(monday + 6, 'D') THEN 1 -- dimanche
            end;
            
        return result;
    END;
    
    FUNCTION GET_JOURS_OUVRES(p_date1 date, p_date2 date)
    RETURN int
    AS
        dernier_ouvre1  date;
        dernier_ouvre2  date;
    BEGIN
        dernier_ouvre1 := CASE  WHEN GET_WEEKDAY_AMERICA(p_date1) = 7 THEN p_date1 - 1
                                WHEN GET_WEEKDAY_AMERICA(p_date1) = 1 THEN p_date1 - 2
                                ELSE p_date1 END;
        dernier_ouvre2 := CASE  WHEN GET_WEEKDAY_AMERICA(p_date2) = 7 THEN p_date2 - 1
                                WHEN GET_WEEKDAY_AMERICA(p_date2) = 1 THEN p_date2 - 2
                                ELSE p_date2 END;
        return ((dernier_ouvre1 - GET_WEEKDAY_AMERICA(dernier_ouvre1)) - (dernier_ouvre2 - GET_WEEKDAY_AMERICA(dernier_ouvre2))) / 7 * 5 +
            GET_WEEKDAY_AMERICA(dernier_ouvre1) - GET_WEEKDAY_AMERICA(dernier_ouvre2);
    END;
    
    PROCEDURE RPT_ONE_RUN(
        id_start    in  int,
        id_end      out int,
        dt_start    out date,
        dt_end      out date)
    AS
    BEGIN
        SELECT MIN(E.ID)
            into id_end
        FROM BL_LOGS E
            JOIN BL_LOGS S ON S.ID = id_start AND S.LOGGER = E.LOGGER
        WHERE E.MSG LIKE 'BILLING_GENERATOR.END%'
            AND E.ID > id_start;
            
        SELECT DT
            into dt_start
        FROM BL_LOGS WHERE ID = id_start;
        
        SELECT DT
            into dt_end
        FROM BL_LOGS WHERE ID = id_end;
    END;        
    
    PROCEDURE RPT_LAST_RUN(
        id_middle   in int default null,
        id_start    out int,
        id_end      out int,
        dt_start    out date,
        dt_end      out date)
    AS
    BEGIN
        if id_middle is null then
            SELECT MAX(S.ID)
                into id_start
            FROM BL_LOGS S
                JOIN BL_LOGS E ON E.LOGGER = S.LOGGER AND E.ID > S.ID
                    AND E.ID = (SELECT MAX(ID) FROM BL_LOGS WHERE MSG LIKE 'BILLING_GENERATOR.END%')
            WHERE S.MSG LIKE 'BILLING_GENERATOR.BEGIN%';
        else
            SELECT MAX(S.ID)
                into id_start
            FROM BL_LOGS S
                LEFT JOIN BL_LOGS SM ON SM.ID = id_middle
                JOIN BL_LOGS E ON E.LOGGER = S.LOGGER AND E.ID > S.ID
                    AND E.ID >= id_middle
                    AND E.MSG LIKE 'BILLING_GENERATOR.END%'
            WHERE S.MSG LIKE 'BILLING_GENERATOR.BEGIN%'
                AND (SM.ID IS NULL OR SM.LOGGER = S.LOGGER)
                AND S.ID <= id_middle;
        end if;
        
        RPT_ONE_RUN(id_start,id_end, dt_start, dt_end);
    EXCEPTION
        when no_data_found then
            id_start := null;
            id_end := null;
    END;
    
    FUNCTION RPT_GET_ID_START(id_middle in int default null)
    RETURN int
    AS
        id_start    int;
        id_end      int;
        dt_start    date;
        dt_end      date;
    BEGIN
        RPT_LAST_RUN(id_middle, id_start, id_end, dt_start, dt_end);
        return id_start;
    END;
    
    FUNCTION RPT_GET_ID_END(id_middle in int default null)
    RETURN int
    AS
        id_start    int;
        id_end      int;
        dt_start    date;
        dt_end      date;
    BEGIN
        RPT_LAST_RUN(id_middle, id_start, id_end, dt_start, dt_end);
        return id_end;
    END;
    
    FUNCTION RPT_GET_DATE_START(id_middle in int default null)
    RETURN date
    AS
        id_start    int;
        id_end      int;
        dt_start    date;
        dt_end      date;
    BEGIN
        RPT_LAST_RUN(id_middle, id_start, id_end, dt_start, dt_end);
        return dt_start;
    END;
    
    FUNCTION RPT_GET_DATE_END(id_middle in int default null)
    RETURN date
    AS
        id_start    int;
        id_end      int;
        dt_start    date;
        dt_end      date;
    BEGIN
        RPT_LAST_RUN(id_middle, id_start, id_end, dt_start, dt_end);
        return dt_end;
    END;
    
    PROCEDURE RPT_PRINT_RUN(id_middle in int default null)
    AS
        id_start    int;
        id_end      int;
        dt_start    date;
        dt_end      date;
        duration    varchar(20);
        curLogs     SYS_REFCURSOR;
        dt_         BL_LOGS.DT%TYPE;
        severity_   BL_LOGS.SEVERITY%TYPE;
        msg_        BL_LOGS.MSG%TYPE;
    BEGIN
        RPT_LAST_RUN(id_middle, id_start, id_end, dt_start, dt_end);
        
        SELECT
            TRIM(TO_CHAR(EXTRACT(HOUR FROM (E.DT - S.DT)), '00')) || ':' ||
            TRIM(TO_CHAR(EXTRACT(MINUTE FROM (E.DT - S.DT)), '00')) || ':' ||
            TRIM(TO_CHAR(EXTRACT(SECOND FROM (E.DT - S.DT)), '00'))
            into duration                
        FROM BL_LOGS S, BL_LOGS E
        WHERE S.ID = id_start AND E.ID = id_end;
        
        open curLogs for
            SELECT * FROM (
                SELECT L.DT, L.SEVERITY, L.MSG
                FROM BL_LOGS L
                    JOIN BL_LOGS S ON S.ID = id_start AND S.LOGGER = L.LOGGER
                WHERE L.ID BETWEEN id_start AND id_end
                ORDER BY L.ID)
            WHERE ROWNUM < 1500;
        
        while curLogs%FOUND or curLogs%FOUND is null loop
            fetch curLogs into dt_, severity_, msg_;
            exit when curLogs%NOTFOUND;
            DBMS_OUTPUT.put(to_char(dt_,'YYYY-MM-DD HH24:MI:SS.FF2    '));
            DBMS_OUTPUT.put_line ('[' || severity_ || ']    ' || msg_);
        end loop;
        
        close curLogs;
        
        DBMS_OUTPUT.put_line('Duration = ' || duration);
    END;
END BL;
/

CREATE OR REPLACE PUBLIC SYNONYM BL FOR BL;
/
