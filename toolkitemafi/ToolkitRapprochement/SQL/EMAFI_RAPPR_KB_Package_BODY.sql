create or replace 
PACKAGE BODY EMAFI_RAPPR_KB AS

FUNCTION test_date (p_date IN VARCHAR2, p_format IN VARCHAR2 DEFAULT 'DD/MM/YY')
RETURN BOOLEAN
IS
	v_date DATE;
BEGIN
	v_date := TO_DATE(p_date, p_format);
	RETURN TRUE;
EXCEPTION
WHEN OTHERS THEN RETURN FALSE;
END;

------------------------------------------------------------------------------------


PROCEDURE test_date_null( originalData varchar2,dateVal varchar2,dateOp varchar2)
IS
BEGIN
    IF(dateVal = '      ')THEN
        update EMAFI_RECO_RELEV_BANC 
        SET 
        DATE_VALEUR = null
        WHERE ORIGINAL_DATA = originalData ;
    END IF;
    
    IF(dateOp = '      ')THEN
        update EMAFI_RECO_RELEV_BANC 
        SET 
        DATE_OPERATION = null
        WHERE ORIGINAL_DATA = originalData ;
    END IF;
END;

------------------------------------------------------------------------------------

function parse_Montant1( montant_char varchar2, nbr_decim number) return number
IS

firstPart varchar2(16);
secondPart varchar2(1);

montant number;
BEGIN
    IF(nbr_decim = 2) then
      firstPart := SUBSTR (montant_char, 1,13);
      secondPart := SUBSTR (montant_char, 14,1);      
      
      case secondPart 
      when '{' then montant := to_number(firstPart||'0');
      when 'A' then montant := to_number(firstPart||'1');
      when 'B' then montant := to_number(firstPart||'2');
      when 'C' then montant := to_number(firstPart||'3');
      when 'D' then montant := to_number(firstPart||'4');
      when 'E' then montant := to_number(firstPart||'5');
      when 'F' then montant := to_number(firstPart||'6');
      when 'G' then montant := to_number(firstPart||'7');
      when 'H' then montant := to_number(firstPart||'8');
      when 'I' then montant := to_number(firstPart||'9');
      ----
      when '}' then montant := to_number(firstPart||'0') * -1;
      when 'J' then montant := to_number(firstPart||'1') * -1;
      when 'K' then montant := to_number(firstPart||'2') * -1;
      when 'L' then montant := to_number(firstPart||'3') * -1;
      when 'M' then montant := to_number(firstPart||'4') * -1;
      when 'N' then montant := to_number(firstPart||'5') * -1;
      when 'O' then montant := to_number(firstPart||'6') * -1;
      when 'P' then montant := to_number(firstPart||'7') * -1;
      when 'Q' then montant := to_number(firstPart||'8') * -1;
      else  montant := to_number(firstPart||'9') * -1;
    end case;
    
    else return 0;
    end if;
 
  return montant/100;
END;
---******************************************************


procedure parse_OriginalData  
IS
CURSOR rapproch_cur IS
SELECT ORIGINAL_DATA FROM EMAFI_RECO_RELEV_BANC where statut is null FOR UPDATE;
ligne rapproch_cur%rowtype;

BEGIN
  OPEN rapproch_cur ;
  LOOP
  FETCH rapproch_cur INTO ligne ;
    EXIT WHEN rapproch_cur%NOTFOUND ;

    
        update EMAFI_RECO_RELEV_BANC 
        SET 
        TYPE_ENREG = SUBSTR (ligne.ORIGINAL_DATA, 1, 2),
        CODE_BANQ = SUBSTR (ligne.ORIGINAL_DATA, 3, 5),
        CODE_OPE = SUBSTR (ligne.ORIGINAL_DATA, 8, 4),
       CODE_GUICHET = SUBSTR (ligne.ORIGINAL_DATA, 12, 5),
       CODE_DEV = SUBSTR (ligne.ORIGINAL_DATA, 17,3),
        NBR_DECIM = SUBSTR (ligne.ORIGINAL_DATA, 20,1),
    NUM_COMPTE = SUBSTR (ligne.ORIGINAL_DATA, 22,11),
     CODE_OPER_INTERB = SUBSTR (ligne.ORIGINAL_DATA, 33,2),
     DATE_OPERATION = SUBSTR (ligne.ORIGINAL_DATA, 35,6),
         DATE_VALEUR = SUBSTR (ligne.ORIGINAL_DATA, 43,6),
       LIBELLE = SUBSTR (ligne.ORIGINAL_DATA, 49,31),
     NUMERO_ECRIT = SUBSTR (ligne.ORIGINAL_DATA, 82,7),
     MONTANT = SUBSTR (ligne.ORIGINAL_DATA, 91,14),
     MONTANT_NUMBER = parse_Montant1(SUBSTR (ligne.ORIGINAL_DATA, 91,14),to_number(SUBSTR (ligne.ORIGINAL_DATA, 20,1))),
        REFER_1 = SUBSTR (ligne.ORIGINAL_DATA, 105,16),
        MOTIF_REJET = null 
		where current of rapproch_cur;		
     
     
  END LOOP ;  
