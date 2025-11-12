# Client Controller URI Migration Guide

## Quick Reference: New URI Structure

| Endpoint | URI Pattern | HTTP Method |
|----------|-------------|-------------|
| **Get All** | `GET /api/clients/get-all` | HttpGet |
| **Get By ID** | `GET /api/clients/get/{id}` | HttpGet |
| **Create** | `POST /api/clients/add` | HttpPost |
| **Update** | `PUT /api/clients/edit/{id}` | HttpPut |
| **Delete** | `DELETE /api/clients/delete/{id}` | HttpDelete |
| **Get with Orgs** | `GET /api/clients/organizations/{id}` | HttpGet |
| **Create with Orgs** | `POST /api/clients/add-with-organizations` | HttpPost |
| **Update with Orgs** | `PUT /api/clients/edit-with-organizations/{id}` | HttpPut |
| **Get Creditors** | `GET /api/clients/creditors` | HttpGet |
| **Get Debtors** | `GET /api/clients/debtors` | HttpGet |

## URI Pattern Rules

### ? Action-Based Explicit URIs
- **Add/Create**: Use `/add` or `/add-with-organizations`
- **Edit/Update**: Use `/edit/{id}` or `/edit-with-organizations/{id}`
- **Delete**: Use `/delete/{id}`
- **Get**: Use `/get/{id}` for single item, `/get-all` for all items
- **Special queries**: Use descriptive names like `/creditors`, `/debtors`

### ? ID Position
- **Always at the end of the URI**
- Examples:
  - ? `/api/clients/edit/{id}`
  - ? `/api/clients/organizations/{id}`
  - ? `/api/clients/{id}/edit` (not used)

## Complete Endpoint Reference

### JavaScript/TypeScript

#### Service Class Example:
```javascript
class ClientService {
  baseUrl = '/api/clients';

  // Get all clients
  async getAll() {
    return fetch(`${this.baseUrl}/get-all`);
  }

  // Get client by ID
  async getById(id) {
    return fetch(`${this.baseUrl}/get/${id}`);
  }

  // Get client with organizations
  async getWithOrganizations(id) {
    return fetch(`${this.baseUrl}/organizations/${id}`);
  }

  // Create client
  async create(clientData) {
    return fetch(`${this.baseUrl}/add`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(clientData)
    });
  }

  // Update client
  async update(id, clientData) {
    return fetch(`${this.baseUrl}/edit/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(clientData)
    });
  }

  // Delete client
  async delete(id) {
    return fetch(`${this.baseUrl}/delete/${id}`, {
      method: 'DELETE'
    });
  }

  // Get creditors
  async getCreditors() {
    return fetch(`${this.baseUrl}/creditors`);
  }

  // Get debtors
  async getDebtors() {
    return fetch(`${this.baseUrl}/debtors`);
  }

  // Create client with organizations
  async createWithOrganizations(data) {
    return fetch(`${this.baseUrl}/add-with-organizations`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data)
    });
  }

  // Update client with organizations
  async updateWithOrganizations(id, data) {
    return fetch(`${this.baseUrl}/edit-with-organizations/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data)
    });
  }
}
```

### Angular Service

```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable()
export class ClientService {
  private baseUrl = '/api/clients';

  constructor(private http: HttpClient) {}

  getAll(): Observable<ClientDto[]> {
    return this.http.get<ClientDto[]>(`${this.baseUrl}/get-all`);
  }

  getById(id: string): Observable<ClientDto> {
    return this.http.get<ClientDto>(`${this.baseUrl}/get/${id}`);
  }

  getWithOrganizations(id: string): Observable<ClientDto> {
    return this.http.get<ClientDto>(`${this.baseUrl}/organizations/${id}`);
  }

  create(client: ClientCreateDto): Observable<ClientDto> {
    return this.http.post<ClientDto>(`${this.baseUrl}/add`, client);
  }

  update(id: string, client: ClientUpdateDto): Observable<ClientDto> {
    return this.http.put<ClientDto>(`${this.baseUrl}/edit/${id}`, client);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/delete/${id}`);
  }

  getCreditors(): Observable<ClientDto[]> {
    return this.http.get<ClientDto[]>(`${this.baseUrl}/creditors`);
  }

  getDebtors(): Observable<ClientDto[]> {
    return this.http.get<ClientDto[]>(`${this.baseUrl}/debtors`);
  }
}
```

### C# HttpClient Service

```csharp
public class ClientApiService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "/api/clients";

    public ClientApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse<List<ClientDto>>> GetAllAsync()
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/get-all");
        return await response.Content.ReadFromJsonAsync<ApiResponse<List<ClientDto>>>();
    }

    public async Task<ApiResponse<ClientDto>> GetByIdAsync(Guid id)
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/get/{id}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<ClientDto>>();
    }

    public async Task<ApiResponse<ClientDto>> GetWithOrganizationsAsync(Guid id)
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/organizations/{id}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<ClientDto>>();
    }

    public async Task<ApiResponse<ClientDto>> CreateAsync(ClientCreateDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/add", dto);
        return await response.Content.ReadFromJsonAsync<ApiResponse<ClientDto>>();
    }

    public async Task<ApiResponse<ClientDto>> UpdateAsync(Guid id, ClientUpdateDto dto)
    {
        var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/edit/{id}", dto);
        return await response.Content.ReadFromJsonAsync<ApiResponse<ClientDto>>();
    }

    public async Task<HttpResponseMessage> DeleteAsync(Guid id)
    {
        return await _httpClient.DeleteAsync($"{BaseUrl}/delete/{id}");
    }

    public async Task<ApiResponse<List<ClientDto>>> GetCreditorsAsync()
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/creditors");
        return await response.Content.ReadFromJsonAsync<ApiResponse<List<ClientDto>>>();
    }

    public async Task<ApiResponse<List<ClientDto>>> GetDebtorsAsync()
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/debtors");
        return await response.Content.ReadFromJsonAsync<ApiResponse<List<ClientDto>>>();
    }
}

