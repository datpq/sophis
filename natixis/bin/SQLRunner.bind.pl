#!c:\Perl\bin\perl


use strict qw(vars);
use Carp qw(cluck);
use DBI;


################################################
# �crit le r�sultat de la requete $ARG0 dans le fichier CSV $ARG1
# arguments optionnels : -append (�crit � la suite du fichier $ARG1)
#											 : -header pour �crire les ent�tes 
# les arguments suppl�mentaires sont remplac�s dans le corps de la requ�te SQL 
# ==> (on remplace les cha�nes du type &X avec X un chiffre, premier indice = 0) 
################################################

#################
#DATABASE CONFIG
#################
my $ora_instance = $ENV{'DB_INSTANCE'} ;
my $ora_user = $ENV{'DB_LOGIN'} ;
my $ora_password = $ENV{'DB_PASSWORD'} ;


#####################################################################################

my ($OutputFile, $SQLFile, $appendToOutput, $withHeader, $decimalSeparator); #arguments obligatoires et options
my @sqlParams = ();  #tableau des arguments � chercher dans les requetes.

#parsing des arguments requis
$SQLFile = @ARGV[0];
$OutputFile = @ARGV[1];	

#parsing des arguments optionnels	(si le param�tre ne correspond pas � une option, il est consid�r� comme un param�tre � ins�rer dans la requete.
for ( my $i = 2 ; $i <= $#ARGV ; $i ++ ) {	
	if (@ARGV[$i] =~ /^-append$/ ){
		$appendToOutput = 1;
		print STDOUT "Appending to $OutputFile\n";
	}elsif (@ARGV[$i] =~ /^-header$/ ){
		$withHeader = 1;
		print STDOUT "will write headers to output file\n";
	} elsif (@ARGV[$i] =~ /^-decimalSep$/ ) {
		$decimalSeparator = $ARGV[$i+1] ;
		print STDOUT "decimal Separator will be forced to [$decimalSeparator]\n";
	} else {
		 push(@sqlParams, @ARGV[$i]);
		 print STDOUT "[&" . $#sqlParams ."] will be replaced by [" . @ARGV[$i] ."]\n"; 
	}
}

		

my $SQL;
{
	local ($/, *SQLFILE );
	open (SQLFILE, $SQLFile) || die "impossible d'ouvrir le fichier SQL : $!";
	$SQL = <SQLFILE>
}

#remplacement des param�tres dans la requ�te SQL.
# dans cette version, on les remplace seulement par des param�tres plus facilement bindables
for (my $i = 0; $i <= $#sqlParams ; $i ++ ){	
	my $toReplace = "'&" . $i ."'"; 
	my $replaced = ":param" .$i;
	$SQL =~ s/$toReplace/$replaced/g ;
}

print STDOUT $SQL;

#connexion � la base
my $dbh = DBI->connect( "dbi:Oracle:$ora_instance",
                                 $ora_user,
                                 $ora_password,
                               ) || die "Database connection not made: $DBI::errstr";

$dbh->do("ALTER SESSION SET NLS_LANGUAGE='FRENCH'");
$dbh->do("ALTER SESSION SET NLS_TERRITORY='FRANCE'");

#pr�paration du statement                               
my $sth = $dbh->prepare($SQL)  or die "Could not prepare statement: " . $dbh->errstr;

for (my $i = 0; $i <= $#sqlParams ; $i ++ ){	
	my $toReplace = ":param" . $i ;
	print STDOUT "Binding $toReplace to @sqlParams[$i]\n";
	$sth->bind_param( $toReplace, @sqlParams[$i] );
}

print STDOUT "executing $SQLFile ...\n" ;                               
                               
my @data;

#ex�cution du statement
$sth->execute or die "Could not execute statement: " . $dbh->errstr;

#ouverture du fichier de sortie
open ( OUTPUT, ($appendToOutput ? ">>" : ">") . $OutputFile ) || die "Impossible d'ouvrir le fichier de sortie : $!" ;

#�criture optionnelle des entetes du fichier
if ($withHeader)
{
	for (my $i = 0 ; $i < $sth->{NUM_OF_FIELDS} ; $i++ ){
		 print OUTPUT $sth->{NAME}->[$i];
		 print OUTPUT ";" unless ($i == $sth->{NUM_OF_FIELDS} - 1);
	}	
	print OUTPUT "\n";
}



#�criture du CSV
while ( @data = $sth->fetchrow_array() )
{
		
	my $i = 0; 
	
	#iteration sur toutes les lignes du ResultSet
	foreach (@data){		
		
		#si le type du resultat est DECIMAL et qu'on doit forcer le s�parateur de d�cimal
		if ( defined($decimalSeparator) and ($dbh->type_info($sth->{TYPE}[$i])->{TYPE_NAME} eq "DECIMAL" or
    $dbh->type_info($sth->{TYPE}[$i])->{TYPE_NAME} eq "DOUBLE PRECISION") ) {
			#print "found new decimal value\n";
			s/[^0-9-]/$decimalSeparator/g;
			print OUTPUT $_;
		} else {
			print OUTPUT $_ ;
		}
		print OUTPUT ";" unless $i == $#data;
		$i ++;
	}
	print OUTPUT "\n";
}

my $rows_count = $sth->rows;
print STDOUT "$rows_count row(s) written\n";
                
$dbh->disconnect;
                               
close OUTPUT;   

exit (0);