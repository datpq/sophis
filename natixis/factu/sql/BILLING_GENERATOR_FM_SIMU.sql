BEGIN
      BL.BILLING_GENERATOR(
          p_source => BL.SRC_FM, -- source FM
          p_date => trunc(to_date('&1', 'YYYYMMDD'), 'MM'),
          p_periodicite => 2, -- Mensuel
          p_mode => BL.MODE_S
      );
END;
/

SET COLSEP '    '
SET trimout OFF 
SET FEEDBACK OFF
SET heading OFF 
SET verify OFF
SET space 0 
SET NEWPAGE 0 
SET PAGESIZE 0 
SET trimspool ON

SET WRAP OFF
SET TRIMOUT OFF 
SET COLSEP ';'
SET LINESIZE 2000
SET ESCAPE '\'

ALTER SESSION SET NLS_NUMERIC_CHARACTERS = ', ';

-- BillingGenerator report
spool '&2';

SELECT '<h3>BillingGenerator Mensuel - ' || TO_CHAR(TRUNC(to_date('&1', 'YYYYMMDD')), 'MON YYYY') || '</h3>' FROM DUAL;

SELECT '<br><b>Message BG</b>' FROM DUAL;
SELECT '<table border=1><tr><th>Id</th><th>DateTime</th><th>Message</th></tr>' FROM DUAL;

SELECT '<tr><td>' ||
    L.ID ||
    '</td><td>' ||
    TO_CHAR(L.DT,'YYYY-MM-DD HH24:MI:SS.FF2') ||
    '</td><td><font color=red>' ||    
    CAST(L.MSG AS VARCHAR2(500)) ||
    '</font></td></tr>' 
FROM BL_LOGS L
    JOIN BL_LOGS S ON S.ID = BL.GET_OUTPUT_VALUE('BeginLogId')
    JOIN BL_LOGS E ON E.ID = BL.GET_OUTPUT_VALUE('EndLogId')
WHERE L.SEVERITY = 'ERROR'
    AND L.MSG LIKE 'REPORT.%'
    AND L.ID BETWEEN S.ID AND E.ID 
    AND L.LOGGER = S.LOGGER
ORDER BY L.ID;

SELECT '</table><br><b>Message GXML</b>' FROM DUAL;
SELECT '<table border=1><tr><th>Id</th><th>Action</th><th>Refcon</th><th>Mvtident</th><th>Type</th><th>Message</th></tr>' FROM DUAL;

SELECT '<tr><td>' ||
    X.ID ||
    '</td><td>' ||
    CAST(EXTRACTVALUE(XMLTYPE(XML), '/Transaction/ACTION') AS VARCHAR2(10)) ||
    '</td><td>' ||
    CAST(CASE EXTRACTVALUE(XMLTYPE(XML), '/Transaction/ACTION')
    WHEN 'UPDATE' then EXTRACTVALUE(XMLTYPE(XML), '/Transaction/REFCON')
    WHEN 'CANCEL' then EXTRACTVALUE(XMLTYPE(XML), '/Transaction/REFCON')
    ELSE NULL END AS NUMBER) ||
    '</td><td>' ||
    CAST(EXTRACTVALUE(XMLTYPE(XML), '/Transaction/MVTIDENT') AS NUMBER) ||
    '</td><td>' ||
    CAST(EXTRACTVALUE(XMLTYPE(XML), '/Transaction/TYPE') AS NUMBER) ||
    '</td><td><font color=red>' ||
    CAST(E.MESSAGE AS VARCHAR2(500)) ||
    '</font></td></tr>' 
FROM TS_XML_TBL X
    LEFT JOIN TS_ERROR_TBL E ON E.ID = X.ID
WHERE X.SYSTEM = 'BillingGenerator'
    AND X.ISDONE = 1
    AND X.ID BETWEEN BL.GET_OUTPUT_VALUE('BeginGXMLId') AND BL.GET_OUTPUT_VALUE('EndGXMLId')
ORDER BY X.ID;

SELECT '</table><br><b>Compte rendu</b>' FROM DUAL;
SELECT '<table border=1><tr><th>Action</th><th>Count</th></tr>' FROM DUAL;

