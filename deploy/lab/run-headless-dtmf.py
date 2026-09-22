"""Opt-in Windows lab probe: real SIP/RTP peer, no sound device or customer destination.

Temporarily routes the pinned LAB-A alias to a dedicated Asterisk peer. Restores the
exact original PJSIP file in finally. No ARI DTMF injection and no database writes.
Run from the repository root while the disposable softphone lab is idle.
"""
from __future__ import annotations
import argparse
import datetime as dt
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import time

SWITCH = 'ginsengfood-ivr-dev-asterisk-1'
DB = 'ginsengfood-ivr-dev-postgres-1'
PEER = 'ivr-headless-dtmf-peer'
NETWORK = 'ginsengfood-ivr-dev_ivr-database-local'


def command(*args: str) -> str:
    return subprocess.check_output(args, encoding='utf-8', errors='strict', stderr=subprocess.STDOUT).strip()


def cli(container: str, text: str) -> str:
    return command('docker', 'exec', container, 'asterisk', '-rx', text)


def sql(text: str) -> str:
    return command('docker', 'exec', DB, 'psql', '-U', 'ivr', '-d', 'ivr', '-Atc', text)


def utc() -> str:
    return dt.datetime.now(dt.timezone.utc).isoformat()


def channels(container: str) -> list[str]:
    return [line.split('!')[0] for line in cli(container, 'core show channels concise').splitlines() if '!' in line]


