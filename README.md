# Swiyu identity provider using Aspire and ASP.NET Core

## Architecture overview

![Architecture](https://github.com/swiss-ssi-group/swiyu-aspire-aspnetcore/blob/main/images/overview.drawio.png)

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

- User has already an account and would like to attach an E-ID for authentication
- User registers
- User validates authentication using E-ID
- User password authentication disabled

> Note: authentication uses E-ID is NOT phishing resistant. Passkeys would be better.

## Authentication Flow

## Recovery flow (name change)

## Links

https://swiyu-admin-ch.github.io/cookbooks/how-to-use-beta-id/

https://swiyu-admin-ch.github.io/cookbooks/onboarding-generic-verifier/

