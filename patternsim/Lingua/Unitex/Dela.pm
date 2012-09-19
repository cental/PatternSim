package Lingua::Unitex::Dela;
#======================================================================
#    NAME: Lingua::Unitex::Dela
#======================================================================
#  AUTHOR: Hubert Naets <hubert.naets@uclouvain.be>
#======================================================================
use Moose;
use strict;
use warnings;
use utf8;
use Carp;
use Data::Dumper;
use IO::File;

has 'index_form'  => (is => 'ro', isa => 'HashRef[HashRef[HashRef]]');
has 'index_lemma' => (is => 'ro', isa => 'HashRef[HashRef[HashRef]]');
has 'index_infos' => (is => 'ro', isa => 'HashRef[HashRef[HashRef]]');

#----------------------------------------------------------------------
# ROUTINE add_file
# Add a new file in DELA format
# The dictionary's entries of this file are added to the current entries
#----------------------------------------------------------------------
sub add_file {
  my $self     = shift;
  my $filename = shift;
  
  my $fh = IO::File->new($filename,'r');
  $fh->binmode(":encoding(utf16le)");
  
  my $line_nb = 0;
  while ( my $line = $fh->getline ) { 
    $line_nb++;
    $line =~ s/\x{feff}// if $line_nb == 1;
    
    my $data_line = &_parse_dela_line($line);
    my $form      = $data_line->{form};
    my $lemma     = $data_line->{lemma};
    my $infos     = $data_line->{infos};
    
    $self->{'index_form'}->{$form}->{$lemma}->{$infos}++;
    $self->{'index_lemma'}->{$lemma}->{$form}->{$infos}++;
    $self->{'index_infos'}->{$infos}->{$lemma}->{$form}++;
  }
  $fh->close();
}

#----------------------------------------------------------------------
# ROUTINE merge_with
# Merge the current DELA dictionary with another dictionary
#----------------------------------------------------------------------
sub merge_with {
  my $self = shift;
  my $dela = shift;
  
  my $dela_forms = $dela->index_form();
  foreach my $form (%$dela_forms) {
    foreach my $lemma (%{$dela_forms->{$form}}) {
      foreach my $infos (%{$dela_forms->{$form}->{$lemma}}) {
        $self->{'index_form'}->{$form}->{$lemma}->{$infos}++;
        $self->{'index_lemma'}->{$lemma}->{$form}->{$infos}++;
        $self->{'index_infos'}->{$infos}->{$lemma}->{$form}++;
      }
    }
  }
  undef $dela_forms;
}

#----------------------------------------------------------------------
# ROUTINE search
# Performs a strict search in the DELA entries,
# using as criteria form, lemma and/or syntactical informations
# e.g. $dela->search(form => 'house');
#      $dela->search(lemma => 'house');
# A 'return' parameter allows to return
# only the form, the lemma or the syntactical informations
#----------------------------------------------------------------------
sub search {
  my $self    = shift;
  my @request = @_;
  my %req = ( form => undef, lemma => undef, infos => undef, return => undef, @request);
  my @results = ();
  
  if ( defined $req{'form'}) {
    if (exists $self->{'index_form'}->{$req{'form'}}) {
      foreach my $lemma (keys %{$self->{'index_form'}->{$req{'form'}}}) {
        next if defined $req{'lemma'} and $lemma ne $req{'lemma'};
        my @infos = keys %{$self->{'index_form'}->{$req{'form'}}->{$lemma}};
        foreach my $infos (@infos) {
         next if defined $req{'infos'} and $infos ne $req{'infos'};
          push @results, {form => $req{'form'}, lemma => $lemma, infos => $infos};
        }
      }
    }
  }
  elsif ( defined $req{'lemma'}) {
    if (exists $self->{'index_lemma'}->{$req{'lemma'}}) {
      foreach my $form (keys %{$self->{'index_lemma'}->{$req{'lemma'}}}) {
        my @infos = keys %{$self->{'index_lemma'}->{$req{'lemma'}}->{$form}};
        foreach my $infos (@infos) {
          next if defined $req{'infos'} and $infos ne $req{'infos'};
          push @results, {form => $form, lemma => $req{'lemma'}, infos => $infos};
        }
      }
    }
  }
  elsif ( defined $req{'infos'}) {
    if (exists $self->{'index_infos'}->{$req{'infos'}}) {
      foreach my $lemma (keys %{$self->{'index_infos'}->{$req{'infos'}}}) {
        my @forms = keys %{$self->{'index_infos'}->{$req{'infos'}}->{$lemma}};
        foreach my $form (@forms) {
          push @results, {form => $form, lemma => $lemma, infos => $req{'infos'}};
        }
      }
    }
  }
  if (defined $req{'return'}) {
    my %tmp = map { $_->{$req{'return'}} => 1 } @results;
    @results = sort keys %tmp;
  }
  return @results;
}

#----------------------------------------------------------------------
# PRIVATE ROUTINE _parse_dela_line
# Parse a DELA entry and return a hashtable
# with form, lemma and syntactical informations extracted from
# the input entry
#----------------------------------------------------------------------
sub _parse_dela_line {
    my $line   = shift;
    my $result = {};

    $line =~ s/[\r\n]+$//;
    ( $result->{form}, $result->{lemma}, $result->{infos} ) =
      $line =~ /^(.+),(.*)\.(.+)$/;

    $result->{lemma} = $result->{form} if $result->{lemma} eq '';
    $result->{infos} = &_reorder_infos($result->{infos});
    $result->{line}  = $line;
    return $result;
}

#----------------------------------------------------------------------
# PRIVATE ROUTINE _reorder_infos
# Reorder syntactical informations
#----------------------------------------------------------------------
sub _reorder_infos {
  my $infos = shift;
  my @parts = split(/:/,$infos);
  return join (':', sort(@parts));
}

1;