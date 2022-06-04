using Eventuous;

namespace Orders.Domain;

public record OrderId(string Value) : AggregateId(Value);
public record OrderRowId(string Value) : AggregateId(Value);
public record ProductId(string Value) : AggregateId(Value);