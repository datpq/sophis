################################################################################
#
#
# Le code suivant convertit un ensemble de fichiers csv en fichier Excel
# en mettant chaque fichier csv dans un onglet du fichier Excel.
#
# Usage: csv2xls.pl Resultat.xsl onglet1.csv onglet2.csv onglet2.csv
#
# reverse('©'), Octobre 2008, Ibrahima Raya TALL, irayatall@exchange.cmi.net
#

use strict;
use Spreadsheet::WriteExcel;
use File::Basename;

# Parameters check
if (($#ARGV < 1) || ($#ARGV > 4)) {
    die("Usage: csv2xls resultfile.xls onglet1.csv [onglet2.csv] [onglet3.csv]\n");
};

# Excel workbook creation
my $workbook  = Spreadsheet::WriteExcel->new($ARGV[0]);

# First Excel Sheet creation 
my($filename, $directories, $suffix) = fileparse($ARGV[1], qr/\.[^.]*/);
csv2Excel($ARGV[1], $workbook, $filename);

# Second Excel sheet creation
if ($#ARGV >= 2){
  my($filename, $directories, $suffix) = fileparse($ARGV[2], qr/\.[^.]*/);
  csv2Excel($ARGV[2], $workbook, $filename);
}

# Third Excel sheet creation
if ($#ARGV >= 3){
  my($filename, $directories, $suffix) = fileparse($ARGV[3], qr/\.[^.]*/);
  csv2Excel($ARGV[3], $workbook, $filename);
}

# TODO if needed : ....th Excel sheet creation


sub csv2Excel {
  my $file       = shift;
  my $workbook   = shift;
  my $sheetname  = shift;

  #my @noms = split('\\', $file);
  #$file =  $noms[$#noms];

  my $worksheet   = $workbook->addworksheet($sheetname);

  # Open the tab delimited file
  open (TABFILE, $file) or die "$file: $!";

  # Row and column are one indexed
  my $row = 1;

  while (<TABFILE>) {
      chomp;
      # Split on single tab
      my @Fld = split('\t', $_);
      
      foreach my $token (@Fld) {
          my @csv_line = split(/;/, $token);
          $worksheet->write_row("A".$row, \@csv_line);
      }
      $row++;
  }
  
  close(TABFILE);
}
