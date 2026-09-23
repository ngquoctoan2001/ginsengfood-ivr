"""W-0344: isolated synthetic SIP/RTP flow. No shared-lab mutation or direct DB writes."""
import os
import datetime as dt
import hashlib
import json
from pathlib import Path
import re
import subprocess
import sys
import time
import urllib.request
import urllib.error

ROOT=Path(__file__).resolve().parent; OUT=Path(os.environ['IVR_FLOW_OUTPUT']).resolve(); PROJECT=os.environ['IVR_FLOW_PROJECT']
SWITCH=PROJECT+'-asterisk-1'; DB=PROJECT+'-postgres-1'; WORKER=PROJECT+'-ivr-worker-1'; TTS=PROJECT+'-ivr-tts-1'
PEER=PROJECT+'-peer'; NETWORK=PROJECT+'-lab'; API='http://127.0.0.1:'+os.environ['IVR_FLOW_PORT']+'/v1/ivr/order-confirmation'
assert OUT.is_dir() and re.fullmatch(r'ivr-w0344-[a-z0-9-]+',PROJECT)
RESUME=False  # Every run starts with a fresh DB; partial runs cannot resume.
RUN=OUT/'run-2'; RUN.mkdir(exist_ok=RESUME)
paused=set()
result={'scope':os.environ['IVR_FLOW_SCOPE'],'REAL_CUSTOMER_CALL_ALLOWED':'NO','started_utc':None,'cases':[]}
if RESUME:
    result=json.loads((RUN/'result.json').read_text(encoding='utf-8'))
    assert result['cases'] and all(c['pass'] for c in result['cases'])
    (RUN/'result-before-resume.json').write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    result['harness_interruption']='Stopped before queued-expiry admission because expanded fixture omitted A/B. Resumed with the base fixture as well.'
    result.pop('error',None)

def now(): return dt.datetime.now(dt.timezone.utc)
def utc(): return now().isoformat()
def save(): (RUN/'result.json').write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
def cmd(*args):
    if args[0]=='docker': args=('docker','--context',os.environ['IVR_FLOW_DOCKER_CONTEXT'],*args[1:])
    return subprocess.check_output(args,encoding='utf-8',stderr=subprocess.STDOUT,timeout=45).strip()
def cli(container,text):
    assert container in (SWITCH,PEER)
    return cmd('docker','exec',container,'asterisk','-rx',text)
def channels(container): return [s.split('!')[0] for s in cli(container,'core show channels concise').splitlines() if '!' in s]
def sql(text):
    assert text.startswith('SELECT ') and ';' not in text
    return cmd('docker','exec',DB,'psql','-U','ivr','-d','ivr','-Atc',text)
def pause(name):
    assert name in (WORKER,TTS)
    cmd('docker','pause',name); paused.add(name)
def unpause(name):
    assert name in paused
    cmd('docker','unpause',name); paused.remove(name)
def post(route,payload,role,identity):
    assert re.fullmatch(r'[A-Za-z0-9-]+',identity)
    token={'intake':'dev-ordercore-token-not-a-real-secret','internal':'dev-internal-token-not-a-real-secret','admin':'dev-admin-danger-token-not-a-real-secret'}[role]
    headers={'Authorization':'Bearer '+token,'Content-Type':'application/json','X-Correlation-Id':payload.get('correlation_id','corr-'+identity),'Idempotency-Key':identity}
    if role=='intake': headers['X-Source-System']='order-core'
    elif role=='internal': headers.update({'X-Source-System':'ivr-worker','X-Service-Scope':'ivr.internal.write'})
    else: headers.update({'X-Service-Scope':'ivr.admin.danger','X-Actor-Id':'w0344-test-operator','X-Action-Reason':'Synthetic W0344 isolated lab verification'})
    request=urllib.request.Request(API+route,data=json.dumps(payload).encode(),headers=headers,method='POST')
    try:
        with urllib.request.urlopen(request,timeout=20) as response: return json.load(response)
    except urllib.error.HTTPError as error:
        raise RuntimeError(f'{route}: HTTP {error.code}: '+error.read().decode()) from error

