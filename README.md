PatternSim
==========

A tool for calculation semantic similarity between words from a text corpus based on lexico-syntactic patterns.

- Currently, the tool consist of two separate programs -- *patternsim* and *patternsim-rank* (see below). 
- This tool implements the extraction method described in these papers: 
	- Panchenko A., Morozova O., Naets H. “A Semantic Similarity Measure Based on Lexico-Syntactic Patterns.” In Proceedings of the 11th Conference on Natural Language Processing (KONVENS 2012), — Vienna (Austria), 2012
	- http://www.oegai.at/konvens2012/proceedings/23_panchenko12p/
	- Kristina Sabirova, Artem Lukanin. Automatic Extraction of Hypernyms and Hyponyms from Russian Texts // Supplementary Proceedings of the 3rd International Conference on Analysis of Images, Social Networks and Texts (AIST 2014) / Ed. by D. I. Ignatov, M. Y. Khachay, A. Panchenko, N. Konstantinova, R. Yavorsky, D. Ustalov. Vol. 1197: Supplementary Proceedings of AIST 2014. CEUR-WS.org, 2014. С. 35-40.
	- http://ceur-ws.org/Vol-1197/paper6.pdf
- A demo of the extraction results provided with this method can be accessed here: http://serelex.cental.be/
- Related repositories: 
	- Source code of the demo system: https://github.com/PomanoB/lsse
	- An evaluation framework for semantic similarity measures: https://github.com/alexanderpanchenko/sim-eval

License 
-------

LGPLv3: http://www.gnu.de/documents/lgpl-3.0.en.html

patternsim
----

A tool for extraction of raw extraction counts with lexico-syntactic patterns. 