END;


------------------------------------------

procedure test_conditions_date_null  
IS
CURSOR rapproch_cur IS
SELECT ORIGINAL_DATA,DATE_VALEUR,DATE_OPERATION FROM EMAFI_RECO_RELEV_BANC where statut is null ;
ligne rapproch_cur%rowtype;
BEGIN
  OPEN rapproch_cur ;
  LOOP
  FETCH rapproch_cur INTO ligne ;
    EXIT WHEN rapproch_cur%NOTFOUND ;

   test_date_null(ligne.ORIGINAL_DATA,ligne.DATE_VALEUR,ligne.DATE_OPERATION);
  
  END LOOP ;  
END;


-----------------------------------------------

procedure test_conditions  
IS
CURSOR rapproch_cur IS
SELECT * FROM EMAFI_RECO_RELEV_BANC where statut is null;
ligne rapproch_cur%rowtype;
lineMax number;
BEGIN
  OPEN rapproch_cur ;
  LOOP
  FETCH rapproch_cur INTO ligne ;
    EXIT WHEN rapproch_cur%NOTFOUND ;

    IF (( ligne.Type_Enreg != '01' and ligne.Type_Enreg != '04' and ligne.Type_Enreg != '07') or ligne.Type_Enreg =null) THEN
        update EMAFI_RECO_RELEV_BANC 
        SET 
        MOTIF_REJET = 'M01'
        WHERE ORIGINAL_DATA = ligne.ORIGINAL_DATA ;
        
        update EMAFI_RECO_RELEV_BANC 
        SET 
        STATUT = 'KO'
        WHERE NOM_FICHIER = ligne.NOM_FICHIER ;
    END IF;

    select max(line_number) into lineMax from EMAFI_RECO_RELEV_BANC where nom_fichier = ligne.nom_fichier;
   IF ((length(ligne.ORIGINAL_DATA) !=120 and ligne.line_number!=lineMax) or
      (length(ligne.ORIGINAL_DATA) !=119 and ligne.line_number=lineMax)  ) THEN
        update EMAFI_RECO_RELEV_BANC 
        SET 
        MOTIF_REJET = 'M00'
        WHERE ORIGINAL_DATA = ligne.ORIGINAL_DATA ;
        
        update EMAFI_RECO_RELEV_BANC 
        SET 
        STATUT = 'KO'
        WHERE NOM_FICHIER = ligne.NOM_FICHIER ;
    END IF;
    
    IF( test_date(ligne.DATE_OPERATION) = false or ligne.DATE_OPERATION is null)THEN
        update EMAFI_RECO_RELEV_BANC 
        SET 
        MOTIF_REJET = 'M02'
        WHERE ORIGINAL_DATA = ligne.ORIGINAL_DATA ;
        
        update EMAFI_RECO_RELEV_BANC 
        SET 
        STATUT = 'KO'
        WHERE NOM_FICHIER = ligne.NOM_FICHIER ;
    END IF;
    
    IF((test_date(ligne.DATE_VALEUR) = false and  ligne.DATE_VALEUR IS NOT NULL and ligne.TYPE_ENREG != '04')
    or (ligne.DATE_VALEUR is null and ligne.TYPE_ENREG = '04'))THEN 
        update EMAFI_RECO_RELEV_BANC 
        SET 
        MOTIF_REJET = 'M03'
        WHERE ORIGINAL_DATA = ligne.ORIGINAL_DATA ;
        
        update EMAFI_RECO_RELEV_BANC 
        SET 
        STATUT = 'KO'
        WHERE NOM_FICHIER = ligne.NOM_FICHIER ;
    END IF;
         
  END LOOP ;  
