# API Changelog 1.0.0-draft.27 vs. 1.0.0-draft.34


## API Changes

### GET /audit-evidence
-  endpoint added


### POST /result-callbacks/{callbackId}:replay
-  endpoint added


### POST /tasks
-  added the new optional request property `phone_e164`
-  added `subschema #1, subschema #2` to the request body `anyOf` list
-  the request property `dial_token` became optional
-  the request property `dial_token_expires_at` became optional
