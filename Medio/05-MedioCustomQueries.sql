spool "D:\DatabaseMigrationScript\Log\05-MedioCustomQueries.log"
set define off
set echo on
set line 200
set feedback on
set serveroutput on
set autocommit on


--[F]--
set time on
set timing on

whenever sqlerror continue;

--------------------------------------------------------
--  File created - Monday-May-25-2021   
--------------------------------------------------------

update expression_style set expression=replace(upper(expression),'INITIAL NOMINAL','Notional (bonds)') where upper(EXPRESSION) like '%INITIAL NOMINAL%';
update expression_style set expression=replace(upper(expression),'INITIAL NOTIONAL','Number of securities') where upper(EXPRESSION) like '%INITIAL NOTIONAL%';

update expression_style set expression ='decode([Nominal]=0,0,[Medio Gamma Cash]*100/[Nominal])' where id=(select positionexp from EXPRESSION_COLUMN where columnname='Medio Gamma Cash in %');
update expression_style set expression ='decode([nominal]=0,0,[Medio BBG Delta Cash]*100/[Nominal])' where id=(select positionexp from EXPRESSION_COLUMN where columnname='Medio Delta Cash in %');

update expression_style set expression ='sum[Compliance - Weight in fund (RBC) simple]' where id=(select portfolioexp from EXPRESSION_COLUMN where columnname='Compliance - Weight in fund (RBC) simple');
update expression_style set expression ='decode(Fund[RBC Fund NAV]=0,0,[Allotment]="INT RATE FUTURE", [Last]FOREX(POSITION.ISOCCY, FUND.ISOCCY)[Number of securities]*[Contract size]/Fund[RBC Fund NAV]*100, [Market Value curr. global]/Fund[RBC Fund NAV]*100)' where id=(select positionexp from EXPRESSION_COLUMN where columnname='Compliance - Weight in fund (RBC) simple');

update expression_style set expression=replace(expression,'(Sec.)','(Mod. Port)') where EXPRESSION like '%(Sec.)%';

commit;

spool off;

exit;



