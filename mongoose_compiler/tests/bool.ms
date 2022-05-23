let x = 5:
let y = 4:
(
  # this will short circuit, not change y
  cout ((x = 2; x;) || (y = 3; y;)); cout "\n";
  cout x; cout "\n";
  cout y; cout "\n";
  2 || 3;
)
