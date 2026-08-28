# API Changelog 1.0.0-draft.20 vs. 1.0.0-draft.22


## API Changes

### GET /account-roles
- :warning: api path removed without deprecation


### GET /accounts
- :warning: api path removed without deprecation


### POST /accounts
- :warning: api path removed without deprecation


### GET /accounts/me
- :warning: api path removed without deprecation


### DELETE /accounts/{accountId}
- :warning: api path removed without deprecation


### GET /accounts/{accountId}
- :warning: api path removed without deprecation


### PATCH /accounts/{accountId}
- :warning: api path removed without deprecation


### POST /accounts/{accountId}:reset-password
- :warning: api path removed without deprecation


### GET /auth/session
- :warning: api path removed without deprecation


### POST /auth/sign-in
- :warning: api path removed without deprecation


### POST /auth/sign-out
- :warning: api path removed without deprecation


### POST /tasks
-  request property `customer_trust_status` deprecated
-  request property `trusted_skip_allowed` deprecated




## Components
-  removed the schema `ConsoleAccount`
-  removed the schema `ConsoleAccountErrorCode`
-  removed the schema `ConsoleAccountErrorEnvelope`
-  removed the schema `ConsoleAccountPage`
-  removed the schema `ConsoleAccountStatus`
-  removed the schema `ConsoleRole`
-  removed the schema `ConsoleRoleMatrix`
-  removed the schema `ConsoleSession`
-  removed the schema `ConsoleSignInRequest`
-  removed the schema `ConsoleSignInResult`
-  removed the schema `ConsoleSignOutResult`
-  removed the schema `CreateConsoleAccountRequest`
-  removed the schema `DeleteConsoleAccountRequest`
-  removed the schema `ResetConsolePasswordRequest`
-  removed the schema `UpdateConsoleAccountRequest`
