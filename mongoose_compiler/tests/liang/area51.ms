# Save your planet by implementing the right kind of scoping rule.

let pi = 314159: 
let area = (lambda r.pi*r*r):
let pi = 1:
let composite = (area 51):
let quotient = composite/100000:
let remainder = composite%100000:
(
  cout "Area 51 is ";
  cout quotient;
  cout ".";
  cout remainder;
  cout "\n";
  if (!(quotient==8171)) cout "your planet is doomed\n";
  else cout "your planet is safe for now\n";
)
