# Order Management API

## 1. Overview

The Order Management API exposes REST endpoints for managing customers, products, and orders.

Base URL:

```text
/api
```

Content type:
```text
application/json
```

## 2. Customers
### GET /customers

Returns a list of customers.

**Response**
```http
200 OK
```

```json
[
  {
    "id": 1,
    "name": "John Smith",
    "email": "john.smith@example.com",
    "isActive": true
  }
]
```

### GET /customers/{id}

Returns a customer by ID.

**Response**
```http
200 OK
```

```json
{
  "id": 1,
  "name": "John Smith",
  "email": "john.smith@example.com",
  "isActive": true
}
```

Not found
```http
404 Not Found
```

### POST /customers

Creates a customer.

**Request**
```json
{
  "name": "John Smith",
  "email": "john.smith@example.com"
}
```

**Validation**
* Name is required.
* Email is required.
* Email must be valid.
* Email must be unique.

**Response**
```http
201 Created
```

```json
{
  "id": 1,
  "name": "John Smith",
  "email": "john.smith@example.com",
  "isActive": true
}
```

## 3. Products
### GET /products

Returns a list of products.

Optional query parameters:
```text
page
pageSize
```

Example:
```http
GET /api/products?page=1&pageSize=20
```

### GET /products/{id}

Returns a product by ID.

**Response**
```http
200 OK
```

```json
{
  "id": 10,
  "sku": "PROD-001",
  "name": "Example Product",
  "unitPrice": 49.99,
  "isActive": true
}
```

### POST /products

Creates a product.

**Request**
```json
{
  "sku": "PROD-001",
  "name": "Example Product",
  "unitPrice": 49.99
}
```

**Validation**
* SKU is required.
* SKU must be unique.
* Name is required.
* UnitPrice must be greater than zero.

**Response**
```http
201 Created
```

## 4. Orders
### GET /orders

Returns a list of orders.

Optional query parameters:
```text
customerId
status
page
pageSize
```

Example:
```http
GET /api/orders?status=Pending&page=1&pageSize=20
```
### GET /orders/{id}

Returns an order including its items.

**Response**
```http
200 OK
```

```json
{
  "id": 100,
  "customerId": 1,
  "orderDate": "2026-01-15T10:30:00Z",
  "status": "Pending",
  "totalAmount": 149.97,
  "items": [
    {
      "productId": 10,
      "quantity": 3,
      "unitPrice": 49.99,
      "subtotal": 149.97
    }
  ]
}
```

### POST /orders

Creates an order.

**Request**
```json
{
  "customerId": 1,
  "items": [
    {
      "productId": 10,
      "quantity": 3
    }
  ]
}
```

**Processing rules**

The application must:

1. Validate the customer.
1. Ensure the customer is active.
1. Validate all products.
1. Ensure all products are active.
1. Retrieve current product prices.
1. Calculate item subtotals.
1. Calculate the order total.
1. Create the order.
1. Persist the order and items atomically.

**Response**
```http
201 Created
```

```json
{
  "id": 100,
  "customerId": 1,
  "status": "Pending",
  "totalAmount": 149.97,
  "items": [
    {
      "productId": 10,
      "quantity": 3,
      "unitPrice": 49.99,
      "subtotal": 149.97
    }
  ]
}
```

### PUT /orders/{id}/status

Changes the status of an order.

**Request**
```json
{
  "status": "Confirmed"
}
```

**Valid transitions**
```text
Pending → Confirmed
Pending → Cancelled
Confirmed → Completed
```

**Invalid transition**

If the requested transition is not allowed:

```http
409 Conflict
```

Example:
```json
{
  "title": "Invalid order status transition",
  "detail": "An order in Completed status cannot be changed to Cancelled."
}
```
## 5. HTTP Status Codes

The API uses the following status codes:

| Status | Usage                   |
| ------ | ----------------------- |
| 200    | Successful GET/update   |
| 201    | Resource created        |
| 400    | Invalid request         |
| 404    | Resource not found      |
| 409    | Business rule/conflict  |
| 500    | Unexpected server error |

## 6. Error Response

The API uses the standard ASP.NET Core problem details format where appropriate.

Example:
```json
{
  "type": "https://example.com/problems/validation-error",
  "title": "Validation failed",
  "status": 400,
  "errors": {
    "email": [
      "A valid email address is required."
    ]
  }
}
```

## 7. Pagination

Endpoints returning collections may support:
```text
page
pageSize
```

Default:
```text
page = 1
pageSize = 20
```

Maximum page size:
```text
100
```

The implementation should avoid loading unbounded collections into memory.

## 8. API Documentation

The API exposes an OpenAPI document.

Swagger UI is available in the development environment to allow interactive testing of the API.

The OpenAPI contract should remain synchronized with the implementation.
