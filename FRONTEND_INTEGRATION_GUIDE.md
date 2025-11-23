# ?? Frontend Integration Guide - Firebase Authentication

## Quick Reference for Angular Frontend Team

---

## ?? API Endpoints Summary

### Base URL
```
https://your-backend-api.com/api
```

### Authentication Endpoints

| Endpoint | Method | Description | Auth Required |
|----------|--------|-------------|---------------|
| `/auth/login` | POST | Login with Firebase ID token | No |
| `/auth/logout` | POST | Logout user | No |
| `/auth/me` | GET | Get current user data | Yes |

---

## ?? Login Flow

### 1. Frontend: Get Firebase ID Token

```typescript
// In your sign-in.component.ts
async signIn(email: string, password: string) {
  try {
    // Sign in with Firebase
    const userCredential = await this.afAuth.signInWithEmailAndPassword(email, password);
    const user = userCredential.user;
    
    // Get ID token
    const idToken = await user.getIdToken();
    
    // Call backend login
    await this.callBackendLogin(idToken);
  } catch (error) {
    console.error('Login error:', error);
  }
}
```

### 2. Call Backend Login API

```typescript
async callBackendLogin(idToken: string) {
  const response = await this.http.post<any>(`${ApiUrl.login}`, {
    idToken: idToken
  }).toPromise();
  
  if (response && response.result) {
    // Store user data
    localStorage.setItem('userData', JSON.stringify(response.result));
    
    // Navigate to dashboard
    this.router.navigate(['/dashboard']);
  }
}
```

---

## ?? Request Format

### Login Request
```json
POST /api/auth/login
Content-Type: application/json

{
  "idToken": "eyJhbGciOiJSUzI1NiIsImtpZCI6Ij..."
}
```

### Logout Request
```json
POST /api/auth/logout
Content-Type: application/json

{
  "idToken": "eyJhbGciOiJSUzI1NiIsImtpZCI6Ij..."
}
```

---

## ?? Response Format (with AutoWrapper)

### Successful Login
```json
{
  "version": "1.0",
  "statusCode": 200,
  "message": "POST Request successful.",
  "isError": false,
  "result": {
    "id": 1,
    "email": "user@example.com",
    "name": "John Doe",
    "firebaseUid": "abc123xyz",
    "role": "User",
    "organizationId": null,
    "isActive": true,
    "createdAt": "2024-01-15T10:30:00Z",
    "lastLoginAt": "2024-01-15T10:30:00Z"
  }
}
```

### Failed Login (Invalid Token)
```json
{
  "version": "1.0",
  "statusCode": 401,
  "message": "Invalid or expired Firebase token",
  "isError": true
}
```

### Failed Login (User Not Authorized)
```json
{
  "version": "1.0",
  "statusCode": 403,
  "message": "You Are Not Authorized",
  "isError": true
}
```

### Successful Logout
```json
{
  "version": "1.0",
  "statusCode": 200,
  "message": "POST Request successful.",
  "isError": false,
  "result": {
    "success": true,
    "message": "Logged out successfully"
  }
}
```

---

## ??? Complete Angular Service Example

```typescript
// auth.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AngularFireAuth } from '@angular/fire/compat/auth';
import { Router } from '@angular/router';
import { ApiUrl } from '../environments/api-url';

interface LoginResponse {
  version: string;
  statusCode: number;
  message: string;
  isError: boolean;
  result?: UserData;
}

interface UserData {
  id: number;
  email: string;
  name: string;
  firebaseUid: string;
  role: string;
  organizationId?: string;
  isActive: boolean;
  createdAt: string;
  lastLoginAt?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  constructor(
    private http: HttpClient,
    private afAuth: AngularFireAuth,
    private router: Router
  ) {}

  /**
   * Sign in with email and password
   */
  async signIn(email: string, password: string): Promise<void> {
    try {
      // Firebase authentication
      const userCredential = await this.afAuth.signInWithEmailAndPassword(email, password);
      const user = userCredential.user;

      if (!user) {
        throw new Error('No user returned from Firebase');
      }

      // Get Firebase ID token
      const idToken = await user.getIdToken();

      // Store Firebase token
      localStorage.setItem('accessToken', idToken);

      // Call backend login
      const response = await this.http.post<LoginResponse>(ApiUrl.login, {
        idToken: idToken
      }).toPromise();

      if (response && !response.isError && response.result) {
        // Store user data
        localStorage.setItem('userData', JSON.stringify(response.result));

        // Navigate to dashboard
        this.router.navigate(['/dashboard']);
      } else {
        throw new Error(response?.message || 'Login failed');
      }
    } catch (error: any) {
      console.error('Sign-in error:', error);
      
      // Handle specific error messages
      if (error?.error?.message === 'You Are Not Authorized') {
        alert('You Are Not Authorized');
      } else {
        alert('Invalid credentials or login error');
      }
      
      throw error;
    }
  }

  /**
   * Sign out
   */
  async signOut(): Promise<void> {
    try {
      // Get current token
      const idToken = localStorage.getItem('accessToken');

      // Call backend logout (optional)
      if (idToken) {
        await this.http.post(ApiUrl.logout, {
          idToken: idToken
        }).toPromise();
      }

      // Firebase sign out
      await this.afAuth.signOut();

      // Clear local storage
      localStorage.removeItem('accessToken');
      localStorage.removeItem('userData');
      localStorage.removeItem('fcmToken');

      // Navigate to login
      this.router.navigate(['/login']);
    } catch (error) {
      console.error('Sign-out error:', error);
      // Always clear local storage and redirect even if API call fails
      localStorage.clear();
      this.router.navigate(['/login']);
    }
  }

  /**
   * Get current user from localStorage
   */
  getCurrentUser(): UserData | null {
    const userDataStr = localStorage.getItem('userData');
    if (userDataStr) {
      try {
        return JSON.parse(userDataStr);
      } catch {
        return null;
      }
    }
    return null;
  }

  /**
   * Check if user is authenticated
   */
  isAuthenticated(): boolean {
    return !!localStorage.getItem('accessToken');
  }

  /**
   * Refresh Firebase token and update backend
   */
  async refreshToken(): Promise<void> {
    try {
      const user = await this.afAuth.currentUser;
      if (user) {
        const newToken = await user.getIdToken(true);
        localStorage.setItem('accessToken', newToken);

        // Optionally re-login to update backend
        const response = await this.http.post<LoginResponse>(ApiUrl.login, {
          idToken: newToken
        }).toPromise();

        if (response && !response.isError && response.result) {
          localStorage.setItem('userData', JSON.stringify(response.result));
        }
      }
    } catch (error) {
      console.error('Token refresh error:', error);
      await this.signOut();
    }
  }
}
```

