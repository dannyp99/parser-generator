# testing recursion
let logbase2 = (lambda n. if (n == 1) 0 else 1+(logbase2 (n/2))): (logbase2 1024)
