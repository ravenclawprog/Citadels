namespace UnitTestMiscellaneous;

using Citadel;

public class UnitTestListRandomizer
{
    [Fact]
    public void TestPermutation()
    {
        List<int> list = [1, 2, 3, 4, 5];
        List<int> examList = new List<int>(list);
        ListRandomizer.Permutate<int>(ref list);
        System.Console.WriteLine("Exam list:        {0}", string.Join(",", examList));
        System.Console.WriteLine("Permutatet list:  {0}", string.Join(",", list));
        Assert.NotEqual(examList, list);
    }
    [Fact]
    public void TestPermutationOfContainerWithMoreElements()
    {
        List<int> list = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11];
        List<int> examList = new List<int>(list);
        ListRandomizer.Permutate<int>(ref list);
        System.Console.WriteLine("Exam list:        {0}", string.Join(",", examList));
        System.Console.WriteLine("Permutatet list:  {0}", string.Join(",", list));
        Assert.NotEqual(examList, list);
    }
}