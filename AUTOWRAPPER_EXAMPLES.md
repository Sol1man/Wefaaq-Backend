# AutoWrapper Response Examples

## Example 1: GET All Clients (Success)

### Request:
```http
GET /api/clients/get-all HTTP/1.1
Host: localhost:5000
```

### Response (Before AutoWrapper):
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "John Doe",
    "email": "john@example.com",
    "phoneNumber": "1234567890",
    "classification": 1,
    "balance": 1000.00,
    "externalWorkersCount": 5
  }
]
```

### Response (After AutoWrapper):
```json
{
  "version": "1.0",
  "statusCode": 200,
  "message": "GET Request successful.",
  "isError": false,
  "result": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "John Doe",
      "email": "john@example.com",
      "phoneNumber": "1234567890",
      "classification": 1,
      "balance": 1000.00,
      "externalWorkersCount": 5
    }
  ]
}
```

## Example 2: POST Create Client (Success)

### Request:
```http
POST /api/clients HTTP/1.1
Host: localhost:5000
Content-Type: application/json

{
  "name": "Jane Smith",
  "email": "jane@example.com",
  "phoneNumber": "0987654321",
  "classification": 2,
  "balance": 500.00,
  "externalWorkersCount": 3,
  "organizationIds": []
}
```

### Response (Before AutoWrapper):
```json
{
  "id": "4fb85f64-5717-4562-b3fc-2c963f66afa7",
  "name": "Jane Smith",
  "email": "jane@example.com",
  "phoneNumber": "0987654321",
  "classification": 2,
  "balance": 500.00,
  "externalWorkersCount": 3
}
```

### Response (After AutoWrapper):
```json
{
  "version": "1.0",
  "statusCode": 201,
  "message": "POST Request successful.",
  "isError": false,
  "result": {
    "id": "4fb85f64-5717-4562-b3fc-2c963f66afa7",
    "name": "Jane Smith",
    "email": "jane@example.com",
    "phoneNumber": "0987654321",
    "classification": 2,
    "balance": 500.00,
    "externalWorkersCount": 3
  }
}
```

## Example 3: GET Client by ID (Not Found)

### Request:
```http
GET /api/clients/00000000-0000-0000-0000-000000000000 HTTP/1.1
Host: localhost:5000
```

### Response (Before AutoWrapper):
```json
{
  "message": "Client with ID 00000000-0000-0000-0000-000000000000 not found"
}
```

### Response (After AutoWrapper):
```json
{
  "version": "1.0",
  "statusCode": 404,
  "message": "Client with ID 00000000-0000-0000-0000-000000000000 not found",
  "isError": true
}
```

## Example 4: POST Create Client (Validation Error)

### Request:
```http
POST /api/clients HTTP/1.1
Host: localhost:5000
Content-Type: application/json

{
  "name": "",
  "email": "invalid-email",
  "classification": 2,
  "balance": -100.00
}
```

### Response (Before AutoWrapper):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": ["The Name field is required."],
    "Email": ["The Email field is not a valid e-mail address."]
  }
}
```

### Response (After AutoWrapper):
```json
{
  "version": "1.0",
  "statusCode": 400,
  "message": "Request responded with validation error(s). Please correct the specified validation errors and try again.",
  "isError": true,
  "validationErrors": [
    {
      "field": "Name",
      "message": "The Name field is required."
    },
    {
      "field": "Email",
      "message": "The Email field is not a valid e-mail address."
    }
  ]
}
```

## Example 5: DELETE Client (Success)

### Request:
```http
DELETE /api/clients/3fa85f64-5717-4562-b3fc-2c963f66afa6 HTTP/1.1
Host: localhost:5000
```

### Response (Before AutoWrapper):
```http
HTTP/1.1 204 No Content
```

### Response (After AutoWrapper):
```json
{
  "version": "1.0",
  "statusCode": 204,
  "message": "DELETE Request successful.",
  "isError": false
}
```

## Example 6: Server Error (Exception)

### Request:
```http
GET /api/clients/invalid-operation HTTP/1.1
Host: localhost:5000
```

### Response (Before AutoWrapper):
```json
{
  "statusCode": 500,
  "message": "An internal server error occurred. Please try again later."
}
```

### Response (After AutoWrapper):
```json
{
  "version": "1.0",
  "statusCode": 500,
  "message": "An unhandled exception occurred while processing the request.",
  "isError": true,
  "error": {
    "type": "System.Exception",
    "message": "An error occurred while processing your request."
  }
}
```

## Example 7: PUT Update Client (Success)

### Request:
```http
PUT /api/clients/3fa85f64-5717-4562-b3fc-2c963f66afa6 HTTP/1.1
Host: localhost:5000
Content-Type: application/json

{
  "name": "John Doe Updated",
  "email": "john.updated@example.com",
  "phoneNumber": "1234567890",
  "classification": 1,
  "balance": 1500.00,
  "externalWorkersCount": 7,
  "organizationIds": []
}
```

### Response (After AutoWrapper):
```json
{
  "version": "1.0",
  "statusCode": 200,
  "message": "PUT Request successful.",
  "isError": false,
  "result": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "John Doe Updated",
    "email": "john.updated@example.com",
    "phoneNumber": "1234567890",
    "classification": 1,
    "balance": 1500.00,
    "externalWorkersCount": 7
  }
}
```

## Testing with cURL

### GET Request:
```bash
curl -X GET "https://localhost:5001/api/clients/get-all" -k
```

### POST Request:
```bash
curl -X POST "https://localhost:5001/api/clients" \
  -H "Content-Type: application/json" \
  -d '{"name":"Test Client","email":"test@example.com","classification":1,"balance":0,"externalWorkersCount":0,"organizationIds":[]}' \
  -k
```

### PUT Request:
```bash
curl -X PUT "https://localhost:5001/api/clients/{id}" \
  -H "Content-Type: application/json" \
  -d '{"name":"Updated Client","email":"updated@example.com","classification":2,"balance":100,"externalWorkersCount":5,"organizationIds":[]}' \
  -k
```

### DELETE Request:
```bash
curl -X DELETE "https://localhost:5001/api/clients/{id}" -k
```

## Testing with Postman

1. Import the collection or create requests manually
2. Set the base URL to your API endpoint
3. Add appropriate headers (Content-Type: application/json)
4. Send requests and observe the wrapped responses
5. All responses will have the consistent format with version, statusCode, message, and isError

## Client-Side Consumption

### JavaScript/TypeScript Example:
```typescript
// Fetch all clients
const response = await fetch('/api/clients/get-all');
const data = await response.json();

if (!data.isError) {
  // Success - access the actual data
  const clients = data.result;
  console.log(clients);
} else {
  // Error - display the error message
  console.error(data.message);
}
```

### C# HttpClient Example:
```csharp
public class ApiResponse<T>
{
    public string Version { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public bool IsError { get; set; }
    public T Result { get; set; }
}

// Usage
var response = await httpClient.GetAsync("/api/clients/get-all");
var content = await response.Content.ReadAsStringAsync();
var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<ClientDto>>>(content);

if (!apiResponse.IsError)
{
    var clients = apiResponse.Result;
    // Process clients
}
```

## Key Observations

1. **Consistent Structure**: All responses follow the same format
2. **Status Code**: Always included in the response body
3. **IsError Flag**: Easy to check if request was successful
4. **Message**: Descriptive message for each response type
5. **Version**: API version is included for versioning support
6. **Result**: Actual data is nested under the "result" property for successful requests
7. **Validation Errors**: Properly formatted and easy to parse
