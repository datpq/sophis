--{{SOPHIS_SQL (do not delete this line)

  CREATE TABLE "CFG_DONNEES_CORPORATES" 
   (	"EMETTEUR" VARCHAR2(256 BYTE) NOT NULL ENABLE, 
	"ANNEE" NUMBER NOT NULL ENABLE, 
	"NATURE_DE_DONNEES" VARCHAR2(256 BYTE) NOT NULL ENABLE, 
	"SOURCES" VARCHAR2(256 BYTE) NOT NULL ENABLE, 
	"SECTEUR" VARCHAR2(256 BYTE), 
	"CA" NUMBER, 
	"DAP" NUMBER, 
	"CHARGES_DU_PERSONNEL" NUMBER, 
	"AUTRES_CHARGES_D_EXPLOITATION" NUMBER, 
	"REX" NUMBER, 
	"RESULTAT_FINANCIER" NUMBER, 
	"AUTRES_CHARGES" NUMBER, 
	"AUTRES_PRODUITS1" NUMBER, 
	"RESULTAT_AVANT_IMPOTS" NUMBER, 
	"IMPOTS" NUMBER, 
	"RESULTAT_NET" NUMBER, 
	"RESULTAT_MIS_EN_EQUIVALENCE" NUMBER, 
	"RNPG" NUMBER, 
	"DIVIDENDES" NUMBER, 
	"INVESTISSEMENTS" NUMBER, 
	"ACHATS" NUMBER, 
	"MARGE_D_INTERETS" NUMBER, 
	"MARGE_SUR_COMMISSIONS" NUMBER, 
	"RESULTATS_ACTIVITES_MARCHE" NUMBER, 
	"AUTRES_PRODUITS2" NUMBER, 
	"PNB" NUMBER, 
	"CHARGES_D_EXPLOITATION" NUMBER, 
	"RBE" NUMBER, 
	"COUT_DU_RISQUE" NUMBER, 
	"PRIMES_EMISES_NETTES" NUMBER, 
	"PRODUIT_DES_PLACEMENTS" NUMBER, 
	"PRESTATIONS_ET_FRAIS_PAYES" NUMBER, 
	"VARIATION_PROV_TECHNIQUES" NUMBER, 
	"SOLDE_DE_REASSURANCE" NUMBER, 
	"VALEUR_AJOUTEE" NUMBER, 
	"RESULTAT_TECHNIQUE" NUMBER, 
	"RESULTAT_NON_TECHNIQUE" NUMBER, 
	"TOTAL_ACTIF" NUMBER, 
	"FONDS_PROPRES" NUMBER, 
	"DETTE_NETTE" NUMBER, 
	"ACTIF_CIRCULANT" NUMBER, 
	"PASSIF_CIRCULANT" NUMBER, 
	"IMMOBILISATIONS_CORPORELLES" NUMBER, 
	"TOTAL_PROVISIONS" NUMBER, 
	"PRETS_ET_CREANCES_SUR_CLIENTS" NUMBER, 
	"DETTE_ENVERS_LA_CLIENTELE" NUMBER, 
	"CREANCES_EN_SOUFFRANCE" NUMBER, 
	"PLACEMENTS_ASSURANCE" NUMBER, 
	"PROVISIONS_TECHNIQUES" NUMBER, 
	"VALEUR_GLOBALE" NUMBER, 
	"VALEUR_PAR_TITRE" NUMBER, 
	"VALEUR_AVEC_DECOTE_15" NUMBER, 
	"TAUX_DE_CROISSANCE_PERPETUEL" NUMBER, 
	"TAUX_SANS_RISQUE" NUMBER, 
	"PRIME_DE_RISQUE_MARCHE_ACTIONS" NUMBER, 
	"BETA" NUMBER, 
	"OCE" NUMBER, 
	"CASH_FLOW" NUMBER, 
	"FREE_CASH_FLOW" NUMBER, 
	"VALEUR_TERMINALE" NUMBER, 
	"DISCOUNT_FACTOR" NUMBER, 
	"DECOTE_SURCOTE" NUMBER, 
	"PE" NUMBER, 
	"PB" NUMBER, 
	"DY" NUMBER, 
	"COMMENTAIRE1" VARCHAR2(255 BYTE), 
	"COMMENTAIRE2" VARCHAR2(255 BYTE), 
	"DATE_DE_SAUVEGARDE" DATE, 
	 CONSTRAINT "CFG_DONNEES_CORPORATES_PK" PRIMARY KEY ("EMETTEUR", "ANNEE", "NATURE_DE_DONNEES", "SOURCES") ENABLE
   );


-- create AUDIT table is created 	
  DROP TABLE "CFG_DONNEES_CORPORATES_AUDIT";
  CREATE TABLE "CFG_DONNEES_CORPORATES_AUDIT" 
   (	
   "EMETTEUR" VARCHAR2(256 BYTE), 
	"ANNEE" NUMBER, 
	"NATURE_DE_DONNEES" VARCHAR2(256 BYTE), 
	"SOURCES" VARCHAR2(256 BYTE), 
	"DATEMODIF" DATE, 
	"USERMODIF" NUMBER(10,0), 
	"TYPEMODIF" VARCHAR2(64 BYTE), 
	"FIELD" VARCHAR2(128 BYTE), 
	"OLDVAL" VARCHAR2(128 BYTE), 
	"NEWVAL" VARCHAR2(128 BYTE)
   );
 

--								
CREATE OR REPLACE TRIGGER "CFG_DONNEES_CORPORATES_TRG" 
AFTER INSERT OR UPDATE OR DELETE ON CFG_DONNEES_CORPORATES
FOR EACH ROW
DECLARE
aud_emetteur	VARCHAR2(256);
aud_annee	NUMBER;
aud_nature_de_donnees	 VARCHAR2(256);
aud_sources    VARCHAR2(256);
aud_datemodif  DATE;
aud_usermodif  NUMBER(10,0);

BEGIN

-- save the constant values
SELECT :old.emetteur, :old.annee, :old.nature_de_donnees, :old.sources, SYSDATE,
GetUserId()
INTO aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources, aud_datemodif,
aud_usermodif
FROM DUAL;

IF INSERTING THEN

INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif, typemodif, field, oldval, newval)
VALUES (:new.emetteur, :new.annee, :new.nature_de_donnees, :new.sources,
aud_datemodif, aud_usermodif, '_CREATED_',NULL,NULL, NULL);

ELSIF DELETING THEN

IF :old.EMETTEUR IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'EMETTEUR',
		TO_CHAR(:old.EMETTEUR), NULL);
END IF;
IF :old.ANNEE IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'ANNEE',
		TO_CHAR(:old.ANNEE), NULL);