def run_headless_matrix() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out', required=True)
    parser.add_argument('--measurements', required=True)
    parser.add_argument('--texts', required=True)
    parser.add_argument('--smoke-only', action='store_true')
    parser.add_argument('--no-input-only', action='store_true')
    parser.add_argument('--final-profile', action='store_true', help='Require W-0338 measured deadline/quota profile')
    parser.add_argument('--fault-timeout', action='store_true', help='One cold smoke call: pause TTS until the first technical failure, then recover')
    parser.add_argument('--regions', nargs='+', choices=['North','Central','South'], default=['North','Central','South'])
    args = parser.parse_args()
    assert not (args.smoke_only and args.no_input_only)
    assert not args.fault_timeout or (args.smoke_only and args.final_profile)
    out = Path(args.out).resolve()
    out.relative_to(Path('.artifacts').resolve())
    out.mkdir(parents=True, exist_ok=False)
    measurements = json.loads(Path(args.measurements).read_text(encoding='utf-8-sig'))['cases']
    fixtures = json.loads(Path(args.texts).read_text(encoding='utf-8-sig'))
    assert fixtures['fixtureOnly'] is True and fixtures['realCustomerCallAllowed'] == 'NO'
    assert not channels(SWITCH), 'Lab has active channels'
    # HELD_ADMIN_REVIEW cannot dispatch until an operator acts. Preserve it as failure evidence.
    assert sql("SELECT count(*) FROM ivr_call_jobs WHERE status NOT IN ('RESULT_READY_FOR_CALLBACK','WINDOW_EXPIRED','HELD_ADMIN_REVIEW')") == '0', 'Unfinished jobs'
    for service in ('ivr-worker', 'ivr-api'):
        env = json.loads(command('docker', 'inspect', f'ginsengfood-ivr-dev-{service}-1'))[0]['Config']['Env']
        assert 'IVR_EXECUTION_MODE=LAB_REAL_SIM' in env and 'REAL_CUSTOMER_CALL_ALLOWED=NO' in env
        assert 'Ivr__Telephony__Asterisk__DestinationAlias=LAB-A' in env
        assert 'Ivr__Telephony__SipTrunk__Enabled=true' not in env
    worker_name = 'ginsengfood-ivr-dev-ivr-worker-1'
    tts_name = 'ginsengfood-ivr-dev-ivr-tts-1'
    worker_info = json.loads(command('docker', 'inspect', worker_name))[0]
    tts_info = json.loads(command('docker', 'inspect', tts_name))[0]
    assert not worker_info['State']['Paused'] and not tts_info['State']['Paused']
    deadline_env = {line.split('=', 1)[0]: line.split('=', 1)[1] for line in worker_info['Config']['Env']
                    if line.startswith(('Ivr__Speech__Tts__TimeoutMilliseconds=', 'Ivr__Speech__Tts__Preparation',
                                        'Ivr__Scheduler__LeaseDurationSeconds='))}
    if args.final_profile:
        assert deadline_env == {
            'Ivr__Speech__Tts__TimeoutMilliseconds': '30000',
            'Ivr__Speech__Tts__PreparationQueueLimit': '8',
            'Ivr__Speech__Tts__PreparationQueueTimeoutMilliseconds': '90000',
            'Ivr__Speech__Tts__PreparationTimeoutMilliseconds': '120000',
            'Ivr__Scheduler__LeaseDurationSeconds': '360'}, deadline_env
        assert tts_info['HostConfig']['NanoCpus'] == 2000000000 and tts_info['HostConfig']['Memory'] == 4 * 1024**3
        assert tts_info['HostConfig']['MemorySwap'] == 4 * 1024**3
    command('node', 'deploy/lab/check-speech-preflight.mjs', '--timeout-seconds', '180')
    switch = json.loads(command('docker','inspect', SWITCH))[0]
    image = switch['Image']
    switch_ip = switch['NetworkSettings']['Networks'][NETWORK]['IPAddress']
    assert re.fullmatch(r'[0-9.]+', switch_ip)
    backup = out/'pjsip-original.conf'
    command('docker', 'cp', f'{SWITCH}:/etc/asterisk/pjsip.conf', str(backup))
    original_hash = hashlib.sha256(backup.read_bytes()).hexdigest()
    peer_config = out/'peer'; peer_config.mkdir()
    (peer_config/'pjsip.conf').write_text(f'''[transport-udp]
type=transport
protocol=udp
bind=0.0.0.0:5060
[switch]
type=endpoint
transport=transport-udp
context=automatic-answer
disallow=all
allow=ulaw
direct_media=no
dtmf_mode=rfc4733
[switch-identify]
type=identify
endpoint=switch
match={switch_ip}
''')
    (peer_config/'extensions.conf').write_text('''[general]
static=yes
writeprotect=yes
[automatic-answer]
exten => LAB-A,1,Answer()
 same => n,Verbose(1,HEADLESS_ANSWER)
 same => n,Wait(120)
 same => n,Hangup()
[send-1]
exten => s,1,Verbose(1,HEADLESS_SEND_DTMF_1)
 same => n,SendDTMF(1,250,160)
 same => n,Wait(10)
 same => n,Hangup()
[send-0]
exten => s,1,Verbose(1,HEADLESS_SEND_DTMF_0)
 same => n,SendDTMF(0,250,160)
 same => n,Wait(10)
 same => n,Hangup()
''')
    (peer_config/'http.conf').write_text('[general]\nenabled=no\n')
    (peer_config/'logger.conf').write_text('[general]\ndateformat=%F %T\n[logfiles]\nconsole=notice,warning,error,verbose,dtmf\n')
    result = {'scope':'LOCAL_SYNTHETIC_SIP','real_customer_call_allowed':'NO',
              'human_listening':'OWNER_ACCEPTED_NO_RELISTEN', 'started_utc':utc(),
              'switch_image':image,'original_config_sha256':original_hash, 'calls':[], 'restored':False}
    result.update(worker_image=worker_info['Image'], tts_image=tts_info['Image'], deadline_configuration=deadline_env,
                  fault_timeout=args.fault_timeout)
    created = False
    switched = False
    worker_paused = False
    tts_paused = False
    try:
        mounts = []
        for name in ('pjsip.conf','extensions.conf','http.conf','logger.conf'):
            mounts += ['--mount', f'type=bind,src={peer_config/name},dst=/etc/asterisk/{name},readonly']
        command('docker','run','-d','--name',PEER,'--network',NETWORK,'--cap-drop','ALL',
                '--security-opt','no-new-privileges',*mounts,'--entrypoint','/usr/sbin/asterisk',image,'-f','-vvv')
        created = True
        peer_ip = json.loads(command('docker','inspect',PEER))[0]['NetworkSettings']['Networks'][NETWORK]['IPAddress']
        assert re.fullmatch(r'[0-9.]+',peer_ip)
        config = out/'pjsip-automatic.conf'
        original = backup.read_text(encoding='utf-8-sig')
        assert original.count('aors=LAB-A')==1 and original.count('media_address=127.0.0.1')==1
        # Keep the user's registration intact; only the outbound endpoint's AOR changes.
        automatic = original.replace('aors=LAB-A','aors=LAB-A-AUTO',1).replace('media_address=127.0.0.1',f'media_address={switch_ip}',1)
        config.write_text(automatic + f'''\n[LAB-A-AUTO]
type=aor
max_contacts=0
contact=sip:LAB-A@{peer_ip}:5060
qualify_frequency=2
''')
        result['contacts_before'] = cli(SWITCH,'pjsip show contacts')
        command('docker','cp',str(config),f'{SWITCH}:/etc/asterisk/pjsip.conf')
        switched = True
        cli(SWITCH,'module reload res_pjsip.so')
        cli(SWITCH,'logger add channel /var/log/asterisk/headless-dtmf dtmf')
        time.sleep(3)
        result['contacts_automatic'] = cli(SWITCH,'pjsip show contacts')
        # The outbound AOR contains only the silent peer, never the user's registered phone.
        endpoint = cli(SWITCH,'pjsip show endpoint LAB-A')
        assert re.search(r'(?m)^\s*aors\s*:\s*LAB-A-AUTO\s*$',endpoint), endpoint
        contact_rows = [s for s in result['contacts_automatic'].splitlines() if 'Contact:  LAB-A-AUTO/' in s]
        assert len(contact_rows)==1 and f'@{peer_ip}:5060' in contact_rows[0], result['contacts_automatic']
        cases = [(r,v,'1' if v=='MultiItem' else '0') for r in args.regions for v in ('MultiItem','LongName')]
        if args.smoke_only:
            cases = cases[:1]
        elif args.no_input_only:
            cases = [('North','A',None)]
        else:
            cases.append(('North','A',None))
        for region, variant, digit in cases:
            assert not channels(SWITCH) and not channels(PEER)
            case = next(c for c in measurements if c['region']==region and c['variant']==variant)
            fixture = next(c for c in fixtures['cases'] if c['region']==region and c['variant']==variant)
            case_id = f'{region}-{variant}-'+(digit or 'no-input')
            item = {'case':case_id,'region':region,'variant':variant,'digit':digit,'started_utc':utc(),
                    'audio_ms':case['segmented_audio_ms'],'sent_utc':None,'pass':False}
            result['calls'].append(item)
            if args.fault_timeout:
                # Keep the worker idle while the guarded intake completes. No DB row is changed.
                command('docker', 'pause', worker_name)
                worker_paused = True
            with (out/f'{case_id}.log').open('w',encoding='utf-8') as log:
                proc = subprocess.Popen([shutil.which('pwsh') or 'pwsh','-NoProfile','-File',
                    'deploy/lab/Invoke-FreeSoftphoneCall.ps1','-NoUi','-HeadlessPeer','-Region',region,
                    '-OrderVariant',variant,'-ResultTimeoutSeconds','180'],stdout=log,stderr=subprocess.STDOUT)
                deadline=time.monotonic()+360
                up_at=None; peer_channel=None
                while time.monotonic()<deadline:
                    if args.fault_timeout:
                        submitted=re.findall(r'TASK-LAB-\d+', (out/f'{case_id}.log').read_text(encoding='utf-8-sig'))
                        if submitted and worker_paused:
                            command('docker', 'pause', tts_name)
                            tts_paused = True
                            command('docker', 'unpause', worker_name)
                            worker_paused = False
                            item['fault_started_utc'] = utc()
                        if submitted and tts_paused:
                            failed=sql(f"SELECT count(*) FROM ivr_call_attempts WHERE task_id='{submitted[-1]}' AND technical_exception_type='TTS_TIMEOUT' AND is_counted_customer_attempt IS FALSE")
                            if failed == '1':
                                assert not channels(SWITCH), 'Timeout must precede dial'
                                item['fault_failed_before_dial'] = True
                                command('docker', 'unpause', tts_name)
                                tts_paused = False
                                item['fault_released_utc'] = utc()
                    if proc.poll() is not None:
                        # The intake script reports the first result, including a technical
                        # retry. Keep the peer/route alive until the job can no longer dial.
                        submitted=re.findall(r'TASK-LAB-\d+', (out/f'{case_id}.log').read_text(encoding='utf-8-sig'))
                        if not submitted:
                            break
                        job_status=sql(f"SELECT status FROM ivr_call_jobs WHERE task_id='{submitted[-1]}'")
                        item['job_status']=job_status
                        if job_status in ('RESULT_READY_FOR_CALLBACK','WINDOW_EXPIRED','HELD_ADMIN_REVIEW'):
                            break
                    peers=channels(PEER)
                    assert len(peers)<=1, 'Unexpected peer channels'
                    if peers and up_at is None:
                        peer_channel=peers[0]; up_at=time.monotonic(); item['answered_utc']=utc()
                        print(f'HEADLESS_ANSWER case={case_id}',flush=True)
                    if peers and digit is not None and item['sent_utc'] is None and time.monotonic()-up_at >= case['segmented_audio_ms']/1000+2:
                        item['rtp_switch']=cli(SWITCH,'pjsip show channelstats')
                        item['rtp_peer']=cli(PEER,'pjsip show channelstats')
                        item['sent_utc']=utc()
                        item['send_result']=cli(PEER,f'channel redirect {peer_channel} send-{digit},s,1')
                        print(f'HEADLESS_DTMF case={case_id} digit={digit}',flush=True)
                    time.sleep(0.4)
                else:
                    if proc.poll() is None:
                        proc.terminate(); proc.wait(10)
                    raise RuntimeError(f'Own call/job timed out: {case_id}')
                item['exit_code']=proc.returncode
            item['ended_utc']=utc()
            script_log=(out/f'{case_id}.log').read_text(encoding='utf-8-sig')
            matches=re.findall(r'TASK-LAB-\d+',script_log)
            assert matches, script_log[-1500:]
            task=matches[-1]; item['task_id']=task
            item['attempts']=json.loads(sql(f"SELECT COALESCE(json_agg(json_build_object('counted',is_counted_customer_attempt,'exception',technical_exception_type,'attempt_no',attempt_number) ORDER BY ended_at),'[]'::json) FROM ivr_call_attempts WHERE task_id='{task}'"))
            if args.fault_timeout:
                assert item.get('fault_failed_before_dial') is True
                assert len(item['attempts']) == 2, item['attempts']
                assert item['attempts'][0]['exception'] == 'TTS_TIMEOUT' and not item['attempts'][0]['counted']
                assert item['attempts'][1]['exception'] is None and item['attempts'][1]['counted']
                assert item['attempts'][0]['attempt_no'] == item['attempts'][1]['attempt_no'] == 1
            item['attempt']=json.loads(sql(f"SELECT json_build_object('status',status,'dtmf',dtmf_key,'voice_id',voice_id,'region',voice_region,'region_resolved',voice_region_resolved,'started_at',started_at,'ended_at',ended_at,'counted',is_counted_customer_attempt,'exception',technical_exception_type) FROM ivr_call_attempts WHERE task_id='{task}' ORDER BY ended_at DESC LIMIT 1"))
            item['result']=json.loads(sql(f"SELECT json_build_object('type',r.result_type,'final',r.is_final_for_ivr,'counted',r.is_counted_customer_attempt) FROM ivr_call_results r JOIN ivr_call_jobs j ON j.ivr_call_job_id=r.ivr_call_job_id WHERE j.task_id='{task}' ORDER BY r.created_at DESC LIMIT 1"))
            logs=command('docker','logs',SWITCH,'--timestamps','--since',item['started_utc'],'--until',item['ended_utc'])
            (out/f'{case_id}-switch.log').write_text(logs,encoding='utf-8')
            peer_logs=command('docker','logs',PEER,'--timestamps','--since',item['started_utc'],'--until',item['ended_utc'])
            (out/f'{case_id}-peer.log').write_text(peer_logs,encoding='utf-8')
            playing=re.findall(r"Playing '([^']+)'",logs)
            item['media']=playing
            switch_channels=set(re.findall(r"<(PJSIP/LAB-A-[^>]+)> Playing",logs))
            assert len(switch_channels)==1,switch_channels
            switch_channel=switch_channels.pop()
            trace=command('docker','exec',SWITCH,'cat','/var/log/asterisk/headless-dtmf')
            received=[line for line in trace.splitlines() if f'received on {switch_channel},' in line and 'DTMF end ' in line]
            item['received_dtmf_trace']=received
            assert len(playing)==7,(case_id,playing)
            for media,seg in zip(playing,fixture['segments']):
                if seg['kind']=='Fixed':
                    assert media==f"ivr-seg-{region.lower()}-{seg['TextHash'][:16]}.slin"
                else:
                    dynamic=next(d for d in case['dynamic'] if d['ordinal']==seg['Ordinal'])
                    assert media.startswith('generated/') and dynamic['pcm_sha256'].startswith(media.split('/')[1].split('.')[0])
            attempt=item['attempt']
            assert attempt['dtmf']==digit and attempt['voice_id']==case['voice'] and attempt['region']==region
            assert attempt['status']=='NORMALIZED_FINAL' and attempt['exception'] is None and attempt['counted']
            parse=lambda s:dt.datetime.fromisoformat(s.replace('Z','+00:00'))
            item['active_ms']=round((parse(attempt['ended_at'])-parse(attempt['started_at'])).total_seconds()*1000)
            assert item['active_ms']>=case['segmented_audio_ms']
            expected='IVR_CONFIRMED' if digit=='1' else 'IVR_CUSTOMER_CANCELLED' if digit=='0' else 'IVR_NO_ANSWER_FINAL'
            assert item['result']=={'type':expected,'final':True,'counted':True},item['result']
            if digit is not None:
                assert item['sent_utc'] is not None and f'HEADLESS_SEND_DTMF_{digit}' in peer_logs
                assert len(received)==1 and f"DTMF end '{digit}'" in received[0],received
                received_at=dt.datetime.fromisoformat(received[0][1:20]).replace(tzinfo=dt.timezone.utc)
                assert -1 <= (received_at-parse(item['sent_utc'])).total_seconds() < 3
                rtp=re.search(r'switch-[a-f0-9]+\s+\S+\s+ulaw\s+(\d+)',item['rtp_peer'])
                assert rtp and int(rtp[1])>=case['segmented_audio_ms']//20, item['rtp_peer']
            else:
                assert item['sent_utc'] is None and 'HEADLESS_SEND_DTMF_' not in peer_logs
                assert received==[],received
            assert proc.returncode==0
            item['pass']=True
            (out/'result.json').write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
            print(f'HEADLESS_PASS case={case_id} result={expected}',flush=True)
            time.sleep(2)
    finally:
        if tts_paused:
            command('docker', 'unpause', tts_name)
        if worker_paused:
            command('docker', 'unpause', worker_name)
        if created:
            for channel in channels(PEER):
                cli(PEER,f'channel request hangup {channel}')
            time.sleep(1)
        if switched:
            command('docker','cp',f'{SWITCH}:/var/log/asterisk/headless-dtmf',str(out/'dtmf-received.log'))
            cli(SWITCH,'logger remove channel /var/log/asterisk/headless-dtmf')
            command('docker','cp',str(backup),f'{SWITCH}:/etc/asterisk/pjsip.conf')
            cli(SWITCH,'module reload res_pjsip.so')
            actual=command('docker','exec',SWITCH,'sha256sum','/etc/asterisk/pjsip.conf').split()[0]
            assert actual==original_hash,'Restore hash mismatch'
            time.sleep(1)
            assert re.search(r'(?m)^\s*aors\s*:\s*LAB-A\s*$',cli(SWITCH,'pjsip show endpoint LAB-A'))
            result['restored']=True
        if created:
            command('docker','rm','-f',PEER)
        result['ended_utc']=utc()
        (out/'result.json').write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(f'HEADLESS_MATRIX_PASS calls={len(result["calls"])} restored={result["restored"]}',flush=True)


if __name__=='__main__':
    run_headless_matrix()
