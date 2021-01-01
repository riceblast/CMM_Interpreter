/*声明、赋值、运算测试*/

/*声明语句*/
int a;
int b;
real c;
real d;
real e;
real f;


/*赋值语句*/
a = 1;
b = 2;
d = 1.0;
e = 1.5;

/*表达式*/
c = a + b;
write c;
c = b - a;
write c;
c = a * b;
write c;
c = a / b;
write c;

f = d + e;
write f;
f = e - d;
write f;
f = d * e;
write f;
f = d / e;
write f;

/*补充测试<>比较符*/
if (a <> b) {
	write a <> b;
}

/*补充测试==比较符*/
if (a == b) {
	write a == b;
}
