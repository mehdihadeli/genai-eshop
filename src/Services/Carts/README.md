## Using Redis for Carts

For high-frequency, low-latency cart operations, we use Redis instead of PostgreSQL, for the following reasons:

- PERFORMANCE: Carts are high-frequency, low-latency operations (add/remove/update).
  Redis (in-memory) provides sub-millisecond response times vs PostgreSQL’s disk I/O.

- EPHEMERAL DATA: Most carts are abandoned or converted to orders within hours.
  Storing them in PostgreSQL creates unnecessary table bloat, WAL overhead, and index maintenance.

- AVOID POSTGRESQL CONCURRENCY OVERHEAD:
  PostgreSQL uses `xmin` (`Version` property) for optimistic concurrency.
  Rapid cart updates (e.g., user clicking “+” 3x fast) can trigger `DbUpdateConcurrencyException`.
  Handling retries adds complexity and latency — Redis avoids this entirely.

- TTL (Time-To-Live): Redis automatically expires abandoned carts (e.g., after 7 days).
  In PostgreSQL, we need scheduled jobs to clean up stale carts.