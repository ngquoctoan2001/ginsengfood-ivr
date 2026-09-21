# W-0023 — Test bắt buộc cho lượt nghiệm thu W-0324

REAL_CUSTOMER_CALL_ALLOWED=NO. Bổ sung mapping hiện hành; không sửa số đo cũ.
Kết quả phải lấy từ bundle đúng commit ở [W-0324](../W-0324/README.md).

| TestId | Source | Method |
| --- | --- | --- |
| `IT-CALLBACK-OUTBOX-06` | `tests/Ivr.IntegrationTests/ResultNormalizationPersistenceTests.cs` | `DeliveryCompletionUsesLeaseFencingAndCreatesAdminVisibleReview` |
| `UT-CALLBACK-ACK-MEDIA-02C` | `tests/Ivr.UnitTests/Callbacks/CallbackDeliveryTests.cs` | `MalformedOrUnsupportedAckIsTerminalInvalid` |
| `UT-CALLBACK-ADAPTER-REJECT-15` | `tests/Ivr.UnitTests/Callbacks/CallbackDeliveryTests.cs` | `UnroutableProgramIsStillARejectedAdapterSelection` |
| `UT-CALLBACK-CIRCUIT-TERMINAL-13` | `tests/Ivr.UnitTests/Callbacks/CallbackDeliveryTests.cs` | `TerminalLocalResultReleasesHalfOpenCircuitProbe` |
| `UT-CALLBACK-CORRELATION-05B` | `tests/Ivr.UnitTests/Callbacks/CallbackDeliveryTests.cs` | `SnapshotCorrelationRemainsAuthoritativeThroughFoundationHandler` |
| `UT-CALLBACK-GH-COMPAT-06` | `tests/Ivr.UnitTests/Callbacks/CallbackDeliveryTests.cs` | `CurrentGoldenHourTransportUsesOnlyPinnedPathHeaderAndDto` |
| `UT-CALLBACK-GH-INTEGRITY-07B` | `tests/Ivr.UnitTests/Callbacks/CallbackDeliveryTests.cs` | `CurrentGoldenHourTransportRejectsChangedPayloadBeforeHttp` |
| `UT-CALLBACK-GH-ISOLATION-07` | `tests/Ivr.UnitTests/Callbacks/CallbackDeliveryTests.cs` | `CurrentGoldenHourTransportRejectsTwentyFourSevenBeforeHttp` |
| `UT-CALLBACK-IDENTITY-GUARD-04` | `tests/Ivr.UnitTests/Callbacks/CallbackDeliveryTests.cs` | `PathBodyMismatchAndInvalidNoAnswerNeverReachSales` |
| `UT-CALLBACK-NOANSWER-05` | `tests/Ivr.UnitTests/Callbacks/CallbackDeliveryTests.cs` | `FinalNoAnswerCarriesOnlyNoStateChangeRecommendation` |
| `UT-CALLBACK-OPTIONS-11` | `tests/Ivr.UnitTests/Callbacks/CallbackDeliveryTests.cs` | `OptionsFailClosedForRealTargetAndImplicitCurrentCompatibility` |
| `UT-CALLBACK-RETRY-AFTER-02B` | `tests/Ivr.UnitTests/Callbacks/CallbackDeliveryTests.cs` | `RateLimitCarriesRetryAfterIntoTheTransportResult` |
| `UT-CALLBACK-RETRY-AFTER-09B` | `tests/Ivr.UnitTests/Callbacks/CallbackDeliveryTests.cs` | `DispatcherDoesNotRetryBeforeTheServerRetryAfter` |
| `UT-CALLBACK-RETRY-EXHAUSTED-09` | `tests/Ivr.UnitTests/Callbacks/CallbackDeliveryTests.cs` | `DispatcherStopsAfterConfiguredRetryBudget` |
| `UT-CALLBACK-RETRY-IDENTITY-02` | `tests/Ivr.UnitTests/Callbacks/CallbackDeliveryTests.cs` | `RetryUsesTheExactSameBodyAndIdempotencyKey` |
| `UT-CALLBACK-SNAPSHOT-12` | `tests/Ivr.UnitTests/Callbacks/CallbackDeliveryTests.cs` | `AtomicSnapshotIsFinalImmutableTargetPayloadWithStableHash` |
| `UT-CALLBACK-STATE-08` | `tests/Ivr.UnitTests/Callbacks/CallbackDeliveryTests.cs` | `(mọi test trong class)` |
| `UT-CALLBACK-TARGET-ACK-01` | `tests/Ivr.UnitTests/Callbacks/CallbackDeliveryTests.cs` | `(mọi test trong class)` |
| `UT-CALLBACK-TARGET-ACK-CROSS-01` | `tests/Ivr.UnitTests/Callbacks/CallbackDeliveryTests.cs` | `AnAckCodeOnTheWrongStatusDeadLetters` |
| `UT-CALLBACK-TIMEOUT-03` | `tests/Ivr.UnitTests/Callbacks/CallbackDeliveryTests.cs` | `TargetTimeoutIsRetryableWithoutLeakingTransportDetails` |
| `UT-CALLBACK-TOKEN-CIRCUIT-10` | `tests/Ivr.UnitTests/Callbacks/CallbackDeliveryTests.cs` | `MockTokenRefreshAndCircuitReadinessAreDeterministic` |
| `UT-CALLBACK-TRANSPORT-INVALIDOP-14` | `tests/Ivr.UnitTests/Callbacks/CallbackDeliveryTests.cs` | `TransportRaisingInvalidOperationIsRetriedRatherThanDeadLettered` |
