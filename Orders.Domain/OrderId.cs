using Eventuous;

namespace Orders.Domain;

public record OrderId(string Value) : AggregateId(Value);
public record OrderRowId(string Value) : AggregateId(Value);
public record ProductOrServiceId(string Value) : AggregateId(Value);