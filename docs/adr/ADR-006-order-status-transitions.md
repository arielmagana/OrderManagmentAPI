# ADR-006: Define Order Status Transitions

## Status

Accepted

## Context

Orders in the Order Management API transition through different states as they are processed.

Without clear definition of valid states and transitions, the application could accept invalid state changes, leading to data inconsistency and unpredictable business logic behavior.

The API must enforce valid transitions to maintain data integrity and prevent invalid operations.

## Decision

Order status transitions will be explicitly defined and validated.

### Valid Order Statuses

The system supports the following order statuses:

- **Pending**: Order has been created but not yet confirmed by the customer.
- **Confirmed**: Customer has confirmed the order and payment is accepted.
- **Completed**: Order has been fulfilled and delivered.
- **Cancelled**: Order has been cancelled and will not be fulfilled.

### Valid Transitions

The following state transitions are valid:

```
Pending    → Confirmed   (Customer confirms order)
Pending    → Cancelled   (Customer cancels before confirmation)
Confirmed  → Completed   (Order is fulfilled and delivered)
```

### Invalid Transitions

The following transitions are NOT allowed and must be rejected with HTTP 409 Conflict:

```
Confirmed  → Pending     (Cannot revert to pending after confirmation)
Confirmed  → Cancelled   (Cannot cancel after confirmation)
Completed  → *           (Completed orders are immutable)
Cancelled  → *           (Cancelled orders are immutable)
```

### Initial Status

All newly created orders begin in the **Pending** state.

### Status Immutability

Once an order reaches **Completed** or **Cancelled** status, it becomes immutable and no further transitions are allowed.

## Consequences

### Positive

- Clear business rules prevent invalid state transitions.
- Consistent order lifecycle across the application.
- Data integrity is protected at the application layer.
- Status transitions can be validated before persistence.
- Future authentication/authorization can control who can trigger specific transitions.

### Negative

- Status transition rules must be validated in the Application layer and kept in sync with API documentation.
- Complex business logic may require additional status values in the future.
- Status changes are not reversible, limiting flexibility for some business scenarios.

## Notes

This ADR works in conjunction with [ADR-007-error-handling.md](ADR-007-error-handling.md) to define how invalid transitions are communicated to API clients (HTTP 409 Conflict).

An invalid transition attempt should return a 409 Conflict status with an error message explaining why the transition is not allowed.

Future enhancements might include:

- Role-based control over who can trigger specific transitions (requires authentication/authorization layer)
- Timestamp tracking for when each status transition occurred
- Audit trail of status changes for compliance or debugging
