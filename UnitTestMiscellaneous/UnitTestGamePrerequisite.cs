namespace UnitTestMiscellaneous;

using Citadel;

public class UnitTestGamePrerequisite
{

    [Fact]
    public void TestRightPlayerCardSequence()
    {
        GamePrerequisite prereq = new BaseGamePrerequisite();
        Assert.Equal(1, prereq.Sequence(new AssassinPlayerCard()));
        Assert.Equal(2, prereq.Sequence(new ThiefPlayerCard()));
        Assert.Equal(3, prereq.Sequence(new SorcererPlayerCard()));
        Assert.Equal(4, prereq.Sequence(new KingPlayerCard()));
        Assert.Equal(5, prereq.Sequence(new BishopPlayerCard()));
        Assert.Equal(6, prereq.Sequence(new MerchantPlayerCard()));
        Assert.Equal(7, prereq.Sequence(new ArchitectPlayerCard()));
        Assert.Equal(8, prereq.Sequence(new СondottierePlayerCard()));
    }
    [Fact]
    public void TestWrongPlayerCardSequence()
    {
        GamePrerequisite prereq = new BaseGamePrerequisite();
        Assert.NotEqual(1, prereq.Sequence(new ThiefPlayerCard()));
        var expectedMessage = "Unknown type of Player card:Citadel.NullPlayerCard";
        var ex = Assert.Throws<WrongPlayerCard>(() => prereq.Sequence(new NullPlayerCard()));
        if (ex != null)
        {
            Console.WriteLine("get exception with message:{0}", ex.Message);
        }
        else
        {
            Assert.False(false,"No throw");
            return;
        }
        Assert.Equal(expectedMessage, ex.Message);
    }
}