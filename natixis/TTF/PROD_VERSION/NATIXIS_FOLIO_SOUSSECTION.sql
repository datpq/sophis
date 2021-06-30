CREATE OR REPLACE VIEW NATIXIS_FOLIO_SOUSSECTION
(IDENT, MGR, NIVEAU1, ENTITE, SECTION, 
 CLOSING, ID_FOLIO)
AS 
SELECT npf.ident, npf.mgr, npf.niveau1,npf.entite, ab.name as section, ab.closing,
    (select ident from folio 
    where ident in (select id_folio from cdc_section_madonne)
        and rownum = 1
    connect by prior mgr = ident
        start with ident = npf.ident) id_folio
  FROM natixis_primary_folio npf
  inner join account_book_folio abf on npf.niveau1 = abf.folio_id
  inner join account_book ab on ab.ID = abf.account_book_id AND ab.record_type = 1;
  
CREATE OR REPLACE PUBLIC SYNONYM NATIXIS_FOLIO_SOUSSECTION FOR NATIXIS_FOLIO_SOUSSECTION;
