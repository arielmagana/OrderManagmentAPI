# ADR-007: Standardized Error Handling and HTTP Status Codes

## Status

Accepted

## Context

REST APIs must communicate errors clearly to clients.

Without standardized error responses, clients cannot reliably distinguish between different types of failures (validation, resource not found, business rule violations, server errors) and cannot parse error messages consistently.

The Order Management API requires a consistent error response format and clear mapping of business scenarios to HTTP status codes.

## Decision

All error responses will use a standardized format and HTTP status codes will be used to indicate the category of error.

### HTTP Status Codes

The API will use the following HTTP status codes:

#### 200 OK
Used for successful GET requests and other successful read operations.

#### 201 Created
Used when a new resource is successfully created via POST.

The response includes the full resource representation including the generated ID.

#### 400 Bad Request
Used when the request is malformed or invalid.

Examples:
- Missing required fields
- Invalid JSON syntax
- Request body cannot be parsed

#### 404 Not Found
Used when a requested resource does not exist.

Examples:
- GET /customers/999 where customer ID 999 does not exist
- PUT /orders/999/status where order ID 999 does not exist

#### 409 Conflict
Used when a business rule or constraint violation occurs.

Examples:
- POST /customers with duplicate email address
- PUT /orders/{id}/status with invalid status transition
- POST /orders with inactive customer
- POST /orders with inactive product

#### 422 Unprocessable Entity
Used when field-level validation fails.

Examples:
- Email format is invalid
- Product unit price is not greater than zero
- Order quantity is negative

#### 500 Internal Server Error
Used when an unexpected server error occurs.

The response should not expose implementation details.

### Error Response Format

All error responses will use the following standardized JSON format:

```json
{
  "code": "ERROR_CODE",
  "message": "Human-readable error message",
  "errors": [
    {
      "field": "fieldName",
      "message": "Field-specific error message"
    }
  ]
}
```

**Field definitions:**

- `code` (string): A machine-readable error code for programmatic handling. Examples: `CUSTOMER_NOT_FOUND`, `INVALID_EMAIL`, `DUPLICATE_EMAIL`, `INVALID_STATUS_TRANSITION`
- `message` (string): A human-readable error message describing what went wrong.
- `errors` (array, optional): Only present for validation errors (400, 422). Contains field-specific error details.

### Error Code Examples

#### Validation Errors (400, 422)
- `INVALID_EMAIL`: Email format is invalid
- `INVALID_PRICE`: Unit price must be greater than zero
- `INVALID_QUANTITY`: Quantity must be a positive integer
- `MISSING_REQUIRED_FIELD`: A required field is missing

#### Resource Not Found (404)
- `CUSTOMER_NOT_FOUND`: Customer with ID does not exist
- `PRODUCT_NOT_FOUND`: Product with ID does not exist
- `ORDER_NOT_FOUND`: Order with ID does not exist

#### Business Rule Violations (409)
- `DUPLICATE_EMAIL`: Email address already exists
- `DUPLICATE_SKU`: SKU already exists
- `CUSTOMER_INACTIVE`: Cannot create order for inactive customer
- `PRODUCT_INACTIVE`: Cannot add inactive product to order
- `INVALID_STATUS_TRANSITION`: Order status transition is not allowed

#### Server Errors (500)
- `INTERNAL_ERROR`: An unexpected error occurred
- `DATABASE_ERROR`: Database operation failed

### Example Error Responses

#### Validation Error (400/422)
```json
{
  "code": "INVALID_EMAIL",
  "message": "The request contains validation errors",
  "errors": [
    {
      "field": "email",
      "message": "Email address must be in valid format"
    }
  ]
}
```

#### Duplicate Resource (409)
```json
{
  "code": "DUPLICATE_EMAIL",
  "message": "Email address already exists",
  "errors": []
}
```

#### Invalid Status Transition (409)
```json
{
  "code": "INVALID_STATUS_TRANSITION",
  "message": "An order in Completed status cannot be changed to Cancelled"
}
```

#### Not Found (404)
```json
{
  "code": "CUSTOMER_NOT_FOUND",
  "message": "Customer with ID 999 does not exist"
}
```

#### Server Error (500)
```json
{
  "code": "INTERNAL_ERROR",
  "message": "An unexpected error occurred. Please contact support if the problem persists."
}
```

## Consequences

### Positive

- All API clients can parse errors consistently.
- Error codes enable programmatic error handling in clients.
- Human-readable messages improve debugging and user experience.
- Clear mapping between business rules and HTTP status codes.
- Field-level validation errors are explicit.
- Implementation details are not exposed to clients.

### Negative

- All layers (API, Application, Infrastructure) must respect the error format contract.
- Application exceptions must be translated to standard error codes.
- Error handling requires careful attention during development.

## Notes

This ADR works in conjunction with:
- [ADR-006-order-status-transitions.md](ADR-006-order-status-transitions.md) — Invalid status transitions return 409 Conflict
- The Application layer should define business exception types for each error scenario
- The API layer should translate domain/application exceptions to standard error responses
- Logging should capture the full exception stack trace for debugging while error responses remain user-friendly
