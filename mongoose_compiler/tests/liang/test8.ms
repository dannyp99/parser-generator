# testing recursion  (implicitly this is a let* or let rec...)
let log2 = (lambda n. if (n == 1) 0 else 1+(log2 (n/2))): (log2 1024)
