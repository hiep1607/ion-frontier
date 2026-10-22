"""Package0.11.1 from explicit current Player QA reports; never invent passed checks.
Usage: python package_release.py --qa-manifest outputs/Combat-Clarity/qa-inputs.json
Manifest: {"build":"Builds/Combat-Clarity","buildSha256":"...", "reports":
 {"playtest":"outputs/.../playtest-report.json",...},"provenance":"...",
 "limitations":["..."]}. Required reports: playtest,campaign,vitals,rating,escape,tactical.
All nine reports are required. Root agent owns AGENTS.md; this script never changes it.
"""
from pathlib import Path
import argparse, hashlib, json, shutil, zipfile

def digest(path):
    h=hashlib.sha256()
    with path.open('rb') as stream:
        for block in iter(lambda:stream.read(1024*1024),b''): h.update(block)
    return h.hexdigest()

def main():
    parser=argparse.ArgumentParser();parser.add_argument('--qa-manifest',required=True);args=parser.parse_args()
    root=Path(__file__).resolve().parents[2];manifest_path=Path(args.qa_manifest)
    if not manifest_path.is_absolute(): manifest_path=root/manifest_path
    manifest=json.loads(manifest_path.read_text(encoding='utf-8-sig'))
    build=(root/manifest.get('build','Builds/Combat-Clarity')).resolve();qa=root/'outputs/Combat-Clarity'
    assert build==root/'Builds/Combat-Clarity','Only package the new isolated edition'
    exe=build/'IonFrontier.exe';assert exe.is_file(),exe
    assert digest(exe)==manifest['buildSha256'],'Build executable does not match QA manifest'
    assembly=build/'IonFrontier_Data/Managed/Assembly-CSharp.dll'
    assert digest(assembly)==manifest['assemblySha256'],'Game code does not match the tested build'
    assert manifest.get('provenance','').strip(),'Describe which build(s) produced every report'
    required={'playtest','campaign','vitals','rating','escape','tactical','clarity','input','stress'}
    assert required<=set(manifest['reports']),'Missing baseline or tactical report'
    reports={};paths={};frame_metrics={}
    for name,value in manifest['reports'].items():
        path=(root/value).resolve();report=json.loads(path.read_text(encoding='utf-8-sig'))
        assert report.get('passed') is True and not report.get('errors'),(name,report.get('errors'))
        assert report.get('checks') and all(c.startswith('PASS: ') for c in report['checks']),(name,'Non-passing or empty checks')
        reports[name]=report;paths[name]=str(path.relative_to(root));metrics=path.parent/'frame-metrics.json'
        if metrics.exists():frame_metrics[name]=json.loads(metrics.read_text(encoding='utf-8-sig'))
    qa.mkdir(parents=True,exist_ok=True)
    limitations=[
        'QA dùng profile riêng. Harness hồi quy tự chọn thẻ theo cờ test; bản chơi thường và mở đủ tính năng vẫn do người chơi chọn.',
        'Escape có lượt chạy tự nhiên toàn tuyến với bảo vệ tàu và không bắn; kết quả chưa xác nhận cân bằng chơi tay.',
        'Module được kiểm qua bắn, va chạm, damage và hút quà thật bằng fixture kiểm soát. Campaign dùng hook tăng tốc mục tiêu/boss.',
        'Lượt Ion cơ bản dùng di chuyển/bắn thật trong cửa sổ phản công; kiểm tra súng/kiểu/chỉ số/module vẫn ở mức đầu. FPS áp dụng cho máy và điều kiện harness hiện tại.'
    ]+manifest.get('limitations',[])
    checks=[f'{name}: {check}' for name,report in reports.items() for check in report['checks']]
    aggregate={'version':'0.11.1','passed':True,'totalChecks':len(checks),'checks':checks,'errors':[],
        'reports':paths,'reportSha256':{name:digest(root/path) for name,path in paths.items()},
        'averageFps':{name:frame_metrics.get(name,{}).get('meanFps',report.get('averageFps')) for name,report in reports.items()},'frameMetrics':frame_metrics,
        'maxProjectiles':{name:report.get('maxProjectiles') for name,report in reports.items()},
        'tacticalMaxEnemies':reports['tactical'].get('maxEnemies'),'tacticalMaxWorld':reports['tactical'].get('maxWorld'),
        'buildSha256':manifest['buildSha256'],'assemblySha256':manifest['assemblySha256'],'provenance':manifest['provenance'],'limitations':limitations}
    previous=root/'TEST-REPORT.json'
    if previous.exists():
        old=json.loads(previous.read_text(encoding='utf-8-sig'));version=old.get('version','unknown')
        archive=qa/f'TEST-REPORT-{version}.json'
        if version!='0.11.1' and not archive.exists(): shutil.copy2(previous,archive)
    previous.write_text(json.dumps(aggregate,ensure_ascii=False,indent=2),encoding='utf-8')
    notes='# QA0.11.1 — Chiến đấu rõ ràng\n\n'+manifest['provenance']+'\n\n'
    for name,report in reports.items(): notes+=f'- {name}: {len(report["checks"])} PASS, {frame_metrics.get(name,{}).get('meanFps',report.get('averageFps',0)):.1f} FPS. Report: `{paths[name]}`.\n'
    notes+='\nĐỉnh lấy mẫu theo frame: tactical '+str(reports['tactical'].get('maxProjectiles'))+' projectile / '+str(reports['tactical'].get('maxEnemies'))+' địch / '+str(reports['tactical'].get('maxWorld'))+' vật thể world. Đầy pool512 còn có probe riêng kiểm không tiêu ammo/buff.\n'
    notes+='\nFrame time toàn bộ sau warm-up1,5s (không bỏ frame chậm):\n'
    for name,metrics in frame_metrics.items():notes+=f'- {name}: p95={metrics["p95ms"]:.2f}ms; p99={metrics["p99ms"]:.2f}ms; max={metrics["maxMs"]:.2f}ms; >100ms={metrics["over100ms"]}; foreground FPS={metrics["focusedMeanFps"]:.2f}; background frames={metrics["backgroundFrames"]}.\n'
    notes+='\nGC được ghi theo thế hệ và peak managed memory; API allocation main-thread của Mono trên máy này trả0 nên ghi allocationSupported=false / bytes=-1, không coi là không cấp phát.\n'
    notes+=f'\nTổng {len(checks)} kiểm tra thực tế. Không cộng kết quả các phiên bản cũ.\n\n'
    notes+='\n'.join('- '+item for item in limitations)+'\n\nZIP được kiểm CRC và SHA256 mọi file. Không tự push GitHub.\n'
    if manifest.get('performanceComparison'):notes+='\nĐối chiếu FPS0.10/0.11 trên cùng máy: `'+manifest['performanceComparison']+'`. Đây là so sánh harness Vitals, không là benchmark mọi tình huống chiến đấu.\n'
    (qa/'QA-NOTES.md').write_text(notes,encoding='utf-8')
    for name in ('README.md','CAMPAIGN.md','GAME-HANDBOOK-VI.md','TEST-REPORT.json'):shutil.copy2(root/name,build/name)
    shutil.copy2(qa/'QA-NOTES.md',build/'QA-NOTES.md');shutil.copy2(root/'Assets/IonFrontier/Resources/Interface-OFL.txt',build/'Interface-OFL.txt')
    shutil.copy2(root/'Design/UPDATE-0.11.1-IMPLEMENTATION.md',build/'UPDATE-0.11.1-IMPLEMENTATION.md')
    shutil.copy2(qa/'BALANCE-NOTES.md',build/'BALANCE-NOTES.md')
    shutil.copy2(qa/'Balance-Release/balance-report.json',build/'balance-report.json')
    (build/'Choi-game.cmd').write_text('@echo off\nstart "ION FRONTIER" "%~dp0IonFrontier.exe"\n',encoding='ascii')
    (build/'Choi-thu-mo-tat-ca.cmd').write_text('@echo off\nstart "ION FRONTIER - Preview" "%~dp0IonFrontier.exe" -ion-unlocked\n',encoding='ascii')
    (build/'HUONG-DAN.txt').write_text('''ION FRONTIER0.11.1 — Chiến đấu rõ ràng
Choi-game.cmd: chơi bằng lưu thật. Choi-thu-mo-tat-ca.cmd: mở13màn/8tàu bằng profile tạm và một Q đầu màn.
Giữ nguyên DLL và thư mục cạnh exe. Chuột/WASD/mũi tên: lái; Space/chuột trái: bắn; F: tự bắn; Shift/chuột phải: lướt; Q: đại chiêu; P/Esc: dừng; M: âm thanh.
Mỗi màn có hai lần chọn1 trong3thẻ: nhấn1/2/3 hoặc bấm thẻ. Combat dừng trong lúc chọn. Tối đa hai mô-đun, reset khi sang màn/chơi lại. Kiểu bắn ghép tối đa hai, nâng cấp chỉ số độc lập.
Hai ô mô-đun ở góc dưới hiện tiến độ/hồi chiêu; khi thiếu kiểu bắn sẽ hiện rõ điều kiện. Thẻ súng hiển thị chỉ số trước–sau và mô-đun bị ngừng khi thay kiểu.
Màn4: sống sót qua bám đuôi/chặn sườn/đá cắt ngang; 50–64giây có thể phản công tàu hạng nặng. Phá hai động cơ18HP rồi thân42HP. Thắng thưởng600điểm+40đạn; bỏ qua vẫn tới boss cuối. Tới3600m rồi hạ boss mới qua màn.
Đội hình địch có tàu nguồn khiên/sửa/kéo/báo động: phá nguồn để ngắt hỗ trợ. Xem README/CAMPAIGN/GAME-HANDBOOK-VI cho chi tiết, QA-NOTES cho phạm vi kiểm tra.
BALANCE-NOTES ghi khảo sát sáu lượt bot không bất tử; đây chưa phải xác nhận cân bằng chơi tay. Không cộng các lượt quan sát vào số kiểm tra PASS.
''',encoding='utf-8')
    files=sorted(p for p in build.rglob('*') if p.is_file() and p.name!='SHA256SUMS.json')
    hashes={p.relative_to(build).as_posix():digest(p) for p in files}
    (build/'SHA256SUMS.json').write_text(json.dumps(hashes,indent=2),encoding='utf-8')
    target=root/'outputs/IonFrontier-Chien-Dau-Ro-Rang-0.11.1-Windows.zip'
    with zipfile.ZipFile(target,'w',zipfile.ZIP_DEFLATED,compresslevel=6) as archive:
        for file in sorted(build.rglob('*')):
            if file.is_file():archive.write(file,'IonFrontier/'+file.relative_to(build).as_posix())
    with zipfile.ZipFile(target) as archive:
        assert archive.testzip() is None
        for name,sha in hashes.items():assert hashlib.sha256(archive.read('IonFrontier/'+name)).hexdigest()==sha,name
    summary={'path':str(target),'bytes':target.stat().st_size,'files':len(hashes)+1,'sha256':digest(target),'crcAndAllFileHashesVerified':True,'tests':len(checks)}
    (qa/'package-verification.json').write_text(json.dumps(summary,indent=2),encoding='utf-8');print(json.dumps(summary,indent=2))
if __name__=='__main__': main()
