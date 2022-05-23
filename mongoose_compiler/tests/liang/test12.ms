let f=(lambda x.
   let g=(lambda y.x+y):
   let h=(lambda z.(g z)):(h 20)):(f 10)
