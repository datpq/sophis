--{{SOPHIS_SQL (do not delete this line)

update FUND_FEES_CFGMANAGEMENT set NAVType = 5 where amount_type = 2 and navtype != 5;

commit;

--}}SOPHIS_SQL