let f = (lambda x.
  if (8 <= x) 1 else
    (let y = (x+x):((f y) + x))):
(f 2)
# should return 7
