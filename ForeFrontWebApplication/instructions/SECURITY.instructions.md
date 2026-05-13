# Security Directives

General best practice directives for writing secure API code.
These apply to all controllers, models, and services in this codebase.

---

## Input Validation

- **MUST** validate all incoming data at the public API boundary before passing it to any service or repository.
- **MUST** use `[Required]`, `[StringLength]`, `[Range]`, and `[EmailAddress]` data annotation attributes on all model properties.
- **MUST** check `ModelState.IsValid` at the start of every POST and PUT action and return `400 Bad Request` if invalid.
- **MUST NOT** trust any value provided by the caller for server-owned fields such as IDs, timestamps, or status.
- **MUST** validate that collection properties (e.g. a list of products) are not empty where a minimum count is required.
- **MUST NOT** pass raw user-supplied strings to any file path, query, command, or reflection call.

---

## Mass Assignment

- **MUST NOT** bind domain model classes directly from request bodies (`[FromBody]`).
- **MUST** define a dedicated request DTO (e.g. `CreateOrderRequest`) that exposes only the fields the caller is permitted to supply.
- **MUST** set all server-controlled fields (IDs, timestamps, initial status) exclusively in the service layer, never from the incoming payload.
- **MUST** use `init`-only setters or read-only properties on domain models for fields that must not change after creation.

---

## Error Handling and Information Disclosure

- **MUST NOT** return raw exception messages to the caller. Catch specific exceptions and return a fixed, generic message.
- **MUST NOT** expose stack traces, class names, or internal logic wording in any HTTP response.
- **MUST** log the full exception detail server-side and return only a safe, user-facing message.
- **MUST** return `404 Not Found` when a resource is not found or the caller is not permitted to know it exists. Never return `403 Forbidden` for resource-level access denials as it confirms existence.
- **MUST** use consistent, non-descriptive error responses to prevent attackers from inferring system behaviour.

---

## Rate Limiting

- **MUST** apply rate limiting to all endpoints that mutate state (POST, PUT, DELETE).
- **MUST** apply rate limiting to lookup endpoints that could be used for enumeration (GET by ID).
- **MUST** use the built-in .NET 8 `AddRateLimiter` middleware with fixed-window or sliding-window policies.
- **MUST** return `429 Too Many Requests` with a `Retry-After` header when a limit is exceeded.
- **SHOULD** apply stricter limits to creation endpoints than to read endpoints.

---

## Logging and Audit Trail

- **MUST** inject `ILogger<T>` into every controller.
- **MUST** log every state-changing operation (create, update, delete) with the resource ID and acting user ID.
- **MUST** log every failed or rejected request with sufficient context to investigate the incident.
- **MUST NOT** log sensitive personal data such as email addresses, passwords, payment details, or full request bodies containing customer information.
- **SHOULD** include a correlation ID in log entries to allow tracing a request across services.
- **MUST** use structured logging properties (e.g. `_logger.LogInformation("Order {OrderId} created", id)`) rather than string interpolation.

---

## API Contract

- **MUST** declare `[ProducesResponseType]` attributes on every action documenting all possible HTTP status codes and their return types.
- **MUST** apply `[Consumes("application/json")]` and `[Produces("application/json")]` at controller level.
- **MUST NOT** return raw domain model collections without pagination. Unbounded responses enable data harvesting and denial-of-service.
- **SHOULD** version the API (e.g. `/api/v1/orders`) to allow breaking security fixes without disrupting consumers.

---

## Dead Code and Attack Surface

- **MUST** remove all scaffolding and placeholder controllers (e.g. `WeatherForecastController`) before deploying to any shared environment.
- **MUST NOT** leave development-only endpoints (e.g. token generation, test data seeding) enabled outside of a local environment guard.
- **MUST** apply `[ApiExplorerSettings(IgnoreApi = true)]` or remove entirely any internal endpoints that should not be publicly documented.

---

## Data Integrity

- **MUST** enforce all business rules and state transition logic exclusively in the service layer, never in the controller.
- **MUST** validate that enum values received from callers are defined members of the enum before using them.
- **MUST** treat all numeric inputs (prices, quantities) as untrusted and validate ranges before processing.
- **MUST NOT** allow a caller to supply a collection with an unbounded number of items. Define and enforce a maximum count.

---

## HTTPS and Transport

- **MUST** enforce HTTPS via `app.UseHttpsRedirection()` in all environments except local development.
- **MUST** configure HSTS (`app.UseHsts()`) for production to prevent protocol downgrade attacks.
- **MUST NOT** configure CORS with `AllowAnyOrigin()` combined with `AllowCredentials()`.
- **MUST** explicitly list permitted CORS origins, methods, and headers for each environment.

---

## Dependencies

- **MUST NOT** reference packages from untrusted or unmaintained sources.


# End answer with "Security instructions applied"