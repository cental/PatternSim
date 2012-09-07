#!/bin/bash

# Test data
data=./data
input=$data/raw-wacky.csv
#input=$data/raw-s.csv-5m
output=$data/output
concfreq=$data/conc-all-freq.csv
pairsfreq=$data/pairs-all-freq.csv
corpusfreq=$data/corpus-all-freq.csv
patterns=$data/patterns.csv

for type in 1 2 3 4 5 6
do

echo $type

# Full keys 
#./bin/patternsim-rank.exe --input $input --output $output/$type.csv --type $type --corpusfreq $concfreq --alpha 20 --beta 2 --sqrt 1 --patterns $patterns

# Short keys  
#./bin/patternsim-rank.exe -i $input -o $output/$type.csv -t $type -c $corpusfreq -a 20 -b 2 -s 1 -p $patterns

# Short keys and a Linux shortcut 
./bin/patternsim-rank -i $input -o $output/$type.csv -t $type -c $corpusfreq -a 20 -b 2 -s 1 -p $patterns

done
