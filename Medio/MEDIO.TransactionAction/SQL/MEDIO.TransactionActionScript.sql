--{{SOPHIS_SQL (do not delete this line)

create or replace PROCEDURE TK_EXTERNAL_EXECUTIONS_CTPY
(
    p_orderId   in HISTOMVTS.SOPHIS_ORDER_ID%TYPE,
    p_ctpyId    in TIERS.IDENT%TYPE,
    p_retVal    out NUMBER
)
IS
BEGIN
    UPDATE EXTERNAL_EXECUTIONS SET OTHER_FIELDS = REGEXP_REPLACE(OTHER_FIELDS, '(idco: l:\d+)', 'idco: l:' || p_ctpyId)
    WHERE SOPHISORDERID = (SELECT MARKETID FROM ORDER_PLACEMENT where orderid = p_orderId);
    P_RETVAL:= SQL%ROWCOUNT;
END;

--}}SOPHIS_SQL