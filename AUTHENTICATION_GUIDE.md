# Neha Surgical API - Dual Authentication System

## Overview
This API uses a **dual-layer authentication system**:
1. **API Key** - Identifies and authenticates the frontend application
2. **JWT Token** - Identifies and authenticates the logged-in user

## Setup Instructions

### 1. Install Required Packages
The following NuGet packages have been added:
- `BCrypt.Net-Next` (4.0.3) - Password hashing
- `Microsoft.AspNetCore.Authentication.JwtBearer` (8.0.0) - JWT authentication
- `System.IdentityModel.Tokens.Jwt` (7.0.0) - JWT token generation

### 2. Run Database Scripts
Execute these SQL scripts in order:

```sql
-- 1. Create API Users table (for frontend applications)
Database/CreateApiUsersTable.sql

-- 2. Create System Users table (for actual users)
Database/CreateSystemUsersTable.sql
```

### 3. Default Credentials

**Frontend Applications (API Keys):**
- Admin Portal: `NS-2025-ADMIN-PORTAL-KEY`
- Web App: `NS-2025-WEB-APP-KEY`
- Mobile App: `NS-2025-MOBILE-APP-KEY`
- Doctor Portal: `NS-2025-DOCTOR-PORTAL-KEY`

**System Users (for login):**
- Email: `admin@nehasurgical.com` | Password: `Admin@123`
- Email: `john@nehasurgical.com` | Password: `Admin@123`
- Email: `mary@nehasurgical.com` | Password: `Admin@123`

## How It Works

### Authentication Flow

```
Frontend App → Sends X-API-Key header → API validates → Allows access to /login
User → Calls /api/auth/login → Gets JWT token → Stores token
User → Makes API calls → Sends both X-API-Key + Bearer Token → API validates both
```

### Required Headers for API Calls

```http
X-API-Key: NS-2025-WEB-APP-KEY
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## API Endpoints

### Authentication Endpoints

#### 1. Login (Get JWT Token)
```http
POST /api/auth/login
Headers: X-API-Key: NS-2025-WEB-APP-KEY
Body: {
  "email": "admin@nehasurgical.com",
  "password": "Admin@123"
}

Response: {
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "abc123...",
    "expiresAt": "2025-12-06T10:30:00Z",
    "user": {
      "systemUserId": 1,
      "fullName": "System Admin",
      "email": "admin@nehasurgical.com",
      "role": "Admin",
      "isActive": "Y"
    }
  }
}
```

#### 2. Register New User
```http
POST /api/auth/register
Headers: 
  X-API-Key: NS-2025-ADMIN-PORTAL-KEY
  Authorization: Bearer {admin_token}
Body: {
  "fullName": "New User",
  "email": "newuser@nehasurgical.com",
  "password": "SecurePass123",
  "role": "User"
}
```

#### 3. Get Current User Info
```http
GET /api/auth/me
Headers: 
  X-API-Key: NS-2025-WEB-APP-KEY
  Authorization: Bearer {token}
```

#### 4. Change Password
```http
POST /api/auth/change-password
Headers: 
  X-API-Key: NS-2025-WEB-APP-KEY
  Authorization: Bearer {token}
Body: {
  "currentPassword": "Admin@123",
  "newPassword": "NewSecurePass456"
}
```

## Frontend Integration Examples

### React/JavaScript Example

```javascript
// 1. Configure API client with API Key
const API_KEY = 'NS-2025-WEB-APP-KEY';
const API_BASE_URL = 'https://api.nehasurgical.com';

// 2. Login function
async function login(email, password) {
  const response = await fetch(`${API_BASE_URL}/api/auth/login`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'X-API-Key': API_KEY
    },
    body: JSON.stringify({ email, password })
  });
  
  const result = await response.json();
  
  if (response.ok) {
    // Store JWT token
    localStorage.setItem('jwt_token', result.data.token);
    localStorage.setItem('user', JSON.stringify(result.data.user));
    return result.data;
  } else {
    throw new Error(result.message);
  }
}

// 3. API call with both API Key and JWT Token
async function getDoctors() {
  const token = localStorage.getItem('jwt_token');
  
  const response = await fetch(`${API_BASE_URL}/api/doctors`, {
    headers: {
      'X-API-Key': API_KEY,
      'Authorization': `Bearer ${token}`
    }
  });
  
  return await response.json();
}

// 4. Axios configuration (recommended)
import axios from 'axios';

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'X-API-Key': API_KEY
  }
});

