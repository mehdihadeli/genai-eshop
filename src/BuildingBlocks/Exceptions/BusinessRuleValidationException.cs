namespace BuildingBlocks.Exceptions;

public class BusinessRuleValidationException(IBusinessRule brokenRule) : DomainException(brokenRule.Message)
{
    public IBusinessRule BrokenRule { get; } = brokenRule;

    public string Details { get; } = brokenRule.Message;

    public override string ToString()
    {
        return $"{BrokenRule.GetType().FullName}: {BrokenRule.Message}";
    }
}

public interface IBusinessRule
{
    string Message { get; }
    int Status { get; }
    bool IsBroken();
}
