--{{SOPHIS_SQL (do not delete this line)


--v x.x.x.x
INSERT INTO NEW_HISTORY_COLUMN (TAG, NAME) VALUES(str_to_num('T IN AM WITHOUT ACCRUED'),'CFG_THEO_IN_AMOUNT_WITHOUT_ACCRUED');
alter table HISTORIQUE add(CFG_THEO_IN_AMOUNT_WITHOUT_ACCRUED);

DECLARE
BEGIN
  LAST_SPOTS_PKG.INSTALL('HISTORIQUE');  
END;
/


--v 1.0.0.0

INSERT INTO NEW_HISTORY_COLUMN (TAG, NAME) VALUES(str_to_num('T IN AMOUNT'),'CFG_THEO_IN_AMOUNT');
INSERT INTO NEW_HISTORY_COLUMN (TAG, NAME) VALUES(str_to_num('T IN %'),'CFG_THEO_IN_PERCENT');
INSERT INTO NEW_HISTORY_COLUMN (TAG, NAME) VALUES(str_to_num('T IN % WITH ACCRUED'),'CFG_THEO_IN_PERC_WITH_ACCRUED');
INSERT INTO NEW_HISTORY_COLUMN (TAG, NAME) VALUES(str_to_num('YTM'),'CFG_YTM');
INSERT INTO NEW_HISTORY_COLUMN (TAG, NAME) VALUES(str_to_num('ACCRUED_IN_AMOUNT'),'CFG_ACCRUED_IN_AMOUNT');
INSERT INTO NEW_HISTORY_COLUMN (TAG, NAME) VALUES(str_to_num('ACCRUED_IN_PERCENT'),'CFG_ACCRUED_IN_PERCENT');
INSERT INTO NEW_HISTORY_COLUMN (TAG, NAME) VALUES(str_to_num('DURATION'),'CFG_DURATION');
INSERT INTO NEW_HISTORY_COLUMN (TAG, NAME) VALUES(str_to_num('SENSITIVITY'),'CFG_SENSITIVITY');


alter table HISTORIQUE add(CFG_THEO_IN_AMOUNT NUMBER);
alter table HISTORIQUE add(CFG_THEO_IN_PERCENT NUMBER);
alter table HISTORIQUE add(CFG_THEO_IN_PERC_WITH_ACCRUED NUMBER);
alter table HISTORIQUE add(CFG_YTM NUMBER);
alter table HISTORIQUE add(CFG_ACCRUED_IN_AMOUNT NUMBER);
alter table HISTORIQUE add(CFG_ACCRUED_IN_PERCENT NUMBER);
alter table HISTORIQUE add(CFG_DURATION NUMBER);
alter table HISTORIQUE add(CFG_SENSITIVITY NUMBER);


DECLARE
BEGIN
  LAST_SPOTS_PKG.INSTALL('HISTORIQUE');  
END;
/

insert into extrnl_references_definition(ref_ident,ref_name,redundancy) values((select max(ref_ident)+1 from extrnl_references_definition),'CFGExternalRef',1);
insert into extrnl_references_definition(ref_ident,ref_name,redundancy) values((select max(ref_ident)+1 from extrnl_references_definition),'CFGActionType',1);
insert into extrnl_references_definition(ref_ident,ref_name,redundancy) values((select max(ref_ident)+1 from extrnl_references_definition),'CFGIntegrStatus',1);

insert into extrnl_references_instruments(SOPHIS_IDENT,REF_IDENT,VALUE) select T.SICOVAM, D.ref_ident, '1' from TITRES T, extrnl_references_definition D where D.REF_NAME = 'CFGActionType';
insert into extrnl_references_instruments(SOPHIS_IDENT,REF_IDENT,VALUE) select T.SICOVAM, D.ref_ident, 'KO' from TITRES T, extrnl_references_definition D where D.REF_NAME = 'CFGIntegrStatus';

commit;


--}}SOPHIS_SQL