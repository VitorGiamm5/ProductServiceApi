# Roadmap

## Current Focus

- Stabilize Blazor Web login/logout.
- Stabilize Keycloak roles and API authorization.
- Stabilize Redis timeout/circuit breaker behavior during startup.
- Keep Docker Compose local stack reliable.

## Next Good Steps

1. Add explicit tests for API authorization policies.
2. Add a "me" endpoint or Blazor user display model.
3. Persist DataProtection keys for Blazor Web and API containers.
4. Refine order ownership model for `orders.view_own`.
5. Test and polish `POST /Orders` idempotency with `IdempotencyKey` and Redis-backed `IdempotentAPI`.
6. Add distributed locking around HTTP idempotency for multi-instance API deployments.
7. Add a persistent Postgres idempotency table for create-order guarantees beyond cache lifetime.
8. Turn the current audit/outbox persistence into a complete integration-event Outbox publisher flow.
9. Add Inbox/deduplication support for future event consumers.
10. Add audit viewer endpoints or admin-only audit queries.
11. Add SOPS + Age encrypted environment configuration.
12. Improve local deployment flow with a self-hosted CI/CD runner.
13. Revisit BFF strategy before adding Angular.

## Future Domains

Planned expansion areas:

- HR
- Production orders
- Order status workflow
- NFe issuance
- Order reports
- Real-time order dashboard
- Inventory control
- Payment control
- User management and permissions
- Telephone/order operator workflow
- Audit history
