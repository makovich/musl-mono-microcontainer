namespace App.UnitTests
{
  using Shouldly;

  public class TruthyTests
  {
    public void Obvious_statement()
    {
      (false).ShouldNotBe(true);
    }
  }
}