END IF;
IF :old.NATURE_DE_DONNEES IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'NATURE_DE_DONNEES',
		TO_CHAR(:old.NATURE_DE_DONNEES), NULL);
END IF;
IF :old.SOURCES IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'SOURCES',
		TO_CHAR(:old.SOURCES), NULL);
END IF;
IF :old.SECTEUR IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'SECTEUR',
		TO_CHAR(:old.SECTEUR), NULL);
END IF;
IF :old.CA IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'CA',
		TO_CHAR(:old.CA), NULL);
END IF;
IF :old.DAP IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'DAP',
		TO_CHAR(:old.DAP), NULL);
END IF;
IF :old.CHARGES_DU_PERSONNEL IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'CHARGES_DU_PERSONNEL',
		TO_CHAR(:old.CHARGES_DU_PERSONNEL), NULL);
END IF;
IF :old.AUTRES_CHARGES_D_EXPLOITATION IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'AUTRES_CHARGES_D_EXPLOITATION',
		TO_CHAR(:old.AUTRES_CHARGES_D_EXPLOITATION), NULL);
END IF;
IF :old.REX IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'REX',
		TO_CHAR(:old.REX), NULL);
END IF;
IF :old.RESULTAT_FINANCIER IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'RESULTAT_FINANCIER',
		TO_CHAR(:old.RESULTAT_FINANCIER), NULL);
END IF;
IF :old.AUTRES_CHARGES IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'AUTRES_CHARGES',
		TO_CHAR(:old.AUTRES_CHARGES), NULL);
END IF;
IF :old.AUTRES_PRODUITS1 IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'AUTRES_PRODUITS1',
		TO_CHAR(:old.AUTRES_PRODUITS1), NULL);
END IF;
IF :old.RESULTAT_AVANT_IMPOTS IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'RESULTAT_AVANT_IMPOTS',
		TO_CHAR(:old.RESULTAT_AVANT_IMPOTS), NULL);
END IF;
IF :old.IMPOTS IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'IMPOTS',
		TO_CHAR(:old.IMPOTS), NULL);
END IF;
IF :old.RESULTAT_NET IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'RESULTAT_NET',
		TO_CHAR(:old.RESULTAT_NET), NULL);
END IF;
IF :old.RESULTAT_MIS_EN_EQUIVALENCE IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'RESULTAT_MIS_EN_EQUIVALENCE',
		TO_CHAR(:old.RESULTAT_MIS_EN_EQUIVALENCE), NULL);
END IF;
IF :old.RNPG IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'RNPG',
		TO_CHAR(:old.RNPG), NULL);
END IF;
IF :old.DIVIDENDES IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'DIVIDENDES',
		TO_CHAR(:old.DIVIDENDES), NULL);
END IF;
IF :old.INVESTISSEMENTS IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'INVESTISSEMENTS',
		TO_CHAR(:old.INVESTISSEMENTS), NULL);
END IF;
IF :old.ACHATS IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'ACHATS',
		TO_CHAR(:old.ACHATS), NULL);
END IF;
IF :old.MARGE_D_INTERETS IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'MARGE_D_INTERETS',
		TO_CHAR(:old.MARGE_D_INTERETS), NULL);
END IF;
IF :old.MARGE_SUR_COMMISSIONS IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'MARGE_SUR_COMMISSIONS',
		TO_CHAR(:old.MARGE_SUR_COMMISSIONS), NULL);
END IF;
IF :old.RESULTATS_ACTIVITES_MARCHE IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'RESULTATS_ACTIVITES_MARCHE',
		TO_CHAR(:old.RESULTATS_ACTIVITES_MARCHE), NULL);
END IF;
IF :old.AUTRES_PRODUITS2 IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'AUTRES_PRODUITS2',
		TO_CHAR(:old.AUTRES_PRODUITS2), NULL);
END IF;
IF :old.PNB IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'PNB',
		TO_CHAR(:old.PNB), NULL);
END IF;
IF :old.CHARGES_D_EXPLOITATION IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'CHARGES_D_EXPLOITATION',
		TO_CHAR(:old.CHARGES_D_EXPLOITATION), NULL);
END IF;
IF :old.RBE IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'RBE',
		TO_CHAR(:old.RBE), NULL);
END IF;
IF :old.COUT_DU_RISQUE IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'COUT_DU_RISQUE',
		TO_CHAR(:old.COUT_DU_RISQUE), NULL);
END IF;
IF :old.PRIMES_EMISES_NETTES IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'PRIMES_EMISES_NETTES',
		TO_CHAR(:old.PRIMES_EMISES_NETTES), NULL);
END IF;
IF :old.PRODUIT_DES_PLACEMENTS IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'PRODUIT_DES_PLACEMENTS',
		TO_CHAR(:old.PRODUIT_DES_PLACEMENTS), NULL);
END IF;
IF :old.PRESTATIONS_ET_FRAIS_PAYES IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'PRESTATIONS_ET_FRAIS_PAYES',
		TO_CHAR(:old.PRESTATIONS_ET_FRAIS_PAYES), NULL);
END IF;
IF :old.VARIATION_PROV_TECHNIQUES IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'VARIATION_PROV_TECHNIQUES',
		TO_CHAR(:old.VARIATION_PROV_TECHNIQUES), NULL);
END IF;
IF :old.SOLDE_DE_REASSURANCE IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'SOLDE_DE_REASSURANCE',
		TO_CHAR(:old.SOLDE_DE_REASSURANCE), NULL);
END IF;
IF :old.VALEUR_AJOUTEE IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'VALEUR_AJOUTEE',
		TO_CHAR(:old.VALEUR_AJOUTEE), NULL);
END IF;
IF :old.RESULTAT_TECHNIQUE IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'RESULTAT_TECHNIQUE',
		TO_CHAR(:old.RESULTAT_TECHNIQUE), NULL);
END IF;
IF :old.RESULTAT_NON_TECHNIQUE IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'RESULTAT_NON_TECHNIQUE',
		TO_CHAR(:old.RESULTAT_NON_TECHNIQUE), NULL);
END IF;
IF :old.TOTAL_ACTIF IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'TOTAL_ACTIF',
		TO_CHAR(:old.TOTAL_ACTIF), NULL);
END IF;
IF :old.FONDS_PROPRES IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'FONDS_PROPRES',
		TO_CHAR(:old.FONDS_PROPRES), NULL);
END IF;
IF :old.DETTE_NETTE IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'DETTE_NETTE',
		TO_CHAR(:old.DETTE_NETTE), NULL);
END IF;
IF :old.ACTIF_CIRCULANT IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'ACTIF_CIRCULANT',
		TO_CHAR(:old.ACTIF_CIRCULANT), NULL);