ORDERS=json.loads((ROOT/'fixtures/fake-orders.json').read_text(encoding='utf-8-sig'))
MEASURE=json.loads((ROOT/'fixtures/measurement.json').read_text(encoding='utf-8-sig'))['cases']
TEXTS=json.loads((ROOT/'fixtures/worker-texts-expanded.json').read_text(encoding='utf-8-sig'))
assert TEXTS['fixtureOnly'] and TEXTS['realCustomerCallAllowed']=='NO'
BASE_TEXTS=json.loads((ROOT/'fixtures/worker-texts.json').read_text(encoding='utf-8-sig'))
assert BASE_TEXTS['fixtureOnly'] and BASE_TEXTS['realCustomerCallAllowed']=='NO'
TEXTS['cases']+=BASE_TEXTS['cases']
AREAS={'North':'Phường Cửa Nam, thành phố Hà Nội','Central':'Phường Hải Châu, thành phố Đà Nẵng','South':'Phường Phú Khương, tỉnh Vĩnh Long'}
TERMINAL=('RESULT_READY_FOR_CALLBACK','WINDOW_EXPIRED','CAPACITY_MISSED','HELD_ADMIN_REVIEW')
# W-0345: the same seven cases for either programme. The wire matrix allows exactly these two pairs,
# and the approved script never speaks the programme name, so the measured audio stays valid.
PROGRAMS={'GOLDEN_HOUR':('ONLINE','Giờ Vàng'),'TWENTY_FOUR_SEVEN':('COD','Bán hàng 24/7')}
PROGRAM=os.environ.get('IVR_FLOW_PROGRAM','GOLDEN_HOUR'); assert PROGRAM in PROGRAMS, PROGRAM
PAYMENT,PROGRAM_NAME=PROGRAMS[PROGRAM]
result.update(program=PROGRAM,payment=PAYMENT)

def state(task):
    assert re.fullmatch(r'TASK-W0344-[A-Z0-9-]+',task)
    return json.loads(sql(f"SELECT json_build_object('job',(SELECT json_build_object('id',ivr_call_job_id,'status',status,'queue',queue_status) FROM ivr_call_jobs WHERE task_id='{task}'),'attempts',(SELECT COALESCE(json_agg(json_build_object('id',ivr_call_attempt_id,'number',attempt_number,'status',status,'counted',is_counted_customer_attempt,'exception',technical_exception_type,'provider',provider_call_id,'dtmf',dtmf_key,'voice',voice_id,'region',voice_region,'started',started_at,'ended',ended_at) ORDER BY scheduled_at),'[]'::json) FROM ivr_call_attempts WHERE task_id='{task}'),'results',(SELECT COALESCE(json_agg(json_build_object('type',result_type,'counted',is_counted_customer_attempt,'final',is_final_for_ivr,'reason',result_reason) ORDER BY created_at),'[]'::json) FROM ivr_call_results WHERE task_id='{task}'),'callbacks',(SELECT count(*) FROM ivr_result_callbacks WHERE task_id='{task}'))"))

def admit(case,region,variant,remaining=285):
    suffix=now().strftime('%Y%m%d%H%M%S%f'); task='TASK-W0344-'+case.upper()+'-'+suffix
    expires=now()+dt.timedelta(seconds=remaining); start=expires-dt.timedelta(seconds=300)
    stamp=lambda d:d.isoformat(timespec='milliseconds').replace('+00:00','Z')
    body={'contract_version':'ivr-order-confirmation.v1','task_id':task,'correlation_id':'corr-'+suffix,'created_at':stamp(start),'order_id':'ORDER-W0344-'+suffix,'order_code':'GF-W0344-'+suffix,'order_code_short':'E2E001','order_version':'1','order_state':'CONFIRMING','payment_method_snapshot':PAYMENT,'ivr_confirmation_required':True,'is_ivr_callable':True,'program_code':PROGRAM,'confirmation_window_started_at':stamp(start),'confirmation_window_expires_at':stamp(expires),'attempt_policy_version':'lab-softphone-v1','max_customer_attempts':1,'attempt_offsets_seconds':[0],'phone_ref':'phone-ref-w0104-fake','phone_masked':'84xxxxx0001','phone_validation_status':'VALID','dial_token':'dial-token-lab-'+suffix,'dial_token_expires_at':stamp(expires),'privacy_safe_order_summary':{'customer_display_name':'anh/chị Giang','order_code_short':'E2E001','items':ORDERS[variant]['items'],'total_amount':ORDERS[variant]['total_amount'],'currency':'VND','delivery_area_short':AREAS[region],'program_display_name':PROGRAM_NAME,'locale':'vi-VN'},'call_restriction':False,'eligibility_snapshot':{'decision':'ELIGIBLE','source_version':'w0104-fake-sales-v1','captured_at':stamp(start),'source_available':True,'blockers':[],'voice_restriction':{'restricted':False,'source_available':True,'source_version':'w0104-fake-voice-v1'}},'call_script_template_id':'SCRIPT-ORDER-CONFIRM','call_script_version':'v3-test-approved','evidence_policy_version':'w0104-lab-evidence-v1','privacy_policy_version':'w0104-lab-privacy-v1','evidence_ref':'evidence://w0344/'+task}
    intake=post('/tasks',body,'intake','intake-'+suffix)
    assert intake['decision']=='TASK_ACCEPTED_CALL_JOB_CREATED',intake
    eligible=post('/eligibility-checks',{'task_id':task},'internal','eligible-'+suffix)
    assert eligible['decision']=='ELIGIBLE_FOR_IVR',eligible
    return task,{'intake':intake,'eligibility':eligible,'expires_utc':stamp(expires)}

