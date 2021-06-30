create or replace 
PACKAGE BODY KAMCO_GL AS

PROCEDURE UPDATESTATUS_  
IS

CURSOR cur IS
SELECT ID, status FROM ACCOUNT_POSTING_KB_GL where 
    status in (1,2,7,9,10)
    and posting_date <= sysdate 
    and posting_type in (select ID from account_posting_types where name !='Asset' and name != 'Technical' and summary is not null)
    and rownum<10 
 FOR UPDATE;
 
ligne cur%rowtype;

BEGIN

  OPEN cur ;
  LOOP
  FETCH cur INTO ligne ;
    EXIT WHEN cur%NOTFOUND ;
        
   if ligne.status = 1 then
          update ACCOUNT_POSTING_KB_GL SET STATUS = 4 where current of cur;
          
    elsif ligne.status = 2 then 
          update ACCOUNT_POSTING_KB_GL SET STATUS = 4 where current of cur;          
    elsif ligne.status = 7 then
          update ACCOUNT_POSTING_KB_GL SET STATUS = 15 where current of cur;          
    else 
          update ACCOUNT_POSTING_KB_GL SET STATUS = 12 where current of cur;  
         
     end if;
         
     
  END LOOP ;  
  
  close cur;
END;




END KAMCO_GL;