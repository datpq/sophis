SET SERVEROUTPUT ON SIZE 1000000
SET VERIFY OFF
SET TRIMSPOOL ON
DECLARE
  v_master   VARCHAR2(30) := UPPER('&&1');
  v_table    VARCHAR2(30) := UPPER('&&2');
  v_newuser  VARCHAR2(30) := UPPER('&&3');
  v_vp_num   VARCHAR2(30) := UPPER('&&4');
  
  v_table_vp      VARCHAR2(30) := substr(v_table,1,23) || '_$VPT' || v_vp_num;
  v_audit_tbl_vp  VARCHAR2(30) := 'AUDIT_' || substr(v_table,1,23) || '_$VPT' || v_vp_num;
  v_audit_tbl     VARCHAR2(30) := 'AUDIT_' || substr(v_table,1,23);
  v_trigger       VARCHAR2(30) := substr(v_table,1,23) || '_$TRG' || v_vp_num;
  
  v_created VARCHAR2(30) :='_CREATED_';
  v_updated VARCHAR2(30) :='_UPDATED_';
  v_deleted VARCHAR2(30) :='_DELETED_';
 
  CURSOR c_columns IS
    SELECT column_name
    FROM   all_tab_columns
    WHERE  owner      = v_master
    AND    table_name = v_table
    AND    data_type not in ('LONG','LONG RAW')
    ORDER BY COLUMN_ID; 
    