END IF;
IF :old.PASSIF_CIRCULANT IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'PASSIF_CIRCULANT',
		TO_CHAR(:old.PASSIF_CIRCULANT), NULL);
END IF;
IF :old.IMMOBILISATIONS_CORPORELLES IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'IMMOBILISATIONS_CORPORELLES',
		TO_CHAR(:old.IMMOBILISATIONS_CORPORELLES), NULL);
END IF;
IF :old.TOTAL_PROVISIONS IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'TOTAL_PROVISIONS',
		TO_CHAR(:old.TOTAL_PROVISIONS), NULL);
END IF;
IF :old.PRETS_ET_CREANCES_SUR_CLIENTS IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'PRETS_ET_CREANCES_SUR_CLIENTS',
		TO_CHAR(:old.PRETS_ET_CREANCES_SUR_CLIENTS), NULL);
END IF;
IF :old.DETTE_ENVERS_LA_CLIENTELE IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'DETTE_ENVERS_LA_CLIENTELE',
		TO_CHAR(:old.DETTE_ENVERS_LA_CLIENTELE), NULL);
END IF;
IF :old.CREANCES_EN_SOUFFRANCE IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'CREANCES_EN_SOUFFRANCE',
		TO_CHAR(:old.CREANCES_EN_SOUFFRANCE), NULL);
END IF;
IF :old.PLACEMENTS_ASSURANCE IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'PLACEMENTS_ASSURANCE',
		TO_CHAR(:old.PLACEMENTS_ASSURANCE), NULL);
END IF;
IF :old.PROVISIONS_TECHNIQUES IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'PROVISIONS_TECHNIQUES',
		TO_CHAR(:old.PROVISIONS_TECHNIQUES), NULL);
END IF;
IF :old.VALEUR_GLOBALE IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'VALEUR_GLOBALE',
		TO_CHAR(:old.VALEUR_GLOBALE), NULL);
END IF;
IF :old.VALEUR_PAR_TITRE IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'VALEUR_PAR_TITRE',
		TO_CHAR(:old.VALEUR_PAR_TITRE), NULL);
END IF;
IF :old.VALEUR_AVEC_DECOTE_15 IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'VALEUR_AVEC_DECOTE_15',
		TO_CHAR(:old.VALEUR_AVEC_DECOTE_15), NULL);
END IF;
IF :old.TAUX_DE_CROISSANCE_PERPETUEL IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'TAUX_DE_CROISSANCE_PERPETUEL',
		TO_CHAR(:old.TAUX_DE_CROISSANCE_PERPETUEL), NULL);
END IF;
IF :old.TAUX_SANS_RISQUE IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'TAUX_SANS_RISQUE',
		TO_CHAR(:old.TAUX_SANS_RISQUE), NULL);
END IF;
IF :old.PRIME_DE_RISQUE_MARCHE_ACTIONS IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'PRIME_DE_RISQUE_MARCHE_ACTIONS',
		TO_CHAR(:old.PRIME_DE_RISQUE_MARCHE_ACTIONS), NULL);
END IF;
IF :old.BETA IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'BETA',
		TO_CHAR(:old.BETA), NULL);
END IF;
IF :old.OCE IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'OCE',
		TO_CHAR(:old.OCE), NULL);
END IF;
IF :old.CASH_FLOW IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'CASH_FLOW',
		TO_CHAR(:old.CASH_FLOW), NULL);
END IF;
IF :old.FREE_CASH_FLOW IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'FREE_CASH_FLOW',
		TO_CHAR(:old.FREE_CASH_FLOW), NULL);
END IF;
IF :old.VALEUR_TERMINALE IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'VALEUR_TERMINALE',
		TO_CHAR(:old.VALEUR_TERMINALE), NULL);
END IF;
IF :old.DISCOUNT_FACTOR IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'DISCOUNT_FACTOR',
		TO_CHAR(:old.DISCOUNT_FACTOR), NULL);
END IF;
IF :old.DECOTE_SURCOTE IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'DECOTE_SURCOTE',
		TO_CHAR(:old.DECOTE_SURCOTE), NULL);
END IF;
IF :old.PE IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'PE',
		TO_CHAR(:old.PE), NULL);
END IF;
IF :old.PB IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'PB',
		TO_CHAR(:old.PB), NULL);
END IF;
IF :old.DY IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'DY',
		TO_CHAR(:old.DY), NULL);
END IF;
IF :old.COMMENTAIRE1 IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'COMMENTAIRE1',
		TO_CHAR(:old.COMMENTAIRE1), NULL);
END IF;
IF :old.COMMENTAIRE2 IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'COMMENTAIRE2',
		TO_CHAR(:old.COMMENTAIRE2), NULL);
END IF;
IF :old.DATE_DE_SAUVEGARDE IS NOT NULL
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_DELETED_', 'DATE_DE_SAUVEGARDE',
		TO_CHAR(:old.DATE_DE_SAUVEGARDE), NULL);
END IF;

ELSE
IF :old.EMETTEUR != :new.EMETTEUR
	OR (:old.EMETTEUR IS NULL AND :new.EMETTEUR IS NOT NULL)
	OR (:old.EMETTEUR IS NOT NULL AND :new.EMETTEUR IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'EMETTEUR',
		TO_CHAR(:old.EMETTEUR), TO_CHAR(:new.EMETTEUR));
END IF;
IF :old.ANNEE != :new.ANNEE
	OR (:old.ANNEE IS NULL AND :new.ANNEE IS NOT NULL)
	OR (:old.ANNEE IS NOT NULL AND :new.ANNEE IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'ANNEE',
		TO_CHAR(:old.ANNEE), TO_CHAR(:new.ANNEE));
END IF;
IF :old.NATURE_DE_DONNEES != :new.NATURE_DE_DONNEES
	OR (:old.NATURE_DE_DONNEES IS NULL AND :new.NATURE_DE_DONNEES IS NOT NULL)
	OR (:old.NATURE_DE_DONNEES IS NOT NULL AND :new.NATURE_DE_DONNEES IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'NATURE_DE_DONNEES',
		TO_CHAR(:old.NATURE_DE_DONNEES), TO_CHAR(:new.NATURE_DE_DONNEES));
END IF;
IF :old.SOURCES != :new.SOURCES
	OR (:old.SOURCES IS NULL AND :new.SOURCES IS NOT NULL)
	OR (:old.SOURCES IS NOT NULL AND :new.SOURCES IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'SOURCES',
		TO_CHAR(:old.SOURCES), TO_CHAR(:new.SOURCES));
