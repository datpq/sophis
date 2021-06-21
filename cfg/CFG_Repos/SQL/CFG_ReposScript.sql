--{{SOPHIS_SQL (do not delete this line)

call tk_add_histomvts_column('BROKER_TVA', 'number', NULL, 'Additional float broker tva');
call tk_add_histomvts_column('GROSS_TVA', 'number', NULL, 'Additional float gross tva');
call tk_add_histomvts_column('COUNTERPARTY_TVA', 'number', NULL, 'Additional float counterparty tva');
call tk_add_histomvts_column('MARKET_TVA', 'number', NULL, 'Additional float market tva');
call tk_add_histomvts_column('SPREAD_HT', 'number', NULL, 'Additional float Spread HT');
call tk_add_histomvts_column('CFG_REPO_AMOUNT', 'number', NULL, 'Additional float Repo amount');
call tk_add_histomvts_column('CFG_INTEREST_AMOUNT', 'number');

-- TVA management

create table CFG_TVA_RATES
( 
		ID			 NUMBER(10),
		TYPE_ID  NUMBER(2) NOT NULL,
		NAME     VARCHAR2(1024) NOT NULL,
		RATE		 NUMBER,
    CONSTRAINT CFG_TVA_RATES_PK PRIMARY KEY (ID)
);

create table CFG_TVA_RATE_TYPE
(
  ID    NUMBER(2),
  NAME  VARCHAR2(1024),
  CONSTRAINT CFG_TVA_RATE_TYPE_PK PRIMARY KEY (ID)
);

alter table CFG_TVA_RATES
	ADD CONSTRAINT CFG_TVA_RATES_FK 
		FOREIGN KEY(TYPE_ID)
		REFERENCES CFG_TVA_RATE_TYPE(ID);

-- Fill CFG_TVA_RATE_TYPE table

insert into CFG_TVA_RATE_TYPE(ID,NAME) values (1, 'Frais de Gestion');
insert into CFG_TVA_RATE_TYPE(ID,NAME) values (2, 'Frais Bancaires');
insert into CFG_TVA_RATE_TYPE(ID,NAME) values (3, 'Frais de transactions');
insert into CFG_TVA_RATE_TYPE(ID,NAME) values (4, 'Interets');

-- Fill CFG_TVA_RATES table

insert into CFG_TVA_RATES(ID, TYPE_ID,  NAME, RATE )
                 values (1, 1,'Frais de Droits de Garde (DDG)', 0.1 );
insert into CFG_TVA_RATES(ID, TYPE_ID,  NAME, RATE )
                 values (2, 1,'Frais CDVM', 0.2 );		
insert into CFG_TVA_RATES(ID, TYPE_ID,  NAME, RATE )
				 values (3, 1,'Frais de Publication', 0.21 );		
insert into CFG_TVA_RATES(ID, TYPE_ID,  NAME, RATE )				 
				 values (4, 1,'Frais de Commissariat aux Comptes', 0.22 );
insert into CFG_TVA_RATES(ID, TYPE_ID,  NAME, RATE )				 
				 values (5, 1,'Frais Maroclear', 0.23 );
insert into CFG_TVA_RATES(ID, TYPE_ID,  NAME, RATE )
				 values (6, 1,'Commission de Gestion', 0.24 );		
insert into CFG_TVA_RATES(ID, TYPE_ID,  NAME, RATE )				 
				 values (7, 2,'Agios', 0.10 );		
insert into CFG_TVA_RATES(ID, TYPE_ID,  NAME, RATE )				 
				 values (8, 2,'Services Bancaires', 0.11 );		
insert into CFG_TVA_RATES(ID, TYPE_ID,  NAME, RATE )				 
				 values (9, 2,'Abonnement Internet', 0.12 );		
insert into CFG_TVA_RATES(ID, TYPE_ID,  NAME, RATE )				 
				 values (10, 2,'OST', 0.13 );		
insert into CFG_TVA_RATES(ID, TYPE_ID,  NAME, RATE )				 
				 values (11, 2,'Autres Frais Bancaires', 0.14 );
insert into CFG_TVA_RATES(ID, TYPE_ID,  NAME, RATE )				 
				 values (12, 3,'Commissions d’Intermédiation (broker)', 0.1 );
insert into CFG_TVA_RATES(ID, TYPE_ID,  NAME, RATE )				 
				 values (13, 3,'Commissions de Bourse', 0.11 );
insert into CFG_TVA_RATES(ID, TYPE_ID,  NAME, RATE )				 
				 values (14, 3,'Frais de Règlement Livraison', 0.12 );
insert into CFG_TVA_RATES(ID, TYPE_ID,  NAME, RATE )
				 values (15, 3,'Conversion de titres', 0.13 );
insert into CFG_TVA_RATES(ID, TYPE_ID,  NAME, RATE )				 
				 values (16, 4,'Prise en Pension (Borrowing)', 0.11 );
insert into CFG_TVA_RATES(ID, TYPE_ID,  NAME, RATE )				 
				 values (17, 4,'Mise en Pension (Lending)', 0.1 );
insert into CFG_TVA_RATES(ID, TYPE_ID,  NAME, RATE )				 
				 values (18, 4,'DAT', 0.12 );

-- Table for the mapping between Sophis business event and CFG TVA rate
create table CFG_BUSINESS_EVENT_RATE 
( 
  BE_ID             Number(10), 
  CFG_TVA_RATE_ID   Number(10) NOT NULL,
  CONSTRAINT CFG_BUSINESS_EVENT_RATE_PK PRIMARY KEY(BE_ID)
);

-- Fill this table with default value 1 which corresponds to 'Frais de Droits de Garde (DDG)'

INSERT INTO CFG_BUSINESS_EVENT_RATE 
  SELECT ID, 1 FROM BUSINESS_EVENTS;

-- For audit
  
Create table CFG_TVA_RATES_AUDIT
( 
	TVA_RATE_ID	         Number(10),
	NAME                 VARCHAR2(40) NOT NULL,
	RATE_BEFORE_MODIF   NUMBER,
	RATE_AFTER_MODIF    NUMBER,
	DATE_MODIF           TIMESTAMP 
);
                                                          

create or replace trigger CFG_TVA_RATES_AUDIT after update on CFG_TVA_RATES     
  
  for each row  
	   begin        
      if :old.RATE <> :new.RATE then
        -- Insert record into audit table
          insert into CFG_TVA_RATES_AUDIT( TVA_RATE_ID,NAME,RATE_BEFORE_MODIF,RATE_AFTER_MODIF,DATE_MODIF)           
            values(:old.ID,:old.NAME,:old.RATE,:new.RATE,SYSDATE);
      end if;
	   end;
/
  			 
commit;   

--}}SOPHIS_SQL