SELECT '<tr><td>' ||
    CAST(EXTRACTVALUE(XMLTYPE(XML), '/Transaction/ACTION') AS VARCHAR2(10)) ||
    '</td><td>' ||
    COUNT(*) ||
    '</td></tr>'
FROM TS_XML_TBL X
WHERE X.SYSTEM = 'BillingGenerator'
    AND X.ID BETWEEN BL.GET_OUTPUT_VALUE('BeginGXMLId') AND BL.GET_OUTPUT_VALUE('EndGXMLId')
GROUP BY CAST(EXTRACTVALUE(XMLTYPE(XML), '/Transaction/ACTION') AS VARCHAR2(10));

SELECT '</table>' FROM DUAL;

-- Error BG
spool '&3';

SELECT 'LOG_ID;DT;MESSAGE' FROM DUAL;

SELECT L.ID, TO_CHAR(L.DT,'YYYY-MM-DD HH24:MI:SS.FF2') DT,
    CAST(L.MSG AS VARCHAR2(500)) MESSAGE
FROM BL_LOGS L
    JOIN BL_LOGS S ON S.ID = BL.GET_OUTPUT_VALUE('BeginLogId')
    JOIN BL_LOGS E ON E.ID = BL.GET_OUTPUT_VALUE('EndLogId')
WHERE L.SEVERITY = 'ERROR'
    AND L.MSG LIKE 'REPORT.%'
    AND L.ID BETWEEN S.ID AND E.ID 
    AND L.LOGGER = S.LOGGER
ORDER BY L.ID;

-- Error GXML
spool '&4';

SELECT 'GXML_ID;ACTION;REFCON;MVTIDENT;TYPE;MESSAGE' FROM DUAL;

SELECT X.ID,
    CAST(EXTRACTVALUE(XMLTYPE(XML), '/Transaction/ACTION') AS VARCHAR2(10)) ACTION,
    CAST(CASE EXTRACTVALUE(XMLTYPE(XML), '/Transaction/ACTION')
    WHEN 'UPDATE' then EXTRACTVALUE(XMLTYPE(XML), '/Transaction/REFCON')
    WHEN 'CANCEL' then EXTRACTVALUE(XMLTYPE(XML), '/Transaction/REFCON')
    ELSE NULL END AS NUMBER) REFCON,
    CAST(EXTRACTVALUE(XMLTYPE(XML), '/Transaction/MVTIDENT') AS NUMBER) MVTIDENT,
    CAST(EXTRACTVALUE(XMLTYPE(XML), '/Transaction/TYPE') AS NUMBER) TYPE,
    CAST(E.MESSAGE AS VARCHAR2(500)) MESSAGE
FROM TS_XML_TBL X
    LEFT JOIN TS_ERROR_TBL E ON E.ID = X.ID
WHERE X.SYSTEM = 'BillingGenerator'
    AND X.ISDONE = 1
    AND X.ID BETWEEN BL.GET_OUTPUT_VALUE('BeginGXMLId') AND BL.GET_OUTPUT_VALUE('EndGXMLId')
ORDER BY X.ID;

-- List of all GXML data with correspondant refcon
spool '&5';

SELECT 'GXML_ID;ACTION;ERROR;REFCON;MVTIDENT;TYPE;ENTITE;CONTREPARTIE;DEVISE' FROM DUAL;

SELECT X.ID, CAST(EXTRACTVALUE(XMLTYPE(XML), '/Transaction/ACTION') AS VARCHAR2(10)) ACTION,
    CASE WHEN X.ISDONE = 1 THEN 'ERROR' ELSE NULL END ERROR,
    CASE WHEN EXTRACTVALUE(XMLTYPE(XML), '/Transaction/ACTION') IN ('UPDATE', 'CANCEL') THEN
        CAST(EXTRACTVALUE(XMLTYPE(XML), '/Transaction/REFCON') AS NUMBER)
    ELSE H.REFCON END REFCON,
    H.MVTIDENT, H.TYPE,
    TE.NAME ENTITE, TC.NAME CONTREPARTIE, OP.FACT_CUR DEVISE
