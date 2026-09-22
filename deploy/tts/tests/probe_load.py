"""Opt-in real VieNeu HTTP load/timeout probe, isolated from telephony and all networks.

Run inside the exact TTS image with --network none. Texts must come from the
synthetic worker renderer fixture. Writes metrics/hashes only, never audio.
"""
from __future__ import annotations
import argparse
from concurrent.futures import ThreadPoolExecutor
import hashlib
import http.client
import json
import math
import os
from pathlib import Path
import resource
import socket
import threading
import time
from shim.server import RuntimeState, TtsServer


def run_load_probe() -> None:
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--texts',required=True)
    parser.add_argument('--out',required=True)
    parser.add_argument('--scope',choices=['LOCAL_LAB','S5_TARGET'],default='LOCAL_LAB')
    args=parser.parse_args()
    assert os.environ.get('IVR_EXECUTION_MODE')=='LAB_REAL_SIM'
    assert os.environ.get('REAL_CUSTOMER_CALL_ALLOWED')=='NO'
    assert os.environ.get('VIE_NEU_MAX_CONCURRENCY','1')=='1'
    fixtures=json.loads(Path(args.texts).read_text(encoding='utf-8-sig'))
    assert fixtures['fixtureOnly'] is True and fixtures['realCustomerCallAllowed']=='NO'
    selections=json.loads(Path(os.environ['VIE_NEU_VOICE_ACCEPTANCE_MANIFEST']).read_text())['selections']
    cases=[c for c in fixtures['cases'] if c['variant'] in ('MultiItem','LongName')]
    assert len(cases)==6
    out=Path(args.out); out.mkdir(parents=True,exist_ok=True)
    assert not (out/'load.json').exists(),'Refuse overwrite'
    started=time.monotonic(); state=RuntimeState(); state.initialize()
    assert state.ready and type(state.backend).__name__=='VieNeuBackend'
    def cgroup(name):
        path=Path('/sys/fs/cgroup')/name
        return path.read_text().strip() if path.exists() else None
    result={'scope':args.scope,'real_customer_call_allowed':'NO','model':'REAL_VIENEU',
            'startup_ms':round((time.monotonic()-started)*1000),'capacity':1,
            'ort_threads':state.backend._engine.ort_intra_op_threads,
            'cpu_max':cgroup('cpu.max'),'memory_max':cgroup('memory.max'),
            'cpu_stat_before':cgroup('cpu.stat'),'requests':[],'recoveries':[]}
    server=TtsServer(('127.0.0.1',0),state)
    thread=threading.Thread(target=server.serve_forever,daemon=True); thread.start()
    def save():
        result['peak_rss_kib']=resource.getrusage(resource.RUSAGE_SELF).ru_maxrss
        (out/'load.json').write_text(json.dumps(result,indent=2)+'\n')
    def request(case,segment,timeout,phase,barrier=None):
        if barrier is not None: barrier.wait(timeout=10)
        body=json.dumps({'text':segment['Text'],'voice_id':selections[case['region']]['voice_id'],
                        'locale':'vi-VN','speaking_rate':1.0,'output_format':'audio/L16','sample_rate':8000}).encode()
        conn=http.client.HTTPConnection('127.0.0.1',server.server_port,timeout=timeout)
        t=time.monotonic()
        row={'phase':phase,'region':case['region'],'variant':case['variant'],
             'ordinal':segment['Ordinal'],'characters':len(segment['Text']),
             'client_timeout_ms':int(timeout*1000)}
        try:
            conn.request('POST','/synthesize',body,{'Content-Type':'application/json; charset=utf-8'})
            response=conn.getresponse(); pcm=response.read()
            row['status']=response.status
            if response.status==200:
                assert response.getheader('Content-Type')=='audio/L16' and len(pcm)>1600 and len(pcm)%2==0 and any(pcm)
                row['pcm_sha256']=hashlib.sha256(pcm).hexdigest(); row['audio_ms']=len(pcm)//16
            else:
                assert response.status==503 and pcm==b'',(response.status,len(pcm))
                assert response.getheader('Connection')=='close'
        except (TimeoutError,socket.timeout):
            row['status']='CLIENT_TIMEOUT'
        finally:
            conn.close()
        row['elapsed_ms']=round((time.monotonic()-t)*1000)
        return row
    def drain(reason):
        # Read-only capacity observation does not bypass the HTTP request path or release work.
        t=time.monotonic()
        assert state.capacity.acquire(timeout=60), 'Inference failed to release capacity'
        state.capacity.release()
        result['recoveries'].append({'reason':reason,'drain_ms':round((time.monotonic()-t)*1000)})
    try:
        # Every dynamic segment of six worker scripts, diagnostic 60s to measure the
        # underlying latency even when a production 5s client would have timed out.
        for case in cases:
            for seg in case['segments']:
                if seg['kind']=='Fixed': continue
                row=request(case,seg,60,'sequential-diagnostic')
                assert row['status']==200,row
                result['requests'].append(row); save()
        # Synchronized bursts observe admission and completion separately. 503s are
        # explicit overload rejection, never counted as successful synthesis.
        for budget in (5,10):
            for clients in (1,2,4):
                for iteration,case in enumerate([c for c in cases if c['region']=='South'],1):
                    seg=max((s for s in case['segments'] if s['kind']!='Fixed'),key=lambda s:len(s['Text']))
                    barrier=threading.Barrier(clients)
                    phase=f'burst-{clients}-clients-{budget}s-r{iteration}'
                    with ThreadPoolExecutor(max_workers=clients) as pool:
                        rows=list(pool.map(lambda _:request(case,seg,budget,phase,barrier),range(clients)))
                    result['requests'].extend(rows)
                    assert sum(r['status'] in (200,'CLIENT_TIMEOUT') for r in rows)==1,rows
                    assert sum(r['status']==503 for r in rows)==clients-1,rows
                    drain(phase); save()
                    print('LOAD_BURST '+json.dumps({'phase':phase,'statuses':[r['status'] for r in rows],'max_ms':max(r['elapsed_ms'] for r in rows)}),flush=True)
        # Force a client disconnect during REAL inference, check overload on the same
        # HTTP/1.1 connection, then success once inference gives capacity back.
        case=next(c for c in cases if c['region']=='South' and c['variant']=='MultiItem')
        longest=max((s for s in case['segments'] if s['kind']!='Fixed'),key=lambda s:len(s['Text']))
        row=request(case,longest,0.05,'forced-disconnect')
        assert row['status']=='CLIENT_TIMEOUT'; result['requests'].append(row)
        conn=http.client.HTTPConnection('127.0.0.1',server.server_port,timeout=2)
        body=json.dumps({'text':longest['Text'],'voice_id':selections['South']['voice_id'],
            'locale':'vi-VN','speaking_rate':1.0,'output_format':'audio/L16','sample_rate':8000}).encode()
        conn.request('POST','/synthesize',body,{'Content-Type':'application/json'})
        busy=conn.getresponse(); busy.read(); assert busy.status==503
        assert busy.getheader('Connection')=='close'
        conn.request('GET','/health/ready'); ready=conn.getresponse(); ready.read()
        assert ready.status==200; conn.close()
        drain('forced-disconnect')
        recovery=request(case,longest,60,'after-disconnect')
        assert recovery['status']==200; result['requests'].append(recovery)
        result['http_recovery']={'busy_status':busy.status,'readiness_next_request':ready.status,
                                 'synthesis_after_drain':recovery['status'],'pass':True}
        assert state.capacity.acquire(blocking=False)
        state.capacity.release()
        sequential=[r['elapsed_ms'] for r in result['requests'] if r['phase']=='sequential-diagnostic']
        result['sequential']={'count':len(sequential),'p50_ms':sorted(sequential)[math.ceil(len(sequential)*0.5)-1],
            'p95_ms':sorted(sequential)[math.ceil(len(sequential)*0.95)-1],'max_ms':max(sequential),
            'over_5s':sum(v>5000 for v in sequential),'over_10s':sum(v>10000 for v in sequential)}
        result['cpu_stat_after']=cgroup('cpu.stat')
        result['all_assertions_pass']=True
        save()
        print('LOAD_PROBE_COMPLETE '+json.dumps(result['sequential']),flush=True)
    finally:
        save(); server.shutdown(); server.server_close(); thread.join(5)


if __name__=='__main__':
    run_load_probe()
