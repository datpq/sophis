--{{SOPHIS_SQL (do not delete this line)

alter table HISTORIQUE drop column CFG_THEO_IN_AMOUNT;
alter table HISTORIQUE drop column CFG_THEO_IN_PERCENT;
alter table HISTORIQUE drop column CFG_THEO_IN_PERC_WITH_ACCRUED;
alter table HISTORIQUE drop column CFG_YTM;
alter table HISTORIQUE drop column CFG_ACCRUED_IN_AMOUNT;
alter table HISTORIQUE drop column CFG_ACCRUED_IN_PERCENT;
alter table HISTORIQUE drop column CFG_DURATION;
alter table HISTORIQUE drop column CFG_SENSITIVITY;

delete from NEW_HISTORY_COLUMN where name = 'CFG_THEO_IN_AMOUNT';
delete from NEW_HISTORY_COLUMN where name = 'CFG_THEO_IN_PERCENT';
delete from NEW_HISTORY_COLUMN where name = 'CFG_THEO_IN_PERC_WITH_ACCRUED';
delete from NEW_HISTORY_COLUMN where name = 'CFG_YTM';
delete from NEW_HISTORY_COLUMN where name = 'CFG_ACCRUED_IN_AMOUNT';
delete from NEW_HISTORY_COLUMN where name = 'CFG_ACCRUED_IN_PERCENT';
delete from NEW_HISTORY_COLUMN where name = 'CFG_DURATION';
delete from NEW_HISTORY_COLUMN where name = 'CFG_SENSITIVITY';


DECLARE
BEGIN
  LAST_SPOTS_PKG.INSTALL('HISTORIQUE');  
END;
/

delete from extrnl_references_instruments where ref_ident in (select ref_ident from extrnl_references_definition where ref_name in ('CFGExternalRef','CFGActionType','CFGIntegrStatus'));
delete from extrnl_references_definition where ref_name in ('CFGExternalRef','CFGActionType','CFGIntegrStatus');

commit;

--}}SOPHIS_SQL