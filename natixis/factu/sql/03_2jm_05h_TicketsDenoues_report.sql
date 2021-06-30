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

prompt -------------------------------------------------------------- ;
prompt  01  - Debut extraction de rapport MAJ des dateval et delivery date  ;
prompt -------------------------------------------------------------- ;

	-- --------------------------------------------------------------------------------------------------------------------------------------------------------- 
	-- Debut extraction de rapport MAJ des dateval et delivery date
	-- --------------------------------------------------------------------------------------------------------------------------------------------------------
	
    delete from NATIXIS_FACTU_DENOUE;
    
    insert into NATIXIS_FACTU_DENOUE
        select H.REFCON, I.EFFECTIVE_DATE, 1 -- 1 is DELIVERY_DATE, 2 is DATEVAL
        from HISTOMVTS H
        join BO_CASH_INSTRUCTION I
            on I.TRADE_ID = H.REFCON
            and I.STATUS = 118
            and I.VERSION = (select max(II.VERSION) from BO_CASH_INSTRUCTION II where II.ID = I.ID)
            and I.REASON_CODE = 'Full settled'
            and (I.EFFECTIVE_DATE > H.DELIVERY_DATE or H.DELIVERY_DATE is null)
            and I.EFFECTIVE_DATE >= add_months(trunc(to_date('&2', 'YYYYMMDD'), 'MM'), -1)
            and H.DATEVAL <= (trunc(to_date('&2', 'YYYYMMDD'), 'MM') - 1)
            and (H.DELIVERY_DATE <= (trunc(to_date('&2', 'YYYYMMDD'), 'MM') - 1) or H.DELIVERY_DATE is null) 
        join TITRES T
            on T.SICOVAM = H.SICOVAM
            and T.AFFECTATION in (59,60,61,62,63,64,65)
        join NATIXIS_FOLIO_SECTION_ENTITE fse
            on fse.ident = H.OPCVM and fse.section != '29A'
        where H.DEPOSITAIRE not in (10019596, 10002015, 10035955, 10035956)
            --and H.DELIVERY_TYPE = 1
        ;
        
    insert into NATIXIS_FACTU_DENOUE
        select H.REFCON, I.EFFECTIVE_DATE, 2 -- 1 is DELIVERY_DATE, 2 is DATEVAL
        from HISTOMVTS H
        join BO_CASH_INSTRUCTION I
            on I.TRADE_ID = H.REFCON
            and I.STATUS = 118
            and I.VERSION = (select max(II.VERSION) from BO_CASH_INSTRUCTION II where II.ID = I.ID)
            and I.REASON_CODE = 'Full settled'
            and I.EFFECTIVE_DATE > H.DATEVAL
            and I.EFFECTIVE_DATE >= add_months(trunc(to_date('&2', 'YYYYMMDD'), 'MM'), -1)
            and H.DATEVAL <= (trunc(to_date('&2', 'YYYYMMDD'), 'MM') - 1)
        join TITRES T
            on T.SICOVAM = H.SICOVAM
            and T.AFFECTATION in (59,60,61,62,63,64,65)
        join NATIXIS_FOLIO_SECTION_ENTITE fse
            on fse.ident = H.OPCVM and fse.section != '29A'
        where H.DEPOSITAIRE not in (10019596, 10002015, 10035955, 10035956)
            and H.DELIVERY_TYPE = 1
        ;

spool &3;
select  'REFCON;MVTIDENT;NAME;DELIVERY_DATE_AVANT;DELIVERY_DATE_APRES;DATEVAL_AVANT;DATEVAL_APRES' from dual;

    select H.REFCON, H.MVTIDENT, BE.NAME,
        nvl2(dd.REFCON, TO_CHAR(H.DELIVERY_DATE, 'DD/MM/YYYY'), null) DELIVERY_DATE_AVANT,
        nvl2(dd.REFCON, TO_CHAR(dd.EFFECTIVE_DATE, 'DD/MM/YYYY'), null) DELIVERY_DATE_APRES,
        nvl2(dv.REFCON, TO_CHAR(H.DATEVAL, 'DD/MM/YYYY'), null) DATEVAL_AVANT,
        nvl2(dv.REFCON, TO_CHAR(dv.EFFECTIVE_DATE, 'DD/MM/YYYY'), null) DATEVAL_APRES
    from HISTOMVTS H
        join BUSINESS_EVENTS BE on BE.ID = H.TYPE
        left join NATIXIS_FACTU_DENOUE DD on DD.REFCON = H.REFCON and DD.TYPE = 1
        left join NATIXIS_FACTU_DENOUE DV on DV.REFCON = H.REFCON and DV.TYPE = 2
    where DD.REFCON is not null or DV.refcon is not null;
     
	-- --------------------------------------------------------------------------------------------------------------------------------------------------------- 
	-- FIN extraction de rapport MAJ des dateval et delivery date
	-- --------------------------------------------------------------------------------------------------------------------------------------------------------

spool off;

/
exit;
