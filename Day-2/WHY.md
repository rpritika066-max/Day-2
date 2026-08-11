# Why Rich Domain Models?

An anemic domain model stores only data and leaves business rules outside the entity. This often causes validation logic to spread across controllers, services, and repositories. As the application grows, different parts of the system may apply different rules, creating inconsistent behavior and bugs.

A rich domain model keeps important business rules close to the data they protect. The Quote entity now controls its own creation through the Quote.Create method, ensuring that every Quote always satisfies its invariants. The entity also controls deletion through SoftDelete instead of allowing external code to directly modify its state.

For example, in an anemic model, a developer could accidentally create a Quote with an empty author or extremely long text from a new API endpoint because they forgot to add validation. The database might accept the invalid data, causing inconsistent records.

With a rich model, invalid states are prevented because the entity itself enforces the rules. Any part of the application that creates a Quote must use the domain rules automatically.

Rich domain models make code easier to maintain, easier to test, and reduce the chance of business logic being duplicated or incorrectly implemented.