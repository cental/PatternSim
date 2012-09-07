PatternSim
==========

A tool for calculation semantic similarity between words from a text corpus based on lexico-syntactic patterns.

patternsim.pl
----

patternsim-rank 
---------------

Reranking semantic similarity scores between words extracted with the patternsim.pl. 

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

 *i, input*

  Required. A CSV file in the format 'target;relatum;type;sim', where 'target' and 'relatum'are words, 'type' is the type of their semantic relations ('?' by default) and 'sim' is the number of extractions between 'target' and 'relatum' by the PatternSim.This file must contain symmetric relations between words (generated by PatternSim by default).If there exist a relations 'target;relatum;type;sim' then there exist one and only one relation 'relatum;target;type;sim' in the same file.

*o, output* 

Required. A CSV 'target;relatum;type;sim' in the same format as input, where 'sim' is a reranked similarity score between 'target' and 'relatum'. The goal of reranking is to improve the initial similarity score 'sim'. 
															    
*t, type* 

Required. Type of reranking: 												
1. Efreq, no reranking, transform scores to the interval [0;1].
2. Efreq-Rfreq, reranking by frequency of relations to other words. Uses option 'alpha'. 
3. Efreq-Rnum, reranking by number of relations to other words. Uses option 'beta'.
4. Efreq-Cfreq, reranking by word frequency. Uses option 'corpusfreq'.
5. Efreq-Rnum-Cfreq, reranking by number of relations to other words and by word frequency.  Uses options 'beta' and 'corpusfreq'.
6. Efreq-Rnum-Cfreq-Pnum, reranking by number of relations to other words, by word frequency and by number of different  patterns extracted the relations. Uses options 'corpusfreq', 'patterns', 'beta' and 'sqrt'.

*c, corpusfreq*

Required. A CSV file 'word;freq' with frequencies of words. 

*a, alpha*

Expected number of relations per word, default -- 15.

*b, beta*

Minimum number of extractions which establish a relation between words, default -- 2.

*s, sqrt*

Sqrt of the number of different patterns, default -- true.

*p, patterns*

A CSV file 'target;relatum;sim;p1;...;p18', where pi is present if the relation "target;relatum" was extracted at least once with the the i-th pattern. Thus, pi is a flag. 
