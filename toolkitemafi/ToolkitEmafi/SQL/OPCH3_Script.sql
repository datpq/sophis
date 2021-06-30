/* Script for the first part (VENTILATION_ACTIF) */

--No script for now, it's done in SOPHIS.

/* Script for the second part (SOUSCRIPTION_RACHAT) */

delete from TIERSPROPERTIES b
where b.name in ('Type_Personne','Type_Residence');
delete from TIERSPROPERTIESLIST a 
where a.name in ('Type_Personne','Type_Residence')
;
Insert into TIERSPROPERTIESLIST (NAME,CODE,NUMERO) values ('Type_Personne',null,null);
Insert into TIERSPROPERTIESLIST (NAME,CODE,NUMERO) values ('Type_Residence',null,'1');
commit;

