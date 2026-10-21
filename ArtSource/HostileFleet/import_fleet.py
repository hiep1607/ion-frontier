from pathlib import Path
import json, uuid, hashlib
from PIL import Image

root = Path(__file__).resolve().parents[2]
art = root / 'Assets/IonFrontier/Art/HostileFleet'
manifest = []
for f in sorted(art.glob('*.png')):
    im = Image.open(f)
    assert im.mode == 'RGBA', f
    alpha = im.getchannel('A')
    histogram = alpha.histogram()
    assert alpha.getextrema()[0] == 0 and alpha.getextrema()[1] >= 250, f
    assert sum(histogram[:10]) / (im.width * im.height) > .25, f
    meta = f.with_suffix('.png.meta')
    guid = meta.read_text().split('guid: ')[1].splitlines()[0] if meta.exists() else uuid.uuid4().hex
    if not meta.exists():
        template = '''fileFormatVersion: 2
guid: GUID
TextureImporter:
  serializedVersion: 13
  internalIDToNameTable: []
  externalObjects: {}
  mipmaps:
    enableMipMap: 0
    sRGBTexture: 1
  isReadable: 1
  textureFormat: 1
  maxTextureSize: 1024
  textureSettings:
    filterMode: 1
    aniso: 1
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {x: 0.5, y: 0.5}
  spritePixelsToUnits: 100
  alphaUsage: 1
  alphaIsTransparency: 1
  textureType: 8
  textureShape: 1
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 1024
    textureFormat: -1
    textureCompression: 0
    overridden: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    physicsShape: []
    bones: []
    spriteID: SPRITEID
    internalID: 0
    secondaryTextures: []
    nameFileIdTable: {}
'''
        meta.write_text(template.replace('GUID', guid).replace('SPRITEID', uuid.uuid4().hex), encoding='utf-8')
    receipt = json.loads((root / 'ArtSource/HostileFleet/Receipts' / f.with_suffix('.json').name).read_text(encoding='utf-8-sig'))
    manifest.append(dict(name=f.stem, path=str(f.relative_to(root)), guid=guid, width=im.width, height=im.height,
                         transparentPercent=round(sum(histogram[:10])*100/(im.width*im.height), 2),
                         sha256=hashlib.sha256(f.read_bytes()).hexdigest(), prompt=receipt['prompt']))
assert len(manifest) == 28 and len({m['sha256'] for m in manifest}) == 28
library = root / 'Assets/IonFrontier/Resources/FleetLibrary.asset'
source = library.read_text(encoding='utf-8').split('  reinforcements:')[0]
for field, prefix in [('reinforcements', 'N'), ('bossFleet', 'B')]:
    source += '  '+field+':\n'+''.join('  - {fileID: 21300000, guid: '+m['guid']+', type: 3}\n' for m in manifest if m['name'].startswith(prefix))
library.write_text(source, encoding='utf-8')
(root / 'ArtSource/HostileFleet/manifest.json').write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding='utf-8')
print('Validated 28 distinct RGBA originals and registered 12 reinforcements / 16 boss sprites.')
