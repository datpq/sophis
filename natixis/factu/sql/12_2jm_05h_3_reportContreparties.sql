WHENEVER SQLERROR EXIT 99 ROLLBACK;
WHENEVER OSERROR  EXIT 98 ROLLBACK;
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
SET LINESIZE 5999
spool '&3';
prompt -------------------------------------------------------------- ;
prompt  Traitement du report des contreparties a affecter             ;
prompt -------------------------------------------------------------- ;
prompt -------------------------------------------------------------- ;
prompt  12b  - Debut Traitement du report des contreparties a affecter;
prompt -------------------------------------------------------------- ;
def datefactu='&2';

-- -------------------------------------------------------------------------- 

-- DEBUT EXTRACTION DES REPORTS DE CONTREPARTIES

-- --------------------------------------------------------------------------
  prompt 12b  - Debut Extraction du fichier ReportContreparties.csv       ;
  spool off;

  spool &4;
  
  select 'Entite;Id Contrepartie;Nom contrepartie;Affecté ?;Tiers Externe/Interne;Personne en charge; Nombre de pret/emprunts' from dual;
 select entite||';'||ident||';'||name||';'||indic_affecte||';'||indic_externe||';'||pers_charg||';'||nb_contrats  
  from  
  (  
  select distinct h.entite, c.ident, c.name name,  
  case when c.ident in (select identifiant from NATIXIS_FACTU_REPORT_CTPY) then 'affecte' else 'Non Affecté' end indic_affecte, 
  'Externe' as indic_externe, 
  (select distinct personne from NATIXIS_FACTU_REPORT_CTPY where identifiant = c.ident) pers_charg,  
  count(h.mvtident) nb_contrats  
  from histomvts h, tiers c, tiers g, titres t  
  where h.contrepartie = c.ident and c.mgr = g.ident and h.sicovam = t.sicovam
  and h.type in (7,101) -- Commission, Remun collat  
  and is_cpty_internal(h.contrepartie,h.entite) = 0
  and t.type in ('P','L')
  and h.dateneg = LAST_DAY(to_date('&datefactu', 'YYYYMMDD')-29) -- négo dernier jour du mois de facturation  
  group by c.name,h.entite,c.ident,g.ident  
 UNION
   select distinct h.entite, c.ident, c.name name,  
  case when c.ident in (select identifiant from NATIXIS_FACTU_REPORT_CTPY) then 'affecte' else 'Non Affecté' end indic_affecte, 
  'Interne' as indic_externe, 
  (select distinct personne from NATIXIS_FACTU_REPORT_CTPY where identifiant = c.ident) pers_charg,  
  count(h.mvtident) nb_contrats  
  from histomvts h, tiers c, tiers g, titres t  
  where h.contrepartie = c.ident and c.mgr = g.ident and h.sicovam = t.sicovam
  and h.type in (7,101) -- Commission, Remun collat  
  and is_cpty_internal(h.contrepartie,h.entite) <> 0
  and t.type in ('P','L')
  and h.dateneg = LAST_DAY(to_date('&datefactu', 'YYYYMMDD')-29) -- négo dernier jour du mois de facturation  
  group by c.name,h.entite,c.ident,g.ident  
  );
  
  spool &3 append;
  prompt 12b  - Fin Extraction du fichier ReportContreparties.csv      ;

-- -------------------------------------------------------------------------- 

-- FIN EXTRACTION DES REPORTS DE CONTREPARTIES

-- --------------------------------------------------------------------------
prompt -------------------------------------------------------------- ;
prompt  12b  - Fin du traitement du report des contreparties a affecter;
prompt -------------------------------------------------------------- ;
spool off;
  
/
exit;
