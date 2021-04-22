#include<stdio.h>
#include<conio.h>

void KiemTraTamGiac(int a, int b, int c)
{
	if (a + b <= c || a + c <= b || b + c <= a)
	{
		printf("\nTam giac khong hop le. Xin kiem tra lai !");
	}
	else
	{
		printf("\nDay la tam giac: ");
		if ((a == b) && (b == c))
		{
			printf("Deu");
		}
		else
		{
			if (a * a + b * b == c * c || a * c + c * c == b * b || b * b + c * c == a * c)
			{
				if (a == b || a == c || b == c)
				{
					printf("Vuong Can");
				}
				else
				{
					printf("Vuong");
				}
			}
			else if (a == b || a == c || b == c)
			{
				printf("Can");
			}
			else
			{
				printf("Thuong");
			}
		}
	}
}

int main()
{
	int a, b, c;
	printf("\nNhap canh a: ");
	scanf_s("%d", &a);

	printf("\nNhap canh b: ");
	scanf_s("%d", &b);

	printf("\nNhap canh c: ");
	scanf_s("%d", &c);

	KiemTraTamGiac(a, b, c);


	_getch();
	return 0;
}