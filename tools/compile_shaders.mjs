import { readFile, writeFile } from 'node:fs/promises';
import { resolve } from 'node:path';
import { pathToFileURL } from 'node:url';

const [lub, output, sourcePrefix = 'shaders/mesh'] = process.argv.slice(2);
const directory = resolve(lub, 'web/public/slang');
const factory = (await import(pathToFileURL(`${directory}/slang-wasm.js`))).default;
const main = await factory({ wasmBinary: await readFile(`${directory}/slang-wasm.wasm`) });
const global = main.createGlobalSession();
const target = main.getCompileTargets().find(t => t.name === 'WGSL').value;
const session = global.createSession(target);
const result = {};
for (const [name, stage] of [['vs', 1], ['fs', 5]]) {
    const source = await readFile(`${sourcePrefix}.${name}.slang`, 'utf8');
    const module = session.loadModuleFromSource('#define LUB_VERTEX_ID SV_VertexID\n' + source, name, name + '.slang');
    if (!module) throw new Error(JSON.stringify(main.getLastError()));
    const entry = module.findAndCheckEntryPoint(`${name}_main`, stage);
    const composite = session.createCompositeComponentType([module, entry]);
    const linked = composite.link();
    if (!linked) throw new Error(JSON.stringify(main.getLastError()));
    const wgsl = linked.getEntryPointCode(0, 0).replace(/@group\(\d+\)(\s+var<storage[^>]*>)/g, '@group(1)$1').replace(/@group\(\d+\)(\s+var [^;]+:\s*(?:texture_\w+<[^>]+>|sampler))/g, '@group(1)$1');
    if (!wgsl || wgsl === 'Error') throw new Error(JSON.stringify(main.getLastError()));
    const layout = linked.getLayout(0);
    result[`${name}_main`] = { wgsl, reflectJson: JSON.stringify(layout.toJsonObject()) };
    layout.delete(); linked.delete(); composite.delete(); entry.delete(); module.delete();
}
await writeFile(output, JSON.stringify(result));
