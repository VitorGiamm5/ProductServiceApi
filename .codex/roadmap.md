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
5. Add audit viewer endpoints or admin-only audit queries.
6. Add SOPS + Age encrypted environment configuration.
7. Improve local deployment flow with a self-hosted CI/CD runner.
8. Revisit BFF strategy before adding Angular.

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
