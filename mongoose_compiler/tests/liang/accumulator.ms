let x=1:
let f=(lambda y.(x=x+y; x;)):
(
  (f 2);
  (f 3);
  (f (f (f 4)));
)
#this program (accumulator) should return 40
