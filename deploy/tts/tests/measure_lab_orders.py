"""Opt-in real-model probe for synthetic worker-rendered orders; never run by unittest discovery."""
from __future__ import annotations
import argparse
import hashlib
import json
import os
from pathlib import Path
import platform
import threading
import time
import urllib.request
import wave
from shim.server import RuntimeState, TtsServer


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument('--texts', required=True)
    parser.add_argument('--fixed', required=True)
    parser.add_argument('--out', required=True)
    parser.add_argument('--repeat', type=int, default=1, choices=range(1, 6))
    args = parser.parse_args()
    if os.environ.get('IVR_EXECUTION_MODE') != 'LAB_REAL_SIM' or os.environ.get('REAL_CUSTOMER_CALL_ALLOWED') != 'NO':
        raise RuntimeError('LAB_ONLY_REAL_CUSTOMERS_FORBIDDEN')
    fixtures = json.loads(Path(args.texts).read_text(encoding='utf-8-sig'))
    if fixtures.get('fixtureOnly') is not True or fixtures.get('realCustomerCallAllowed') != 'NO':
        raise RuntimeError('SYNTHETIC_RENDERER_FIXTURE_REQUIRED')
    accepted = json.loads(Path(os.environ['VIE_NEU_VOICE_ACCEPTANCE_MANIFEST']).read_text())['selections']
    out = Path(args.out); out.mkdir(parents=True, exist_ok=True)
    start = time.monotonic(); state = RuntimeState(); state.initialize()
    assert state.ready and type(state.backend).__name__ == 'VieNeuBackend'
    result = {'scope': 'LOCAL_LAB_MEASUREMENT', 'python': platform.python_version(),
              'load_ms': round((time.monotonic()-start)*1000), 'cases': [],
              'threads': state.backend._engine.ort_intra_op_threads, 'REAL_CUSTOMER_CALL_ALLOWED': 'NO'}
    server = TtsServer(('127.0.0.1', 0), state)
    thread = threading.Thread(target=server.serve_forever, daemon=True); thread.start()
    try:
        for iteration in range(1, args.repeat+1):
            for case in fixtures['cases']:
                region, variant = case['region'], case['variant']
                voice = accepted[region]['voice_id']; pieces = []; dynamic = []
                for segment in case['segments']:
                    if segment['kind'] == 'Fixed':
                        path = Path(args.fixed)/f"ivr-seg-{region.lower()}-{segment['TextHash'][:16]}.wav"
                        with wave.open(str(path), 'rb') as audio:
                            assert (audio.getnchannels(),audio.getsampwidth(),audio.getframerate()) == (1,2,8000)
                            pcm = audio.readframes(audio.getnframes())
                    else:
                        body = json.dumps({'text':segment['Text'],'voice_id':voice,'locale':'vi-VN',
                            'speaking_rate':1.0,'output_format':'audio/L16','sample_rate':8000}).encode()
                        request = urllib.request.Request(f'http://127.0.0.1:{server.server_port}/synthesize',
                            data=body,headers={'Content-Type':'application/json; charset=utf-8'})
                        started = time.monotonic()
                        with urllib.request.urlopen(request, timeout=60) as response:
                            pcm = response.read(); assert response.status == 200 and response.headers['Content-Type']=='audio/L16'
                        elapsed = round((time.monotonic()-started)*1000)
                        assert len(pcm)>1600 and len(pcm)%2==0 and any(pcm)
                        dynamic.append({'ordinal':segment['Ordinal'],'characters':len(segment['Text']),
                            'elapsed_ms':elapsed,'audio_ms':len(pcm)//16,'pcm_sha256':hashlib.sha256(pcm).hexdigest()})
                    pieces.append(pcm)
                pcm = b''.join(pieces); filename=f'{region}-{variant}-{iteration}.wav'
                with wave.open(str(out/filename), 'wb') as audio:
                    audio.setnchannels(1); audio.setsampwidth(2); audio.setframerate(8000); audio.writeframes(pcm)
                entry={'region':region,'variant':variant,'iteration':iteration,'voice':voice,'dynamic':dynamic,
                    'segmented_ms':sum(d['elapsed_ms'] for d in dynamic),'segmented_audio_ms':len(pcm)//16,
                    'segmented_pcm_sha256':hashlib.sha256(pcm).hexdigest(),'file':filename,
                    'within_5s_each':all(d['elapsed_ms']<=5000 for d in dynamic)}
                result['cases'].append(entry)
                (out/'measurement.json').write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8')
                print('LAB_ORDER_MEASURE '+json.dumps(entry),flush=True)
    finally:
        server.shutdown(); server.server_close(); thread.join(5)
    print(f"LAB_ORDER_MEASUREMENT_COMPLETE cases={len(result['cases'])} within_5s_each={all(c['within_5s_each'] for c in result['cases'])}",flush=True)


if __name__ == '__main__':
    main()