---

## ?? HTTP Interceptor for Protected Requests

```typescript
// auth.interceptor.ts
import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private router: Router) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    // Get token from localStorage
    const token = localStorage.getItem('accessToken');

    // Clone request and add authorization header if token exists
    if (token) {
      request = request.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
    }

    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401) {
          // Token expired or invalid - redirect to login
          localStorage.clear();
          this.router.navigate(['/login']);
        }
        return throwError(() => error);
      })
    );
  }
}
```

### Register Interceptor in app.module.ts

```typescript
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { AuthInterceptor } from './interceptors/auth.interceptor';

@NgModule({
  // ...
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    }
  ]
})
export class AppModule { }
```

---

## ??? Auth Guard (Already Implemented)

Your existing AuthGuard should work with the new backend:

```typescript
// auth.guard.ts
import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AngularFireAuth } from '@angular/fire/compat/auth';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(
    private afAuth: AngularFireAuth,
    private router: Router
  ) {}

  async canActivate(): Promise<boolean> {
    const user = await this.afAuth.currentUser;
    const token = localStorage.getItem('accessToken');

    if (user && token) {
      return true;
    } else {
      this.router.navigate(['/login']);
      return false;
    }
  }
}
```

---

## ?? What to Store in localStorage

```typescript
// After successful login:
localStorage.setItem('accessToken', firebaseIdToken);      // Firebase ID token
localStorage.setItem('userData', JSON.stringify(userData)); // User data from backend
localStorage.setItem('fcmToken', fcmToken);                 // FCM token (if using notifications)
```

```typescript
// After logout:
localStorage.removeItem('accessToken');
localStorage.removeItem('userData');
localStorage.removeItem('fcmToken');
// OR
localStorage.clear();
```

---

## ?? Important Notes

### 1. Token Expiration
- Firebase ID tokens expire after **1 hour**
- Frontend should refresh token automatically using `getIdToken(true)`
- If token expired, backend returns `401 Unauthorized`
- Interceptor should catch 401 and redirect to login

### 2. User Auto-Creation
- First time a user logs in, backend automatically creates a User record
- Default role is "User"
- Default IsActive is true
- If you need custom roles/permissions, set them in database after user is created

### 3. Error Handling
- Always check `response.isError` in API responses
- Handle `statusCode: 401` (Invalid token)
- Handle `statusCode: 403` (User not authorized)
- Show appropriate error messages to users

### 4. CORS
- Backend has "AllowAll" CORS policy configured
- Works with `http://localhost:4200` for development
- For production, update CORS policy to specific origins

---

## ?? Testing Checklist

- [ ] User can sign in with valid email/password
- [ ] After sign-in, `/api/auth/login` is called successfully
- [ ] User data is stored in localStorage
- [ ] User is redirected to `/dashboard` after login
- [ ] Protected routes require authentication
- [ ] Unauthenticated users are redirected to `/login`
- [ ] User can sign out successfully
- [ ] After sign-out, localStorage is cleared
- [ ] Token is sent in Authorization header for protected requests
- [ ] Expired tokens trigger logout and redirect to login
- [ ] Error messages are displayed correctly

---

## ?? Common Issues & Solutions

### Issue: "Invalid or expired Firebase token"
**Solution**: 
- Token might have expired (1 hour lifetime)
- Call `getIdToken(true)` to force refresh
- Then retry backend login

### Issue: "You Are Not Authorized"
**Solution**:
- User exists but IsActive = false in database
- Admin needs to activate the user account
- Check Users table in database

### Issue: CORS error
**Solution**:
- Backend has CORS configured for all origins
- If still getting CORS error, check backend is running
- Check API URL is correct

### Issue: 401 on protected endpoints
**Solution**:
- Ensure Authorization header is being sent
- Check token format: `Bearer {token}`
- Verify HTTP Interceptor is registered
- Check token hasn't expired

---

## ?? Contact Backend Team If:

- You need additional user fields in the response
- You need to change default user role
- You need custom authorization logic
- You encounter any backend errors
- You need help with endpoint integration

---

**Happy Coding! ??**

