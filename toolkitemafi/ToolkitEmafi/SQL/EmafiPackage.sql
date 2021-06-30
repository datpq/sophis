create or replace PACKAGE EMAFI
AS
  -- VARIABLES
  fund_id_data number;
  vl_date_data varchar2(20);
  
  FUNCTION FUND_ID RETURN NUMBER;
  FUNCTION VL_DATE RETURN VARCHAR2;
	-- Gestion des ODs
	TYPE TEMP_CRE IS VARRAY(20) OF EMAFI_CRE%rowtype;
	TYPE TEMP_POSTING IS VARRAY(20) OF ACCOUNT_POSTING%rowtype;
	one_cre TEMP_CRE := TEMP_CRE();
	one_posting TEMP_POSTING := TEMP_POSTING();
	od_indx number := 0;
PROCEDURE INSERT_ONE_ROW(
   FOLIO_ID IN NUMBER, COMPTE IN VARCHAR2, SENS_ IN VARCHAR2, MONTANT IN NUMBER, VALUE_DATE IN NUMBER, DEVISE IN VARCHAR2, CODE_DEVISE IN NUMBER, JOURNAL_ IN VARCHAR2, PIECE_ IN VARCHAR2, COMMENTAIRE_ IN VARCHAR2, 
  COMMENTAIRE_CODE IN VARCHAR2,USER_ID IN NUMBER, TIERS_ID_ IN NUMBER);
	PROCEDURE ADD_OD;
	PROCEDURE DELETE_OD(OD_NUM_ IN NUMBER);
	 
	/*UPDATING CUSTOMISED RIBBON FOR EMAFI PROGRMS*/  
	PROCEDURE INSTALL_CUSTOMISED_RIBBON(USER_ID IN NUMBER);
	PROCEDURE UNINSTALL_CUSTOMISED_RIBBON(USER_ID IN NUMBER);
	
	PROCEDURE UPDATE_CRE;
	--PROCEDURE BILAN;
	PROCEDURE LABEL_UP(p_id in EMAFI_LABEL.ID%TYPE);
	PROCEDURE LABEL_DOWN(p_id in EMAFI_LABEL.ID%TYPE);
	PROCEDURE LABEL_DELETE(p_id in EMAFI_LABEL.ID%TYPE);
	PROCEDURE RUBRIC_UP(p_id in EMAFI_RUBRIC.ID%TYPE);
	PROCEDURE RUBRIC_DOWN(p_id in EMAFI_RUBRIC.ID%TYPE);
	PROCEDURE RUBRIC_DELETE(p_id in EMAFI_RUBRIC.ID%TYPE);
	
    /******************************************************************************
    Utilities function
    ******************************************************************************/
	FUNCTION BOOL_TO_STR_YN(p_bool int) RETURN varchar2;
	FUNCTION GET_OUTPUT_VALUE(name varchar2) RETURN varchar2;
	FUNCTION GET_ACCOUNT_INFORMATIONS(account_number_ varchar2, information_type varchar2) RETURN varchar2;
	FUNCTION IS_BILAN(account_number_ varchar2) RETURN number;
	FUNCTION BLOBTOXML(blob_in IN BLOB) RETURN XMLTYPE;
	/*OPCH3*/
	FUNCTION ALLOTMENT_TITRE(sicovam_ in INTEGER) RETURN VARCHAR2;
	FUNCTION FUNDS_VENTILATION_ACTIF(FUND_ID IN NUMBER,NAV_DATE_  IN DATE, CATEG IN VARCHAR2) RETURN NUMBER;
	FUNCTION FUND_SOUSCRIPTION_RACHAT(FUND_ID_ IN NUMBER,NAV_DATE_ IN DATE,TYPE_PERSONNE_ IN VARCHAR2,TYPE_RESID_ IN VARCHAR2, TYPE_RESULTAT_ IN VARCHAR2) RETURN NUMBER;
	FUNCTION GET_TITRE_SECTOR(SICOVAM_ IN NUMBER, SECTOR_TYPE_ IN VARCHAR2) RETURN VARCHAR2;
	FUNCTION GET_TYPE_CLIENT(THIRD_PARTY IN NUMBER, TYPE_SECTOR IN VARCHAR2) RETURN VARCHAR2;
	FUNCTION GET_FX_RATE(v_funds IN VARCHAR2 , v_date in date , v_currency in number ) RETURN NUMBER ;

