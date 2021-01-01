/*if while嵌套*/
int x;
real sum;
x = 1;
sum = 0;
/*if自嵌套*/
if (x < 10) {
	write x < 10;
}
else
{
	if (x > 10) {
		write x > 10;
	}
	else {
		write x == 10;
	}
}

/*while自嵌套*/
while (x < 10) {
	sum = sum + 1;
	while (x < 5)
	{
		x = x + 1;
	}
	x = x + 1;
}
write sum;

x = 1;
/*if中嵌套while,while中嵌套if*/
if (x < 10) {
	while (x<5)
	{
		x = x + 1;
		if (x == 3)
		{
			write x;
		}
	}
}
write x;