END IF;
IF :old.SECTEUR != :new.SECTEUR
	OR (:old.SECTEUR IS NULL AND :new.SECTEUR IS NOT NULL)
	OR (:old.SECTEUR IS NOT NULL AND :new.SECTEUR IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'SECTEUR',
		TO_CHAR(:old.SECTEUR), TO_CHAR(:new.SECTEUR));
END IF;
IF :old.CA != :new.CA
	OR (:old.CA IS NULL AND :new.CA IS NOT NULL)
	OR (:old.CA IS NOT NULL AND :new.CA IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'CA',
		TO_CHAR(:old.CA), TO_CHAR(:new.CA));
END IF;
IF :old.DAP != :new.DAP
	OR (:old.DAP IS NULL AND :new.DAP IS NOT NULL)
	OR (:old.DAP IS NOT NULL AND :new.DAP IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'DAP',
		TO_CHAR(:old.DAP), TO_CHAR(:new.DAP));
END IF;
IF :old.CHARGES_DU_PERSONNEL != :new.CHARGES_DU_PERSONNEL
	OR (:old.CHARGES_DU_PERSONNEL IS NULL AND :new.CHARGES_DU_PERSONNEL IS NOT
NULL)
	OR (:old.CHARGES_DU_PERSONNEL IS NOT NULL AND :new.CHARGES_DU_PERSONNEL IS
NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'CHARGES_DU_PERSONNEL',
		TO_CHAR(:old.CHARGES_DU_PERSONNEL), TO_CHAR(:new.CHARGES_DU_PERSONNEL));
END IF;
IF :old.AUTRES_CHARGES_D_EXPLOITATION != :new.AUTRES_CHARGES_D_EXPLOITATION
	OR (:old.AUTRES_CHARGES_D_EXPLOITATION IS NULL AND
:new.AUTRES_CHARGES_D_EXPLOITATION IS NOT NULL)
	OR (:old.AUTRES_CHARGES_D_EXPLOITATION IS NOT NULL AND
:new.AUTRES_CHARGES_D_EXPLOITATION IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'AUTRES_CHARGES_D_EXPLOITATION',
		TO_CHAR(:old.AUTRES_CHARGES_D_EXPLOITATION),
TO_CHAR(:new.AUTRES_CHARGES_D_EXPLOITATION));
END IF;
IF :old.REX != :new.REX
	OR (:old.REX IS NULL AND :new.REX IS NOT NULL)
	OR (:old.REX IS NOT NULL AND :new.REX IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'REX',
		TO_CHAR(:old.REX), TO_CHAR(:new.REX));
END IF;
IF :old.RESULTAT_FINANCIER != :new.RESULTAT_FINANCIER
	OR (:old.RESULTAT_FINANCIER IS NULL AND :new.RESULTAT_FINANCIER IS NOT NULL)
	OR (:old.RESULTAT_FINANCIER IS NOT NULL AND :new.RESULTAT_FINANCIER IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'RESULTAT_FINANCIER',
		TO_CHAR(:old.RESULTAT_FINANCIER), TO_CHAR(:new.RESULTAT_FINANCIER));
END IF;
IF :old.AUTRES_CHARGES != :new.AUTRES_CHARGES
	OR (:old.AUTRES_CHARGES IS NULL AND :new.AUTRES_CHARGES IS NOT NULL)
	OR (:old.AUTRES_CHARGES IS NOT NULL AND :new.AUTRES_CHARGES IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'AUTRES_CHARGES',
		TO_CHAR(:old.AUTRES_CHARGES), TO_CHAR(:new.AUTRES_CHARGES));
END IF;
IF :old.AUTRES_PRODUITS1 != :new.AUTRES_PRODUITS1
	OR (:old.AUTRES_PRODUITS1 IS NULL AND :new.AUTRES_PRODUITS1 IS NOT NULL)
	OR (:old.AUTRES_PRODUITS1 IS NOT NULL AND :new.AUTRES_PRODUITS1 IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'AUTRES_PRODUITS1',
		TO_CHAR(:old.AUTRES_PRODUITS1), TO_CHAR(:new.AUTRES_PRODUITS1));
END IF;
IF :old.RESULTAT_AVANT_IMPOTS != :new.RESULTAT_AVANT_IMPOTS
	OR (:old.RESULTAT_AVANT_IMPOTS IS NULL AND :new.RESULTAT_AVANT_IMPOTS IS NOT
NULL)
	OR (:old.RESULTAT_AVANT_IMPOTS IS NOT NULL AND :new.RESULTAT_AVANT_IMPOTS IS
NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'RESULTAT_AVANT_IMPOTS',
		TO_CHAR(:old.RESULTAT_AVANT_IMPOTS), TO_CHAR(:new.RESULTAT_AVANT_IMPOTS));
END IF;
IF :old.IMPOTS != :new.IMPOTS
	OR (:old.IMPOTS IS NULL AND :new.IMPOTS IS NOT NULL)
	OR (:old.IMPOTS IS NOT NULL AND :new.IMPOTS IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'IMPOTS',
		TO_CHAR(:old.IMPOTS), TO_CHAR(:new.IMPOTS));
END IF;
IF :old.RESULTAT_NET != :new.RESULTAT_NET
	OR (:old.RESULTAT_NET IS NULL AND :new.RESULTAT_NET IS NOT NULL)
	OR (:old.RESULTAT_NET IS NOT NULL AND :new.RESULTAT_NET IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'RESULTAT_NET',
		TO_CHAR(:old.RESULTAT_NET), TO_CHAR(:new.RESULTAT_NET));
END IF;
IF :old.RESULTAT_MIS_EN_EQUIVALENCE != :new.RESULTAT_MIS_EN_EQUIVALENCE
	OR (:old.RESULTAT_MIS_EN_EQUIVALENCE IS NULL AND
:new.RESULTAT_MIS_EN_EQUIVALENCE IS NOT NULL)
	OR (:old.RESULTAT_MIS_EN_EQUIVALENCE IS NOT NULL AND
:new.RESULTAT_MIS_EN_EQUIVALENCE IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'RESULTAT_MIS_EN_EQUIVALENCE',
		TO_CHAR(:old.RESULTAT_MIS_EN_EQUIVALENCE),
TO_CHAR(:new.RESULTAT_MIS_EN_EQUIVALENCE));
END IF;
IF :old.RNPG != :new.RNPG
	OR (:old.RNPG IS NULL AND :new.RNPG IS NOT NULL)
	OR (:old.RNPG IS NOT NULL AND :new.RNPG IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'RNPG',
		TO_CHAR(:old.RNPG), TO_CHAR(:new.RNPG));