END EMAFI;
/
create or replace PACKAGE BODY EMAFI
AS
    -- use a hashtable to store all output parameters
    TYPE MAP_VARCHAR IS TABLE OF VARCHAR2(255) INDEX BY VARCHAR2(20);
    output_params       MAP_VARCHAR;
    
  FUNCTION FUND_ID RETURN NUMBER AS
  BEGIN
    RETURN fund_id_data;
  END;
  
  FUNCTION VL_DATE RETURN VARCHAR2 AS
  BEGIN
    RETURN vl_date_data;
  END;
  
	PROCEDURE INSERT_ONE_ROW(
   FOLIO_ID IN NUMBER, COMPTE IN VARCHAR2, SENS_ IN VARCHAR2, MONTANT IN NUMBER, VALUE_DATE IN NUMBER, DEVISE IN VARCHAR2, CODE_DEVISE IN NUMBER, JOURNAL_ IN VARCHAR2, PIECE_ IN VARCHAR2, COMMENTAIRE_ IN VARCHAR2, 
  COMMENTAIRE_CODE IN VARCHAR2,USER_ID IN NUMBER, TIERS_ID_ IN NUMBER)
	AS
		FOLIO_NAME_ varchar2(50);
		TIERS_NAME_ varchar2(50);
	BEGIN
		-- INCEREMENTING THE INDEX OF THE TWO TEMPORARY VARRAYS
		od_indx := od_indx + 1;

		/************************************* ACCOUNT_POSTING TABLE **********************************************/
		one_posting.extend;
		
		select nvl (
			(SELECT  MAX(B.ACCOUNT_ENTITY_ID) FROM FOLIO A, ACCOUNT_DEAL_RULES B WHERE A.ENTITE = B.ENTITY AND A.IDENT = folio_id),0 )
			INTO one_posting(od_indx).ACCOUNT_ENTITY_ID from dual;
  
		SELECT MAX(ACC_POS_T.ID) INTO one_posting(od_indx).POSTING_TYPE FROM ACCOUNT_POSTING_TYPES ACC_POS_T WHERE NAME like '%OD%';
  
		one_posting(od_indx).GENERATION_DATE := SYSDATE;  
		one_posting(od_indx).POSTING_DATE := to_date('01/01/1904','DD/MM/YYYY') + value_date; /* 01/01/1904 IS THE STARTING DATE (LONG = 0) OF SOPHIS*/
  
		SELECT MAX(A.ACCOUNT_NAME_ID) INTO one_posting(od_indx).ACCOUNT_NAME_ID FROM ACCOUNT_MAP A WHERE A.ACCOUNT_NUMBER = COMPTE; /* THIS QUERY RETURNS A NULL VALUE */

		SELECT MAX((b.ACC_PORTFOLIO)) INTO one_posting(od_indx).ACCOUNTING_BOOK_ID FROM FOLIO A, ACCOUNT_DEAL_RULES B
		WHERE A.ENTITE = B.ENTITY AND A.IDENT = folio_id;

    
    SELECT CRE_STATUS INTO  one_posting(od_indx).STATUS FROM EMAFI_ODSTATUS WHERE ID_STATUS = 1;
    
    
		one_posting(od_indx).AMOUNT := MONTANT;
		one_posting(od_indx).CREDIT_DEBIT := SENS_;
		one_posting(od_indx).ACCOUNT_NUMBER := COMPTE;
		one_posting(od_indx).THIRD_PARTY_ID := TIERS_ID_;
		one_posting(od_indx).COMMENTS := COMMENTAIRE_;
    one_posting(od_indx).CURRENCY:=CODE_DEVISE;
    one_posting(od_indx).Account_currency:= 0;
		/********************* TABLE EMAFI_CRE *****************************************************/

		one_cre.extend;
    
		SELECT NAME INTO FOLIO_NAME_ FROM FOLIO WHERE IDENT = FOLIO_ID;
		SELECT NAME INTO TIERS_NAME_ FROM TIERS WHERE IDENT = TIERS_ID_;
        
		SELECT A.entite INTO one_cre(od_indx).ENTITY_ID FROM folio A WHERE A.ident= FOLIO_ID;
		/*****************************************************************/
    
		SELECT MAX(T.NAME) INTO one_cre(od_indx).ENTITY_NAME FROM FOLIO A ,TIERS T WHERE A.ident =folio_id AND A.entite = T.ident;
  
		one_cre(od_indx).GENERATION_DATE := SYSDATE;
		one_cre(od_indx).POSTING_DATE := to_date('01/01/1904','DD/MM/YYYY') + VALUE_DATE; /* 01/01/1904 IS THE STARTING DATE (LONG = 0) OF SOPHIS*/
    
		SELECT MAX(A.id) INTO one_cre(od_indx).posting_type FROM ACCOUNT_POSTING_TYPES A
		WHERE A.NAME LIKE '%OD%';
    
    SELECT CRE_STATUS INTO  one_cre(od_indx).STATUS FROM EMAFI_ODSTATUS WHERE ID_STATUS = 1;
    
		
        one_cre(od_indx).PTF_ID := FOLIO_ID;
		one_cre(od_indx).PTF_NAME := FOLIO_NAME_;
		one_cre(od_indx).ACCOUNT_NUMBER := compte;
		one_cre(od_indx).SENS := SENS_;
		one_cre(od_indx).AMOUNT := MONTANT;
    one_cre(od_indx).OD_STATUS := 1;
		one_cre(od_indx).JOURNAL := JOURNAL_;
		one_cre(od_indx).PIECE := PIECE_;
    one_cre(od_indx).COMMENTAIRE_DESC := COMMENTAIRE_;
    one_cre(od_indx).COMMENTAIRE_CODE := COMMENTAIRE_CODE;
		one_cre(od_indx).OPERATEUR := USER_ID;
		one_cre(od_indx).TIERS_ID := TIERS_ID_;
		one_cre(od_indx).TIERS_NAME := TIERS_NAME_;
    one_cre(od_indx).CURRENCY:=DEVISE;
		COMMIT;
	EXCEPTION
  WHEN NO_DATA_FOUND THEN 
    DBMS_OUTPUT.PUT_LINE('Donnee introuvable'||sqlerrm);
    one_cre.DELETE;
    one_posting.DELETE;
    od_indx := 0;
    RAISE;
  WHEN OTHERS THEN
		one_cre.DELETE;
		one_posting.DELETE;
		od_indx := 0;
		RAISE;
	END INSERT_ONE_ROW;
  
	 PROCEDURE ADD_OD

AS

indx number;
totalCreditAmt number:=0;
totalDebitAmt number:=0;
num_od number;
BEGIN



IF totalCreditAmt != totalDebitAmt THEN
one_cre.DELETE;
one_posting.DELETE;
od_indx:=0;
COMMIT;
return;
END IF;


num_od := EMAFI_CRE_OD_SEQ.NEXTVAL;
FOR indx in 1..one_posting.COUNT LOOP

  one_posting(indx).ID := ACCT_POSTING_SEQ.NEXTVAL;
  one_cre(indx).ID_CRE := EMAFI_CRE_SEQ.NEXTVAL;
  one_cre(indx).ID_POSTING := one_posting(indx).ID;
  one_cre(indx).OD_NUM := num_od;
  INSERT INTO EMAFI_CRE VALUES one_cre(indx);
   
  
  INSERT INTO ACCOUNT_POSTING VALUES one_posting(indx);
  
END LOOP;

one_cre.DELETE;
one_posting.DELETE;
od_indx:=0;
COMMIT;


EXCEPTION

WHEN OTHERS THEN
ROLLBACK;
one_cre.DELETE;
one_posting.DELETE;
od_indx :=0;
RAISE;