// AutoWrapper Response Model
public class ApiResponse<T>
{
    public string Version { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public bool IsError { get; set; }
    public T Result { get; set; }
}
```

## Error Handling

All endpoints now return error details in a consistent format:

```json
{
  "version": "1.0",
  "statusCode": 400,
  "message": "Detailed exception message",
  "isError": true
}
```

### Handling Errors in Client Code:

```javascript
async function getClient(id) {
  try {
    const response = await fetch(`/api/clients/get/${id}`);
    const data = await response.json();
    
    if (data.isError) {
      console.error('Error:', data.message);
      return null;
    }
    
    return data.result;
  } catch (error) {
    console.error('Network error:', error);
    return null;
  }
}
```

## Postman Collection Update

### Update your Postman requests:

1. **Get All Clients**
   - URL: `{{baseUrl}}/api/clients/get-all`
   - Method: GET

2. **Get Client by ID**
   - URL: `{{baseUrl}}/api/clients/get/{{clientId}}`
   - Method: GET

3. **Create Client**
   - URL: `{{baseUrl}}/api/clients/add`
   - Method: POST

4. **Update Client**
   - URL: `{{baseUrl}}/api/clients/edit/{{clientId}}`
   - Method: PUT

5. **Delete Client**
   - URL: `{{baseUrl}}/api/clients/delete/{{clientId}}`
   - Method: DELETE

6. **Get Client with Organizations**
   - URL: `{{baseUrl}}/api/clients/organizations/{{clientId}}`
   - Method: GET

7. **Create Client with Organizations**
   - URL: `{{baseUrl}}/api/clients/add-with-organizations`
   - Method: POST

8. **Update Client with Organizations**
   - URL: `{{baseUrl}}/api/clients/edit-with-organizations/{{clientId}}`
   - Method: PUT

9. **Get Creditors**
   - URL: `{{baseUrl}}/api/clients/creditors`
   - Method: GET

10. **Get Debtors**
    - URL: `{{baseUrl}}/api/clients/debtors`
    - Method: GET

## Integration Test Updates

```csharp
public class ClientControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsAllClients()
    {
        var response = await _client.GetAsync("/api/clients/get-all");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetById_ReturnsClient()
    {
        var id = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/clients/get/{id}");
        // Assertions...
    }

    [Fact]
    public async Task Create_ReturnsCreatedClient()
    {
        var dto = new ClientCreateDto { /* ... */ };
        var response = await _client.PostAsJsonAsync("/api/clients/add", dto);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Update_ReturnsUpdatedClient()
    {
        var id = Guid.NewGuid();
        var dto = new ClientUpdateDto { /* ... */ };
        var response = await _client.PutAsJsonAsync($"/api/clients/edit/{id}", dto);
        // Assertions...
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        var id = Guid.NewGuid();
        var response = await _client.DeleteAsync($"/api/clients/delete/{id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
```

## Key Differences Summary

### URI Pattern:
- ? **Action-based**: Clear intent with action verbs
- ? **ID at end**: Consistent parameter position
- ? **Explicit routes**: `/add`, `/edit/{id}`, `/delete/{id}`, `/get-all`

### Error Handling:
- ? **All endpoints** have try-catch blocks
- ? **Logged errors** with structured logging
- ? **Consistent error format** with AutoWrapper

---

**Migration Status:** Ready for implementation  
**URI Pattern:** Action-Based Explicit  
**Error Handling:** Comprehensive  
**Logging:** Structured with ILogger
