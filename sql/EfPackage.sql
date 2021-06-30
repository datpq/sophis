CREATE OR REPLACE PACKAGE EF
AS
	PROCEDURE ARCHIVE(p_date varchar2);
	PROCEDURE ARC_DISABLE_TRIGGERS;
	PROCEDURE ARC_ACTIVATE_TRIGGERS;
	FUNCTION DDL(p_tableName USER_TABLES.TABLE_NAME%TYPE, p_newTableName USER_TABLES.TABLE_NAME%TYPE) RETURN varchar2;
	
    /******************************************************************************
    Utilities function
    ******************************************************************************/
    -- log functions
    LOG_ERROR           constant int := 1;
    LOG_WARN            constant int := 2;
    LOG_INFO            constant int := 3;
    LOG_DEBUG           constant int := 4;
	
    PROCEDURE SET_LOG_LEVEL(level int);
    PROCEDURE SET_LOGGER(logger varchar2);
	PROCEDURE SET_LOG_HEADER(hasHeader boolean);
	PROCEDURE SET_LOG_DBSAVED(dbSaved boolean);
	
    PROCEDURE ERROR(s varchar2,
        p1 varchar2 default null, p2 varchar2 default null, p3 varchar2 default null,
        p4 varchar2 default null, p5 varchar2 default null, p6 varchar2 default null,
        p7 varchar2 default null, p8 varchar2 default null, p9 varchar2 default null,
        p10 varchar2 default null, p11 varchar2 default null, p12 varchar2 default null, p13 varchar2 default null);

    PROCEDURE WARN(s varchar2,
        p1 varchar2 default null, p2 varchar2 default null, p3 varchar2 default null,
        p4 varchar2 default null, p5 varchar2 default null, p6 varchar2 default null,
        p7 varchar2 default null, p8 varchar2 default null, p9 varchar2 default null,
        p10 varchar2 default null, p11 varchar2 default null, p12 varchar2 default null, p13 varchar2 default null);

    PROCEDURE INFO(s varchar2,
        p1 varchar2 default null, p2 varchar2 default null, p3 varchar2 default null,
        p4 varchar2 default null, p5 varchar2 default null, p6 varchar2 default null,
        p7 varchar2 default null, p8 varchar2 default null, p9 varchar2 default null,
        p10 varchar2 default null, p11 varchar2 default null, p12 varchar2 default null, p13 varchar2 default null);

    PROCEDURE DEBUG(s varchar2,
        p1 varchar2 default null, p2 varchar2 default null, p3 varchar2 default null,
        p4 varchar2 default null, p5 varchar2 default null, p6 varchar2 default null,
        p7 varchar2 default null, p8 varchar2 default null, p9 varchar2 default null,
        p10 varchar2 default null, p11 varchar2 default null, p12 varchar2 default null, p13 varchar2 default null);

    FUNCTION FORMAT(s varchar2,
        p1 varchar2 default null, p2 varchar2 default null, p3 varchar2 default null,
        p4 varchar2 default null, p5 varchar2 default null, p6 varchar2 default null,
        p7 varchar2 default null, p8 varchar2 default null, p9 varchar2 default null,
        p10 varchar2 default null, p11 varchar2 default null, p12 varchar2 default null, p13 varchar2 default null)
        RETURN varchar2;
		
	FUNCTION BLOB2XMLTYPE(blob_in IN BLOB) RETURN XMLTYPE;
END EF;
/

