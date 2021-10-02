#!/usr/bin/env sh

sed -n '/^First/p' rust_out.txt | sort > rust_firsts.txt
sed -n '/^First/p' cs_out.txt | sort > cs_firsts.txt

# manual processing, easy using kak
# sort the individual symbols on each line
# select each set of symbols, then:
# 	sed -e 's/ $//' -e 's/ /\n/g' | sort | tr '\n' ' '
# run sort on files aftewards, something quirky, they're not sorting the same way
# vimdiff *edited.txt
