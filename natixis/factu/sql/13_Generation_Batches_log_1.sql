WHENEVER SQLERROR EXIT 99 ROLLBACK;
WHENEVER OSERROR  EXIT 98 ROLLBACK;

SET COLSEP '	'
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

def datefactu='&2';


spool '&3'; 

prompt WHENEVER SQLERROR EXIT 99 ROLLBACK;
prompt WHENEVER OSERROR  EXIT 98 ROLLBACK;


prompt SET trimout ON 
prompt SET FEEDBACK OFF
prompt SET heading OFF 
prompt SET verify OFF
prompt SET space 0 
prompt SET NEWPAGE 0 
prompt SET PAGESIZE 0 
prompt SET trimspool ON

prompt SET WRAP OFF
prompt SET TRIMOUT OFF 
prompt SET COLSEP ';'
prompt SET LINESIZE 2000


prompt
select 'spool &4 append ;' from dual;
select 'select to_char(dt,''YYYY/MM/DD-HH24:MI:ss''), substr(severity,1,10), substr(logger,1,25), substr(info,1,100) from natixis_batches_log where dt >= to_date(''&datefactu'', ''YYYY/MM/DD-HH24:MI:ss'') and logger like ''%FA_%'' order by 1, 4;' from dual ;
prompt
select 'exit ;' from dual;

spool off;

/
exit;
