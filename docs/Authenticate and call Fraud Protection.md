# Microsoft Dynamics 365 Fraud Protection - API Examples
## Authenticate and call Fraud Protection

You must send an authentication token with Dynamics 365 Fraud Protection (Fraud Protection) API calls. The following example shows one way to do that and then walks you through creating a request and handling the response.

## Helpful links
- [API spec](https://apidocs.microsoft.com/services/graphriskapi)
- [Sample Site - Fraud Protection service](../src/Infrastructure/Services/FraudProtectionService.cs)

## Authenticate with Fraud Protection API
This example requires the following data to obtain a bearer token:

1. **Authority**: Your OAuth authority URL to authenticate against.
1. **Client ID**: Your Fraud Protection merchant application's ID in Azure Active Directory.
1. **Certificate**: The private certificate you've selected and placed its public portion in your Fraud Protection merchant app in Azure Active Directory.

```csharp
public class TokenProviderService : ITokenProvider
{
    private TokenProviderServiceSettings _settings;

    public TokenProviderService(IOptions<TokenProviderServiceSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<string> AcquireTokenAsync(string resource)
    {
        var assertionCert = CertificateUtility.GetByThumbprint(_settings.CertificateThumbprint);
        var clientAssertion = new ClientAssertionCertificate(_settings.ClientId, assertionCert);
        
        var context = new AuthenticationContext(_settings.Authority);
        var authenticationResult = await context.AcquireTokenAsync(resource, clientAssertion);

        return authenticationResult.AccessToken;
    }
}
```
## Send events to Fraud Protection
All events sent to Fraud Protection follow the same JSON model:
```json
{
    merchantLocalDate = <event date in ISO 8601 format>,
    Data = <event data>
}
```

> The update account event is the only exception, also requiring a ```customerLocalDate``` top level property.

The event data model varies from one event type to the next, though.

For example, see the following request and response when sending a refund event to Fraud Protection.

### Request

```http
POST https://api.dfp.microsoft.com/KnowledgeGateway/activities/Refund HTTP/1.1
Host: api.dfp.microsoft.com
Content-Type: application/json; charset=utf-8
x-ms-correlation-id: <correlation ID>
x-ms-tracking-id: <tracking ID>
Authorization: bearer <token>
Content-Length: <content length>

{
  "MerchantLocalDate": "<event date in ISO 8601 format>",
  "Data": {
    "RefundId": "<refund ID>",
    "Purchase": { "PurchaseId": "<purchase ID>" },
    "User": { "UserId": "<user ID>" },
    "Status": "COMPLETED",
    "BankEventTimestamp": "<refund date from bank in ISO 8601 format>",
    "Amount": 23.79,
    "Currency": "USD"
  }
}
```
### Response
```http
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
Date: <date>
Content-Length: 27

{
  "resultDetails": {}
}
```

### Error response example 1
This response is the result of sending an expired/invalid bearer token:
```http
HTTP/1.1 401 Unauthorized
Content-Type: application/json
ErrorReason: TokenExpired
Date: Thu, 08 Nov 2018 08:57:07 GMT
Content-Length: 85

{
    "statusCode": 401,
    "message": "Unauthorized. Access token is missing or invalid."
}
```
### Error response example 2
This response is the result of sending a refund event without the required "user" property:
```http
HTTP/1.1 400 Bad Request
Content-Type: application/json
Date: Thu, 08 Nov 2018 08:44:02 GMT
Content-Length: 148

{
  "Code": null,
  "ErrorType": "ClientError",
  "Message": "Entity of type User is expected at //User",
  "Retryable": false,
  "Parameters": null,
  "OtherErrors": null
}
```
### Error response example 3
This response represents an intermittent error. In this case, retrying the request may be successful:
```http
HTTP/1.1 500 Internal Server Error
Content-Type: application/json
Date: Thu, 08 Nov 2018 09:01:51 GMT
Content-Length: 171

{
  "Code": null,
  "ErrorType": "ServerError",
  "Message": "Internal Server Error - please try again",
  "Retryable": true,
  "Parameters": null,
  "OtherErrors": null
}
```