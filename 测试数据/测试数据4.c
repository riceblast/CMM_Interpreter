/*数组声明、赋值、运算，read语句*/
int a[2];
real b[2];

write a[0];
write a[1];
write b[0];
write b[1];

a[0] = 1;
a[1] = 2;
b[0] = 1.5;
b[1] = 2.5;

write a[0];
write a[1];
write b[0];
write b[1];

a[0] = a[0] + a[1];
a[1] = 2 * a[1] - a[0];
b[0] = b[0] + b[1];
b[1] = 2 * b[1] - b[0];

write a[0];
write a[1];
write b[0];
write b[1];

/*read*/
real sum;

read sum;