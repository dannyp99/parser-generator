# this program will not terminate under call_by_value:
let f = (lambda x. while (1) cout "still true\n"):
let g = (lambda x.1):
(g (f 0))