// Add JWT token to all requests
apiClient.interceptors.request.use(config => {
  const token = localStorage.getItem('jwt_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Usage
await apiClient.post('/api/auth/login', { email, password });
await apiClient.get('/api/doctors');
```

### Mobile App (React Native) Example

```javascript
import AsyncStorage from '@react-native-async-storage/async-storage';

const API_KEY = 'NS-2025-MOBILE-APP-KEY';

// Login
async function login(email, password) {
  const response = await fetch('https://api.nehasurgical.com/api/auth/login', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'X-API-Key': API_KEY
    },
    body: JSON.stringify({ email, password })
  });
  
  const result = await response.json();
  await AsyncStorage.setItem('jwt_token', result.data.token);
  return result.data;
}

// API call
async function fetchData(endpoint) {
  const token = await AsyncStorage.getItem('jwt_token');
  
  const response = await fetch(`https://api.nehasurgical.com${endpoint}`, {
    headers: {
      'X-API-Key': API_KEY,
      'Authorization': `Bearer ${token}`
    }
  });
  
  return await response.json();
}
```

## Security Features

1. **Dual Authentication**
   - API Key validates the frontend application
   - JWT Token validates the user session
   - Both are required for protected endpoints

2. **Password Security**
   - Passwords are hashed using BCrypt
   - Never stored in plain text
   - Salted and hashed with 11 rounds

3. **JWT Token Features**
   - Expires after 24 hours
   - Contains user ID, email, and role
   - Signed with secret key
   - Cannot be tampered with

4. **Role-Based Access**
   - User role stored in JWT claims
   - Can be used for authorization logic
   - Roles: Admin, Doctor, Staff, User

## Testing in Swagger

1. **Access Swagger UI**
   - Go to: `http://localhost:5280/swagger`
   - Enter passkey: `nehaSurgical@2025`

2. **Authenticate in Swagger**
   - Click "Authorize" button (🔒)
   - Enter API Key: `NS-2025-ADMIN-PORTAL-KEY`
   - Login via `/api/auth/login` to get JWT token
   - Copy the token from response
   - Click "Authorize" again
   - Enter: `Bearer {your_token}` in the Bearer field
   - Now you can test all endpoints

## User Context in Controllers

Both API User and System User information are available in controllers:

```csharp
// In your controller actions
var apiUserId = HttpContext.Items["ApiUserId"]; // Which frontend app
var apiUsername = HttpContext.Items["ApiUsername"]; // e.g., "web_app"

var systemUserId = HttpContext.Items["SystemUserId"]; // Which user
var systemUserEmail = HttpContext.Items["SystemUserEmail"]; // User's email
var systemUserRole = HttpContext.Items["SystemUserRole"]; // User's role

// Or use [Authorize] attribute and User.Claims
var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
var email = User.FindFirst(ClaimTypes.Email)?.Value;
var role = User.FindFirst(ClaimTypes.Role)?.Value;
```

## Troubleshooting

### "API Key is missing"
- Ensure you're sending `X-API-Key` header
- Check the header name is exactly `X-API-Key` (case-sensitive)

### "Invalid API Key"
- Verify the API key exists in `api_users` table
- Check if `is_active = 'Y'` for that API user

### "Unauthorized" on protected endpoints
- Ensure you have both API Key AND JWT token
- Check if token has expired (24 hours)
- Verify token format: `Bearer {token}`

### "Invalid email or password"
- Check credentials in `system_users` table
- Password may need to be reset using BCrypt hash generator
- Use `/api/auth/register` to create new users with proper hashing

## Managing Users

### Add New Frontend Application
```sql
INSERT INTO api_users (username, api_key, role, is_active) 
VALUES ('partner_app', 'NS-2025-PARTNER-APP-KEY', 'User', 'Y');
```

### Add New System User (via API)
```http
POST /api/auth/register
Body: {
  "fullName": "New Employee",
  "email": "employee@nehasurgical.com",
  "password": "TempPass123",
  "role": "Staff"
}
```

### Deactivate User
```sql
UPDATE system_users SET is_active = 'N' WHERE email = 'user@example.com';
```

### Regenerate API Key (if compromised)
```sql
UPDATE api_users SET api_key = 'NS-2025-NEW-KEY-HERE' WHERE username = 'web_app';
```

## Production Considerations

1. **Change Default Credentials**
   - Update all API keys before deployment
   - Reset all default passwords

2. **Use HTTPS**
   - Never send tokens over HTTP
   - Enable SSL/TLS in production

3. **Secure JWT Secret**
   - Use a strong, random secret key
   - Store in environment variables, not appsettings.json

4. **Token Expiry**
   - Configure appropriate expiry time
   - Implement refresh token mechanism

5. **Rate Limiting**
   - Add rate limiting to prevent abuse
   - Especially for login endpoint

6. **Logging**
   - Log all authentication attempts
   - Monitor for suspicious activity