def setup_peer():
    assert sql('SELECT count(*) FROM ivr_confirmation_tasks')=='0', 'Only a fresh dedicated DB is allowed'
    for name in (SWITCH,DB,WORKER,TTS,PROJECT+'-ivr-api-1'):
        info=json.loads(cmd('docker','inspect',name))[0]
        assert info['Config']['Labels']['com.docker.compose.project']==PROJECT
        assert not info['State']['Paused']
        if name in (WORKER,PROJECT+'-ivr-api-1'):
            e=info['Config']['Env']; assert 'REAL_CUSTOMER_CALL_ALLOWED=NO' in e and 'IVR_EXECUTION_MODE=LAB_REAL_SIM' in e
    assert json.loads(cmd('docker','inspect',TTS))[0]['State']['Health']['Status']=='healthy'
    switch=json.loads(cmd('docker','inspect',SWITCH))[0]; switch_ip=switch['NetworkSettings']['Networks'][NETWORK]['IPAddress']
    assert re.fullmatch(r'[0-9.]+',switch_ip)
    peer=RUN/'peer'; peer.mkdir()
    (peer/'pjsip.conf').write_text(f'[transport-udp]\ntype=transport\nprotocol=udp\nbind=0.0.0.0:5060\n[switch]\ntype=endpoint\ntransport=transport-udp\ncontext=automatic-answer\ndisallow=all\nallow=ulaw\ndirect_media=no\ndtmf_mode=rfc4733\n[switch-identify]\ntype=identify\nendpoint=switch\nmatch={switch_ip}\n')
    (peer/'extensions.conf').write_text('[general]\nstatic=yes\nwriteprotect=yes\n[automatic-answer]\nexten => LAB-A,1,Answer()\n same => n,Verbose(1,W0344_ANSWER)\n same => n,Wait(150)\n same => n,Hangup()\n'+''.join(f'[send-{d}]\nexten => s,1,Verbose(1,W0344_DTMF_{d})\n same => n,SendDTMF({d},250,160)\n same => n,Wait(10)\n same => n,Hangup()\n' for d in ('0','1')))
    (peer/'http.conf').write_text('[general]\nenabled=no\n')
    (peer/'logger.conf').write_text('[general]\ndateformat=%F %T\n[logfiles]\nconsole=notice,warning,error,verbose,dtmf\n')
    mounts=[]
    # S5 runs this under umask 077 and the peer's root has no CAP_DAC_OVERRIDE (cap-drop ALL), so
    # 0600 files owned by ssv were unreadable there (run 20260923-103255). None of these holds a secret.
    for name in ('pjsip.conf','extensions.conf','http.conf','logger.conf'): (peer/name).chmod(0o644)
    for name in ('pjsip.conf','extensions.conf','http.conf','logger.conf'): mounts+=['--mount',f'type=bind,src={peer/name},dst=/etc/asterisk/{name},readonly']
    cmd('docker','run','-d','--name',PEER,'--label','ivr.work=W0344','--label','ivr.flow.project='+PROJECT,'--cpus','0.5','--memory','256m','--memory-swap','256m','--network',NETWORK,'--cap-drop','ALL','--security-opt','no-new-privileges',*mounts,'--entrypoint','/usr/sbin/asterisk',switch['Image'],'-f','-vvv')
    peer_ip=json.loads(cmd('docker','inspect',PEER))[0]['NetworkSettings']['Networks'][NETWORK]['IPAddress']
    original=RUN/'pjsip-original.conf'; cmd('docker','cp',SWITCH+':/etc/asterisk/pjsip.conf',str(original))
    text=original.read_text(encoding='utf-8'); assert text.count('aors=LAB-A')==1 and text.count('media_address=127.0.0.1')==1
    modified=text.replace('aors=LAB-A','aors=LAB-A-AUTO',1).replace('media_address=127.0.0.1','media_address='+switch_ip,1)
    modified+=f'\n[LAB-A-AUTO]\ntype=aor\nmax_contacts=0\ncontact=sip:LAB-A@{peer_ip}:5060\nqualify_frequency=2\n'
    config=RUN/'pjsip-automatic.conf'; config.write_text(modified,encoding='utf-8'); cmd('docker','cp',str(config),SWITCH+':/etc/asterisk/pjsip.conf')
    cli(SWITCH,'module reload res_pjsip.so'); cli(SWITCH,'logger add channel /var/log/asterisk/w0344-dtmf dtmf'); time.sleep(3)
    assert re.search(r'(?m)^\s*aors\s*:\s*LAB-A-AUTO\s*$',cli(SWITCH,'pjsip show endpoint LAB-A'))
    assert '@'+peer_ip+':5060' in cli(SWITCH,'pjsip show contacts')
    result.update(peer_ip=peer_ip,switch_ip=switch_ip,original_config_sha256=hashlib.sha256(original.read_bytes()).hexdigest())