**Requirements**
- Perl 5.14.x
- Unitex 3.0beta (http://www-igm.univ-mlv.fr/~unitex/)

**Installation on Ubuntu 12.04**

1. Install CPAN: "sudo cpan App::cpanminus"
2. Install module "sudo cpan Config::General"
3. "sudo cpan File::HomeDir"
4. sudo cpan Moose
5. sudo cpan IPC::Run3
6. sudo cpan IPC::Run
6. "sudo cpan Parallel::ForkManager"
6. Install Unitex 3.0beta (http://www-igm.univ-mlv.fr/~unitex/zips/Unitex3.0beta.zip)

**Quick Start**

Use *./rerank.sh* to rerank relations with the default formula, and as an example of usage of patternsim-rank.

**Synopsis**

patternsim [options] [corpus_file(s) ...]

**Options**

Usage:
    patternsim [options] [corpus_file(s) ...]

      Options:
        --vocabulary (-v)        input vocabulary file
        --output (-o)            output directory
        --unitex                 Unitex main directory

        --verbose                verbose mode
        --help                   brief help message
        --man                    full documentation

Options:
    --vocabulary --vocab -v *vocabulary_file*
            Specify the UTF-8 input vocabulary file (one word per line)

    --unitex *unitex_main_directory*
            Specify the Unitex main directory if you want to use your own
            Unitex installation (overwite the patternsim configuration file)

            At first run, patternsim will ask you if you want to install the
            Unitex program automatically or if you want to specify the
            location of your Unitex main directory.

    --output -o *output_directory*
            Specify the output directory

    --verbose
            Explains what is being done

    --help -h
            Prints a brief help message and exits.

    --man   Prints the manual page and exits.

    --verbose
            Activates the verbose mode. Explains all the processes. Outputs
            will be shown on stderr

**Example**

./patternsim --unitex /home/sasha/Unitex3.0beta -v vocabulary.txt -o ./output corpus.txt

The output of this command -- a set of files in the directory "./output":
- *conc-freq.csv* -- a frequency list derived from a set of extraction concordances
- *corpus-freq.csv* -- a frequency list derived from an input corpus "corpus.txt"
- *pairs.csv* -- similarity matrix containing raw extraction counts between all single words
- *pairs-np.csv* -- similarity matrix containing raw extraction counts between all noun phrases
- *pairs-voc.csv* -- similarity matrix containing raw extraction counts between terms from the input vocabulary "vocabulary.txt"

The files *conc-freq.csv* and *corpus-freq.csv* are CSV files in the following format: 

```
word;frequency\n
```

The files *pairs.csv*, *pairs-np.csv* and *pairs-voc.csv* are CSV files in the following format:

```
target-word;relatum-word;e-syno;e-cohypo;e-hyper-hypo;e-hyper;e-hypo;e-all;e1;e2;e3;е4;е5;е6;е7;е8;е9;е10;е11;е12;е13;е14;е15;е16;е17\n
```

Here *target-word* and *related-word* are words, ' *e-all* is the number of extractions between *target-word* and *relatum-word* with all the 17 patterns, *ei* is number of extractions between *target-word* and *relatum-word* with
the *i*-th pattern (see the referenced above paper for details). Thus *e-all* = sum_*i* (*ei*).

 *e-syno*, *e-cohypo*, *e-hyper*, *e-hyper-hypo*, *e-hypo*  is the number of specific relations extracted between terms (synonyms, co-hyponyms, hypernyms, hyponym, hypernyms+hyponyms).

**Corpus**

Here are some corpora which you may use with this tool:
- Some Wikipedia articles: http://cental.fltr.ucl.ac.be/team/~panchenko/patternsim/corpus/
- For even bigger corpora use ukWaC and WaCkypedia: http://wacky.sslmit.unibo.it/doku.php?id=corpora
- Use DBPedia dump of Wikipedia: http://wiki.dbpedia.org/Downloads
- Use a corpus of your own

**Russian morphological dictionary**

The Russian dictionary in this repository is an extract of the Russian computational morphological dictionary developed at CIS, Munich. This extract contains about 15% of the original dictionary (the most frequent lemmata). The whole dictionary actually contains 140,000 simple entries (= 2.7 million distinct forms), 166,000 simple proper nouns (= 900,000 distinct forms) and 1800 compound words.

If you want to use the full version of the lexicon, please contact: 

    Sebastian Nagel
    CIS
    Oettingenstr. 67
    80538 München
    Germany
    wastl@cis.uni-muenchen.de
    http://www.cis.uni-muenchen.de

For additional information see: 

Nagel, Sebastian 2002: Formenbildung im Russischen. Formale Beschreibung und Automatisierung für das CISLEX-Wörterbuchsystem (http://www.cis.uni-muenchen.de/~wastl/pub/ruslex.pdf)

For a short description (in German), see http://www.cis.uni-muenchen.de/~wastl/pub/ruslexUnitex.pdf

rank 
---------------

Reranking semantic similarity scores between words extracted with the patternsim. Directory -- "rank".
 
**Synopsis**

patternsim-rank [*options*]

**System Requirements**

- Windows -- Microsoft .NET framework 4.0 or higher (http://www.microsoft.com/net). 
- Linux or Mac OSX -- Mono 2.0 or higher (http://www.go-mono.com/mono-downloads/). For instance, for Ubuntu 12.04 use "sudo apt-get install mono-runtime". 
- At least 4Gb of RAM is recommended.

**Binaries**

Binaries are readily available the bin folder. On Unix based systems you may use  "./patternsim-rank" or "./patternsim-rank.exe". On Windows, use "patternsim-rank.exe".

**Testing**

1. Download test data http://cental.fltr.ucl.ac.be/team/~panchenko/sim-eval/patternsim-rank-data.tgz.
2. Save the archive to the "rank" directory.
3. Extract the data (tar xzf patternsim-rank-data.tgz). The directory "data" should appear. 
4. Run tests.sh script. It will produce the output in the data/output folder. 

**Recompilation**

1. Open patternsim-rank.sln with MonoDevelop or Visual Studio. 
2. Build the solution. 

**Options**

 *p, pairs*

Required. An UTF-8 encoded CSV file in provided by the PattenSim program. In the format: 

```
target;relatum;syno;cohypo;hyper_hypo;hyper;hypo;sum;pattern;pattern2;pattern3;pattern4;pattern5;pattern6;pattern7;pattern8;pattern9;pattern10;pattern11;pattern12;pattern13;pattern14;pattern15;pattern16;pattern17
```

This file must contain symmetric relations between words (generated by the PatternSim by default). If there exist a relation 'target;relatum;type;sim' then there should exist one and only one relation 'relatum;target;type;sim' in the same file.

*o, output* 

Required. An UTF-8 encoded CSV file 'target;relatum;sim', where 'sim' is similarity score between 'target' and 'relatum'. This file is sorted by 'target' and then 'sim'. 
															    
*c, corpusfreq*

Required. An UTF-8 encoded CSV file 'word;freq' with frequencies of words.

*t, type* 

Required. Type of reranking: 												
1. Efreq, no reranking, transform scores to the interval [0;1].
2. Efreq-Rfreq, reranking by frequency of relations to other words. Uses option 'alpha'. 
3. Efreq-Rnum, reranking by number of relations to other words. Uses option 'beta'.
4. Efreq-Cfreq, reranking by word frequency. Uses option 'corpusfreq'.
5. Efreq-Rnum-Cfreq, reranking by number of relations to other words and by word frequency.  Uses options 'beta' and 'corpusfreq'.
6. Efreq-Rnum-Cfreq-Pnum, reranking by number of relations to other words, by word frequency and by number of different  patterns extracted the relations. Uses options 'corpusfreq', 'patterns', 'beta' and 'sqrt'.

*a, alpha*

Expected number of relations per word, default -- 15.

*b, beta*

Minimum number of extractions which establish a relation between words, default -- 2.

*s, sqrt*

Sqrt of the number of different patterns, default -- true.
