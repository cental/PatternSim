package Lingua::Unitex::Corpus;

#======================================================================
#    NAME: Lingua::Unitex::Corpus
#======================================================================
#  AUTHOR: Hubert Naets <hubert.naets@uclouvain.be>
#======================================================================
use Moose;
use Carp;
use IO::File;
use File::Basename;
use Data::Dumper;
use Lingua::Unitex::Dela;
use utf8;

has filename => ( is => 'ro', isa => 'Str', trigger => \&_create_snt );
has snt_file => ( is => 'ro', isa => 'Str' );
has snt_dir  => ( is => 'ro', isa => 'Str' );
has unitex => ( is => 'ro', isa => 'Lingua::Unitex' );

#----------------------------------------------------------------------
#----------------------------------------------------------------------
sub normalize {
    my $self      = shift;
    my $norm_file = shift;
    $norm_file = $self->unitex->normalization_file if not defined $norm_file;
    my @command = ( 'Normalize', $self->filename(), '-r' . $norm_file );
    my ( $error_code, $out, $err ) = $self->unitex->_toollogger(@command);
    if ($error_code) {
        carp 'Error: '
          . join( " ", @command )
          . " returns error code "
          . $error_code
          . " ($err)";
        return;
    }
}

#----------------------------------------------------------------------
#----------------------------------------------------------------------
sub apply_graph_in_merge_mode {
    my $self          = shift;
    my $sentence_file = shift;
    my $alphabet_file = shift;

    $sentence_file = $self->unitex->sentence_file if not defined $sentence_file;
    $alphabet_file = $self->unitex->alphabet_file if not defined $alphabet_file;
    ( my $sentence_fst2 = $sentence_file ) =~ s/\.grf$/.fst2/;

    my @command = ();
    my ( $error_code, $out, $err );

    if ( not -e $sentence_fst2 ) {

        @command =
          ( 'Grf2Fst2', $sentence_file, '-y', '--alphabet=' . $alphabet_file );
        ( $error_code, $out, $err ) = $self->unitex->_toollogger(@command);
        if ($error_code) {
            carp 'Error: '
              . join( " ", @command )
              . " returns error code "
              . $error_code
              . " ($err)";
            return;
        }
        @command = ( 'Flatten', $sentence_fst2, '--rtn', '-d5' );
        ( $error_code, $out, $err ) = $self->unitex->_toollogger(@command);
        if ($error_code) {
            carp 'Error: '
              . join( " ", @command )
              . " returns error code "
              . $error_code
              . " ($err)";
            return;
        }

    }

    if ( not -e $sentence_fst2 ) {
        carp "Error: file $sentence_fst2 does not exist";
        return;
    }
    #
    #
    #~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    @command = (
        'Fst2Txt', '-t' . $self->snt_file,
        $sentence_fst2, '-a' . $alphabet_file, '-M'
    );
    ( $error_code, $out, $err ) = $self->unitex->_toollogger(@command);
    if ($error_code) {
        carp 'Error: '
          . join( " ", @command )
          . " returns error code "
          . $error_code
          . " ($err)";
        return;
    }
    return 1;
}

#----------------------------------------------------------------------
#----------------------------------------------------------------------
sub apply_graph_in_replace_mode {
    my $self          = shift;
    my $replace_file  = shift;
    my $alphabet_file = shift;

    $replace_file  = $self->unitex->replace_file  if not defined $replace_file;
    $alphabet_file = $self->unitex->alphabet_file if not defined $alphabet_file;
    ( my $replace_fst2 = $replace_file ) =~ s/\.grf$/.fst2/;

    my @command = ();
    my ( $error_code, $out, $err );

    if ( not -e $replace_fst2 ) {
        @command =
          ( 'Grf2Fst2', $replace_file, '-y', '--alphabet=' . $alphabet_file );
        ( $error_code, $out, $err ) = $self->unitex->_toollogger(@command);
        if ($error_code) {
            carp 'Error: '
              . join( " ", @command )
              . " returns error code "
              . $error_code
              . " ($err)";
            return;
        }
    }
    #
    #~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    if ( not -e $replace_fst2 ) {
        carp "Error: file $replace_fst2 does not exist";
        return;
    }

    @command = (
        'Fst2Txt', '-t' . $self->snt_file,
        $replace_fst2, '-a' . $alphabet_file, '-R'
    );
    ( $error_code, $out, $err ) = $self->unitex->_toollogger(@command);
    if ($error_code) {
        carp 'Error: '
          . join( " ", @command )
          . " returns error code "
          . $error_code
          . " ($err)";
        return;
    }
    return 1;
}

#----------------------------------------------------------------------
#----------------------------------------------------------------------
sub tokenize {
    my $self          = shift;
    my $alphabet_file = shift;
    $alphabet_file = $self->unitex->alphabet_file if not defined $alphabet_file;

    my @command = ( 'Tokenize', $self->snt_file, '-a' . $alphabet_file );
    my ( $error_code, $out, $err ) = $self->unitex->_toollogger(@command);
    if ($error_code) {
        carp 'Error: '
          . join( " ", @command )
          . " returns error code "
          . $error_code
          . " ($err)";
        return;
    }
    return 1;
}