END IF;
IF :old.DIVIDENDES != :new.DIVIDENDES
	OR (:old.DIVIDENDES IS NULL AND :new.DIVIDENDES IS NOT NULL)
	OR (:old.DIVIDENDES IS NOT NULL AND :new.DIVIDENDES IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'DIVIDENDES',
		TO_CHAR(:old.DIVIDENDES), TO_CHAR(:new.DIVIDENDES));
END IF;
IF :old.INVESTISSEMENTS != :new.INVESTISSEMENTS
	OR (:old.INVESTISSEMENTS IS NULL AND :new.INVESTISSEMENTS IS NOT NULL)
	OR (:old.INVESTISSEMENTS IS NOT NULL AND :new.INVESTISSEMENTS IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'INVESTISSEMENTS',
		TO_CHAR(:old.INVESTISSEMENTS), TO_CHAR(:new.INVESTISSEMENTS));
END IF;
IF :old.ACHATS != :new.ACHATS
	OR (:old.ACHATS IS NULL AND :new.ACHATS IS NOT NULL)
	OR (:old.ACHATS IS NOT NULL AND :new.ACHATS IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'ACHATS',
		TO_CHAR(:old.ACHATS), TO_CHAR(:new.ACHATS));
END IF;
IF :old.MARGE_D_INTERETS != :new.MARGE_D_INTERETS
	OR (:old.MARGE_D_INTERETS IS NULL AND :new.MARGE_D_INTERETS IS NOT NULL)
	OR (:old.MARGE_D_INTERETS IS NOT NULL AND :new.MARGE_D_INTERETS IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'MARGE_D_INTERETS',
		TO_CHAR(:old.MARGE_D_INTERETS), TO_CHAR(:new.MARGE_D_INTERETS));
END IF;
IF :old.MARGE_SUR_COMMISSIONS != :new.MARGE_SUR_COMMISSIONS
	OR (:old.MARGE_SUR_COMMISSIONS IS NULL AND :new.MARGE_SUR_COMMISSIONS IS NOT
NULL)
	OR (:old.MARGE_SUR_COMMISSIONS IS NOT NULL AND :new.MARGE_SUR_COMMISSIONS IS
NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'MARGE_SUR_COMMISSIONS',
		TO_CHAR(:old.MARGE_SUR_COMMISSIONS), TO_CHAR(:new.MARGE_SUR_COMMISSIONS));
END IF;
IF :old.RESULTATS_ACTIVITES_MARCHE != :new.RESULTATS_ACTIVITES_MARCHE
	OR (:old.RESULTATS_ACTIVITES_MARCHE IS NULL AND :new.RESULTATS_ACTIVITES_MARCHE
IS NOT NULL)
	OR (:old.RESULTATS_ACTIVITES_MARCHE IS NOT NULL AND
:new.RESULTATS_ACTIVITES_MARCHE IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'RESULTATS_ACTIVITES_MARCHE',
		TO_CHAR(:old.RESULTATS_ACTIVITES_MARCHE),
TO_CHAR(:new.RESULTATS_ACTIVITES_MARCHE));
END IF;
IF :old.AUTRES_PRODUITS2 != :new.AUTRES_PRODUITS2
	OR (:old.AUTRES_PRODUITS2 IS NULL AND :new.AUTRES_PRODUITS2 IS NOT NULL)
	OR (:old.AUTRES_PRODUITS2 IS NOT NULL AND :new.AUTRES_PRODUITS2 IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'AUTRES_PRODUITS2',
		TO_CHAR(:old.AUTRES_PRODUITS2), TO_CHAR(:new.AUTRES_PRODUITS2));
END IF;
IF :old.PNB != :new.PNB
	OR (:old.PNB IS NULL AND :new.PNB IS NOT NULL)
	OR (:old.PNB IS NOT NULL AND :new.PNB IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'PNB',
		TO_CHAR(:old.PNB), TO_CHAR(:new.PNB));
END IF;
IF :old.CHARGES_D_EXPLOITATION != :new.CHARGES_D_EXPLOITATION
	OR (:old.CHARGES_D_EXPLOITATION IS NULL AND :new.CHARGES_D_EXPLOITATION IS NOT
NULL)
	OR (:old.CHARGES_D_EXPLOITATION IS NOT NULL AND :new.CHARGES_D_EXPLOITATION IS
NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'CHARGES_D_EXPLOITATION',
		TO_CHAR(:old.CHARGES_D_EXPLOITATION), TO_CHAR(:new.CHARGES_D_EXPLOITATION));
END IF;
IF :old.RBE != :new.RBE
	OR (:old.RBE IS NULL AND :new.RBE IS NOT NULL)
	OR (:old.RBE IS NOT NULL AND :new.RBE IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'RBE',
		TO_CHAR(:old.RBE), TO_CHAR(:new.RBE));
END IF;
IF :old.COUT_DU_RISQUE != :new.COUT_DU_RISQUE
	OR (:old.COUT_DU_RISQUE IS NULL AND :new.COUT_DU_RISQUE IS NOT NULL)
	OR (:old.COUT_DU_RISQUE IS NOT NULL AND :new.COUT_DU_RISQUE IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'COUT_DU_RISQUE',
		TO_CHAR(:old.COUT_DU_RISQUE), TO_CHAR(:new.COUT_DU_RISQUE));
END IF;
IF :old.PRIMES_EMISES_NETTES != :new.PRIMES_EMISES_NETTES
	OR (:old.PRIMES_EMISES_NETTES IS NULL AND :new.PRIMES_EMISES_NETTES IS NOT
NULL)
	OR (:old.PRIMES_EMISES_NETTES IS NOT NULL AND :new.PRIMES_EMISES_NETTES IS
NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'PRIMES_EMISES_NETTES',
		TO_CHAR(:old.PRIMES_EMISES_NETTES), TO_CHAR(:new.PRIMES_EMISES_NETTES));
END IF;
IF :old.PRODUIT_DES_PLACEMENTS != :new.PRODUIT_DES_PLACEMENTS
	OR (:old.PRODUIT_DES_PLACEMENTS IS NULL AND :new.PRODUIT_DES_PLACEMENTS IS NOT
NULL)
	OR (:old.PRODUIT_DES_PLACEMENTS IS NOT NULL AND :new.PRODUIT_DES_PLACEMENTS IS
NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'PRODUIT_DES_PLACEMENTS',
		TO_CHAR(:old.PRODUIT_DES_PLACEMENTS), TO_CHAR(:new.PRODUIT_DES_PLACEMENTS));
