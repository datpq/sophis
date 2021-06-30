BEGIN
    BL.BILLING_GENERATOR(
        p_source => BL.SRC_F2, -- source F2
        p_date => add_months(trunc(sysdate, 'MM'), -1), -- previous month
        p_periodicite => 2 -- Mensuel
    );
END;
/
EXIT;