CREATE OR REPLACE PACKAGE BODY EF
AS
    LOG_LEVEL			int := LOG_INFO;
    LOG_LOGGER          EF_LOGS.LOGGER%TYPE := 'EF';
	LOG_HEADER			boolean := TRUE;
	LOG_DBSAVED			boolean := FALSE;
    sid                 varchar2(8);
    TYPE MAP_VARCHAR IS TABLE OF VARCHAR2(255) INDEX BY VARCHAR2(20);
    output_params       MAP_VARCHAR;
	
	PROCEDURE ARCHIVE(p_date varchar2)
	AS
		CURSOR curTables IS SELECT TABLE_NAME FROM EF_TABLES ORDER BY 1;
		tb			curTables%ROWTYPE;
		ddlQuery	varchar2(4000);
	BEGIN
		SET_LOG_LEVEL(LOG_DEBUG);
		
		DEBUG('Disabling triggers...');
		ARC_DISABLE_TRIGGERS;
		
		open curTables;
		loop
			fetch curTables into tb;
			exit when curTables%NOTFOUND;
			DEBUG('Processing table {1}...', tb.TABLE_NAME);
			ddlQuery := DDL(tb.TABLE_NAME, tb.TABLE_NAME || '_' || p_date);
			DEBUG(ddlQuery);
		end loop;
		close curTables;

		DEBUG('Activating triggers...');
		ARC_ACTIVATE_TRIGGERS;
	END;
	
	FUNCTION DDL(
		p_tableName USER_TABLES.TABLE_NAME%TYPE,
		p_newTableName USER_TABLES.TABLE_NAME%TYPE) RETURN varchar2
	AS
		CURSOR curCols IS SELECT COLUMN_NAME, DATA_TYPE, DATA_LENGTH, DATA_PRECISION, DATA_SCALE, NULLABLE
        FROM USER_TAB_COLUMNS WHERE TABLE_NAME = p_tableName ORDER BY COLUMN_ID;
		col		curCols%ROWTYPE;
		query	varchar2(4000);
	BEGIN
		query := FORMAT('CREATE TABLE {1}
(', p_newTableName);
		open curCols;
		loop
			fetch curCols into col;
			exit when curCols%NOTFOUND;
            if col.DATA_TYPE = 'NUMBER' then
                if col.DATA_PRECISION is null then
                    query := query || FORMAT('
    {1} {2}', col.COLUMN_NAME, col.DATA_TYPE);
                else
                    query := query || FORMAT('
    {1} {2}({3}{4})', col.COLUMN_NAME, col.DATA_TYPE, col.DATA_PRECISION, case when NVL(col.DATA_SCALE, 0) = 0 then '' else ',' || col.DATA_SCALE end);
                end if;
            elsif col.DATA_TYPE in ('DATE', 'CLOB') then
                    query := query || FORMAT('
    {1} {2}', col.COLUMN_NAME, col.DATA_TYPE);
            else
                    query := query || FORMAT('
    {1} {2}({3})', col.COLUMN_NAME, col.DATA_TYPE, col.DATA_LENGTH);
            end if;
            query := query || case when col.NULLABLE = 'N' then ' NOT NULL,' else ',' end;
		end loop;
		close curCols;
        query := substr(query, 1, length(query) - 1) || '
)';
		--DEBUG(query);
		return query;
	END;
	
	PROCEDURE ARC_DISABLE_TRIGGERS
	AS
		CURSOR curTriggers IS SELECT STR1 FROM EF_TEMPS ORDER BY 1;
		tg curTriggers%ROWTYPE;
	BEGIN
		DELETE EF_TEMPS WHERE CATEGORY = 'TRIGGER';
		INSERT INTO EF_TEMPS(CATEGORY, STR1)
			SELECT 'TRIGGER', TRIGGER_NAME FROM USER_TRIGGERS
			WHERE BASE_OBJECT_TYPE = 'TABLE' AND STATUS = 'ENABLED'
				AND TABLE_NAME IN (SELECT TABLE_NAME FROM EF_TABLES);
		open curTriggers;
		loop
			fetch curTriggers into tg;
			exit when curTriggers%NOTFOUND;
			DEBUG('Disable trigger {1}...', tg.STR1);
			--TODO: Uncomment the following line
			--EXECUTE IMMEDIATE FORMAT('ALTER TRIGGER {1} DISABLE', tg.STR1);
		end loop;
		close curTriggers;
	END;
	
	PROCEDURE ARC_ACTIVATE_TRIGGERS
    AS
		CURSOR curTriggers IS SELECT STR1 FROM EF_TEMPS ORDER BY 1;
		tg curTriggers%ROWTYPE;
	BEGIN
		open curTriggers;
		loop
			fetch curTriggers into tg;
			exit when curTriggers%NOTFOUND;
			DEBUG('Activate trigger {1}...', tg.STR1);
			--TODO: Uncomment the following line
			--EXECUTE IMMEDIATE FORMAT('ALTER TRIGGER {1} ENABLE', tg.STR1);
		end loop;
		close curTriggers;
		DELETE EF_TEMPS WHERE CATEGORY = 'TRIGGER';
	END;

    /******************************************************************************
    Utilities function
    ******************************************************************************/
    PROCEDURE SET_LOG_LEVEL(level int) AS
    BEGIN
        LOG_LEVEL := level;
    END;
    
    PROCEDURE SET_LOGGER(logger varchar2) AS
    BEGIN
        LOG_LOGGER := logger;
    END;
    
    PROCEDURE SET_LOG_HEADER(hasHeader boolean) AS
    BEGIN
        LOG_HEADER := hasHeader;
    END;
	
    PROCEDURE SET_LOG_DBSAVED(dbSaved boolean) AS
    BEGIN
        LOG_DBSAVED := dbSaved;
    END;
	
    PROCEDURE LOG(s varchar2, level int) AS
        severity        EF_LOGS.SEVERITY%TYPE;
        logger          EF_LOGS.LOGGER%TYPE;
        PRAGMA AUTONOMOUS_TRANSACTION;
        MAX_LENGTH      constant int := 4000; -- max length of varchar field in oracle table
        sTemp           varchar2(4000);
        pos             int;
    BEGIN
        severity := case level  when LOG_ERROR  then 'ERROR'
                                when LOG_WARN   then 'WARN'
                                when LOG_INFO   then 'INFO'
                                when LOG_DEBUG  then 'DEBUG' end;
		if (LOG_HEADER) then
			--DBMS_OUTPUT.put(to_char(sysdate,'YYYY-MM-DD HH24:MI:SS:    ') '[' || severity || ']    ');
			DBMS_OUTPUT.put(to_char(localtimestamp,'YYYY-MM-DD HH24:MI:SS.FF2    ') || '[' || severity || ']    ');
		end if;
        if sid is null then
            SELECT SYS_CONTEXT('USERENV','SID') into sid FROM DUAL;
            output_params('Logger') := LOG_LOGGER || '_' || sid;
        end if;
		
		if (LOG_DBSAVED) then
			logger := LOG_LOGGER || '_' || sid;
        
			pos := 1;
			while pos <= length(s)
			loop
				sTemp := substr(s, pos, MAX_LENGTH);
				INSERT INTO EF_LOGS VALUES (EF_LOGS_SEQ.NEXTVAL, localtimestamp, severity, logger, sTemp);
				pos := pos + MAX_LENGTH;
			end loop;
			COMMIT;
		end if;
        DBMS_OUTPUT.put_line (s);
    EXCEPTION
        WHEN OTHERS THEN
            ERROR('LOG. code = {1}, message = {2}, backtrace={3}', SQLCODE, SQLERRM, DBMS_UTILITY.FORMAT_ERROR_BACKTRACE);
            RAISE;
    END;
    
    PROCEDURE ERROR(s varchar2,
        p1 varchar2 default null, p2 varchar2 default null, p3 varchar2 default null,
        p4 varchar2 default null, p5 varchar2 default null, p6 varchar2 default null,
        p7 varchar2 default null, p8 varchar2 default null, p9 varchar2 default null,
        p10 varchar2 default null, p11 varchar2 default null, p12 varchar2 default null, p13 varchar2 default null) AS
    BEGIN
        if LOG_LEVEL >= LOG_ERROR then 
            if p1 is not null then
                ERROR(FORMAT(s, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13));
            else
                LOG(s, LOG_ERROR);
            end if;
        end if;
    END;
    
    PROCEDURE WARN(s varchar2,
        p1 varchar2 default null, p2 varchar2 default null, p3 varchar2 default null,
        p4 varchar2 default null, p5 varchar2 default null, p6 varchar2 default null,
        p7 varchar2 default null, p8 varchar2 default null, p9 varchar2 default null,
        p10 varchar2 default null, p11 varchar2 default null, p12 varchar2 default null, p13 varchar2 default null) AS
    BEGIN
        if LOG_LEVEL >= LOG_WARN then
            if p1 is not null then
                WARN(FORMAT(s, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13));
            else
                LOG(s, LOG_WARN);
            end if;
        end if;
    END;
    
    PROCEDURE INFO(s varchar2,
        p1 varchar2 default null, p2 varchar2 default null, p3 varchar2 default null,
        p4 varchar2 default null, p5 varchar2 default null, p6 varchar2 default null,
        p7 varchar2 default null, p8 varchar2 default null, p9 varchar2 default null,
        p10 varchar2 default null, p11 varchar2 default null, p12 varchar2 default null, p13 varchar2 default null) AS
    BEGIN
        if LOG_LEVEL >= LOG_INFO then
            if p1 is not null then
                INFO(FORMAT(s, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13));
            else
                LOG(s, LOG_INFO);
            end if;
        end if;
    END;
    
    PROCEDURE DEBUG(s varchar2,
        p1 varchar2 default null, p2 varchar2 default null, p3 varchar2 default null,
        p4 varchar2 default null, p5 varchar2 default null, p6 varchar2 default null,
        p7 varchar2 default null, p8 varchar2 default null, p9 varchar2 default null,
        p10 varchar2 default null, p11 varchar2 default null, p12 varchar2 default null, p13 varchar2 default null) AS
    BEGIN
        if LOG_LEVEL >= LOG_DEBUG then
            if p1 is not null then
                DEBUG(FORMAT(s, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13));
            else
                LOG(s, LOG_DEBUG);
            end if;
        end if;
    END;
    
    FUNCTION FORMAT(s varchar2,
        p1 varchar2 default null, p2 varchar2 default null, p3 varchar2 default null,
        p4 varchar2 default null, p5 varchar2 default null, p6 varchar2 default null,
        p7 varchar2 default null, p8 varchar2 default null, p9 varchar2 default null,
        p10 varchar2 default null, p11 varchar2 default null, p12 varchar2 default null, p13 varchar2 default null)
    RETURN varchar2
    AS
        type ParamList is varray(13) of varchar2(10000);
        params ParamList := ParamList(
            p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13);
        result varchar2(10000) := s;
    BEGIN
        for i in 1..params.count loop
            --exit when params(i) is null;
            result := replace(result, '{' || i || '}', params(i));
        end loop;
		result := replace(replace(result, '\n', CHR(10)), '\t', '    ');
        return result;
    END;
	
	FUNCTION BLOB2XMLTYPE(blob_in IN BLOB)
	RETURN XMLTYPE
	AS
		v_clob		CLOB;
		v_varchar	VARCHAR2(32767);
		v_start		PLS_INTEGER := 1;
		v_buffer	PLS_INTEGER := 32767;
	BEGIN
		DBMS_LOB.CREATETEMPORARY(v_clob, TRUE);

		FOR i IN 1..CEIL(DBMS_LOB.GETLENGTH(blob_in) / v_buffer)
		LOOP
			v_varchar := UTL_RAW.CAST_TO_VARCHAR2(DBMS_LOB.SUBSTR(blob_in, v_buffer, v_start));
			if i = CEIL(DBMS_LOB.GETLENGTH(blob_in) / v_buffer) then
				DEBUG(v_varchar);
				v_varchar := rtrim(v_varchar);
			end if;
			DBMS_LOB.WRITEAPPEND(v_clob, LENGTH(v_varchar), v_varchar);
			v_start := v_start + v_buffer;
		END LOOP;

		RETURN XMLTYPE(v_clob);
	END BLOB2XMLTYPE;
END EF;
/

CREATE OR REPLACE PUBLIC SYNONYM EF FOR EF;
/