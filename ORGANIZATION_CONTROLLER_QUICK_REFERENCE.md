# ?? Organization Controller - Quick Reference Card

## ?? Base URL
```
/api/organizations
```

---

## ?? GET Endpoints

### Get All Organizations
```http
GET /api/organizations/get-all
```
**Returns:** List of all organizations  
**Error Handling:** ? Try-Catch with Logger

### Get Organization by ID
```http
GET /api/organizations/get/{id}
```
**Parameters:** `id` (Guid)  
**Returns:** Single organization details  
**Error Handling:** ? Try-Catch with Logger

### Get Organization with Details
```http
GET /api/organizations/details/{id}
```
**Parameters:** `id` (Guid)  
**Returns:** Organization with all related data (records, workers, licenses, cars)  
**Error Handling:** ? Try-Catch with Logger

### Get Organizations with Expiring Cards
```http
GET /api/organizations/expiring-cards
```
**Returns:** Organizations where cardExpiringSoon = true  
**Error Handling:** ? Try-Catch with Logger

---

## ? POST Endpoints

### Create Organization
```http
POST /api/organizations/add
Content-Type: application/json

{
  "name": "string",
  "cardExpiringSoon": false,
  "clientId": "guid"
}
```
**Returns:** 201 Created with organization details  
**Error Handling:** ? Try-Catch with Logger

### Add Record to Organization
```http
POST /api/organizations/records/add/{organizationId}
Content-Type: application/json

{
  "number": "string",
  "expiryDate": "2025-12-31",
  "imagePath": "string"
}
```
**Parameters:** `organizationId` (Guid)  
**Returns:** 201 Created with record details  
**Error Handling:** ? Try-Catch with Logger

### Add Worker to Organization
```http
POST /api/organizations/workers/add/{organizationId}
Content-Type: application/json

{
  "name": "string",
  "residenceNumber": "string",
  "residenceImagePath": "string",
  "expiryDate": "2025-12-31"
}
```
**Parameters:** `organizationId` (Guid)  
**Returns:** 201 Created with worker details  
**Error Handling:** ? Try-Catch with Logger

---

## ?? PUT Endpoints

### Update Organization
```http
PUT /api/organizations/edit/{id}
Content-Type: application/json

{
  "name": "string",
  "cardExpiringSoon": false,
  "clientId": "guid"
}
```
**Parameters:** `id` (Guid)  
**Returns:** 200 OK with updated organization  
**Error Handling:** ? Try-Catch with Logger

### Update Organization Record
```http
PUT /api/organizations/records/edit/{organizationId}/{recordId}
Content-Type: application/json

{
  "number": "string",
  "expiryDate": "2025-12-31",
  "imagePath": "string"
}
```
**Parameters:** `organizationId` (Guid), `recordId` (Guid)  
**Returns:** 200 OK with updated record  
**Error Handling:** ? Try-Catch with Logger

### Update Organization Worker
```http
PUT /api/organizations/workers/edit/{organizationId}/{workerId}
Content-Type: application/json

{
  "name": "string",
  "residenceNumber": "string",
  "residenceImagePath": "string",
  "expiryDate": "2025-12-31"
}
```
**Parameters:** `organizationId` (Guid), `workerId` (Guid)  
**Returns:** 200 OK with updated worker  
**Error Handling:** ? Try-Catch with Logger

---

## ? DELETE Endpoints

### Delete Organization
```http
DELETE /api/organizations/delete/{id}
```
**Parameters:** `id` (Guid)  
**Returns:** 204 No Content  
**Error Handling:** ? Try-Catch with Logger

### Delete Organization Record
```http
DELETE /api/organizations/records/delete/{organizationId}/{recordId}
```
**Parameters:** `organizationId` (Guid), `recordId` (Guid)  
**Returns:** 204 No Content  
**Error Handling:** ? Try-Catch with Logger

### Delete Organization Worker
```http
DELETE /api/organizations/workers/delete/{organizationId}/{workerId}
```
**Parameters:** `organizationId` (Guid), `workerId` (Guid)  
**Returns:** 204 No Content  
**Error Handling:** ? Try-Catch with Logger

---

## ?? URI Pattern Rules

