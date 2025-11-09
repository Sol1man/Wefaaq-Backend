# ?? Client Controller - Quick Reference Card

## ?? Base URL
```
/api/clients
```

## ?? GET Endpoints

### Get All Clients
```http
GET /api/clients/get-all
```
**Returns:** List of all clients  
**Error Handling:** ? Try-Catch with Logger

### Get Client by ID
```http
GET /api/clients/get/{id}
```
**Parameters:** `id` (Guid)  
**Returns:** Single client details  
**Error Handling:** ? Try-Catch with Logger

### Get Client with Organizations
```http
GET /api/clients/organizations/{id}
```
**Parameters:** `id` (Guid)  
**Returns:** Client with all associated organizations  
**Error Handling:** ? Try-Catch with Logger

### Get Creditors (Positive Balance)
```http
GET /api/clients/creditors
```
**Returns:** Clients with balance > 0 (????)  
**Error Handling:** ? Try-Catch with Logger

### Get Debtors (Negative Balance)
```http
GET /api/clients/debtors
```
**Returns:** Clients with balance < 0 (????)  
**Error Handling:** ? Try-Catch with Logger

## ? POST Endpoints

### Create Client
```http
POST /api/clients/add
Content-Type: application/json

{
  "name": "string",
  "email": "string",
  "phoneNumber": "string",
  "classification": 1,
  "balance": 0,
  "externalWorkersCount": 0,
  "organizationIds": []
}
```
**Returns:** 201 Created with client details  
**Error Handling:** ? Try-Catch with Logger

### Create Client with Organizations
```http
POST /api/clients/add-with-organizations
Content-Type: application/json

{
  "name": "string",
  "email": "string",
  "phoneNumber": "string",
  "classification": 1,
  "balance": 0,
  "externalWorkersCount": 0,
  "organizations": [
    {
      "name": "string",
      "cardExpiringSoon": false
    }
  ]
}
```
**Returns:** 201 Created with client and organizations  
**Error Handling:** ? Try-Catch with Logger

## ?? PUT Endpoints

### Update Client
```http
PUT /api/clients/edit/{id}
Content-Type: application/json

{
  "name": "string",
  "email": "string",
  "phoneNumber": "string",
  "classification": 1,
  "balance": 0,
  "externalWorkersCount": 0,
  "organizationIds": []
}
```
**Parameters:** `id` (Guid)  
**Returns:** 200 OK with updated client  
**Error Handling:** ? Try-Catch with Logger

### Update Client with Organizations
```http
PUT /api/clients/edit-with-organizations/{id}
Content-Type: application/json

{
  "name": "string",
  "email": "string",
  "phoneNumber": "string",
  "classification": 1,
  "balance": 0,
  "externalWorkersCount": 0,
  "organizations": [
    {
      "name": "string",
      "cardExpiringSoon": false
    }
  ]
}
```
**Parameters:** `id` (Guid)  
**Returns:** 200 OK with updated client and organizations  
**Error Handling:** ? Try-Catch with Logger

## ? DELETE Endpoints

### Delete Client
```http
DELETE /api/clients/delete/{id}
```
**Parameters:** `id` (Guid)  
**Returns:** 204 No Content  
**Error Handling:** ? Try-Catch with Logger

## ?? URI Pattern Rules

? **Action-Based URIs:**
- Add operations: `/add`
- Edit operations: `/edit/{id}`
- Delete operations: `/delete/{id}`
- Get operations: `/get/{id}` or `/get-all`

? **ID Position:**
- Always at the end of the URI
- Examples: `/edit/{id}`, `/delete/{id}`, `/organizations/{id}`

## ?? Response Format (AutoWrapper)

### Success Response
```json
{
  "version": "1.0",
  "statusCode": 200,
  "message": "GET Request successful.",
  "isError": false,
  "result": { /* your data */ }
}
```

### Error Response
```json
{
  "version": "1.0",
  "statusCode": 400,
  "message": "Exception error message",
  "isError": true
}
```

## ?? HTTP Status Codes

| Code | Meaning | When |
|------|---------|------|
| 200 | OK | Successful GET/PUT |
| 201 | Created | Successful POST |
| 204 | No Content | Successful DELETE |
| 400 | Bad Request | Exception or validation error |
| 404 | Not Found | Resource not found |

## ?? cURL Examples

### Get All
```bash
curl -X GET "https://localhost:5001/api/clients/get-all" -k
```

### Get by ID
```bash
curl -X GET "https://localhost:5001/api/clients/get/{id}" -k
```

### Create
```bash
curl -X POST "https://localhost:5001/api/clients/add" \
  -H "Content-Type: application/json" \
  -d '{"name":"John Doe","email":"john@example.com","classification":1,"balance":0,"externalWorkersCount":0,"organizationIds":[]}' \
  -k
```

### Update
```bash
curl -X PUT "https://localhost:5001/api/clients/edit/{id}" \
  -H "Content-Type: application/json" \
  -d '{"name":"John Updated","email":"john@example.com","classification":1,"balance":100,"externalWorkersCount":5,"organizationIds":[]}' \
  -k
```

### Delete
```bash
curl -X DELETE "https://localhost:5001/api/clients/delete/{id}" -k
```

### Get with Organizations
```bash
curl -X GET "https://localhost:5001/api/clients/organizations/{id}" -k
```

## ?? Client Classification Enum
```
1 = Mumayyaz (???? - Distinguished/Premium)
2 = Aadi (???? - Regular/Standard)
3 = Mahwari (????? - Pivotal/Central)
```

## ??? Error Handling Pattern

All endpoints follow this pattern:
```csharp
try
{
    // Business logic
    var result = await _clientService.MethodAsync();
    return Ok(result);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error context with {Parameter}", value);
    return BadRequest(new { message = ex.Message });
}
```

## ?? Key Features

? **Explicit Action URIs** - Clear intent with `/add`, `/edit`, `/delete`  
? **ID at End** - Consistent parameter positioning  
? **Try-Catch All** - Comprehensive error handling  
? **Structured Logging** - ILogger with context  
? **Type-Safe Responses** - ProducesResponseType attributes  
? **AutoWrapper Integration** - Standardized response format  

---

**Print this card for quick reference!**
