--{{SOPHIS_SQL (do not delete this line)
--check id value--
INSERT INTO order_externalsystems(name, forecolor) VALUES('RBC', 'Red');

INSERT INTO ORDER_PROPERTY (ID,CATEGORY, NAME, DATATYPE, VISIBILITY,POSSIBLEVALUES,DEFAULTVALUE) VALUES ((SELECT NVL(MAX(id), 0)FROM order_property) + 1,'RBC', 'FullRedemption', 'System.String', '1','Yes;No','No')

--insert execution property here

--}}SOPHIS_SQL