FROM TS_XML_TBL X
    LEFT JOIN HISTOMVTS H ON ((EXTRACTVALUE(XMLTYPE(XML), '/Transaction/ACTION') IN ('CREATE')
        --AND H.MVTIDENT = EXTRACTVALUE(XMLTYPE(XML), '/Transaction/MVTIDENT')
        --AND DECODE(H.TYPE, 701, 101, 700, 7, H.TYPE) = EXTRACTVALUE(XMLTYPE(XML), '/Transaction/TYPE')
        --AND H.DATENEG = TO_DATE(EXTRACTVALUE(XMLTYPE(XML), '/Transaction/DATENEG'), 'YYYY-MM-DD')
        AND H.INFOSBACKOFFICE = EXTRACTVALUE(XMLTYPE(XML), '/Transaction/ID'))
        OR (EXTRACTVALUE(XMLTYPE(XML), '/Transaction/ACTION') IN ('UPDATE', 'CANCEL')
        AND EXTRACTVALUE(XMLTYPE(XML), '/Transaction/REFCON') = H.REFCON))
    LEFT JOIN TIERS TE ON TE.IDENT = H.ENTITE
    LEFT JOIN TIERS TC ON TC.IDENT = H.CONTREPARTIE
    LEFT JOIN CMA_RPT_OPERATIONS OP ON OP.MVTIDENT = H.MVTIDENT AND OP.TA_REFCON = H.REFCON
WHERE X.SYSTEM = 'BillingGenerator'
    AND X.ID BETWEEN BL.GET_OUTPUT_VALUE('BeginGXMLId') AND BL.GET_OUTPUT_VALUE('EndGXMLId')
ORDER BY X.ID;

spool '&6';

SELECT 'ENTITE;CONTREPARTIE;DEVISEPAY;INTERNE;PERIODE_FACTU;MVTIDENT;REFCON;TYPE;ACTION;MONTANT_TICKET;MONTANT_BG;ECART;STATUT_TICKET;PAIEMENT;PROVENANCE;ALLOTEMENT;SECTION;SICOVAM;DATENEG_MEP;DATVAL_THEO_MEP;DATVAL_REELLE_MEP;DATSET_THEO_MEP;DATSET_REELLE_MEP;DATENEG_CLOSING;DATVAL_THEO_CLOSING;DATVAL_REELLE_CLOSING;DATSET_THEO_CLOSING;DATSET_REELLE_CLOSING' FROM DUAL;