BEGIN

    DBMS_OUTPUT.PUT_LINE(' -- create table  v_table_vp	'			);
    DBMS_OUTPUT.PUT_LINE(' DROP TABLE ' || v_master || '.' || v_table_vp ||';');
    DBMS_OUTPUT.PUT_LINE(' CREATE TABLE ' || v_master || '.' || v_table_vp || ' as select * from '|| v_master || '.' || v_table ||';');
    
    -- Add it to the list of MD tables, to be managed by table right access managment
    DBMS_OUTPUT.PUT_LINE('CALL '||v_master||'.P_OVERLOADTAB.DECLARATION('''|| v_table_vp ||'''); ');       
    
    DBMS_OUTPUT.PUT_LINE(' -- create AUDIT table is created 	'			);
    DBMS_OUTPUT.PUT_LINE(' DROP TABLE ' || v_master || '.' || v_audit_tbl_vp ||';');
    DBMS_OUTPUT.PUT_LINE(' CREATE TABLE ' || v_master || '.' || v_audit_tbl_vp || '(' 		);
    DBMS_OUTPUT.PUT_LINE('   SICOVAM     NUMBER(10,0),'						);
    DBMS_OUTPUT.PUT_LINE('   DATEMODIF   DATE,'							);
    DBMS_OUTPUT.PUT_LINE('   USERMODIF   NUMBER(10,0),'						);    
    DBMS_OUTPUT.PUT_LINE('   TYPEMODIF   VARCHAR2(64),'						);
    DBMS_OUTPUT.PUT_LINE('   DATEPREV    DATE,'							);
    DBMS_OUTPUT.PUT_LINE('   FIELD       VARCHAR2(128),'					);
    DBMS_OUTPUT.PUT_LINE('   OLDVAL      VARCHAR2(128),'					);
    DBMS_OUTPUT.PUT_LINE('   NEWVAL      VARCHAR2(128));'					);
    DBMS_OUTPUT.PUT_LINE('									');
    DBMS_OUTPUT.PUT_LINE('GRANT SELECT, INSERT, UPDATE, DELETE ON ' || v_master || '.' || v_table_vp || ' TO '|| v_newuser || ';');
    -- Make syn both on v_table and v_table_vp to make for instance master.historique and master.historique_$vpt01 accesible at the vp level	
    DBMS_OUTPUT.PUT_LINE('DROP SYNONYM ' || v_newuser || '.' || v_table_vp || ';');
    DBMS_OUTPUT.PUT_LINE('CREATE SYNONYM ' || v_newuser || '.' || v_table_vp || ' FOR '|| v_master || '.' || v_table_vp || ';');	    
    DBMS_OUTPUT.PUT_LINE('GRANT SELECT, INSERT, UPDATE, DELETE ON ' || v_master || '.' || v_audit_tbl_vp || ' TO '|| v_newuser || ';');
    DBMS_OUTPUT.PUT_LINE('DROP SYNONYM ' || v_newuser || '.' || v_audit_tbl_vp|| ';');
    DBMS_OUTPUT.PUT_LINE('CREATE SYNONYM ' || v_newuser || '.' || v_audit_tbl_vp|| ' FOR '|| v_master || '.' || v_audit_tbl_vp|| ';');
    
    DBMS_OUTPUT.PUT_LINE('									');
    DBMS_OUTPUT.PUT_LINE(' DROP TRIGGER ' || v_master || '.' || v_trigger|| ' ;'		);
    DBMS_OUTPUT.PUT_LINE(' CREATE TRIGGER ' || v_master || '.' || v_trigger		);
    DBMS_OUTPUT.PUT_LINE(' AFTER INSERT OR UPDATE OR DELETE ON ' || v_master || '.' || v_table_vp);
    DBMS_OUTPUT.PUT_LINE(' FOR EACH ROW'								);
    DBMS_OUTPUT.PUT_LINE(' DECLARE'								);
    DBMS_OUTPUT.PUT_LINE('      aud_sicovam    NUMBER(10,0);'					);
    DBMS_OUTPUT.PUT_LINE('      aud_datemodif  DATE;'						);
    DBMS_OUTPUT.PUT_LINE('      aud_usermodif  NUMBER(10,0);'					);
    DBMS_OUTPUT.PUT_LINE('      aud_dateprev   DATE;'						);
    DBMS_OUTPUT.PUT_LINE('									');
    DBMS_OUTPUT.PUT_LINE('    BEGIN'								);
    DBMS_OUTPUT.PUT_LINE('									');
    DBMS_OUTPUT.PUT_LINE('    -- save the constant values					');
    DBMS_OUTPUT.PUT_LINE('    SELECT :old.sicovam, SYSDATE, GetUserId_TKT_Safe(), :old.jour'	);
    DBMS_OUTPUT.PUT_LINE('      INTO aud_sicovam, aud_datemodif, aud_usermodif, aud_dateprev '	);
    DBMS_OUTPUT.PUT_LINE('      FROM DUAL;'							);
    DBMS_OUTPUT.PUT_LINE('									');  
    DBMS_OUTPUT.PUT_LINE('   IF INSERTING THEN'							);
    DBMS_OUTPUT.PUT_LINE('									'); 
    DBMS_OUTPUT.PUT_LINE('      INSERT INTO '|| v_master || '.' || v_audit_tbl_vp ||' (sicovam, datemodif, usermodif, typemodif, dateprev, field, oldval, newval)		');
    DBMS_OUTPUT.PUT_LINE('        VALUES (:new.sicovam, aud_datemodif, aud_usermodif, ''' ||v_created ||''',NULL,NULL,NULL, NULL);'	);
    DBMS_OUTPUT.PUT_LINE('									');
    DBMS_OUTPUT.PUT_LINE('    ELSIF DELETING THEN														');
    DBMS_OUTPUT.PUT_LINE('									');
				  FOR cur_rec IN c_columns LOOP
				    DBMS_OUTPUT.PUT_LINE('IF '||':'||'old'||'.'||cur_rec.column_name||' IS NOT NULL '  		);
				    DBMS_OUTPUT.PUT_LINE('THEN'								);
				    DBMS_OUTPUT.PUT_LINE('	INSERT INTO ' || v_master || '.' || v_audit_tbl_vp || ' (sicovam, datemodif, usermodif,typemodif, dateprev, field, oldval, newval)'		);
				    DBMS_OUTPUT.PUT_LINE('	VALUES (aud_sicovam, aud_datemodif, aud_usermodif,'''|| v_deleted ||''', aud_dateprev, '''||cur_rec.column_name||''','				);
				    DBMS_OUTPUT.PUT_LINE('		TO_CHAR('||':'||'old'||'.'||cur_rec.column_name||'), NULL);' 			);
				    DBMS_OUTPUT.PUT_LINE('END IF;'																);   
				  END LOOP;    
    DBMS_OUTPUT.PUT_LINE('									');  
    DBMS_OUTPUT.PUT_LINE('    ELSE'													);
				  FOR cur_rec IN c_columns LOOP
				    DBMS_OUTPUT.PUT_LINE('IF '||':'||'old'||'.'||cur_rec.column_name||' != :new.'||cur_rec.column_name				  		);
				    DBMS_OUTPUT.PUT_LINE('	OR ('||':'||'old'||'.'||cur_rec.column_name||' IS NULL AND '||':'||'new'||'.'||cur_rec.column_name||' IS NOT NULL)'  		);
				    DBMS_OUTPUT.PUT_LINE('	OR ('||':'||'old'||'.'||cur_rec.column_name||' IS NOT NULL AND '||':'||'new'||'.'||cur_rec.column_name||' IS NULL)'  		);
				    DBMS_OUTPUT.PUT_LINE('THEN'																	);
				    DBMS_OUTPUT.PUT_LINE('	INSERT INTO ' || v_master || '.' || v_audit_tbl_vp || ' (sicovam, datemodif, usermodif,typemodif, dateprev, field, oldval, newval)'		);
				    DBMS_OUTPUT.PUT_LINE('	VALUES (aud_sicovam, aud_datemodif, aud_usermodif,'''|| v_updated ||''', aud_dateprev, '''||cur_rec.column_name||''','				);
				    DBMS_OUTPUT.PUT_LINE('		TO_CHAR('||':'||'old'||'.'||cur_rec.column_name||'), TO_CHAR(:new.'||cur_rec.column_name||'));' 			);
				    DBMS_OUTPUT.PUT_LINE('END IF;'																);   
				  END LOOP;
    DBMS_OUTPUT.PUT_LINE('	END IF;'																			);				       	  	  
    DBMS_OUTPUT.PUT_LINE(' END;');
    DBMS_OUTPUT.PUT_LINE('/');
END;
/









