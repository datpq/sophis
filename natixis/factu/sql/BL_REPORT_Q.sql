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

spool '&1';

SELECT 'MVTIDENT' FROM DUAL;
SELECT SUBSTR(L.MSG, INSTR(L.MSG, 'mvtident = ') + 11) MVTIDENT
FROM BL_LOGS L
    JOIN BL_LOGS S ON S.ID = BL.RPT_GET_ID_START()
    JOIN BL_LOGS E ON E.ID = BL.RPT_GET_ID_END()
WHERE L.ID BETWEEN S.ID AND E.ID
    AND L.LOGGER = S.LOGGER
    AND L.MSG LIKE 'Processing%'
ORDER BY L.ID;

spool '&2';

SELECT 'LOG_ID;DT;MESSAGE' FROM DUAL;

SELECT L.ID, TO_CHAR(L.DT,'YYYY-MM-DD HH24:MI:SS.FF2') DT,
    CAST(L.MSG AS VARCHAR2(500)) MESSAGE
FROM BL_LOGS L
    JOIN BL_LOGS S ON S.ID = BL.RPT_GET_ID_START()
    JOIN BL_LOGS E ON E.ID = BL.RPT_GET_ID_END()
WHERE L.SEVERITY = 'ERROR'
    AND L.MSG LIKE 'REPORT.%'
    AND L.ID BETWEEN S.ID AND E.ID 
    AND L.LOGGER = S.LOGGER
ORDER BY L.ID;

spool '&3';

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
    AND X.DATEINSERT BETWEEN    BL.RPT_GET_DATE_START()
                            AND BL.RPT_GET_DATE_END()
ORDER BY X.ID;

spool '&4';

SELECT '<h3>Billing Generator Quotidien</h3>' FROM DUAL;

SELECT 'Date Situation = <b>' ||
    CAST(SUBSTR(S.MSG, INSTR(S.MSG, 'date=') + 5,
    INSTR(S.MSG, ',', INSTR(S.MSG, 'date=')) - INSTR(S.MSG, 'date=') - 5) AS VARCHAR2(20)) ||
    '</b>. Number of contracts = <b>' ||
    SUBSTR(SP.MSG, INSTR(SP.MSG, '/') + 1, INSTR(SP.MSG, ' ', INSTR(SP.MSG, '/')) - INSTR(SP.MSG, '/') - 1) ||
    '</b>'
FROM BL_LOGS SP
    JOIN BL_LOGS S ON S.ID = BL.RPT_GET_ID_START()
    JOIN BL_LOGS E ON E.ID = BL.RPT_GET_ID_END()
WHERE SP.ID BETWEEN S.ID AND E.ID
    AND SP.LOGGER = S.LOGGER
    AND SP.MSG LIKE 'Processing contract 1/%';

SELECT '<br><br><b>Message BG</b>' FROM DUAL;
SELECT '<table border=1><tr><th>Id</th><th>DateTime</th><th>Message</th></tr>' FROM DUAL;

SELECT '<tr><td>' ||
    L.ID ||
    '</td><td>' ||
    TO_CHAR(L.DT,'YYYY-MM-DD HH24:MI:SS.FF2') ||
    '</td><td><font color=red>' ||    
    CAST(L.MSG AS VARCHAR2(500)) ||
    '</font></td></tr>' 
FROM BL_LOGS L
    JOIN BL_LOGS S ON S.ID = BL.RPT_GET_ID_START()
    JOIN BL_LOGS E ON E.ID = BL.RPT_GET_ID_END()
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
    AND X.DATEINSERT BETWEEN    BL.RPT_GET_DATE_START()
                            AND BL.RPT_GET_DATE_END()
ORDER BY X.ID;

spool off;

EXIT;
