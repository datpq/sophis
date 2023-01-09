--{{SOPHIS_SQL (do not delete this line)

begin
    tk_add_histomvts_column('TKT_RBC_TRADE_ID', 'Date', NULL, 'RBC Transaction ID');
end;

-- adding Trading target portfolio parameter
INSERT INTO MEDIO_TKT_CONFIG VALUES('Trading_Target_Folio_Name','MIFL','Name of main trading portfolio');
-- Changing portfolio Names
update FOLIO set name = 'MIFL' where name = 'MAML';
update FOLIO set name = 'MIFL Cash' where name = 'MAML Cash';

commit;

-- since v

begin
    tk_add_histomvts_column('MEDIO_GROSS_CONS_AMOUNT', 'NUMBER', NULL, 'MEDIO Gross Consideration Amount');
end;


begin
    tk_add_histomvts_column('MEDIO_CDS_IMPORTPRICE', 'NUMBER', NULL, 'MEDIO import price from Bloomberg for CDS/CDX deals');
end;
--}}SOPHIS_SQL