// translation of mongoose program
#include<stdio.h>
#include<stdlib.h>
#include "mgc_base.c"


 int mg_seq_4(int* x,int* y)
 {  int mg_line_result;

  mg_line_result = *x = (*x * *x);
  mg_line_result = *y = (*y + 1);
  mg_line_result = mongoose_cout_str("the value of y is now ");
  mg_line_result = mongoose_cout_expr(*y);
  mg_line_result = mongoose_cout_str("\n");

  return mg_line_result;
 }

int mg_while_function_5(int* x,int* y)
 { int while_result;
  while ((*y < 4)) while_result=mg_seq_4(x,y);
  return while_result;
 }


 int mg_seq_3(int* x,int* y)
 {  int mg_line_result;

  mg_line_result = mg_while_function_5(x,y);
  mg_line_result = mongoose_cout_str("the final value of x is ");
  mg_line_result = mongoose_cout_expr(*x);
  mg_line_result = mongoose_cout_str("\n");

  return mg_line_result;
 }

int mg_let_func_2(int x, int* y)
  {
 int mg_ret_val;

  mg_ret_val = mg_seq_3(&x,y);
  return mg_ret_val;
 }

int mg_let_func_1(int y)
  {
 int mg_ret_val;

  mg_ret_val = mg_let_func_2(2,&y);
  return mg_ret_val;
 }


int main()                           
{
  int main_result =  mongoose_cout_expr(mg_let_func_1(0));
 return 0;
}//main
