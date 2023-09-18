namespace MethodAggregator.Tests;

internal class ClassForRegisterClassTest
{
    public static int StaticTestMethod1(int i)
    {
        return i;
    }

    public void TestMethod1(string stringParameter)
    {
        //do something
    }

    public string TestMethod2(string stringParameter)
    {
        return stringParameter;
    }

    public string TestMethod3()
    {
        return nameof(TestMethod3);
    }
}