END IF;
IF :old.PRESTATIONS_ET_FRAIS_PAYES != :new.PRESTATIONS_ET_FRAIS_PAYES
	OR (:old.PRESTATIONS_ET_FRAIS_PAYES IS NULL AND :new.PRESTATIONS_ET_FRAIS_PAYES
IS NOT NULL)
	OR (:old.PRESTATIONS_ET_FRAIS_PAYES IS NOT NULL AND
:new.PRESTATIONS_ET_FRAIS_PAYES IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'PRESTATIONS_ET_FRAIS_PAYES',
		TO_CHAR(:old.PRESTATIONS_ET_FRAIS_PAYES),
TO_CHAR(:new.PRESTATIONS_ET_FRAIS_PAYES));
END IF;
IF :old.VARIATION_PROV_TECHNIQUES != :new.VARIATION_PROV_TECHNIQUES
	OR (:old.VARIATION_PROV_TECHNIQUES IS NULL AND :new.VARIATION_PROV_TECHNIQUES
IS NOT NULL)
	OR (:old.VARIATION_PROV_TECHNIQUES IS NOT NULL AND
:new.VARIATION_PROV_TECHNIQUES IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'VARIATION_PROV_TECHNIQUES',
		TO_CHAR(:old.VARIATION_PROV_TECHNIQUES),
TO_CHAR(:new.VARIATION_PROV_TECHNIQUES));
END IF;
IF :old.SOLDE_DE_REASSURANCE != :new.SOLDE_DE_REASSURANCE
	OR (:old.SOLDE_DE_REASSURANCE IS NULL AND :new.SOLDE_DE_REASSURANCE IS NOT
NULL)
	OR (:old.SOLDE_DE_REASSURANCE IS NOT NULL AND :new.SOLDE_DE_REASSURANCE IS
NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'SOLDE_DE_REASSURANCE',
		TO_CHAR(:old.SOLDE_DE_REASSURANCE), TO_CHAR(:new.SOLDE_DE_REASSURANCE));
END IF;
IF :old.VALEUR_AJOUTEE != :new.VALEUR_AJOUTEE
	OR (:old.VALEUR_AJOUTEE IS NULL AND :new.VALEUR_AJOUTEE IS NOT NULL)
	OR (:old.VALEUR_AJOUTEE IS NOT NULL AND :new.VALEUR_AJOUTEE IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'VALEUR_AJOUTEE',
		TO_CHAR(:old.VALEUR_AJOUTEE), TO_CHAR(:new.VALEUR_AJOUTEE));
END IF;
IF :old.RESULTAT_TECHNIQUE != :new.RESULTAT_TECHNIQUE
	OR (:old.RESULTAT_TECHNIQUE IS NULL AND :new.RESULTAT_TECHNIQUE IS NOT NULL)
	OR (:old.RESULTAT_TECHNIQUE IS NOT NULL AND :new.RESULTAT_TECHNIQUE IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'RESULTAT_TECHNIQUE',
		TO_CHAR(:old.RESULTAT_TECHNIQUE), TO_CHAR(:new.RESULTAT_TECHNIQUE));
END IF;
IF :old.RESULTAT_NON_TECHNIQUE != :new.RESULTAT_NON_TECHNIQUE
	OR (:old.RESULTAT_NON_TECHNIQUE IS NULL AND :new.RESULTAT_NON_TECHNIQUE IS NOT
NULL)
	OR (:old.RESULTAT_NON_TECHNIQUE IS NOT NULL AND :new.RESULTAT_NON_TECHNIQUE IS
NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'RESULTAT_NON_TECHNIQUE',
		TO_CHAR(:old.RESULTAT_NON_TECHNIQUE), TO_CHAR(:new.RESULTAT_NON_TECHNIQUE));
END IF;
IF :old.TOTAL_ACTIF != :new.TOTAL_ACTIF
	OR (:old.TOTAL_ACTIF IS NULL AND :new.TOTAL_ACTIF IS NOT NULL)
	OR (:old.TOTAL_ACTIF IS NOT NULL AND :new.TOTAL_ACTIF IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'TOTAL_ACTIF',
		TO_CHAR(:old.TOTAL_ACTIF), TO_CHAR(:new.TOTAL_ACTIF));
END IF;
IF :old.FONDS_PROPRES != :new.FONDS_PROPRES
	OR (:old.FONDS_PROPRES IS NULL AND :new.FONDS_PROPRES IS NOT NULL)
	OR (:old.FONDS_PROPRES IS NOT NULL AND :new.FONDS_PROPRES IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'FONDS_PROPRES',
		TO_CHAR(:old.FONDS_PROPRES), TO_CHAR(:new.FONDS_PROPRES));
END IF;
IF :old.DETTE_NETTE != :new.DETTE_NETTE
	OR (:old.DETTE_NETTE IS NULL AND :new.DETTE_NETTE IS NOT NULL)
	OR (:old.DETTE_NETTE IS NOT NULL AND :new.DETTE_NETTE IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'DETTE_NETTE',
		TO_CHAR(:old.DETTE_NETTE), TO_CHAR(:new.DETTE_NETTE));
END IF;
IF :old.ACTIF_CIRCULANT != :new.ACTIF_CIRCULANT
	OR (:old.ACTIF_CIRCULANT IS NULL AND :new.ACTIF_CIRCULANT IS NOT NULL)
	OR (:old.ACTIF_CIRCULANT IS NOT NULL AND :new.ACTIF_CIRCULANT IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'ACTIF_CIRCULANT',
		TO_CHAR(:old.ACTIF_CIRCULANT), TO_CHAR(:new.ACTIF_CIRCULANT));
END IF;
IF :old.PASSIF_CIRCULANT != :new.PASSIF_CIRCULANT
	OR (:old.PASSIF_CIRCULANT IS NULL AND :new.PASSIF_CIRCULANT IS NOT NULL)
	OR (:old.PASSIF_CIRCULANT IS NOT NULL AND :new.PASSIF_CIRCULANT IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'PASSIF_CIRCULANT',
		TO_CHAR(:old.PASSIF_CIRCULANT), TO_CHAR(:new.PASSIF_CIRCULANT));
END IF;
IF :old.IMMOBILISATIONS_CORPORELLES != :new.IMMOBILISATIONS_CORPORELLES
	OR (:old.IMMOBILISATIONS_CORPORELLES IS NULL AND
:new.IMMOBILISATIONS_CORPORELLES IS NOT NULL)
	OR (:old.IMMOBILISATIONS_CORPORELLES IS NOT NULL AND
:new.IMMOBILISATIONS_CORPORELLES IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'IMMOBILISATIONS_CORPORELLES',
		TO_CHAR(:old.IMMOBILISATIONS_CORPORELLES),
TO_CHAR(:new.IMMOBILISATIONS_CORPORELLES));
END IF;
IF :old.TOTAL_PROVISIONS != :new.TOTAL_PROVISIONS
	OR (:old.TOTAL_PROVISIONS IS NULL AND :new.TOTAL_PROVISIONS IS NOT NULL)
	OR (:old.TOTAL_PROVISIONS IS NOT NULL AND :new.TOTAL_PROVISIONS IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'TOTAL_PROVISIONS',
		TO_CHAR(:old.TOTAL_PROVISIONS), TO_CHAR(:new.TOTAL_PROVISIONS));
