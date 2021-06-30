WHENEVER SQLERROR EXIT 99 ROLLBACK
WHENEVER OSERROR  EXIT 98 ROLLBACK
SET trimout ON
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

spool D:\Factu\logs\Factu_batches_log.txt append ;
select to_char(dt,'YYYY/MM/DD-HH24:MI:ss'), substr(severity,1,10), substr(logger,1,25), substr(info,1,100) from natixis_batches_log where dt >= to_date('2013/06/04-06:45:12', 'YYYY/MM/DD-HH24:MI:ss') and logger like '%FA_%' order by 1, 4;

exit ;
