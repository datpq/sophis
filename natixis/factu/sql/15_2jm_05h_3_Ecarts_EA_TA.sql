WHENEVER SQLERROR EXIT 99 ROLLBACK;
WHENEVER OSERROR  EXIT 98 ROLLBACK;


SET trimout OFF 
SET FEEDBACK OFF
SET heading OFF 
SET verify OFF
SET space 0 
SET NEWPAGE 0 
SET PAGESIZE 0 
SET trimspool ON

SET WRAP OFF
SET TRIMOUT OFF 
SET COLSEP ';'
SET LINESIZE 2000

def datefactu='&2';

spool '&3';

prompt -------------------------------------------------------------- ;
prompt  Checking des ecarts entre Explained Amount et Ticket Amount   ;
prompt -------------------------------------------------------------- ;

prompt -------------------------------------------------------------- ;
prompt  15  - Debut Extraction du fichier Ecarts_EA_TA.csv            ;
prompt -------------------------------------------------------------- ;
spool off;

spool &4;

SELECT 'CTPY_ID;Contrepartie;Reference;MvtIdent;Billing frequence;Billing type;Refcon;TA Amount;Explained Amount;delta;Devise;Payment ID;Payment status' FROM DUAL;  
SELECT op.ctpy_id,
       op.ctpy_libelle, 
       op_ref, 
       op.mvtident, 
       op.fact_freq,
       DECODE (op.mvttype, 0, 'Fees', 1, 'Per Contract', 2, 'Repo', 3, 'Pool') 
          type_factu, 
              h.refcon ta_refcon, 
       h.montant ta_amount, 
       ROUND (CASE 
                 WHEN op.mvttype = 0 
                 THEN op.expl_amount
                 WHEN OP.EXPL_AMOUNT IS NOT NULL THEN OP.EXPL_AMOUNT  
                 ELSE 
                    (SELECT SUM (ex.interest) 
                     FROM cma_rpt_explanations ex 
                     WHERE ex.op_id = op.op_id AND EX.QTY IS NOT NULL) 
              END, 2 
       ) 
           explained_amount, 
        ABS (h.montant 
             - ROUND (CASE 
                         WHEN op.mvttype = 0 
                         THEN op.expl_amount
                         WHEN OP.EXPL_AMOUNT IS NOT NULL THEN OP.EXPL_AMOUNT 
                         ELSE 
                            (SELECT SUM (ex.interest) 
                             FROM cma_rpt_explanations ex 
                             WHERE ex.op_id = op.op_id AND EX.QTY IS NOT NULL) 
                      END, 2 
               ) 
        ) 
           delta, 
        op.fact_cur devise_factu, 
        m.ident payment_id, 
        s.name payment_status 
 FROM    cma_rpt_operations op 
      LEFT JOIN 
            histomvts h 
         LEFT JOIN 
               bo_messages m 
            JOIN 
               bo_external_status s 
            ON m.status = s.id 
         ON m.trade_id = h.refcon AND m.grp_code = 1 
      ON     h.mvtident = op.mvtident 
         AND TO_CHAR (h.dateneg, 'MM/YYYY') = TO_CHAR (op.mois_fact, 'MM/YYYY') 
         AND h.TYPE = DECODE (op.mvttype, 0, 7, 2, 7, 101) 
 WHERE mois_fact = add_months(trunc(TO_DATE ('&datefactu', 'YYYY/MM/DD'), 'MM'), -1) -- mois de facturation considéré 
       AND ( (op.mvttype = 0 
              AND ( ( op.expl_amount IS NOT NULL))) 
            OR (op.mvttype != 0 
                AND ( (OP.EXPL_AMOUNT IS NOT NULL OR (SELECT SUM (ex.interest) 
                         FROM cma_rpt_explanations ex 
                         WHERE ex.op_id = op.op_id AND EX.QTY IS NOT NULL) IS NOT NULL)))) 
 ORDER BY op.ctpy_libelle, op.fact_cur, op.op_ref;
 
spool off;
  
spool &3 append;

prompt -------------------------------------------------------------- ;
prompt 15  - Fin Extraction du fichier Ecarts_EA_TA.csv               ;
prompt -------------------------------------------------------------- ;

spool off;
  
/
exit;
