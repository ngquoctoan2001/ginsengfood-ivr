# API Changelog 1.0.0-draft.24 vs. 1.0.0-draft.25


## API Changes

### POST /call-jobs/{ivrCallJobId}:terminate
- :warning: added the new required `header` request parameter `x-action-reason`


### POST /call-jobs:terminate-all
- :warning: added the new required `header` request parameter `x-action-reason`


### POST /feature-flags/{environment}
- :warning: added the new required `header` request parameter `x-action-reason`
-  added the new optional `header` request parameter `x-destination-ref`


### POST /queue:pause
- :warning: added the new required `header` request parameter `x-action-reason`


### POST /queue:resume
- :warning: added the new required `header` request parameter `x-action-reason`


### POST /scripts
-  added the new optional `header` request parameter `x-script-permissions`


### POST /scripts/{templateId}/{version}:approve
-  added the new optional `header` request parameter `x-script-permissions`


### POST /scripts/{templateId}/{version}:retire
-  added the new optional `header` request parameter `x-script-permissions`


### POST /scripts/{templateId}/{version}:submit
-  added the new optional `header` request parameter `x-script-permissions`


### POST /sim-channels/{simChannelId}:disable
- :warning: added the new required `header` request parameter `x-action-reason`


### POST /sim-channels/{simChannelId}:enable
- :warning: added the new required `header` request parameter `x-action-reason`


### POST /technical-retries
- :warning: added the new required `header` request parameter `x-action-reason`
