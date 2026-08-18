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
All endpoint paths below are relative to /api

## 2. Customers
### GET /customers

Returns a paginated list of customers.

Optional query parameters:
```text
page
pageSize
```

Example:
```http
GET /api/customers?page=1&pageSize=20
```

**Response**
```http
200 OK
```

```json
{
  "pageNumber": 1,
  "pageSize": 20,
  "totalCount": 42,
  "totalPages": 3,
  "items": [
    {
      "id": 1,
      "name": "John Smith",
      "email": "john.smith@example.com",
      "isActive": true
    }
  ]
}
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

**Error Responses**

Duplicate email (409 Conflict):
```json
{
  "code": "DUPLICATE_EMAIL",
  "message": "Email address already exists"
}
```

Invalid email format (422 Unprocessable Entity):
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

Missing required field (400 Bad Request):
```json
{
  "code": "MISSING_REQUIRED_FIELD",
  "message": "The request contains validation errors",
  "errors": [
    {
      "field": "name",
      "message": "Name is required"
    }
  ]
}
```

## 3. Products
### GET /products

Returns a paginated list of products.

Optional query parameters:
```text
page
pageSize
```

Example:
```http
GET /api/products?page=1&pageSize=20
```

**Response**
```http
200 OK
```

```json
{
  "pageNumber": 1,
  "pageSize": 20,
  "totalCount": 150,
  "totalPages": 8,
  "items": [
    {
      "id": 10,
      "sku": "PROD-001",
      "name": "Example Product",
      "unitPrice": 49.99,
      "isActive": true
    }
  ]
}
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

Returns a paginated list of orders.

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

**Response**
```http
200 OK
```

```json
{
  "pageNumber": 1,
  "pageSize": 20,
  "totalCount": 95,
  "totalPages": 5,
  "items": [
    {
      "id": 100,
      "customerId": 1,
      "orderDate": "2026-01-15T10:30:00Z",
      "status": "Pending",
      "totalAmount": 149.97
    }
  ]
}
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

**Validation**
* CustomerId is required.
* Items array is required and must contain at least one item.
* Each item must specify productId and quantity.
* Quantity must be greater than zero.
* Customer must exist and be active.
* All products must exist and be active.

**Processing rules**

The application must:

1. Validate the customer exists and is active.
1. Validate all products exist and are active.
1. Retrieve current product prices.
1. Calculate item subtotals.
1. Calculate the order total.
1. Create the order with status = Pending.
1. Persist the order and items atomically.

**Response**
```http
201 Created
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

**Error Responses**

Customer not found (404 Not Found):
```json
{
  "code": "CUSTOMER_NOT_FOUND",
  "message": "Customer with ID 999 does not exist"
}
```

Customer inactive (409 Conflict):
```json
{
  "code": "CUSTOMER_INACTIVE",
  "message": "Cannot create order for inactive customer"
}
```

Product not found (404 Not Found):
```json
{
  "code": "PRODUCT_NOT_FOUND",
  "message": "Product with ID 999 does not exist"
}
```

Product inactive (409 Conflict):
```json
{
  "code": "PRODUCT_INACTIVE",
  "message": "Cannot add inactive product to order"
}
```

Invalid quantity (422 Unprocessable Entity):
```json
{
  "code": "INVALID_QUANTITY",
  "message": "The request contains validation errors",
  "errors": [
    {
      "field": "items[0].quantity",
      "message": "Quantity must be greater than zero"
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

See [ADR-006: Order Status Transitions](./adr/ADR-006-order-status-transitions.md) for complete transition rules.

```text
Pending → Confirmed (Customer confirms order)
Pending → Cancelled  (Customer cancels before confirmation)
Confirmed → Completed (Order fulfilled)
```

**Response**
```http
200 OK
```

```json
{
  "id": 100,
  "customerId": 1,
  "orderDate": "2026-01-15T10:30:00Z",
  "status": "Confirmed",
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

**Error Responses**

Order not found (404 Not Found):
```json
{
  "code": "ORDER_NOT_FOUND",
  "message": "Order with ID 999 does not exist"
}
```

Invalid status transition (409 Conflict):
```json
{
  "code": "INVALID_STATUS_TRANSITION",
  "message": "An order in Completed status cannot be changed to Cancelled"
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
| 422    | Field-level validation failure |
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
