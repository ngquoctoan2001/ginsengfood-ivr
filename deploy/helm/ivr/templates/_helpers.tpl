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
  {{- if not (has $env (list "pilot" "prod")) -}}
    {{- fail (printf "REAL_CUSTOMER_CALL_ALLOWED is true for environment '%s'. Only pilot and prod may ever carry it, and only after a DF-03 sign-off (P7-2 section 11)." $env) -}}
  {{- end -}}
{{- end -}}
{{- if and (eq .Values.governance.executionMode "LAB_REAL_SIM") (not .Values.governance.labDestinationAllowlist) -}}
  {{- fail "LAB_REAL_SIM requires a non-empty labDestinationAllowlist: a lab run that may dial anything is not a lab run." -}}
{{- end -}}
{{- if and (ne .Values.governance.executionMode "MOCK") (not .Values.governance.killSwitchEnabled) -}}
  {{- fail "A non-MOCK execution mode requires the kill switch to remain enabled." -}}
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
  value: "Host={{ .Values.database.host }};Port={{ .Values.database.port }};Database={{ .Values.database.name }};Username={{ .Values.database.user }};Password=$(IVR_DB_PASSWORD)"
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
{{- end -}}
