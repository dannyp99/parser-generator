# This program is supposed to go into an infinite loop when evaluated, but
# you should be able to compile it.

# modified from Liang's test5 to use literal "==" instead of "eq", which
# is correct for the current language spec

let x = 1:
while (x == 1)
(
  x = x+1;
  x = x-1;
)
# good luck compiling this by calling eval.  Respect the Halting Problem!
