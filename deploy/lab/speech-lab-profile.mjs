#!/usr/bin/env node
// Explicit acceptance/rollback overlay; never loaded by the normal lab or production by default.
import { readFileSync, writeFileSync } from 'node:fs';
import { resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

export function createSpeechLabProfile(mode, imageId) {
  if (!['segmented', 'whole'].includes(mode)) throw new Error('LAB_SPEECH_PROFILE_INVALID_MODE');
  if (!/^sha256:[a-f0-9]{64}$/.test(imageId)) throw new Error('LAB_SPEECH_PROFILE_IMAGE_NOT_PINNED');
  const catalog = JSON.parse(readFileSync(new URL('./asterisk/audio/segments-appsettings.json', import.meta.url), 'utf8'));
  const environment = {
    IVR_EXECUTION_MODE: 'LAB_REAL_SIM', REAL_CUSTOMER_CALL_ALLOWED: 'NO',
    Ivr__Speech__Tts__Segmentation__Enabled: String(mode === 'segmented'),
    Ivr__Speech__Tts__Segmentation__FixedSegments: 'Catalog',
    // Whole-script rollback needs its measured lab budget; production defaults stay unchanged.
    Ivr__Speech__Tts__TimeoutMilliseconds: mode === 'segmented' ? '5000' : '60000',
  };
  for (const region of ['North', 'Central', 'South']) {
    const entries = catalog.Ivr.Speech.Tts.RegionalVoices[region].FixedSegments;
    if (entries.length !== 4) throw new Error('LAB_SPEECH_PROFILE_CATALOG_INCOMPLETE');
    entries.forEach((entry, index) => {
      if (!/^[a-f0-9]{64}$/.test(entry.TextHash) || !/^sound:ivr-seg-(north|central|south)-[a-f0-9]{16}$/.test(entry.MediaReference)
        || !Number.isInteger(entry.DurationMilliseconds) || entry.DurationMilliseconds <= 0) {
        throw new Error('LAB_SPEECH_PROFILE_CATALOG_INVALID');
      }
      for (const field of ['TextHash', 'MediaReference', 'DurationMilliseconds']) {
        environment[`Ivr__Speech__Tts__RegionalVoices__${region}__FixedSegments__${index}__${field}`] = String(entry[field]);
      }
    });
  }
  return { services: {
    'ivr-worker': { environment },
    'ivr-tts': { image: imageId, pull_policy: 'never', environment: {
      IVR_EXECUTION_MODE: 'LAB_REAL_SIM', REAL_CUSTOMER_CALL_ALLOWED: 'NO',
      VIE_NEU_ORT_THREADS: '1', OPENBLAS_NUM_THREADS: '1', OMP_NUM_THREADS: '1', MKL_NUM_THREADS: '1',
    } },
  } };
}

if (process.argv[1] && resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  const [mode, image, output, ...extra] = process.argv.slice(2);
  if (!output || extra.length) throw new Error('Usage: speech-lab-profile.mjs segmented|whole sha256:<image-id> <output.json>');
  writeFileSync(output, JSON.stringify(createSpeechLabProfile(mode, image), null, 2) + '\n');
  console.log(`LAB_SPEECH_PROFILE_WRITTEN mode=${mode} REAL_CUSTOMER_CALL_ALLOWED=NO listening=PENDING_OWNER`);
}