END;

------------------------------------------

procedure test_somme_montants  
IS
CURSOR compte_cur IS
SELECT DISTINCT NOM_FICHIER,CODE_DEV,NUM_COMPTE  FROM EMAFI_RECO_RELEV_BANC;
compte compte_cur%rowtype;


somme01 number := 0;
somme04 number := 0;
somme07 number :=0;

BEGIN
      OPEN compte_cur;
      LOOP
      FETCH compte_cur INTO compte ;
       EXIT WHEN compte_cur%NOTFOUND ;
            select SUM(MONTANT_NUMBER) INTO somme01 from EMAFI_RECO_RELEV_BANC 
            where NOM_FICHIER = compte.NOM_FICHIER and CODE_DEV=compte.CODE_DEV and TYPE_ENREG = '01'
            and NUM_COMPTE= compte.NUM_COMPTE;
                        
            select SUM(MONTANT_NUMBER) INTO somme04 from EMAFI_RECO_RELEV_BANC 
            where NOM_FICHIER = compte.NOM_FICHIER and CODE_DEV=compte.CODE_DEV and TYPE_ENREG = '04'
            and NUM_COMPTE= compte.NUM_COMPTE;
                      
          select SUM(MONTANT_NUMBER) INTO somme07 from EMAFI_RECO_RELEV_BANC 
            where NOM_FICHIER = compte.NOM_FICHIER and CODE_DEV=compte.CODE_DEV and TYPE_ENREG = '07'
           and NUM_COMPTE= compte.NUM_COMPTE;
          
          if(somme01 + somme04 != somme07) then
           update EMAFI_RECO_RELEV_BANC 
            SET 
    
            MOTIF_REJET = 'M04', STATUT='KO'
            WHERE NOM_FICHIER = compte.NOM_FICHIER and CODE_DEV=compte.CODE_DEV
             and NUM_COMPTE= compte.NUM_COMPTE ;
          
          end if;
          
          somme01 :=0;
          somme04:= 0;
          somme07 := 0;
         
        END LOOP ;  
      CLOSE compte_cur ;
END;

procedure statut_OK  
IS
CURSOR rapproch_cur IS
SELECT nom_fichier FROM EMAFI_RECO_RELEV_BANC where statut is null for update;
ligne rapproch_cur%rowtype;

BEGIN
  OPEN rapproch_cur ;
  LOOP
  FETCH rapproch_cur INTO ligne ;
    EXIT WHEN rapproch_cur%NOTFOUND ;

    
        update EMAFI_RECO_RELEV_BANC 
        SET STATUT='OK' where current of  rapproch_cur ;
     
     
  END LOOP ;  
END;

procedure insert_into_balance  
IS
CURSOR compte_cur IS
SELECT distinct NOM_FICHIER,CODE_DEV,NUM_COMPTE , CODE_BANQ,CODE_GUICHET, STATUT
FROM EMAFI_RECO_RELEV_BANC where STATUT= 'OK' and (type_enreg = '01' or type_enreg = '07') and  REFER_FI IS NULL;

compte compte_cur%rowtype;
montant01 number;
montant07 number;

