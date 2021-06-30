my $count = 0;
while (<>) {
	$count ++;
}
print STDOUT "$count line(s).\n";
exit $count;