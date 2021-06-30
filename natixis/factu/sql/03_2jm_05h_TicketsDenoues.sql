WHENEVER SQLERROR EXIT 99 ROLLBACK;
WHENEVER OSERROR  EXIT 98 ROLLBACK;

SET AUTOCOMMIT OFF;
SET SERVEROUTPUT ON;
SET FLUSH ON;
SET ECHO ON;
SET FEEDBACK ON;
SET TERMOUT ON;

spool '&3'; 

prompt -------------------------------------------------------------- ;
prompt  Dénouement des tickets avant traitement de facturation        ;
prompt -------------------------------------------------------------- ;

DECLARE
	batchname  varchar(20) := 'FA_TDENOUES_2JM_05H';
	batchcode  varchar(20) := '03_';
	
  datefactu  varchar(20) := '&2';
	format_datefactu  varchar(20) := 'YYYYMMDD';
BEGIN
  dbms_output.put_line('03  -- Debut du traitement de dénouement des tickets ');
  natixis_ta.addLogAutonomous('INFO', batchname, batchcode || '00 : BEGIN '  );

	-- -------------------------------------------------------------------------- 
	-- DEBUT DENOUEMENT DES TICKETS
	-- --------------------------------------------------------------------------
	insert into security_log values(seqlog.nextval, (SELECT ident FROM riskusers WHERE NAME = 'MOE_BO'), sysdate, 'BATCH', 1, 101, 0,0,0);
	
    update HISTOMVTS H
    set H.DELIVERY_DATE = (select EFFECTIVE_DATE from NATIXIS_FACTU_DENOUE where REFCON = H.REFCON and TYPE = 1)
        where exists (select * from NATIXIS_FACTU_DENOUE where REFCON = H.REFCON and TYPE = 1);
  natixis_ta.addLogAutonomous('DEBUG', batchname, batchcode || '01 : ' || SQL%ROWCOUNT || ' rows updated');

    update HISTOMVTS H
    set H.DATEVAL = (select EFFECTIVE_DATE from NATIXIS_FACTU_DENOUE where REFCON = H.REFCON and TYPE = 2)
        where exists (select * from NATIXIS_FACTU_DENOUE where REFCON = H.REFCON and TYPE = 2);
  natixis_ta.addLogAutonomous('DEBUG', batchname, batchcode || '02 : ' || SQL%ROWCOUNT || ' rows updated');

	-- -------------------------------------------------------------------------- 
	-- FIN DENOUEMENT DES TICKETS
	-- --------------------------------------------------------------------------

  dbms_output.put_line('03  -- Fin du traitement de dénouement des tickets ');
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
