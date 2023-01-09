--{{SOPHIS_SQL (do not delete this line)

	insert into USER_RIGHT_TABLE (IDX,NAME,CATEGORY,COMMENTS,INTERNAL_RIGHT,RIGHT,RIGHT_TYPE) 
	values ((select max(idx)+1 from USER_RIGHT_TABLE),'See Expired FX Frwd','Medio','Right to see expired FX positions',0, -1, 2);

--}}SOPHIS_SQL