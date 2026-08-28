# VieNeu-TTS upstream provenance

This directory is a mechanical vendor import of one immutable upstream Git tree. Project-owned
adapter code lives under `deploy/tts/`; do not patch the vendored implementation in place.

| Field | Value |
| --- | --- |
| Upstream | `https://github.com/pnnbao97/VieNeu-TTS.git` |
| Full commit | `36c4b501b0634a8f59805e6b529a058fbd30190b` |
| Git tree | `16632c30c2484aa4f86c8cde68a074192bd52736` |
| Retrieved | `2026-08-27` |
| Upstream commit date | `2026-08-20T09:19:34+07:00` |
| License file SHA-256 | `c71d239df91726fc519c6eb72d318ec65820627232b2f796219e87dcf35d0ab4` |
| Dependency lock SHA-256 | `f04d0713ee2e0041fae1234064fad0f22958be712f852fc6464e1beb3a724b4e` |
| Voice manifest SHA-256 | `574e6acf03823c4cafdc43f106731ce5fce6de30228fe383831b8b9064ee0bd8` |
| Upstream NOTICE | Absent at the pinned tree |

Verification:

```powershell
git clone --filter=blob:none --no-checkout https://github.com/pnnbao97/VieNeu-TTS.git <temp>
git -C <temp> checkout --detach 36c4b501b0634a8f59805e6b529a058fbd30190b
git -C <temp> rev-parse 'HEAD^{tree}'
```

The model and codec weights are not committed here. Their exact repositories, revisions, paths,
sizes and SHA-256 values are recorded in `deploy/tts/models/MODELS.lock`.