? **Action-Based URIs:**
- Add operations: `/add`, `/records/add/{organizationId}`, `/workers/add/{organizationId}`
- Edit operations: `/edit/{id}`, `/records/edit/{organizationId}/{recordId}`, `/workers/edit/{organizationId}/{workerId}`
- Delete operations: `/delete/{id}`, `/records/delete/{organizationId}/{recordId}`, `/workers/delete/{organizationId}/{workerId}`
- Get operations: `/get/{id}`, `/get-all`, `/details/{id}`

? **ID Position:**
- Always at the end of the URI
- Examples: `/edit/{id}`, `/records/add/{organizationId}`, `/workers/edit/{organizationId}/{workerId}`

? **Hierarchical Sub-Resources:**
- Records: `/records/add/{organizationId}`, `/records/edit/{organizationId}/{recordId}`
- Workers: `/workers/add/{organizationId}`, `/workers/edit/{organizationId}/{workerId}`

---

## ?? Response Format (AutoWrapper)

### Success Response
```json
{
  "version": "1.0",
  "statusCode": 200,
  "message": "GET Request successful.",
  "isError": false,
  "result": {
    "id": "guid",
    "name": "Organization Name",
    "cardExpiringSoon": false,
    "clientId": "guid",
    "client": "Client Name",
    "records": [],
    "licenses": [],
    "workers": [],
    "cars": []
  }
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

---

## ?? HTTP Status Codes

| Code | Meaning | When |
|------|---------|------|
| 200 | OK | Successful GET/PUT |
| 201 | Created | Successful POST |
| 204 | No Content | Successful DELETE |
| 400 | Bad Request | Exception or validation error |
| 404 | Not Found | Resource not found |

---

## ?? cURL Examples

### Get All Organizations
```bash
curl -X GET "https://localhost:5001/api/organizations/get-all" -k
```

### Get Organization by ID
```bash
curl -X GET "https://localhost:5001/api/organizations/get/{id}" -k
```

### Create Organization
```bash
curl -X POST "https://localhost:5001/api/organizations/add" \
  -H "Content-Type: application/json" \
  -d '{"name":"New Org","cardExpiringSoon":false,"clientId":"guid"}' \
  -k
```

### Update Organization
```bash
curl -X PUT "https://localhost:5001/api/organizations/edit/{id}" \
  -H "Content-Type: application/json" \
  -d '{"name":"Updated Org","cardExpiringSoon":true,"clientId":"guid"}' \
  -k
```

### Delete Organization
```bash
curl -X DELETE "https://localhost:5001/api/organizations/delete/{id}" -k
```

### Add Record to Organization
```bash
curl -X POST "https://localhost:5001/api/organizations/records/add/{organizationId}" \
  -H "Content-Type: application/json" \
  -d '{"number":"REC-001","expiryDate":"2025-12-31","imagePath":"/path"}' \
  -k
```

### Add Worker to Organization
```bash
curl -X POST "https://localhost:5001/api/organizations/workers/add/{organizationId}" \
  -H "Content-Type: application/json" \
  -d '{"name":"John","residenceNumber":"RES-123","residenceImagePath":"/path","expiryDate":"2025-12-31"}' \
  -k
```

---

## ??? Error Handling Pattern

All endpoints follow this pattern:
```csharp
try
{
    // Business logic
    var result = await _organizationService.MethodAsync();
    return Ok(result);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error context with {Parameter}", value);
    return BadRequest(new { message = ex.Message });
}
```

---

## ?? Key Features

? **Explicit Action URIs** - Clear intent with `/add`, `/edit`, `/delete`  
? **ID at End** - Consistent parameter positioning  
? **Try-Catch All** - Comprehensive error handling on all 13 endpoints  
? **Structured Logging** - ILogger with context  
? **Type-Safe Responses** - ProducesResponseType attributes  
? **AutoWrapper Integration** - Standardized response format  
? **Hierarchical Sub-Resources** - Clear organization of records and workers

---

## ?? Endpoint Summary

| Region | Count | Endpoints |
|--------|-------|-----------|
| **Organization CRUD** | 7 | get-all, get/{id}, details/{id}, expiring-cards, add, edit/{id}, delete/{id} |
| **Organization Records** | 3 | records/add/{orgId}, records/edit/{orgId}/{recId}, records/delete/{orgId}/{recId} |
| **Organization Workers** | 3 | workers/add/{orgId}, workers/edit/{orgId}/{wrkId}, workers/delete/{orgId}/{wrkId} |
| **Total** | **13** | All with try-catch and logging |

---

**Print this card for quick reference!**
