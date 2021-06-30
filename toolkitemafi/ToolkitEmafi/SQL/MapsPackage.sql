CREATE OR REPLACE PACKAGE EM
AS
	PROCEDURE GENERATEALL;
	FUNCTION GENERATE_INSERT(p_tableName VARCHAR2, p_whereClause VARCHAR2, p_recursive boolean default false,
		p_simpleInsert boolean default false, p_backupSuffix VARCHAR2 default null) RETURN CLOB;
	PROCEDURE CLEANUP(p_TableNamePrefix VARCHAR2 default 'MAPS_BK_', p_TableNameSuffix VARCHAR2 default null);
	PROCEDURE RESTORE(p_TableNamePrefix VARCHAR2 default 'MAPS_BK_', p_TableNameSuffix VARCHAR2 default null);
	PROCEDURE FIND_COLUMNVALUE(p_tableLike VARCHAR2, p_columnName VARCHAR2, p_columnValue NUMBER);
END EM;
/

CREATE OR REPLACE PACKAGE BODY EM
AS
	PROCEDURE FIND_COLUMNVALUE(p_tableLike VARCHAR2, p_columnName VARCHAR2, p_columnValue NUMBER)
	AS
		CURSOR curTables IS
			SELECT T.TABLE_NAME
			FROM USER_TABLES T
				JOIN USER_TAB_COLUMNS C ON C.TABLE_NAME = T.TABLE_NAME AND C.COLUMN_NAME = p_columnName AND C.DATA_TYPE = 'NUMBER'
			WHERE T.TABLE_NAME LIKE p_tableLike;
		tableName USER_TABLES.TABLE_NAME%TYPE;
		rc integer;
	BEGIN
		open curTables;
		loop
			fetch curTables into tableName;
			exit when curTables%NOTFOUND;
			EXECUTE IMMEDIATE EF.FORMAT('SELECT COUNT(*) FROM {1} WHERE {2} = {3}', tableName, p_columnName, p_columnValue) INTO rc;
			if (rc > 0) then
				EF.INFO('Found table {1} with {2} = {3}', tableName, p_columnName, p_columnValue);
			end if;
		end loop;
		close curTables;
	END;
	PROCEDURE GENERATEALL
	AS
		CURSOR curTables IS
			SELECT DISTINCT TABLE_NAME, NIV FROM (
			SELECT TABLE_NAME, NIV FROM (
				SELECT TABLE_NAME, R_TABLE_NAME, LEVEL NIV FROM (
				SELECT TABLE_NAME, R_TABLE_NAME FROM MAPS_FK
				UNION
				SELECT R_TABLE_NAME, NULL FROM MAPS_FK WHERE R_TABLE_NAME NOT IN (SELECT TABLE_NAME FROM MAPS_FK))
				CONNECT BY PRIOR TABLE_NAME = R_TABLE_NAME) T1
			WHERE NIV = (SELECT MAX(NIV) FROM (
				SELECT TABLE_NAME, R_TABLE_NAME, LEVEL NIV FROM (
				SELECT TABLE_NAME, R_TABLE_NAME FROM MAPS_FK
				UNION
				SELECT R_TABLE_NAME, NULL FROM MAPS_FK WHERE R_TABLE_NAME NOT IN (SELECT TABLE_NAME FROM MAPS_FK))
				CONNECT BY PRIOR TABLE_NAME = R_TABLE_NAME) T2
				WHERE T2.TABLE_NAME = T1.TABLE_NAME)
				AND TABLE_NAME IN (SELECT TABLE_NAME FROM MAPS_TABLES WHERE ENABLED = 1)
			)
			ORDER BY NIV, TABLE_NAME;
		resultStr CLOB;
		pTableRow curTables%ROWTYPE;
		pBackupSuffix VARCHAR2(20);
	BEGIN
		SELECT TO_CHAR(SYSDATE, 'YYMMDDHH24MI') INTO pBackupSuffix FROM DUAL;
		open curTables;
		loop
			fetch curTables into pTableRow;
			exit when curTables%NOTFOUND;
			resultStr := GENERATE_INSERT(
				p_tableName => pTableRow.TABLE_NAME,
				p_whereClause => '1 = 1',
				p_simpleInsert => pTableRow.TABLE_NAME not in ('ACCOUNT_DEAL_RULES_FIRST', 'ACCOUNT_AUX_LEDGER_RULES_FIRST', 'ACCOUNT_NAME', ' ACCOUNT_ENTITY', 'ACCOUNT_PNL_RULES', 'ACCOUNT_RULES'),
				p_backupSuffix => pBackupSuffix);
		end loop;
		close curTables;
	END;

    FUNCTION GENERATE_INSERT(p_tableName VARCHAR2, p_whereClause VARCHAR2, p_recursive boolean default false,
		p_simpleInsert boolean default false, p_backupSuffix VARCHAR2 default null) RETURN CLOB
    AS
		fieldList VARCHAR2(8000);
		fieldValueList NVARCHAR2(8000);
		fieldValueListSql VARCHAR2(8000);
		resultStr CLOB; subResultStr CLOB;
		CURSOR curCols IS
			SELECT C.COLUMN_NAME, C.DATA_TYPE
			FROM USER_TAB_COLUMNS C
                JOIN MAPS_TABLES T ON T.TABLE_NAME = C.TABLE_NAME
                    AND INSTR(',' || T.OFF_COLUMNS || ',', ',' || C.COLUMN_NAME || ',') = 0
			WHERE C.TABLE_NAME = p_tableName
            ORDER BY C.COLUMN_ID;
		col curCols%ROWTYPE;
		curFkCols SYS_REFCURSOR;
		curIns SYS_REFCURSOR;
		curInsQuery CLOB;
		curInsQueryJoin VARCHAR2(8000);
		joinCount integer;
		insertLine VARCHAR2(8000);
		curLines SYS_REFCURSOR;
		curLinesQuery VARCHAR2(8000);
		fkValue VARCHAR2(255);
		CURSOR curFks IS
			SELECT TABLE_NAME, COLUMN_NAME, R_TABLE_NAME, R_COLUMN_NAME
			FROM MAPS_FK WHERE R_TABLE_NAME = p_tableName ORDER BY TABLE_NAME;
		fk curFks%ROWTYPE;
		CURSOR curRefTables IS
			SELECT FK.R_TABLE_NAME, FK.COLUMN_NAME
			FROM MAPS_FK FK
				JOIN USER_TAB_COLUMNS C ON C.TABLE_NAME = FK.TABLE_NAME AND C.COLUMN_NAME = FK.COLUMN_NAME AND C.DATA_TYPE = 'NUMBER'
			WHERE FK.TABLE_NAME = p_tableName;
		rtRow curRefTables%ROWTYPE;
		rc integer;
		pos integer;
		fkRow MAPS_FK%ROWTYPE;
		pTableRow MAPS_TABLES%ROWTYPE;
		rTableRow MAPS_TABLES%ROWTYPE;
		maxField MAPS_TABLES.MAXBY%TYPE;
		seqName MAPS_TABLES.SEQUENCES%TYPE;
		seqField MAPS_TABLES.SEQUENCES%TYPE;
		pkField MAPS_FK.COLUMN_NAME%TYPE;
		pkFieldType USER_TAB_COLUMNS.DATA_TYPE%TYPE;
		pkLine VARCHAR2(8000);
		pkAllLines VARCHAR2(8000);
		pkLineWhere VARCHAR2(8000);
		ifExistWhere NVARCHAR2(8000);
		temp VARCHAR2(8000);
		tempB boolean;
    BEGIN
        EF.SET_LOG_LEVEL(EF.LOG_DEBUG);
		EF.DEBUG('---GENERATE_INSERT.BEGIN({1} WHERE {2}), p_recursive = {3}, p_simpleInsert = {4}', p_tableName, p_whereClause,
			case when p_recursive then 'true' else 'false' end, case when p_simpleInsert then 'true' else 'false' end);
		EXECUTE IMMEDIATE EF.FORMAT('SELECT COUNT(*) FROM USER_TABLES WHERE TABLE_NAME = ''{1}''', p_tableName) INTO rc;
		if (rc = 0) then return null; end if;
		EXECUTE IMMEDIATE EF.FORMAT('SELECT COUNT(*) FROM {1} T WHERE {2}', p_tableName, p_whereClause) INTO rc;
		if (rc = 0) then return null; end if;
		
		SELECT * INTO pTableRow FROM MAPS_TABLES WHERE TABLE_NAME = p_tableName;
		
		EF.DEBUG('CREATE TABLE {1} AS SELECT * FROM {2};', REPLACE(pTableRow.BK_TABLE_NAME, '{TIMESTAMP}', p_backupSuffix), p_tableName);
		
		-- get IfExist where clause based on PK_COLUMNS
		ifExistWhere := NULL;
		if pTableRow.PK_COLUMNS IS NOT NULL THEN
			loop
				pos := INSTR(pTableRow.PK_COLUMNS, ',');
				if (pos = 0) then
					pkField := pTableRow.PK_COLUMNS;
				else
					pkField := TRIM(SUBSTR(pTableRow.PK_COLUMNS, 1, pos - 1));
					pTableRow.PK_COLUMNS := TRIM(SUBSTR(pTableRow.PK_COLUMNS, pos + 1));
				end if;
				SELECT DATA_TYPE INTO pkFieldType FROM USER_TAB_COLUMNS WHERE TABLE_NAME = pTableRow.TABLE_NAME AND COLUMN_NAME = pkField;
				if pkFieldType = 'VARCHAR2' then
					ifExistWhere := EF.FORMAT('{1} || '' AND {2} = '''''' || T.{2} || ''''''''', ifExistWhere, pkField);
				else
					ifExistWhere := EF.FORMAT('{1} || '' AND {2} = '' || NVL(TO_CHAR(T.{2}), ''NULL'')', ifExistWhere, pkField);
				end if;
				exit when pos = 0;
			end loop;
			ifExistWhere := SUBSTR(ifExistWhere, 11);
			--EF.DEBUG('ifExistWhere = {1}', ifExistWhere);
		END IF;
		
		joinCount := 0;
		seqName := NULL;
		seqField := NULL;
		curInsQueryJoin := NULL;
		fieldList := NULL;
		fieldValueList := NULL;
		fieldValueListSql := NULL;
		-- browse all columns of the table
		open curCols;
		loop
			fetch curCols into col;
			exit when curCols%NOTFOUND;
			fieldList := fieldList || ', ' || col.COLUMN_NAME;
			if col.DATA_TYPE != 'CLOB' then
				fieldValueList := EF.FORMAT('{1} || '','' || REPLACE(T.{2}, '''''''', '''''''''''')', fieldValueList, col.COLUMN_NAME);
			end if;
			-- EF.DEBUG(fieldList);
			maxField := NULL;
			-- PRIORITY@LINK_ID --> (SELECT NVL(MAX(PRIORITY), 0) + 1 FROM ACCOUNT_DEAL_RULES WHERE LINK_ID IN (SELECT ID FROM ACCOUNT_DEAL_RULES_FIRST WHERE NAME = '...name...' AND RECORD_TYPE = 1))
			if (INSTR(pTableRow.MAXBY, col.COLUMN_NAME || '@') > 0) then -- TODO treat the case where there's multiple columns of type MAXBY
				maxField := SUBSTR(pTableRow.MAXBY, length(col.COLUMN_NAME) + 2);
				SELECT COUNT(*) INTO rc FROM MAPS_FK F
					JOIN MAPS_TABLES T ON T.TABLE_NAME = F.R_TABLE_NAME AND T.PK_COLUMNS IS NOT NULL
				WHERE F.TABLE_NAME = p_tableName AND F.COLUMN_NAME = maxField;
			else
				SELECT COUNT(*) INTO rc FROM MAPS_FK F
					JOIN MAPS_TABLES T ON T.TABLE_NAME = F.R_TABLE_NAME AND T.PK_COLUMNS IS NOT NULL
				WHERE F.TABLE_NAME = p_tableName AND F.COLUMN_NAME = col.COLUMN_NAME;
			end if;
			-- if it's a foreign key ACCOUNT_POSTING_RULES.RULE_ID --> (SELECT ID FROM ACCOUNT_RULES WHERE NAME = '...name...' AND RECORD_TYPE = 1)
			if rc > 0 then
				pkAllLines := NULL;
				open curFkCols for SELECT * FROM MAPS_FK WHERE TABLE_NAME = p_tableName AND COLUMN_NAME = NVL(maxField, col.COLUMN_NAME);
				loop
					fetch curFkCols into fkRow;
					exit when curFkCols%NOTFOUND;
					joinCount := joinCount + 1;
					SELECT * INTO rTableRow FROM MAPS_TABLES WHERE TABLE_NAME = fkRow.R_TABLE_NAME;
					if rTableRow.PK_COLUMNS is null then
						pkLine := EF.FORMAT('NVL(TO_CHAR(R{1}.{2}), ''NULL'')', joinCount, fkRow.R_COLUMN_NAME);
					else
						pkLineWhere := NULL;
						loop
							pos := INSTR(rTableRow.PK_COLUMNS, ',');
							if (pos = 0) then
								pkField := rTableRow.PK_COLUMNS;
							else
								pkField := SUBSTR(rTableRow.PK_COLUMNS, 1, pos - 1);
								rTableRow.PK_COLUMNS := SUBSTR(rTableRow.PK_COLUMNS, pos + 1);
							end if;
							SELECT DATA_TYPE INTO pkFieldType FROM USER_TAB_COLUMNS WHERE TABLE_NAME = fkRow.R_TABLE_NAME AND COLUMN_NAME = pkField;
							if pkFieldType = 'VARCHAR2' then
								pkLineWhere := EF.FORMAT('{1} || '' AND {2} = '''''' || R{3}.{2} || ''''''''', pkLineWhere, pkField, joinCount);
							else
								pkLineWhere := EF.FORMAT('{1} || '' AND {2} = '' || NVL(TO_CHAR(R{3}.{2}), ''NULL'')', pkLineWhere, pkField, joinCount);
							end if;
							exit when pos = 0;
						end loop;
						pkLine := EF.FORMAT('''SELECT {1} FROM {2} WHERE {3}', fkRow.R_COLUMN_NAME, fkRow.R_TABLE_NAME, SUBSTR(pkLineWhere, 11));
					end if;
					curInsQueryJoin := EF.FORMAT('{1} LEFT JOIN {2} R{3} ON R{3}.{4} = T.{5}', curInsQueryJoin, fkRow.R_TABLE_NAME, joinCount, fkRow.R_COLUMN_NAME, fkRow.COLUMN_NAME);
					--EF.DEBUG(pkLine);
					--EF.DEBUG(curInsQueryJoin);
					pkAllLines := EF.FORMAT('{1} || '', ('' || {2} || '')''', pkAllLines, pkLine);
				end loop;
				close curFkCols;
				pkAllLines := substr(pkAllLines, 8);
				-- EF.DEBUG(pkAllLines);
				tempB := fieldValueListSql is null;
				if maxField is null then
					if rc = 1 then
						fieldValueListSql := EF.FORMAT('{1} || '', COALESCE('' || ''{2} || '', NULL)''', fieldValueListSql, pkAllLines);
					else
						fieldValueListSql := EF.FORMAT('{1} || '', COALESCE('' || ''{2} || '')''', fieldValueListSql, pkAllLines);
					end if;
				else
					fieldValueListSql := EF.FORMAT('{1} || '', (SELECT NVL(MAX({3}), 0) + 1 FROM {4} WHERE {5} IN ('' || {2} || ''))''', fieldValueListSql, pkLine, col.COLUMN_NAME, p_tableName, maxField);
				end if;
				if tempB then
					fieldValueListSql := '''' || substr(fieldValueListSql, 8);
					--EF.DEBUG(fieldValueListSql);
				end if;
			-- if it's a sequence ACCOUNT_RULES.ID --> SEQACCOUNT.NEXTVAL
			elsif INSTR(pTableRow.SEQUENCES, col.COLUMN_NAME || '@') = 1 then
				tempB := fieldValueListSql is null;
				seqName := SUBSTR(pTableRow.SEQUENCES, length(col.COLUMN_NAME) + 2);
				seqField := col.COLUMN_NAME;
				fieldValueListSql := EF.FORMAT('{1} || '', '' || ''{2}.NEXTVAL''', fieldValueListSql, seqName);
				if tempB then
					fieldValueListSql := substr(fieldValueListSql, 13);
				end if;
			else
				tempB := fieldValueListSql is null;
				-- if it's a varchar ACCOUNT_RULES.NAME --> ', ''' || NAME || ''''
				if col.DATA_TYPE = 'VARCHAR2' or col.DATA_TYPE = 'CLOB' then
					fieldValueListSql := EF.FORMAT('{1} || '', '' || '''''''' || REPLACE(T.{2}, '''''''', '''''''''''') || ''''''''', fieldValueListSql, col.COLUMN_NAME);
				elsif col.DATA_TYPE = 'DATE' then
					fieldValueListSql := EF.FORMAT('{1} || '', '' || ''T.{2}''', fieldValueListSql, col.COLUMN_NAME);
				else
					-- if it's a number ACCOUNT_RULES.RECORD_TYPE --> ', ' || RECORD_TYPE
					fieldValueListSql := EF.FORMAT('{1} || '', '' || NVL(TO_CHAR(T.{2}), ''NULL'')', fieldValueListSql, col.COLUMN_NAME);
				end if;
				if tempB then
					-- if (col.DATA_TYPE = 'VARCHAR2') then
						fieldValueListSql := substr(fieldValueListSql, 13);
					-- else
						-- fieldValueListSql := substr(fieldValueListSql, 13);
					-- end if;
				end if;
				-- EF.DEBUG(fieldValueList);
			end if;
		end loop;
		close curCols;
		fieldList := substr(fieldList, 3);
		fieldValueList := substr(fieldValueList, 8);
		-- EF.DEBUG('fieldList = {1}', fieldList);
		-- EF.DEBUG('fieldValueList = {1}', fieldValueList);
		-- EF.DEBUG('fieldValueListSql = {1}', fieldValueListSql);
		temp := CASE WHEN pTableRow.ORDERBY IS NULL THEN NULL ELSE 'ORDER BY T.' || REGEXP_REPLACE(pTableRow.ORDERBY, ',( ){1,}', ', T.') END;
		if p_simpleInsert then
			IF seqName IS NULL THEN
				curInsQuery := EF.FORMAT('SELECT ''INSERT INTO {1}({2})\n\tVALUES('' || {3} || '');'' INSERTLINE FROM {1} T{6} WHERE {4} {5}', p_tableName, fieldList, fieldValueListSql, p_whereClause, temp, curInsQueryJoin);
			ELSE
				curInsQuery := EF.FORMAT('SELECT ''INSERT INTO {1}({2})\n\tVALUES('' || {3} || '');\nINSERT INTO MAPS_IDS(TABLE_NAME, OLD_ID, NEW_ID) VALUES(''''{1}'''', '' || T.{9} || '', {8}.CURRVAL);'' INSERTLINE FROM {1} T{6} WHERE {4} {5}', p_tableName, fieldList, fieldValueListSql, p_whereClause, temp, curInsQueryJoin, NULL, seqName, seqField);
			END IF;
		else
			IF ifExistWhere IS NULL THEN
				IF seqName IS NULL THEN
				curInsQuery := EF.FORMAT('SELECT ''BEGIN\n\tDBMS_OUTPUT.PUT_LINE(''''INSERT INTO {1}('''' || ''''''{7} || '''''' || '''')'''');\n\tINSERT INTO {1}({2}) VALUES('' || {3} || '');\n\tDBMS_OUTPUT.PUT_LINE(''''1 rows inserted'''');\n\t--ROWSINSERTED := ROWSINSERTED + SQL%ROWCOUNT;\n\t--COMMIT;\nEXCEPTION\n\tWHEN OTHERS THEN\n\t\tDBMS_OUTPUT.PUT_LINE(''''ERROR CODE='''' || SQLCODE || '''',MSG='''' || SQLERRM);\n\t\tDBMS_OUTPUT.PUT_LINE(''''{1}('''' || ''''''{7} || '''''' || '''')'''');\nEND;\n/'' INSERTLINE FROM {1} T{6} WHERE {4} {5}', p_tableName, fieldList, fieldValueListSql, p_whereClause, temp, curInsQueryJoin, fieldValueList);
				ELSE
				curInsQuery := EF.FORMAT('SELECT ''BEGIN\n\tDBMS_OUTPUT.PUT_LINE(''''INSERT INTO {1}('''' || ''''''{7} || '''''' || '''')'''');\n\tINSERT INTO {1}({2}) VALUES('' || {3} || '');\n\tDBMS_OUTPUT.PUT_LINE(''''1 rows inserted'''');\n\t--ROWSINSERTED := ROWSINSERTED + SQL%ROWCOUNT;\n\tINSERT INTO MAPS_IDS(TABLE_NAME, OLD_ID, NEW_ID) VALUES(''''{1}'''', '' || T.{9} || '', {8}.CURRVAL);\n\t--COMMIT;\nEXCEPTION\n\tWHEN OTHERS THEN\n\t\tDBMS_OUTPUT.PUT_LINE(''''ERROR CODE='''' || SQLCODE || '''',MSG='''' || SQLERRM);\n\t\tDBMS_OUTPUT.PUT_LINE(''''{1}('''' || ''''''{7} || '''''' || '''')'''');\nEND;\n/'' INSERTLINE FROM {1} T{6} WHERE {4} {5}', p_tableName, fieldList, fieldValueListSql, p_whereClause, temp, curInsQueryJoin, fieldValueList, seqName, seqField);
				END IF;
			ELSE
				curInsQuery := EF.FORMAT('SELECT ''BEGIN\n\tDECLARE RC INTEGER;\n\tBEGIN\n\t\tDBMS_OUTPUT.PUT_LINE(''''INSERT INTO {1}('''' || ''''''{7} || '''''' || '''')'''');\n\t\tSELECT COUNT(*) INTO RC FROM {1} WHERE {8} || '';\n\t\tIF RC = 0 THEN\n\t\t\tINSERT INTO {1}({2}) VALUES('' || {3} || '');\n\t\t\tDBMS_OUTPUT.PUT_LINE(''''1 rows inserted'''');\n\t\t\t--ROWSINSERTED := ROWSINSERTED + SQL%ROWCOUNT;\n\t\tEND IF;\n\tEND;\n\t--COMMIT;\nEXCEPTION\n\tWHEN OTHERS THEN\n\t\tDBMS_OUTPUT.PUT_LINE(''''ERROR CODE='''' || SQLCODE || '''',MSG='''' || SQLERRM);\nEND;\n/'' INSERTLINE FROM {1} T{6} WHERE {4} {5}', p_tableName, fieldList, fieldValueListSql, p_whereClause, temp, curInsQueryJoin, fieldValueList, ifExistWhere);
			END IF;
		end if;
		-- EF.DEBUG(curInsQueryJoin);
		-- EF.DEBUG('curInsQuery = {1}', curInsQuery);
		open curIns for curInsQuery;
		loop
			fetch curIns into insertLine;
			exit when curIns%NOTFOUND;
			EF.DEBUG(insertLine);
			-- resultStr := resultStr || CHR(10) || insertLine;
		end loop;
		close curIns;
		--EF.DEBUG('{1} {2}', p_tableName, p_whereClause);
		
		open curRefTables;
		loop
			fetch curRefTables into rtRow;
			exit when curRefTables%NOTFOUND;
			if curRefTables%ROWCOUNT = 1 then
				EF.DEBUG('BEGIN\n\tDBMS_OUTPUT.PUT_LINE(''UPDATE {1}'');\nEND;\n/', p_tableName);
			end if;
			EF.DEBUG('UPDATE {1} T SET {2} = (SELECT M.NEW_ID FROM MAPS_IDS M WHERE M.TABLE_NAME = ''{3}'' AND M.OLD_ID = T.{2}) WHERE EXISTS (SELECT * FROM MAPS_IDS M WHERE M.TABLE_NAME = ''{3}'' AND M.OLD_ID = T.{2});', p_tableName, rtRow.COLUMN_NAME, rtRow.R_TABLE_NAME);
		end loop;
		close curRefTables;
		EF.DEBUG('--COMMIT');
		
		if not p_recursive then
			return resultStr;
		end if;
		-- browse all the table has a foreign key references to this table
		open curFks;
		loop
			fetch curFks into fk;
			exit when curFks%NOTFOUND;
			temp := CASE WHEN pTableRow.ORDERBY IS NULL THEN NULL ELSE 'ORDER BY ' || pTableRow.ORDERBY END;
			curLinesQuery := EF.FORMAT('SELECT {1} FROM {2} T WHERE {3} {4}', fk.R_COLUMN_NAME, p_tableName, p_whereClause, temp);
			-- EF.DEBUG('curLinesQuery = {1}', curLinesQuery);
			open curLines for curLinesQuery;
			loop
				fetch curLines into fkValue;
				exit when curLines%NOTFOUND;
				SELECT DATA_TYPE INTO pkFieldType FROM USER_TAB_COLUMNS WHERE TABLE_NAME = p_tableName AND COLUMN_NAME = fk.R_COLUMN_NAME;
				subResultStr := GENERATE_INSERT(fk.TABLE_NAME, EF.FORMAT('T.{1} = {2}', fk.COLUMN_NAME,
					CASE WHEN pkFieldType = 'VARCHAR2' THEN '''' || fkValue || '''' ELSE fkValue END), p_recursive, p_simpleInsert);
				if (subResultStr is not null) then
					resultStr := resultStr || CHR(10) || subResultStr;
				end if;
			end loop;
			close curLines;
		end loop;
		close curFks;
		
		EF.DEBUG('---GENERATE_INSERT.END({1} WHERE {2}), p_recursive = {3}, p_simpleInsert = {4}', p_tableName, p_whereClause,
			case when p_recursive then 'true' else 'false' end, case when p_simpleInsert then 'true' else 'false' end);
		return resultStr;
    EXCEPTION
        WHEN OTHERS THEN
            EF.ERROR('GENERATE_INSERT. code = {1}, message = {2}, backtrace={3}', SQLCODE, SQLERRM, DBMS_UTILITY.FORMAT_ERROR_BACKTRACE);
            RAISE;
    END;
	
	PROCEDURE RESTORE(p_TableNamePrefix VARCHAR2 default 'MAPS_BK_', p_TableNameSuffix VARCHAR2 default null)
	AS
		CURSOR curTables IS
			SELECT M.TABLE_NAME, T.TABLE_NAME
			FROM MAPS_TABLES M
				JOIN USER_TABLES T ON T.TABLE_NAME LIKE (REPLACE(REPLACE(M.BK_TABLE_NAME, '{TIMESTAMP}', NULL), '_', '\_') || '%') ESCAPE '\'
					AND T.TABLE_NAME LIKE ('%' || p_TableNameSuffix)
			WHERE M.BK_TABLE_NAME LIKE (p_TableNamePrefix || '%');
		tableName MAPS_TABLES.TABLE_NAME%TYPE;
		bkTableName USER_TABLES.TABLE_NAME%TYPE;
	BEGIN
		EF.SET_LOG_LEVEL(EF.LOG_DEBUG);
		EF.DEBUG('RESTORE p_TableNamePrefix = {1}, p_TableNameSuffix = {2}', p_TableNamePrefix, p_TableNameSuffix);
		open curTables;
		loop
			fetch curTables into tableName, bkTableName;
			--EF.DEBUG('tableName = {1}, bkTableName = {2}', tableName, bkTableName);
			EF.DEBUG('DELETE {1}', tableName);
			EXECUTE IMMEDIATE EF.FORMAT('DELETE {1}', tableName);
			EF.DEBUG('INSERT INTO {1} SELECT * FROM {2}', tableName, bkTableName);
			EXECUTE IMMEDIATE EF.FORMAT('INSERT INTO {1} SELECT * FROM {2}', tableName, bkTableName);
			exit when curTables%NOTFOUND;
		end loop;
		close curTables;
	END;
	
	PROCEDURE CLEANUP(p_TableNamePrefix VARCHAR2 default 'MAPS_BK_', p_TableNameSuffix VARCHAR2 default null)
	AS
		likeClause VARCHAR2(255);
		curTableNames SYS_REFCURSOR;
		tableName USER_TABLES.TABLE_NAME%TYPE;
	BEGIN
		EF.SET_LOG_LEVEL(EF.LOG_DEBUG);
		EF.DEBUG('CLEANUP p_TableNamePrefix = {1}, p_TableNameSuffix = {2}', p_TableNamePrefix, p_TableNameSuffix);
		if p_TableNamePrefix is null then
			if p_TableNameSuffix is null then
				EF.ERROR('Parameters error.');
			else
				likeClause := '%' || p_TableNameSuffix;
			end if;
		else
			likeClause := p_TableNamePrefix || '%' || p_TableNameSuffix;
		end if;
		open curTableNames for SELECT TABLE_NAME FROM USER_TABLES WHERE TABLE_NAME LIKE likeClause;
		loop
			fetch curTableNames into tableName;
			exit when curTableNames%NOTFOUND;
			EF.DEBUG('DROP TABLE {1}', tableName);
			EXECUTE IMMEDIATE EF.FORMAT('DROP TABLE {1}', tableName);
		end loop;
		close curTableNames;
	END;
    
END EM;
/

CREATE OR REPLACE PUBLIC SYNONYM EM FOR EM;
/