def run_case(name,region,variant,digit=None,fault=None):
    assert not channels(SWITCH) and not channels(PEER)
    measured=next(c for c in MEASURE if c['region']==region and c['variant']==variant)
    fixture=next(c for c in TEXTS['cases'] if c['region']==region and c['variant']==variant)
    item={'case':name,'region':region,'variant':variant,'digit':digit,'fault':fault,'started_utc':utc(),'pass':False,'observed_channels':False}
    result['cases'].append(item); save()
    if fault=='queued-expiry': pause(WORKER)
    if fault in ('tts-expiry','tts-timeout'): pause(TTS)
    task,item['admission']=admit(name,region,variant,remaining=6 if fault=='queued-expiry' else 18 if fault=='tts-expiry' else 285)
    item['task_id']=task; save()
    item['program']=sql(f"SELECT program_type||'|'||payment_method_snapshot FROM ivr_confirmation_tasks WHERE task_id='{task}'")
    assert item['program']==PROGRAM+'|'+PAYMENT,item['program']
    if fault=='queued-expiry': time.sleep(7); unpause(WORKER)
    started=time.monotonic(); answered=None; sent=False; terminated=False
    while time.monotonic()-started<360:
        current=state(task); peers=channels(PEER); item['state']=current
        if fault=='tts-timeout' and TTS in paused and any(a['exception']=='TTS_TIMEOUT' for a in current['attempts']):
            assert not channels(SWITCH); item['failure_before_dial']=True; item['fault_released_utc']=utc(); unpause(TTS)
        if fault=='tts-expiry' and current['results'] and TTS in paused: unpause(TTS)
        if peers:
            assert len(peers)==1
            item['observed_channels']=True
            if answered is None: answered=time.monotonic(); item['answered_utc']=utc()
            if fault=='operator-cancel' and not terminated and time.monotonic()-answered>3:
                job=current['job']['id']; reason={'reason':'Synthetic W0344 single-call termination'}
                item['termination']=post('/call-jobs/'+job+':terminate',reason,'admin','terminate-'+task)
                item['queue_pause']=post('/queue:pause',{'reason':'Synthetic W0344 retain operator-cancel evidence'},'admin','pause-'+task)
                terminated=True
            elif digit is not None and not sent and time.monotonic()-answered>=measured['segmented_audio_ms']/1000+2:
                item['rtp']=cli(PEER,'pjsip show channelstats')
                # Wall time is only a lower bound under load. Wait for the same
                # received-audio threshold that the final assertion enforces.
                received_rtp=re.search(r'switch-[a-f0-9]+\s+\S+\s+ulaw\s+(\d+)',item['rtp'])
                if received_rtp and int(received_rtp[1])>=measured['segmented_audio_ms']//20:
                    item['sent_utc']=utc()
                    cli(PEER,f'channel redirect {peers[0]} send-{digit},s,1'); sent=True
        if current['job']['status'] in TERMINAL or (fault=='operator-cancel' and any(r['type']=='IVR_TECHNICAL_EXCEPTION' for r in current['results'])):
            break
        time.sleep(0.5)
    else: raise AssertionError('Case did not settle: '+name)
    for _ in range(20):
        if not channels(SWITCH) and not channels(PEER): break
        time.sleep(.5)
    item['ended_utc']=utc(); item['state']=state(task)
    logs=cmd('docker','logs',SWITCH,'--timestamps','--since',item['started_utc'],'--until',item['ended_utc'])
    (RUN/(name+'-switch.log')).write_text(logs,encoding='utf-8')
    (RUN/(name+'-peer.log')).write_text(cmd('docker','logs',PEER,'--timestamps','--since',item['started_utc'],'--until',item['ended_utc']),encoding='utf-8')
    playing=re.findall(r"Playing '([^']+)'",logs); item['media']=playing
    current=item['state']; attempts=current['attempts']; results=current['results']; counted=sum(a['counted'] is True for a in attempts)
    if fault in ('queued-expiry','tts-expiry'):
        assert not item['observed_channels'] and not playing and counted==0
        assert all(a['provider'] is None and not a['counted'] for a in attempts)
        assert results and all(not r['counted'] for r in results) and sum(r['final'] for r in results)==1
        assert current['job']['status'] in ('WINDOW_EXPIRED','CAPACITY_MISSED','HELD_ADMIN_REVIEW'),current
        if fault=='queued-expiry': assert not attempts
        else: assert any(a['exception']=='TTS_CACHE_WINDOW_EXPIRED' for a in attempts),current
    elif fault=='operator-cancel':
        assert terminated and counted==0 and len(attempts)==1,current
        assert attempts[0]['exception']=='CALL_TERMINATED_BY_OPERATOR'
        assert results==[{'type':'IVR_TECHNICAL_EXCEPTION','counted':False,'final':False,'reason':results[0]['reason']}],current
        assert current['callbacks']==0 and not channels(SWITCH)
    else:
        expected='IVR_CONFIRMED' if digit=='1' else 'IVR_CUSTOMER_CANCELLED' if digit=='0' else 'IVR_NO_ANSWER_FINAL'
        assert counted==1 and results[-1]['type']==expected and results[-1]['final'] and results[-1]['counted'],current
        assert len(playing)==7,(name,playing)
        for media,seg in zip(playing,fixture['segments']):
            if seg['kind']=='Fixed': assert media==f"ivr-seg-{region.lower()}-{seg['TextHash'][:16]}.slin",media
            else:
                dynamic=next(d for d in measured['dynamic'] if d['ordinal']==seg['Ordinal'])
                assert media.startswith('generated/') and dynamic['pcm_sha256'].startswith(media.split('/')[1].split('.')[0]),media
        assert attempts[-1]['dtmf']==digit and attempts[-1]['region']==region and attempts[-1]['voice']==measured['voice']
        trace=cmd('docker','exec',SWITCH,'cat','/var/log/asterisk/w0344-dtmf')
        call_channels=set(re.findall(r'<(PJSIP/LAB-A-[^>]+)> Playing',logs)); assert len(call_channels)==1
        channel=next(iter(call_channels)); received=[s for s in trace.splitlines() if f'received on {channel},' in s and 'DTMF end ' in s]
        item['received_dtmf']=received
        if digit is not None:
            assert sent and len(received)==1 and f"DTMF end '{digit}'" in received[0]
            rtp=re.search(r'switch-[a-f0-9]+\s+\S+\s+ulaw\s+(\d+)',item['rtp'])
            assert rtp and int(rtp[1])>=measured['segmented_audio_ms']//20
        else: assert not received
        if fault=='tts-timeout':
            assert item.get('failure_before_dial') and len(attempts)==2 and len(results)==2,current
            assert attempts[0]['exception']=='TTS_TIMEOUT' and not attempts[0]['counted']
            assert [a['number'] for a in attempts]==[1,1]
            assert results[0]['type']=='IVR_TECHNICAL_EXCEPTION' and not results[0]['counted'] and not results[0]['final']
        else: assert len(attempts)==1 and len(results)==1,current
    item['pass']=True; save(); print('W0344_CASE_PASS '+name,flush=True); time.sleep(2)

