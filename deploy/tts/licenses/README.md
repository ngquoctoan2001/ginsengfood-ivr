# Published license evidence

`LICENSES.json` binds four byte-identical public documents collected in W-0341:

- VieNeu model card at `2da0efab622a1722125991736524f080b751ef5b`.
- MOSS Nano ONNX model card at `ceff0d0749bfb3fa2d61149794ec6feef0d1e1ae`.
- The Apache License 2.0 text from the Apache Software Foundation.
- The MOSS source LICENSE at `8c50ac4c5d7287d2ed6ea20a08c90ca439887d23`, supplementary
  attribution evidence, not a file claimed to exist in the pinned ONNX repository.

The manifest retains every source URL, byte size and SHA-256. Its own digest is pinned in
`../models/MODELS.lock` and independently in the CI provenance gate. Each of the 13 model
artifacts references the declaration for its exact repository/revision. Neither upstream
model repository ships a standalone LICENSE file at the selected revision; the legacy
`license_file_sha256` remains null rather than borrowing another component's license hash.

The VieNeu publisher's declaration explicitly covers commercial use of the shipped presets
and generated audio, and states that speaker/rightsholder consent was obtained. This is a
publisher statement, not an independent audit of private speaker agreements or training data.
The preset declaration is bound to the exact source commit and preset JSON digest.

Retain these license texts and applicable copyright/attribution notices when redistributing.
Identify modifications and preserve upstream NOTICE content when applicable. Attribution:
Pham Nguyen Ngoc Bao / pnnbao97/VieNeu-TTS / pnnbao-ump/VieNeu-TTS-v3-Turbo;
OpenMOSS Team / OpenMOSS-Team/MOSS-Audio-Tokenizer-Nano-ONNX. The supplementary MOSS source
license names OpenMOSS Team, Fudan University, SII and MOSI.

Both validators operate offline and reject missing, tampered, mismatched or linked documents.
Evidence validation does not grant Legal/Privacy or production approval.
See [W-0342](../../../docs/evidence/W-0342/README.md) for verification and historical bindings.
