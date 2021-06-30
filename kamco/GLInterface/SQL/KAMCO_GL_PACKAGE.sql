create or replace 
PACKAGE KAMCO_GL AS 

  PROCEDURE AUDIT_Procedure;
  PROCEDURE UPDATE_POSTING_STATUS;
  PROCEDURE UPDATE_AUDIT_STATUS;
  PROCEDURE updateJournalName;
END KAMCO_GL;

/
create or replace 
PACKAGE BODY KAMCO_GL AS

PROCEDURE updateJournalName
IS
CURSOR cur  IS
SELECT Accounting_Date ,to_char(Accounting_Date, 'YY') year_,  decode(COMPANY,null,'00',COMPANY) COMPANY
FROM GL_INTERFACE_AUDIT where status_audit = 1
order by Accounting_Date 
for update of journal_name;

date_ date;
year_ varchar2(2);
ligne cur%rowtype;

test_bool number := 0;
counter number := 0;
BEGIN
   
    
      OPEN cur;
        
      LOOP
      FETCH cur INTO ligne ;
        EXIT WHEN cur%NOTFOUND ;
      
        while test_bool = 0 loop
          date_ := ligne.accounting_date;
          year_ := ligne.year_;
          
          select nvl( max(to_number(SUBSTR(journal_name,7,3))),0) into counter from GL_INTERFACE_AUDIT where 
            to_char(Accounting_Date, 'YY') = ligne.year_;
            
          counter := counter +1 ;
          test_bool := 1;
        
        end loop;
        
        if  ligne.year_ <>  year_   then
          year_ :=ligne.year_;
          select nvl( max(to_number(SUBSTR(journal_name,7,3))),0) into counter from GL_INTERFACE_AUDIT where 
            to_char(Accounting_Date, 'YY') = ligne.year_;
            
          counter := counter +1 ;
          date_ := ligne.accounting_date;
          
        end if;
        
        if date_<> ligne.accounting_date then
          date_ := ligne.accounting_date;
          counter := counter +1 ;
        end if;
        
        update GL_INTERFACE_AUDIT set journal_name = to_char(ligne.accounting_date, 'YYMM')||ligne.COMPANY||LPAD(counter, 3, '0') where current of cur;
        
      END LOOP ; 
      close cur; 

  
END;





PROCEDURE AUDIT_Procedure  
IS
BEGIN

    INSERT INTO GL_INTERFACE_AUDIT (ID, id_posting,LEDGER,CATEGORY,
    SOURCE, CURRENCY, ACCOUNTING_DATE,
    COMPANY,  CURRENTCY,  ACCOUNT,  SUBACCOUNT,REGION,RELATION,FUTURE,DEBIT,CREDIT,
    LINE_DESCRIPTION,JOURNAL_DESCRIPTION, TREATMENT_TIME,OPERATOR_ID,status_audit)
     ( SELECT  KAMCO_GL_SEQ.NEXTVAL, id_posting,	LEDGER,	CATEGORY, 'MANUAL',
    CURRENCY,	posting_date,  decode(COMPANY,null,'00',COMPANY) COMPANY,CURRENCY,
    ACCOUNT_NUMBER,'',
	REGION,RELATION,'00',debit,credit,journal_desc,journal_desc ,  SYSdATE, getuserid,1
    FROM 
    (
  SELECT ap.posting_date, ap.id id_posting, TTR.LIBELLE LEDGER ,(select tr.value from tiersproperties tr where tr.code = ttr.code_emet  and tr.name in ('GL_Category')) CATEGORY,  
	devise_to_str(ttr.devisectt) CURRENCY, TO_CHAR(ap.posting_date, 'DD/MM/YYYY')  Accounting_Date, 
	(select tr.value from tiersproperties tr where tr.code = ttr.code_emet  and tr.name in('GL_Company')) COMPANY, ACCOUNT_NUMBER, 
	(select tr.value from tiersproperties tr where tr.code = ttr.code_emet  and tr.name in('GL_Region')) REGION, 
	nvl((select case when upper(tr.name) like '%BURGAN%' then '01' else '02' end case from tiers tr where tr.ident = ap.third_party_id), '00') RELATION, 
	to_char(ap.amount*decode(ap.credit_debit, 'D', 1, 0)) Debit, to_char(ap.amount*decode(ap.credit_debit, 'C', 1, 0)) credit,  
	ap.trade_id||';'||decode(ap.sign,'+','Purchase','-','Sales')||';'|| ( select t_.libelle from titres t_ where t_.sicovam = ap.instrument_id )||';'||ap.quantity||';'||ap.amount||';'||TTR.MNEMO journal_desc 
	FROM ACCOUNT_POSTING AP 
	JOIN HISTOMVTS H ON H.REFCON = AP.TRADE_ID 
	JOIN POSITION P ON P.MVTIDENT = H.MVTIDENT 
	JOIN FOLIO F ON F.IDENT = P.OPCVM 
	JOIN TITRES TTR ON TTR.TYPE = 'Z' AND  TTR.MNEMO in(SELECT CONNECT_BY_ROOT ident  parent 
	FROM folio    WHERE   LEVEL > 1  and    ident = P.OPCVM   CONNECT BY PRIOR ident = mgr) 
	where 
	ap.status in(1, 2, 7, 9, 10) and
	ap.posting_date <= to_date(sysdate,'DD-MM-YY')   
	order by ap.posting_date));
  
  
  
END;




PROCEDURE UPDATE_POSTING_STATUS
IS

CURSOR cur IS
SELECT ap.status
	FROM ACCOUNT_POSTING AP 
	where ap.id in (select id_posting from gl_interface_audit where status_audit = 1)
  FOR UPDATE of ap.status;
 
ligne cur%rowtype;

BEGIN
  

  OPEN cur ;
  LOOP
  FETCH cur INTO ligne ;
    EXIT WHEN cur%NOTFOUND ;
    
	/**************************  update status in account_posting table ************/
    
   if ligne.status = 1 then
          update ACCOUNT_POSTING SET STATUS = 4 where current of cur;
          
   elsif ligne.status = 2 then 
          update ACCOUNT_POSTING SET STATUS = 4 where current of cur;          
    elsif ligne.status = 7 then
          update ACCOUNT_POSTING SET STATUS = 15 where current of cur;          
    else 
          update ACCOUNT_POSTING SET STATUS = 12 where current of cur; 
         
     end if;
  --********** update journal_entry in account_posting table **********************
     
     update ACCOUNT_POSTING SET journal_entry = SYSDATE where current of cur;
     
  END LOOP ; 
  
  
  close cur;
    
END;







PROCEDURE UPDATE_AUDIT_STATUS
IS
BEGIN
  /**************************  update status to 2 in the audit table ************/

     update gl_interface_audit set status_audit=2 where status_audit =1 ;  
      
END;





END KAMCO_GL;