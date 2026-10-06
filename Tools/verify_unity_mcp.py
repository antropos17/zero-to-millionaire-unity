"""Bounded standard MCP handshake/list/read verification, no AI agent invocation."""
from pathlib import Path
import json,os,queue,subprocess,sys,threading,time

root=Path(__file__).resolve().parents[1]
cli=Path(os.environ.get('ZTM_UNITY_CLI',str(root/'.tooling/unity-cli/bin/unity.exe')))
env=dict(os.environ,UNITY_NO_CONSENT_PROMPT='1',UNITY_NO_CLI_INVOKED_TELEMETRY='1',UNITY_NO_CRASH_REPORT='1')
messages=queue.Queue()
error_log=open(root/'Evidence/unity-mcp-probe-stderr.log','w')
process=subprocess.Popen([str(cli),'mcp','--project-path',str(root)],stdin=subprocess.PIPE,stdout=subprocess.PIPE,
                         stderr=error_log,text=True,encoding='utf-8',env=env,cwd=root)
def consume():
    for line in process.stdout:
        try: messages.put(json.loads(line))
        except json.JSONDecodeError: pass
threading.Thread(target=consume,daemon=True).start()
def request(identifier,method,params):
    process.stdin.write(json.dumps({'jsonrpc':'2.0','id':identifier,'method':method,'params':params})+'\n');process.stdin.flush()
    deadline=time.monotonic()+30
    while time.monotonic()<deadline:
        try: reply=messages.get(timeout=min(1,deadline-time.monotonic()))
        except queue.Empty:
            if process.poll() is not None:raise RuntimeError('MCP process exited '+str(process.returncode))
            continue
        if reply.get('id')==identifier:
            if 'error' in reply:raise RuntimeError(json.dumps(reply['error']))
            return reply['result']
    raise TimeoutError(method+' exceeded30s')
try:
    initialized=request(1,'initialize',{'protocolVersion':'2025-03-26','capabilities':{},
        'clientInfo':{'name':'zero-to-millionaire-local-verify','version':'0.2.0'}})
    process.stdin.write(json.dumps({'jsonrpc':'2.0','method':'notifications/initialized'})+'\n');process.stdin.flush()
    catalog=request(2,'tools/list',{})
    if not catalog.get('tools'):
        # Editor discovery is asynchronous. Wait for its catalog notification,
        # then one supported refresh rather than reporting an empty warmup list.
        deadline=time.monotonic()+12
        while time.monotonic()<deadline:
            try:notification=messages.get(timeout=min(1,deadline-time.monotonic()))
            except queue.Empty:continue
            if notification.get('method')=='notifications/tools/list_changed':break
        catalog=request(3,'tools/list',{})
    receipt={'initialize':{'protocolVersion':initialized.get('protocolVersion'),'serverInfo':initialized.get('serverInfo')},
             'tools':catalog.get('tools',[])}
    (root/'Evidence/unity-mcp-catalog.json').write_text(json.dumps(receipt,indent=2))
    print(json.dumps({'initialize':receipt['initialize'],'tool_count':len(receipt['tools']),
                     'first_tools':[tool['name'] for tool in receipt['tools'][:5]]}))
    if len(sys.argv)>1:
        tool=sys.argv[1]; arguments=json.loads(sys.argv[2]) if len(sys.argv)>2 else {}
        result=request(4,'tools/call',{'name':tool,'arguments':arguments})
        (root/'Evidence/unity-mcp-read.json').write_text(json.dumps({'tool':tool,'result':result},indent=2))
        (root/'Evidence'/('unity-mcp-read-'+tool+'.json')).write_text(json.dumps({'tool':tool,'result':result},indent=2))
        print(json.dumps({'called':tool,'isError':result.get('isError',False),'result':result}))
finally:
    process.stdin.close()
    try:process.wait(timeout=3)
    except subprocess.TimeoutExpired:process.terminate();process.wait(timeout=3)
    error_log.close()
