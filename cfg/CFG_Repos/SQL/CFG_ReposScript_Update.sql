--{{SOPHIS_SQL (do not delete this line)

update CFG_TVA_RATE_TYPE set NAME = 'Interets' where ID = 4;

update CFG_TVA_RATES set NAME = 'Prise en Pension (Borrowing)' , RATE = 0.11 where ID = 16;				 				 
update CFG_TVA_RATES set NAME = 'Mise en Pension (Lending)', RATE = 0.1 where ID = 17;				 
  			 
commit;   

--}}SOPHIS_SQL