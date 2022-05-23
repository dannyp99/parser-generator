# Hi,
# I want to clarify some things I said in class today (4/22).
# Someone made a very meaningful comment that destructive
# assignment behaves 'inconsistently' under call-by-name.  If I understood
# correctly, what 'inconsistent' here means is that the result is different
# from call-by-value.  This is to be expected, because destructive assignment
# is not within pure lambda calculus and we will in fact lose the Church-
# Rosser property.  Specifically, the following program will return 1
# (as the final value of x) under call by value (f gets called first, then g)
# but will return 0 under call by name (g gets called first, then f).

let x = 2:
let f = (lambda y. (x = x*0; y;)):
let g = (lambda y. (x = x+1; y;)):
(
 (g (f 1));
 x;
) 
# (this mongoose program is now available on the server as test number 15)

# This assignment is perhaps the most important the semester because 
# you will learn the most from it: not just about F# but about programming
# languages in general, about the kind of choices you have while
# implementing a language and the impact of those choices on the language.
# The person who made this observation also suggested that perhaps destructive
# assignment should be disabled under call-by-name.  That is one way to
# get rid of the inconsistency.  This is precisely the kind of thinking that
# I hope you will all engage in.  The most important example of a programming
# language that uses call-by-name as default is Haskell, which is more
# purely functional than F#.  In Haskell, a variable can only be assigned to
# once when it's declared (single assignment = no destructive assignments),
# so disabling destructive assignment under call-by-name is a reasonable
# choice.
