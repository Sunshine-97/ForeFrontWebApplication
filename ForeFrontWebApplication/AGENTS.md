# Agents Guidelines

## Design Principles

### Single Responsibility Principle (SRP)

- Each class and method must have **one and only one reason to change**.
- Do not combine unrelated behaviors in a single class. Extract separate concerns into their own classes or services.
- Controllers should only handle HTTP concerns; delegate business logic to service classes.
- Keep data access isolated from business rules.

### Open/Closed Principle (OCP)

- Classes should be **open for extension but closed for modification**.
- Favor abstractions (interfaces, abstract classes) over concrete implementations so new behavior can be added without altering existing code.
- Use dependency injection to supply implementations rather than hard-coding them.
- When adding new functionality, prefer creating new classes that implement existing interfaces over modifying existing classes.

## Clean Code Directives

### Naming

- Use clear, descriptive names for classes, methods, variables, and parameters. Names should reveal intent.
- Avoid abbreviations, single-letter names, and Hungarian notation.
- Use `PascalCase` for public members and type names; `camelCase` for local variables and parameters; prefix private fields with `_`.

### Methods

- Keep methods short and focused — ideally under 20 lines.
- A method should do one thing and do it well.
- Limit method parameters to three or fewer; use an object to group related parameters when needed.
- Avoid boolean flag parameters — split into separate methods instead.

### Formatting & Readability

- Use consistent indentation and formatting throughout the codebase.
- Add blank lines to separate logical sections within a method.
- Avoid deeply nested code — use early returns and guard clauses to reduce nesting.
- Remove dead code, commented-out code, and unused `using` directives.

### Comments

- Write code that is self-documenting; prefer clear names over comments.
- Only add comments to explain **why**, not **what**.
- Use XML doc comments (`///`) on public APIs.

### Error Handling

- Use exceptions for exceptional conditions, not for control flow.
- Catch only specific exceptions you can handle; let others propagate.
- Never swallow exceptions silently — at minimum, log them.
- Validate inputs at public API boundaries and fail fast.
