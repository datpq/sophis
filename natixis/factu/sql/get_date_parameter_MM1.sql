SET HEAD OFF;
SELECT TO_CHAR(ADD_MONTHS(TRUNC(SYSDATE, 'MM'), -1), 'YYYYMMDD') from dual;
exit 0;