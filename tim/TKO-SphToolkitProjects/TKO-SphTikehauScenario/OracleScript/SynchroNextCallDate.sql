DROP VIEW "TKO_SYNCHRO_NEXTCALLDATE_BBG"

CREATE OR REPLACE FORCE VIEW "TKO_SYNCHRO_NEXTCALLDATE_BBG" ("SICOVAM", "REFERENCE", "NXT_CALL_DT_SOPHIS", "NXT_CALL_DT_BBG") AS 
	SELECT	T.sicovam, 
			T.reference,  
			to_char(least(min(decode(sign(DATEDEB-trunc(sysdate)),-1,DATEFIN,DATEDEB)), min(DATEFIN)),'YYYY/MM/DD') NXT_CALL_DT_SOPHIS, 
			to_char(num_to_date(h.TKO_NXT_CALL_DT), 'YYYY/MM/DD') NXT_CALL_DT_BBG 
FROM   TITRES T
LEFT JOIN clause C ON T.sicovam = C.sicovam and C.TYPE = 2 and (C.DATEDEB >= trunc(sysdate) OR C.DATEFIN >= trunc(sysdate) ) and C.com1 like 'AMERICAN'
JOIN historique h on h.sicovam = T.sicovam and h.jour = trunc(sysdate -1)
WHERE  EXISTS  
(
		SELECT 1 FROM histomvts h WHERE h.type IN 
        (SELECT ID FROM BUSINESS_EVENTS S WHERE compta = 1 ) AND h.sicovam = t.sicovam 
        AND H.opcvm IN (SELECT ident FROM folio START WITH ident in ('16641') CONNECT BY mgr = PRIOR ident)--Choix de Folio (Exp OPEN-ENDED STRATEGIES)
        GROUP BY h.mvtident HAVING SUM(h.QUANTITE)>0
  )
  GROUP BY T.sicovam, T.reference,h.TKO_NXT_CALL_DT 
  HAVING  least(min(decode(sign(DATEDEB-trunc(sysdate)),-1,DATEFIN,DATEDEB)), min(DATEFIN)) != num_to_date(h.TKO_NXT_CALL_DT);