# Multi-Factor Authentication (MFA) Core Engine

A production-grade, secure **Multi-Factor Authentication (MFA)** microservice backend built from scratch with **.NET 8/9** and **Visual Studio 2022**. This system implements standalone **Time-Based One-Time Passwords (TOTP)** matching industry standards (RFC 6238) along with secure, single-use **BCrypt-hashed Account Recovery Codes**.

---

## 🛠️ Tech Stack & Key Dependencies

*   **Runtime Framework:** .NET 8.0 / .NET 9.0 Web API
*   **IDE:** Visual Studio 2022
*   **Core Logic (TOTP):** `Otp.NET` (Handles cryptographic windowing and alignment)
*   **Visual Generation:** `QRCoder` (Converts schema strings into on-the-fly PNG graphics)
*   **Security Hashing:** `BCrypt.Net-Next` (Salts and hashes recovery backup arrays)

---

## 📦 System Architecture & Folder Layout

The code follows a structured **Layered Architecture** to keep data representations, verification mathematical logic, and controller routes decoupled.

```text
📦 MfaSystemDemo
 ┣ 📂 Controllers
 ┃ ┗ 📜 AuthController.cs      # Exposes setup endpoints, visual streams, and verification
 ┣ 📂 Models
 ┃ ┣ 📜 UserDbRecord.cs        # Main user configuration data layout
 ┃ ┗ 📜 VerifyRequest.cs       # Contract structure defining payload signatures
 ┣ 📂 Services
 ┃ ┗ 📜 MfaService.cs          # Encapsulates cryptographic calculations, validation, & QR rendering
 ┣ 📜 appsettings.json         # Stores environmental settings like the application display name
 ┣ 📜 MfaSystemDemo.csproj     # Project dependencies manifest file
 ┗ 📜 Program.cs               # Service bootstrap lifecycle manager
```

---

## 🚀 Setup & Installation Instructions

### 1. Prerequisites
Ensure you have **Visual Studio 2022** installed along with the **ASP.NET and web development** workload toolsets.

### 2. Restoring Packages
Open the solution inside Visual Studio. Execute these explicit dependency calls inside the **Package Manager Console** (`Tools > NuGet Package Manager > Package Manager Console`):

```shell
Install-Package Otp.NET
Install-Package QRCoder
Install-Package BCrypt.Net-Next
```

### 3. Application Configurations
Open `appsettings.json` and adjust the app metadata properties. The system dynamically streams this value directly into users' verification phone apps:

```json
{
  "MfaSettings": {
    "Issuer": "My Secure Enterprise App"
  }
}
```

---

## 🧪 Detailed Step-by-Step Testing Guide

Press **F5** to start the web application. A Swagger interactive documentation interface will automatically launch in your browser at `https://localhost:xxxx/swagger/index.html`.

### Phase A: Requesting Credentials & Enrollment Initialization
1.  Navigate to **`POST /api/Auth/setup`**.
2.  Click **Try it out** and type your email payload identifier (e.g., `"john.doe@example.com"`).
3.  Click **Execute**.
4.  **Save the outputs:**
    *   Copy the string literal inside `"secret"`.
    *   Temporarily copy the array of strings generated under `"recoveryCodes"`.

### Phase B: Registering Your Authenticator Device
*   **Option 1 (Using Text Secret):** Open Google or Microsoft Authenticator. Click **Add Account**, select **Enter a setup key manually**, type your key, and save.
*   **Option 2 (Using Visual QR Image):** Expand the **`GET /api/Auth/setup-qr-image`** block in Swagger. Click **Try it out**, fill in the exact email parameter, and click **Execute**. Point your phone's camera at the printed visual image code appearing directly inside your browser.

### Phase C: Finalizing Security Activation
The engine flags accounts as disabled until you execute confirmation.
1.  Look at the flashing 6-digit number changing on your phone screen.
2.  Expand **`POST /api/Auth/verify-setup`** inside Swagger.
3.  Click **Try it out** and construct your input verification payload:
    ```json
    {
      "email": "john.doe@example.com",
      "code": "YOUR_LIVE_6_DIGIT_PHONE_CODE"
    }
    ```
4.  Click **Execute**. A success code `200 OK` confirms your registration is fully activated.

### Phase D: Step-Two Runtime Authentication Testing
1.  Expand **`POST /api/Auth/login-mfa`**.
2.  Provide your profile email and your newest app sequence digits. Click **Execute** to view your authenticated session profile access confirmation.
3.  **To Test Account Recovery:** Instead of using your phone app's 6 digits, paste one of your copied multi-character backup hashes (e.g., `ABCD-1234`) directly into the `"code"` field. Hit execute. The system consumes and completely invalidates that single-use code to guarantee high-integrity protection.

---

## 🔒 Security Operations Blueprint

*   **Time Shift Defenses:** The validation core implements `VerificationWindow.RfcSpecifiedNetworkDelay` inside `MfaService`. This automatically accommodates network latency and clock drifts up to ±30 seconds.
*   **Immutable Destruction Strategy:** Backup codes are destroyed immediately upon successful verification, rendering stolen logs or duplicate attacks useless.
*   **Storage Compliance:** Secrets are separated into discrete strings. For production rollouts, replace the mock `ConcurrentDictionary` with a formal relational database and apply encryption to the `SecretKey` column using a Key Management Service (KMS).