END IF;
IF :old.PRETS_ET_CREANCES_SUR_CLIENTS != :new.PRETS_ET_CREANCES_SUR_CLIENTS
	OR (:old.PRETS_ET_CREANCES_SUR_CLIENTS IS NULL AND
:new.PRETS_ET_CREANCES_SUR_CLIENTS IS NOT NULL)
	OR (:old.PRETS_ET_CREANCES_SUR_CLIENTS IS NOT NULL AND
:new.PRETS_ET_CREANCES_SUR_CLIENTS IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'PRETS_ET_CREANCES_SUR_CLIENTS',
		TO_CHAR(:old.PRETS_ET_CREANCES_SUR_CLIENTS),
TO_CHAR(:new.PRETS_ET_CREANCES_SUR_CLIENTS));
END IF;
IF :old.DETTE_ENVERS_LA_CLIENTELE != :new.DETTE_ENVERS_LA_CLIENTELE
	OR (:old.DETTE_ENVERS_LA_CLIENTELE IS NULL AND :new.DETTE_ENVERS_LA_CLIENTELE
IS NOT NULL)
	OR (:old.DETTE_ENVERS_LA_CLIENTELE IS NOT NULL AND
:new.DETTE_ENVERS_LA_CLIENTELE IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'DETTE_ENVERS_LA_CLIENTELE',
		TO_CHAR(:old.DETTE_ENVERS_LA_CLIENTELE),
TO_CHAR(:new.DETTE_ENVERS_LA_CLIENTELE));
END IF;
IF :old.CREANCES_EN_SOUFFRANCE != :new.CREANCES_EN_SOUFFRANCE
	OR (:old.CREANCES_EN_SOUFFRANCE IS NULL AND :new.CREANCES_EN_SOUFFRANCE IS NOT
NULL)
	OR (:old.CREANCES_EN_SOUFFRANCE IS NOT NULL AND :new.CREANCES_EN_SOUFFRANCE IS
NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'CREANCES_EN_SOUFFRANCE',
		TO_CHAR(:old.CREANCES_EN_SOUFFRANCE), TO_CHAR(:new.CREANCES_EN_SOUFFRANCE));
END IF;
IF :old.PLACEMENTS_ASSURANCE != :new.PLACEMENTS_ASSURANCE
	OR (:old.PLACEMENTS_ASSURANCE IS NULL AND :new.PLACEMENTS_ASSURANCE IS NOT
NULL)
	OR (:old.PLACEMENTS_ASSURANCE IS NOT NULL AND :new.PLACEMENTS_ASSURANCE IS
NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'PLACEMENTS_ASSURANCE',
		TO_CHAR(:old.PLACEMENTS_ASSURANCE), TO_CHAR(:new.PLACEMENTS_ASSURANCE));
END IF;
IF :old.PROVISIONS_TECHNIQUES != :new.PROVISIONS_TECHNIQUES
	OR (:old.PROVISIONS_TECHNIQUES IS NULL AND :new.PROVISIONS_TECHNIQUES IS NOT
NULL)
	OR (:old.PROVISIONS_TECHNIQUES IS NOT NULL AND :new.PROVISIONS_TECHNIQUES IS
NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'PROVISIONS_TECHNIQUES',
		TO_CHAR(:old.PROVISIONS_TECHNIQUES), TO_CHAR(:new.PROVISIONS_TECHNIQUES));
END IF;
IF :old.VALEUR_GLOBALE != :new.VALEUR_GLOBALE
	OR (:old.VALEUR_GLOBALE IS NULL AND :new.VALEUR_GLOBALE IS NOT NULL)
	OR (:old.VALEUR_GLOBALE IS NOT NULL AND :new.VALEUR_GLOBALE IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'VALEUR_GLOBALE',
		TO_CHAR(:old.VALEUR_GLOBALE), TO_CHAR(:new.VALEUR_GLOBALE));
END IF;
IF :old.VALEUR_PAR_TITRE != :new.VALEUR_PAR_TITRE
	OR (:old.VALEUR_PAR_TITRE IS NULL AND :new.VALEUR_PAR_TITRE IS NOT NULL)
	OR (:old.VALEUR_PAR_TITRE IS NOT NULL AND :new.VALEUR_PAR_TITRE IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'VALEUR_PAR_TITRE',
		TO_CHAR(:old.VALEUR_PAR_TITRE), TO_CHAR(:new.VALEUR_PAR_TITRE));
END IF;
IF :old.VALEUR_AVEC_DECOTE_15 != :new.VALEUR_AVEC_DECOTE_15
	OR (:old.VALEUR_AVEC_DECOTE_15 IS NULL AND :new.VALEUR_AVEC_DECOTE_15 IS NOT
NULL)
	OR (:old.VALEUR_AVEC_DECOTE_15 IS NOT NULL AND :new.VALEUR_AVEC_DECOTE_15 IS
NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'VALEUR_AVEC_DECOTE_15',
		TO_CHAR(:old.VALEUR_AVEC_DECOTE_15), TO_CHAR(:new.VALEUR_AVEC_DECOTE_15));
END IF;
IF :old.TAUX_DE_CROISSANCE_PERPETUEL != :new.TAUX_DE_CROISSANCE_PERPETUEL
	OR (:old.TAUX_DE_CROISSANCE_PERPETUEL IS NULL AND
:new.TAUX_DE_CROISSANCE_PERPETUEL IS NOT NULL)
	OR (:old.TAUX_DE_CROISSANCE_PERPETUEL IS NOT NULL AND
:new.TAUX_DE_CROISSANCE_PERPETUEL IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'TAUX_DE_CROISSANCE_PERPETUEL',
		TO_CHAR(:old.TAUX_DE_CROISSANCE_PERPETUEL),
TO_CHAR(:new.TAUX_DE_CROISSANCE_PERPETUEL));
END IF;
IF :old.TAUX_SANS_RISQUE != :new.TAUX_SANS_RISQUE
	OR (:old.TAUX_SANS_RISQUE IS NULL AND :new.TAUX_SANS_RISQUE IS NOT NULL)
	OR (:old.TAUX_SANS_RISQUE IS NOT NULL AND :new.TAUX_SANS_RISQUE IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'TAUX_SANS_RISQUE',
		TO_CHAR(:old.TAUX_SANS_RISQUE), TO_CHAR(:new.TAUX_SANS_RISQUE));
