CREATE OR REPLACE PACKAGE PH
AS
    FUNCTION GET_SLEEVE_ID(FolioId FOLIO.IDENT%TYPE) RETURN FOLIO.IDENT%TYPE;
    PROCEDURE INIT_SLEEVE_IDS_BY_DATE(p_Date in date);
    PROCEDURE GEN_FX_SLEEVES(
        p_Date      in      date,
        p_Idents    in      varchar2,
        p_Ccys      in      varchar2);
END PH;
/

CREATE OR REPLACE PACKAGE BODY PH
AS
    FUNCTION GET_SLEEVE_ID(FolioId FOLIO.IDENT%TYPE)
    RETURN FOLIO.IDENT%TYPE AS
        ans FOLIO.IDENT%TYPE;
        rc number;
    BEGIN
        SELECT COUNT(*)
            into rc
        FROM PFR_MODEL_LINK WHERE FOLIO = FolioId;
        if rc = 0 then
            SELECT IDENT
                into ans
            FROM (
                SELECT CONNECT_BY_ROOT IDENT IDENT
                FROM FOLIO WHERE IDENT = FolioId
                CONNECT BY PRIOR IDENT = MGR)
            WHERE IDENT IN (SELECT FOLIO FROM PFR_MODEL_LINK);
        else
            ans := FolioId;
        end if;    
        return ans;
    END;
    
    PROCEDURE INIT_SLEEVE_IDS_BY_DATE(p_Date in date)
    AS
    BEGIN
        DELETE MEDIO_VALUES;
        INSERT INTO MEDIO_VALUES(VALNUM)
            SELECT DISTINCT PH.GET_SLEEVE_ID(HP.OPCVM) SLEEVE_ID
                FROM JOIN_POSITION_HISTOMVTS HP
                    JOIN TITRES T ON T.SICOVAM = HP.SICOVAM
                        AND T.AFFECTATION IN(SELECT IDENT FROM AFFECTATION WHERE LIBELLE IN ('EQUITY', 'ETF'))
                WHERE HP.DATENEG = p_Date AND HP.TYPE = 1
                    AND HP.OPERATEUR NOT IN (SELECT IDENT FROM RISKUSERS WHERE NAME IN ('MANAGER', 'BBHUploader', 'RBCUploader', 'Forecast'));
    END;
    
    PROCEDURE GEN_FX_SLEEVES(
        p_Date      in      date,
        p_Idents    in      varchar2,
        p_Ccys      in      varchar2)
    AS
        rc          number;
        curCcys     SYS_REFCURSOR;
        curIdents   SYS_REFCURSOR;
        ccy         char(3);
        folioId     FOLIO.IDENT%TYPE;
    BEGIN
        DBMS_OUTPUT.put(to_char(localtimestamp,'YYYY-MM-DD HH24:MI:SS.FF2    '));
        DBMS_OUTPUT.put_line ('[DEBUG]    BEGIN.p_Idents=' || p_Idents ||', p_Ccys=' || p_Ccys || ', p_Date' || p_Date);
        
        DELETE MEDIO_FX_SLEEVES;
        DELETE MEDIO_VALUES;
        open curIdents for
            with rws as (select p_Idents str from dual)
            select to_number(regexp_substr (str, '[^,]+', 1, level)) value from rws
            connect by level <= length(str) - length(replace(str, ',')) + 1;
        LOOP
            FETCH curIdents INTO folioId;
            EXIT WHEN curIdents%NOTFOUND;
            
            DBMS_OUTPUT.put(to_char(localtimestamp,'YYYY-MM-DD HH24:MI:SS.FF2    '));
            DBMS_OUTPUT.put_line ('[DEBUG]    folioId=' || folioId);
            
            INSERT INTO MEDIO_VALUES(VALNUM, VALNUM2)
                SELECT folioId, IDENT FROM FOLIO F START WITH IDENT = folioId CONNECT BY PRIOR IDENT = MGR;
        END LOOP;
        close curIdents;
        --DBMS_OUTPUT.put(to_char(localtimestamp,'YYYY-MM-DD HH24:MI:SS.FF2    '));
        --DBMS_OUTPUT.put_line ('[DEBUG]    DELETE MEDIO_FX_SLEEVES ROWCOUNT=' || SQL%ROWCOUNT);

        --Folio hierarchy. NodeType: Sleeve = 1, Ancestor of Sleeve = 0
        INSERT INTO MEDIO_FX_SLEEVES(PARENTID, ID, NAME, CURRENCY, BPS, THRESHOLD, NODETYPE)
        SELECT DISTINCT CONNECT_BY_ROOT MGR PARENTID, CONNECT_BY_ROOT F.IDENT ID, CONNECT_BY_ROOT NAME,
            CASE WHEN LEVEL = 1 THEN NVL(S.CCY, (SELECT UPPER(DEVISE_TO_STR(DEVISECTT)) FROM TITRES WHERE SICOVAM = F.SICOVAM)) ELSE NULL END CURRENCY,
            CASE WHEN LEVEL = 1 THEN S.BPS ELSE NULL END BPS,
            CASE WHEN LEVEL = 1 THEN CASE WHEN S.BPS IS NULL OR S.BPS = 0 THEN S.THRESHOLD ELSE NULL END END THRESHOLD,
            CASE WHEN LEVEL > 1 THEN 0 ELSE 1 END NODETYPE
        FROM FOLIO F
			LEFT JOIN MEDIO_FXAUTO_SLEEVES S ON F.IDENT = S.IDENT
		WHERE F.IDENT IN (SELECT DISTINCT VALNUM FROM MEDIO_VALUES WHERE VALNUM IS NOT NULL)
            AND F.IDENT IN (SELECT FOLIO FROM PFR_MODEL_LINK)
        CONNECT BY PRIOR F.IDENT = MGR;
        
        DBMS_OUTPUT.put(to_char(localtimestamp,'YYYY-MM-DD HH24:MI:SS.FF2    '));
        DBMS_OUTPUT.put_line ('[DEBUG]    MEDIO_FX_SLEEVES ROWCOUNT=' || SQL%ROWCOUNT);
        
        --INSERT INTO MEDIO_VALUES(VALSTR)
        open curCcys for
            with rws as (select p_Ccys str from dual)
            select regexp_substr (str, '[^,]+', 1, level) value from rws
            connect by level <= length(str) - length(replace(str, ',')) + 1
            order by 1;
        --open curCcys for SELECT VALSTR FROM MEDIO_VALUES ORDER BY 1;
        LOOP
            FETCH curCcys INTO ccy;
            EXIT WHEN curCcys%NOTFOUND;
            DBMS_OUTPUT.put(to_char(localtimestamp,'YYYY-MM-DD HH24:MI:SS.FF2    '));
            DBMS_OUTPUT.put_line ('[DEBUG]    ccy=' || ccy);
            
            --Currency Level. NodeType = 2
            INSERT INTO MEDIO_FX_SLEEVES(PARENTID, ID, BALANCE, NAME, NODETYPE)
            SELECT SLEEVE_ID PARENTID, SLEEVE_ID || '_' || CURRENCY ID, SUM(BALANCE), CURRENCY, 2 NODETYPE
            FROM(
            SELECT ccy CURRENCY,
                CASE WHEN HP.DATEVAL > p_Date THEN 0 WHEN ((T.TYPE = 'E' AND UPPER(DEVISE_TO_STR(T.MARCHE)) = ccy) OR (T.LIBELLE = 'Cash for currency ''' || ccy || '''')) THEN HP.QUANTITE ELSE -HP.MONTANT END BALANCE,
                F.IDENT SLEEVE_ID, HP.SICOVAM, T.LIBELLE, F.NAME, NVL(HP.DELIVERY_DATE, HP.DATEVAL) DELIVERY_DATE,
                CASE WHEN HP.DATENEG = p_Date AND T.AFFECTATION IN (SELECT IDENT FROM AFFECTATION WHERE LIBELLE IN ('EQUITY', 'ETF')) AND HP.TYPE = 1 THEN 1 ELSE 0 END OK,
                CASE WHEN ((T.TYPE = 'E' AND UPPER(DEVISE_TO_STR(T.MARCHE)) = ccy) OR (T.LIBELLE = 'Cash for currency ''' || ccy || '''')) THEN HP.QUANTITE WHEN HP.DATENEG = p_Date AND T.AFFECTATION IN (SELECT IDENT FROM AFFECTATION WHERE LIBELLE IN ('EQUITY', 'ETF')) AND HP.TYPE = 1
                    THEN CASE WHEN HP.QUANTITE < 0 THEN ABS(HP.MONTANT) ELSE -HP.MONTANT END ELSE 0 END AMOUNT
            FROM JOIN_POSITION_HISTOMVTS HP
                JOIN TITRES T ON T.SICOVAM = HP.SICOVAM-- AND T.AFFECTATION = 1-- OR T.TYPE = 'E')
                JOIN MEDIO_VALUES MV ON MV.VALNUM2 = HP.OPCVM
                JOIN FOLIO F ON F.IDENT = MV.VALNUM
            WHERE HP.MONTANT != 0 AND (T.LIBELLE = 'Cash for currency ''' || ccy || '''' OR HP.DATENEG <= p_Date)
                AND HP.OPCVM IN (SELECT VALNUM2 FROM MEDIO_VALUES WHERE VALNUM2 IS NOT NULL)
                AND ((T.TYPE = 'E' AND UPPER(DEVISE_TO_STR(T.MARCHE)) = ccy) OR UPPER(DEVISE_TO_STR(DEVISEPAY)) = ccy)
                --AND ((HP.COURSDEVPAY = 1 AND UPPER(DEVISE_TO_STR(DEVISEPAY)) = ccy) OR (HP.COURSDEVPAY = 0 AND (UPPER(DEVISE_TO_STR(T.DEVISECTT)) = ccy OR
                --    (T.TYPE = 'E' AND UPPER(DEVISE_TO_STR(T.MARCHE)) = ccy))))
                AND HP.BACKOFFICE IN (SELECT DISTINCT KSC.KERNEL_STATUS_ID FROM BO_KERNEL_STATUS_COMPONENT KSC
                    JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID AND KSG.NAME = 'MAPS - All trades' AND KSG.RECORD_TYPE = 1)
            )
            --WHERE OK = 1
            GROUP BY SLEEVE_ID, CURRENCY
            HAVING SUM(AMOUNT) != 0
            ORDER BY SLEEVE_ID, CURRENCY;
            
            DBMS_OUTPUT.put(to_char(localtimestamp,'YYYY-MM-DD HH24:MI:SS.FF2    '));
            DBMS_OUTPUT.put_line ('[DEBUG]    MEDIO_FX_SLEEVES ROWCOUNT=' || SQL%ROWCOUNT);
            
            --SettlementDate Level. NodeType = 3
            INSERT INTO MEDIO_FX_SLEEVES(PARENTID, ID, BALANCE, NAME, CURRENCY, NODETYPE, DATESETTLEMENT, AMOUNT)
            SELECT SLEEVE_ID || '_' || CURRENCY PARENTID, SLEEVE_ID || '_' || CURRENCY || '_' || DELIVERY_DATE ID, NULL/*SUM(BALANCE)*/, DELIVERY_DATE NAME, CURRENCY, 3 NODETYPE, DELIVERY_DATE, SUM(AMOUNT)
            FROM(
            SELECT ccy CURRENCY,
                CASE WHEN HP.DATEVAL > p_Date THEN 0 WHEN ((T.TYPE = 'E' AND UPPER(DEVISE_TO_STR(T.MARCHE)) = ccy) OR (T.LIBELLE = 'Cash for currency ''' || ccy || '''')) THEN HP.QUANTITE ELSE -HP.MONTANT END BALANCE,
                F.IDENT SLEEVE_ID, HP.SICOVAM, T.LIBELLE, F.NAME, NVL(HP.DELIVERY_DATE, HP.DATEVAL) DELIVERY_DATE,
                CASE WHEN HP.DATENEG = p_Date AND T.AFFECTATION IN (SELECT IDENT FROM AFFECTATION WHERE LIBELLE IN ('EQUITY', 'ETF')) AND HP.TYPE = 1 THEN 1 ELSE 0 END OK,
                CASE WHEN ((T.TYPE = 'E' AND UPPER(DEVISE_TO_STR(T.MARCHE)) = ccy) OR (T.LIBELLE = 'Cash for currency ''' || ccy || '''')) THEN HP.QUANTITE WHEN HP.DATENEG = p_Date AND T.AFFECTATION IN (SELECT IDENT FROM AFFECTATION WHERE LIBELLE IN ('EQUITY', 'ETF')) AND HP.TYPE = 1
                    THEN CASE WHEN HP.QUANTITE < 0 THEN ABS(HP.MONTANT) ELSE -HP.MONTANT END ELSE 0 END AMOUNT
            FROM JOIN_POSITION_HISTOMVTS HP
                JOIN TITRES T ON T.SICOVAM = HP.SICOVAM-- AND T.AFFECTATION = 1-- OR T.TYPE = 'E')
                JOIN MEDIO_VALUES MV ON MV.VALNUM2 = HP.OPCVM
                JOIN FOLIO F ON F.IDENT = MV.VALNUM
            WHERE HP.MONTANT != 0 AND (T.LIBELLE = 'Cash for currency ''' || ccy || '''' OR HP.DATENEG <= p_Date)
                AND HP.OPCVM IN (SELECT VALNUM2 FROM MEDIO_VALUES WHERE VALNUM2 IS NOT NULL)
                AND ((T.TYPE = 'E' AND UPPER(DEVISE_TO_STR(T.MARCHE)) = ccy) OR UPPER(DEVISE_TO_STR(DEVISEPAY)) = ccy)
                --AND ((HP.COURSDEVPAY = 1 AND UPPER(DEVISE_TO_STR(DEVISEPAY)) = ccy) OR (HP.COURSDEVPAY = 0 AND (UPPER(DEVISE_TO_STR(T.DEVISECTT)) = ccy OR
                    --(T.TYPE = 'E' AND UPPER(DEVISE_TO_STR(T.MARCHE)) = ccy))))
                AND HP.BACKOFFICE IN (SELECT DISTINCT KSC.KERNEL_STATUS_ID FROM BO_KERNEL_STATUS_COMPONENT KSC
                    JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID AND KSG.NAME = 'MAPS - All trades' AND KSG.RECORD_TYPE = 1)
            )
            WHERE OK = 1
            GROUP BY SLEEVE_ID, CURRENCY, DELIVERY_DATE
            HAVING SUM(AMOUNT) != 0
            ORDER BY SLEEVE_ID, CURRENCY, DELIVERY_DATE;
            
            --Position Level. NodeType = 4
            INSERT INTO MEDIO_FX_SLEEVES(PARENTID, ID, BALANCE, NAME, CURRENCY, NODETYPE, DATESETTLEMENT, AMOUNT)
            SELECT SLEEVE_ID || '_' || CURRENCY || '_' || DELIVERY_DATE PARENTID, SLEEVE_ID || '_' || CURRENCY || '_' || DELIVERY_DATE || '_' || SICOVAM ID, NULL/*SUM(BALANCE)*/, LIBELLE NAME, CURRENCY, 4 NODETYPE, DELIVERY_DATE, SUM(AMOUNT)
            FROM(
            SELECT ccy CURRENCY,
                CASE WHEN HP.DATEVAL > p_Date THEN 0 WHEN ((T.TYPE = 'E' AND UPPER(DEVISE_TO_STR(T.MARCHE)) = ccy) OR (T.LIBELLE = 'Cash for currency ''' || ccy || '''')) THEN HP.QUANTITE ELSE -HP.MONTANT END BALANCE,
                F.IDENT SLEEVE_ID, HP.SICOVAM, T.LIBELLE, F.NAME, NVL(HP.DELIVERY_DATE, HP.DATEVAL) DELIVERY_DATE,
                CASE WHEN HP.DATENEG = p_Date AND T.AFFECTATION IN (SELECT IDENT FROM AFFECTATION WHERE LIBELLE IN ('EQUITY', 'ETF')) AND HP.TYPE = 1 THEN 1 ELSE 0 END OK,
                CASE WHEN ((T.TYPE = 'E' AND UPPER(DEVISE_TO_STR(T.MARCHE)) = ccy) OR (T.LIBELLE = 'Cash for currency ''' || ccy || '''')) THEN HP.QUANTITE WHEN HP.DATENEG = p_Date AND T.AFFECTATION IN (SELECT IDENT FROM AFFECTATION WHERE LIBELLE IN ('EQUITY', 'ETF')) AND HP.TYPE = 1
                    THEN CASE WHEN HP.QUANTITE < 0 THEN ABS(HP.MONTANT) ELSE -HP.MONTANT END ELSE 0 END AMOUNT
            FROM JOIN_POSITION_HISTOMVTS HP
                JOIN TITRES T ON T.SICOVAM = HP.SICOVAM-- AND T.AFFECTATION = 1-- OR T.TYPE = 'E')
                JOIN MEDIO_VALUES MV ON MV.VALNUM2 = HP.OPCVM
                JOIN FOLIO F ON F.IDENT = MV.VALNUM
            WHERE HP.MONTANT != 0 AND (T.LIBELLE = 'Cash for currency ''' || ccy || '''' OR HP.DATENEG <= p_Date)
                AND HP.OPCVM IN (SELECT VALNUM2 FROM MEDIO_VALUES WHERE VALNUM2 IS NOT NULL)
                AND ((T.TYPE = 'E' AND UPPER(DEVISE_TO_STR(T.MARCHE)) = ccy) OR UPPER(DEVISE_TO_STR(DEVISEPAY)) = ccy)
                --AND ((HP.COURSDEVPAY = 1 AND UPPER(DEVISE_TO_STR(DEVISEPAY)) = ccy) OR (HP.COURSDEVPAY = 0 AND (UPPER(DEVISE_TO_STR(T.DEVISECTT)) = ccy OR
                    --(T.TYPE = 'E' AND UPPER(DEVISE_TO_STR(T.MARCHE)) = ccy))))
                AND HP.BACKOFFICE IN (SELECT DISTINCT KSC.KERNEL_STATUS_ID FROM BO_KERNEL_STATUS_COMPONENT KSC
                    JOIN BO_KERNEL_STATUS_GROUP KSG ON KSG.ID = KSC.KERNEL_STATUS_GROUP_ID AND KSG.NAME = 'MAPS - All trades' AND KSG.RECORD_TYPE = 1)
            )
            WHERE OK = 1
            GROUP BY SLEEVE_ID, CURRENCY, DELIVERY_DATE, SICOVAM, LIBELLE
            HAVING SUM(AMOUNT) != 0
            ORDER BY SLEEVE_ID, CURRENCY, DELIVERY_DATE, LIBELLE;
            
            DBMS_OUTPUT.put(to_char(localtimestamp,'YYYY-MM-DD HH24:MI:SS.FF2    '));
            DBMS_OUTPUT.put_line ('[DEBUG]    MEDIO_FX_SLEEVES ROWCOUNT=' || SQL%ROWCOUNT);
            
            DELETE MEDIO_FX_SLEEVES MS WHERE NODETYPE = 2 AND NOT EXISTS (SELECT * FROM MEDIO_FX_SLEEVES WHERE PARENTID = MS.ID);
        END LOOP;
        close curCcys;
        
    EXCEPTION
    WHEN OTHERS THEN 
        DBMS_OUTPUT.put(to_char(localtimestamp,'YYYY-MM-DD HH24:MI:SS.FF2    '));
        DBMS_OUTPUT.put_line ('[ERROR]    SQLCODE=' || SQLCODE || ', SQLERRM=' || SQLERRM);
        RAISE;
    END;
END PH;
/

CREATE OR REPLACE PUBLIC SYNONYM PH FOR PH;
/
