{{- define "ivr.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{- define "ivr.fullname" -}}
{{- printf "%s-%s" .Release.Name (include "ivr.name" .) | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{- define "ivr.labels" -}}
app.kubernetes.io/name: {{ include "ivr.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end -}}

{{- define "ivr.image" -}}
{{- $registry := .Values.image.registry -}}
{{- if $registry -}}{{ printf "%s/" $registry }}{{- end -}}
{{- end -}}

{{/*
The ladder guard, evaluated at RENDER time. A values file that tries to open real calling outside
prod fails `helm template` rather than reaching a cluster: the earliest possible place to stop it,
and the only one that cannot be skipped by a hurried operator.
*/}}
{{- define "ivr.assertLadder" -}}
{{- $env := .Values.governance.environmentName | default "dev" -}}
{{- if .Values.governance.realCustomerCallAllowed -}}
  {{- if not (has $env (list "lab" "prod")) -}}
    {{- fail (printf "REAL_CUSTOMER_CALL_ALLOWED is true for environment '%s'. Only lab and prod may ever carry it, and only after a DF-03 sign-off (P7-2 section 11)." $env) -}}
  {{- end -}}
{{- end -}}
{{- if and (eq .Values.governance.executionMode "LAB_REAL_SIM") (not .Values.governance.labDestinationAllowlist) -}}
  {{- fail "LAB_REAL_SIM requires a non-empty labDestinationAllowlist: a lab run that may dial anything is not a lab run." -}}
{{- end -}}
{{- if and (ne .Values.governance.executionMode "MOCK") (not .Values.governance.killSwitchEnabled) -}}
  {{- fail "A non-MOCK execution mode requires the kill switch to remain enabled." -}}
{{- end -}}
{{- include "ivr.assertDatabaseTls" . -}}
{{- include "ivr.assertTtsCandidate" . -}}
{{- end -}}

{{/*
W-0122. A self-hosted TTS sidecar changes the worker Pod's supply chain, resource footprint and
media trust boundary. The default is disabled. If an operator enables it, every unresolved owner
gate must become an explicit render failure rather than a half-configured Pod.
*/}}
{{- define "ivr.assertTtsCandidate" -}}
{{- $tts := .Values.worker.tts -}}
{{- if $tts.enabled -}}
  {{- $env := .Values.governance.environmentName | default "dev" -}}
  {{- if ne $env "prod" -}}
    {{- fail (printf "worker.tts.enabled is true in '%s'. W-0122 Helm wiring is a production candidate; lab must use the explicit Compose overlay." $env) -}}
  {{- end -}}
  {{- if ne .Values.governance.executionMode "PRODUCTION_REAL" -}}
    {{- fail "worker.tts.enabled requires governance.executionMode=PRODUCTION_REAL; the external provider is forbidden in MOCK." -}}
  {{- end -}}
  {{- if or (not $tts.image.repository) (regexMatch "@|:[^/]+$" $tts.image.repository) -}}
    {{- fail "worker.tts.image.repository must name an internal repository without a tag or embedded digest." -}}
  {{- end -}}
  {{- if not (regexMatch "^sha256:[a-f0-9]{64}$" $tts.image.digest) -}}
    {{- fail "worker.tts.image.digest must be an exact sha256 digest." -}}
  {{- end -}}
  {{- if or (not $tts.modelBundle.existingClaim) (not (regexMatch "^[a-f0-9]{64}$" $tts.modelBundle.lockSha256)) -}}
    {{- fail "worker.tts.modelBundle requires an existingClaim and the approved 64-character MODELS.lock SHA-256." -}}
  {{- end -}}
  {{- if or
        (not $tts.voiceAcceptance.existingConfigMap)
        (gt (len $tts.voiceAcceptance.existingConfigMap) 253)
        (not (regexMatch "^[a-z0-9]([-a-z0-9.]*[a-z0-9])?$" $tts.voiceAcceptance.existingConfigMap))
        (ne $tts.voiceAcceptance.key "voice-acceptance-manifest.json")
        (ne $tts.voiceAcceptance.mountPath "/run/ivr-tts/voice-acceptance-manifest.json") -}}
    {{- fail "worker.tts.voiceAcceptance requires a valid existingConfigMap and the fixed voice-acceptance-manifest.json key/mount path." -}}
  {{- end -}}
  {{- if or (ne $tts.mediaSink.type "rwx-pvc") (not $tts.mediaSink.existingClaim) -}}
    {{- fail "worker.tts.mediaSink must be an owner-approved rwx-pvc with an existingClaim; OD-VOICE-08 is not inferred by the chart." -}}
  {{- end -}}
  {{- range $name, $value := dict
        "legalRef" $tts.approvals.legalRef
        "platformRef" $tts.approvals.platformRef
        "telephonyRef" $tts.approvals.telephonyRef
        "internalMirrorRef" $tts.approvals.internalMirrorRef
        "voiceAcceptanceRef" $tts.approvals.voiceAcceptanceRef -}}
    {{- if not $value -}}
      {{- fail (printf "worker.tts.approvals.%s is required before the production candidate can render." $name) -}}
    {{- end -}}
  {{- end -}}
  {{- $timeoutMs := int $tts.timeoutMilliseconds -}}
  {{- $segments := int $tts.dynamicSegmentsPerCall -}}
  {{- $preDialBudgetMs := int $tts.preDialBudgetMilliseconds -}}
  {{- if or (lt $timeoutMs 1000) (gt $timeoutMs 60000) -}}
    {{- fail "worker.tts.timeoutMilliseconds must be between 1000 and 60000." -}}
  {{- end -}}
  {{- if or (lt $segments 1) (gt $segments 8) -}}
    {{- fail "worker.tts.dynamicSegmentsPerCall must be between 1 and 8." -}}
  {{- end -}}
  {{- if or (lt $preDialBudgetMs 1000) (gt $preDialBudgetMs 600000) -}}
    {{- fail "worker.tts.preDialBudgetMilliseconds must be between 1000 and 600000." -}}
  {{- end -}}
  {{- if and (gt $timeoutMs 5000) (not $tts.approvals.performanceRef) -}}
    {{- fail (printf "worker.tts.timeoutMilliseconds is %d, above the accepted worker baseline of 5000. Raising the per-request timeout is not a substitute for capacity (W-0122 4.6); set worker.tts.approvals.performanceRef to the target-hardware measurement that justifies it." $timeoutMs) -}}
  {{- end -}}
  {{- $coldPreDialMs := mul $segments $timeoutMs -}}
  {{- if gt (mul $coldPreDialMs 100) (mul $preDialBudgetMs 80) -}}
    {{- fail (printf "cold pre-dial synthesis can consume %d ms (%d dynamic segments x %d ms, synthesised sequentially) which leaves less than the 20 percent headroom W-0122 4.6 requires under the %d ms pre-dial budget." $coldPreDialMs $segments $timeoutMs $preDialBudgetMs) -}}
  {{- end -}}
  {{- if or
        (not $tts.resources.requests.cpu)
        (not $tts.resources.requests.memory)
        (not $tts.resources.limits.cpu)
        (not $tts.resources.limits.memory) -}}
    {{- fail "worker.tts.resources needs measured CPU/memory requests and limits; laptop defaults are forbidden." -}}
  {{- end -}}
  {{- range $region, $voice := $tts.voices -}}
    {{- if or (not $voice.voiceId) (lt (float64 $voice.speakingRate) 0.5) (gt (float64 $voice.speakingRate) 2.0) -}}
      {{- fail (printf "worker.tts.voices.%s needs an owner-accepted voiceId and speakingRate between 0.5 and 2.0." $region) -}}
    {{- end -}}
    {{- if ne (len $voice.fixedSegments) 4 -}}
      {{- fail (printf "worker.tts.voices.%s must carry exactly four accepted fixed-segment entries." $region) -}}
    {{- end -}}
    {{- range $segment := $voice.fixedSegments -}}
      {{- if or
            (not (regexMatch "^[a-f0-9]{64}$" $segment.textHash))
            (not (regexMatch "^sound:[A-Za-z0-9/_-]+$" $segment.mediaReference))
            (lt (int $segment.durationMilliseconds) 1)
            (gt (int $segment.durationMilliseconds) 300000) -}}
        {{- fail (printf "worker.tts.voices.%s contains an invalid fixed-segment hash/reference/duration." $region) -}}
      {{- end -}}
    {{- end -}}
  {{- end -}}
  {{- if or
        (eq $tts.voices.North.voiceId $tts.voices.Central.voiceId)
        (eq $tts.voices.North.voiceId $tts.voices.South.voiceId)
        (eq $tts.voices.Central.voiceId $tts.voices.South.voiceId) -}}
    {{- fail "worker.tts requires three distinct owner-accepted regional voice IDs." -}}
  {{- end -}}
{{- end -}}
{{- end -}}

{{/*
W-0053 / P10-2. In-transit protection for the database, enforced at render.

Prefer is refused everywhere, including dev. "Encrypt if convenient" is not a policy: it produces a
plaintext connection under exactly the condition a policy exists to cover, and it does so without an
error anyone would see. Disable is at least honest about what it is, which is why dev may use it and
nothing else may.
*/}}
{{- define "ivr.assertDatabaseTls" -}}
{{- $env := .Values.governance.environmentName | default "dev" -}}
{{- $mode := .Values.database.sslMode | default "Require" -}}
{{- if eq $mode "Prefer" -}}
  {{- fail (printf "database.sslMode is 'Prefer' for environment '%s'. Prefer falls back to plaintext in silence; choose Require, or Disable in dev where the intent is explicit." $env) -}}
{{- end -}}
{{- if and (eq $mode "Disable") (ne $env "dev") -}}
  {{- fail (printf "database.sslMode is 'Disable' for environment '%s'. Only dev may run without TLS to the database." $env) -}}
{{- end -}}
{{- if and .Values.database.trustServerCertificate (eq $env "prod") -}}
  {{- fail "database.trustServerCertificate is true in prod. Encryption without certificate validation stops passive eavesdropping and not a machine in the middle." -}}
{{- end -}}
{{- end -}}

{{/*
Environment shared by api and worker. Written once so the two cannot drift: a worker that thinks it
is in MOCK while the api thinks otherwise is the worst possible split.
*/}}
{{- define "ivr.governanceEnv" -}}
- name: IVR_EXECUTION_MODE
  value: {{ .Values.governance.executionMode | quote }}
- name: IVR_ADAPTER_MODE
  value: {{ .Values.governance.adapterMode | quote }}
- name: REAL_CUSTOMER_CALL_ALLOWED
  value: {{ ternary "YES" "NO" .Values.governance.realCustomerCallAllowed | quote }}
- name: IVR_KILL_SWITCH_ENABLED
  value: {{ .Values.governance.killSwitchEnabled | quote }}
{{- if .Values.governance.labDestinationAllowlist }}
- name: IVR_LAB_DESTINATION_ALLOWLIST
  value: {{ join "," .Values.governance.labDestinationAllowlist | quote }}
{{- end }}
{{- end -}}

{{/*
Order matters here and is not cosmetic. Kubernetes expands $(VAR) in an env value only against
variables declared EARLIER in the same list; a later one stays literal. With the connection string
first, every pod received the text "$(IVR_DB_PASSWORD)" as its password and failed authentication
with 28P01 -- which surfaced as "the database rejects us", pointing at the credential rather than at
the ordering.
*/}}
{{- define "ivr.dbEnv" -}}
- name: IVR_DB_PASSWORD
  valueFrom:
    secretKeyRef:
      name: {{ .Values.database.existingSecret }}
      key: {{ .Values.database.existingSecretPasswordKey }}
- name: ConnectionStrings__IvrDb
  value: "Host={{ .Values.database.host }};Port={{ .Values.database.port }};Database={{ .Values.database.name }};Username={{ .Values.database.user }};Password=$(IVR_DB_PASSWORD);SSL Mode={{ .Values.database.sslMode | default "Require" }};Trust Server Certificate={{ .Values.database.trustServerCertificate | default false }}"
{{- end -}}

{{- define "ivr.appSecretEnv" -}}
- name: IVR_INTERNAL_SERVICE_TOKEN
  valueFrom:
    secretKeyRef:
      name: {{ .Values.secrets.existingSecret }}
      key: {{ .Values.secrets.internalServiceTokenKey }}
- name: ORDER_CORE_SERVICE_TOKEN
  valueFrom:
    secretKeyRef:
      name: {{ .Values.secrets.existingSecret }}
      key: {{ .Values.secrets.orderCoreServiceTokenKey }}
{{- if .Values.secrets.orderCoreServiceTokenPreviousKey }}
# The outgoing half of a credential rotation. Optional, and absent by default: a previous token
# that is always present is a second live credential rather than an overlap.
#
# Without these two the chart could not express a rotation at all. RotatingCredentialProvider and
# the runbook both describe an overlap, and on Kubernetes the only available shape was a hard
# cutover -- the exact window the provider exists to remove. IT-K8S-ROTATE-07 measures what the
# overlap buys across a fleet, and what it does not.
#
# `optional: true` sits on the key, not on the reference: the secret always exists because it
# carries the current token, and it is the ROTATION key inside it that comes and goes.
- name: ORDER_CORE_SERVICE_TOKEN_PREVIOUS
  valueFrom:
    secretKeyRef:
      name: {{ .Values.secrets.existingSecret }}
      key: {{ .Values.secrets.orderCoreServiceTokenPreviousKey }}
      optional: true
{{- end }}
{{- if .Values.secrets.orderCoreServiceTokenPreviousRetiresAt }}
# An instant, not a duration. A duration would restart with every pod, so a rotation would never
# finish while anything was being rescheduled -- and "never finishes" is how an overlap becomes a
# permanent second credential.
- name: ORDER_CORE_SERVICE_TOKEN_PREVIOUS_RETIRES_AT
  value: {{ .Values.secrets.orderCoreServiceTokenPreviousRetiresAt | quote }}
{{- end }}
{{- end -}}