END;

	PROCEDURE DELETE_OD(OD_NUM_ IN NUMBER)
	AS 
		TYPE IDS IS TABLE OF EMAFI_CRE%rowtype INDEX BY PLS_INTEGER;
		TYPE IDS_POS IS TABLE OF ACCOUNT_POSTING%rowtype INDEX BY PLS_INTEGER;

		tmpRow EMAFI_CRE%ROWTYPE;
		odd_ids IDS; 
		odd_ids_pos IDS_POS;
		maxID_CRE number;
		maxID_POSTING number;
		indx number:=1;

		comm_test varchar2(50);
	BEGIN
		output_params('SqlRowCount') := 0;
    
		FOR r in (SELECT * FROM EMAFI_CRE WHERE OD_NUM = OD_NUM_ ORDER BY ID_CRE) LOOP
			odd_ids(indx) := r;
			indx := indx + 1;
		END LOOP;
		FOR indx IN 1..odd_ids.COUNT LOOP
			SELECT * INTO odd_ids_pos(indx) FROM ACCOUNT_POSTING AP WHERE AP.ID = odd_ids(indx).ID_POSTING;
		END LOOP;

		output_params('SqlRowCount') := odd_ids.COUNT;

		/************************ TABLE : EMAFI_CRE **************************************/
		FOR indx IN 1..odd_ids.COUNT LOOP
			odd_ids(indx).id_cre := EMAFI_CRE_SEQ.NEXTVAL;
			odd_ids(indx).id_posting := SEQACCOUNT.NEXTVAL;

			IF odd_ids(indx).SENS = 'C' THEN
				odd_ids(indx).SENS := 'D';
			ELSE
				odd_ids(indx).SENS := 'C';
			END IF;
			odd_ids(indx).OD_STATUS := -1;
			odd_ids(indx).COMMENTAIRE_DESC := 'ANNUL-OD ' || odd_ids(indx).COMMENTAIRE_DESC;
			INSERT INTO EMAFI_CRE VALUES odd_ids(indx);
		END LOOP;
  
		UPDATE EMAFI_CRE SET OD_STATUS = -1 WHERE OD_NUM = OD_NUM_; 
		/************************ TABLE : ACCOUNT_POSTING **************************************/
		FOR indx IN 1..odd_ids_pos.COUNT LOOP
			SELECT MAX(ID) INTO maxID_POSTING FROM ACCOUNT_POSTING;
			odd_ids_pos(indx).id := maxID_POSTING + 1;

			IF odd_ids_pos(indx).CREDIT_DEBIT = 'C' THEN
				odd_ids_pos(indx).CREDIT_DEBIT := 'D';
			ELSE
				odd_ids_pos(indx).CREDIT_DEBIT := 'C';
			END IF;
				odd_ids_pos(indx).COMMENTS := 'ANNUL-OD ' || odd_ids_pos(indx).COMMENTS;
			INSERT INTO ACCOUNT_POSTING VALUES odd_ids_pos(indx);
		END LOOP;
		COMMIT;
	END;
	
	PROCEDURE INSTALL_CUSTOMISED_RIBBON(USER_ID IN NUMBER)
	AS
		blob_in	blob;
		x		xmltype;
		nodes	VARCHAR2(2000);
		xmlTest xmltype;
	BEGIN

		select u.value into blob_in  from userprefslr u where u.userid = USER_ID and utl_raw.cast_to_varchar2(DBMS_LOB.SUBSTR(u.VALUE, 2000, 1)) like '%<ribbon%';  
		x := BLOBTOXML(blob_in); 

		select extract(x,'/*[local-name()=''ribbon'']/*[local-name()=''tabs'']/*[local-name()=''tab''][@name="EMAFI"]') into xmlTest from dual;

		if xmlTest is not null then
		  return;
		end if;

		nodes := '<tab keys="EM" name="EMAFI"><group name = "Generation des Etats Comptables">
		<command idQ = "GENERATE_REPORTS_RIBBON" name = "Generation des rapports"/>
		<command idQ = "CONFIGURATION_RIBBON" name = "Configuration"/>
			</group>
			<group name = "Gestion des Operations Diverses">
		<command idQ = "GESTION_OD_RIBBON" name = "Gestion des OD"/>
		<command idQ = "CONFIGURATION_OD_RIBBON" name = "Configuration"/>
			</group>
		</tab>';
		-- the action

		select insertchildxml(x,'/*[local-name()=''ribbon'']/*[local-name()=''tabs'']','tab',xmltype(nodes)) into x from dual;
		  
		select updatexml(x, '//ribbon/@xmlns', 'http://www.sophis.net/ribbon.xsd') into x from dual;

		update userprefslr u set u.value = x.getblobval(873)
		where u.userid = USER_ID and utl_raw.cast_to_varchar2(DBMS_LOB.SUBSTR(u.VALUE, 2000, 1)) like '%<ribbon%';

		COMMIT;
		
		EXCEPTION
			when no_data_found then
				DBMS_OUTPUT.PUT_LINE('User preference not found');
	END;

	PROCEDURE UNINSTALL_CUSTOMISED_RIBBON(USER_ID IN NUMBER)
	AS
		blob_in	blob;
		x		xmltype;
	BEGIN
		select u.value into blob_in  from userprefslr u where u.userid = USER_ID and utl_raw.cast_to_varchar2(DBMS_LOB.SUBSTR(u.VALUE, 2000, 1)) like '%<ribbon%';  

		x := BLOBTOXML(blob_in); 

		SELECT deletexml(x,'/*[local-name()=''ribbon'']/*[local-name()=''tabs'']/*[local-name()=''tab''][@name="EMAFI"]') into x from dual;
		  
		update userprefslr u set u.value = x.getblobval(873)
		where u.userid = USER_ID and utl_raw.cast_to_varchar2(DBMS_LOB.SUBSTR(u.VALUE, 2000, 1)) like '%<ribbon%';

		COMMIT;
	EXCEPTION
		when no_data_found then
			NULL;
	END;

	PROCEDURE UPDATE_CRE
	AS
	BEGIN
		INSERT INTO EMAFI_CRE(ID_CRE, ID_POSTING, ID_TRADE, TYPE_TRADE, ENTITY_ID, ENTITY_NAME, ENTITY_DESC, PTF_ID, PTF_NAME, PTF_DESC,
			ID_TITRE, ACCOUNT_NUMBER, CURRENCY, SENS, QTE, AMOUNT, STATUS, POSTING_DATE, GENERATION_DATE, CRE_DATE)
		SELECT EMAFI_CRE_SEQ.NEXTVAL, AP.ID, AP.TRADE_ID, H.TYPE, H.ENTITE, T.NAME, NULL, P.OPCVM, F.NAME, NULL,
			P.SICOVAM, AP.ACCOUNT_NUMBER, DEVISE_TO_STR(AP.ACCOUNT_CURRENCY), AP.CREDIT_DEBIT, AP.QUANTITY, AP.AMOUNT, AP.STATUS, AP.POSTING_DATE, AP.GENERATION_DATE, SYSDATE
			FROM ACCOUNT_POSTING AP
			JOIN HISTOMVTS H ON H.REFCON = AP.TRADE_ID
			JOIN TIERS T ON T.IDENT = H.ENTITE
			JOIN POSITION P ON P.MVTIDENT = H.MVTIDENT
			JOIN FOLIO F ON F.IDENT = P.OPCVM
			WHERE AP.ID NOT IN (SELECT ID_POSTING FROM EMAFI_CRE)
				--AND H.TYPE IN (1, 141)
			;
		UPDATE (
			SELECT EC.STATUS OLD_STATUS, A.STATUS NEW_STATUS, EC.ID_POSTING
			FROM EMAFI_CRE EC
				JOIN ACCOUNT_POSTING A ON A.ID = EC.ID_POSTING
			WHERE EC.OD_NUM IS NULL
				AND A.STATUS <> EC.STATUS
		) SET OLD_STATUS = NEW_STATUS;
	END UPDATE_CRE;
	
	PROCEDURE LABEL_UP(p_id in EMAFI_LABEL.ID%TYPE)
	AS
		LL EMAFI_LABEL%ROWTYPE;
		next_row_count int;
	BEGIN
		output_params('SqlRowCount') := 0;
		SELECT * INTO LL FROM EMAFI_LABEL WHERE ID = p_id;
		SELECT COUNT(*)
			into next_row_count
		FROM EMAFI_LABEL WHERE ID_RUBRIC = LL.ID_RUBRIC AND REPORT_ORDER = LL.REPORT_ORDER - 1;
		if (next_row_count > 0) then
		begin
			UPDATE EMAFI_LABEL SET REPORT_ORDER = 0 WHERE ID = LL.ID; -- A = 0
			UPDATE EMAFI_LABEL SET REPORT_ORDER = LL.REPORT_ORDER WHERE ID_RUBRIC = LL.ID_RUBRIC AND REPORT_ORDER = LL.REPORT_ORDER - 1; -- B = A
			UPDATE EMAFI_LABEL SET REPORT_ORDER = LL.REPORT_ORDER - 1 WHERE ID = LL.ID; -- A = B
			output_params('SqlRowCount') := SQL%ROWCOUNT;
		end;
		end if;
	END;
	
	PROCEDURE LABEL_DOWN(p_id in EMAFI_LABEL.ID%TYPE)
	AS
		LL EMAFI_LABEL%ROWTYPE;
		next_row_count int;
	BEGIN
		output_params('SqlRowCount') := 0;
		SELECT * INTO LL FROM EMAFI_LABEL WHERE ID = p_id;
		SELECT COUNT(*)
			into next_row_count
		FROM EMAFI_LABEL WHERE ID_RUBRIC = LL.ID_RUBRIC AND REPORT_ORDER = LL.REPORT_ORDER + 1;
		if (next_row_count > 0) then
		begin
			UPDATE EMAFI_LABEL SET REPORT_ORDER = 0 WHERE ID = LL.ID; -- A = 0
			UPDATE EMAFI_LABEL SET REPORT_ORDER = LL.REPORT_ORDER WHERE ID_RUBRIC = LL.ID_RUBRIC AND REPORT_ORDER = LL.REPORT_ORDER + 1; -- B = A
			UPDATE EMAFI_LABEL SET REPORT_ORDER = LL.REPORT_ORDER + 1 WHERE ID = LL.ID; -- A = B
			output_params('SqlRowCount') := SQL%ROWCOUNT;
		end;
		end if;
	END;
	
	PROCEDURE LABEL_DELETE(p_id in EMAFI_LABEL.ID%TYPE)
	AS
		LL EMAFI_LABEL%ROWTYPE;
		rc int;
	BEGIN
		output_params('SqlRowCount') := 0;
		SELECT * INTO LL FROM EMAFI_LABEL WHERE ID = p_id;
		DELETE EMAFI_LABEL WHERE ID = LL.ID;
		output_params('SqlRowCount') := SQL%ROWCOUNT;
		UPDATE EMAFI_LABEL SET REPORT_ORDER = REPORT_ORDER - 1 WHERE ID_RUBRIC = LL.ID AND REPORT_ORDER > LL.REPORT_ORDER;
	END;
	
	PROCEDURE RUBRIC_UP(p_id in EMAFI_RUBRIC.ID%TYPE)
	AS
		LL EMAFI_RUBRIC%ROWTYPE;
		next_row_count int;
	BEGIN
		output_params('SqlRowCount') := 0;
		SELECT * INTO LL FROM EMAFI_RUBRIC WHERE ID = p_id;
		SELECT COUNT(*)
			into next_row_count
		FROM EMAFI_RUBRIC WHERE REPORT_TYPE = LL.REPORT_TYPE AND REPORT_ORDER = LL.REPORT_ORDER - 1;
		if (next_row_count > 0) then
		begin
			UPDATE EMAFI_RUBRIC SET REPORT_ORDER = 0 WHERE ID = LL.ID; -- A = 0
			UPDATE EMAFI_RUBRIC SET REPORT_ORDER = LL.REPORT_ORDER WHERE REPORT_TYPE = LL.REPORT_TYPE AND REPORT_ORDER = LL.REPORT_ORDER - 1; -- B = A
			UPDATE EMAFI_RUBRIC SET REPORT_ORDER = LL.REPORT_ORDER - 1 WHERE ID = LL.ID; -- A = B
			output_params('SqlRowCount') := SQL%ROWCOUNT;
		end;
		end if;
	END;
	
	PROCEDURE RUBRIC_DOWN(p_id in EMAFI_RUBRIC.ID%TYPE)
	AS
		LL EMAFI_RUBRIC%ROWTYPE;
		next_row_count int;
	BEGIN
		output_params('SqlRowCount') := 0;
		SELECT * INTO LL FROM EMAFI_RUBRIC WHERE ID = p_id;
		SELECT COUNT(*)
			into next_row_count
		FROM EMAFI_RUBRIC WHERE REPORT_TYPE = LL.REPORT_TYPE AND REPORT_ORDER = LL.REPORT_ORDER + 1;
		if (next_row_count > 0) then
		begin
			UPDATE EMAFI_RUBRIC SET REPORT_ORDER = 0 WHERE ID = LL.ID; -- A = 0
			UPDATE EMAFI_RUBRIC SET REPORT_ORDER = LL.REPORT_ORDER WHERE REPORT_TYPE = LL.REPORT_TYPE AND REPORT_ORDER = LL.REPORT_ORDER + 1; -- B = A
			UPDATE EMAFI_RUBRIC SET REPORT_ORDER = LL.REPORT_ORDER + 1 WHERE ID = LL.ID; -- A = B
			output_params('SqlRowCount') := SQL%ROWCOUNT;
		end;
		end if;
	END;
	
	PROCEDURE RUBRIC_DELETE(p_id in EMAFI_RUBRIC.ID%TYPE)
	AS
		LL EMAFI_RUBRIC%ROWTYPE;
		rc int;
	BEGIN
		output_params('SqlRowCount') := 0;
		SELECT COUNT(*)
			into rc
		FROM EMAFI_LABEL WHERE ID_RUBRIC = p_id;
		if (rc = 0) then
			SELECT * INTO LL FROM EMAFI_RUBRIC WHERE ID = p_id;
			DELETE EMAFI_RUBRIC WHERE ID = LL.ID;
			output_params('SqlRowCount') := SQL%ROWCOUNT;
			UPDATE EMAFI_RUBRIC SET REPORT_ORDER = REPORT_ORDER - 1 WHERE REPORT_TYPE = LL.REPORT_TYPE AND REPORT_ORDER > LL.REPORT_ORDER;
		end if;
	END;
	
    /******************************************************************************
    Utilities function
    ******************************************************************************/
    FUNCTION BOOL_TO_STR_YN(p_bool int)
    RETURN varchar2
    AS
        result varchar2(1);
    BEGIN
        result := CASE p_bool WHEN 1 THEN 'Y' ELSE 'N' END;
        return result;
    END;
	
    FUNCTION GET_OUTPUT_VALUE(name varchar2)
    RETURN varchar2 AS
    BEGIN
        return output_params(name);
    exception
        when no_data_found then
            return null;
    END;
    
        FUNCTION GET_ACCOUNT_INFORMATIONS(account_number_ varchar2, information_type varchar2)
    RETURN varchar2 AS
    account_information varchar2(100);
    BEGIN
      IF information_type like '%CATEGORY%' THEN
        SELECT DECODE(ACCOUNT_TYPE,0,NULL,1,'Asset',2,'Liability',3,'Expenses',4,'Revenue',5,'Off Balance Sheet',6,'Third Party Account',7,'Revenue/Expenses',8,'Off Balance Sheet Credit',9,'Off Balance Sheet Debit',10,'Technical Account',NULL) 
        INTO account_information FROM ACCOUNT_NAME WHERE ID IN (SELECT MAX(ACCOUNT_NAME_ID) FROM ACCOUNT_MAP AM WHERE AM.ACCOUNT_NUMBER = account_number_);
      END IF;
      IF information_type like '%CLASSIFICATION%' THEN
        SELECT 
        CASE 
          WHEN ACCOUNT_TYPE IN (0,1,2,3,4,6,7) THEN 'BILAN'
          WHEN ACCOUNT_TYPE IN (5,8,9,10) THEN 'HORS BILAN'
        ELSE NULL 
        END
        INTO account_information FROM ACCOUNT_NAME WHERE ID IN (SELECT MAX(ACCOUNT_NAME_ID) FROM ACCOUNT_MAP WHERE ACCOUNT_NUMBER = account_number_);
        END IF;
        return account_information;
    END;
    
    FUNCTION IS_BILAN(account_number_ varchar2)
    RETURN number as
   -- is_bilan boolean;
    BEGIN
    IF
      GET_ACCOUNT_INFORMATIONS(account_number_,'CLASSIFICATION') = 'BILAN' THEN
       -- is_bilan := true;
       return 1;
    END IF;
    --is_bilan := false;
    return 0;
    END;
	
	 FUNCTION BLOBTOXML(blob_in IN BLOB) RETURN XMLTYPE
    AS
      v_clob    CLOB;
      v_varchar VARCHAR2(32767);
      v_start   PLS_INTEGER := 1;
      v_buffer  PLS_INTEGER := 32767;
      x         xmltype;
    BEGIN
      DBMS_LOB.CREATETEMPORARY(v_clob, TRUE);
      FOR i IN 1 .. CEIL(DBMS_LOB.GETLENGTH(blob_in) / v_buffer) LOOP
          v_varchar := UTL_RAW.CAST_TO_VARCHAR2(DBMS_LOB.SUBSTR(blob_in, v_buffer, v_start));

          -- COMMENT this IF and you will get the error in parsing xml
          if (i = CEIL(DBMS_LOB.GETLENGTH(blob_in) / v_buffer) and SUBSTR(v_varchar,-1) = CHR(0)) then
              v_varchar := substr(v_varchar, 1, length(v_varchar) - 1);
          end if;

          DBMS_LOB.WRITEAPPEND(v_clob, LENGTH(v_varchar), v_varchar);
        
          v_start := v_start + v_buffer;
      END LOOP;
    
      x := xmltype.createxml(v_clob);
      return x;
  END;
  
  FUNCTION ALLOTMENT_TITRE(sicovam_ in INTEGER) RETURN VARCHAR2
  AS
  allotement_titre VARCHAR2(50);
  BEGIN
    SELECT EP.DESCRIPTION INTO allotement_titre FROM EMAFI_PARAMETRAGE EP, TITRES T WHERE  T.SICOVAM = sicovam_ AND
    EP.CODE = TO_CHAR(F_INSTRALLOTMENT(T.SICOVAM)) AND EP.CATEGORIE LIKE '%Allotement_Titre%';
    RETURN allotement_titre;
  END;
  
  
