import { readFile, writeFile } from 'node:fs/promises';
import { resolve } from 'node:path';
import { pathToFileURL } from 'node:url';

// compile_shaders.mjs <lub> <output> [prefix] [shader...]
// Without extra shaders the output maps each entry point of <prefix>.{vs,fs}.slang to its WGSL.
// With them it is a list matched by source text: a path adds both stages of that file, and
// NAME:path adds the fragment stage of the file compiled with NAME defined.
const [lub, output, sourcePrefix = 'shaders/mesh', ...extra] = process.argv.slice(2);
const directory = resolve(lub, 'web/public/slang');
const factory = (await import(pathToFileURL(`${directory}/slang-wasm.js`))).default;
const main = await factory({ wasmBinary: await readFile(`${directory}/slang-wasm.wasm`) });
const global = main.createGlobalSession();
const target = main.getCompileTargets().find(t => t.name === 'WGSL').value;
const session = global.createSession(target);
// The text lub puts before a shader on the web (prelude_for_target in its shader.cpp).
const prelude = '#define LUB_TEXTURE2D(n) Texture2D n; SamplerState n##_smp\n'
    + '#define LUB_SAMPLE(t, uv) t.Sample(t##_smp, uv)\n'
    + '#define LUB_SAMPLE_LOD(t, uv) t.SampleLevel(t##_smp, uv, 0.0)\n'
    + '#define LUB_VERTEX_ID SV_VertexID\n'
    + '#define LUB_INSTANCE_ID SV_InstanceID\n';
const list = extra.length > 0;
const result = list ? [] : {};
const stages = [[`${sourcePrefix}.vs.slang`, 'vs', 1, ''], [`${sourcePrefix}.fs.slang`, 'fs', 5, '']];
for (const shader of extra) {
    const [file, define] = shader.split(':').reverse();
    if (define) stages.push([file, 'fs', 5, define]);
    else stages.push([file, 'vs', 1, ''], [file, 'fs', 5, '']);
}
for (const [file, name, stage, define] of stages) {
    const source = (define && `#define ${define}\n`) + await readFile(file, 'utf8');
    const module = session.loadModuleFromSource(prelude + source, file + define, file + define);
    if (!module) throw new Error(JSON.stringify(main.getLastError()));
    const entry = module.findAndCheckEntryPoint(`${name}_main`, stage);
    const composite = session.createCompositeComponentType([module, entry]);
    const linked = composite.link();
    if (!linked) throw new Error(JSON.stringify(main.getLastError()));
    const wgsl = linked.getEntryPointCode(0, 0).replace(/@group\(\d+\)(\s+var<storage[^>]*>)/g, '@group(1)$1').replace(/@group\(\d+\)(\s+var [^;]+:\s*(?:texture_\w+<[^>]+>|sampler))/g, '@group(1)$1');
    if (!wgsl || wgsl === 'Error') throw new Error(JSON.stringify(main.getLastError()));
    const layout = linked.getLayout(0);
    const compiled = { wgsl, reflectJson: JSON.stringify(layout.toJsonObject()) };
    if (list) result.push({ source, entry: `${name}_main`, ...compiled });
    else result[`${name}_main`] = compiled;
    layout.delete(); linked.delete(); composite.delete(); entry.delete(); module.delete();
}
// The page takes the first entry whose source ends the requested text, so a variant precedes its base file.
if (list) result.sort((a, b) => b.source.length - a.source.length);
await writeFile(output, JSON.stringify(result));
