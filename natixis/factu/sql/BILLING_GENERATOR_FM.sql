BEGIN
    -- BG sur 3 mois glissants
    -- Mois M - 2
    BL.BILLING_GENERATOR(
        p_source => BL.SRC_FM, -- source FM
        p_date => add_months(trunc(to_date('&1', 'YYYYMMDD'), 'MM'), -2),
        p_periodicite => 2 -- Mensuel
    );
    -- Mois M - 1
    BL.BILLING_GENERATOR(
        p_source => BL.SRC_FM, -- source FM
        p_date => add_months(trunc(to_date('&1', 'YYYYMMDD'), 'MM'), -1),
        p_periodicite => 2 -- Mensuel
    );
    -- Mois M
    BL.BILLING_GENERATOR(
        p_source => BL.SRC_FM, -- source FM
        p_date => add_months(trunc(to_date('&1', 'YYYYMMDD'), 'MM'), 0),
        p_periodicite => 2 -- Mensuel
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

SELECT '<h3>BillingGenerator Mensuel - ' || TO_CHAR(TRUNC(to_date('&1', 'YYYYMMDD')), 'MON YYYY') || ' - ' || TO_CHAR(TRUNC(add_months(to_date('&1', 'YYYYMMDD'), -1)), 'MON YYYY') || ' - ' || TO_CHAR(TRUNC(add_months(to_date('&1', 'YYYYMMDD'), -2)), 'MON YYYY') || '</h3>' FROM DUAL;

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

SELECT 'GXML_ID;ACTION;ERROR;REFCON;MVTIDENT;TYPE;TRADE DATE;ENTITE;CONTREPARTIE;DEVISE' FROM DUAL;

SELECT X.ID, CAST(EXTRACTVALUE(XMLTYPE(XML), '/Transaction/ACTION') AS VARCHAR2(10)) ACTION,
    CASE WHEN X.ISDONE = 1 THEN 'ERROR' ELSE NULL END ERROR,
    CASE WHEN EXTRACTVALUE(XMLTYPE(XML), '/Transaction/ACTION') IN ('UPDATE', 'CANCEL') THEN
        CAST(EXTRACTVALUE(XMLTYPE(XML), '/Transaction/REFCON') AS NUMBER)
    ELSE H.REFCON END REFCON,
    H.MVTIDENT, H.TYPE, H.DATENEG,
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

spool off;

/
EXIT;