FUNCTION FUNDS_VENTILATION_ACTIF(FUND_ID IN NUMBER,NAV_DATE_ 
  IN DATE, CATEG IN VARCHAR2) RETURN NUMBER
  AS
  OUT_PUT NUMBER;
  cNAME VARCHAR2(40);
  gNAME VARCHAR2(40);
  BEGIN
          
  
  SELECT 
  CASE CATEG
    WHEN 'IMMOB' THEN (SELECT SUM(FHP.ASSET_VALUE * GET_FX_RATE(FHP.CODE ,FHP.NAV_DATE ,FHP.CURRENCY) ) FROM FUND_HISTORY_POSITIONS FHP
          WHERE ALLOTMENT_TITRE(FHP.INSTRUMENT) = 'IMMOB' AND
          FHP.CODE = FUND_ID AND
          FHP.NAV_DATE = NAV_DATE_)
    WHEN 'ACT_COTEE' THEN
          (SELECT SUM(FHP.ASSET_VALUE * GET_FX_RATE(FHP.CODE ,FHP.NAV_DATE ,FHP.CURRENCY)) FROM FUND_HISTORY_POSITIONS FHP 
          WHERE ALLOTMENT_TITRE(FHP.INSTRUMENT) = 'ACTION' AND
          FHP.CODE = FUND_ID AND
          FHP.NAV_DATE = NAV_DATE_ AND
          GET_TITRE_SECTOR(FHP.INSTRUMENT, 'COTATION') = 'C')

    WHEN 'OBL_COTEE_GAR' THEN
          (SELECT SUM(FHP.ASSET_VALUE * GET_FX_RATE(FHP.CODE ,FHP.NAV_DATE ,FHP.CURRENCY)) FROM FUND_HISTORY_POSITIONS FHP
          
          WHERE ALLOTMENT_TITRE(FHP.INSTRUMENT) = 'OBLIGATION' AND
          FHP.CODE = FUND_ID AND
          FHP.NAV_DATE = NAV_DATE_ AND
           GET_TITRE_SECTOR(FHP.INSTRUMENT, 'COTATION') = 'C' AND  GET_TITRE_SECTOR(FHP.INSTRUMENT, 'GARANTIE')= 'O')
    WHEN 'OBL_COTEE_NGAR' THEN
           (SELECT SUM(FHP.ASSET_VALUE * GET_FX_RATE(FHP.CODE ,FHP.NAV_DATE ,FHP.CURRENCY)) FROM FUND_HISTORY_POSITIONS FHP
          
          WHERE ALLOTMENT_TITRE(FHP.INSTRUMENT) = 'OBLIGATION' AND
          FHP.CODE = FUND_ID AND
          FHP.NAV_DATE = NAV_DATE_ AND
           GET_TITRE_SECTOR(FHP.INSTRUMENT, 'COTATION') ='C' AND ( GET_TITRE_SECTOR(FHP.INSTRUMENT, 'GARANTIE') = 'N' or GET_TITRE_SECTOR(FHP.INSTRUMENT, 'GARANTIE') IS NULL)
          )
    WHEN 'ACT_NCOTEE' THEN
          (SELECT SUM(FHP.ASSET_VALUE * GET_FX_RATE(FHP.CODE ,FHP.NAV_DATE ,FHP.CURRENCY)) FROM FUND_HISTORY_POSITIONS FHP
          
          WHERE ALLOTMENT_TITRE(FHP.INSTRUMENT) = 'ACTION' AND
          FHP.CODE = FUND_ID AND
          FHP.NAV_DATE = NAV_DATE_ AND
          (GET_TITRE_SECTOR(FHP.INSTRUMENT, 'COTATION') ='N' OR GET_TITRE_SECTOR(FHP.INSTRUMENT, 'COTATION') IS NULL)
          )
    WHEN 'OBL_NCOTEE_GAR' THEN
          (SELECT SUM(FHP.ASSET_VALUE * GET_FX_RATE(FHP.CODE ,FHP.NAV_DATE ,FHP.CURRENCY)) FROM FUND_HISTORY_POSITIONS FHP
          WHERE ALLOTMENT_TITRE(FHP.INSTRUMENT) = 'OBLIGATION' AND
          FHP.CODE = FUND_ID AND
          FHP.NAV_DATE = NAV_DATE_ AND
          (GET_TITRE_SECTOR(FHP.INSTRUMENT, 'COTATION') ='N' OR GET_TITRE_SECTOR(FHP.INSTRUMENT, 'COTATION') IS NULL) AND
          GET_TITRE_SECTOR(FHP.INSTRUMENT, 'GARANTIE') = 'O'
          )
    WHEN 'OBL_NCOTEE_NGAR' THEN
          (SELECT SUM(FHP.ASSET_VALUE * GET_FX_RATE(FHP.CODE ,FHP.NAV_DATE ,FHP.CURRENCY)) FROM FUND_HISTORY_POSITIONS FHP
          WHERE ALLOTMENT_TITRE(FHP.INSTRUMENT) = 'OBLIGATION' AND
          FHP.CODE = FUND_ID AND
          FHP.NAV_DATE = NAV_DATE_ AND
          (GET_TITRE_SECTOR(FHP.INSTRUMENT, 'COTATION') ='N' OR GET_TITRE_SECTOR(FHP.INSTRUMENT, 'COTATION') IS NULL) AND
          (GET_TITRE_SECTOR(FHP.INSTRUMENT, 'GARANTIE') = 'N' OR GET_TITRE_SECTOR(FHP.INSTRUMENT, 'GARANTIE') IS NULL)
          )        
    WHEN 'TCN_CD' THEN 
          (SELECT SUM(FHP.ASSET_VALUE * GET_FX_RATE(FHP.CODE ,FHP.NAV_DATE ,FHP.CURRENCY)) FROM FUND_HISTORY_POSITIONS FHP
          WHERE ALLOTMENT_TITRE(FHP.INSTRUMENT) = 'CD' AND
          FHP.CODE = FUND_ID AND
          FHP.NAV_DATE = NAV_DATE_)
    WHEN 'TCN_BSF' THEN
          (SELECT SUM(FHP.ASSET_VALUE) FROM FUND_HISTORY_POSITIONS FHP
          WHERE ALLOTMENT_TITRE(FHP.INSTRUMENT) = 'BSF' AND
          FHP.CODE = FUND_ID AND
          FHP.NAV_DATE = NAV_DATE_)
    
    WHEN 'TCN_BT' THEN
          (SELECT SUM(FHP.ASSET_VALUE * GET_FX_RATE(FHP.CODE ,FHP.NAV_DATE ,FHP.CURRENCY)) FROM FUND_HISTORY_POSITIONS FHP
          WHERE ALLOTMENT_TITRE(FHP.INSTRUMENT) = 'BT' AND
          FHP.CODE = FUND_ID AND
          FHP.NAV_DATE = NAV_DATE_) 
    
    WHEN 'TCN' THEN
          (SELECT NVL(FUNDS_VENTILATION_ACTIF(FUND_ID,NAV_DATE_,'TCN_CD'),0) +  NVL(FUNDS_VENTILATION_ACTIF(FUND_ID,NAV_DATE_,'TCN_BSF'),0)+ NVL(FUNDS_VENTILATION_ACTIF(FUND_ID,NAV_DATE_,'TCN_BT'),0)
          FROM DUAL)
          
    WHEN 'OPCVM_SICAV' THEN
          (SELECT SUM(FHP.ASSET_VALUE * GET_FX_RATE(FHP.CODE ,FHP.NAV_DATE ,FHP.CURRENCY)) FROM FUND_HISTORY_POSITIONS FHP  ,titres a , fund_legal_form f 
          WHERE --ALLOTMENT_TITRE(FHP.INSTRUMENT) = 'SICAV' AND
                      
         fhp.instrument = a.sicovam 
         and a.codesj2  = f.id 
         
         and f.name = 'SICAV'
         and  FHP.CODE = FUND_ID AND
          FHP.NAV_DATE = NAV_DATE_
          )
    
    WHEN 'OPCVM_FCP' THEN
          (SELECT SUM(FHP.ASSET_VALUE * GET_FX_RATE(FHP.CODE ,FHP.NAV_DATE ,FHP.CURRENCY)) FROM FUND_HISTORY_POSITIONS FHP  ,titres a , fund_legal_form f 
          WHERE --ALLOTMENT_TITRE(FHP.INSTRUMENT) = 'OPCVM_FCP' AND
                      
         fhp.instrument = a.sicovam 
         and a.codesj2  = f.id 
         
         and f.name = 'FCP'
         and  FHP.CODE = FUND_ID AND
          FHP.NAV_DATE = NAV_DATE_
          )          
    WHEN 'DAT' THEN
          (SELECT SUM(FHP.ASSET_VALUE * GET_FX_RATE(FHP.CODE ,FHP.NAV_DATE ,FHP.CURRENCY)) FROM FUND_HISTORY_POSITIONS FHP
          WHERE ALLOTMENT_TITRE(FHP.INSTRUMENT) = 'DAT' AND
          FHP.CODE = FUND_ID AND
          FHP.NAV_DATE = NAV_DATE_)

    WHEN 'REPO' THEN
          (SELECT SUM(FHP.ASSET_VALUE) FROM FUND_HISTORY_POSITIONS FHP
          WHERE ALLOTMENT_TITRE(FHP.INSTRUMENT) = 'REPO/RREPO' AND
          FHP.CODE = FUND_ID AND
          FHP.NAV_DATE = NAV_DATE_
          and sign (fhp.quantity) = -1
          )

    WHEN 'RREPO' THEN
          (SELECT SUM(FHP.ASSET_VALUE * GET_FX_RATE(FHP.CODE ,FHP.NAV_DATE ,FHP.CURRENCY)) FROM FUND_HISTORY_POSITIONS FHP
          WHERE ALLOTMENT_TITRE(FHP.INSTRUMENT) = 'REPO/RREPO' AND
          FHP.CODE = FUND_ID AND
          FHP.NAV_DATE = NAV_DATE_
          and sign (fhp.quantity) = 1
          )

    WHEN 'PRET' THEN
          (SELECT SUM(FHP.ASSET_VALUE * GET_FX_RATE(FHP.CODE ,FHP.NAV_DATE ,FHP.CURRENCY)) FROM FUND_HISTORY_POSITIONS FHP
          WHERE ALLOTMENT_TITRE(FHP.INSTRUMENT) = 'PRET/EMPRUNT' AND
          FHP.CODE = FUND_ID AND
          FHP.NAV_DATE = NAV_DATE_
          and sign (fhp.quantity) = -1
          )
          
    WHEN 'EMPRUNT' THEN
          (SELECT SUM(FHP.ASSET_VALUE) FROM FUND_HISTORY_POSITIONS FHP
          WHERE ALLOTMENT_TITRE(FHP.INSTRUMENT) = 'PRET/EMPRUNT' AND
          FHP.CODE = FUND_ID AND
          FHP.NAV_DATE = NAV_DATE_
          and sign (fhp.quantity) = 1
          )

    WHEN 'TITRES_AUTRES' THEN
          (SELECT SUM(FHP.ASSET_VALUE * GET_FX_RATE(FHP.CODE ,FHP.NAV_DATE ,FHP.CURRENCY)) FROM FUND_HISTORY_POSITIONS FHP
          WHERE ALLOTMENT_TITRE(FHP.INSTRUMENT) = 'AUTRES' OR ALLOTMENT_TITRE(FHP.INSTRUMENT) IS NULL  AND
          FHP.CODE = FUND_ID AND
          FHP.NAV_DATE = NAV_DATE_)
          
    WHEN 'BANQUE' THEN
         /* (SELECT SUM(FHP.CASH) FROM FUND_HISTORY_POSITIONS FHP
          WHERE
          FHP.CODE = FUND_ID AND
          FHP.NAV_DATE = NAV_DATE_)*/
		  (SELECT FHG.CASH_ADJUST FROM FUND_HISTORY_GLOBAL FHG
		  WHERE
		  FHG.CODE = FUND_ID AND
		  FHG.NAV_DATE = NAV_DATE_)

    WHEN 'ACHAT_ENCOURS' THEN
          (SELECT SUM(CASE WHEN FHP.UNSETTLED_CASH < 0 THEN FHP.UNSETTLED_CASH  ELSE 0 END * GET_FX_RATE(FHP.CODE ,FHP.NAV_DATE ,FHP.CURRENCY))  FROM FUND_HISTORY_POSITIONS FHP
          WHERE   
          FHP.CODE = FUND_ID AND
          FHP.NAV_DATE = NAV_DATE_)
          
    WHEN 'VENTE_ENCOURS' THEN
          (SELECT SUM(CASE WHEN FHP.UNSETTLED_CASH > 0 THEN FHP.UNSETTLED_CASH ELSE 0 END  * GET_FX_RATE(FHP.CODE ,FHP.NAV_DATE ,FHP.CURRENCY))  FROM FUND_HISTORY_POSITIONS FHP
          WHERE   
          FHP.CODE = FUND_ID AND
          FHP.NAV_DATE = NAV_DATE_)

    WHEN 'FDG' THEN
          (SELECT SUM(FHG.FEES) FROM FUND_HISTORY_GLOBAL FHG
          WHERE
          FHG.CODE = FUND_ID AND
          FHG.NAV_DATE = NAV_DATE_)
    
    WHEN 'DETTES' THEN
          (
          ---Banque négative seulement
          SELECT ABS(decode (sign (nvl(EMAFI.FUNDS_VENTILATION_ACTIF(FUND_ID,NAV_DATE_,'BANQUE'),0)) ,-1,
          nvl(EMAFI.FUNDS_VENTILATION_ACTIF(FUND_ID,NAV_DATE_,'BANQUE'),0) ,0  )) + 
          
          
          ABS(nvl(EMAFI.FUNDS_VENTILATION_ACTIF(FUND_ID,NAV_DATE_,'ACHAT_ENCOURS'),0)) + 
           ABS(nvl(EMAFI.FUNDS_VENTILATION_ACTIF(FUND_ID,NAV_DATE_,'FDG'),0))
           
      
           FROM DUAL
         
           )
    
    WHEN 'NBR_PARTS' THEN
          (SELECT SUM(FHG.NUMBER_SHARES) FROM FUND_HISTORY_GLOBAL FHG
          WHERE
          FHG.CODE = FUND_ID AND
          FHG.NAV_DATE = NAV_DATE_)          
          
    --- ajouté 
       WHEN 'CREANCES_AUTRES' THEN
          (SELECT ABS(nvl(EMAFI.FUNDS_VENTILATION_ACTIF(FUND_ID,NAV_DATE_,'VENTE_ENCOURS'),0)) + 
          
          nvl(EMAFI.FUNDS_VENTILATION_ACTIF(FUND_ID,NAV_DATE_,'REPO'),0) + 
           nvl(EMAFI.FUNDS_VENTILATION_ACTIF(FUND_ID,NAV_DATE_,'RREPO'),0) +
            nvl(EMAFI.FUNDS_VENTILATION_ACTIF(FUND_ID,NAV_DATE_,'PRET'),0) +
             nvl(EMAFI.FUNDS_VENTILATION_ACTIF(FUND_ID,NAV_DATE_,'EMPRUNT'),0)
           FROM DUAL)
		   
	
	 -- OPC6
    
    WHEN 'ACTIF_NET' THEN
          (SELECT FHG.TOTAL_GAV_THEO FROM FUND_HISTORY_GLOBAL FHG
          WHERE
          FHG.CODE = FUND_ID AND
          FHG.NAV_DATE = NAV_DATE_)   

	WHEN 'VL' THEN
          (SELECT FHG.SHARE_PRICE FROM FUND_HISTORY_GLOBAL FHG
          WHERE
          FHG.CODE = FUND_ID AND
          FHG.NAV_DATE = NAV_DATE_) 
	
	WHEN 'SOUSCRIPTION' THEN
		(SELECT sum(FP.AMOUNT) FROM FUND_PURCHASE FP
          WHERE
          FP.FUND = FUND_ID AND
          FP.value_date = NAV_DATE
          and SIGN(FP.NUMBER_SHARES) = 1 )
	
	
	WHEN 'RACHAT' THEN
		(SELECT sum(FP.AMOUNT) FROM FUND_PURCHASE FP
          WHERE
          FP.FUND = FUND_ID AND
          FP.value_date = NAV_DATE
          and SIGN(FP.NUMBER_SHARES) = -1 )
		  
		  
  END
  INTO OUT_PUT FROM DUAL;
 
  RETURN OUT_PUT;
  END;
  
  
  
  FUNCTION FUND_SOUSCRIPTION_RACHAT(FUND_ID_ IN NUMBER,NAV_DATE_ IN DATE,TYPE_PERSONNE_ IN VARCHAR2,TYPE_RESID_ IN VARCHAR2, TYPE_RESULTAT_ IN VARCHAR2) 
      RETURN NUMBER
      AS
      sResult NUMBER;

      tPerson VARCHAR2(140);
      tResident VARCHAR2(140);
      BEGIN
        
        
       SELECT 
        CASE TYPE_RESULTAT_
        WHEN 'SOUS_NBRE' THEN 
        (SELECT COUNT(*) FROM FUND_PURCHASE FP
        WHERE FP.FUND = FUND_ID_ AND
        FP.VALUE_DATE = NAV_DATE_ AND
        SIGN(FP.NUMBER_SHARES) = 1 AND
        (GET_TYPE_CLIENT(FP.THIRD1,'PERSONNE') = TYPE_PERSONNE_ OR ((GET_TYPE_CLIENT(FP.THIRD1,'PERSONNE') IS NULL OR GET_TYPE_CLIENT(FP.THIRD1,'PERSONNE') NOT IN ('P','M')) AND TYPE_PERSONNE_ = 'M')) AND
        (GET_TYPE_CLIENT(FP.THIRD1,'RESIDENCE') = TYPE_RESID_ OR ((GET_TYPE_CLIENT(FP.THIRD1,'RESIDENCE') IS NULL OR GET_TYPE_CLIENT(FP.THIRD1,'RESIDENCE') NOT IN ('R','NR'))
        AND TYPE_RESID_ = 'NR'))
        )
        WHEN 'SOUS_QTE' THEN
        (SELECT SUM(FP.NUMBER_SHARES) FROM FUND_PURCHASE FP
        WHERE FP.FUND = FUND_ID_ AND
        FP.VALUE_DATE = NAV_DATE_ AND
        SIGN(FP.NUMBER_SHARES) = 1 AND
        (GET_TYPE_CLIENT(FP.THIRD1,'PERSONNE') = TYPE_PERSONNE_ OR ((GET_TYPE_CLIENT(FP.THIRD1,'PERSONNE') IS NULL OR GET_TYPE_CLIENT(FP.THIRD1,'PERSONNE') NOT IN ('P','M')) AND TYPE_PERSONNE_ = 'M')) AND
        (GET_TYPE_CLIENT(FP.THIRD1,'RESIDENCE') = TYPE_RESID_ OR ((GET_TYPE_CLIENT(FP.THIRD1,'RESIDENCE') IS NULL OR GET_TYPE_CLIENT(FP.THIRD1,'RESIDENCE') NOT IN ('R','NR'))
        AND TYPE_RESID_ = 'NR'))
        )
        
        WHEN 'SOUS_VOL' THEN
        (
        SELECT SUM(FP.AMOUNT) FROM FUND_PURCHASE FP
        WHERE FP.FUND = FUND_ID_ AND
        FP.VALUE_DATE = NAV_DATE_ AND
        SIGN(FP.NUMBER_SHARES) = 1 AND
        (GET_TYPE_CLIENT(FP.THIRD1,'PERSONNE') = TYPE_PERSONNE_ OR ((GET_TYPE_CLIENT(FP.THIRD1,'PERSONNE') IS NULL OR GET_TYPE_CLIENT(FP.THIRD1,'PERSONNE') NOT IN ('P','M')) AND TYPE_PERSONNE_ = 'M')) AND
        (GET_TYPE_CLIENT(FP.THIRD1,'RESIDENCE') = TYPE_RESID_ OR ((GET_TYPE_CLIENT(FP.THIRD1,'RESIDENCE') IS NULL OR GET_TYPE_CLIENT(FP.THIRD1,'RESIDENCE') NOT IN ('R','NR'))
        AND TYPE_RESID_ = 'NR'))
        )
        
        WHEN 'RACH_NBRE' THEN
        (SELECT COUNT(*) FROM FUND_PURCHASE FP
        WHERE FP.FUND = FUND_ID_ AND
        FP.VALUE_DATE = NAV_DATE_ AND
        SIGN(FP.NUMBER_SHARES) = -1 AND
        (GET_TYPE_CLIENT(FP.THIRD1,'PERSONNE') = TYPE_PERSONNE_ OR ((GET_TYPE_CLIENT(FP.THIRD1,'PERSONNE') IS NULL OR GET_TYPE_CLIENT(FP.THIRD1,'PERSONNE') NOT IN ('P','M')) AND TYPE_PERSONNE_ = 'M')) AND
        (GET_TYPE_CLIENT(FP.THIRD1,'RESIDENCE') = TYPE_RESID_ OR ((GET_TYPE_CLIENT(FP.THIRD1,'RESIDENCE') IS NULL OR GET_TYPE_CLIENT(FP.THIRD1,'RESIDENCE') NOT IN ('R','NR'))
        AND TYPE_RESID_ = 'NR'))
        )
        
        WHEN 'RACH_QTE' THEN
        (SELECT ABS(SUM(FP.NUMBER_SHARES)) FROM FUND_PURCHASE FP
        WHERE FP.FUND = FUND_ID_ AND
        FP.VALUE_DATE = NAV_DATE_ AND
        SIGN(FP.NUMBER_SHARES) = -1 AND
        (GET_TYPE_CLIENT(FP.THIRD1,'PERSONNE') = TYPE_PERSONNE_ OR ((GET_TYPE_CLIENT(FP.THIRD1,'PERSONNE') IS NULL OR GET_TYPE_CLIENT(FP.THIRD1,'PERSONNE') NOT IN ('P','M')) AND TYPE_PERSONNE_ = 'M')) AND
        (GET_TYPE_CLIENT(FP.THIRD1,'RESIDENCE') = TYPE_RESID_ OR ((GET_TYPE_CLIENT(FP.THIRD1,'RESIDENCE') IS NULL OR GET_TYPE_CLIENT(FP.THIRD1,'RESIDENCE') NOT IN ('R','NR'))
        AND TYPE_RESID_ = 'NR'))
        )
        
        WHEN 'RACH_VOL' THEN
        (SELECT ABS(SUM(FP.AMOUNT)) FROM FUND_PURCHASE FP
        WHERE FP.FUND = FUND_ID_ AND
        FP.VALUE_DATE = NAV_DATE_ AND
        SIGN(FP.NUMBER_SHARES) = -1 AND
        (GET_TYPE_CLIENT(FP.THIRD1,'PERSONNE') = TYPE_PERSONNE_ OR ((GET_TYPE_CLIENT(FP.THIRD1,'PERSONNE') IS NULL OR GET_TYPE_CLIENT(FP.THIRD1,'PERSONNE') NOT IN ('P','M')) AND TYPE_PERSONNE_ = 'M')) AND
        (GET_TYPE_CLIENT(FP.THIRD1,'RESIDENCE') = TYPE_RESID_ OR ((GET_TYPE_CLIENT(FP.THIRD1,'RESIDENCE') IS NULL OR GET_TYPE_CLIENT(FP.THIRD1,'RESIDENCE') NOT IN ('R','NR'))
        AND TYPE_RESID_ = 'NR'))
        )
        
        WHEN 'PORT_PARTS' THEN
        (SELECT COUNT(*) FROM (SELECT FP.THIRD1, SUM(FP.NUMBER_SHARES) FROM FUND_PURCHASE FP
        WHERE 
        FP.FUND = FUND_ID_ AND
        FP.VALUE_DATE <= NAV_DATE_ AND
        (GET_TYPE_CLIENT(FP.THIRD1,'PERSONNE') = TYPE_PERSONNE_ OR ((GET_TYPE_CLIENT(FP.THIRD1,'PERSONNE') IS NULL OR GET_TYPE_CLIENT(FP.THIRD1,'PERSONNE') NOT IN ('P','M')) AND TYPE_PERSONNE_ = 'M')) AND
        (GET_TYPE_CLIENT(FP.THIRD1,'RESIDENCE') = TYPE_RESID_ OR ((GET_TYPE_CLIENT(FP.THIRD1,'RESIDENCE') IS NULL OR GET_TYPE_CLIENT(FP.THIRD1,'RESIDENCE') NOT IN ('R','NR'))
        AND TYPE_RESID_ = 'NR'))
        GROUP BY FP.THIRD1 HAVING SUM(FP.NUMBER_SHARES) <> 0)
        )
        END
        INTO sResult FROM DUAL;
        
        RETURN sResult;
      END;
  
		FUNCTION GET_TITRE_SECTOR(SICOVAM_ IN NUMBER, SECTOR_TYPE_ IN VARCHAR2) 
		RETURN VARCHAR2 AS
		SECTOR_ VARCHAR2(40);
		BEGIN
            SELECT NVL(
			(SELECT VAL.CODE FROM SECTORS VAL,SECTOR_INSTRUMENT_ASSOCIATION SIA,SECTORS SEC_PARENT 
			WHERE 
			SIA.SICOVAM = SICOVAM_ AND
			SIA.SECTOR = VAL.ID AND
			VAL.PARENT = SEC_PARENT.ID AND
			SEC_PARENT.NAME = CASE SECTOR_TYPE_ WHEN 'COTATION' THEN 'Type_Cotation' WHEN 'GARANTIE' THEN 'Type_Garantie' END),'') INTO SECTOR_ FROM DUAL;
      RETURN SECTOR_;
		END;	
    
    FUNCTION GET_TYPE_CLIENT(THIRD_PARTY IN NUMBER, TYPE_SECTOR IN VARCHAR2) RETURN VARCHAR2 
	AS
		TYPE_CLIENT_ VARCHAR2(140);
	BEGIN
		SELECT NVL(
			(SELECT T.VALUE  FROM TIERSPROPERTIES T WHERE T.CODE = THIRD_PARTY AND T.NAME = CASE TYPE_SECTOR WHEN 'PERSONNE' THEN 'Type_Personne' WHEN 'RESIDENCE' THEN 'Type_Residence' END)
			,'') INTO TYPE_CLIENT_ FROM DUAL;
		RETURN TYPE_CLIENT_;
	END;
  
	--- Taux de change des devises 
	FUNCTION GET_FX_RATE(v_funds IN VARCHAR2 , v_date in date , v_currency in number ) RETURN NUMBER 
	AS
		v_taux_change  number ;
	BEGIN
		select nvl(
			(select dev.fx_rate 
			from fund_history_currency dev 
			where dev.code = v_funds 
			and dev.nav_date = v_date
			and dev.currency = v_currency   )
			,0)
			into v_taux_change
			from dual;
		return v_taux_change;
	END;

END EMAFI;
/

CREATE OR REPLACE PUBLIC SYNONYM EMAFI FOR EMAFI;
/
