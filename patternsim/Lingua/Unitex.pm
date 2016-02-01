package Lingua::Unitex;
#======================================================================
#    NAME: Lingua::Unitex
#======================================================================
#  AUTHOR: Hubert Naets <hubert.naets@uclouvain.be>
#======================================================================
use strict;
use warnings;
use Moose;
use IPC::Run qw( run timeout);
use Carp;
use Log::Message::Simple qw[msg error debug carp croak cluck confess];
local $Log::Message::Simple::DEBUG_FH = \*STDERR;
use Data::Dumper;

use Lingua::Unitex::Corpus;

has 'unitex_dir' => ( is => 'ro', isa => 'Str' )
  ;    # TODO: contrôler que le Unitex_dir existe et est valable
has 'language' => ( is => 'ro', isa => 'Str' );
has 'os' => ( is => 'ro', isa => 'Str', builder => '_get_os' );
has 'user_dir' => (
    is      => 'ro',
    isa     => 'Str',
    lazy    => 1,
    builder => "_get_default_user_dir"
);
has 'default_dictionaries' => (
    is      => 'ro',
    isa     => 'ArrayRef[Str]',
    lazy    => 1,
    builder => "_get_default_dictionaries"
);
has 'toollogger_exe' => (
    is      => 'ro',
    isa     => 'Str',
    lazy    => 1,
    builder => "_get_unitex_toollogger"
);

has 'normalization_file' => (
    is      => 'ro',
    isa     => 'Str',
    lazy    => 1,
    builder => "_get_normalization_file"
);

has 'sentence_file' => (
    is      => 'ro',
    isa     => 'Str',
    lazy    => 1,
    builder => "_get_sentence_file"
);

has 'replace_file' => (
    is      => 'ro',
    isa     => 'Str',
    lazy    => 1,
    builder => "_get_replace_file"
);

has 'alphabet_file' => (
    is      => 'ro',
    isa     => 'Str',
    lazy    => 1,
    builder => "_get_alphabet_file"
);

has 'alphabet_sort_file' => (
    is      => 'ro',
    isa     => 'Str',
    lazy    => 1,
    builder => "_get_alphabet_sort_file"
);

has 'verbose_log' => ( is => 'rw', isa => 'Bool', default => 0 );
has 'debug_log'   => ( is => 'rw', isa => 'Bool', default => 0 );
has 'error_log'   => ( is => 'rw', isa => 'Bool', default => 1 );

#----------------------------------------------------------------------
#----------------------------------------------------------------------
sub corpus {
    my $self = shift;
    my @args = @_;

    return Lingua::Unitex::Corpus->new( @args, unitex => $self );

}

#----------------------------------------------------------------------
#----------------------------------------------------------------------
sub _toollogger {
    my $self    = shift;
    my @args    = @_;
    my $command = [ $self->toollogger_exe, @args ];

    my ( $out, $err );
    debug( "EXECUTE: " . join( " ", @$command ), $self->debug_log );
    eval { 
      run( $command, \undef, \$out, \$err, timeout(900) );
    };
    if ($@) {
	    #return( 666, '', 'IPC::Run timeout' );
    }
#    run3( $command, \undef, \undef, \undef, { return_if_system_error => 1 } );
#     system(@$command);

    my $error_code = $?;
    debug( "ERROR CODE:$error_code", $self->debug_log );
    debug( "STDOUT: $out",           $self->debug_log );
    debug( "STDERR: $err",           $self->debug_log );
    debug( '-' x 72,                 $self->debug_log );
    if ( $? != 0 ) {
        my $err_msg =
          "Command '" . join( " ", @$command ) . "' returns the error code $?";
        error( $err_msg, $self->error_log );
        return;
    }
    return ( $error_code, $out, $err );

}

#----------------------------------------------------------------------
#----------------------------------------------------------------------
sub _get_normalization_file {
    my $self   = shift;
    my $result = undef;

    my $norm_file = $self->user_dir . '/' . $self->language . '/' . 'Norm.txt';
    if ( not -e $norm_file ) {
        carp "Impossible to get the Norm.txt file";
        return;
    }
    $result = $norm_file;
    return $result;
}

#----------------------------------------------------------------------
#----------------------------------------------------------------------
sub _get_alphabet_file {
    my $self   = shift;
    my $result = undef;

    my $alphabet_file =
      $self->user_dir . '/' . $self->language . '/' . 'Alphabet.txt';
    if ( not -e $alphabet_file ) {
        carp "Impossible to get the Alphabet.txt file";
        return;
    }
    $result = $alphabet_file;
    return $result;
}

