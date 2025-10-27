# Use swiyu with Duende identity provider using Aspire and ASP.NET Core

[![.NET](https://github.com/swiss-ssi-group/swiyu-idp-aspire-aspnetcore/actions/workflows/dotnet.yml/badge.svg)](https://github.com/swiss-ssi-group/swiyu-idp-aspire-aspnetcore/actions/workflows/dotnet.yml)

[Use swiyu, the Swiss E-ID to authenticate users with Duende and .NET Aspire](https://damienbod.com/2025/10/27/use-swiyu-the-swiss-e-id-to-authenticate-users-with-duende-and-net-aspire/)

## Architecture overview

A Duende identity server is used as an OpenID Connect server for web applications. When the user authenticates, the Swiss E-ID can be used to authenticate.
The applications are implemented using Aspire, ASP.NET Core and the Swiss public beta generic containers. The containers implement the OpenID verifiable credential standards and provide a simple API to integrate applications. Using swiyu is simple, but not a good way of doing authentication as it is not phishing resistant.

![Architecture](https://github.com/swiss-ssi-group/swiyu-idp-aspire-aspnetcore/blob/main/images/overview.drawio.png)

## Getting started:

- [swiyu](https://swiyu-admin-ch.github.io/cookbooks/onboarding-base-and-trust-registry/)
- [Dev docs](DEV.md)
- [Changelog](CHANGELOG.md)

## Used OSS packages, containers, repositories 

- ImageMagick: https://github.com/manuelbl/QrCodeGenerator/tree/master/Demo-ImageMagick
- Microsoft Aspire: https://learn.microsoft.com/en-us/dotnet/aspire/get-started/aspire-overview
- Net.Codecrete.QrCodeGenerator: https://github.com/manuelbl/QrCodeGenerator/
- swiyu
  - https://github.com/swiyu-admin-ch/swiyu-verifier

## Register Flow

Used data:  given_name, family_name, birth_date and birth_place.

1. User has already an account and would like to attach an E-ID for authentication
2. User registers
3. User validates authentication using E-ID
4. User password authentication disabled

> Note: authentication uses E-ID is NOT phishing resistant. Passkeys would be better.

## Authentication Flow

> Note: authentication uses E-ID is NOT phishing resistant. Passkeys would be better.

## Recovery flow (name change)

## 2FA flow

## Password reset flow

## Links

https://swiyu-admin-ch.github.io/cookbooks/how-to-use-beta-id/

https://swiyu-admin-ch.github.io/cookbooks/onboarding-generic-verifier/

https://swiyu-admin-ch.github.io/

https://www.eid.admin.ch/en/public-beta-e

https://learn.microsoft.com/en-us/dotnet/aspire/get-started/aspire-overview

https://www.npmjs.com/package/ngrok

https://swiyu-admin-ch.github.io/specifications/interoperability-profile/

https://andrewlock.net/converting-a-docker-compose-file-to-aspire/

https://swiyu-admin-ch.github.io/cookbooks/onboarding-generic-verifier/

https://github.com/orgs/swiyu-admin-ch/projects/2/views/2

Standards

https://identity.foundation/trustdidweb/

https://openid.net/specs/openid-4-verifiable-credential-issuance-1_0.html

https://openid.net/specs/openid-4-verifiable-presentations-1_0.html

https://datatracker.ietf.org/doc/draft-ietf-oauth-selective-disclosure-jwt/

https://datatracker.ietf.org/doc/draft-ietf-oauth-sd-jwt-vc/

https://datatracker.ietf.org/doc/draft-ietf-oauth-status-list/

https://www.w3.org/TR/vc-data-model-2.0/
