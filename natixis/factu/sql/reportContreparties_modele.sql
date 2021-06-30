WHENEVER SQLERROR EXIT 99 ROLLBACK;
WHENEVER OSERROR  EXIT 98 ROLLBACK;

SET AUTOCOMMIT OFF;
SET SERVEROUTPUT ON;
SET FLUSH ON;
SET ECHO ON;
SET FEEDBACK ON;
SET TERMOUT ON;

spool '&3';

prompt --------------------------------------------------------------------------------------------------------------------------;
prompt  Insertion des contreparties dans NATIXIS_FACTU_REPORT_CTPY ;
prompt -------------------------------------------------------------- -----------------------------------------------------------;

DECLARE
	batchname  varchar(20) := 'FA_RPT_CTPY_2JM_05H2';
	batchcode  varchar(20) := '01_';
	
	datefactu  varchar(20) := '&2';
	format_datefactu  varchar(20) := 'YYYYMMDD';

BEGIN
  dbms_output.put_line('01  -- Debut du traitement d insertion des contreparties ');
  natixis_ta.addLogAutonomous('INFO', batchname, batchcode || '00 : BEGIN '  );

	-- -------------------------------------------------------------------------- 
	-- VIDAGE DE LA TABLE
	-- --------------------------------------------------------------------------
  delete from NATIXIS_FACTU_REPORT_CTPY;
  natixis_ta.addLogAutonomous('DEBUG', batchname, batchcode || '01 : ' || SQL%ROWCOUNT || ' rows deleted');

	-- -------------------------------------------------------------------------- 
	-- DEBUT EXTRACTION DES REPORTS DE CONTREPARTIES
	-- --------------------------------------------------------------------------

  ~~~~LISTE_INSERT_CONTREPARTIES~~~~

  natixis_ta.addLogAutonomous('DEBUG', batchname, batchcode || '02 : ' || SQL%ROWCOUNT || ' rows inserted');
	-- -------------------------------------------------------------------------- 
	-- FIN EXTRACTION DES REPORTS DE CONTREPARTIES
	-- --------------------------------------------------------------------------

  dbms_output.put_line('02  -- Fin du traitement d insertion des contreparties ');
  natixis_ta.addLogAutonomous('INFO', batchname, batchcode || '99 : END ' ); 
  
EXCEPTION 
	WHEN OTHERS THEN 
	begin
	  natixis_ta.addLogAutonomous('ERR', batchname, batchcode || '99: ROLLBACK :'|| SQLCODE ||':'||SQLERRM);
	  rollback;
	  raise;
	end;
END;
/
exit
