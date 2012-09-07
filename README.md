PatternSim
==========

A tool for calculation semantic similarity between words from a text corpus based on lexico-syntactic patterns.


patternsim-rank
---------------

**System Requirements**

- Windows -- Microsoft .NET framework 4.0 or higher (http://www.microsoft.com/net). 
- Linux or Mac OSX -- Mono 2.0 or higher (http://www.go-mono.com/mono-downloads/). For instance, for Ubuntu 12.04 use "sudo apt-get install mono-runtime". 
- At least 4Gb of RAM is recommended.

**Binaries**

Binary is readily available the bin folder. On Unix based systems you may use  "./patternsim-rank" or "./patternsim-rank.exe". On Windows, use "patternsim-rank.exe".

**Testing**
1. Download test data http://cental.fltr.ucl.ac.be/team/~panchenko/sim-eval/patternsim-rank-data.tgz.
2. Save the archive to the "rank" directory.
3. Extract the data (tar xzf patternsim-rank-data.tgz). The directory "data" should appear. 
4. Run tests.sh script. It will produce the output in the data/output folder. 

**Compilation**

1. Open patternsim-rank.sln with MonoDevelop or Visual Studio. 
2. Build the solution. 