#----------------------------------------------------------------------
#----------------------------------------------------------------------
sub apply_lexical_ressources {
    my $self               = shift;
    my $lexical_ressources = shift;
    my $alphabet_file      = shift;
    my $alphabet_sort_file = shift;

    $lexical_ressources = $self->unitex->default_dictionaries
      if not defined $lexical_ressources;
    $alphabet_file = $self->unitex->alphabet_file if not defined $alphabet_file;
    $alphabet_sort_file = $self->unitex->alphabet_sort_file
      if not defined $alphabet_sort_file;

    my @command = (
        'Dico',
        '-t' . $self->snt_file,
        '-a' . $alphabet_file,
        @$lexical_ressources
    );
    my ( $error_code, $out, $err ) = $self->unitex->_toollogger(@command);
    if ($error_code) {
        carp 'Error: '
          . join( " ", @command )
          . " returns error code "
          . $error_code
          . " ($err)";
        return;
    }

    @command = (
        'SortTxt',
        $self->snt_dir . 'dlf',
        '-l' . $self->snt_dir . 'dlf.n',
        '-o' . $alphabet_sort_file
    );
    ( $error_code, $out, $err ) = $self->unitex->_toollogger(@command);
    if ($error_code) {
        carp 'Error: '
          . join( " ", @command )
          . " returns error code "
          . $error_code
          . " ($err)";
        return;
    }

    @command = (
        'SortTxt',
        $self->snt_dir . 'dlc',
        '-l' . $self->snt_dir . 'dlc.n',
        '-o' . $alphabet_sort_file
    );
    ( $error_code, $out, $err ) = $self->unitex->_toollogger(@command);
    if ($error_code) {
        carp 'Error: '
          . join( " ", @command )
          . " returns error code "
          . $error_code
          . " ($err)";
        return;
    }

    @command = (
        'SortTxt',
        $self->snt_dir . 'err',
        '-l' . $self->snt_dir . 'err.n',
        '-o' . $alphabet_sort_file
    );
    ( $error_code, $out, $err ) = $self->unitex->_toollogger(@command);
    if ($error_code) {
        carp 'Error: '
          . join( " ", @command )
          . " returns error code "
          . $error_code
          . " ($err)";
        return;
    }

    @command = (
        'SortTxt',
        $self->snt_dir . 'tags_err',
        '-l' . $self->snt_dir . 'tags_err.n',
        '-o' . $alphabet_sort_file
    );
    ( $error_code, $out, $err ) = $self->unitex->_toollogger(@command);
    if ($error_code) {
        carp 'Error: '
          . join( " ", @command )
          . " returns error code "
          . $error_code
          . " ($err)";
        return;
    }
    return 1;
}

#----------------------------------------------------------------------
#----------------------------------------------------------------------
sub apply_graph {

# TODO: graph compilation must be done in another module (Lingua::Unitex::Graph)
#       only in case of .grf use
    my $self               = shift;
    my $graph_file         = shift;
    my $alphabet_file      = shift;
    my $alphabet_sort_file = shift;

    $alphabet_file = $self->unitex->alphabet_file if not defined $alphabet_file;
    $alphabet_sort_file = $self->unitex->alphabet_sort_file
      if not defined $alphabet_sort_file;
    if ( not defined $graph_file ) {
        carp 'Error: graph file must be defined';
        return undef;
    }
    if ( not -e $graph_file ) {
        carp 'Error: file $graph_file does not exist';
        return undef;
    }

    ( my $extension ) = $graph_file =~ /^.+\.(.+?)$/;

    my $graph_fst2 = undef;
    if ( $extension eq "grf" ) {
        $graph_fst2 = $self->_grf2fst2( $graph_file, $alphabet_file );
    }
    elsif ( $extension eq "fst2" ) {
        $graph_fst2 = $graph_file;
    }

    #    my @command =
    #      ( 'Grf2Fst2', $graph_file, '-y', '--alphabet=' . $alphabet_file );
    #    my ( $error_code, $out, $err ) = $self->unitex->_toollogger(@command);
    #    if ($error_code) {
    #        carp 'Error: '
    #          . join( " ", @command )
    #          . " returns error code "
    #          . $error_code
    #          . " ($err)";
    #        return;
    #    }
    #
    #    ( my $graph_fst2 = $graph_file ) =~ s/\.grf$/.fst2/;
    #    if ( not -e $graph_fst2 ) {
    #        carp "Error: file $graph_fst2 does not exist";
    #        return;
    #    }
    #
    my @command = (
        'Locate',    '-t' . $self->snt_file,
        $graph_fst2, '-a' . $alphabet_file,
        '-L',        '-M',
        '--all',     '-z',
        '-Y'
    );
    my ( $error_code, $out, $err ) = $self->unitex->_toollogger(@command);
    if ($error_code) {
        carp 'Error: '
          . join( " ", @command )
          . " returns error code "
          . $error_code
          . " ($err)";
        return undef;
    }
}