refer number;
BEGIN

      OPEN compte_cur;
      LOOP
      FETCH compte_cur INTO compte ;
       EXIT WHEN compte_cur%NOTFOUND ;
        if(compte.statut = 'OK') then
          select MONTANT_NUMBER into montant01 from EMAFI_RECO_RELEV_BANC where 
          NOM_FICHIER = compte.NOM_FICHIER and CODE_DEV=compte.CODE_DEV
          and NUM_COMPTE= compte.NUM_COMPTE and TYPE_ENREG='01';
          
          select MONTANT_NUMBER into montant07 from EMAFI_RECO_RELEV_BANC where 
          NOM_FICHIER = compte.NOM_FICHIER and CODE_DEV=compte.CODE_DEV
          and NUM_COMPTE= compte.NUM_COMPTE and TYPE_ENREG='07';
          
          
          select nvl(max(ID),0)+1 INTO refer from RECON_EXTERNAL_BALANCES;
          
          update EMAFI_RECO_RELEV_BANC set REFER_FI = refer 
          where NOM_FICHIER =compte.NOM_FICHIER  AND NUM_COMPTE= compte.NUM_COMPTE  AND CODE_DEV= compte.CODE_DEV 
          and (TYPE_ENREG='01' or TYPE_ENREG='07') and CODE_BANQ = compte.code_banq and CODE_GUICHET=compte.CODE_GUICHET ;
          
          
          insert into RECON_EXTERNAL_BALANCES VALUES (
          refer,
          (  select nvl(max(bo_treasury_account.id),0) from bo_treasury_account
          where bo_treasury_account.ACCOUNT_AT_CUSTODIAN like '%'||compte.CODE_BANQ||compte.CODE_GUICHET||compte.NUM_COMPTE||'%'),
          montant01,montant07,
         ( select to_date(
         (select DATE_OPERATION from EMAFI_RECO_RELEV_BANC where TYPE_ENREG='07'
          and NOM_FICHIER =compte.NOM_FICHIER  AND NUM_COMPTE= compte.NUM_COMPTE  AND CODE_DEV= compte.CODE_DEV)
         ,'DD-MM-RR') from dual),
          (select max(devisev2.code) from devisev2 where devise_to_str (devisev2.code) = compte.CODE_DEV),
          NULL,NULL,montant01,montant07,NULL,
          compte.CODE_BANQ||compte.CODE_GUICHET||compte.NUM_COMPTE);
          
          
          
          
        
        end if;
        END LOOP ;  
      CLOSE compte_cur ;

END;


procedure insert_into_trades  
IS

CURSOR mouvement_cur IS
SELECT MONTANT_NUMBER,CODE_BANQ,CODE_GUICHET,NUM_COMPTE ,CODE_DEV, NOM_FICHIER,DATE_OPERATION,DATE_VALEUR, LINE_NUMBER
FROM EMAFI_RECO_RELEV_BANC
where STATUT= 'OK' and type_enreg = '04'  and REFER_FI IS NULL;
mouvement mouvement_cur%rowtype;

refer number;
BEGIN

      OPEN mouvement_cur;
      LOOP
      FETCH mouvement_cur INTO mouvement;
       EXIT WHEN mouvement_cur%NOTFOUND ;
                
          
          select nvl(max(ID),0)+1 INTO refer from RECON_EXTERNAL_TRADES;
          
          
          
          Insert into RECON_EXTERNAL_TRADES 
          (ID,ACCOUNT,TRADE_DATE,VALUE_DATE,AMOUNT,CURRENCY,Tkt_External_account,quantity,depositary) 
          values 
          (
          refer, 
           (  select nvl(max(bo_treasury_account.id),0) from bo_treasury_account
          where bo_treasury_account.ACCOUNT_AT_CUSTODIAN like '%'||mouvement.CODE_BANQ||mouvement.CODE_GUICHET||mouvement.NUM_COMPTE||'%'),
          --date
          ( select to_date(mouvement.DATE_OPERATION,'DD-MM-RR') from dual),
          --date
          ( select to_date(mouvement.DATE_VALEUR,'DD-MM-RR') from dual),
          --
          mouvement.MONTANT_NUMBER * -1,
          (select max(devisev2.code) from devisev2 where devise_to_str (devisev2.code) = mouvement.CODE_DEV),
          mouvement.CODE_BANQ||mouvement.CODE_GUICHET||mouvement.NUM_COMPTE,
          (select sign(mouvement.MONTANT_NUMBER * -1) from dual),
         ( select max(a.depositary) from  bo_treasury_account  a 
          where a.ACCOUNT_AT_CUSTODIAN  like '%'||mouvement.CODE_BANQ||mouvement.CODE_GUICHET||mouvement.NUM_COMPTE||'%' )
          );
      
          
          update EMAFI_RECO_RELEV_BANC set REFER_FI = refer 
          where NOM_FICHIER =mouvement.NOM_FICHIER  AND NUM_COMPTE= mouvement.NUM_COMPTE  AND CODE_DEV= mouvement.CODE_DEV 
          and TYPE_ENREG='04' and LINE_NUMBER= mouvement.LINE_NUMBER;
          

        END LOOP ;  
      CLOSE mouvement_cur ;

END;

procedure Rapprochement_Procedures
is
begin
		parse_OriginalData;
		test_somme_montants;
		test_conditions_date_null; 
		test_conditions; 
		statut_OK; 
		insert_into_balance;
		insert_into_trades; 
end;


END EMAFI_RAPPR_KB;