if not RESUME: result['started_utc']=utc()
try:
    if not RESUME: setup_peer()
    else:
        assert json.loads(cmd('docker','inspect',PEER))[0]['Config']['Labels']['ivr.work']=='W0344'
        assert not channels(SWITCH) and not channels(PEER)
        assert sql("SELECT count(*) FROM ivr_call_jobs WHERE status NOT IN ('RESULT_READY_FOR_CALLBACK','WINDOW_EXPIRED','CAPACITY_MISSED','HELD_ADMIN_REVIEW')")=='0'
    cases=[('confirm','North','MultiItem','1',None),('customer-cancel','South','LongName','0',None),('technical-retry','Central','MultiItem','1','tts-timeout'),('queued-expiry','North','B',None,'queued-expiry'),('tts-expiry','South','MultiItem',None,'tts-expiry'),('no-input','North','A',None,None),('operator-cancel','Central','LongName',None,'operator-cancel')]
    completed={c['case'] for c in result['cases'] if c['pass']}
    for case in cases:
        if case[0] not in completed: run_case(*case)
    result['status']='PASS'
except BaseException as error:
    result['status']='FAIL'; result['error']=repr(error); raise
finally:
    for name in list(paused): unpause(name)
    result['ended_utc']=utc(); save()
    print('W0344_RUN_'+result['status'],flush=True)