SELECT  T.ALL_COLUMN FROM (
SELECT DISTINCT TE.NAME ENTITE, TC.NAME CONTREPARTIE,
    DEVISE_TO_STR(
        NVL2(H.REFCON, H.DEVISEPAY,
            TO_NUMBER(SUBSTR(L.MSG, INSTR(L.MSG,'devisepay=') + LENGTH('devisepay='),
                INSTR(L.MSG, ';', INSTR(L.MSG,'devisepay=')) - (INSTR(L.MSG,'devisepay=') + LENGTH('devisepay=')))))) DEVISEPAY,
    TO_CHAR(TO_DATE('&1', 'YYYYMMDD'), 'MM/YYYY') PERIOD_FACTU, NVL2(H.REFCON, H.MVTIDENT, HO.MVTIDENT) MVTIDENT,
    NVL(TE.NAME, 0)  || ';' || NVL(TC.NAME, 0) || ';' ||
    DEVISE_TO_STR(
        NVL2(H.REFCON, H.DEVISEPAY,
            TO_NUMBER(SUBSTR(L.MSG, INSTR(L.MSG,'devisepay=') + LENGTH('devisepay='),
                INSTR(L.MSG, ';', INSTR(L.MSG,'devisepay=')) - (INSTR(L.MSG,'devisepay=') + LENGTH('devisepay=')))))) || ';' ||
    CASE IS_CPTY_INTERNAL(NVL2(H.REFCON, H.CONTREPARTIE, HO.CONTREPARTIE), NVL2(H.REFCON, H.ENTITE, HO.ENTITE))
        WHEN 1 THEN 'YES' ELSE 'NO'END || ';' ||
    TO_CHAR(TO_DATE('&1', 'YYYYMMDD'), 'MM/YYYY') || ';' ||
    NVL2(H.REFCON, H.MVTIDENT, HO.MVTIDENT) || ';' ||
    H.REFCON || ';' ||
    BE.NAME || ';' ||
    CASE SUBSTR(L.MSG, INSTR(L.MSG,'provenance=') + LENGTH('provenance='), 1) WHEN 'C' THEN 'CREATION'
        WHEN 'M' THEN 'MODIFICATION' ELSE 'SUPPRESSION' END || ';' ||
    H.MONTANT || ';' ||
    CASE SUBSTR(L.MSG, INSTR(L.MSG,'provenance=') + LENGTH('provenance='), 1) WHEN 'S' THEN 0
        ELSE TO_NUMBER(SUBSTR(L.MSG, INSTR(L.MSG,'montant=') + LENGTH('montant='),
            INSTR(L.MSG, ';', INSTR(L.MSG,'montant=')) - (INSTR(L.MSG,'montant=') + LENGTH('montant='))),
            '999999999D999999', 'NLS_NUMERIC_CHARACTERS = '',.''') END || ';' ||
    (CASE SUBSTR(L.MSG, INSTR(L.MSG,'provenance=') + LENGTH('provenance='), 1) WHEN 'S' THEN 0
        ELSE TO_NUMBER(SUBSTR(L.MSG, INSTR(L.MSG,'montant=') + LENGTH('montant='),
            INSTR(L.MSG, ';', INSTR(L.MSG,'montant=')) - (INSTR(L.MSG,'montant=') + LENGTH('montant='))),
            '999999999D999999', 'NLS_NUMERIC_CHARACTERS = '',.''') END - H.MONTANT) || ';' ||
    BKS.NAME || ';' ||
    BES.NAME || ';' ||
    NVL2(H.REFCON, NVL(H.ECN, RU.NAME), 'BG') || ';' ||
    NVL(AF.LIBELLE, ' ') || ';' ||
    NVL(S.SECTION, ' ') || ';' ||
    NVL(T.SICOVAM, 0) || ';' ||
    decode ((NVL(to_char(HP.DATENEG, 'DD/MM/YYYY'), ' ')), '01/01/1904', ' ',NVL((to_char(HP.DATENEG, 'DD/MM/YYYY')), ' ')) || ';' ||
    decode ((NVL(to_char(HP.DATEVAL, 'DD/MM/YYYY'), ' ')), '01/01/1904', ' ',NVL((to_char(HP.DATEVAL, 'DD/MM/YYYY')), ' ')) || ';' ||
    decode ((NVL(to_char(HP.REAL_DATEVAL, 'DD/MM/YYYY'), ' ')), '01/01/1904', ' ',NVL((to_char(HP.REAL_DATEVAL, 'DD/MM/YYYY')), ' ')) || ';' ||
    decode ((NVL(to_char(HP.DELIVERY_DATE, 'DD/MM/YYYY'), ' ')), '01/01/1904', ' ',NVL((to_char(HP.REAL_DATEVAL, 'DD/MM/YYYY')), ' ')) || ';' ||
    decode ((NVL(to_char(HP.REAL_DELIVERY_DATE, 'DD/MM/YYYY'), ' ')), '01/01/1904', ' ',NVL((to_char(HP.REAL_DELIVERY_DATE, 'DD/MM/YYYY')), ' ')) || ';' ||
    decode ((NVL(to_char(HC.DATENEG, 'DD/MM/YYYY'), ' ')), '01/01/1904', ' ',NVL((to_char(HC.DATENEG, 'DD/MM/YYYY')), ' ')) || ';' ||
    decode ((NVL(to_char(HC.DATEVAL, 'DD/MM/YYYY'), ' ')), '01/01/1904', ' ',NVL((to_char(HC.DATEVAL, 'DD/MM/YYYY')), ' ')) || ';' ||
    decode ((NVL(to_char(HC.REAL_DATEVAL, 'DD/MM/YYYY'), ' ')), '01/01/1904', ' ',NVL((to_char(HC.REAL_DATEVAL, 'DD/MM/YYYY')), ' ')) || ';' ||
    decode ((NVL(to_char(HC.DELIVERY_DATE, 'DD/MM/YYYY'), ' ')), '01/01/1904', ' ',NVL((to_char(HC.DELIVERY_DATE, 'DD/MM/YYYY')), ' ')) || ';' ||
    decode ((NVL(to_char(HC.REAL_DELIVERY_DATE, 'DD/MM/YYYY'), ' ')), '01/01/1904', ' ',NVL((to_char(HC.REAL_DELIVERY_DATE, 'DD/MM/YYYY')), ' '))
    ALL_COLUMN
FROM BL_LOGS L
    LEFT JOIN HISTOMVTS H ON SUBSTR(L.MSG, INSTR(L.MSG,'provenance=') + LENGTH('provenance='), 1) IN ('M', 'S')
        AND H.REFCON = TO_NUMBER(SUBSTR(L.MSG, INSTR(L.MSG,'refcon=') + LENGTH('refcon='),
            INSTR(L.MSG, ';', INSTR(L.MSG,'refcon=')) - (INSTR(L.MSG,'refcon=') + LENGTH('refcon='))))
    LEFT JOIN HISTOMVTS HO ON H.REFCON IS NULL
        AND HO.REFCON = TO_NUMBER(SUBSTR(L.MSG, INSTR(L.MSG,'refcon=') + LENGTH('refcon='),
            INSTR(L.MSG, ';', INSTR(L.MSG,'refcon=')) - (INSTR(L.MSG,'refcon=') + LENGTH('refcon='))))
    JOIN BUSINESS_EVENTS BE ON (H.REFCON IS NOT NULL AND BE.ID = H.TYPE)
        OR (H.REFCON IS NULL AND BE.ID =
            TO_NUMBER(SUBSTR(L.MSG, INSTR(L.MSG,'type=') + LENGTH('type='),
                INSTR(L.MSG, ';', INSTR(L.MSG,'type=')) - (INSTR(L.MSG,'type=') + LENGTH('type=')))))
    JOIN TIERS TE ON TE.IDENT = NVL2(H.REFCON, H.ENTITE, HO.ENTITE) 
    JOIN TIERS TC ON TC.IDENT= NVL2(H.REFCON, H.CONTREPARTIE, HO.CONTREPARTIE)
    LEFT JOIN BO_KERNEL_STATUS BKS ON BKS.ID = H.BACKOFFICE
    LEFT JOIN AUDIT_MVT AM ON H.REFCON = AM.REFCON AND AM.VERSION = 1 
    LEFT JOIN RISKUSERS RU ON RU.IDENT = AM.USERID
    JOIN TITRES T ON T.SICOVAM = NVL2(H.REFCON, H.SICOVAM, HO.SICOVAM)
    JOIN AFFECTATION AF ON AF.IDENT = T.AFFECTATION
    JOIN NATIXIS_FOLIO_SECTION_ENTITE S ON S.IDENT = NVL2(H.REFCON, H.OPCVM, HO.OPCVM)
    LEFT JOIN HISTOMVTS HP ON HP.MVTIDENT = NVL2(H.REFCON, H.MVTIDENT, HO.MVTIDENT) AND HP.TYPE IN (1, 500)
    LEFT JOIN HISTOMVTS HC ON HC.MVTIDENT = NVL2(H.REFCON, H.MVTIDENT, HO.MVTIDENT) AND HC.TYPE IN (102, 501)
    LEFT JOIN BO_MESSAGES BM ON BM.TRADE_ID = H.REFCON
    LEFT JOIN BO_EXTERNAL_STATUS BES ON BES.ID = BM.STATUS
    JOIN BL_LOGS LS ON LS.ID = BL.GET_OUTPUT_VALUE('BeginLogId')
WHERE L.ID BETWEEN BL.GET_OUTPUT_VALUE('BeginLogId') AND BL.GET_OUTPUT_VALUE('EndLogId')
    AND L.LOGGER = LS.LOGGER 
--WHERE L.DT BETWEEN TRUNC(SYSDATE) AND TRUNC(SYSDATE) + 12/24
    AND L.MSG LIKE 'SIMULATION.%'
    AND (BM.IDENT IS NULL OR BM.IDENT = (SELECT MAX(IDENT) FROM BO_MESSAGES WHERE TRADE_ID = H.REFCON))
) T ORDER BY ENTITE, CONTREPARTIE, DEVISEPAY, PERIOD_FACTU, MVTIDENT;

spool off;

/
EXIT;