END IF;
IF :old.PRIME_DE_RISQUE_MARCHE_ACTIONS != :new.PRIME_DE_RISQUE_MARCHE_ACTIONS
	OR (:old.PRIME_DE_RISQUE_MARCHE_ACTIONS IS NULL AND
:new.PRIME_DE_RISQUE_MARCHE_ACTIONS IS NOT NULL)
	OR (:old.PRIME_DE_RISQUE_MARCHE_ACTIONS IS NOT NULL AND
:new.PRIME_DE_RISQUE_MARCHE_ACTIONS IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'PRIME_DE_RISQUE_MARCHE_ACTIONS',
		TO_CHAR(:old.PRIME_DE_RISQUE_MARCHE_ACTIONS),
TO_CHAR(:new.PRIME_DE_RISQUE_MARCHE_ACTIONS));
END IF;
IF :old.BETA != :new.BETA
	OR (:old.BETA IS NULL AND :new.BETA IS NOT NULL)
	OR (:old.BETA IS NOT NULL AND :new.BETA IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'BETA',
		TO_CHAR(:old.BETA), TO_CHAR(:new.BETA));
END IF;
IF :old.OCE != :new.OCE
	OR (:old.OCE IS NULL AND :new.OCE IS NOT NULL)
	OR (:old.OCE IS NOT NULL AND :new.OCE IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'OCE',
		TO_CHAR(:old.OCE), TO_CHAR(:new.OCE));
END IF;
IF :old.CASH_FLOW != :new.CASH_FLOW
	OR (:old.CASH_FLOW IS NULL AND :new.CASH_FLOW IS NOT NULL)
	OR (:old.CASH_FLOW IS NOT NULL AND :new.CASH_FLOW IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'CASH_FLOW',
		TO_CHAR(:old.CASH_FLOW), TO_CHAR(:new.CASH_FLOW));
END IF;
IF :old.FREE_CASH_FLOW != :new.FREE_CASH_FLOW
	OR (:old.FREE_CASH_FLOW IS NULL AND :new.FREE_CASH_FLOW IS NOT NULL)
	OR (:old.FREE_CASH_FLOW IS NOT NULL AND :new.FREE_CASH_FLOW IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'FREE_CASH_FLOW',
		TO_CHAR(:old.FREE_CASH_FLOW), TO_CHAR(:new.FREE_CASH_FLOW));
END IF;
IF :old.VALEUR_TERMINALE != :new.VALEUR_TERMINALE
	OR (:old.VALEUR_TERMINALE IS NULL AND :new.VALEUR_TERMINALE IS NOT NULL)
	OR (:old.VALEUR_TERMINALE IS NOT NULL AND :new.VALEUR_TERMINALE IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'VALEUR_TERMINALE',
		TO_CHAR(:old.VALEUR_TERMINALE), TO_CHAR(:new.VALEUR_TERMINALE));
END IF;
IF :old.DISCOUNT_FACTOR != :new.DISCOUNT_FACTOR
	OR (:old.DISCOUNT_FACTOR IS NULL AND :new.DISCOUNT_FACTOR IS NOT NULL)
	OR (:old.DISCOUNT_FACTOR IS NOT NULL AND :new.DISCOUNT_FACTOR IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'DISCOUNT_FACTOR',
		TO_CHAR(:old.DISCOUNT_FACTOR), TO_CHAR(:new.DISCOUNT_FACTOR));
END IF;
IF :old.DECOTE_SURCOTE != :new.DECOTE_SURCOTE
	OR (:old.DECOTE_SURCOTE IS NULL AND :new.DECOTE_SURCOTE IS NOT NULL)
	OR (:old.DECOTE_SURCOTE IS NOT NULL AND :new.DECOTE_SURCOTE IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'DECOTE_SURCOTE',
		TO_CHAR(:old.DECOTE_SURCOTE), TO_CHAR(:new.DECOTE_SURCOTE));
END IF;
IF :old.PE != :new.PE
	OR (:old.PE IS NULL AND :new.PE IS NOT NULL)
	OR (:old.PE IS NOT NULL AND :new.PE IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'PE',
		TO_CHAR(:old.PE), TO_CHAR(:new.PE));
END IF;
IF :old.PB != :new.PB
	OR (:old.PB IS NULL AND :new.PB IS NOT NULL)
	OR (:old.PB IS NOT NULL AND :new.PB IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'PB',
		TO_CHAR(:old.PB), TO_CHAR(:new.PB));
END IF;
IF :old.DY != :new.DY
	OR (:old.DY IS NULL AND :new.DY IS NOT NULL)
	OR (:old.DY IS NOT NULL AND :new.DY IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'DY',
		TO_CHAR(:old.DY), TO_CHAR(:new.DY));
END IF;
IF :old.COMMENTAIRE1 != :new.COMMENTAIRE1
	OR (:old.COMMENTAIRE1 IS NULL AND :new.COMMENTAIRE1 IS NOT NULL)
	OR (:old.COMMENTAIRE1 IS NOT NULL AND :new.COMMENTAIRE1 IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'COMMENTAIRE1',
		TO_CHAR(:old.COMMENTAIRE1), TO_CHAR(:new.COMMENTAIRE1));
END IF;
IF :old.COMMENTAIRE2 != :new.COMMENTAIRE2
	OR (:old.COMMENTAIRE2 IS NULL AND :new.COMMENTAIRE2 IS NOT NULL)
	OR (:old.COMMENTAIRE2 IS NOT NULL AND :new.COMMENTAIRE2 IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'COMMENTAIRE2',
		TO_CHAR(:old.COMMENTAIRE2), TO_CHAR(:new.COMMENTAIRE2));
END IF;
IF :old.DATE_DE_SAUVEGARDE != :new.DATE_DE_SAUVEGARDE
	OR (:old.DATE_DE_SAUVEGARDE IS NULL AND :new.DATE_DE_SAUVEGARDE IS NOT NULL)
	OR (:old.DATE_DE_SAUVEGARDE IS NOT NULL AND :new.DATE_DE_SAUVEGARDE IS NULL)
THEN
	INSERT INTO CFG_DONNEES_CORPORATES_AUDIT (emetteur, annee, nature_de_donnees,
sources, datemodif, usermodif,typemodif, field, oldval, newval)
	VALUES (aud_emetteur, aud_annee, aud_nature_de_donnees, aud_sources,
aud_datemodif, aud_usermodif,'_UPDATED_', 'DATE_DE_SAUVEGARDE',
		TO_CHAR(:old.DATE_DE_SAUVEGARDE), TO_CHAR(:new.DATE_DE_SAUVEGARDE));
END IF;
	END IF;
END;
/


--}}SOPHIS_SQL