namespace PersonalFinance.BuildingBlocks.Domain;

public interface IDomainEvent
{
    DateTime EventTime { get; }   

}
