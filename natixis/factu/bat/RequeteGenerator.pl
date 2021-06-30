#######################################################################################################
#
# Le code suivant fait 2 choses
# Premièrement : Il copie l'extraction des deals internes dans le dossier CFT
# Deuxièmement : Il gènère une clause sql in (a,b,c,...) à partir d'une liste de valeurs a,b,c,...
# contenue dans un fichier et met la sortie dans une variable template contenue dans un autre 
# fichier passé en paramètre.
#
# Usage: RequeteGenerator.pl fichier_liste.txt fichier_sortie.sql
# Application : Report des contreparties, la liste des contreparties est fournie dans un fichier plat.
#
# reverse('©'), Aout 2009, Ibrahima Raya TALL, irayatall@exchange.cmi.net
#

use strict;
use File::Basename;
use File::Copy;
#use lib "D:/Product/Perl64/site/lib";
#use lib "D:/Product/Perl64/lib";
use Config::IniFiles;

# Parameters check
if ($#ARGV != 3) {
    die("Usage: RequeteGenerator fichier.ini fichier_liste.txt table_en_base deals_internes\n");
};

# Fichier contenant la liste des contreparties
my $fichier_ini  = $ARGV[0];
# Fichier csv des deals internes à copier dans un dossier reseau
# my $fichier_deals_internes  = $ARGV[1];
# Fichier modèle contenant la variable template à remplacer par le sql généré
my $fichier_template  = $ARGV[1];
# Fichier sql généré à partir du fichier modèle
my $fichier_genere  = $ARGV[2];
# Fichier modèle contenant la variable template à remplacer par le sql généré
my $table_en_base  = $ARGV[3];

#Lecture du fichier des contreparties et génération de la requete
my $chaine_requete="";
my $token1 = "";
my $token2 = "";
my $token3 = "";

# Recuperation des cle/valeur définis dans le fichier .ini
my $ini_reader = Config::IniFiles->new(-file => $fichier_ini);
my $networkFilePath = $ini_reader->val("report_ctpy", "file");
my $dealsInternesNetworkFilePath = $ini_reader->val('deals_internes', "file");

# copie du fichier local des deals internes vers le dossier reseau
#copy($fichier_deals_internes, "$dealsInternesNetworkFilePath") or die "Un probleme est survenu lors de la copie du fichier local des deals internes ".$fichier_deals_internes." vers le dossier resau ".$dealsInternesNetworkFilePath." $! \n";

open FICHIER_LISTE, "< $networkFilePath" or die "Un probleme est survenu pendant l ouverture du fichier input des contreparties : ".$networkFilePath."\n";
while (<FICHIER_LISTE>) {
    chomp;

    # Split on single tab
    my @Fld = split(';', $_);

    # Une contrepartie et un nom par ligne
    $token1 = @Fld[0];
    $token2 = @Fld[1];
    $token3 = @Fld[2];

    # On echappe les quotes en sql pas en perl
    $token2 =~ s/'/''/g;
    
    $chaine_requete .= "insert into NATIXIS_FACTU_REPORT_CTPY values (".$token1.",'".$token2."','".$token3."');\n"
}
# On ferme la requete
close(FICHIER_LISTE);

# Génération du fichier sql 12_3jm_12h_insertContreparties.sql à partir du modele
open FICHIER_MODELE, "< $fichier_template" or die "Un probleme est survenu pendant l ouverture du fichier modele :".$fichier_template."\n";
open FICHIER_GENERE, "> $fichier_genere" or die "Probleme pendant l ouverture du fichier resultat : ".$fichier_genere."\n";
while (<FICHIER_MODELE>) {
    chomp;
    # Split on single tab
    my @Fld = split('\t', $_);
    
    foreach my $token (@Fld) {
        # On remplace la variable template par la nouvelle requete
        $token =~ s/~~~~LISTE_INSERT_CONTREPARTIES~~~~/$chaine_requete/;
          # On écrit le nouveau fichier
          print FICHIER_GENERE $token."\n";
    }
}
close(FICHIER_MODELE);
close(FICHIER_GENERE);
