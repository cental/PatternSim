# This is a vim regexp which let you convert pairs.csv to CSV file in the format "target;relatum;extraction-sum"
:%s/^\([^;]\+\);\([^;]\+\);[^;]\+;[^;]\+;[^;]\+;[^;]\+;[^;]\+;\([^;]\+\);.*/\1;\2;\3/gc
