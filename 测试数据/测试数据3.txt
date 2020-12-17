int x=1;
int sum=0;

/*if语句*/
if(x<10){
	write("x<10");
}else if(x>100){
	write("x>10");
}else{
	write("x=10");
}

/*while语句*/
while(x<10){
	sum=sum+1;
	x=x+1;
}
write(sum);

/*read*/
read("abc");

/*
output:
x<10
45
*/