#----------------------------------------------------------------------
#----------------------------------------------------------------------
sub _get_alphabet_sort_file {
    my $self   = shift;
    my $result = undef;

    my $alphabet_file =
      $self->user_dir . '/' . $self->language . '/' . 'Alphabet_sort.txt';
    if ( not -e $alphabet_file ) {
        carp "Impossible to get the Alphabet_sort.txt file";
        return;
    }
    $result = $alphabet_file;
    return $result;
}

#----------------------------------------------------------------------
#----------------------------------------------------------------------
sub _get_sentence_file {
    my $self   = shift;
    my $result = undef;

    my $sentence_file =
        $self->user_dir . '/'
      . $self->language . '/'
      . 'Graphs/Preprocessing/Sentence/Sentence.grf';
    if ( not -e $sentence_file ) {
        carp "Impossible to get the Sentence.grf file";
        return;
    }
    $result = $sentence_file;
    return $result;
}

#----------------------------------------------------------------------
#----------------------------------------------------------------------
sub _get_replace_file {
    my $self   = shift;
    my $result = undef;

    my $replace_file =
        $self->user_dir . '/'
      . $self->language . '/'
      . 'Graphs/Preprocessing/Replace/Replace.grf';
    if ( not -e $replace_file ) {
        carp "Impossible to get the Replace.grf file";
        return;
    }
    $result = $replace_file;
    return $result;
}

#----------------------------------------------------------------------
#----------------------------------------------------------------------
sub _get_default_user_dir {

    # TODO: to complete. Look for the user dir; if not found use the unitex dir
    my $self   = shift;
    my $result = undef;
    if ( not defined $self->unitex_dir ) {
        carp "Impossible to get the unitex path";
        return;
    }
    $result = $self->unitex_dir;

    return $result;
}

#----------------------------------------------------------------------
#----------------------------------------------------------------------
sub _get_default_dictionaries {
    my $self   = shift;
    my $result = [];
    if ( not defined $self->user_dir ) {
        carp "Impossible to fetch the default dictionaries names : "
          . "no 'user_dir' defined";
        return $result;
    }
    if ( not defined $self->language ) {
        carp "Impossible to fetch the default dictionaries names : "
          . "no 'language' defined";
        return $result;
    }
    my $language_dir = $self->user_dir . '/' . $self->language;
    my $system_dic   = $language_dir . '/' . 'system_dic.def';
    if ( not -e $system_dic ) {
        carp "Impossible to fetch the default dictionaries names : "
          . "'$system_dic' not found";
        return $result;
    }

    my $open_ok = open my $system_dic_fh, '<', $system_dic;
    if ( not $open_ok ) {
        carp "Impossible to fetch the default dictionaries names : "
          . "impossible to open '$system_dic' file";
        return $result;

    }
    my @dictionaries = <$system_dic_fh>;
    map { $_ =~ s/^(.+)[\r\n]*$/$language_dir\/Dela\/$1/ } @dictionaries;
    close $system_dic_fh;

    $result = \@dictionaries;
    return $result;

}

#----------------------------------------------------------------------
#----------------------------------------------------------------------
sub _get_unitex_toollogger {
    my $self = shift;

    my $unitex_exe = "UnitexToolLogger";
    $unitex_exe = "UnitexToolLogger.exe" if $self->os eq "Win";
    my $unitex_path = $self->unitex_dir . '/App/' . $unitex_exe;
    if ( not -e $unitex_path ) {
        my $err_msg = "Impossible to find the $unitex_exe program in "
          . $self->unitex_dir . '/App/';
        error( $err_msg, $self->error_log );
        return;
    }

    # test if UnitexToolLogger returns a 0 value
    my @command = ($unitex_path);
    my ($in, $out, $err);
    eval {
      run( \@command, \$in, \$out, \$err );
    };
    if ($@) {
    #if ( $? != 0 ) {
        my $err_msg = "$unitex_path returns the error code $?";
        error( $err_msg, $self->error_log );
        return;
    }
    debug( "UnitexToolLogger validation: PASS", $self->debug_log );
    return $unitex_path;
}

#----------------------------------------------------------------------
#----------------------------------------------------------------------
sub _get_os {
    my %os = (
        MacOS   => 'Unix',
        MSWin32 => 'Win',
    );
    my $os = $os{$^O} || 'Unix';
    return $os;
}

__PACKAGE__->meta->make_immutable;

1;
