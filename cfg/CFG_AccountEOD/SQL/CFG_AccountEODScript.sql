--{{SOPHIS_SQL (do not delete this line)


--create table cfg_repo_posting_to_treat as select * from account_posting where 1 = 0;

alter table cfg_repo_posting_to_treat add (ORIGINAL_POSTING_DATE date);

create or replace
PROCEDURE CFG_TREAT_REPO_CANC_POSTING (pInitRepoPostingType in number, pInitLinkedRepoPostingType in number, pExpRepoPostingType in number, pExpLinkedRepoPostingType in number, pNavDate in Date,pAccEntId in number) as

lSizeTable number;
lAveragePrice number;
lCurrentPostingId number;
lAlPostingToReverseId number;
lInitLinkedPostingId number;
lExpLinkedPostingId number;
lInitAlLinkedPostingToRevertId number;
lExpAlLinkedPostingToRevertId number;

lID ACCOUNT_POSTING.ID%TYPE;
lACCOUNT_ENTITY_ID ACCOUNT_POSTING.ACCOUNT_ENTITY_ID%TYPE;
lPOSTING_RULE_ID ACCOUNT_POSTING.POSTING_RULE_ID%TYPE;
lTRADE_ID ACCOUNT_POSTING.TRADE_ID%TYPE;
lVERSION_ID ACCOUNT_POSTING.VERSION_ID%TYPE;
lPOSTING_TYPE ACCOUNT_POSTING.POSTING_TYPE%TYPE;
lGENERATION_DATE ACCOUNT_POSTING.GENERATION_DATE%TYPE;
lPOSTING_DATE ACCOUNT_POSTING.POSTING_DATE%TYPE;
lACCOUNT_NUMBER ACCOUNT_POSTING.ACCOUNT_NUMBER%TYPE;
lACCOUNT_CURRENCY ACCOUNT_POSTING.ACCOUNT_CURRENCY%TYPE;
lAMOUNT ACCOUNT_POSTING.AMOUNT%TYPE;
lCREDIT_DEBIT ACCOUNT_POSTING.CREDIT_DEBIT%TYPE;
lTHIRD_PARTY_ID ACCOUNT_POSTING.THIRD_PARTY_ID%TYPE;
lINSTRUMENT_ID ACCOUNT_POSTING.INSTRUMENT_ID%TYPE;
lCURRENCY ACCOUNT_POSTING.CURRENCY%TYPE;
lQUANTITY ACCOUNT_POSTING.QUANTITY%TYPE;
lSIGN ACCOUNT_POSTING.SIGN%TYPE;
lAUXILIARY1_ACCOUNT ACCOUNT_POSTING.AUXILIARY1_ACCOUNT%TYPE;
lAUXILIARY2_ACCOUNT ACCOUNT_POSTING.AUXILIARY2_ACCOUNT%TYPE;
lCOMMENTS ACCOUNT_POSTING.COMMENTS%TYPE;
lJOURNAL_ENTRY ACCOUNT_POSTING.JOURNAL_ENTRY%TYPE;
lSTATUS ACCOUNT_POSTING.STATUS%TYPE;
lAUXILLARY_DATE ACCOUNT_POSTING.AUXILLARY_DATE%TYPE;
lACCOUNT_NAME_ID ACCOUNT_POSTING.ACCOUNT_NAME_ID%TYPE;
lAUXILIARY1_ACCOUNT_ID ACCOUNT_POSTING.AUXILIARY1_ACCOUNT_ID%TYPE;
lAUXILIARY2_ACCOUNT_ID ACCOUNT_POSTING.AUXILIARY2_ACCOUNT_ID%TYPE;
lLINK_ID ACCOUNT_POSTING.LINK_ID%TYPE;
lRULE_TYPE ACCOUNT_POSTING.RULE_TYPE%TYPE;
lAUXILIARY3_ACCOUNT ACCOUNT_POSTING.AUXILIARY3_ACCOUNT%TYPE;
lAUXILIARY3_ACCOUNT_ID ACCOUNT_POSTING.AUXILIARY3_ACCOUNT_ID%TYPE;
lPOSITION_ID ACCOUNT_POSTING.POSITION_ID%TYPE;
lAUXILIARY_ID ACCOUNT_POSTING.AUXILIARY_ID%TYPE;
lACCOUNTING_BOOK_ID ACCOUNT_POSTING.ACCOUNTING_BOOK_ID%TYPE;
lAMORTIZING_RULE_ID ACCOUNT_POSTING.AMORTIZING_RULE_ID%TYPE;
lAMOUNT_CURRENCY ACCOUNT_POSTING.AMOUNT_CURRENCY%TYPE;
lTRADE_TYPE ACCOUNT_POSTING.TRADE_TYPE%TYPE;
lORIGINAL_POSTING_DATE ACCOUNT_POSTING.ORIGINAL_POSTING_DATE%TYPE;

lNEWAMOUNT ACCOUNT_POSTING.AMOUNT%TYPE;

BEGIN

-- Start with Init postings

-- Fill table cfg_repo_posting_to_treat
execute immediate 'truncate table cfg_repo_posting_to_treat';
insert into cfg_repo_posting_to_treat select * from account_posting where posting_type=pInitRepoPostingType and status = 10 and posting_date<=pNavDate and account_entity_id=pAccEntId order by trade_id asc;
commit;

select count(*) into lSizeTable from cfg_repo_posting_to_treat;

DBMS_OUTPUT.PUT_LINE(lSizeTable);

FOR i IN 0 .. lSizeTable-1 LOOP
        
        -- Find Auxiliary Ledger Posting to Reverse
        select id into lCurrentPostingId from (select id,row_number() over (order by Trade_id) num from cfg_repo_posting_to_treat) where num=i+1;
        
        -- Retrieve id of Auxiliary Ledger Posting to Reverse
        select id into lAlPostingToReverseId from account_posting 
        where id<>lCurrentPostingId
        and rule_type=4
        and link_id = (select link_id from account_posting where id=lCurrentPostingId);
        
        -- Load posting data of Auxiliary Ledger Posting to Reverse
        
        select ACCOUNT_ENTITY_ID,POSTING_RULE_ID,TRADE_ID,VERSION_ID,POSTING_TYPE,GENERATION_DATE,POSTING_DATE,ACCOUNT_NUMBER,ACCOUNT_CURRENCY,AMOUNT,CREDIT_DEBIT,THIRD_PARTY_ID,INSTRUMENT_ID,CURRENCY,QUANTITY,SIGN,AUXILIARY1_ACCOUNT,AUXILIARY2_ACCOUNT,COMMENTS,JOURNAL_ENTRY,STATUS,AUXILLARY_DATE,ACCOUNT_NAME_ID,AUXILIARY1_ACCOUNT_ID,AUXILIARY2_ACCOUNT_ID,LINK_ID,RULE_TYPE,AUXILIARY3_ACCOUNT,AUXILIARY3_ACCOUNT_ID,POSITION_ID,AUXILIARY_ID,ACCOUNTING_BOOK_ID,AMORTIZING_RULE_ID,AMOUNT_CURRENCY,TRADE_TYPE,ORIGINAL_POSTING_DATE 
        into lACCOUNT_ENTITY_ID,lPOSTING_RULE_ID,lTRADE_ID,lVERSION_ID,lPOSTING_TYPE,lGENERATION_DATE,lPOSTING_DATE,lACCOUNT_NUMBER,lACCOUNT_CURRENCY,lAMOUNT,lCREDIT_DEBIT,lTHIRD_PARTY_ID,lINSTRUMENT_ID,lCURRENCY,lQUANTITY,lSIGN,lAUXILIARY1_ACCOUNT,lAUXILIARY2_ACCOUNT,lCOMMENTS,lJOURNAL_ENTRY,lSTATUS,lAUXILLARY_DATE,lACCOUNT_NAME_ID,lAUXILIARY1_ACCOUNT_ID,lAUXILIARY2_ACCOUNT_ID,lLINK_ID,lRULE_TYPE,lAUXILIARY3_ACCOUNT,lAUXILIARY3_ACCOUNT_ID,lPOSITION_ID,lAUXILIARY_ID,lACCOUNTING_BOOK_ID,lAMORTIZING_RULE_ID,lAMOUNT_CURRENCY,lTRADE_TYPE,lORIGINAL_POSTING_DATE from account_posting 
        where id=lAlPostingToReverseId;

        -- Create a reversal posting
        
        select seqaccount.nextval into lID from dual; --set id to the next available sequence
        lGENERATION_DATE:=Sysdate; --set generation date to the Oracle System Date
        lSTATUS:=7; --set status to ready AL
        if lCREDIT_DEBIT = 'C' then
          lCREDIT_DEBIT:='D';
        else
          lCREDIT_DEBIT:='C';
        end if;
        if lSIGN = '-' then
          lSIGN:='+';
        else
          lSIGN:='-';
        end if;
        
        
        insert into account_posting (ID,ACCOUNT_ENTITY_ID,POSTING_RULE_ID,TRADE_ID,VERSION_ID,POSTING_TYPE,GENERATION_DATE,POSTING_DATE,ACCOUNT_NUMBER,ACCOUNT_CURRENCY,AMOUNT,CREDIT_DEBIT,THIRD_PARTY_ID,INSTRUMENT_ID,CURRENCY,QUANTITY,SIGN,AUXILIARY1_ACCOUNT,AUXILIARY2_ACCOUNT,COMMENTS,JOURNAL_ENTRY,STATUS,AUXILLARY_DATE,ACCOUNT_NAME_ID,AUXILIARY1_ACCOUNT_ID,AUXILIARY2_ACCOUNT_ID,LINK_ID,RULE_TYPE,AUXILIARY3_ACCOUNT,AUXILIARY3_ACCOUNT_ID,POSITION_ID,AUXILIARY_ID,ACCOUNTING_BOOK_ID,AMORTIZING_RULE_ID,AMOUNT_CURRENCY,TRADE_TYPE,ORIGINAL_POSTING_DATE)
        values (lID,lACCOUNT_ENTITY_ID,lPOSTING_RULE_ID,lTRADE_ID,lVERSION_ID,lPOSTING_TYPE,lGENERATION_DATE,lPOSTING_DATE,lACCOUNT_NUMBER,lACCOUNT_CURRENCY,lAMOUNT,lCREDIT_DEBIT,lTHIRD_PARTY_ID,lINSTRUMENT_ID,lCURRENCY,lQUANTITY,lSIGN,lAUXILIARY1_ACCOUNT,lAUXILIARY2_ACCOUNT,lCOMMENTS,lJOURNAL_ENTRY,lSTATUS,lAUXILLARY_DATE,lACCOUNT_NAME_ID,lAUXILIARY1_ACCOUNT_ID,lAUXILIARY2_ACCOUNT_ID,lLINK_ID,lRULE_TYPE,lAUXILIARY3_ACCOUNT,lAUXILIARY3_ACCOUNT_ID,lPOSITION_ID,lAUXILIARY_ID,lACCOUNTING_BOOK_ID,lAMORTIZING_RULE_ID,lAMOUNT_CURRENCY,lTRADE_TYPE,lORIGINAL_POSTING_DATE);
        commit;
        
        -- Update treated posting to reflect treatment
        update account_posting set status=16 where id = lCurrentPostingId;
        commit;
      
        -- Deal with repo posting associated
        -- Retrieve Linked Posting
        select ID into lInitLinkedPostingId from account_posting where trade_id=lTRADE_ID and posting_type=pInitLinkedRepoPostingType and status=13;
        
        -- Retrieve Linked Al Posting to Reverse
        select id into lInitAlLinkedPostingToRevertId from account_posting 
        where id<>lInitLinkedPostingId
        and rule_type=4
        and link_id = lInitLinkedPostingId;
        
        
        select ID,ACCOUNT_ENTITY_ID,POSTING_RULE_ID,TRADE_ID,VERSION_ID,POSTING_TYPE,GENERATION_DATE,POSTING_DATE,ACCOUNT_NUMBER,ACCOUNT_CURRENCY,AMOUNT,CREDIT_DEBIT,THIRD_PARTY_ID,INSTRUMENT_ID,CURRENCY,QUANTITY,SIGN,AUXILIARY1_ACCOUNT,AUXILIARY2_ACCOUNT,COMMENTS,JOURNAL_ENTRY,STATUS,AUXILLARY_DATE,ACCOUNT_NAME_ID,AUXILIARY1_ACCOUNT_ID,AUXILIARY2_ACCOUNT_ID,LINK_ID,RULE_TYPE,AUXILIARY3_ACCOUNT,AUXILIARY3_ACCOUNT_ID,POSITION_ID,AUXILIARY_ID,ACCOUNTING_BOOK_ID,AMORTIZING_RULE_ID,AMOUNT_CURRENCY,TRADE_TYPE,ORIGINAL_POSTING_DATE
        into lID,lACCOUNT_ENTITY_ID,lPOSTING_RULE_ID,lTRADE_ID,lVERSION_ID,lPOSTING_TYPE,lGENERATION_DATE,lPOSTING_DATE,lACCOUNT_NUMBER,lACCOUNT_CURRENCY,lAMOUNT,lCREDIT_DEBIT,lTHIRD_PARTY_ID,lINSTRUMENT_ID,lCURRENCY,lQUANTITY,lSIGN,lAUXILIARY1_ACCOUNT,lAUXILIARY2_ACCOUNT,lCOMMENTS,lJOURNAL_ENTRY,lSTATUS,lAUXILLARY_DATE,lACCOUNT_NAME_ID,lAUXILIARY1_ACCOUNT_ID,lAUXILIARY2_ACCOUNT_ID,lLINK_ID,lRULE_TYPE,lAUXILIARY3_ACCOUNT,lAUXILIARY3_ACCOUNT_ID,lPOSITION_ID,lAUXILIARY_ID,lACCOUNTING_BOOK_ID,lAMORTIZING_RULE_ID,lAMOUNT_CURRENCY,lTRADE_TYPE,lORIGINAL_POSTING_DATE from account_posting 
        where id=lInitAlLinkedPostingToRevertId;

        -- Update treated posting to reflect treatment
        update account_posting set status=16 where id = lInitLinkedPostingId;
        commit;
        
        -- Create a reversal posting
                
        select seqaccount.nextval into lID from dual; --set id to the next available sequence
        lGENERATION_DATE:=Sysdate; --set generation date to the Oracle System Date
        lSTATUS:=7; --set status to ready AL
        if lCREDIT_DEBIT = 'C' then -- to switch Credit to Debit
          lCREDIT_DEBIT:='D';
        else
          lCREDIT_DEBIT:='C';
        end if;
        if lSIGN = '-' then -- to switch Quantity
          lSIGN:='+';
        else
          lSIGN:='-';
        end if;
        
        insert into account_posting (ID,ACCOUNT_ENTITY_ID,POSTING_RULE_ID,TRADE_ID,VERSION_ID,POSTING_TYPE,GENERATION_DATE,POSTING_DATE,ACCOUNT_NUMBER,ACCOUNT_CURRENCY,AMOUNT,CREDIT_DEBIT,THIRD_PARTY_ID,INSTRUMENT_ID,CURRENCY,QUANTITY,SIGN,AUXILIARY1_ACCOUNT,AUXILIARY2_ACCOUNT,COMMENTS,JOURNAL_ENTRY,STATUS,AUXILLARY_DATE,ACCOUNT_NAME_ID,AUXILIARY1_ACCOUNT_ID,AUXILIARY2_ACCOUNT_ID,LINK_ID,RULE_TYPE,AUXILIARY3_ACCOUNT,AUXILIARY3_ACCOUNT_ID,POSITION_ID,AUXILIARY_ID,ACCOUNTING_BOOK_ID,AMORTIZING_RULE_ID,AMOUNT_CURRENCY,TRADE_TYPE,ORIGINAL_POSTING_DATE)
        values (lID,lACCOUNT_ENTITY_ID,lPOSTING_RULE_ID,lTRADE_ID,lVERSION_ID,lPOSTING_TYPE,lGENERATION_DATE,lPOSTING_DATE,lACCOUNT_NUMBER,lACCOUNT_CURRENCY,lAMOUNT,lCREDIT_DEBIT,lTHIRD_PARTY_ID,lINSTRUMENT_ID,lCURRENCY,lQUANTITY,lSIGN,lAUXILIARY1_ACCOUNT,lAUXILIARY2_ACCOUNT,lCOMMENTS,lJOURNAL_ENTRY,lSTATUS,lAUXILLARY_DATE,lACCOUNT_NAME_ID,lAUXILIARY1_ACCOUNT_ID,lAUXILIARY2_ACCOUNT_ID,lLINK_ID,lRULE_TYPE,lAUXILIARY3_ACCOUNT,lAUXILIARY3_ACCOUNT_ID,lPOSITION_ID,lAUXILIARY_ID,lACCOUNTING_BOOK_ID,lAMORTIZING_RULE_ID,lAMOUNT_CURRENCY,lTRADE_TYPE,lORIGINAL_POSTING_DATE);
        commit;
        
end loop;

-- Continue with expiry postings

-- Fill table cfg_repo_posting_to_treat
execute immediate 'truncate table cfg_repo_posting_to_treat';
insert into cfg_repo_posting_to_treat select * from account_posting where posting_type=pExpRepoPostingType and status = 10 and posting_date<=pNavDate and account_entity_id=pAccEntId order by trade_id asc;
commit;

select count(*) into lSizeTable from cfg_repo_posting_to_treat;

DBMS_OUTPUT.PUT_LINE(lSizeTable);

FOR i IN 0 .. lSizeTable-1 LOOP
        
        -- Find Auxiliary Ledger Posting to Reverse
        select id into lCurrentPostingId from (select id,row_number() over (order by Trade_id) num from cfg_repo_posting_to_treat) where num=i+1;

DBMS_OUTPUT.PUT_LINE(lCurrentPostingId);

        -- Retrieve id of Auxiliary Ledger Posting to Reverse
        select id into lAlPostingToReverseId from account_posting 
        where id<>lCurrentPostingId
        and rule_type=4
        and link_id = (select link_id from account_posting where id=lCurrentPostingId);
        
        -- Load posting data of Auxiliary Ledger Posting to Reverse
        
        select ACCOUNT_ENTITY_ID,POSTING_RULE_ID,TRADE_ID,VERSION_ID,POSTING_TYPE,GENERATION_DATE,POSTING_DATE,ACCOUNT_NUMBER,ACCOUNT_CURRENCY,AMOUNT,CREDIT_DEBIT,THIRD_PARTY_ID,INSTRUMENT_ID,CURRENCY,QUANTITY,SIGN,AUXILIARY1_ACCOUNT,AUXILIARY2_ACCOUNT,COMMENTS,JOURNAL_ENTRY,STATUS,AUXILLARY_DATE,ACCOUNT_NAME_ID,AUXILIARY1_ACCOUNT_ID,AUXILIARY2_ACCOUNT_ID,LINK_ID,RULE_TYPE,AUXILIARY3_ACCOUNT,AUXILIARY3_ACCOUNT_ID,POSITION_ID,AUXILIARY_ID,ACCOUNTING_BOOK_ID,AMORTIZING_RULE_ID,AMOUNT_CURRENCY,TRADE_TYPE,ORIGINAL_POSTING_DATE 
        into lACCOUNT_ENTITY_ID,lPOSTING_RULE_ID,lTRADE_ID,lVERSION_ID,lPOSTING_TYPE,lGENERATION_DATE,lPOSTING_DATE,lACCOUNT_NUMBER,lACCOUNT_CURRENCY,lAMOUNT,lCREDIT_DEBIT,lTHIRD_PARTY_ID,lINSTRUMENT_ID,lCURRENCY,lQUANTITY,lSIGN,lAUXILIARY1_ACCOUNT,lAUXILIARY2_ACCOUNT,lCOMMENTS,lJOURNAL_ENTRY,lSTATUS,lAUXILLARY_DATE,lACCOUNT_NAME_ID,lAUXILIARY1_ACCOUNT_ID,lAUXILIARY2_ACCOUNT_ID,lLINK_ID,lRULE_TYPE,lAUXILIARY3_ACCOUNT,lAUXILIARY3_ACCOUNT_ID,lPOSITION_ID,lAUXILIARY_ID,lACCOUNTING_BOOK_ID,lAMORTIZING_RULE_ID,lAMOUNT_CURRENCY,lTRADE_TYPE,lORIGINAL_POSTING_DATE from account_posting 
        where id=lAlPostingToReverseId;

        -- Create a reversal posting
        
        select seqaccount.nextval into lID from dual; --set id to the next available sequence
        lGENERATION_DATE:=Sysdate; --set generation date to the Oracle System Date
        lSTATUS:=7; --set status to ready AL
        if lCREDIT_DEBIT = 'C' then
          lCREDIT_DEBIT:='D';
        else
          lCREDIT_DEBIT:='C';
        end if;
        if lSIGN = '-' then
          lSIGN:='+';
        else
          lSIGN:='-';
        end if;
        
        
        insert into account_posting (ID,ACCOUNT_ENTITY_ID,POSTING_RULE_ID,TRADE_ID,VERSION_ID,POSTING_TYPE,GENERATION_DATE,POSTING_DATE,ACCOUNT_NUMBER,ACCOUNT_CURRENCY,AMOUNT,CREDIT_DEBIT,THIRD_PARTY_ID,INSTRUMENT_ID,CURRENCY,QUANTITY,SIGN,AUXILIARY1_ACCOUNT,AUXILIARY2_ACCOUNT,COMMENTS,JOURNAL_ENTRY,STATUS,AUXILLARY_DATE,ACCOUNT_NAME_ID,AUXILIARY1_ACCOUNT_ID,AUXILIARY2_ACCOUNT_ID,LINK_ID,RULE_TYPE,AUXILIARY3_ACCOUNT,AUXILIARY3_ACCOUNT_ID,POSITION_ID,AUXILIARY_ID,ACCOUNTING_BOOK_ID,AMORTIZING_RULE_ID,AMOUNT_CURRENCY,TRADE_TYPE,ORIGINAL_POSTING_DATE)
        values (lID,lACCOUNT_ENTITY_ID,lPOSTING_RULE_ID,lTRADE_ID,lVERSION_ID,lPOSTING_TYPE,lGENERATION_DATE,lPOSTING_DATE,lACCOUNT_NUMBER,lACCOUNT_CURRENCY,lAMOUNT,lCREDIT_DEBIT,lTHIRD_PARTY_ID,lINSTRUMENT_ID,lCURRENCY,lQUANTITY,lSIGN,lAUXILIARY1_ACCOUNT,lAUXILIARY2_ACCOUNT,lCOMMENTS,lJOURNAL_ENTRY,lSTATUS,lAUXILLARY_DATE,lACCOUNT_NAME_ID,lAUXILIARY1_ACCOUNT_ID,lAUXILIARY2_ACCOUNT_ID,lLINK_ID,lRULE_TYPE,lAUXILIARY3_ACCOUNT,lAUXILIARY3_ACCOUNT_ID,lPOSITION_ID,lAUXILIARY_ID,lACCOUNTING_BOOK_ID,lAMORTIZING_RULE_ID,lAMOUNT_CURRENCY,lTRADE_TYPE,lORIGINAL_POSTING_DATE);
        commit;
        
        -- Update treated posting to reflect treatment
        update account_posting set status=16 where id = lCurrentPostingId;
        commit;
      
        -- Deal with repo posting associated
        -- Retrieve Linked Posting
        select ID into lExpLinkedPostingId from account_posting where trade_id=lTRADE_ID and posting_type=pExpLinkedRepoPostingType and status=13;
        
        -- Retrieve Linked Al Posting to Reverse
        select id into lExpAlLinkedPostingToRevertId from account_posting 
        where id<>lExpLinkedPostingId
        and rule_type=4
        and link_id = lExpLinkedPostingId;
        
        
        select ID,ACCOUNT_ENTITY_ID,POSTING_RULE_ID,TRADE_ID,VERSION_ID,POSTING_TYPE,GENERATION_DATE,POSTING_DATE,ACCOUNT_NUMBER,ACCOUNT_CURRENCY,AMOUNT,CREDIT_DEBIT,THIRD_PARTY_ID,INSTRUMENT_ID,CURRENCY,QUANTITY,SIGN,AUXILIARY1_ACCOUNT,AUXILIARY2_ACCOUNT,COMMENTS,JOURNAL_ENTRY,STATUS,AUXILLARY_DATE,ACCOUNT_NAME_ID,AUXILIARY1_ACCOUNT_ID,AUXILIARY2_ACCOUNT_ID,LINK_ID,RULE_TYPE,AUXILIARY3_ACCOUNT,AUXILIARY3_ACCOUNT_ID,POSITION_ID,AUXILIARY_ID,ACCOUNTING_BOOK_ID,AMORTIZING_RULE_ID,AMOUNT_CURRENCY,TRADE_TYPE,ORIGINAL_POSTING_DATE
        into lID,lACCOUNT_ENTITY_ID,lPOSTING_RULE_ID,lTRADE_ID,lVERSION_ID,lPOSTING_TYPE,lGENERATION_DATE,lPOSTING_DATE,lACCOUNT_NUMBER,lACCOUNT_CURRENCY,lAMOUNT,lCREDIT_DEBIT,lTHIRD_PARTY_ID,lINSTRUMENT_ID,lCURRENCY,lQUANTITY,lSIGN,lAUXILIARY1_ACCOUNT,lAUXILIARY2_ACCOUNT,lCOMMENTS,lJOURNAL_ENTRY,lSTATUS,lAUXILLARY_DATE,lACCOUNT_NAME_ID,lAUXILIARY1_ACCOUNT_ID,lAUXILIARY2_ACCOUNT_ID,lLINK_ID,lRULE_TYPE,lAUXILIARY3_ACCOUNT,lAUXILIARY3_ACCOUNT_ID,lPOSITION_ID,lAUXILIARY_ID,lACCOUNTING_BOOK_ID,lAMORTIZING_RULE_ID,lAMOUNT_CURRENCY,lTRADE_TYPE,lORIGINAL_POSTING_DATE from account_posting 
        where id=lExpAlLinkedPostingToRevertId;

        -- Update treated posting to reflect treatment
        update account_posting set status=16 where id = lExpLinkedPostingId;
        commit;
        
        -- Create a reversal posting
                
        select seqaccount.nextval into lID from dual; --set id to the next available sequence
        lGENERATION_DATE:=Sysdate; --set generation date to the Oracle System Date
        lSTATUS:=7; --set status to ready AL
        if lCREDIT_DEBIT = 'C' then -- to switch Credit to Debit
          lCREDIT_DEBIT:='D';
        else
          lCREDIT_DEBIT:='C';
        end if;
        if lSIGN = '-' then -- to switch Quantity
          lSIGN:='+';
        else
          lSIGN:='-';
        end if;
        
        insert into account_posting (ID,ACCOUNT_ENTITY_ID,POSTING_RULE_ID,TRADE_ID,VERSION_ID,POSTING_TYPE,GENERATION_DATE,POSTING_DATE,ACCOUNT_NUMBER,ACCOUNT_CURRENCY,AMOUNT,CREDIT_DEBIT,THIRD_PARTY_ID,INSTRUMENT_ID,CURRENCY,QUANTITY,SIGN,AUXILIARY1_ACCOUNT,AUXILIARY2_ACCOUNT,COMMENTS,JOURNAL_ENTRY,STATUS,AUXILLARY_DATE,ACCOUNT_NAME_ID,AUXILIARY1_ACCOUNT_ID,AUXILIARY2_ACCOUNT_ID,LINK_ID,RULE_TYPE,AUXILIARY3_ACCOUNT,AUXILIARY3_ACCOUNT_ID,POSITION_ID,AUXILIARY_ID,ACCOUNTING_BOOK_ID,AMORTIZING_RULE_ID,AMOUNT_CURRENCY,TRADE_TYPE,ORIGINAL_POSTING_DATE)
        values (lID,lACCOUNT_ENTITY_ID,lPOSTING_RULE_ID,lTRADE_ID,lVERSION_ID,lPOSTING_TYPE,lGENERATION_DATE,lPOSTING_DATE,lACCOUNT_NUMBER,lACCOUNT_CURRENCY,lAMOUNT,lCREDIT_DEBIT,lTHIRD_PARTY_ID,lINSTRUMENT_ID,lCURRENCY,lQUANTITY,lSIGN,lAUXILIARY1_ACCOUNT,lAUXILIARY2_ACCOUNT,lCOMMENTS,lJOURNAL_ENTRY,lSTATUS,lAUXILLARY_DATE,lACCOUNT_NAME_ID,lAUXILIARY1_ACCOUNT_ID,lAUXILIARY2_ACCOUNT_ID,lLINK_ID,lRULE_TYPE,lAUXILIARY3_ACCOUNT,lAUXILIARY3_ACCOUNT_ID,lPOSITION_ID,lAUXILIARY_ID,lACCOUNTING_BOOK_ID,lAMORTIZING_RULE_ID,lAMOUNT_CURRENCY,lTRADE_TYPE,lORIGINAL_POSTING_DATE);
        commit;
        
end loop;

END;


------------------------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------------------------
create or replace
PROCEDURE CFG_TREAT_REPO_EXPIRY_POSTING (pRepoPostingType in number,pLinkedRepoPostingType in number,pNavDate in Date,pAccEntId in number) as

lSizeTable number;
lAveragePrice number;
lCurrentPostingId number;
lRepoInitiationTradeDate HISTOMVTS.DATENEG%TYPE;
lRepoInitiationTradeId HISTOMVTS.REFCON%TYPE;

lID ACCOUNT_POSTING.ID%TYPE;
lACCOUNT_ENTITY_ID ACCOUNT_POSTING.ACCOUNT_ENTITY_ID%TYPE;
lPOSTING_RULE_ID ACCOUNT_POSTING.POSTING_RULE_ID%TYPE;
lTRADE_ID ACCOUNT_POSTING.TRADE_ID%TYPE;
lVERSION_ID ACCOUNT_POSTING.VERSION_ID%TYPE;
lPOSTING_TYPE ACCOUNT_POSTING.POSTING_TYPE%TYPE;
lGENERATION_DATE ACCOUNT_POSTING.GENERATION_DATE%TYPE;
lPOSTING_DATE ACCOUNT_POSTING.POSTING_DATE%TYPE;
lACCOUNT_NUMBER ACCOUNT_POSTING.ACCOUNT_NUMBER%TYPE;
lACCOUNT_CURRENCY ACCOUNT_POSTING.ACCOUNT_CURRENCY%TYPE;
lAMOUNT ACCOUNT_POSTING.AMOUNT%TYPE;
lCREDIT_DEBIT ACCOUNT_POSTING.CREDIT_DEBIT%TYPE;
lTHIRD_PARTY_ID ACCOUNT_POSTING.THIRD_PARTY_ID%TYPE;
lINSTRUMENT_ID ACCOUNT_POSTING.INSTRUMENT_ID%TYPE;
lCURRENCY ACCOUNT_POSTING.CURRENCY%TYPE;
lQUANTITY ACCOUNT_POSTING.QUANTITY%TYPE;
lSIGN ACCOUNT_POSTING.SIGN%TYPE;
lAUXILIARY1_ACCOUNT ACCOUNT_POSTING.AUXILIARY1_ACCOUNT%TYPE;
lAUXILIARY2_ACCOUNT ACCOUNT_POSTING.AUXILIARY2_ACCOUNT%TYPE;
lCOMMENTS ACCOUNT_POSTING.COMMENTS%TYPE;
lJOURNAL_ENTRY ACCOUNT_POSTING.JOURNAL_ENTRY%TYPE;
lSTATUS ACCOUNT_POSTING.STATUS%TYPE;
lAUXILLARY_DATE ACCOUNT_POSTING.AUXILLARY_DATE%TYPE;
lACCOUNT_NAME_ID ACCOUNT_POSTING.ACCOUNT_NAME_ID%TYPE;
lAUXILIARY1_ACCOUNT_ID ACCOUNT_POSTING.AUXILIARY1_ACCOUNT_ID%TYPE;
lAUXILIARY2_ACCOUNT_ID ACCOUNT_POSTING.AUXILIARY2_ACCOUNT_ID%TYPE;
lLINK_ID ACCOUNT_POSTING.LINK_ID%TYPE;
lRULE_TYPE ACCOUNT_POSTING.RULE_TYPE%TYPE;
lAUXILIARY3_ACCOUNT ACCOUNT_POSTING.AUXILIARY3_ACCOUNT%TYPE;
lAUXILIARY3_ACCOUNT_ID ACCOUNT_POSTING.AUXILIARY3_ACCOUNT_ID%TYPE;
lPOSITION_ID ACCOUNT_POSTING.POSITION_ID%TYPE;
lAUXILIARY_ID ACCOUNT_POSTING.AUXILIARY_ID%TYPE;
lACCOUNTING_BOOK_ID ACCOUNT_POSTING.ACCOUNTING_BOOK_ID%TYPE;
lAMORTIZING_RULE_ID ACCOUNT_POSTING.AMORTIZING_RULE_ID%TYPE;
lAMOUNT_CURRENCY ACCOUNT_POSTING.AMOUNT_CURRENCY%TYPE;
lTRADE_TYPE ACCOUNT_POSTING.TRADE_TYPE%TYPE;
lORIGINAL_POSTING_DATE ACCOUNT_POSTING.ORIGINAL_POSTING_DATE%TYPE;

lNEWAMOUNT ACCOUNT_POSTING.AMOUNT%TYPE;

BEGIN

-- Fill table cfg_repo_posting_to_treat
execute immediate 'truncate table cfg_repo_posting_to_treat';
insert into cfg_repo_posting_to_treat select * from account_posting where posting_type=pRepoPostingType and status = 2 and posting_date<=pNavDate and account_entity_id=pAccEntId order by trade_id asc;
commit;

select count(*) into lSizeTable from cfg_repo_posting_to_treat;

DBMS_OUTPUT.PUT_LINE(lSizeTable);

FOR i IN 0 .. lSizeTable-1 LOOP

        -- Load current posting data
        select id into lCurrentPostingId from (select id,row_number() over (order by Trade_id) num from cfg_repo_posting_to_treat) where num=i+1;
        select ACCOUNT_ENTITY_ID,POSTING_RULE_ID,TRADE_ID,VERSION_ID,POSTING_TYPE,GENERATION_DATE,POSTING_DATE,ACCOUNT_NUMBER,ACCOUNT_CURRENCY,AMOUNT,CREDIT_DEBIT,THIRD_PARTY_ID,INSTRUMENT_ID,CURRENCY,QUANTITY,SIGN,AUXILIARY1_ACCOUNT,AUXILIARY2_ACCOUNT,COMMENTS,JOURNAL_ENTRY,STATUS,AUXILLARY_DATE,ACCOUNT_NAME_ID,AUXILIARY1_ACCOUNT_ID,AUXILIARY2_ACCOUNT_ID,LINK_ID,RULE_TYPE,AUXILIARY3_ACCOUNT,AUXILIARY3_ACCOUNT_ID,POSITION_ID,AUXILIARY_ID,ACCOUNTING_BOOK_ID,AMORTIZING_RULE_ID,AMOUNT_CURRENCY,TRADE_TYPE,ORIGINAL_POSTING_DATE
        into lACCOUNT_ENTITY_ID,lPOSTING_RULE_ID,lTRADE_ID,lVERSION_ID,lPOSTING_TYPE,lGENERATION_DATE,lPOSTING_DATE,lACCOUNT_NUMBER,lACCOUNT_CURRENCY,lAMOUNT,lCREDIT_DEBIT,lTHIRD_PARTY_ID,lINSTRUMENT_ID,lCURRENCY,lQUANTITY,lSIGN,lAUXILIARY1_ACCOUNT,lAUXILIARY2_ACCOUNT,lCOMMENTS,lJOURNAL_ENTRY,lSTATUS,lAUXILLARY_DATE,lACCOUNT_NAME_ID,lAUXILIARY1_ACCOUNT_ID,lAUXILIARY2_ACCOUNT_ID,lLINK_ID,lRULE_TYPE,lAUXILIARY3_ACCOUNT,lAUXILIARY3_ACCOUNT_ID,lPOSITION_ID,lAUXILIARY_ID,lACCOUNTING_BOOK_ID,lAMORTIZING_RULE_ID,lAMOUNT_CURRENCY,lTRADE_TYPE,lORIGINAL_POSTING_DATE from account_posting where id=lCurrentPostingId;

        -- Retrieve trade date of repo initiation
        select dateneg into lRepoInitiationTradeDate from histomvts where refcon in (select max(refcon) from histomvts h where h.mvtident=lPOSITION_ID and h.type=158);
        select refcon into lRepoInitiationTradeId from histomvts where refcon in (select max(refcon) from histomvts h where h.mvtident=lPOSITION_ID and h.type=158);
        select CFG_AVERAGE_PRICE(lRepoInitiationTradeDate,lACCOUNT_NUMBER,lINSTRUMENT_ID,lACCOUNT_CURRENCY,lACCOUNT_ENTITY_ID,lCOMMENTS,lRepoInitiationTradeId) into lAveragePrice from dual;
        lNEWAMOUNT:=round(abs(lQUANTITY)*lAveragePrice,2);

        -- Create a new posting with the good amount out of this posting
        
        lLINK_ID:=lCurrentPostingId; -- link with original posting
        select seqaccount.nextval into lID from dual; --set id to the next available sequence
        lGENERATION_DATE:=Sysdate; --set generation date to the Oracle System Date
        lAUXILLARY_DATE:=lPOSTING_DATE;
        lSTATUS:=7; --set status to ready AL
        lRULE_TYPE:=4; --set rule type to aux-ledger
        lPOSTING_TYPE:=6; --set posting type to simple posting
        lCOMMENTS:='AL Asset';
        select SEQACCOUNTAUXID.nextval into lAUXILIARY_ID from dual;
        
        insert into account_posting (ID,ACCOUNT_ENTITY_ID,POSTING_RULE_ID,TRADE_ID,VERSION_ID,POSTING_TYPE,GENERATION_DATE,POSTING_DATE,ACCOUNT_NUMBER,ACCOUNT_CURRENCY,AMOUNT,CREDIT_DEBIT,THIRD_PARTY_ID,INSTRUMENT_ID,CURRENCY,QUANTITY,SIGN,AUXILIARY1_ACCOUNT,AUXILIARY2_ACCOUNT,COMMENTS,JOURNAL_ENTRY,STATUS,AUXILLARY_DATE,ACCOUNT_NAME_ID,AUXILIARY1_ACCOUNT_ID,AUXILIARY2_ACCOUNT_ID,LINK_ID,RULE_TYPE,AUXILIARY3_ACCOUNT,AUXILIARY3_ACCOUNT_ID,POSITION_ID,AUXILIARY_ID,ACCOUNTING_BOOK_ID,AMORTIZING_RULE_ID,AMOUNT_CURRENCY,TRADE_TYPE,ORIGINAL_POSTING_DATE)
        values (lID,lACCOUNT_ENTITY_ID,lPOSTING_RULE_ID,lTRADE_ID,lVERSION_ID,lPOSTING_TYPE,lGENERATION_DATE,lPOSTING_DATE,lACCOUNT_NUMBER,lACCOUNT_CURRENCY,lNEWAMOUNT,lCREDIT_DEBIT,lTHIRD_PARTY_ID,lINSTRUMENT_ID,lCURRENCY,lQUANTITY,lSIGN,lAUXILIARY1_ACCOUNT,lAUXILIARY2_ACCOUNT,lCOMMENTS,lJOURNAL_ENTRY,lSTATUS,lAUXILLARY_DATE,lACCOUNT_NAME_ID,lAUXILIARY1_ACCOUNT_ID,lAUXILIARY2_ACCOUNT_ID,lLINK_ID,lRULE_TYPE,lAUXILIARY3_ACCOUNT,lAUXILIARY3_ACCOUNT_ID,lPOSITION_ID,lAUXILIARY_ID,lACCOUNTING_BOOK_ID,lAMORTIZING_RULE_ID,lAMOUNT_CURRENCY,lTRADE_TYPE,lORIGINAL_POSTING_DATE);
        commit;
        
        -- Update treated posting to reflect treatment
        update account_posting set status=8 where id = lCurrentPostingId;
        commit;
      
        -- Deal with repo posting associated
        -- Retrieve Linked Posting
        
        select ID,ACCOUNT_ENTITY_ID,POSTING_RULE_ID,TRADE_ID,VERSION_ID,POSTING_TYPE,GENERATION_DATE,POSTING_DATE,ACCOUNT_NUMBER,ACCOUNT_CURRENCY,AMOUNT,CREDIT_DEBIT,THIRD_PARTY_ID,INSTRUMENT_ID,CURRENCY,QUANTITY,SIGN,AUXILIARY1_ACCOUNT,AUXILIARY2_ACCOUNT,COMMENTS,JOURNAL_ENTRY,STATUS,AUXILLARY_DATE,ACCOUNT_NAME_ID,AUXILIARY1_ACCOUNT_ID,AUXILIARY2_ACCOUNT_ID,LINK_ID,RULE_TYPE,AUXILIARY3_ACCOUNT,AUXILIARY3_ACCOUNT_ID,POSITION_ID,AUXILIARY_ID,ACCOUNTING_BOOK_ID,AMORTIZING_RULE_ID,AMOUNT_CURRENCY,TRADE_TYPE,ORIGINAL_POSTING_DATE
        into lID,lACCOUNT_ENTITY_ID,lPOSTING_RULE_ID,lTRADE_ID,lVERSION_ID,lPOSTING_TYPE,lGENERATION_DATE,lPOSTING_DATE,lACCOUNT_NUMBER,lACCOUNT_CURRENCY,lAMOUNT,lCREDIT_DEBIT,lTHIRD_PARTY_ID,lINSTRUMENT_ID,lCURRENCY,lQUANTITY,lSIGN,lAUXILIARY1_ACCOUNT,lAUXILIARY2_ACCOUNT,lCOMMENTS,lJOURNAL_ENTRY,lSTATUS,lAUXILLARY_DATE,lACCOUNT_NAME_ID,lAUXILIARY1_ACCOUNT_ID,lAUXILIARY2_ACCOUNT_ID,lLINK_ID,lRULE_TYPE,lAUXILIARY3_ACCOUNT,lAUXILIARY3_ACCOUNT_ID,lPOSITION_ID,lAUXILIARY_ID,lACCOUNTING_BOOK_ID,lAMORTIZING_RULE_ID,lAMOUNT_CURRENCY,lTRADE_TYPE,lORIGINAL_POSTING_DATE from account_posting 
        where trade_id=lTRADE_ID and posting_type=pLinkedRepoPostingType and status = 2;

        -- Update treated posting to reflect treatment
        update account_posting set status=8 where id = lId;
        commit;
        
        -- Create a new posting with the good amount out of this posting
        
        lLINK_ID:=lID; -- link with original posting
        select seqaccount.nextval into lID from dual; --set id to the next available sequence
        lGENERATION_DATE:=Sysdate; --set generation date to the Oracle System Date
        lAMOUNT_CURRENCY:=lNEWAMOUNT; --set also the amount in currency to the good amount
        lSTATUS:=7; --set status to ready AL
        lRULE_TYPE:=4; --set posting of AL
        lPOSTING_TYPE:=6; --set posting type to simple posting
        lCOMMENTS:='Expiry Repo Posting';

        insert into account_posting (ID,ACCOUNT_ENTITY_ID,POSTING_RULE_ID,TRADE_ID,VERSION_ID,POSTING_TYPE,GENERATION_DATE,POSTING_DATE,ACCOUNT_NUMBER,ACCOUNT_CURRENCY,AMOUNT,CREDIT_DEBIT,THIRD_PARTY_ID,INSTRUMENT_ID,CURRENCY,QUANTITY,SIGN,AUXILIARY1_ACCOUNT,AUXILIARY2_ACCOUNT,COMMENTS,JOURNAL_ENTRY,STATUS,AUXILLARY_DATE,ACCOUNT_NAME_ID,AUXILIARY1_ACCOUNT_ID,AUXILIARY2_ACCOUNT_ID,LINK_ID,RULE_TYPE,AUXILIARY3_ACCOUNT,AUXILIARY3_ACCOUNT_ID,POSITION_ID,AUXILIARY_ID,ACCOUNTING_BOOK_ID,AMORTIZING_RULE_ID,AMOUNT_CURRENCY,TRADE_TYPE,ORIGINAL_POSTING_DATE)
        values (lID,lACCOUNT_ENTITY_ID,lPOSTING_RULE_ID,lTRADE_ID,lVERSION_ID,lPOSTING_TYPE,lGENERATION_DATE,lPOSTING_DATE,lACCOUNT_NUMBER,lACCOUNT_CURRENCY,lNEWAMOUNT,lCREDIT_DEBIT,lTHIRD_PARTY_ID,lINSTRUMENT_ID,lCURRENCY,lQUANTITY,lSIGN,lAUXILIARY1_ACCOUNT,lAUXILIARY2_ACCOUNT,lCOMMENTS,lJOURNAL_ENTRY,lSTATUS,lAUXILLARY_DATE,lACCOUNT_NAME_ID,lAUXILIARY1_ACCOUNT_ID,lAUXILIARY2_ACCOUNT_ID,lLINK_ID,lRULE_TYPE,lAUXILIARY3_ACCOUNT,lAUXILIARY3_ACCOUNT_ID,lPOSITION_ID,lAUXILIARY_ID,lACCOUNTING_BOOK_ID,lAMORTIZING_RULE_ID,lAMOUNT_CURRENCY,lTRADE_TYPE,lORIGINAL_POSTING_DATE);
        commit;
        
end loop;

END;

------------------------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------------------------
create or replace
PROCEDURE CFG_TREAT_REPO_INIT_POSTING (pRepoPostingType in number,pLinkedRepoPostingType in number,pNavDate in Date,pAccEntId in number) as

lSizeTable number;
lAveragePrice number;
lCurrentPostingId number;

lID ACCOUNT_POSTING.ID%TYPE;
lACCOUNT_ENTITY_ID ACCOUNT_POSTING.ACCOUNT_ENTITY_ID%TYPE;
lPOSTING_RULE_ID ACCOUNT_POSTING.POSTING_RULE_ID%TYPE;
lTRADE_ID ACCOUNT_POSTING.TRADE_ID%TYPE;
lVERSION_ID ACCOUNT_POSTING.VERSION_ID%TYPE;
lPOSTING_TYPE ACCOUNT_POSTING.POSTING_TYPE%TYPE;
lGENERATION_DATE ACCOUNT_POSTING.GENERATION_DATE%TYPE;
lPOSTING_DATE ACCOUNT_POSTING.POSTING_DATE%TYPE;
lACCOUNT_NUMBER ACCOUNT_POSTING.ACCOUNT_NUMBER%TYPE;
lACCOUNT_CURRENCY ACCOUNT_POSTING.ACCOUNT_CURRENCY%TYPE;
lAMOUNT ACCOUNT_POSTING.AMOUNT%TYPE;
lCREDIT_DEBIT ACCOUNT_POSTING.CREDIT_DEBIT%TYPE;
lTHIRD_PARTY_ID ACCOUNT_POSTING.THIRD_PARTY_ID%TYPE;
lINSTRUMENT_ID ACCOUNT_POSTING.INSTRUMENT_ID%TYPE;
lCURRENCY ACCOUNT_POSTING.CURRENCY%TYPE;
lQUANTITY ACCOUNT_POSTING.QUANTITY%TYPE;
lSIGN ACCOUNT_POSTING.SIGN%TYPE;
lAUXILIARY1_ACCOUNT ACCOUNT_POSTING.AUXILIARY1_ACCOUNT%TYPE;
lAUXILIARY2_ACCOUNT ACCOUNT_POSTING.AUXILIARY2_ACCOUNT%TYPE;
lCOMMENTS ACCOUNT_POSTING.COMMENTS%TYPE;
lJOURNAL_ENTRY ACCOUNT_POSTING.JOURNAL_ENTRY%TYPE;
lSTATUS ACCOUNT_POSTING.STATUS%TYPE;
lAUXILLARY_DATE ACCOUNT_POSTING.AUXILLARY_DATE%TYPE;
lACCOUNT_NAME_ID ACCOUNT_POSTING.ACCOUNT_NAME_ID%TYPE;
lAUXILIARY1_ACCOUNT_ID ACCOUNT_POSTING.AUXILIARY1_ACCOUNT_ID%TYPE;
lAUXILIARY2_ACCOUNT_ID ACCOUNT_POSTING.AUXILIARY2_ACCOUNT_ID%TYPE;
lLINK_ID ACCOUNT_POSTING.LINK_ID%TYPE;
lRULE_TYPE ACCOUNT_POSTING.RULE_TYPE%TYPE;
lAUXILIARY3_ACCOUNT ACCOUNT_POSTING.AUXILIARY3_ACCOUNT%TYPE;
lAUXILIARY3_ACCOUNT_ID ACCOUNT_POSTING.AUXILIARY3_ACCOUNT_ID%TYPE;
lPOSITION_ID ACCOUNT_POSTING.POSITION_ID%TYPE;
lAUXILIARY_ID ACCOUNT_POSTING.AUXILIARY_ID%TYPE;
lACCOUNTING_BOOK_ID ACCOUNT_POSTING.ACCOUNTING_BOOK_ID%TYPE;
lAMORTIZING_RULE_ID ACCOUNT_POSTING.AMORTIZING_RULE_ID%TYPE;
lAMOUNT_CURRENCY ACCOUNT_POSTING.AMOUNT_CURRENCY%TYPE;
lTRADE_TYPE ACCOUNT_POSTING.TRADE_TYPE%TYPE;
lORIGINAL_POSTING_DATE ACCOUNT_POSTING.ORIGINAL_POSTING_DATE%TYPE;

lNEWAMOUNT ACCOUNT_POSTING.AMOUNT%TYPE;

BEGIN

-- Fill table cfg_repo_posting_to_treat
execute immediate 'truncate table cfg_repo_posting_to_treat';
insert into cfg_repo_posting_to_treat select * from account_posting where posting_type=pRepoPostingType and status = 2 and posting_date<=pNavDate and account_entity_id=pAccEntId order by trade_id asc;
commit;

select count(*) into lSizeTable from cfg_repo_posting_to_treat;

DBMS_OUTPUT.PUT_LINE(lSizeTable);

FOR i IN 0 .. lSizeTable-1 LOOP

        -- Load current posting data
        select id into lCurrentPostingId from (select id,row_number() over (order by Trade_id) num from cfg_repo_posting_to_treat) where num=i+1;
        select ACCOUNT_ENTITY_ID,POSTING_RULE_ID,TRADE_ID,VERSION_ID,POSTING_TYPE,GENERATION_DATE,POSTING_DATE,ACCOUNT_NUMBER,ACCOUNT_CURRENCY,AMOUNT,CREDIT_DEBIT,THIRD_PARTY_ID,INSTRUMENT_ID,CURRENCY,QUANTITY,SIGN,AUXILIARY1_ACCOUNT,AUXILIARY2_ACCOUNT,COMMENTS,JOURNAL_ENTRY,STATUS,AUXILLARY_DATE,ACCOUNT_NAME_ID,AUXILIARY1_ACCOUNT_ID,AUXILIARY2_ACCOUNT_ID,LINK_ID,RULE_TYPE,AUXILIARY3_ACCOUNT,AUXILIARY3_ACCOUNT_ID,POSITION_ID,AUXILIARY_ID,ACCOUNTING_BOOK_ID,AMORTIZING_RULE_ID,AMOUNT_CURRENCY,TRADE_TYPE,ORIGINAL_POSTING_DATE
        into lACCOUNT_ENTITY_ID,lPOSTING_RULE_ID,lTRADE_ID,lVERSION_ID,lPOSTING_TYPE,lGENERATION_DATE,lPOSTING_DATE,lACCOUNT_NUMBER,lACCOUNT_CURRENCY,lAMOUNT,lCREDIT_DEBIT,lTHIRD_PARTY_ID,lINSTRUMENT_ID,lCURRENCY,lQUANTITY,lSIGN,lAUXILIARY1_ACCOUNT,lAUXILIARY2_ACCOUNT,lCOMMENTS,lJOURNAL_ENTRY,lSTATUS,lAUXILLARY_DATE,lACCOUNT_NAME_ID,lAUXILIARY1_ACCOUNT_ID,lAUXILIARY2_ACCOUNT_ID,lLINK_ID,lRULE_TYPE,lAUXILIARY3_ACCOUNT,lAUXILIARY3_ACCOUNT_ID,lPOSITION_ID,lAUXILIARY_ID,lACCOUNTING_BOOK_ID,lAMORTIZING_RULE_ID,lAMOUNT_CURRENCY,lTRADE_TYPE,lORIGINAL_POSTING_DATE from account_posting where id=lCurrentPostingId;
        
        -- Get the good average price for each posting
        select CFG_AVERAGE_PRICE(lPOSTING_DATE,lACCOUNT_NUMBER,lINSTRUMENT_ID,lACCOUNT_CURRENCY,lACCOUNT_ENTITY_ID,lCOMMENTS,lTRADE_ID) into lAveragePrice from dual;
        
        -- Create a new posting with the good amount out of this posting
        
        lLINK_ID:=lCurrentPostingId; -- link with original posting
        select seqaccount.nextval into lID from dual; --set id to the next available sequence
        lGENERATION_DATE:=Sysdate; --set generation date to the Oracle System Date
        lNEWAMOUNT:=round(abs(lQUANTITY)*lAveragePrice,2); --set amount to the average price of the position
        lAMOUNT_CURRENCY:= lNEWAMOUNT; --set also amount in original currency to the right amount
        lRULE_TYPE:=4; --set rule type to aux-ledger
        lSTATUS:=7; --set status to ready AL
        lPOSTING_TYPE:=6; --set posting type to simple posting
        lCOMMENTS:='AL Asset';
        select SEQACCOUNTAUXID.nextval into lAUXILIARY_ID from dual;
        
        insert into account_posting (ID,ACCOUNT_ENTITY_ID,POSTING_RULE_ID,TRADE_ID,VERSION_ID,POSTING_TYPE,GENERATION_DATE,POSTING_DATE,ACCOUNT_NUMBER,ACCOUNT_CURRENCY,AMOUNT,CREDIT_DEBIT,THIRD_PARTY_ID,INSTRUMENT_ID,CURRENCY,QUANTITY,SIGN,AUXILIARY1_ACCOUNT,AUXILIARY2_ACCOUNT,COMMENTS,JOURNAL_ENTRY,STATUS,AUXILLARY_DATE,ACCOUNT_NAME_ID,AUXILIARY1_ACCOUNT_ID,AUXILIARY2_ACCOUNT_ID,LINK_ID,RULE_TYPE,AUXILIARY3_ACCOUNT,AUXILIARY3_ACCOUNT_ID,POSITION_ID,AUXILIARY_ID,ACCOUNTING_BOOK_ID,AMORTIZING_RULE_ID,AMOUNT_CURRENCY,TRADE_TYPE,ORIGINAL_POSTING_DATE)
        values (lID,lACCOUNT_ENTITY_ID,lPOSTING_RULE_ID,lTRADE_ID,lVERSION_ID,lPOSTING_TYPE,lGENERATION_DATE,lPOSTING_DATE,lACCOUNT_NUMBER,lACCOUNT_CURRENCY,lNEWAMOUNT,lCREDIT_DEBIT,lTHIRD_PARTY_ID,lINSTRUMENT_ID,lCURRENCY,lQUANTITY,lSIGN,lAUXILIARY1_ACCOUNT,lAUXILIARY2_ACCOUNT,lCOMMENTS,lJOURNAL_ENTRY,lSTATUS,lAUXILLARY_DATE,lACCOUNT_NAME_ID,lAUXILIARY1_ACCOUNT_ID,lAUXILIARY2_ACCOUNT_ID,lLINK_ID,lRULE_TYPE,lAUXILIARY3_ACCOUNT,lAUXILIARY3_ACCOUNT_ID,lPOSITION_ID,lAUXILIARY_ID,lACCOUNTING_BOOK_ID,lAMORTIZING_RULE_ID,lAMOUNT_CURRENCY,lTRADE_TYPE,lORIGINAL_POSTING_DATE);
        commit;
        
        -- Update treated posting to reflect treatment
        update account_posting set status=8 where id = lCurrentPostingId;
        commit;
      
        -- Deal with repo posting associated
        -- Retrieve Linked Posting
        
        select ID,ACCOUNT_ENTITY_ID,POSTING_RULE_ID,TRADE_ID,VERSION_ID,POSTING_TYPE,GENERATION_DATE,POSTING_DATE,ACCOUNT_NUMBER,ACCOUNT_CURRENCY,AMOUNT,CREDIT_DEBIT,THIRD_PARTY_ID,INSTRUMENT_ID,CURRENCY,QUANTITY,SIGN,AUXILIARY1_ACCOUNT,AUXILIARY2_ACCOUNT,COMMENTS,JOURNAL_ENTRY,STATUS,AUXILLARY_DATE,ACCOUNT_NAME_ID,AUXILIARY1_ACCOUNT_ID,AUXILIARY2_ACCOUNT_ID,LINK_ID,RULE_TYPE,AUXILIARY3_ACCOUNT,AUXILIARY3_ACCOUNT_ID,POSITION_ID,AUXILIARY_ID,ACCOUNTING_BOOK_ID,AMORTIZING_RULE_ID,AMOUNT_CURRENCY,TRADE_TYPE,ORIGINAL_POSTING_DATE
        into lID,lACCOUNT_ENTITY_ID,lPOSTING_RULE_ID,lTRADE_ID,lVERSION_ID,lPOSTING_TYPE,lGENERATION_DATE,lPOSTING_DATE,lACCOUNT_NUMBER,lACCOUNT_CURRENCY,lAMOUNT,lCREDIT_DEBIT,lTHIRD_PARTY_ID,lINSTRUMENT_ID,lCURRENCY,lQUANTITY,lSIGN,lAUXILIARY1_ACCOUNT,lAUXILIARY2_ACCOUNT,lCOMMENTS,lJOURNAL_ENTRY,lSTATUS,lAUXILLARY_DATE,lACCOUNT_NAME_ID,lAUXILIARY1_ACCOUNT_ID,lAUXILIARY2_ACCOUNT_ID,lLINK_ID,lRULE_TYPE,lAUXILIARY3_ACCOUNT,lAUXILIARY3_ACCOUNT_ID,lPOSITION_ID,lAUXILIARY_ID,lACCOUNTING_BOOK_ID,lAMORTIZING_RULE_ID,lAMOUNT_CURRENCY,lTRADE_TYPE,lORIGINAL_POSTING_DATE from account_posting 
        where trade_id=lTRADE_ID and posting_type=pLinkedRepoPostingType and status=2;

        -- Update treated posting to reflect treatment
        update account_posting set status=8 where id = lId;
        commit;
        
        -- Create a new posting with the good amount out of this posting
        
        lLINK_ID:=lID; -- link with original posting
        select seqaccount.nextval into lID from dual; --set id to the next available sequence
        lGENERATION_DATE:=Sysdate; --set generation date to the Oracle System Date
        lNEWAMOUNT:=round(abs(lQUANTITY)*lAveragePrice,2); --set amount to the average price of the position
        lAMOUNT_CURRENCY:= lNEWAMOUNT; --set also amount in original currency to the right amount
        lRULE_TYPE:=4; --set rule type to aux-ledger
        lSTATUS:=7; --set status to ready AL
        lPOSTING_TYPE:=6; --set posting type to simple posting
        lCOMMENTS:='Initiation Repo Asset Posting';
        
        insert into account_posting (ID,ACCOUNT_ENTITY_ID,POSTING_RULE_ID,TRADE_ID,VERSION_ID,POSTING_TYPE,GENERATION_DATE,POSTING_DATE,ACCOUNT_NUMBER,ACCOUNT_CURRENCY,AMOUNT,CREDIT_DEBIT,THIRD_PARTY_ID,INSTRUMENT_ID,CURRENCY,QUANTITY,SIGN,AUXILIARY1_ACCOUNT,AUXILIARY2_ACCOUNT,COMMENTS,JOURNAL_ENTRY,STATUS,AUXILLARY_DATE,ACCOUNT_NAME_ID,AUXILIARY1_ACCOUNT_ID,AUXILIARY2_ACCOUNT_ID,LINK_ID,RULE_TYPE,AUXILIARY3_ACCOUNT,AUXILIARY3_ACCOUNT_ID,POSITION_ID,AUXILIARY_ID,ACCOUNTING_BOOK_ID,AMORTIZING_RULE_ID,AMOUNT_CURRENCY,TRADE_TYPE,ORIGINAL_POSTING_DATE)
        values (lID,lACCOUNT_ENTITY_ID,lPOSTING_RULE_ID,lTRADE_ID,lVERSION_ID,lPOSTING_TYPE,lGENERATION_DATE,lPOSTING_DATE,lACCOUNT_NUMBER,lACCOUNT_CURRENCY,lNEWAMOUNT,lCREDIT_DEBIT,lTHIRD_PARTY_ID,lINSTRUMENT_ID,lCURRENCY,lQUANTITY,lSIGN,lAUXILIARY1_ACCOUNT,lAUXILIARY2_ACCOUNT,lCOMMENTS,lJOURNAL_ENTRY,lSTATUS,lAUXILLARY_DATE,lACCOUNT_NAME_ID,lAUXILIARY1_ACCOUNT_ID,lAUXILIARY2_ACCOUNT_ID,lLINK_ID,lRULE_TYPE,lAUXILIARY3_ACCOUNT,lAUXILIARY3_ACCOUNT_ID,lPOSITION_ID,lAUXILIARY_ID,lACCOUNTING_BOOK_ID,lAMORTIZING_RULE_ID,lAMOUNT_CURRENCY,lTRADE_TYPE,lORIGINAL_POSTING_DATE);
        commit;
        
end loop;

END;




--}}SOPHIS_SQL