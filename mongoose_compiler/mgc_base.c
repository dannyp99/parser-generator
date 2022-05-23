#include<stdio.h>
#include<stdlib.h>
#include<stdint.h>

typedef int32_t i32;

i32 mongoose_cin()
{
  i32 x;
  printf(">> ");
  scanf("%d",&x);
  return x;
}//mongoose_cin()

i32 mongoose_cout_str(char* s)
{
  printf("%s",s);  return 0;
}
i32 mongoose_cout_expr(i32 x)
{
  printf("%d",x);  return 0;
}

i32 mongoose_expt(i32 x, i32 n)  // x**n
{
  i32 ax = 1;
  i32 fct = x;
  while (n>0)
    {
      if (n%2==1) ax*=fct;
      fct *=fct;
      n/=2;
    }
  return ax;          
}

i32 mongoose_assign(i32* x, i32 y)
{
  *x=y;
  return y;
}

i32 mongoose_eq(i32 x, i32 y)
{
  if (x==y) return 1; else return 0;
}
i32 mongoose_lt(i32 x, i32 y)
{
  if (x<y) return 1; else return 0;
}
i32 mongoose_leq(i32 x, i32 y)
{
  if (x<=y) return 1; else return 0;
}
i32 mongoose_or(i32 x, i32 y)
{
  if (x==0 && y==0) return 0; else return 1;
}
i32 mongoose_and(i32 x, i32 y)
{
  if (x==0 || y==0) return 0; else return 1;
}
