namespace UnitTestMiscellaneous;

using Citadel;

public class UnitTestListRandomizer
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(11)]
    public void PermutationPreservesElements(int count)
    {
        // Duplicates must retain their multiplicity too.
        List<int> values = Enumerable.Range(0, count).Select(i => i / 2).ToList();
        var expected = values.ToArray();
        ListRandomizer.Permutate(ref values);
        Assert.Equal(expected, values.OrderBy(value => value));
    }
}
