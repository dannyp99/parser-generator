# this program will not terminate under call_by_value:
let f = (lambda x.(cout "running...\n"; (f x);)):
let g = (lambda x. 1):
(g (f 0))
