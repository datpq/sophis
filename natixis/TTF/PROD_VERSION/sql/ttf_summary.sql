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

-- TTF Summary
spool '&2';

SELECT '<h3>Calcul de la TTF du mois : ' || TO_CHAR(TRUNC(to_date('&1', 'YYYYMMDD'), 'MM'), 'MON YYYY') || '</h3>' FROM DUAL;

SELECT '<br><b>TTF EDA Compte propre</b>' FROM DUAL;
SELECT '<table border=1><tr><th>Crit\&egrave;re Exon\&eacute;ration</th><th>Base Taxable (Mios \&euro;)</th><th>TTF (\&euro;)</th></tr>' FROM DUAL;

SELECT '<tr><td>' ||
    TG.EXONERATION ||
    '</td><td>' ||
    TRIM(TO_CHAR(SUM(NVL(TA.TAXABLE_AMOUNT, 0))/1000000, '999G999G999G999D9', 'NLS_NUMERIC_CHARACTERS = '', ''')) ||
    '</td><td>' ||
    TRIM(TO_CHAR(SUM(NVL(TA.TTF_AMOUNT, 0)), '999G999G999G999D99', 'NLS_NUMERIC_CHARACTERS = '', ''')) ||
    '</td></tr>'
FROM (SELECT DISTINCT SOUSSECTION, ID_GROUP, TAXABLE_AMOUNT, TTF_AMOUNT
    FROM NATIXIS_TTF_AUDIT
    WHERE TRUNC(DATE_CALCUL, 'MM') = TRUNC(to_date('&1', 'YYYYMMDD'), 'MM')) TA
    JOIN NATIXIS_TTF_GROUP TG ON TG.ID = TA.ID_GROUP
GROUP BY TG.EXONERATION;

SELECT '<tr><td><b>TOTAL</b></td><td><b>' ||
    TRIM(TO_CHAR(SUM(NVL(TA.TAXABLE_AMOUNT, 0))/1000000, '999G999G999G999D9', 'NLS_NUMERIC_CHARACTERS = '', ''')) ||
    '</b></td><td><b>' ||
    TRIM(TO_CHAR(SUM(NVL(TA.TTF_AMOUNT, 0)), '999G999G999G999D99', 'NLS_NUMERIC_CHARACTERS = '', ''')) ||
    '</b></td></tr>'
FROM (SELECT DISTINCT SOUSSECTION, ID_GROUP, TAXABLE_AMOUNT, TTF_AMOUNT
    FROM NATIXIS_TTF_AUDIT
    WHERE TRUNC(DATE_CALCUL, 'MM') = TRUNC(to_date('&1', 'YYYYMMDD'), 'MM')) TA
    JOIN NATIXIS_TTF_GROUP TG ON TG.ID = TA.ID_GROUP
;

SELECT '</table><br>D\&eacute;tail de la TTF par sous section \&eacute;ligible :' FROM DUAL;
SELECT '<table border=1><tr><th>Sous section</th><th>Base Taxable (\&euro;)</th><th>TTF (\&euro;)</th></tr>' FROM DUAL;

SELECT '<tr><td>' ||
    TA.SOUSSECTION ||
    '</td><td>' ||
    TRIM(TO_CHAR(SUM(NVL(TA.TAXABLE_AMOUNT, 0)), '999G999G999G999D99', 'NLS_NUMERIC_CHARACTERS = '', ''')) ||
    '</td><td>' ||
    TRIM(TO_CHAR(SUM(NVL(TA.TTF_AMOUNT, 0)), '999G999G999G999D99', 'NLS_NUMERIC_CHARACTERS = '', ''')) ||
    '</td></tr>'
FROM (SELECT DISTINCT SOUSSECTION, ID_GROUP, TAXABLE_AMOUNT, TTF_AMOUNT
    FROM NATIXIS_TTF_AUDIT
    WHERE TRUNC(DATE_CALCUL, 'MM') = TRUNC(to_date('&1', 'YYYYMMDD'), 'MM')) TA
WHERE TA.SOUSSECTION IS NOT NULL
GROUP BY TA.SOUSSECTION;

SELECT '<tr><td><b>TOTAL</b></td><td><b>' ||
    TRIM(TO_CHAR(SUM(NVL(TA.TAXABLE_AMOUNT, 0)), '999G999G999G999D99', 'NLS_NUMERIC_CHARACTERS = '', ''')) ||
    '</b></td><td><b>' ||
    TRIM(TO_CHAR(SUM(NVL(TA.TTF_AMOUNT, 0)), '999G999G999G999D99', 'NLS_NUMERIC_CHARACTERS = '', ''')) ||
    '</b></td></tr>'
FROM (SELECT DISTINCT SOUSSECTION, ID_GROUP, TAXABLE_AMOUNT, TTF_AMOUNT
    FROM NATIXIS_TTF_AUDIT
    WHERE TRUNC(DATE_CALCUL, 'MM') = TRUNC(to_date('&1', 'YYYYMMDD'), 'MM')) TA
WHERE TA.SOUSSECTION IS NOT NULL
;

SELECT '</table>' FROM DUAL;

SELECT '<br><b>TTF EDA Collect\&eacute;e</b>' FROM DUAL;
SELECT '<table border=1><tr><th>Section</th><th>Base Taxable (Mios \&euro;)</th><th>TTF (\&euro;)</th></tr>' FROM DUAL;

SELECT '<tr><td>' ||
    TF.SECTION ||
    '</td><td>' ||
    TRIM(TO_CHAR(SUM(NVL(TF.TAXABLE_AMOUNT, 0))/1000000, '999G999G999G999D9', 'NLS_NUMERIC_CHARACTERS = '', ''')) ||
    '</td><td>' ||
    TRIM(TO_CHAR(SUM(NVL(TF.TTF_AMOUNT, 0)), '999G999G999G999D99', 'NLS_NUMERIC_CHARACTERS = '', ''')) ||
    '</td></tr>'
FROM NATIXIS_TTFCOLLECTED_AUDIT TF
WHERE TRUNC(TF.DATE_CALCUL, 'MM') = TRUNC(to_date('&1', 'YYYYMMDD'), 'MM')
GROUP BY TF.SECTION;

SELECT '<tr><td><b>TOTAL</b></td><td><b>' ||
    TRIM(TO_CHAR(SUM(NVL(TF.TAXABLE_AMOUNT, 0))/1000000, '999G999G999G999D9', 'NLS_NUMERIC_CHARACTERS = '', ''')) ||
    '</b></td><td><b>' ||
    TRIM(TO_CHAR(SUM(NVL(TF.TTF_AMOUNT, 0)), '999G999G999G999D99', 'NLS_NUMERIC_CHARACTERS = '', ''')) ||
    '</b></td></tr>'
FROM NATIXIS_TTFCOLLECTED_AUDIT TF
WHERE TRUNC(TF.DATE_CALCUL, 'MM') = TRUNC(to_date('&1', 'YYYYMMDD'), 'MM')
;

SELECT '</table>' FROM DUAL;

spool off;

/
EXIT;
