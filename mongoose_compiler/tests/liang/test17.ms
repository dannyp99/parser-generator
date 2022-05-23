let a = 10:
let b = 20:
let c = 40:
let f = (lambda x.
   let g = (lambda y. y+x+a+b):
   (g b)+c):
(f 1)
