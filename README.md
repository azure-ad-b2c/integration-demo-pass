# Sample Integration between Azure AD B2C and PASS

This repository contains code which demonstrates an integration between [Azure AD B2C](https://docs.microsoft.com/en-us/azure/active-directory-b2c/overview) and [PASS](https://www.passlogin.com/), a Korean mobile phone-based authentication service.

This integration is powered by the Identity Experience Framework in Azure AD B2C. For more information on TrustFramework Policies and the Identity Experience Framework, see the [Azure AD B2C documentation](https://docs.microsoft.com/en-us/azure/active-directory-b2c/custom-policy-overview).

## Demo site
In the `src/` folder is the source code for a simple website which uses OpenID Connect to authenticate using two Azure AD B2C policies which demonstrate different types of integration options with IPification.

This website hosts a single REST API endpoint which is consumed by the Azure AD B2C policies in order to decrypt the claims which are returned by the PASS OAuth2 service.

The website also hosts a login template page which is used by the Azure AD B2C policies for UI customization.

You can find the demo site at: https://b2c-pass-demo.azurewebsites.net

## Identity Experience Framework Policies

In the `policies/` folder you'll find custom policy definitions for integration with PASS:

### PASS as an authentication provider

The policies demonstrate how to connect with PASS as an identity provider in a similar way to a connection to social login services. Azure AD B2C authenticates with PASS using the OAuth2 protocol. PASS authenticates the user by prompting them for a biometric or pin-based challenge and consent approval on their mobile device which has the PASS application installed.

This scenario is based on the [`SocialAndLocalAccountsWithMfa` starter pack](https://github.com/Azure-Samples/active-directory-b2c-custom-policy-starterpack/tree/master/SocialAndLocalAccountsWithMfa) although local account authentication has been removed in this sample.

## Deployment

This repository uses GitHub Actions to deploy both the Azure AD B2C policies and the website. You can find the deployment workflows in `.github/workflows/`.

## Community Help and Support
Use [Stack Overflow](https://stackoverflow.com/questions/tagged/azure-ad-b2c) to get support from the community. Ask your questions on Stack Overflow first and browse existing issues to see if someone has asked your question before. Make sure that your questions or comments are tagged with [azure-ad-b2c].

If you find a bug in the sample, please raise the issue on [GitHub Issues](https://github.com/azure-ad-b2c/deploy-trustframework-policy/issues).

To provide product feedback, visit the Azure AD B2C [feedback page](https://feedback.azure.com/forums/169401-azure-active-directory?category_id=160596).