CREATE OR REPLACE PACKAGE TTF
AS
    /******************************************************************************
    PACKAGE TTF (Taxe sur les Transaction Financière)
    
    20/08/2012 - @DPH: First creation of package
    15/10/2012 - @DPH: V102 error: end-of-file on communication channel
                       Do not use Decode(, NULL, )
    16/10/2012 - @DPH: V201 TTFCollecte
    08/11/2012 - @YO: V202 TAUX_DR_DC modification on TTFPropre and TTFCollecte
    03/01/2013 - @DPH: V203 TTF PE
    13/03/2013 - @DPH: V300 TTF Italia (not active). TTF FR corrected.
    28/05/2013 - @DPH: V301 TTF Italia
    24/06/2013 - @DPH: V302 TTF Simulation
    ******************************************************************************/
    
    PROCEDURE MAIN(p_date in date default trunc(sysdate), p_country in varchar2 default null, p_simu in int default 0);
    
    FUNCTION IS_CTPY_INTRAGROUP_NATIXIS(p_ctpy_id TIERS.IDENT%TYPE, p_entity_id TIERS.IDENT%TYPE) RETURN NUMBER;
    
END TTF;
/

CREATE OR REPLACE PACKAGE BODY TTF
AS
    TYPE MAP_DATE IS TABLE OF DATE INDEX BY VARCHAR2(2);
    countryDate         MAP_DATE;
    
    GXML_SYSTEM         constant TS_XML_TBL.SYSTEM%TYPE := 'TTF';
    GXML_EVENT_CA       constant BO_KERNEL_EVENTS.NAME%TYPE := 'New deal accept';
    GXML_EVENT_CP       constant BO_KERNEL_EVENTS.NAME%TYPE := 'New deal pending';
    GXML_ACTION_CREATE  constant varchar2(6)    := 'CREATE';
    GXML_FORMAT_DATE    constant varchar2(10)   := 'YYYY-MM-DD';
    GXML_FORMAT_NUM     constant varchar2(20)   := '9999999999.99';
    GXML_ECN            constant HISTOMVTS.ECN%TYPE := 'TTF';
    
    -- global parameters
    GP_DATE             date;
    GP_COUNTRY          varchar2(2);
    GP_SIMU             int;
    
    IT_PAYDATE          int;
    
    PROCEDURE GXML_CREATE_UPDATE(
        p_id            in  varchar2,
        HH              in  HISTOMVTS%ROWTYPE)
    AS
        gxml_id_str         TS_XML_TBL.ID%TYPE;
        result              XMLType;
        --xml_cours           varchar2(20);
        xml_montant         varchar2(20);
    BEGIN
        BL.INFO('GXML_CREATE_UPDATE({1})', p_id);
        
        --xml_cours := TRIM(TO_CHAR(round(HH.COURS, 2), GXML_FORMAT_NUM));
        xml_montant := TRIM(TO_CHAR(round(HH.MONTANT, 2), GXML_FORMAT_NUM));
        
        SELECT XMLRoot(XMLElement("Transaction",
                XMLAttributes(  'http://www.w3.org/2001/XMLSchema-instance' AS "xmlns:xsi",
                                'http://www.w3.org/2001/XMLSchema' AS "xmlns:xsd"),
            XMLForest(
                p_id                ID,
                GXML_EVENT_CA       "WorkflowEvent",
                GXML_ACTION_CREATE  "ACTION",
                NULL                "REFCON",
                HH.OPCVM            OPCVM,
                HH.SICOVAM          SICOVAM,
                TO_CHAR(HH.DATENEG, GXML_FORMAT_DATE) DATENEG,
                TO_CHAR(HH.DATEVAL, GXML_FORMAT_DATE) DATEVAL,
                HH.TYPE             TYPE,
                NULL                QUANTITE, -- QUANTITE and COURS are determined by Sophis 
                NULL                COURS, -- QUANTITE and COURS are determined by Sophis
                xml_montant         MONTANT,
                NULL                COUPON,
                HH.INFOS            INFOS,
                NULL                MVTIDENT,
                NULL                HEURENEG,
                NULL                COURTIER,
                NULL                DEPOSITAIRE,
                NULL                AJUSTEMENT,
                NULL                TYPESICO,
                HH.CONTREPARTIE     CONTREPARTIE,
                HH.OPERATEUR        OPERATEUR,
                --BO_CONTROLLED  BACKOFFICE, -- determined by Sophis
                NULL                INFOSBACKOFFICE,
                NULL                FRAISMARCHE,
                NULL                FRAISCOURTAGE,
                NULL                TRANSBACK,
                NULL                REFERENCE,
                NULL                BRUT_NET,
                NULL                CMPT_ORDRE,
                NULL                MONTANTCOURU, -- sera renseigné pour LOT2
                NULL                TYPECOURS,
                NULL                DATECOUPON,
                NULL                DATEJOUISSANCE,
                NULL                MONTANTCOURU2, -- sera renseigné pour LOT2
                HH.DEVISEPAY        DEVISEPAY,
                1                   TAUXCHANGE,
                NULL                DATECOMPTABLE,
                NULL                REFMVTBACK,
                NULL                TAXTRANSACTION,
                NULL                TAXIMPOT,
                NULL                CONTREPARTIE2,
                HH.ENTITE           ENTITE,
                NULL                CERTAIN,
                2                   CREATION,
                NULL                COURSDEVPAY,
                NULL                CDC_ENVOI,
                NULL                REFGRAPPAGE,
                NULL                ARCHIVE,
                NULL                WORKFLOW_ID,
                NULL                DELIVERY_TYPE,
                NVL((SELECT NAME FROM BO_CASH_WORKFLOW WHERE ID = HH.WORKFLOW_ID), 'N/A') || ' / ' ||
                        DECODE(HH.DELIVERY_TYPE, -1, 'All', 1, 'DVP', 2, 'FOP', 'N/A') SMDT,
                NULL                FRAISCOUNTERPARTY,
                NULL                FIXING_TYPE,
                NULL                COMMISSION,
                NULL                COMMISSION_DATE,
                NULL                BACK2BACK,
                NULL                LOOKLIKE,
                HH.ECN              ECN,
                NULL                TRADEID_ECN, -- id venant du GXML
                NULL                TRADEVERSIONID_ECN,
                (SELECT NVL(LIBELLE,'SANS') FROM PAYMENTMETHOD WHERE IDENT = HH.PAYMENT_METHOD) PAYMENT_METHOD,
                NULL                DELIVERY_DATE,
                NULL                NXS_CUSTOMER,
                NULL                FORCELOAD,
                NULL                SALES,
                NULL                PRODUCT_AXIS,
                NULL                CLIENT_AXIS,
                NULL                RC,
                NULL                CX,
                NULL                SC,
                NULL                RFQ_ID,
                NULL                RFQ_TRADEID,
                --1                   CHECKBROKERFEES, --hard code in GXML
                --1                   CHECKMARKETFEES, --hard code in GXML
                --1                   CHECKCOUNTERFEES, --hard code in GXML
                NULL                ENTRY_DATE,
                NULL                CASH_DEPOSITARY,
                NULL                TEMPLATE_ID,
                --HH.DEPOSITARY_OF_THE_COUNTERPART DEPOSITARY_OF_THE_COUNTERPART, -- determined by Sophis
                NULL                MIRROR_REFERENCE,
                --1                   VERSION, -- determined by Sophis
                NULL                NXS_COMMENT,
                NULL                EVG_DURATION,
                NULL                NXS_PREPAID,
                NULL                REAL_DATEVAL,
                NULL                REAL_DELIVERY_DATE)),
            VERSION '1.0')
        into result FROM DUAL;
            
        gxml_id_str := BL.GET_NEXT_GXML_ID(GXML_SYSTEM);
        INSERT INTO TS_XML_TBL(ID, SYSTEM, XML, ISDONE, DATEINSERT)
        VALUES (gxml_id_str, GXML_SYSTEM, result.getClobVal(), 0, sysdate);
                    
        BL.INFO('{1} was inserted into GXML. rowcount = {2}', gxml_id_str, SQL%ROWCOUNT);
    EXCEPTION
        WHEN OTHERS THEN
            BL.ERROR('GXML_CREATE_UPDATE(p_id={3}). code = {1}, message = {2}', SQLCODE, SQLERRM, p_id);
            RAISE;
    END;
    
    PROCEDURE SEND_TO_GXML_COMPTE_TIERS
    AS
        cursor curImports is
        SELECT TF.REFCON, TF.DATE_CALCUL, TF.TTF_COUNTRY, TF.TTF_AMOUNT, H.OPCVM, H.ENTITE
        FROM NATIXIS_TTFCOLLECTED_AUDIT TF
            JOIN HISTOMVTS H ON H.REFCON = TF.REFCON
        WHERE TF.STATUS_IMPORT = 1;
        
        HH                  HISTOMVTS%ROWTYPE;
        p_refcon            HISTOMVTS.REFCON%TYPE;
        p_id                HISTOMVTS.INFOSBACKOFFICE%TYPE;
        suffix_unique       varchar2(20);
        TYPE IMPORT_LIST IS TABLE OF curImports%ROWTYPE;
        il                  IMPORT_LIST := IMPORT_LIST();
    BEGIN
        BL.INFO('SEND_TO_GXML_COMPTE_TIERS.BEGIN');
        
        HH.SICOVAM := 81609253; -- Sicovam unique de TTF
        HH.TYPE := 741; -- Tax Collected
        HH.QUANTITE := 0;
        HH.COURS := 0;
        HH.OPERATEUR := 29589; -- APP_PROD_TTF
        HH.DEVISEPAY := STR_TO_DEVISE('EUR');
        HH.CONTREPARTIE := 10002015; -- Sicovam CACEIS
        HH.WORKFLOW_ID := -1;
        HH.DELIVERY_TYPE := 3;
        HH.ECN := GXML_ECN;
        HH.PAYMENT_METHOD := 104; -- SANS
        
        suffix_unique := to_char(sysdate, 'YYYYMMDDHH24MISS');
        
        for rec in curImports
        loop
            HH.DATENEG := rec.DATE_CALCUL;
            if rec.TTF_COUNTRY = 'FR' then
                -- get the last working day of the month
                HH.DATEVAL := BL.GET_FIRST_WORKING_DAY(add_months(trunc(rec.DATE_CALCUL, 'MM'), 1) - 1, STR_TO_DEVISE('EUR'), -1);
            elsif rec.TTF_COUNTRY = 'IT' then
                HH.DATEVAL := trunc(rec.DATE_CALCUL, 'MM') - 1; --last day of previous month
                -- get the 5th working day of the month
                for i in 1..IT_PAYDATE loop
                    HH.DATEVAL := BL.GET_FIRST_WORKING_DAY(HH.DATEVAL + 1, STR_TO_DEVISE('EUR'));--next working day
                end loop;
            end if;
            HH.MONTANT := rec.TTF_AMOUNT;
            HH.OPCVM := rec.OPCVM;
            HH.ENTITE := rec.ENTITE;
            
            il.extend;
            il(il.count).TTF_COUNTRY := rec.TTF_COUNTRY;
            il(il.count).REFCON := rec.REFCON;
            p_id := GXML_SYSTEM || '_' || rec.TTF_COUNTRY || '_' || rec.REFCON || '_' || suffix_unique;
            
            GXML_CREATE_UPDATE(p_id, HH);
            COMMIT;
            
        end loop;
        
        BL.WAIT_FOR_GXML(GXML_SYSTEM);
        
        for i in 1..il.count
        loop
            p_id := GXML_SYSTEM || '_' || il(i).TTF_COUNTRY || '_' || il(i).REFCON || '_' || suffix_unique;
            begin
                SELECT REFCON
                    into p_refcon
                FROM HISTOMVTS WHERE TYPE = HH.TYPE AND SICOVAM = HH.SICOVAM AND INFOSBACKOFFICE = p_id;
            exception
                when no_data_found then
                    p_refcon := null;
                when too_many_rows then
                    BL.ERROR('get refcon error(code = {1}, message = {2}, INFOSBACKOFFICE={3})', SQLCODE, SQLERRM, p_id);
                    p_refcon := null;
            end;
            BL.INFO('p_id={1}, p_refcon={2}', p_id, p_refcon);
            if p_refcon is not null then
                UPDATE NATIXIS_TTFCOLLECTED_AUDIT SET STATUS_IMPORT = 2, ID_TTF_SOPHIS = p_refcon
                WHERE STATUS_IMPORT = 1 AND REFCON = il(i).REFCON;
            else
                UPDATE NATIXIS_TTFCOLLECTED_AUDIT SET STATUS_IMPORT = 3
                WHERE STATUS_IMPORT = 1 AND REFCON = il(i).REFCON;
            end if;
        end loop;
        
        COMMIT;
        
        BL.INFO('SEND_TO_GXML_COMPTE_TIERS.END');
    exception
        when others then
            BL.ERROR('SEND_TO_GXML_COMPTE_TIERS error(code = {1}, message = {2})', SQLCODE, SQLERRM);
            raise;
    END;

    PROCEDURE SEND_TO_GXML_COMPTE_PROPRE
    AS
        cursor curImports is
        SELECT DATE_CALCUL, TTF_COUNTRY, SOUSSECTION, SUM(TTF_AMOUNT) TTF_AMOUNT FROM
            (SELECT DISTINCT TA.DATE_CALCUL, TA.SOUSSECTION, TA.ID_GROUP, TG.TTF_COUNTRY, TA.TTF_AMOUNT
            FROM NATIXIS_TTF_AUDIT TA
                JOIN NATIXIS_TTF_GROUP TG ON TG.ID = TA.ID_GROUP
            WHERE TA.STATUS_IMPORT = 1)
        GROUP BY DATE_CALCUL, TTF_COUNTRY, SOUSSECTION;
        
        HH                  HISTOMVTS%ROWTYPE;
        p_refcon            HISTOMVTS.REFCON%TYPE;
        p_id                HISTOMVTS.INFOSBACKOFFICE%TYPE;
        suffix_unique       varchar2(20);
        TYPE IMPORT_LIST IS TABLE OF curImports%ROWTYPE;
        il                  IMPORT_LIST := IMPORT_LIST();
        --f_ident             CDC_SECTION_MADONNE.ID_FOLIO%TYPE;
    BEGIN
        BL.INFO('SEND_TO_GXML_COMPTE_PROPRE.BEGIN');
        
        HH.SICOVAM := 81609253; -- Sicovam unique de TTF
        HH.TYPE := 740; -- Tax
        HH.QUANTITE := 0;
        HH.COURS := 0;
        HH.OPERATEUR := 29589; -- APP_PROD_TTF
        HH.DEVISEPAY := STR_TO_DEVISE('EUR');
        HH.WORKFLOW_ID := -1;
        HH.DELIVERY_TYPE := 3;
        HH.ECN := GXML_ECN;
        HH.PAYMENT_METHOD := 104; -- SANS
        
        suffix_unique := to_char(sysdate, 'YYYYMMDDHH24MISS');
        
        for rec in curImports
        loop
            BL.INFO('DATE_CALCUL={1}, TTF_COUNTRY={2}, SOUSSECTION={3}', rec.DATE_CALCUL, rec.TTF_COUNTRY, rec.SOUSSECTION);
            
            begin
                -- retrieve min(folio) of sous section
                SELECT IDENT, ENTITE
                    into HH.OPCVM, HH.ENTITE
                FROM NATIXIS_FOLIO_SECTION_ENTITE
                WHERE IDENT = (SELECT MIN(CSM.ID_FOLIO) FROM CDC_SECTION_MADONNE CSM
                                WHERE CSM.ID_SECTION = rec.SOUSSECTION); 
                /* Here we retrieve the folio of Section
                -- get folio and entity. All folio of sous-section has the same section, so we get the first one here.
                SELECT CSM.ID_FOLIO
                    into f_ident
                FROM CDC_SECTION_MADONNE CSM
                WHERE CSM.ID_SECTION = rec.SOUSSECTION AND ROWNUM = 1;
                
                SELECT IDENT, ENTITE
                    into HH.OPCVM, HH.ENTITE
                FROM (
                SELECT NPF.IDENT, NPF.ENTITE, LEVEL LVL
                FROM NATIXIS_PRIMARY_FOLIO NPF
                    JOIN ACCOUNT_BOOK_FOLIO ABF ON NPF.NIVEAU1 = ABF.FOLIO_ID
                    JOIN ACCOUNT_BOOK AB ON AB.ID = ABF.ACCOUNT_BOOK_ID AND AB.RECORD_TYPE = 1
                CONNECT BY PRIOR MGR = IDENT
                START WITH IDENT = f_ident
                ORDER BY LEVEL DESC) WHERE ROWNUM = 1;*/
            exception
                when others then
                    BL.ERROR('Error when retrieve folio, entite(code = {1}, message = {2}, SOUSSECTION={3})', SQLCODE, SQLERRM, rec.SOUSSECTION);
                    HH.OPCVM := NULL;
                    HH.ENTITE := NULL;
            end;
            
            HH.CONTREPARTIE := CASE rec.TTF_COUNTRY WHEN 'FR' THEN 10002015 WHEN 'IT' THEN 10025235 END; -- FR: Sicovam CACEIS, IT: NATIXIS MILAN
            HH.DATENEG := rec.DATE_CALCUL;
            if rec.TTF_COUNTRY = 'FR' then
                -- get the last working day of the month
                HH.DATEVAL := BL.GET_FIRST_WORKING_DAY(add_months(trunc(rec.DATE_CALCUL, 'MM'), 1) - 1, STR_TO_DEVISE('EUR'), -1);
            elsif rec.TTF_COUNTRY = 'IT' then
                HH.DATEVAL := trunc(rec.DATE_CALCUL, 'MM') - 1; --last day of previous month
                -- get the 5th working day of the month
                for i in 1..IT_PAYDATE loop
                    HH.DATEVAL := BL.GET_FIRST_WORKING_DAY(HH.DATEVAL + 1, STR_TO_DEVISE('EUR'));--next working day
                end loop;
            end if;
            HH.MONTANT := rec.TTF_AMOUNT;
        
            il.extend;
            il(il.count).DATE_CALCUL := rec.DATE_CALCUL;
            il(il.count).TTF_COUNTRY := rec.TTF_COUNTRY;
            il(il.count).SOUSSECTION := rec.SOUSSECTION;
            p_id := GXML_SYSTEM || '_' || rec.TTF_COUNTRY || '_' || rec.SOUSSECTION || '_' || suffix_unique;
            
            GXML_CREATE_UPDATE(p_id, HH);
            COMMIT;
            
        end loop;
        
        BL.WAIT_FOR_GXML(GXML_SYSTEM);
        
        for i in 1..il.count
        loop
            p_id := GXML_SYSTEM || '_' || il(i).TTF_COUNTRY || '_' || il(i).SOUSSECTION || '_' || suffix_unique;
            begin
                SELECT REFCON
                    into p_refcon
                FROM HISTOMVTS WHERE TYPE = HH.TYPE AND SICOVAM = HH.SICOVAM AND INFOSBACKOFFICE = p_id;
            exception
                when no_data_found then
                    p_refcon := null;
                when too_many_rows then
                    BL.ERROR('get refcon error(code = {1}, message = {2}, INFOSBACKOFFICE={3})', SQLCODE, SQLERRM, p_id);
                    p_refcon := null;
            end;
            BL.INFO('p_id={1}, p_refcon={2}', p_id, p_refcon);
            if p_refcon is not null then
                UPDATE NATIXIS_TTF_AUDIT SET STATUS_IMPORT = 2, ID_TTF_SOPHIS = p_refcon
                WHERE STATUS_IMPORT = 1 AND DATE_CALCUL = il(i).DATE_CALCUL AND SOUSSECTION = il(i).SOUSSECTION
                    AND il(i).TTF_COUNTRY = (SELECT TTF_COUNTRY FROM NATIXIS_TTF_GROUP WHERE ID = ID_GROUP); 
            else
                UPDATE NATIXIS_TTF_AUDIT SET STATUS_IMPORT = 3
                WHERE STATUS_IMPORT = 1 AND DATE_CALCUL = il(i).DATE_CALCUL AND SOUSSECTION = il(i).SOUSSECTION
                    AND il(i).TTF_COUNTRY = (SELECT TTF_COUNTRY FROM NATIXIS_TTF_GROUP WHERE ID = ID_GROUP);
            end if;
        end loop;
        
        COMMIT;
        
        BL.INFO('SEND_TO_GXML_COMPTE_PROPRE.END');
    exception
        when others then
            BL.ERROR('SEND_TO_GXML_COMPTE_PROPRE error(code = {1}, message = {2})', SQLCODE, SQLERRM);
            raise;
    END;
    
    PROCEDURE COMPTE_PROPRE
    AS
        -- Pour l'optimization, on utilise ROLLUP dans cette requête pour lister tous les refcon du groupe.
        -- car cette liste de refcon va être insérée dans la table audit.
        -- pareil pour id_section, rollup pour lister tous les sous sections du groupe
        cursor curDealsFR is
        SELECT TT1.MNEMO_V2 ISIN, CP.FAMILLE PRODUCT_FAMILY, H.DATENEG DATE_NEG, H.DATEVAL DATE_VAL,
            DECODE(CP.FAMILLE, 3, 6, NVL(SS.EXOTTF, 0)) EXONERATION, ERI.VALUE TTF_COUNTRY, SUM(H.QUANTITE) NB_TITRE,
            SUM((CASE WHEN H.QUANTITE > 0 THEN H.QUANTITE ELSE 0 END) * H.COURS *                
                DECODE(TT1.DEVISECTT, STR_TO_DEVISE('EUR'), 1,
                    BS_LIB.EXCHANGE_RATE(TT1.DEVISECTT, STR_TO_DEVISE('EUR'), TO_CHAR(H.DATENEG, 'YYYYMMDD'), FSE.SECTION))) / -- TAUX_DC_EUR
                    DECODE(SUM(CASE WHEN H.QUANTITE > 0 THEN H.QUANTITE ELSE 0 END), 0, 1,
                           SUM(CASE WHEN H.QUANTITE > 0 THEN H.QUANTITE ELSE 0 END)) PX_MOYEN_ACHAT,
            SUM((CASE WHEN H.QUANTITE > 0 THEN H.QUANTITE ELSE 0 END) *
                TO_NUMBER(RATE_FR.VALUE, '999G999G999D999999', 'NLS_NUMERIC_CHARACTERS = '',.''')) /
                DECODE(SUM(CASE WHEN H.QUANTITE > 0 THEN H.QUANTITE ELSE 0 END), 0, 1,
                    SUM(CASE WHEN H.QUANTITE > 0 THEN H.QUANTITE ELSE 0 END)) TTF_RATE_MOYEN,
            SS.ID_SECTION, H.REFCON, GROUPING(SS.ID_SECTION) AS G_SOUSSECTION, GROUPING(H.REFCON) AS G_REFCON
        FROM HISTOMVTS H
            JOIN TITRES T ON T.SICOVAM = H.SICOVAM AND T.TYPE != 'L'
            JOIN CDC_BO_CODE_PRODUIT CP ON CP.AFFECTATION = T.AFFECTATION AND CP.REF_IDENT IS NULL
                AND CP.FAMILLE IN (1, 3) -- 1 (ferme) ou 3 (Cessions temporaire)
            LEFT JOIN HISTOMVTS HREPO ON HREPO.REFCON = CASE WHEN CP.FAMILLE = 3 AND T.TYPE = 'L' THEN H.REFERENCE ELSE NULL END 
                LEFT JOIN TITRES TREPO ON TREPO.SICOVAM = HREPO.SICOVAM
            JOIN TITRES TT1 ON TT1.SICOVAM = CASE WHEN CP.FAMILLE = 1 THEN T.SICOVAM ELSE
                CASE WHEN T.TYPE = 'L' THEN TREPO.CODE_EMET ELSE T.CODE_EMET END END
            JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = TT1.SICOVAM
                AND ERI.REF_IDENT = (SELECT REF_IDENT FROM EXTRNL_REFERENCES_DEFINITION WHERE REF_NAME = 'TTF Eligible')
                AND ERI.VALUE IN ('FR') -- [UNDERLYING_ID] porte une external references 'FR' 
            JOIN NATIXIS_FOLIO_SOUSSECTION FSE ON FSE.IDENT = H.OPCVM AND FSE.SECTION != '799' -- not in section simulation
            JOIN CDC_SECTION_MADONNE SS ON SS.ID_FOLIO = FSE.ID_FOLIO
            LEFT JOIN NATIXIS_GROUP_PARAM RATE_FR ON RATE_FR.TYPE = 'TTF' AND RATE_FR.KEY = 'TTFrate'
        WHERE H.DATEVAL BETWEEN TRUNC(GP_DATE, 'MM') - (SELECT TO_NUMBER(VALUE) FROM NATIXIS_GROUP_PARAM WHERE TYPE='TTF' AND KEY='BackValueLag') AND TRUNC(GP_DATE)
            AND H.DATENEG >= TO_DATE('20120801', 'YYYYMMDD') AND H.DATEVAL >= TO_DATE('20120801', 'YYYYMMDD') -- date d'entrée en vigueur de la TTF FR
            AND INSTR(',' || (SELECT VALUE FROM NATIXIS_GROUP_PARAM WHERE TYPE = 'TTF' AND KEY = 'BE selected' AND ENABLED = 1) || ',', ',' || H.TYPE || ',') > 0
            AND (H.REFCON IS NULL OR H.REFCON NOT IN (SELECT REFCON FROM NATIXIS_TTF_AUDIT)) -- IMPORTANT: use IS NULL to avoid the slowness (may be caused by ROLLUP)
            AND IS_CTPY_INTRAGROUP_NATIXIS(H.CONTREPARTIE, H.ENTITE) = 0
            AND H.BACKOFFICE IN (
                SELECT KSC.KERNEL_STATUS_ID FROM BO_KERNEL_STATUS_COMPONENT KSC
                    JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                    AND KSG.RECORD_TYPE = 1 AND KSG.NAME = 'All But Pending FO')
            --AND H.REFCON IN (219121048, 219413216, 219592954, 219926596, 220010326, 220010646, 220148653, 220733875, 220735408)
        GROUP BY TT1.MNEMO_V2, CP.FAMILLE, H.DATENEG, H.DATEVAL,
            DECODE(CP.FAMILLE, 3, 6, NVL(SS.EXOTTF, 0)), ERI.VALUE, ROLLUP (SS.ID_SECTION, H.REFCON);
        
        cursor curDealsIT is
        SELECT TT1.MNEMO_V2 ISIN, CP.FAMILLE PRODUCT_FAMILY, H.DATENEG DATE_NEG, H.DATEVAL DATE_VAL,
            DECODE(CP.FAMILLE, 3, 6, NVL(
                CASE WHEN MARKETMAKERFROM.VALUE IS NULL THEN 0
                     WHEN INSTR(',' || MARKETMAKINGDESK_IT.VALUE || ',', ',' || FSE.SECTION || ',') > 0
                        AND TO_DATE(MARKETMAKERFROM.VALUE, 'DD/MM/YYYY') <= H.DATENEG THEN 3
                     ELSE 0 END, 0)) EXONERATION, ERI.VALUE TTF_COUNTRY, SUM(H.QUANTITE) NB_TITRE,
            SUM((CASE WHEN H.QUANTITE > 0 THEN H.QUANTITE ELSE 0 END) * H.COURS *                
                DECODE(TT1.DEVISECTT, STR_TO_DEVISE('EUR'), 1,
                    BS_LIB.EXCHANGE_RATE(TT1.DEVISECTT, STR_TO_DEVISE('EUR'), TO_CHAR(H.DATENEG, 'YYYYMMDD'), FSE.SECTION))) / -- TAUX_DC_EUR
                    DECODE(SUM(CASE WHEN H.QUANTITE > 0 THEN H.QUANTITE ELSE 0 END), 0, 1,
                           SUM(CASE WHEN H.QUANTITE > 0 THEN H.QUANTITE ELSE 0 END)) PX_MOYEN_ACHAT,
            SUM((CASE WHEN H.QUANTITE > 0 THEN H.QUANTITE ELSE 0 END) *
                CASE WHEN ESTCOURTIER(TC.OPTIONS) = 1 OR ISEXCHANGE(TC.OPTIONS) = 1
                    THEN
                        TO_NUMBER(RATE_IT_REDUIT.VALUE, '999G999G999D999999', 'NLS_NUMERIC_CHARACTERS = '',.''')
                    ELSE
                        TO_NUMBER(RATE_IT.VALUE, '999G999G999D999999', 'NLS_NUMERIC_CHARACTERS = '',.''')
                    END) /
                DECODE(SUM(CASE WHEN H.QUANTITE > 0 THEN H.QUANTITE ELSE 0 END), 0, 1,
                    SUM(CASE WHEN H.QUANTITE > 0 THEN H.QUANTITE ELSE 0 END)) TTF_RATE_MOYEN,
            SS.ID_SECTION, H.REFCON, GROUPING(SS.ID_SECTION) AS G_SOUSSECTION, GROUPING(H.REFCON) AS G_REFCON
        FROM HISTOMVTS H
            JOIN TIERS TC ON TC.IDENT = H.CONTREPARTIE
            JOIN TITRES T ON T.SICOVAM = H.SICOVAM AND T.TYPE != 'L'
            JOIN CDC_BO_CODE_PRODUIT CP ON CP.AFFECTATION = T.AFFECTATION AND CP.REF_IDENT IS NULL
                AND CP.FAMILLE IN (1, 3) -- 1 (ferme) ou 3 (Cessions temporaire)
            LEFT JOIN HISTOMVTS HREPO ON HREPO.REFCON = CASE WHEN CP.FAMILLE = 3 AND T.TYPE = 'L' THEN H.REFERENCE ELSE NULL END 
                LEFT JOIN TITRES TREPO ON TREPO.SICOVAM = HREPO.SICOVAM
            JOIN TITRES TT1 ON TT1.SICOVAM = CASE WHEN CP.FAMILLE = 1 THEN T.SICOVAM ELSE
                CASE WHEN T.TYPE = 'L' THEN TREPO.CODE_EMET ELSE T.CODE_EMET END END
            JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = TT1.SICOVAM
                AND ERI.REF_IDENT = (SELECT REF_IDENT FROM EXTRNL_REFERENCES_DEFINITION WHERE REF_NAME = 'TTF Eligible')
                AND ERI.VALUE IN ('IT') -- [UNDERLYING_ID] porte une external references 'IT' 
            JOIN NATIXIS_FOLIO_SOUSSECTION FSE ON FSE.IDENT = H.OPCVM AND FSE.SECTION != '799' -- not in section simulation
            JOIN CDC_SECTION_MADONNE SS ON SS.ID_FOLIO = FSE.ID_FOLIO
            LEFT JOIN EXTRNL_REFERENCES_DEFINITION ERD ON ERD.REF_NAME = 'MarketMakerFrom'
            LEFT JOIN EXTRNL_REFERENCES_INSTRUMENTS MARKETMAKERFROM ON ERI.VALUE = 'IT'
                AND MARKETMAKERFROM.SOPHIS_IDENT = TT1.SICOVAM
                AND MARKETMAKERFROM.REF_IDENT = ERD.REF_IDENT
            LEFT JOIN NATIXIS_GROUP_PARAM RATE_IT ON RATE_IT.TYPE = 'TTF' AND RATE_IT.KEY = 'TTFrate_IT'
            LEFT JOIN NATIXIS_GROUP_PARAM RATE_IT_REDUIT ON RATE_IT_REDUIT.TYPE = 'TTF' AND RATE_IT_REDUIT.KEY = 'TTFrate_IT_reduit'
            LEFT JOIN NATIXIS_GROUP_PARAM MARKETMAKINGDESK_IT ON MARKETMAKINGDESK_IT.TYPE = 'TTF' AND MARKETMAKINGDESK_IT.KEY = 'MarketMakingDesk_IT'
        WHERE H.DATEVAL BETWEEN ADD_MONTHS(TRUNC(GP_DATE, 'MM'), -1) - (SELECT TO_NUMBER(VALUE) FROM NATIXIS_GROUP_PARAM WHERE TYPE='TTF' AND KEY='BackValueLag') AND (TRUNC(GP_DATE, 'MM') - 1)
            AND H.DATENEG >= TO_DATE('20130301', 'YYYYMMDD') AND H.DATEVAL >= TO_DATE('20130301', 'YYYYMMDD') -- date d'entrée en vigueur de la TTF IT
            AND INSTR(',' || (SELECT VALUE FROM NATIXIS_GROUP_PARAM WHERE TYPE = 'TTF' AND KEY = 'BE selected' AND ENABLED = 1) || ',', ',' || H.TYPE || ',') > 0
            AND (H.REFCON IS NULL OR H.REFCON NOT IN (SELECT REFCON FROM NATIXIS_TTF_AUDIT)) -- IMPORTANT: use IS NULL to avoid the slowness (may be caused by ROLLUP)
            AND IS_CTPY_INTRAGROUP_NATIXIS(H.CONTREPARTIE, H.ENTITE) = 0
            AND H.BACKOFFICE IN (
                SELECT KSC.KERNEL_STATUS_ID FROM BO_KERNEL_STATUS_COMPONENT KSC
                    JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                    AND KSG.RECORD_TYPE = 1 AND KSG.NAME = 'All But Pending FO')
            --AND H.REFCON IN (219121048, 219413216, 219592954, 219926596, 220010326, 220010646, 220148653, 220733875, 220735408)
        GROUP BY TT1.MNEMO_V2, CP.FAMILLE, H.DATENEG, H.DATEVAL,
            DECODE(CP.FAMILLE, 3, 6, NVL(
                CASE WHEN MARKETMAKERFROM.VALUE IS NULL THEN 0
                     WHEN INSTR(',' || MARKETMAKINGDESK_IT.VALUE || ',', ',' || FSE.SECTION || ',') > 0
                        AND TO_DATE(MARKETMAKERFROM.VALUE, 'DD/MM/YYYY') <= H.DATENEG THEN 3
                     ELSE 0 END, 0)), ERI.VALUE, ROLLUP (SS.ID_SECTION, H.REFCON);

        TYPE DEAL_LIST IS TABLE OF curDealsFR%ROWTYPE;
        gl                      DEAL_LIST := DEAL_LIST(); -- tous les deals du groupe
        TYPE REFCON_LIST IS TABLE OF HISTOMVTS.REFCON%TYPE; -- tous les refcon du sous section
        sl                      REFCON_LIST := REFCON_LIST();
        
        id_group                NATIXIS_TTF_GROUP.ID%TYPE;
        BaseTaxable             HISTOMVTS.MONTANT%TYPE;
        BaseTaxableSection      HISTOMVTS.MONTANT%TYPE;
        MontantTTF              HISTOMVTS.MONTANT%TYPE;
        MontantTTFSection       HISTOMVTS.MONTANT%TYPE;
        PNA_SectionPositives    HISTOMVTS.MONTANT%TYPE := 0;
        rc                      number;
        rec                     curDealsFR%ROWTYPE;
    BEGIN
        BL.INFO('COMPTE_PROPRE.BEGIN');
        
        --goto GXML;
        --goto FIN;
        
        --for rec in curDeals('FR', TO_DATE('20120801', 'YYYYMMDD'))
        --for rec in curDeals('IT', TO_DATE('20130301', 'YYYYMMDD'))
        if GP_COUNTRY = 'FR' then
            open curDealsFR;
        elsif GP_COUNTRY = 'IT' then
            open curDealsIT;
        end if;
        BL.INFO('TTF for {1}', GP_COUNTRY);
        loop
            if GP_COUNTRY = 'FR' then
                fetch curDealsFR into rec;
                exit when curDealsFR%NOTFOUND;
            elsif GP_COUNTRY = 'IT' then
                fetch curDealsIT into rec;
                exit when curDealsIT%NOTFOUND;
            end if;
            if rec.G_REFCON = 1 and rec.G_SOUSSECTION = 1 then -- new group found
                BL.DEBUG('Processing group(ISIN={1}, PRODUCT_FAMILY={2}, DATE_NEG={3}, DATE_VAL={4}, EXONERATION={5}, TTF_COUNTRY={6}, PNA={7})',
                    rec.ISIN, rec.PRODUCT_FAMILY, rec.DATE_NEG, rec.DATE_VAL, rec.EXONERATION, rec.TTF_COUNTRY, rec.NB_TITRE);
                begin
                    SELECT ID
                        into id_group
                    FROM NATIXIS_TTF_GROUP G
                    WHERE G.ISIN = rec.ISIN AND G.PRODUCT_FAMILY = rec.PRODUCT_FAMILY
                        AND G.DATE_NEG = rec.DATE_NEG AND G.DATE_VAL = rec.DATE_VAL AND G.EXONERATION = rec.EXONERATION AND G.TTF_COUNTRY = rec.TTF_COUNTRY;
                exception
                    when no_data_found then
                        -- Group not found --> create new
                        SELECT NATIXIS_TTF_GROUP_SEQ.NEXTVAL INTO id_group FROM DUAL;
                        INSERT INTO NATIXIS_TTF_GROUP(ID, ISIN, PRODUCT_FAMILY, DATE_NEG, DATE_VAL, EXONERATION, TTF_COUNTRY)
                            VALUES(id_group, rec.ISIN, rec.PRODUCT_FAMILY, rec.DATE_NEG, rec.DATE_VAL, rec.EXONERATION, rec.TTF_COUNTRY);
                end;
                -- Group found --> update PNA
                UPDATE NATIXIS_TTF_GROUP SET PNA_GROUP = rec.NB_TITRE WHERE ID = id_group;
                if rec.NB_TITRE <= 0 then
                    BL.DEBUG('PNA_GROUP <= 0');
                    rc := 0;
                    for i in 1..gl.count loop
                        if gl(i).G_REFCON = 0 then
                            INSERT INTO NATIXIS_TTF_AUDIT(REFCON, ID_GROUP, QTY_UNIT, ID_TTF_SOPHIS, DATE_CALCUL,
                                SOUSSECTION, PNA_AVG_PRICE, PNA_SECTION, TAXABLE_AMOUNT, TTF_AMOUNT, TTF_RATE, TTF_CUR, STATUS_IMPORT, STATUS_REPORT)
                                VALUES(gl(i).REFCON, id_group, 'UNT', NULL, TRUNC(GP_DATE), NULL, NULL, NULL, 0, 0,
                                    CASE WHEN rec.TTF_COUNTRY = 'FR' THEN
                                        (SELECT TO_NUMBER(RATE_FR.VALUE, '999G999G999D999999', 'NLS_NUMERIC_CHARACTERS = '',.''')
                                        FROM NATIXIS_GROUP_PARAM RATE_FR WHERE RATE_FR.TYPE = 'TTF' AND RATE_FR.KEY = 'TTFrate')
                                    WHEN rec.TTF_COUNTRY = 'IT' THEN
                                        (SELECT 
                                            CASE WHEN ESTCOURTIER(TC.OPTIONS) = 1 OR ISEXCHANGE(TC.OPTIONS) = 1 THEN
                                                TO_NUMBER(RATE_IT_REDUIT.VALUE, '999G999G999D999999', 'NLS_NUMERIC_CHARACTERS = '',.''')
                                            ELSE
                                                TO_NUMBER(RATE_IT.VALUE, '999G999G999D999999', 'NLS_NUMERIC_CHARACTERS = '',.''')
                                            END
                                        FROM HISTOMVTS H
                                            JOIN TIERS TC ON TC.IDENT = H.CONTREPARTIE
                                            LEFT JOIN NATIXIS_GROUP_PARAM RATE_IT ON RATE_IT.TYPE = 'TTF' AND RATE_IT.KEY = 'TTFrate_IT'
                                            LEFT JOIN NATIXIS_GROUP_PARAM RATE_IT_REDUIT ON RATE_IT_REDUIT.TYPE = 'TTF' AND RATE_IT_REDUIT.KEY = 'TTFrate_IT_reduit'
                                        WHERE H.REFCON = gl(i).REFCON)    
                                    END, NULL, 0, 0);
                            rc := rc + 1;
                        end if;
                    end loop;
                    BL.DEBUG('PNA_GROUP <= 0. RowCount = {1}', rc);
                    /* uncoment this block when upgraded to Oracle 11g --> more rapide with FORALL
                    PLS-00436: implementation restriction: cannot reference fields of BULK In-BIND table of records. --> will be OK in Oracle 11g
                    -- remove all sous section group
                    for i in 1..gl.count loop
                        if gl(i).G_REFCON = 1 then
                            gl.delete(i);
                        end if;
                    end loop;
                    -- use forall for optimization, all insert is executed once
                    forall i in indices of gl
                        INSERT INTO NATIXIS_TTF_AUDIT(REFCON, ID_GROUP, QTY_UNIT, ID_TTF_SOPHIS, DATE_CALCUL,
                            SOUSSECTION, PNA_AVG_PRICE, PNA_SECTION, TAXABLE_AMOUNT, TTF_AMOUNT, TTF_CUR, STATUS_IMPORT, STATUS_REPORT)
                            VALUES(gl(i).REFCON, id_group, 'UNT', NULL, TRUNC(p_date), NULL, NULL, NULL, 0, 0, NULL, 0, 0);
                    BL.DEBUG('PNA_GROUP <= 0. RowCount = {1}', SQL%ROWCOUNT);*/
                    gl.delete;
                    gl := DEAL_LIST();
                elsif rec.EXONERATION is null or rec.EXONERATION = 0 then
                    BL.DEBUG('EXONERATION = 0'); -- eligible
                    
                    -- update PNA_AVG_PRICE, TAXABLE AMOUNT, TTF_AMOUNT, TTF_CUR
                    BaseTaxable := round(rec.NB_TITRE * rec.PX_MOYEN_ACHAT, 2);
                    MontantTTF := round(BaseTaxable * rec.TTF_RATE_MOYEN / 100, 2); -- TTFrate for FR, TTFrate_Moyen for IT
                    UPDATE NATIXIS_TTF_GROUP SET
                        PNA_AVG_PRICE = rec.PX_MOYEN_ACHAT,
                        TAXABLE_AMOUNT = BaseTaxable,
                        TTF_AMOUNT = MontantTTF,
                        TTF_CUR = 'EUR'
                    WHERE ID = id_group;
                    
                    for i in 1..gl.count loop
                        if gl(i).G_REFCON = 1 then -- new sous section
                            if gl(i).NB_TITRE > 0 then
                                BaseTaxableSection := round(BaseTaxable * gl(i).NB_TITRE / PNA_SectionPositives, 2);
                                MontantTTFSection := round(MontantTTF * gl(i).NB_TITRE / PNA_SectionPositives, 2); -- TTFrate for FR, TTFrate_Moyen for IT
                            else
                                BaseTaxableSection := 0;
                                MontantTTFSection := 0;
                            end if;
                            
                            for j in 1..sl.count loop
                                --BL.DEBUG('refcon = {1}', sl(j));
                                INSERT INTO NATIXIS_TTF_AUDIT(REFCON, ID_GROUP, QTY_UNIT, ID_TTF_SOPHIS, DATE_CALCUL,
                                    SOUSSECTION, PNA_AVG_PRICE, PNA_SECTION, TAXABLE_AMOUNT, TTF_AMOUNT, TTF_RATE, TTF_CUR, STATUS_IMPORT, STATUS_REPORT)
                                    VALUES (sl(j), id_group, 'UNT', NULL, TRUNC(GP_DATE),
                                        gl(i).ID_SECTION, 0, gl(i).NB_TITRE, BaseTaxableSection, MontantTTFSection,
                                        CASE WHEN rec.TTF_COUNTRY = 'FR' THEN
                                            (SELECT TO_NUMBER(RATE_FR.VALUE, '999G999G999D999999', 'NLS_NUMERIC_CHARACTERS = '',.''')
                                            FROM NATIXIS_GROUP_PARAM RATE_FR WHERE RATE_FR.TYPE = 'TTF' AND RATE_FR.KEY = 'TTFrate')
                                        WHEN rec.TTF_COUNTRY = 'IT' THEN
                                            (SELECT 
                                                CASE WHEN ESTCOURTIER(TC.OPTIONS) = 1 OR ISEXCHANGE(TC.OPTIONS) = 1 THEN
                                                    TO_NUMBER(RATE_IT_REDUIT.VALUE, '999G999G999D999999', 'NLS_NUMERIC_CHARACTERS = '',.''')
                                                ELSE
                                                    TO_NUMBER(RATE_IT.VALUE, '999G999G999D999999', 'NLS_NUMERIC_CHARACTERS = '',.''')
                                                END
                                            FROM HISTOMVTS H
                                                JOIN TIERS TC ON TC.IDENT = H.CONTREPARTIE
                                                LEFT JOIN NATIXIS_GROUP_PARAM RATE_IT ON RATE_IT.TYPE = 'TTF' AND RATE_IT.KEY = 'TTFrate_IT'
                                                LEFT JOIN NATIXIS_GROUP_PARAM RATE_IT_REDUIT ON RATE_IT_REDUIT.TYPE = 'TTF' AND RATE_IT_REDUIT.KEY = 'TTFrate_IT_reduit'
                                            WHERE H.REFCON = sl(j))    
                                        END, 'EUR', CASE WHEN MontantTTFSection = 0 THEN 0 ELSE 1 END, CASE WHEN MontantTTFSection = 0 THEN 0 ELSE 1 END);
                            end loop;
                            BL.DEBUG('EXONERATION = 0. ID_SECTION = {1}, RowCount = {2}', gl(i).ID_SECTION, sl.count);
                            /* uncoment this block when upgraded to Oracle 11g --> more rapide with FORALL 
                            PLS-00436: implementation restriction: cannot reference fields of BULK In-BIND table of records. --> will be OK in Oracle 11g
                            -- use forall for optimization, all insert is executed once
                            forall j in 1..sl.count
                                INSERT INTO NATIXIS_TTF_AUDIT(REFCON, ID_GROUP, QTY_UNIT, ID_TTF_SOPHIS, DATE_CALCUL,
                                    SOUSSECTION, PNA_AVG_PRICE, PNA_SECTION, TAXABLE_AMOUNT, TTF_AMOUNT, TTF_CUR, STATUS_IMPORT, STATUS_REPORT)
                                    VALUES (sl(j).REFCON, id_group, 'UNT', NULL, TRUNC(GP_DATE),
                                        gl(i).ID_SECTION, gl(i).PX_MOYEN_ACHAT, gl(i).NB_TITRE, BaseTaxable, MontantTTF, 'EUR', 1, 1);
                            BL.DEBUG('EXONERATION = 0. ID_SECTION = {1}, RowCount = {2}', gl(i).ID_SECTION, SQL%ROWCOUNT);*/
                            sl.delete;
                            sl := REFCON_LIST();
                        else
                            sl.extend;
                            sl(sl.count) := gl(i).REFCON;
                            --BL.DEBUG('extend sl. count = {1}, refcon = {2}', sl.count, sl(sl.count));
                        end if; 
                    end loop;
                    gl.delete;
                    gl := DEAL_LIST();
                else
                    BL.DEBUG('EXONERATION <> 0'); -- exoneree
                    -- update PNA_AVG_PRICE, TAXABLE AMOUNT, TTF_AMOUNT, TTF_CUR
                    BaseTaxable := round(rec.NB_TITRE * rec.PX_MOYEN_ACHAT, 2);
                    MontantTTF := 0;
                    UPDATE NATIXIS_TTF_GROUP SET
                        PNA_AVG_PRICE = rec.PX_MOYEN_ACHAT,
                        TAXABLE_AMOUNT = BaseTaxable,
                        TTF_AMOUNT = MontantTTF,
                        TTF_CUR = 'EUR'
                    WHERE ID = id_group;
                    rc := 0;
                    for i in 1..gl.count loop
                        if gl(i).G_REFCON = 0 then
                            INSERT INTO NATIXIS_TTF_AUDIT(REFCON, ID_GROUP, QTY_UNIT, ID_TTF_SOPHIS, DATE_CALCUL, SOUSSECTION,
                                PNA_AVG_PRICE, PNA_SECTION, TAXABLE_AMOUNT, TTF_AMOUNT, TTF_RATE, TTF_CUR, STATUS_IMPORT, STATUS_REPORT)
                                VALUES(gl(i).REFCON, id_group, 'UNT', NULL, TRUNC(GP_DATE), NULL,
                                0, NULL, BaseTaxable, 0,
                                CASE WHEN rec.TTF_COUNTRY = 'FR' THEN
                                    (SELECT TO_NUMBER(RATE_FR.VALUE, '999G999G999D999999', 'NLS_NUMERIC_CHARACTERS = '',.''')
                                    FROM NATIXIS_GROUP_PARAM RATE_FR WHERE RATE_FR.TYPE = 'TTF' AND RATE_FR.KEY = 'TTFrate')
                                WHEN rec.TTF_COUNTRY = 'IT' THEN
                                    (SELECT 
                                        CASE WHEN ESTCOURTIER(TC.OPTIONS) = 1 OR ISEXCHANGE(TC.OPTIONS) = 1 THEN
                                            TO_NUMBER(RATE_IT_REDUIT.VALUE, '999G999G999D999999', 'NLS_NUMERIC_CHARACTERS = '',.''')
                                        ELSE
                                            TO_NUMBER(RATE_IT.VALUE, '999G999G999D999999', 'NLS_NUMERIC_CHARACTERS = '',.''')
                                        END
                                    FROM HISTOMVTS H
                                        JOIN TIERS TC ON TC.IDENT = H.CONTREPARTIE
                                        LEFT JOIN NATIXIS_GROUP_PARAM RATE_IT ON RATE_IT.TYPE = 'TTF' AND RATE_IT.KEY = 'TTFrate_IT'
                                        LEFT JOIN NATIXIS_GROUP_PARAM RATE_IT_REDUIT ON RATE_IT_REDUIT.TYPE = 'TTF' AND RATE_IT_REDUIT.KEY = 'TTFrate_IT_reduit'
                                    WHERE H.REFCON = gl(i).REFCON)    
                                END, 'EUR', 0, 1);
                            rc := rc + 1;
                        end if;
                    end loop;
                    BL.DEBUG('EXONERATION <> 0. RowCount = {1}', rc);
                    /* uncoment this block when upgraded to Oracle 11g --> more rapide with FORALL 
                    PLS-00436: implementation restriction: cannot reference fields of BULK In-BIND table of records. --> will be OK in Oracle 11g
                    -- remove all sous section group
                    for i in 1..gl.count loop
                        if gl(i).G_REFCON = 1 then
                            gl.delete(i);
                        end if;
                    end loop;
                    BaseTaxable := round(rec.NB_TITRE * rec.PX_MOYEN_ACHAT, 2);
                    -- use forall for optimization, all insert is executed once
                    forall i in indices of gl
                        INSERT INTO NATIXIS_TTF_AUDIT(REFCON, ID_GROUP, QTY_UNIT, ID_TTF_SOPHIS, DATE_CALCUL,
                            SOUSSECTION, PNA_AVG_PRICE, PNA_SECTION, TAXABLE_AMOUNT, TTF_AMOUNT, TTF_CUR, STATUS_IMPORT, STATUS_REPORT)
                            VALUES(gl(i).REFCON, id_group, 'UNT', NULL, TRUNC(GP_DATE), NULL, rec.PX_MOYEN_ACHAT, NULL, BaseTaxable, 0, 'EUR', 0, 1);
                    BL.DEBUG('EXONERATION <> 0. RowCount = {1}', SQL%ROWCOUNT);*/
                    gl.delete;
                    gl := DEAL_LIST();
                end if;
                
                PNA_SectionPositives := 0; -- reset for the next group
                
                COMMIT;
                BL.DEBUG('Commit group {1}', id_group);
            else
                --if rec.G_REFCON = 0 then
                --    BL.DEBUG('Refcon collected = {1}', rec.REFCON);
                --end if;
                
                -- new sous section found ==> calculate PNA_SectionPostives 
                if rec.G_REFCON = 1 and rec.NB_TITRE > 0 then
                    PNA_SectionPositives := PNA_SectionPositives + rec.NB_TITRE;
                end if;
                
                gl.extend;
                gl(gl.count) := rec;
            end if;
        end loop;
        if GP_COUNTRY = 'FR' then
            close curDealsFR;
        elsif GP_COUNTRY = 'IT' then
            close curDealsIT;
        end if;
        
        --goto FIN;
        
        <<GXML>>
        if GP_SIMU = 0 then -- not simulation
            SEND_TO_GXML_COMPTE_PROPRE;
        end if;
        
        <<FIN>>
        BL.INFO('COMPTE_PROPRE.END');
        
    EXCEPTION
        when others then
            BL.ERROR('COMPTE_PROPRE. code = {1}, message = {2}, backtrace={3}', SQLCODE, SQLERRM, DBMS_UTILITY.FORMAT_ERROR_BACKTRACE);
            BL.INFO('COMPTE_PROPRE.END');
            raise;
    END;
    
    PROCEDURE COMPTE_TIERS
    AS
        rc                      number;
    BEGIN
        BL.INFO('COMPTE_TIERS.BEGIN');
        
        --goto GXML;
        --goto FIN;
        
        BL.INFO('TTF Collected for {1}', GP_COUNTRY);
        if GP_COUNTRY = 'FR' then
        INSERT INTO NATIXIS_TTFCOLLECTED_AUDIT(TTF_COUNTRY, REFCON, QTY_UNIT, ID_TTF_SOPHIS, DATE_CALCUL, FOLIO, SECTION, QT,
            PRICE, ISIN, DATE_NEG, DATE_VAL, TAXABLE_AMOUNT, TTF_AMOUNT, TTF_CUR, STATUS_IMPORT, STATUS_REPORT)
        SELECT ERI.VALUE, H.REFCON, 'UNT', NULL, TRUNC(GP_DATE), H.OPCVM, FSE.SECTION, ABS(H.QUANTITE),
            CASE WHEN H.TAUXCHANGE IS NULL OR H.TAUXCHANGE = 0 THEN 1 WHEN H.CERTAIN=1 THEN H.TAUXCHANGE ELSE 1/H.TAUXCHANGE END * -- PRICE = COURS_CV = TAUX_DR_DC * EXCHANGE_RATE
                DECODE(TT1.DEVISECTT, STR_TO_DEVISE('EUR'), 1,
                    BS_LIB.EXCHANGE_RATE(TT1.DEVISECTT, STR_TO_DEVISE('EUR'), TO_CHAR(H.DATENEG, 'YYYYMMDD'), FSE.SECTION)),
            TT1.MNEMO_V2, H.DATENEG, H.DATEVAL,
            ABS(H.NXS_TAX_VALUE * DECODE(H.TAUXCHANGE, NULL, 1, 0, 1, H.TAUXCHANGE) * -- TAXABLE = Montant TTF / TTF Rate * 100 
                DECODE(TT1.DEVISECTT, STR_TO_DEVISE('EUR'), 1,
                    BS_LIB.EXCHANGE_RATE(TT1.DEVISECTT, STR_TO_DEVISE('EUR'), TO_CHAR(H.DATENEG, 'YYYYMMDD'), FSE.SECTION))) * 100 /
                    TO_NUMBER(RATE_FR.VALUE, '999G999G999D999999', 'NLS_NUMERIC_CHARACTERS = '',.'''),--for IT: BaseTaxable = H.COURS * H.QUANTITE * COURS_CV 
            ABS(H.NXS_TAX_VALUE * DECODE(H.TAUXCHANGE, NULL, 1, 0, 1, H.TAUXCHANGE) * -- TTF = nxs_tax_value * COURS_CV
                DECODE(TT1.DEVISECTT, STR_TO_DEVISE('EUR'), 1,
                    BS_LIB.EXCHANGE_RATE(TT1.DEVISECTT, STR_TO_DEVISE('EUR'), TO_CHAR(H.DATENEG, 'YYYYMMDD'), FSE.SECTION))),
            'EUR', 1, 1
        FROM HISTOMVTS H
            JOIN TITRES T ON T.SICOVAM = H.SICOVAM
            JOIN CDC_BO_CODE_PRODUIT CP ON CP.AFFECTATION = T.AFFECTATION AND CP.REF_IDENT IS NULL
                AND CP.FAMILLE IN (1) -- 1 (ferme)
            LEFT JOIN HISTOMVTS HREPO ON HREPO.REFCON = CASE WHEN CP.FAMILLE = 3 AND T.TYPE = 'L' THEN H.REFERENCE ELSE NULL END 
                LEFT JOIN TITRES TREPO ON TREPO.SICOVAM = HREPO.SICOVAM
            JOIN TITRES TT1 ON TT1.SICOVAM = CASE WHEN CP.FAMILLE = 1 THEN T.SICOVAM ELSE
                CASE WHEN T.TYPE = 'L' THEN TREPO.CODE_EMET ELSE T.CODE_EMET END END
            JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = TT1.SICOVAM
                AND ERI.REF_IDENT = (SELECT REF_IDENT FROM EXTRNL_REFERENCES_DEFINITION WHERE REF_NAME = 'TTF Eligible')
                AND ERI.VALUE IN ('FR') -- [UNDERLYING_ID] porte une external references 'FR', 'IT' 
            JOIN NATIXIS_FOLIO_SECTION_ENTITE FSE ON FSE.IDENT = H.OPCVM AND FSE.SECTION != '799' -- not in section simulation
            LEFT JOIN NATIXIS_GROUP_PARAM RATE_FR ON RATE_FR.TYPE = 'TTF' AND RATE_FR.KEY = 'TTFrate'
        WHERE H.DATEVAL BETWEEN TRUNC(GP_DATE, 'MM') - (SELECT TO_NUMBER(VALUE) FROM NATIXIS_GROUP_PARAM WHERE TYPE='TTF' AND KEY='BackValueLag') AND TRUNC(GP_DATE)
            AND H.DATENEG >= TO_DATE('20120801', 'YYYYMMDD') AND H.DATEVAL >= TO_DATE('20120801', 'YYYYMMDD') -- date d'entrée en vigueur de la TTF FR
            AND INSTR(',' || (SELECT VALUE FROM NATIXIS_GROUP_PARAM WHERE TYPE = 'TTF' AND KEY = 'BE selected' AND ENABLED = 1) || ',', ',' || H.TYPE || ',') > 0
            AND H.REFCON NOT IN (SELECT REFCON FROM NATIXIS_TTFCOLLECTED_AUDIT)
            AND H.QUANTITE < 0 AND H.NXS_TAX_NAME = 'TTF' AND H.NXS_TAX_VALUE IS NOT NULL AND H.NXS_TAX_VALUE != 0
            AND H.BACKOFFICE IN (
                SELECT KSC.KERNEL_STATUS_ID FROM BO_KERNEL_STATUS_COMPONENT KSC
                    JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                    AND KSG.RECORD_TYPE = 1 AND KSG.NAME = 'All But Pending FO');
        elsif GP_COUNTRY = 'IT' then
        INSERT INTO NATIXIS_TTFCOLLECTED_AUDIT(TTF_COUNTRY, REFCON, QTY_UNIT, ID_TTF_SOPHIS, DATE_CALCUL, FOLIO, SECTION, QT,
            PRICE, ISIN, DATE_NEG, DATE_VAL, TAXABLE_AMOUNT, TTF_AMOUNT, TTF_CUR, STATUS_IMPORT, STATUS_REPORT)
        SELECT ERI.VALUE, H.REFCON, 'UNT', NULL, TRUNC(GP_DATE), H.OPCVM, FSE.SECTION, ABS(H.QUANTITE),
            CASE WHEN H.TAUXCHANGE IS NULL OR H.TAUXCHANGE = 0 THEN 1 WHEN H.CERTAIN=1 THEN H.TAUXCHANGE ELSE 1/H.TAUXCHANGE END * -- PRICE = COURS_CV = TAUX_DR_DC * EXCHANGE_RATE
                DECODE(TT1.DEVISECTT, STR_TO_DEVISE('EUR'), 1,
                    BS_LIB.EXCHANGE_RATE(TT1.DEVISECTT, STR_TO_DEVISE('EUR'), TO_CHAR(H.DATENEG, 'YYYYMMDD'), FSE.SECTION)),
            TT1.MNEMO_V2, H.DATENEG, H.DATEVAL,
            H.COURS * H.QUANTITE * DECODE(TT1.DEVISECTT, STR_TO_DEVISE('EUR'), 1,
                BS_LIB.EXCHANGE_RATE(TT1.DEVISECTT, STR_TO_DEVISE('EUR'), TO_CHAR(H.DATENEG, 'YYYYMMDD'), FSE.SECTION)),--for IT: BaseTaxable = H.COURS * H.QUANTITE * COURS_CV 
            ABS(H.NXS_TAX_VALUE * DECODE(H.TAUXCHANGE, NULL, 1, 0, 1, H.TAUXCHANGE) * -- TTF = nxs_tax_value * COURS_CV
                DECODE(TT1.DEVISECTT, STR_TO_DEVISE('EUR'), 1,
                    BS_LIB.EXCHANGE_RATE(TT1.DEVISECTT, STR_TO_DEVISE('EUR'), TO_CHAR(H.DATENEG, 'YYYYMMDD'), FSE.SECTION))),
            'EUR', 1, 1
        FROM HISTOMVTS H
            JOIN TITRES T ON T.SICOVAM = H.SICOVAM
            JOIN CDC_BO_CODE_PRODUIT CP ON CP.AFFECTATION = T.AFFECTATION AND CP.REF_IDENT IS NULL
                AND CP.FAMILLE IN (1) -- 1 (ferme)
            LEFT JOIN HISTOMVTS HREPO ON HREPO.REFCON = CASE WHEN CP.FAMILLE = 3 AND T.TYPE = 'L' THEN H.REFERENCE ELSE NULL END 
                LEFT JOIN TITRES TREPO ON TREPO.SICOVAM = HREPO.SICOVAM
            JOIN TITRES TT1 ON TT1.SICOVAM = CASE WHEN CP.FAMILLE = 1 THEN T.SICOVAM ELSE
                CASE WHEN T.TYPE = 'L' THEN TREPO.CODE_EMET ELSE T.CODE_EMET END END
            JOIN EXTRNL_REFERENCES_INSTRUMENTS ERI ON ERI.SOPHIS_IDENT = TT1.SICOVAM
                AND ERI.REF_IDENT = (SELECT REF_IDENT FROM EXTRNL_REFERENCES_DEFINITION WHERE REF_NAME = 'TTF Eligible')
                AND ERI.VALUE IN ('IT') -- [UNDERLYING_ID] porte une external references 'FR', 'IT' 
            JOIN NATIXIS_FOLIO_SECTION_ENTITE FSE ON FSE.IDENT = H.OPCVM AND FSE.SECTION != '799' -- not in section simulation
        WHERE H.DATEVAL BETWEEN TRUNC(GP_DATE, 'MM') - (SELECT TO_NUMBER(VALUE) FROM NATIXIS_GROUP_PARAM WHERE TYPE='TTF' AND KEY='BackValueLag') AND TRUNC(GP_DATE)
            AND H.DATENEG >= TO_DATE('20130301', 'YYYYMMDD') AND H.DATEVAL >= TO_DATE('20130301', 'YYYYMMDD') -- date d'entrée en vigueur de la TTF IT
            AND INSTR(',' || (SELECT VALUE FROM NATIXIS_GROUP_PARAM WHERE TYPE = 'TTF' AND KEY = 'BE selected' AND ENABLED = 1) || ',', ',' || H.TYPE || ',') > 0
            AND H.REFCON NOT IN (SELECT REFCON FROM NATIXIS_TTFCOLLECTED_AUDIT)
            AND H.QUANTITE < 0 AND H.NXS_TAX_NAME = 'TTF' AND H.NXS_TAX_VALUE IS NOT NULL AND H.NXS_TAX_VALUE != 0
            AND H.BACKOFFICE IN (
                SELECT KSC.KERNEL_STATUS_ID FROM BO_KERNEL_STATUS_COMPONENT KSC
                    JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID
                    AND KSG.RECORD_TYPE = 1 AND KSG.NAME = 'All But Pending FO');
        end if;
        
        COMMIT;
        
        --goto FIN;
        
        <<GXML>>
        if GP_SIMU = 0 then -- not simulation
            SEND_TO_GXML_COMPTE_TIERS;
        end if;
        
        <<FIN>>
        BL.INFO('COMPTE_TIERS.END');
        
    EXCEPTION
        when others then
            BL.ERROR('COMPTE_TIERS. code = {1}, message = {2}, backtrace={3}', SQLCODE, SQLERRM, DBMS_UTILITY.FORMAT_ERROR_BACKTRACE);
            BL.INFO('COMPTE_TIERS.END');
            raise;
    END;
    
    PROCEDURE TTF_IT_DERIVE
    AS
    BEGIN
        BL.INFO('TTF_IT_DERIVE.BEGIN');
        
        INSERT INTO NATIXIS_TTFDERIVATIVES_AUDIT(TTF_COUNTRY, REFCON, MVTIDENT, DATE_CALCUL, FOLIO, SECTION,
            TAXABLE_AMOUNT, TTF_AMOUNT, TTF_CUR, ISIN, PRODUCT_FAMILY, DATE_NEG, STATUS_REPORT)
        SELECT 'IT', H.REFCON, H.MVTIDENT, TRUNC(GP_DATE), H.OPCVM, FSE.SECTION,
            BS_LIB.NOMINAL_CE_P_2(H.MVTIDENT, TO_CHAR(GP_DATE, 'YYYYMMDD')), ABS(H.MONTANT), 'EUR', T.MNEMO_V2, CP.FAMILLE, H.DATENEG, 1
        FROM HISTOMVTS H
            JOIN TITRES T ON T.SICOVAM = H.SICOVAM-- AND T.TYPE != 'L'
            JOIN CDC_BO_CODE_PRODUIT CP ON CP.AFFECTATION = T.AFFECTATION AND CP.REF_IDENT IS NULL
                AND CP.FAMILLE IN (1, 2, 4) -- 1 (ferme) ou 2 (OTC), 4 (DERIVES ORGANISES)
            LEFT JOIN HISTOMVTS HREPO ON HREPO.REFCON = CASE WHEN CP.FAMILLE = 3 AND T.TYPE = 'L' THEN H.REFERENCE ELSE NULL END 
                LEFT JOIN TITRES TREPO ON TREPO.SICOVAM = HREPO.SICOVAM
            JOIN NATIXIS_FOLIO_SECTION_ENTITE FSE ON FSE.IDENT = H.OPCVM AND FSE.SECTION != '799' -- not in section simulation
        WHERE H.DATENEG BETWEEN TRUNC(GP_DATE, 'MM') - (SELECT TO_NUMBER(VALUE) FROM NATIXIS_GROUP_PARAM WHERE TYPE='TTF' AND KEY='BackValueLag') AND TRUNC(GP_DATE)
            AND H.TYPE = 740
            AND H.BACKOFFICE = 1077
            AND H.REFCON NOT IN (SELECT REFCON FROM NATIXIS_TTFDERIVATIVES_AUDIT)
            AND H.DATENEG >= TO_DATE('20130701', 'YYYYMMDD');
                
        BL.INFO('TTF_IT_DERIVE.END. rowcount = {1}', SQL%ROWCOUNT);
    END;
    
    PROCEDURE MAIN(p_date in date default trunc(sysdate), p_country in varchar2 default null, p_simu in int default 0)
    AS
        rc                      number;
    BEGIN
        BL.SET_LOGGER('TTF');
        
        GP_DATE := p_date;
        GP_COUNTRY := p_country;
        GP_SIMU := p_simu;
        
        BL.INFO('MAIN.BEGIN(p_date={1},p_country={2},p_simu={3})', GP_DATE, GP_COUNTRY, GP_SIMU);
        
        if BL.GET_OUTPUT_VALUE('BeginLogId') is null then
            SELECT BL_LOGS_SEQ.CURRVAL INTO rc FROM DUAL;
            BL.SET_OUTPUT_VALUE('BeginLogId', rc);
        end if;
        if BL.GET_OUTPUT_VALUE('BeginGXMLId') is null then
            BL.SET_OUTPUT_VALUE('BeginGXMLId', 1); BL.SET_OUTPUT_VALUE('EndGXMLId', 0); -- between begin and end return no rows
        end if;
        
        SELECT TO_NUMBER(VALUE)
            into IT_PAYDATE
        FROM NATIXIS_GROUP_PARAM WHERE TYPE = 'TTF' AND KEY = 'TTF_IT_Paydate';
        
        COMPTE_PROPRE;
        COMPTE_TIERS;
        if GP_COUNTRY = 'IT' then
            TTF_IT_DERIVE;
        end if;
        
        BL.INFO('MAIN.END');
        SELECT BL_LOGS_SEQ.CURRVAL INTO rc FROM DUAL;
        BL.SET_OUTPUT_VALUE('EndLogId', rc);
        
    EXCEPTION
        when others then
            BL.ERROR('MAIN. code = {1}, message = {2}, backtrace={3}', SQLCODE, SQLERRM, DBMS_UTILITY.FORMAT_ERROR_BACKTRACE);
            BL.INFO('MAIN.END');
            SELECT BL_LOGS_SEQ.CURRVAL INTO rc FROM DUAL;
            BL.SET_OUTPUT_VALUE('EndLogId', rc);
            raise;
    END;
    
    FUNCTION IS_CTPY_INTRAGROUP_NATIXIS(
        p_ctpy_id       TIERS.IDENT%TYPE,
        p_entity_id     TIERS.IDENT%TYPE)
    RETURN NUMBER
    AS
        LibEntite       TIERS.REFERENCE%TYPE;
        LibEntiteCtp    TIERS.REFERENCE%TYPE;
        IsCtpEntity     number;
        result          number;
    BEGIN
        result := 0;
        
        SELECT NVL(T.ENTITY, 0)
            INTO IsCtpEntity
        FROM TIERS T WHERE T.IDENT = p_ctpy_id;
        
        if IsCtpEntity = 0 then
            -- libelle de l'entite de l'op¿ au temps
            SELECT DECODE(T.REFERENCE, 'CDCM', 'CCBPPA', T.REFERENCE)
                INTO LibEntite
            FROM TIERS T WHERE T.IDENT = p_entity_id;

            -- libelle entite de rattachement de la CTP
            SELECT DECODE(T.VALUE, 'CDCM', 'CCBPPA', T.VALUE)
                INTO LibEntiteCtp
            FROM TIERSPROPERTIES T
            WHERE T.NAME = 'Rattachement' AND T.CODE = p_ctpy_id;
            
            if LibEntiteCtp is not null and LibEntiteCtp = LibEntite then
                result := 1;
            end if;
        else
            result := 1;
        end if;
        
        return result;
        
    EXCEPTION
        when NO_DATA_FOUND then
            return result;
        when OTHERS then
            raise;
    END;

END TTF;
/

CREATE OR REPLACE PUBLIC SYNONYM TTF FOR TTF;
/