sub merge_concord {
    my $self = shift;
    my $filename = shift;    # TODO: if no filename is provided, generate a filename;

    my $result = undef;

    my @command =
      ( 'Concord', $self->snt_dir . 'concord.ind', '-m', $filename );
    my ( $error_code, $out, $err ) = $self->unitex->_toollogger(@command);
    if ($error_code) {
        carp 'Error: '
          . join( " ", @command )
          . " returns error code "
          . $error_code
          . " ($err)";
        return undef;
    }
    $result = Lingua::Unitex::Corpus->new(
        unitex   => $self->{unitex},
        filename => $filename
    );
    return $result;
}

#----------------------------------------------------------------------
#----------------------------------------------------------------------
sub get_last_concord {
    my $self = shift;

    my $alphabet_file      = $self->unitex->alphabet_file;
    my $alphabet_sort_file = $self->unitex->alphabet_sort_file;

    my @command = (
        'Concord', $self->snt_dir . 'concord.ind',
        '-f',      'Courier new',
        '-s',      '12',
        '-l',      '40',
        '-r',      '55',
        '--html',  '-a' . $alphabet_sort_file,
        '--CL'
    );
    my ( $error_code, $out, $err ) = $self->unitex->_toollogger(@command);
    if ($error_code) {
        carp 'Error: '
          . join( " ", @command )
          . " returns error code "
          . $error_code
          . " ($err)";
        return;
    }

    my $format = 'ind';    #TODO: ind or html

    my @result = undef;

    my $concord_file = $self->snt_dir . 'concord.ind';
    if ( -e $concord_file ) {
        my $fh = IO::File->new( $concord_file, 'r' );
        binmode( $fh, ':encoding(utf16le)' );
        <$fh>;             # first line not taken in account
        @result = <$fh>;
        map { $_ =~ s/\015\012$//; } @result;    # remove <CR><LF>
        $fh->close;
    }
    return @result;
}

#----------------------------------------------------------------------
#----------------------------------------------------------------------
sub get_dictionary {
    my $self = shift;
    my $dela = Lingua::Unitex::Dela->new();

    my $dlf_file = $self->snt_dir . 'dlf';
    my $dlc_file = $self->snt_dir . 'dlc';

    $dela->add_file($dlf_file) if -e $dlf_file;
    $dela->add_file($dlc_file) if -e $dlc_file;

    return $dela;
}

#----------------------------------------------------------------------
#----------------------------------------------------------------------
sub get_tokens_by_frequency {
    my $self   = shift;
    my $result = undef;

    my $tok_by_freq_file = $self->snt_dir . 'tok_by_freq.txt';
    if ( -e $tok_by_freq_file ) {
        my $fh = IO::File->new( $tok_by_freq_file, 'r' );
        binmode( $fh, ':encoding(utf16le)' );
        while ( my $line = <$fh> ) {
            $line =~ s/\x{feff}//;
            $line =~ s/\s+$//;
            my ( $freq, $tok ) = split( /\t+/, $line );
            next if not defined $freq or not defined $tok;
            $result->{$tok} = $freq;
        }
        $fh->close;
    }
    return $result;
}

#----------------------------------------------------------------------
#----------------------------------------------------------------------
sub delete_snt_files {
    my $self = shift;
    unlink glob $self->snt_dir . '*';
    rmdir $self->snt_dir;
    unlink $self->snt_file;
}

sub delete_all_files {
    my $self = shift;
    $self->delete_snt_files;
    unlink $self->filename;
}

#----------------------------------------------------------------------
#----------------------------------------------------------------------
sub _create_snt {
    my $self     = shift;
    my $filename = shift;
    my ( $name, $path, $suffix ) = fileparse( $filename, qr/\.[^.]*/ );
    my $snt_file = $path . $name . '.snt';
    my $snt_dir  = $path . $name . '_snt';
    mkdir $snt_dir;
    $self->{'snt_file'} = $snt_file;
    $self->{'snt_dir'}  = $snt_dir . '/';
}

#----------------------------------------------------------------------
#----------------------------------------------------------------------
sub _grf2fst2 {
    my $self          = shift;
    my $graph_file    = shift;
    my $alphabet_file = shift;

    my $result = undef;

    my @command =
      ( 'Grf2Fst2', $graph_file, '-y', '--alphabet=' . $alphabet_file );
    my ( $error_code, $out, $err ) = $self->unitex->_toollogger(@command);
    if ($error_code) {
        carp 'Error: '
          . join( " ", @command )
          . " returns error code "
          . $error_code
          . " ($err)";
        return;
    }

    ( my $graph_fst2 = $graph_file ) =~ s/\.grf$/.fst2/;
    if ( not -e $graph_fst2 ) {
        carp "Error: file $graph_fst2 does not exist";
        return;
    }
    $result = 1;
    return $graph_fst2;
}

#----------------------------------------------------------------------
#----------------------------------------------------------------------
sub DEMOLISH {
    my $self = shift;
    $self->{'unitex'} = undef;
}
__PACKAGE__->meta->make_immutable;

1;
