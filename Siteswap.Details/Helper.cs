namespace Siteswap.Details;

public static class Helper
{
    public static int Gcf(int a, int b)
    {
        while (b != 0)
        {
            int temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }

    public static int Lcm(int a, int b)
    {
        return a / Gcf(a, b) * b;
    }
}
