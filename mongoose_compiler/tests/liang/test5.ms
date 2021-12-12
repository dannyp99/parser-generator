# This program is supposed to go into an infinite loop when evaluated, but
# you should be able to compile it.

let x = 1:
while (x eq 1)
(
  x = x+1;
  x = x-1;
)
# good luck compiling this by calling eval.  Respect the Halting Problem!
