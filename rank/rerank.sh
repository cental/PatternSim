#!/bin/bash

# Process the parameters
if [ $# -lt 1 ] ; then
	echo "This script ranks pairs according to the Efreq-Rnum-Cfreq-Pnum reranking formula."
	echo "Usage: rerank.sh <data-dir>"
	echo "<data-dir> Should contain 'pairs.csv' and 'corpus-freq.csv'. The result will be saved into the <data-dir>."
	exit
fi

DATA=$1
PAIRS=$DATA/pairs.csv
FREQ=$DATA/corpus-freq.csv
LOG=$DATA/log.txt
OUTPUT=$DATA/pairs-efreq-rnum-cfreq-pnum.csv RERANKING_TYPE=6 # Efreq-Rnum-Cfreq-Pnum reranking formula 

# Rerank the relations
if [ -f $PAIRS  ] && [ -f $FREQ ]; then
	echo "Reranking semanting relations..."
	./bin/patternsim-rank -p $PAIRS -c $FREQ -o $OUTPUT -t $RERANKING_TYPE > $LOG
	echo "Done! See $LOG for more information."
else
	echo "The directory '$DATA' should contain files '$PAIRS' and '$FREQ'."
fi
