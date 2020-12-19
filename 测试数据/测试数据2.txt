int a=1;
int b=1;
int sum=0;

/*if语句*/
if(a<b){
	write("a<b");
}else if(a>b){
	write("a>b");
}else{
	write("a=b");
}

/*补充测试<>比较符*/
if(a<>b){
	write("a<>b");
}

/*补充测试==比较符*/
if(a==b){
	write("a=b");
}

/*while语句*/
while(a<10){
	sum=sum+a;
	a=a+1;
}
write(sum);
/*
output:
a=b
a=b
45
*/