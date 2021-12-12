# this program will return
let x = 2:
let f = (lambda y. x = x*0):
let g = (lambda y. x = x+y):
(g (f 1))
