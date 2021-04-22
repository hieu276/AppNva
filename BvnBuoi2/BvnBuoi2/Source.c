#include<stdio.h>
#define MAX(x1, x2) (((x1) > (x2)) ? (x1) : (x2))
#define MIN(x1, x2) (((x1) > (x2)) ? (x2) : (x1))
void nhap(int a[], int n)
{
    for (int i = 0; i < n; i++)
    {
        printf("Nhap vao phan tu a[%d]: ", i);
        scanf_s("%d", &a[i]);
    }
}
int max(int* arr, int n)
{
    if (n == 2)
    {
        return MAX(arr[0], arr[1]);
    }

    return MAX(arr[n - 1], max(arr, n - 1));
}

int min(int* arr, int n)
{
    if (n == 2)
    {
        return MIN(arr[0], arr[1]);
    }

    return MIN(arr[n - 1], min(arr, n - 1));
}
int main()
{
    int a[1000];
    int n;
    printf("\nNhap n = ");
    scanf_s("%d", &n);
    nhap(a, n);
    printf("\nMax = %d", max(a, n));
    printf("\nMin = %d", min(a, n));
    return 0;
}

