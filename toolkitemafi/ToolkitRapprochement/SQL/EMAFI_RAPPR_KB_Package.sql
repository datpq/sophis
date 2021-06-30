create or replace 
PACKAGE EMAFI_RAPPR_KB AS 

  /* TODO enter package declarations (types, exceptions, methods etc) here */ 

FUNCTION test_date (p_date IN VARCHAR2, p_format IN VARCHAR2 DEFAULT 'DD/MM/YY') RETURN BOOLEAN;

PROCEDURE test_date_null( originalData varchar2,dateVal varchar2,dateOp varchar2);



function parse_Montant1( montant_char varchar2, nbr_decim number) return number;
--************************************************



procedure parse_OriginalData ;



------------------------------------------

procedure test_conditions_date_null;


-----------------------------------------------

procedure test_conditions ;

------------------------------------------

procedure test_somme_montants ;
procedure statut_OK ;
procedure insert_into_balance;
procedure insert_into_trades;

procedure Rapprochement_Procedures;
END EMAFI_RAPPR_KB;