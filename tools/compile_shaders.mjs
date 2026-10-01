import { readFile, writeFile } from 'node:fs/promises';
import { resolve } from 'node:path';
import { pathToFileURL } from 'node:url';

const [lub, output, sourcePrefix = 'shaders/mesh', outputShader] = process.argv.slice(2);
const directory = resolve(lub, 'web/public/slang');
const factory = (await import(pathToFileURL(`${directory}/slang-wasm.js`))).default;
const main = await factory({ wasmBinary: await readFile(`${directory}/slang-wasm.wasm`) });
const global = main.createGlobalSession();
const target = main.getCompileTargets().find(t => t.name === 'WGSL').value;
const session = global.createSession(target);
const result = outputShader ? [] : {};
const stages = [[`${sourcePrefix}.vs.slang`, "vs", 1], [`${sourcePrefix}.fs.slang`, "fs", 5]];
if (outputShader) stages.push([outputShader, "vs", 1], [outputShader, "fs", 5]);
for (const [file, name, stage] of stages) {
    const source = await readFile(file, 'utf8');
    const module = session.loadModuleFromSource('#define LUB_VERTEX_ID SV_VertexID\n' + source, file, file);
    if (!module) throw new Error(JSON.stringify(main.getLastError()));
    const entry = module.findAndCheckEntryPoint(`${name}_main`, stage);
    const composite = session.createCompositeComponentType([module, entry]);
    const linked = composite.link();
    if (!linked) throw new Error(JSON.stringify(main.getLastError()));
    const wgsl = linked.getEntryPointCode(0, 0).replace(/@group\(\d+\)(\s+var<storage[^>]*>)/g, '@group(1)$1').replace(/@group\(\d+\)(\s+var [^;]+:\s*(?:texture_\w+<[^>]+>|sampler))/g, '@group(1)$1');
    if (!wgsl || wgsl === 'Error') throw new Error(JSON.stringify(main.getLastError()));
    const layout = linked.getLayout(0);
    const compiled = { wgsl, reflectJson: JSON.stringify(layout.toJsonObject()) };
    if (outputShader) result.push({ source, entry: `${name}_main`, ...compiled });
    else result[`${name}_main`] = compiled;
    layout.delete(); linked.delete(); composite.delete(); entry.delete(); module.delete();
}
await writeFile(output, JSON.stringify(result));
